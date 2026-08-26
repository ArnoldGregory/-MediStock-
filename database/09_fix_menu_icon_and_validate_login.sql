-- ============================================================
--  MediStock — Fix menu_icon + validate_login
--  Run these commands individually in MySQL Workbench
-- ============================================================

USE medistock;

-- 1. Add menu_icon column to menu_access
ALTER TABLE menu_access ADD COLUMN menu_icon varchar(50) DEFAULT 'fa-circle' AFTER sub_menu_name;

-- 2. Seed menu_icon values
UPDATE menu_access SET menu_icon = 'fa-dashboard' WHERE main_menu_name = 'Dashboard';
UPDATE menu_access SET menu_icon = 'fa-cube' WHERE main_menu_name = 'Inventory';
UPDATE menu_access SET menu_icon = 'fa-shopping-cart' WHERE main_menu_name = 'Sales';
UPDATE menu_access SET menu_icon = 'fa-users' WHERE main_menu_name = 'Customers';
UPDATE menu_access SET menu_icon = 'fa-truck' WHERE main_menu_name = 'Suppliers';
UPDATE menu_access SET menu_icon = 'fa-money' WHERE main_menu_name = 'Finance';
UPDATE menu_access SET menu_icon = 'fa-bar-chart' WHERE main_menu_name = 'Reports';
UPDATE menu_access SET menu_icon = 'fa-heartbeat' WHERE main_menu_name = 'Clinical';
UPDATE menu_access SET menu_icon = 'fa-balance-scale' WHERE main_menu_name = 'DDA';
UPDATE menu_access SET menu_icon = 'fa-cog' WHERE main_menu_name = 'Settings';
UPDATE menu_access SET menu_icon = 'fa-user-secret' WHERE main_menu_name = 'Admin';

-- 3. Fix validate_login — ADMIN query missing change_password
DROP PROCEDURE IF EXISTS validate_login;

DELIMITER $$
CREATE PROCEDURE validate_login(
    IN username    VARCHAR(200),
    IN profiletype VARCHAR(50)
)
BEGIN
    IF profiletype = 'ADMIN' THEN
        SELECT id, pharmacy_id, role_id, first_name, middle_name, last_name,
               email, mobile, password, avatar, locked, approved,
               0 AS change_password, 0 AS failed_login_attempts,
               is_deleted, created_by, created_on
        FROM portal_users
        WHERE email = p_username AND is_deleted = 0
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
DELIMITER ;
