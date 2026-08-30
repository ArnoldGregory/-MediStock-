-- ============================================================
--  MediStock — Sales Returns & Credit Notes
--  Idempotent skip-if-exists for tables; procedures are DROP/CREATE.
-- ============================================================

USE medistock;

ALTER TABLE sale_items
    ADD COLUMN returned_qty DECIMAL(10,2) NOT NULL DEFAULT 0 AFTER quantity;

CREATE TABLE IF NOT EXISTS sales_returns (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    pharmacy_id    BIGINT       NOT NULL,
    sale_id        BIGINT       NOT NULL,
    customer_id    BIGINT       NULL,
    return_number  VARCHAR(50)  NOT NULL UNIQUE,
    reason         VARCHAR(500) NULL,
    total_refund   DECIMAL(15,2) NOT NULL DEFAULT 0,
    status         VARCHAR(20)  NOT NULL DEFAULT 'Completed',
    created_by     BIGINT       NULL,
    created_on     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_ret_pharmacy (pharmacy_id),
    INDEX idx_ret_sale (sale_id)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

CREATE TABLE IF NOT EXISTS sales_return_items (
    id            BIGINT AUTO_INCREMENT PRIMARY KEY,
    return_id     BIGINT       NOT NULL,
    sale_item_id  BIGINT       NOT NULL,
    product_id    BIGINT       NOT NULL,
    batch_id      BIGINT       NULL,
    quantity      INT          NOT NULL,
    unit_price    DECIMAL(15,2) NOT NULL DEFAULT 0,
    refund_amount DECIMAL(15,2) NOT NULL DEFAULT 0,
    created_on    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_retitem_return (return_id),
    INDEX idx_retitem_saleitem (sale_item_id)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4;

-- ------------------------------------------------------------------
--  Atomic return: validate, insert header + lines, increment returned_qty,
--  restock product(s) and (if a batch was sold) the batch.
-- ------------------------------------------------------------------
DROP PROCEDURE IF EXISTS create_sale_return;
DELIMITER $$
CREATE PROCEDURE create_sale_return(
    IN p_pharmacy_id BIGINT,
    IN p_sale_id     BIGINT,
    IN p_customer_id BIGINT,
    IN p_reason      VARCHAR(500),
    IN p_total_refund DECIMAL(15,2),
    IN p_created_by  BIGINT,
    IN p_items_json  JSON
)
BEGIN
    DECLARE v_return_id BIGINT DEFAULT 0;
    DECLARE v_return_number VARCHAR(50);
    DECLARE v_count INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    SELECT COUNT(*) INTO v_count
    FROM sales
    WHERE id = p_sale_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;
    IF v_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Sale not found for this pharmacy';
    END IF;

    IF p_items_json IS NULL OR JSON_LENGTH(p_items_json) = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Return must have at least one item';
    END IF;

    SET v_return_number = CONCAT('RET-', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s'),
                                 '-', LPAD(FLOOR(RAND() * 10000), 4, '0'));

    INSERT INTO sales_returns (pharmacy_id, sale_id, customer_id, return_number,
                               reason, total_refund, status, created_by)
    VALUES (p_pharmacy_id, p_sale_id, NULLIF(p_customer_id, 0), v_return_number,
            p_reason, p_total_refund, 'Completed', p_created_by);
    SET v_return_id = LAST_INSERT_ID();

    INSERT INTO sales_return_items (return_id, sale_item_id, product_id, batch_id,
                                    quantity, unit_price, refund_amount)
    SELECT v_return_id, ji.sale_item_id, ji.product_id, NULLIF(ji.batch_id, 0),
           ji.quantity, ji.unit_price, ji.refund
    FROM JSON_TABLE(p_items_json, '$[*]' COLUMNS (
        sale_item_id BIGINT       PATH '$.sale_item_id',
        product_id   BIGINT       PATH '$.product_id',
        batch_id     BIGINT       PATH '$.batch_id',
        quantity     INT          PATH '$.quantity',
        unit_price   DECIMAL(15,2) PATH '$.unit_price',
        refund       DECIMAL(15,2) PATH '$.refund'
    )) ji;

    IF EXISTS (
        SELECT 1 FROM sales_return_items ri
        WHERE ri.return_id = v_return_id AND ri.quantity <= 0
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Return quantity must be greater than zero';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM sales_return_items ri
        JOIN sale_items si ON si.id = ri.sale_item_id
        WHERE ri.return_id = v_return_id
          AND (si.quantity - COALESCE(si.returned_qty, 0)) < ri.quantity
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Return quantity exceeds the quantity sold';
    END IF;

    UPDATE sale_items si
    JOIN sales_return_items ri
      ON ri.return_id = v_return_id AND ri.sale_item_id = si.id
    SET si.returned_qty = COALESCE(si.returned_qty, 0) + ri.quantity;

    UPDATE products p
    JOIN sales_return_items ri ON ri.return_id = v_return_id AND ri.product_id = p.id
    SET p.stock_qty = p.stock_qty + ri.quantity
    WHERE p.pharmacy_id = p_pharmacy_id AND p.is_deleted = 0;

    UPDATE product_batches b
    JOIN sales_return_items ri ON ri.return_id = v_return_id AND ri.batch_id = b.id
    SET b.quantity = b.quantity + ri.quantity,
        b.quantity_sold = GREATEST(b.quantity_sold - ri.quantity, 0)
    WHERE b.pharmacy_id = p_pharmacy_id AND b.is_deleted = 0;

    COMMIT;

    SELECT v_return_id AS id, v_return_number AS return_number;
END$$
DELIMITER ;

-- ------------------------------------------------------------------
--  List returns for a pharmacy
-- ------------------------------------------------------------------
DROP PROCEDURE IF EXISTS get_sale_returns;
DELIMITER $$
CREATE PROCEDURE get_sale_returns(IN p_pharmacy_id BIGINT)
BEGIN
    SELECT r.id, r.return_number, r.sale_id, r.sale_id AS sale_number_lbl,
           s.sale_number,
           CASE WHEN s.customer_id IS NULL OR s.customer_id = 0 THEN 'Walk-In'
                ELSE CONCAT_WS(' ', c.first_name, c.last_name) END AS customer_name,
           r.total_refund, r.status, r.reason, r.created_on,
           (SELECT COUNT(*) FROM sales_return_items ri WHERE ri.return_id = r.id) AS item_count
    FROM sales_returns r
    LEFT JOIN sales s ON s.id = r.sale_id
    LEFT JOIN customers c ON c.id = s.customer_id
    WHERE r.pharmacy_id = p_pharmacy_id
    ORDER BY r.id DESC;
END$$
DELIMITER ;

-- ------------------------------------------------------------------
--  Items still eligible for return on a given sale
-- ------------------------------------------------------------------
DROP PROCEDURE IF EXISTS get_sale_returnable_items;
DELIMITER $$
CREATE PROCEDURE get_sale_returnable_items(IN p_sale_id BIGINT)
BEGIN
    SELECT si.id AS sale_item_id, si.sale_id, si.product_id, si.batch_id,
           si.quantity, COALESCE(si.returned_qty, 0) AS returned_qty,
           (si.quantity - COALESCE(si.returned_qty, 0)) AS remaining,
           si.unit_price, p.name AS product_name, p.sku
    FROM sale_items si
    JOIN products p ON p.id = si.product_id AND p.is_deleted = 0
    WHERE si.sale_id = p_sale_id
      AND (si.quantity - COALESCE(si.returned_qty, 0)) > 0;
END$$
DELIMITER ;

-- ------------------------------------------------------------------
--  Line items for an existing return (for printing / credit note)
-- ------------------------------------------------------------------
DROP PROCEDURE IF EXISTS get_sale_return_items;
DELIMITER $$
CREATE PROCEDURE get_sale_return_items(IN p_return_id BIGINT)
BEGIN
    SELECT ri.return_id, ri.product_id, ri.batch_id, ri.quantity, ri.unit_price,
           ri.refund_amount, p.name AS product_name, p.sku
    FROM sales_return_items ri
    JOIN products p ON p.id = ri.product_id
    WHERE ri.return_id = p_return_id;
END$$
DELIMITER ;