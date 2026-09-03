-- Roles table + seed for MediStock
-- The C# layer (AccessController / AdminController) already writes to a `roles`
-- table (INSERT/UPDATE/DELETE) and reads it via get_records('roles'), but the
-- table was never created and get_records had no 'roles' branch. This file
-- creates + seeds it so role CRUD and the Access Control dropdown work.
--
-- Roles: 1=SuperAdmin, 2=Admin, 3=Pharmacist, 4=Staff, 5=Cashier

-- 1. CREATE TABLE (idempotent)
CREATE TABLE IF NOT EXISTS `roles` (
  `id`           INT          NOT NULL AUTO_INCREMENT,
  `role_name`    VARCHAR(100) NOT NULL,
  `description`  VARCHAR(255) DEFAULT NULL,
  `created_by`   INT          DEFAULT NULL,
  `created_on`   DATETIME     DEFAULT CURRENT_TIMESTAMP,
  `updated_by`   INT          DEFAULT NULL,
  `updated_on`   DATETIME     DEFAULT NULL,
  `deleted_by`   INT          DEFAULT NULL,
  `deleted_on`   DATETIME     DEFAULT NULL,
  `is_deleted`   TINYINT      NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_role_name` (`role_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. SEED the 5 canonical roles
INSERT IGNORE INTO `roles` (id, role_name, description, created_on) VALUES
  (1, 'SuperAdmin', 'Platform-wide administrator', NOW()),
  (2, 'Admin',      'Pharmacy administrator',           NOW()),
  (3, 'Pharmacist', 'Licensed pharmacist',              NOW()),
  (4, 'Staff',      'Pharmacy staff',                   NOW()),
  (5, 'Cashier',    'Point of sale cashier',            NOW());

-- 3./4. The get_records_by_id 'roles' branch and delete_records 'roles' soft-delete
--    branch are implemented by later migrations (15_get_records_elseif.sql and
--    25_sp_generic_align.sql), so no inline patch is needed here.
