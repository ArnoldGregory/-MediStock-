-- ============================================================
--  MediStock — SuperAdmin Platform Module
--  1. Add "Platform" menu to the master catalog (menu_access_data)
--  2. Add "Platform" menu_access rows for SuperAdmin (role_id=1)
--  3. Create platform stored procedures (p_* param convention)
--  Run AFTER 01_tables / 10_fix_menu_riziki_pattern / 12_fix_menu_access_data
-- ============================================================

USE medistock;

-- ============================================================
-- 1. MASTER MENU CATALOG — Platform group
--    NOTE: menu_access_data has NO unique key on (main_menu_name, sub_menu_name),
--    so a plain INSERT IGNORE duplicates on re-run. Use delete-then-insert.
-- ============================================================
DELETE FROM menu_access_data WHERE main_menu_name = 'Platform';

INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type)
VALUES
('Platform',   'Overview',          'fa-globe',         95, 1, '~/SuperAdmin/Index',       'SUPERADMIN'),
('Platform',   'Pharmacies',        'fa-hospital-o',    95, 2, '~/SuperAdmin/Pharmacies',   'SUPERADMIN'),
('Platform',   'Users',             'fa-users',         95, 3, '~/SuperAdmin/Users',        'SUPERADMIN'),
('Platform',   'Audit Log',         'fa-history',       95, 4, '~/SuperAdmin/Audit',        'SUPERADMIN'),
('Platform',   'Access Control',    'fa-shield',        95, 5, '~/Admin/AccessControl',     'SUPERADMIN');

-- ============================================================
-- 2. MENU ACCESS — assign Platform menu to SuperAdmin (role_id=1)
--    Kept name-keyed to match the live get_menu SP.
--    menu_access HAS a unique key on (role_id, main_menu_name, sub_menu_name),
--    but delete-then-insert keeps this seed fully idempotent.
-- ============================================================
DELETE FROM menu_access WHERE role_id = 1 AND main_menu_name = 'Platform';

INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order)
VALUES
(1, 'Platform', 'Overview',       'fa-globe',       '~/SuperAdmin/Index',     1, 95, 1),
(1, 'Platform', 'Pharmacies',     'fa-hospital-o',  '~/SuperAdmin/Pharmacies',1, 95, 2),
(1, 'Platform', 'Users',          'fa-users',       '~/SuperAdmin/Users',     1, 95, 3),
(1, 'Platform', 'Audit Log',      'fa-history',     '~/SuperAdmin/Audit',     1, 95, 4),
(1, 'Platform', 'Access Control', 'fa-shield',      '~/Admin/AccessControl',  1, 95, 5);

DELIMITER $$

-- ============================================================
-- 3a. get_all_pharmacies — platform-wide pharmacy list
-- ============================================================
DROP PROCEDURE IF EXISTS get_all_pharmacies$$
CREATE PROCEDURE get_all_pharmacies()
BEGIN
    SELECT
        p.id,
        p.name,
        p.slug,
        p.phone,
        p.email,
        p.address,
        p.license_number,
        p.currency,
        p.subscription_plan,
        p.subscription_expiry,
        p.is_active,
        p.created_on,
        COALESCE(u.first_name, '') AS owner_first_name,
        COALESCE(u.last_name,  '')  AS owner_last_name,
        COALESCE(u.email,       '')  AS owner_email,
        (SELECT COUNT(*) FROM pharmacy_users pu
          WHERE pu.pharmacy_id = p.id AND pu.is_deleted = 0) AS user_count
    FROM pharmacies p
    LEFT JOIN pharmacy_users u
           ON u.pharmacy_id = p.id
          AND u.role_id = 2
          AND u.is_deleted = 0
    WHERE p.is_deleted = 0
    ORDER BY p.created_on DESC;
END$$

