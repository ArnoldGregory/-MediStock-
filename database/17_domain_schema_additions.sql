-- Schema additions required so the updated DBHandler.cs (model-to-table mapping)
-- can persist all fields. Run this BEFORE recreating the domain write SPs.
-- Uses INFORMATION_SCHEMA guards because this MySQL server does not support
-- `ADD COLUMN IF NOT EXISTS` (5.7.x). Run each SET @sql block as one statement.
USE `medistock`;

SET @db = 'medistock';

-- suppliers.city
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='suppliers' AND column_name='city');
SET @sql = IF(@c = 0, 'ALTER TABLE `suppliers` ADD COLUMN `city` VARCHAR(100) NULL AFTER `address`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- suppliers.country
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='suppliers' AND column_name='country');
SET @sql = IF(@c = 0, 'ALTER TABLE `suppliers` ADD COLUMN `country` VARCHAR(100) NULL AFTER `city`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- patients.allergies
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='patients' AND column_name='allergies');
SET @sql = IF(@c = 0, 'ALTER TABLE `patients` ADD COLUMN `allergies` TEXT NULL AFTER `nhif_number`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- patients.medical_history
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='patients' AND column_name='medical_history');
SET @sql = IF(@c = 0, 'ALTER TABLE `patients` ADD COLUMN `medical_history` TEXT NULL AFTER `allergies`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- prescriptions.hospital
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='prescriptions' AND column_name='hospital');
SET @sql = IF(@c = 0, 'ALTER TABLE `prescriptions` ADD COLUMN `hospital` VARCHAR(200) NULL AFTER `doctor_name`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- sales.notes
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='sales' AND column_name='notes');
SET @sql = IF(@c = 0, 'ALTER TABLE `sales` ADD COLUMN `notes` TEXT NULL AFTER `payment_reference`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- pharmacies.owner_name (DBHandler AddPharmacy binds @in_owner_name)
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='pharmacies' AND column_name='owner_name');
SET @sql = IF(@c = 0, 'ALTER TABLE `pharmacies` ADD COLUMN `owner_name` VARCHAR(200) NULL AFTER `license_number`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- purchase_orders line-item columns (DBHandler AddPurchaseOrder binds them)
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='purchase_orders' AND column_name='product_id');
SET @sql = IF(@c = 0, 'ALTER TABLE `purchase_orders` ADD COLUMN `product_id` BIGINT NULL AFTER `supplier_id`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='purchase_orders' AND column_name='quantity');
SET @sql = IF(@c = 0, 'ALTER TABLE `purchase_orders` ADD COLUMN `quantity` INT NULL AFTER `product_id`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='purchase_orders' AND column_name='unit_cost');
SET @sql = IF(@c = 0, 'ALTER TABLE `purchase_orders` ADD COLUMN `unit_cost` DECIMAL(15,2) NULL AFTER `quantity`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='purchase_orders' AND column_name='total_cost');
SET @sql = IF(@c = 0, 'ALTER TABLE `purchase_orders` ADD COLUMN `total_cost` DECIMAL(15,2) NULL AFTER `unit_cost`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='purchase_orders' AND column_name='notes');
SET @sql = IF(@c = 0, 'ALTER TABLE `purchase_orders` ADD COLUMN `notes` TEXT NULL AFTER `expected_date`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- dda_register clinical columns (DBHandler AddDDAEntry binds them)
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='dda_register' AND column_name='patient_id');
SET @sql = IF(@c = 0, 'ALTER TABLE `dda_register` ADD COLUMN `patient_id` BIGINT NULL AFTER `pharmacy_id`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='dda_register' AND column_name='prescription_id');
SET @sql = IF(@c = 0, 'ALTER TABLE `dda_register` ADD COLUMN `prescription_id` BIGINT NULL AFTER `patient_id`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='dda_register' AND column_name='dispensed_date');
SET @sql = IF(@c = 0, 'ALTER TABLE `dda_register` ADD COLUMN `dispensed_date` DATE NULL AFTER `quantity`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='dda_register' AND column_name='notes');
SET @sql = IF(@c = 0, 'ALTER TABLE `dda_register` ADD COLUMN `notes` TEXT NULL AFTER `reference_number`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- expenses.notes (DBHandler AddExpense binds @in_notes)
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='expenses' AND column_name='notes');
SET @sql = IF(@c = 0, 'ALTER TABLE `expenses` ADD COLUMN `notes` TEXT NULL AFTER `reference`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- customers.date_of_birth / gender (DBHandler AddCustomer binds them)
SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='customers' AND column_name='date_of_birth');
SET @sql = IF(@c = 0, 'ALTER TABLE `customers` ADD COLUMN `date_of_birth` DATE NULL AFTER `address`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=@db AND table_name='customers' AND column_name='gender');
SET @sql = IF(@c = 0, 'ALTER TABLE `customers` ADD COLUMN `gender` VARCHAR(20) NULL AFTER `date_of_birth`', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
