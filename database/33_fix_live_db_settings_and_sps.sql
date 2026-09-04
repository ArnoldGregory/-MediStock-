-- ============================================================================
-- 33_fix_live_db_settings_and_sps.sql
-- Applies pending live-DB data-model fixes that the 41-test replay already
-- guarantees (migrations 25 & 27) but the live server never received:
--   1. pharmacy_config      table missing -> create settings config table
--      (SettingsController.SavePharmacySetting / POST api/settings/config).
--   2. delete_records       SP uses non-p params (recordid/in_deleted_by/
--      module) that don't match the C# binding (p_recordid/p_deleted_by/
--      p_module) -> all DeleteRecord() calls fail with
--      "Parameter 'recordid' not found". Re-create with API-compatible sig.
--   3. get_records_by_id    SP uses non-p params (module/record_id) vs C#
--      binding (p_module/p_record_id) -> silently returns empty. Re-create.
-- All bodies copied verbatim from migrations 25 & 27. Idempotent.
-- ============================================================================

-- 1. pharmacy_config (from 27_pharmacy_config.sql)
CREATE TABLE IF NOT EXISTS `pharmacy_config` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `config_key` varchar(100) NOT NULL,
  `config_value` text,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_by` bigint DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_pharmacy_config` (`pharmacy_id`, `config_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. get_records_by_id (from 25_sp_generic_align.sql)
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

-- 3. delete_records (from 25_sp_generic_align.sql)
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