-- ============================================================
-- 3b. get_all_users — platform-wide user list
-- ============================================================
DROP PROCEDURE IF EXISTS get_all_users$$
CREATE PROCEDURE get_all_users(
    IN p_exclude_user_id BIGINT
)
BEGIN
    SELECT
        u.id,
        u.pharmacy_id,
        u.role_id,
        u.first_name,
        u.middle_name,
        u.last_name,
        u.email,
        u.mobile,
        u.is_deleted,
        u.locked,
        u.created_on,
        p.name AS pharmacy_name,
        CASE u.role_id
            WHEN 1 THEN 'SuperAdmin'
            WHEN 2 THEN 'Admin'
            WHEN 3 THEN 'Pharmacist'
            WHEN 4 THEN 'Staff'
            WHEN 5 THEN 'Cashier'
            ELSE 'Staff'
        END AS role_name
    FROM pharmacy_users u
    LEFT JOIN pharmacies p ON p.id = u.pharmacy_id
    WHERE u.id <> p_exclude_user_id
    ORDER BY p.name, u.first_name;
END$$

-- ============================================================
-- 3c. add_pharmacy_platform — create pharmacy + owner (role 2)
--     Returns pharmacy_id + user_id via OUT params.
-- ============================================================
DROP PROCEDURE IF EXISTS add_pharmacy_platform$$
CREATE PROCEDURE add_pharmacy_platform(
    IN p_name            VARCHAR(200),
    IN p_slug            VARCHAR(100),
    IN p_phone           VARCHAR(50),
    IN p_email           VARCHAR(200),
    IN p_address         TEXT,
    IN p_license_number  VARCHAR(100),
    IN p_currency        VARCHAR(10),
    IN p_owner_first     VARCHAR(100),
    IN p_owner_last      VARCHAR(100),
    IN p_owner_email     VARCHAR(200),
    IN p_owner_mobile    VARCHAR(50),
    IN p_owner_password  VARCHAR(200),
    IN p_portal_password VARCHAR(200),
    IN p_created_by      BIGINT,
    OUT p_pharmacy_id    BIGINT,
    OUT p_user_id        BIGINT,
    OUT p_error_code     INT,
    OUT p_error_desc     VARCHAR(500)
)
BEGIN
    DECLARE v_pharmacy_id BIGINT;
    DECLARE v_user_id     BIGINT;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_error_code = 1;
        SET p_error_desc = 'An unexpected error occurred';
        SET p_pharmacy_id = 0;
        SET p_user_id     = 0;
    END;

    SET p_error_code = 0;
    SET p_error_desc = 'OK';

    IF p_name = '' OR p_slug = '' OR p_owner_email = '' OR p_owner_password = '' THEN
        SET p_error_code = 3;
        SET p_error_desc = 'Pharmacy name, slug, owner email and password are required';
        SET p_pharmacy_id = 0;
        SET p_user_id     = 0;
    ELSE
        START TRANSACTION;

        -- Phone/email uniqueness guards
        IF EXISTS (SELECT 1 FROM pharmacies WHERE slug = p_slug AND is_deleted = 0) THEN
            SET p_error_code = 4;
            SET p_error_desc = 'Slug already taken';
        ELSEIF EXISTS (SELECT 1 FROM pharmacy_users WHERE email = p_owner_email AND is_deleted = 0) THEN
            SET p_error_code = 5;
            SET p_error_desc = 'Owner email already registered';
        ELSEIF EXISTS (SELECT 1 FROM portal_users WHERE email = p_owner_email AND is_deleted = 0) THEN
            SET p_error_code = 6;
            SET p_error_desc = 'Owner email already registered';
        ELSE
            INSERT INTO pharmacies (name, slug, phone, email, address, license_number, currency, subscription_plan, is_active, is_deleted, created_on)
            VALUES (p_name, p_slug, p_phone, p_email, p_address, p_license_number, p_currency, 'Starter', 1, 0, NOW());

            SET v_pharmacy_id = LAST_INSERT_ID();

            INSERT INTO pharmacy_users (pharmacy_id, role_id, first_name, last_name, email, mobile, password, is_deleted, created_by, created_on, change_password)
            VALUES (v_pharmacy_id, 2, p_owner_first, p_owner_last, p_owner_email, p_owner_mobile, p_owner_password, 0, p_created_by, NOW(), 1);

            SET v_user_id = LAST_INSERT_ID();

            -- Mirror the owner into portal_users so they can log into THIS portal
            -- as that pharmacy's Admin (role 2). NOTE: this portal validates via
            -- Rijndael-decrypt (clientlogin), so portal_password must be the
            -- Rijndael-ENCRYPTED value (NOT the BCrypt hash used for pharmacy_users).
            INSERT INTO portal_users (pharmacy_id, role_id, first_name, middle_name, last_name, email, mobile, password, avatar, locked, approved, google_authenticate, sec_key, is_deleted, created_by, created_on)
            VALUES (v_pharmacy_id, 2, p_owner_first, NULL, p_owner_last, p_owner_email, p_owner_mobile, p_portal_password, 'user-default.svg', 0, 1, 0, '', 0, p_created_by, NOW());

            COMMIT;
            SET p_pharmacy_id = v_pharmacy_id;
            SET p_user_id     = v_user_id;
        END IF;
    END IF;
