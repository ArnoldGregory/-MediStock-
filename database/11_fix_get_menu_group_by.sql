-- ============================================================
-- MediStock — Fix get_menu SP (GROUP BY pattern for existing data)
-- Run commands individually in MySQL Workbench
-- ============================================================

USE medistock;

-- 1. Recreate get_menu SP using GROUP BY
DROP PROCEDURE IF EXISTS get_menu;

DELIMITER $$
CREATE PROCEDURE get_menu(
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

-- 2. Test
CALL get_menu(1, 'main', '');
CALL get_menu(1, 'sub', 'Dashboard');
CALL get_menu(1, 'sub', 'Inventory');
