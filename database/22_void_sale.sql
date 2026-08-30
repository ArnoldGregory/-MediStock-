-- ============================================================
-- 22 — Void Sale + auth gap fixes
--  1. void_sale  — void a sale, restore product + batch stock
-- ============================================================

DROP PROCEDURE IF EXISTS `void_sale`;
DELIMITER $$
CREATE PROCEDURE `void_sale`(
    IN p_sale_id     BIGINT,
    IN p_pharmacy_id BIGINT
)
BEGIN
    DECLARE v_exists  INT DEFAULT 0;
    DECLARE v_status  VARCHAR(20) DEFAULT '';

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Failed to void sale';
    END;

    SELECT COUNT(*), MAX(status) INTO v_exists, v_status
    FROM sales
    WHERE id = p_sale_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;

    IF v_exists = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Sale not found';
    END IF;

    IF v_status = 'Voided' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Sale already voided';
    END IF;

    START TRANSACTION;

    UPDATE sales
    SET status = 'Voided'
    WHERE id = p_sale_id AND pharmacy_id = p_pharmacy_id;

    UPDATE products p
    JOIN sale_items si ON si.product_id = p.id
    SET p.stock_qty = p.stock_qty + (si.quantity - COALESCE(si.returned_qty, 0))
    WHERE si.sale_id = p_sale_id AND p.pharmacy_id = p_pharmacy_id AND p.is_deleted = 0;

    UPDATE product_batches pb
    JOIN sale_items si ON si.batch_id = pb.id
    SET pb.quantity_sold = GREATEST(pb.quantity_sold - (si.quantity - COALESCE(si.returned_qty, 0)), 0)
    WHERE si.sale_id = p_sale_id
      AND si.batch_id IS NOT NULL
      AND pb.pharmacy_id = p_pharmacy_id
      AND pb.is_deleted = 0;

    COMMIT;
END$$
DELIMITER ;