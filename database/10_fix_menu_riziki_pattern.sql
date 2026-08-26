-- ============================================================
-- MediStock — Fix Menu System to Match Riziki
-- Run these commands individually in MySQL Workbench
-- ============================================================

USE medistock;

-- 1. Create menu_access_data table (global menu definitions with icons)
CREATE TABLE IF NOT EXISTS `menu_access_data` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `main_menu_name` varchar(100) NOT NULL,
  `sub_menu_name` varchar(100) DEFAULT NULL,
  `menu_icon` varchar(100) DEFAULT 'fa-circle',
  `menu_order` int DEFAULT 0,
  `sub_menu_order` int DEFAULT 0,
  `page_url` varchar(500) DEFAULT NULL,
  `menu_type` varchar(50) DEFAULT 'ALL',
  PRIMARY KEY (`id`),
  KEY `idx_menu_data_name` (`main_menu_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Seed menu_access_data with all menu definitions
INSERT IGNORE INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type)
VALUES
('Dashboard',     'Overview',         'fa-dashboard',     1,  1,   '~/Dashboard/Index',          'ALL'),
('Inventory',     'Products',         'fa-cube',          10, 1,   '~/Products/Index',           'ALL'),
('Inventory',     'Categories',       'fa-cube',          10, 2,   '~/Products/Categories',      'ALL'),
('Inventory',     'Batches',          'fa-cube',          10, 3,   '~/Stock/Batches',            'ALL'),
('Inventory',     'Stock Adjustments','fa-cube',          10, 4,   '~/Stock/Adjustments',        'ALL'),
('Inventory',     'Stock Take',       'fa-cube',          10, 5,   '~/Stock/StockTake',          'ALL'),
('Sales',         'POS',              'fa-shopping-cart', 20, 1,   '~/Sales/POS',                'ALL'),
('Sales',         'Sales History',    'fa-shopping-cart', 20, 2,   '~/Sales/History',            'ALL'),
('Customers',     'Retail',           'fa-users',         30, 1,   '~/Customers/Retail',         'ALL'),
('Customers',     'Wholesale',        'fa-users',         30, 2,   '~/Customers/Wholesale',      'ALL'),
('Suppliers',     'Suppliers',        'fa-truck',         40, 1,   '~/Suppliers/Index',          'ALL'),
('Suppliers',     'Purchase Orders',  'fa-truck',         40, 2,   '~/Suppliers/PurchaseOrders', 'ALL'),
('Suppliers',     'Receive Stock',    'fa-truck',         40, 3,   '~/Suppliers/ReceiveStock',   'ALL'),
('Finance',       'Expenses',         'fa-money',         50, 1,   '~/Finance/Expenses',         'ALL'),
('Finance',       'Purchase Orders',  'fa-money',         50, 2,   '~/Finance/PurchaseOrders',   'ALL'),
('Reports',       'Sales Report',     'fa-bar-chart',     60, 1,   '~/Reports/Sales',            'ALL'),
('Reports',       'Stock Report',     'fa-bar-chart',     60, 2,   '~/Reports/Stock',            'ALL'),
('Reports',       'Financial Report', 'fa-bar-chart',     60, 3,   '~/Reports/Financial',        'ALL'),
('Clinical',      'Patients',         'fa-heartbeat',     70, 1,   '~/Clinical/Patients',        'ALL'),
('Clinical',      'Prescriptions',    'fa-heartbeat',     70, 2,   '~/Clinical/Prescriptions',   'ALL'),
('DDA',           'Register',         'fa-balance-scale', 75, 1,   '~/DDA/Register',             'ALL'),
('DDA',           'Report',           'fa-balance-scale', 75, 2,   '~/DDA/Report',               'ALL'),
('Settings',      'Profile',          'fa-cog',           80, 1,   '~/Settings/Profile',         'ALL'),
('Settings',      'Pharmacy',         'fa-cog',           80, 2,   '~/Settings/Pharmacy',        'ALL'),
('Admin',         'Users',            'fa-user-secret',   90, 1,   '~/Admin/Users',              'ALL');

-- 3. Drop and recreate get_menu SP to match Riziki exactly
DROP PROCEDURE IF EXISTS get_menu;

DELIMITER $$
CREATE PROCEDURE get_menu(
    IN p_profile_id INT,
    IN p_type       VARCHAR(20),
    IN p_menu_name  VARCHAR(100)
)
BEGIN
    IF p_type = 'main' THEN
        SELECT
            ma.menu_order,
            ma.main_menu_name,
            (SELECT menu_icon FROM menu_access_data
             WHERE main_menu_name = ma.main_menu_name
               AND (sub_menu_name IS NULL OR sub_menu_name = '')
             LIMIT 1) AS menu_icon
        FROM menu_access ma
        WHERE ma.can_access = 1
          AND ma.role_id = p_profile_id
          AND (ma.sub_menu_name IS NULL OR ma.sub_menu_name = '')
        ORDER BY ma.menu_order ASC;

    ELSEIF p_type = 'sub' THEN
        SELECT
            ma.sub_menu_order,
            ma.sub_menu_name,
            ma.page_url
        FROM menu_access ma
        WHERE ma.role_id = p_profile_id
          AND ma.main_menu_name = p_menu_name
          AND ma.can_access = 1
          AND (ma.sub_menu_name IS NOT NULL AND ma.sub_menu_name != '')
        ORDER BY ma.sub_menu_order ASC;

    ELSEIF p_type = 'page_url' THEN
        SELECT ma.page_url
        FROM menu_access ma
        WHERE ma.main_menu_name = p_menu_name
          AND ma.role_id = p_profile_id
          AND ma.can_access = 1
          AND (ma.sub_menu_name IS NULL OR ma.sub_menu_name = '')
        LIMIT 1;
    END IF;
END$$
DELIMITER ;
