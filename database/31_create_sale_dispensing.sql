-- ============================================================
-- 31_create_sale_dispensing.sql
-- Update create_sale to persist the dispensing-first columns
-- (sale_mode / prescription_id / dispensed_by) added by
-- migration 30. Defaults sale_mode to 'POS' for backwards
-- compatibility with the fast walk-in shop flow.
-- ============================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `create_sale`$$

CREATE PROCEDURE `create_sale`(
    IN  in_pharmacy_id      BIGINT,
    IN  in_customer_id      BIGINT,
    IN  in_user_id          BIGINT,
    IN  in_total_amount     DECIMAL(15,2),
    IN  in_discount         DECIMAL(15,2),
    IN  in_tax              DECIMAL(15,2),
    IN  in_net_amount       DECIMAL(15,2),
    IN  in_amount_paid      DECIMAL(15,2),
    IN  in_payment_method   VARCHAR(50),
    IN  in_notes            TEXT,
    IN  in_sale_mode        VARCHAR(20),
    IN  in_prescription_id  BIGINT,
    IN  in_dispensed_by     BIGINT,
    OUT p_sale_id           BIGINT
)
BEGIN
    DECLARE v_sale_number VARCHAR(50);
    SET p_sale_id = 0;
    SET v_sale_number = CONCAT('SAL-', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s'), '-', FLOOR(1000 + RAND() * 9000));
    INSERT INTO sales
        (pharmacy_id, customer_id, sale_number, sale_type, subtotal, vat_amount,
         discount, total, amount_paid, payment_method, notes, sold_by,
         sale_mode, prescription_id, dispensed_by, created_on)
    VALUES
        (in_pharmacy_id, in_customer_id, v_sale_number, 'Retail',
         COALESCE(in_total_amount, 0), COALESCE(in_tax, 0),
         COALESCE(in_discount, 0), COALESCE(in_net_amount, 0),
         COALESCE(in_amount_paid, 0), COALESCE(in_payment_method, 'Cash'),
         in_notes, in_user_id,
         COALESCE(in_sale_mode, 'POS'), in_prescription_id, in_dispensed_by, NOW());
    SET p_sale_id = LAST_INSERT_ID();
END$$

DELIMITER ;
