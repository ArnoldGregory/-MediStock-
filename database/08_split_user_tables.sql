-- ============================================================
--  MediStock — Split User Tables (Riziki Pattern)
--  Creates portal_users + p_external_portal_user
--  Migrates data from pharmacy_users
--  Fixes validate_login to route ADMIN/CLIENT
--  Creates get_menu SP
--  Run AFTER files 01–07
-- ============================================================

USE medistock;

DELIMITER $$

-- ============================================================
-- 1. CREATE portal_users (SuperAdmin/Admin — role_id 1, 2)
-- ============================================================
DROP TABLE IF EXISTS `portal_users`$$
CREATE TABLE `portal_users` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `role_id` int NOT NULL DEFAULT 1,
  `first_name` varchar(100) DEFAULT NULL,
  `middle_name` varchar(100) DEFAULT NULL,
  `last_name` varchar(100) DEFAULT NULL,
  `email` varchar(200) NOT NULL,
  `mobile` varchar(50) DEFAULT NULL,
  `password` varchar(200) NOT NULL,
  `avatar` varchar(500) DEFAULT NULL,
  `locked` tinyint DEFAULT 0,
  `approved` tinyint DEFAULT 1,
  `google_authenticate` tinyint DEFAULT 0,
  `sec_key` varchar(200) DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_portal_user_email` (`email`),
  KEY `idx_portal_user_pharmacy` (`pharmacy_id`),
  KEY `idx_portal_user_role` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4$$

-- ============================================================
-- 2. CREATE p_external_portal_user (Pharmacist/Staff/Cashier — role_id 3,4,5)
-- ============================================================
DROP TABLE IF EXISTS `p_external_portal_user`$$
CREATE TABLE `p_external_portal_user` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `role_id` int NOT NULL DEFAULT 3,
  `first_name` varchar(100) DEFAULT NULL,
  `middle_name` varchar(100) DEFAULT NULL,
  `last_name` varchar(100) DEFAULT NULL,
  `email` varchar(200) NOT NULL,
  `mobile` varchar(50) DEFAULT NULL,
  `password` varchar(200) NOT NULL,
  `avatar` varchar(500) DEFAULT NULL,
  `locked` tinyint DEFAULT 0,
  `change_password` tinyint DEFAULT 0,
  `failed_login_attempts` int DEFAULT 0,
  `google_authenticate` tinyint DEFAULT 0,
  `sec_key` varchar(200) DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_ext_user_email` (`email`),
  KEY `idx_ext_user_pharmacy` (`pharmacy_id`),
  KEY `idx_ext_user_role` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4$$

-- ============================================================
-- 3. MIGRATE DATA from pharmacy_users
--    role_id 1 (SuperAdmin) + 2 (Admin) → portal_users
--    role_id 3 (Pharmacist) + 4 (Staff) + 5 (Cashier) → p_external_portal_user
-- ============================================================
INSERT IGNORE INTO portal_users
  (id, pharmacy_id, role_id, first_name, middle_name, last_name,
   email, mobile, password, avatar, locked, google_authenticate, sec_key,
   is_deleted, created_by, created_on)
SELECT
  id, pharmacy_id, role_id, first_name, middle_name, last_name,
  email, mobile, password, avatar, locked, google_authenticate, sec_key,
  is_deleted, created_by, created_on
FROM pharmacy_users
WHERE role_id IN (1, 2) AND is_deleted = 0$$

INSERT IGNORE INTO p_external_portal_user
  (id, pharmacy_id, role_id, first_name, middle_name, last_name,
   email, mobile, password, avatar, locked, change_password,
   failed_login_attempts, google_authenticate, sec_key,
   is_deleted, created_by, created_on)
SELECT
  id, pharmacy_id, role_id, first_name, middle_name, last_name,
  email, mobile, password, avatar, locked, change_password,
  failed_login_attempts, google_authenticate, sec_key,
  is_deleted, created_by, created_on
FROM pharmacy_users
WHERE role_id IN (3, 4, 5) AND is_deleted = 0$$

-- ============================================================
-- 4. ADD menu_icon column to menu_access (idempotent, wrapped in
--    a temp procedure so it works on fresh installs)
-- ============================================================
DROP PROCEDURE IF EXISTS `mig_add_menu_icon`$$
CREATE PROCEDURE `mig_add_menu_icon`()
BEGIN
  IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'menu_access' AND COLUMN_NAME = 'menu_icon') = 0 THEN
    ALTER TABLE `menu_access` ADD COLUMN `menu_icon` varchar(50) DEFAULT 'fa-circle' AFTER `sub_menu_name`;
  END IF;