END$$

-- ============================================================
-- 3d. update_pharmacy_status — activate / deactivate a pharmacy
-- ============================================================
DROP PROCEDURE IF EXISTS update_pharmacy_status$$
CREATE PROCEDURE update_pharmacy_status(
    IN p_pharmacy_id BIGINT,
    IN p_is_active   TINYINT,
    OUT p_error_code INT,
    OUT p_error_desc VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET p_error_code = 1;
        SET p_error_desc = 'Failed to update pharmacy status';
    END;

    SET p_error_code = 0;
    SET p_error_desc = 'OK';

    UPDATE pharmacies SET is_active = p_is_active WHERE id = p_pharmacy_id;
END$$

-- ============================================================
-- 3e. get_platform_stats — platform-wide summary
-- ============================================================
DROP PROCEDURE IF EXISTS get_platform_stats$$
CREATE PROCEDURE get_platform_stats()
BEGIN
    SELECT
        (SELECT COUNT(*) FROM pharmacies WHERE is_deleted = 0)                                    AS total_pharmacies,
        (SELECT COUNT(*) FROM pharmacies WHERE is_deleted = 0 AND is_active = 1)                  AS active_pharmacies,
        (SELECT COUNT(*) FROM pharmacy_users WHERE is_deleted = 0)                                AS total_users,
        (SELECT COUNT(*) FROM products WHERE is_deleted = 0)                                      AS total_products,
        (SELECT COUNT(*) FROM pharmacies WHERE is_deleted = 0
           AND subscription_plan <> 'Starter')                                                    AS subscribed_pharmacies,
        (SELECT COUNT(*) FROM pharmacies WHERE is_deleted = 0
           AND created_on >= DATE_SUB(NOW(), INTERVAL 30 DAY))                                    AS new_pharmacies_30d,
        (SELECT COUNT(*) FROM pharmacy_users WHERE is_deleted = 0
           AND created_on >= DATE_SUB(NOW(), INTERVAL 30 DAY))                                    AS new_users_30d;
END$$

-- ============================================================
-- 3f. get_platform_audit — recent platform-wide audit trail
-- ============================================================
DROP PROCEDURE IF EXISTS get_platform_audit$$
CREATE PROCEDURE get_platform_audit(
    IN p_limit INT
)
BEGIN
    SELECT id, user_name, action_type, action_description, page_accessed, client_ip_address, created_on
    FROM audit_trail
    ORDER BY created_on DESC
    LIMIT p_limit;
END$$

-- ============================================================
-- 4. get_menu — group main menus by NAME only (single entry per
--    main menu regardless of per-sub-menu icons). This keeps the
--    sidebar rendering one entry per main menu and keeps the
--    Access Control catalog in sync.
-- ============================================================
DROP PROCEDURE IF EXISTS get_menu$$
CREATE PROCEDURE get_menu(
    IN p_profile_id INT,
    IN p_type       VARCHAR(20),
    IN p_menu_name  VARCHAR(100)
)
BEGIN
    IF p_type = 'main' THEN
        SELECT main_menu_name, MIN(menu_icon) AS menu_icon, menu_order
        FROM menu_access
        WHERE role_id = p_profile_id AND can_access = 1
        GROUP BY main_menu_name, menu_order
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
