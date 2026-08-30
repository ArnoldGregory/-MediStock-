-- ============================================================
--  MediStock — Schema drift fixes
--  Adds columns that exist on the production DB (added by hand
--  during development) to the versioned schema, so a fresh
--  install matches production exactly.
--  Idempotent: safe to run on any environment.
--  Run AFTER files 01-22.
-- ============================================================

USE medistock;

DELIMITER $$

DROP PROCEDURE IF EXISTS `mig_add_column`$$
CREATE PROCEDURE `mig_add_column`(IN p_table_name VARCHAR(64), IN p_column_name VARCHAR(64), IN p_column_ddl TEXT)
BEGIN
  IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = p_table_name AND COLUMN_NAME = p_column_name) = 0 THEN
    SET @ddl = CONCAT('ALTER TABLE `', p_table_name, '` ADD COLUMN `', p_column_name, '` ', p_column_ddl);
    PREPARE stmt FROM @ddl;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
  END IF;
END$$

CALL `mig_add_column`('suppliers', 'city', 'varchar(100) NULL')$$
CALL `mig_add_column`('suppliers', 'country', 'varchar(100) NULL')$$

CALL `mig_add_column`('customers', 'date_of_birth', 'date NULL')$$
CALL `mig_add_column`('customers', 'gender', 'varchar(20) NULL')$$

CALL `mig_add_column`('sales', 'notes', 'text NULL')$$

CALL `mig_add_column`('pharmacies', 'owner_name', 'varchar(200) NULL')$$

CALL `mig_add_column`('dda_register', 'patient_id', 'bigint NULL')$$
CALL `mig_add_column`('dda_register', 'prescription_id', 'bigint NULL')$$
CALL `mig_add_column`('dda_register', 'dispensed_date', 'date NULL')$$
CALL `mig_add_column`('dda_register', 'notes', 'text NULL')$$

CALL `mig_add_column`('expenses', 'notes', 'text NULL')$$

CALL `mig_add_column`('patients', 'allergies', 'text NULL')$$
CALL `mig_add_column`('patients', 'medical_history', 'text NULL')$$

CALL `mig_add_column`('prescriptions', 'hospital', 'varchar(200) NULL')$$

CALL `mig_add_column`('purchase_orders', 'product_id', 'bigint NULL')$$
CALL `mig_add_column`('purchase_orders', 'quantity', 'int NULL')$$
CALL `mig_add_column`('purchase_orders', 'unit_cost', 'decimal(15,2) NULL')$$
CALL `mig_add_column`('purchase_orders', 'total_cost', 'decimal(15,2) NULL')$$
CALL `mig_add_column`('purchase_orders', 'notes', 'text NULL')$$

DROP PROCEDURE IF EXISTS `mig_add_column`$$
DELIMITER ;