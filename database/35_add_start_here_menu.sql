-- ============================================================================
-- 35_add_start_here_menu.sql
-- Adds a 'Start Here' guided-tour menu item for every pharmacy role.
--   menu_access_data : master catalog row
--   menu_access      : per-role rows (2 Admin, 3 Pharmacist, 4 Staff, 5 Cashier)
--   Dashboard overview card reformatted to deep-link the wizard (view-level).
-- Idempotent.
-- ============================================================================

USE medistock;

-- 1. Master catalog
INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type)
SELECT 'Dashboard', 'Start Here', 'fa-road', 1, 0, '~/Dashboard/StartHere', 'ALL'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM menu_access_data WHERE page_url = '~/Dashboard/StartHere'
);

-- 2. Per-role access rows (skip SuperAdmin role 1 — platform only)
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order)
SELECT r.role_id, 'Dashboard', 'Start Here', 'fa-road', '~/Dashboard/StartHere', 1, 1, 0
FROM (SELECT 2 AS role_id UNION SELECT 3 UNION SELECT 4 UNION SELECT 5) r
WHERE NOT EXISTS (
    SELECT 1 FROM menu_access ma
    WHERE ma.role_id = r.role_id AND ma.main_menu_name = 'Dashboard' AND ma.sub_menu_name = 'Start Here'
);