END$$
CALL `mig_add_menu_icon`()$$
DROP PROCEDURE IF EXISTS `mig_add_menu_icon`$$

-- ============================================================
-- 5. SEED menu_icon values for existing menu_access rows
-- ============================================================
UPDATE menu_access SET menu_icon = 'fa-dashboard'      WHERE main_menu_name = 'Dashboard'$$
UPDATE menu_access SET menu_icon = 'fa-cube'            WHERE main_menu_name = 'Inventory'$$
UPDATE menu_access SET menu_icon = 'fa-shopping-cart'   WHERE main_menu_name = 'Sales'$$
UPDATE menu_access SET menu_icon = 'fa-users'           WHERE main_menu_name = 'Customers'$$
UPDATE menu_access SET menu_icon = 'fa-truck'           WHERE main_menu_name = 'Suppliers'$$
UPDATE menu_access SET menu_icon = 'fa-money'           WHERE main_menu_name = 'Finance'$$
UPDATE menu_access SET menu_icon = 'fa-bar-chart'       WHERE main_menu_name = 'Reports'$$
UPDATE menu_access SET menu_icon = 'fa-heartbeat'       WHERE main_menu_name = 'Clinical'$$
UPDATE menu_access SET menu_icon = 'fa-balance-scale'   WHERE main_menu_name = 'DDA'$$
UPDATE menu_access SET menu_icon = 'fa-cog'             WHERE main_menu_name = 'Settings'$$
UPDATE menu_access SET menu_icon = 'fa-user-secret'     WHERE main_menu_name = 'Admin'$$

-- ============================================================
-- 6. FIX validate_login — route ADMIN → portal_users, CLIENT → p_external_portal_user
-- ============================================================
DROP PROCEDURE IF EXISTS `validate_login`$$
CREATE PROCEDURE `validate_login`(
    IN username    VARCHAR(200),
    IN profiletype VARCHAR(50)
)
BEGIN
    IF profiletype = 'ADMIN' THEN
        SELECT id, pharmacy_id, role_id, first_name, middle_name, last_name,
               email, mobile, password, avatar, locked, approved,
               is_deleted, created_by, created_on
        FROM portal_users
        WHERE email = username AND is_deleted = 0
        LIMIT 1;
    ELSE
        SELECT id, pharmacy_id, role_id, first_name, middle_name, last_name,
               email, mobile, password, avatar, locked, change_password,
               failed_login_attempts, is_deleted, created_by, created_on
        FROM p_external_portal_user
        WHERE email = username AND is_deleted = 0
        LIMIT 1;
    END IF;
END$$

-- ============================================================
-- 7. FIX client_password_reset — update both tables
-- ============================================================
DROP PROCEDURE IF EXISTS `client_password_reset`$$
CREATE PROCEDURE `client_password_reset`(
    IN p_email     VARCHAR(200),
    IN p_password  VARCHAR(200),
    IN profiletype VARCHAR(50)
)
BEGIN
    IF profiletype = 'ADMIN' OR profiletype IS NULL THEN
        UPDATE portal_users
        SET password = p_password
        WHERE email = p_email AND is_deleted = 0;
    END IF;

    IF profiletype = 'CLIENT' OR profiletype IS NULL THEN
        UPDATE p_external_portal_user
        SET password = p_password, change_password = 0, failed_login_attempts = 0
        WHERE email = p_email AND is_deleted = 0;
    END IF;
END$$

-- ============================================================
-- 8. CREATE get_menu SP (GROUP BY pattern — works with existing data)
-- ============================================================
DROP PROCEDURE IF EXISTS `get_menu`$$
CREATE PROCEDURE `get_menu`(
    IN p_profile_id INT,
    IN p_type       VARCHAR(20),
    IN p_menu_name  VARCHAR(100)
)
BEGIN
    IF p_type = 'main' THEN
        SELECT main_menu_name, menu_icon, menu_order
        FROM menu_access
        WHERE role_id = p_profile_id AND can_access = 1
        GROUP BY main_menu_name, menu_icon, menu_order
        ORDER BY menu_order;

    ELSEIF p_type = 'sub' THEN
        SELECT sub_menu_name, page_url, sub_menu_order
        FROM menu_access
        WHERE role_id = p_profile_id
          AND main_menu_name = p_menu_name
          AND can_access = 1
          AND sub_menu_name IS NOT NULL
          AND sub_menu_name != ''
        ORDER BY sub_menu_order;

    ELSEIF p_type = 'page_url' THEN
        SELECT page_url
        FROM menu_access
        WHERE role_id = p_profile_id
          AND main_menu_name = p_menu_name
          AND can_access = 1
        LIMIT 1;
    END IF;
END$$

DELIMITER ;
