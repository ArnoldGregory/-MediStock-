-- ============================================================================
-- 34_fix_user_delete_and_views.sql
-- Fixes:
--   1. delete_records  — final version (migrations 25/33) only soft-deletes
--      pharmacy_users, but login reads portal_users (role 1,2) and
--      p_external_portal_user (role 3,4,5). Deleted users could still log in.
--      Add all three user branches.
--   2. get_all_users   — reads pharmacy_users (legacy copy). Users are split
--      into portal_users + p_external_portal_user. Rewrite to read both,
--      joined to pharmacies, so the SuperAdmin users grid matches the
--      tables used for login and shows no duplicate rows.
--   3. get_user_by_id  — only looked at portal_users; extend to fall back to
--      p_external_portal_user so Admin edit falls inside id-4/id-5 users.
--   4. get_users_by_pharmacy — reads portal_users only. Union with
--      p_external_portal_user so the Admin Users page lists everyone in the
--      pharmacy (portal + external) and matches admin API dtPortal/dtExternal.
-- Idempotent.
-- ============================================================================

USE medistock;

DELIMITER $$

-- ------------------------------------------------------------
-- 1. delete_records — add portal_user / p_external_portal_user branches
-- ------------------------------------------------------------
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
    ELSEIF p_module = 'pharmacy_user' OR p_module = 'portal_user' OR p_module = 'p_external_portal_user' OR p_module = 'user' THEN
        UPDATE pharmacy_users SET is_deleted = 1 WHERE id = p_recordid;
        UPDATE portal_users SET is_deleted = 1 WHERE id = p_recordid;
        UPDATE p_external_portal_user SET is_deleted = 1 WHERE id = p_recordid;
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

-- ------------------------------------------------------------
-- 2. get_all_users — read portal_users + p_external_portal_user
-- ------------------------------------------------------------
DROP PROCEDURE IF EXISTS `get_all_users`$$

CREATE PROCEDURE `get_all_users`(
    IN p_exclude_user_id BIGINT
)
BEGIN
    SELECT
        'portal' AS user_type,
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
    FROM portal_users u
    LEFT JOIN pharmacies p ON p.id = u.pharmacy_id
    WHERE u.id <> p_exclude_user_id

    UNION ALL

    SELECT
        'external' AS user_type,
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
            WHEN 3 THEN 'Pharmacist'
            WHEN 4 THEN 'Staff'
            WHEN 5 THEN 'Cashier'
            ELSE 'Staff'
        END AS role_name
    FROM p_external_portal_user u
    LEFT JOIN pharmacies p ON p.id = u.pharmacy_id
    WHERE u.id <> p_exclude_user_id

    ORDER BY pharmacy_name, first_name;
END$$

-- ------------------------------------------------------------
-- 3. get_user_by_id — fall back to p_external_portal_user
-- ------------------------------------------------------------
DROP PROCEDURE IF EXISTS `get_user_by_id`$$

CREATE PROCEDURE `get_user_by_id`(
    IN p_id BIGINT
)
BEGIN
    SELECT id, pharmacy_id, role_id, first_name, last_name, email,
           mobile, avatar, locked, IF(locked=1,0,1) AS is_active, created_on
    FROM portal_users
    WHERE id = p_id AND is_deleted = 0
    LIMIT 1;

    IF ROW_COUNT() = 0 THEN
        SELECT id, pharmacy_id, role_id, first_name, last_name, email,
               'user-default.svg' AS avatar, locked, IF(locked=1,0,1) AS is_active, created_on
        FROM p_external_portal_user
        WHERE id = p_id AND is_deleted = 0
        LIMIT 1;
    END IF;
END$$

-- ------------------------------------------------------------
-- 4. get_users_by_pharmacy — portal + external union
-- ------------------------------------------------------------
DROP PROCEDURE IF EXISTS `get_users_by_pharmacy`$$

CREATE PROCEDURE `get_users_by_pharmacy`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT id, pharmacy_id, role_id, first_name, last_name, email,
           mobile, avatar, locked, IF(locked=1,0,1) AS is_active, created_on
    FROM portal_users
    WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0

    UNION ALL

    SELECT id, pharmacy_id, role_id, first_name, last_name, email,
           mobile, 'user-default.svg' AS avatar, locked, IF(locked=1,0,1) AS is_active, created_on
    FROM p_external_portal_user
    WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0

    ORDER BY first_name;
END$$

DELIMITER ;