-- ============================================================================
-- MediStock - Generic SP parameter alignment (TEST/CI replay only)
-- The API binds stored-procedure parameters BY NAME:
--     GetRecords    -> @p_module, @p_param1..@p_param4        (get_records)
--     GetRecordsById-> @p_module, @p_record_id                (get_records_by_id)
--     DeleteRecord  -> @p_recordid, @p_deleted_by, @p_module  (delete_records)
-- File 15's dump-style re-creations used non-p names (module/record_id/
-- in_deleted_by), which MySqlConnector rejects on fresh installs.
-- This superset replaces them with API-compatible signatures using the SAME
-- module branches as file 15. Apply to live ONLY after verifying the app.
-- ============================================================================
DELIMITER $$

DROP PROCEDURE IF EXISTS `get_records_by_id`$$

CREATE PROCEDURE `get_records_by_id`(
    IN p_module    VARCHAR(100),
    IN p_record_id VARCHAR(100)
)
BEGIN
    IF p_module = 'roles' THEN
        SELECT * FROM roles WHERE id = p_record_id AND COALESCE(is_deleted, 0) = 0;

    ELSEIF p_module = 'product' THEN
        SELECT * FROM products WHERE id = p_record_id AND is_deleted = 0;

    ELSEIF p_module = 'category' THEN
        SELECT * FROM product_categories WHERE id = p_record_id AND is_deleted = 0;

    ELSEIF p_module = 'batch' THEN
        SELECT b.*, p.name AS product_name
        FROM product_batches b
        JOIN products p ON p.id = b.product_id
        WHERE b.id = p_record_id AND b.is_deleted = 0;

    ELSEIF p_module = 'customer' THEN
        SELECT * FROM customers WHERE id = p_record_id AND is_deleted = 0;

    ELSEIF p_module = 'supplier' THEN
        SELECT * FROM suppliers WHERE id = p_record_id AND is_deleted = 0;

    ELSEIF p_module = 'sale' THEN
        SELECT s.*, CONCAT(c.first_name, ' ', COALESCE(c.last_name, '')) AS customer_name
        FROM sales s
        LEFT JOIN customers c ON c.id = s.customer_id
        WHERE s.id = p_record_id AND s.is_deleted = 0;

    ELSEIF p_module = 'purchase_order' THEN
        SELECT po.*, sp.name AS supplier_name
        FROM purchase_orders po
        JOIN suppliers sp ON sp.id = po.supplier_id
        WHERE po.id = p_record_id AND po.is_deleted = 0;

    ELSEIF p_module = 'patient' THEN
        SELECT * FROM patients WHERE id = p_record_id AND is_deleted = 0;

    ELSEIF p_module = 'prescription' THEN
        SELECT pr.*, CONCAT(pt.first_name, ' ', COALESCE(pt.last_name, '')) AS patient_name
        FROM prescriptions pr
        JOIN patients pt ON pt.id = pr.patient_id
        WHERE pr.id = p_record_id AND pr.is_deleted = 0;

    ELSEIF p_module = 'expense' THEN
        SELECT e.*, ec.name AS category_name
        FROM expenses e
        LEFT JOIN expense_categories ec ON ec.id = e.category_id
        WHERE e.id = p_record_id AND e.is_deleted = 0;

    ELSEIF p_module = 'user' THEN
        SELECT id, pharmacy_id, role_id, first_name, last_name, email, mobile,
               avatar, locked, is_active, created_on
        FROM pharmacy_users WHERE id = p_record_id AND is_deleted = 0;

    ELSEIF p_module = 'pharmacy' THEN
        SELECT * FROM pharmacies WHERE id = p_record_id AND is_deleted = 0;

    ELSEIF p_module = 'dda_entry' THEN
        SELECT d.*, p.name AS product_name
        FROM dda_register d
        JOIN products p ON p.id = d.product_id
        WHERE d.id = p_record_id AND d.is_deleted = 0;

    ELSEIF p_module = 'stock_take_session' THEN
        SELECT * FROM stock_take_sessions WHERE id = p_record_id AND is_deleted = 0;

    ELSE
        SELECT 'Unknown module' AS `error`;
    END IF;
END$$

DELIMITER ;

DELIMITER $$

DROP PROCEDURE IF EXISTS `delete_records`$$

CREATE PROCEDURE `delete_records`(
    IN p_recordid     BIGINT,
    IN p_deleted_by   BIGINT,
    IN p_module       VARCHAR(100)
)
BEGIN
    IF p_module = 'roles' THEN
        UPDATE roles SET is_deleted = 1, deleted_by = p_deleted_by, deleted_on = NOW()
        WHERE id = p_recordid;

    ELSEIF p_module = 'product' THEN
        UPDATE products SET is_deleted = 1, created_by = p_deleted_by WHERE id = p_recordid;
    ELSEIF p_module = 'category' THEN
        UPDATE product_categories SET is_deleted = 1, created_by = p_deleted_by WHERE id = p_recordid;
    ELSEIF p_module = 'batch' THEN
        UPDATE product_batches SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'customer' THEN
        UPDATE customers SET is_deleted = 1, created_by = p_deleted_by WHERE id = p_recordid;
    ELSEIF p_module = 'supplier' THEN
        UPDATE suppliers SET is_deleted = 1, created_by = p_deleted_by WHERE id = p_recordid;
    ELSEIF p_module = 'sale' THEN
        UPDATE sales SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'purchase_order' THEN
        UPDATE purchase_orders SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'expense' THEN
        UPDATE expenses SET is_deleted = 1, created_by = p_deleted_by WHERE id = p_recordid;
    ELSEIF p_module = 'expense_category' THEN
        UPDATE expense_categories SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'patient' THEN
        UPDATE patients SET is_deleted = 1, created_by = p_deleted_by WHERE id = p_recordid;
    ELSEIF p_module = 'prescription' THEN
        UPDATE prescriptions SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'dda_entry' THEN
        UPDATE dda_register SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'stock_adjustment' THEN
        UPDATE stock_adjustments SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'stock_take_session' THEN
        UPDATE stock_take_sessions SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'pharmacy_user' THEN
        UPDATE pharmacy_users SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'notification' THEN
        UPDATE notifications SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'supplier_price_history' THEN
        DELETE FROM supplier_price_history WHERE id = p_recordid;
    ELSEIF p_module = 'patient_allergy' THEN
        UPDATE patient_allergies SET is_deleted = 1 WHERE id = p_recordid;
    ELSEIF p_module = 'patient_condition' THEN
        UPDATE patient_conditions SET is_deleted = 1 WHERE id = p_recordid;

    ELSE
        SELECT 'Unknown module' AS `error`;
    END IF;
END$$

DELIMITER ;