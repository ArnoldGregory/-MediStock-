-- ============================================================
--  MediStock — Menu Seed Data
--  role_id: 1 = SuperAdmin, 2 = Admin, 3 = Pharmacist, 4 = Staff, 5 = Cashier
-- ============================================================

USE medistock;

-- ── SUPER ADMIN (role_id = 1) ──
INSERT IGNORE INTO menu_access (role_id, main_menu_name, sub_menu_name, page_url, can_access, menu_order, sub_menu_order)
VALUES
(1, 'Dashboard',     'Overview',         '~/Admin/Dashboard',          1, 1, 1),
(1, 'Inventory',     'Products',         '~/Products/Index',           1, 10, 1),
(1, 'Inventory',     'Categories',       '~/Products/Categories',      1, 10, 2),
(1, 'Inventory',     'Batches',          '~/Stock/Batches',            1, 10, 3),
(1, 'Inventory',     'Stock Adjustments', '~/Stock/Adjustments',       1, 10, 4),
(1, 'Inventory',     'Stock Take',       '~/Stock/StockTake',          1, 10, 5),
(1, 'Sales',         'POS',              '~/Sales/POS',                1, 20, 1),
(1, 'Sales',         'Sales History',    '~/Sales/History',            1, 20, 2),
(1, 'Customers',     'Retail',           '~/Customers/Retail',         1, 30, 1),
(1, 'Customers',     'Wholesale',        '~/Customers/Wholesale',      1, 30, 2),
(1, 'Suppliers',     'Suppliers',        '~/Suppliers/Index',          1, 40, 1),
(1, 'Suppliers',     'Purchase Orders',  '~/Suppliers/PurchaseOrders', 1, 40, 2),
(1, 'Suppliers',     'Receive Stock',    '~/Suppliers/ReceiveStock',   1, 40, 3),
(1, 'Finance',       'Expenses',         '~/Finance/Expenses',         1, 50, 1),
(1, 'Finance',       'Purchase Orders',  '~/Finance/PurchaseOrders',   1, 50, 2),
(1, 'Reports',       'Sales Report',     '~/Reports/Sales',            1, 60, 1),
(1, 'Reports',       'Stock Report',     '~/Reports/Stock',            1, 60, 2),
(1, 'Reports',       'Financial Report', '~/Reports/Financial',        1, 60, 3),
(1, 'Clinical',      'Patients',         '~/Clinical/Patients',        1, 70, 1),
(1, 'Clinical',      'Prescriptions',    '~/Clinical/Prescriptions',   1, 70, 2),
(1, 'DDA',           'Register',         '~/DDA/Register',             1, 75, 1),
(1, 'DDA',           'Report',           '~/DDA/Report',               1, 75, 2),
(1, 'Settings',      'Profile',          '~/Settings/Profile',         1, 80, 1),
(1, 'Settings',      'Pharmacy',         '~/Settings/Pharmacy',        1, 80, 2),
(1, 'Admin',         'Users',            '~/Admin/Users',              1, 90, 1);

-- ── ADMIN (role_id = 2) ──
INSERT IGNORE INTO menu_access (role_id, main_menu_name, sub_menu_name, page_url, can_access, menu_order, sub_menu_order)
VALUES
(2, 'Dashboard',     'Overview',         '~/Dashboard/Index',          1, 1, 1),
(2, 'Inventory',     'Products',         '~/Products/Index',           1, 10, 1),
(2, 'Inventory',     'Categories',       '~/Products/Categories',      1, 10, 2),
(2, 'Inventory',     'Batches',          '~/Stock/Batches',            1, 10, 3),
(2, 'Inventory',     'Stock Adjustments', '~/Stock/Adjustments',       1, 10, 4),
(2, 'Inventory',     'Stock Take',       '~/Stock/StockTake',          1, 10, 5),
(2, 'Sales',         'POS',              '~/Sales/POS',                1, 20, 1),
(2, 'Sales',         'Sales History',    '~/Sales/History',            1, 20, 2),
(2, 'Customers',     'Retail',           '~/Customers/Retail',         1, 30, 1),
(2, 'Customers',     'Wholesale',        '~/Customers/Wholesale',      1, 30, 2),
(2, 'Suppliers',     'Suppliers',        '~/Suppliers/Index',          1, 40, 1),
(2, 'Suppliers',     'Purchase Orders',  '~/Suppliers/PurchaseOrders', 1, 40, 2),
(2, 'Suppliers',     'Receive Stock',    '~/Suppliers/ReceiveStock',   1, 40, 3),
(2, 'Finance',       'Expenses',         '~/Finance/Expenses',         1, 50, 1),
(2, 'Finance',       'Purchase Orders',  '~/Finance/PurchaseOrders',   1, 50, 2),
(2, 'Reports',       'Sales Report',     '~/Reports/Sales',            1, 60, 1),
(2, 'Reports',       'Stock Report',     '~/Reports/Stock',            1, 60, 2),
(2, 'Reports',       'Financial Report', '~/Reports/Financial',        1, 60, 3),
(2, 'Clinical',      'Patients',         '~/Clinical/Patients',        1, 70, 1),
(2, 'Clinical',      'Prescriptions',    '~/Clinical/Prescriptions',   1, 70, 2),
(2, 'DDA',           'Register',         '~/DDA/Register',             1, 75, 1),
(2, 'DDA',           'Report',           '~/DDA/Report',               1, 75, 2),
(2, 'Settings',      'Profile',          '~/Settings/Profile',         1, 80, 1),
(2, 'Settings',      'Pharmacy',         '~/Settings/Pharmacy',        1, 80, 2),
(2, 'Admin',         'Users',            '~/Admin/Users',              1, 90, 1);

-- ── PHARMACIST (role_id = 3) ──
INSERT IGNORE INTO menu_access (role_id, main_menu_name, sub_menu_name, page_url, can_access, menu_order, sub_menu_order)
VALUES
(3, 'Dashboard',     'Overview',         '~/Dashboard/Index',          1, 1, 1),
(3, 'Inventory',     'Products',         '~/Products/Index',           1, 10, 1),
(3, 'Inventory',     'Categories',       '~/Products/Categories',      1, 10, 2),
(3, 'Inventory',     'Batches',          '~/Stock/Batches',            1, 10, 3),
(3, 'Inventory',     'Stock Adjustments', '~/Stock/Adjustments',       1, 10, 4),
(3, 'Sales',         'POS',              '~/Sales/POS',                1, 20, 1),
(3, 'Sales',         'Sales History',    '~/Sales/History',            1, 20, 2),
(3, 'Customers',     'Retail',           '~/Customers/Retail',         1, 30, 1),
(3, 'Customers',     'Wholesale',        '~/Customers/Wholesale',      1, 30, 2),
(3, 'Suppliers',     'Suppliers',        '~/Suppliers/Index',          1, 40, 1),
(3, 'Suppliers',     'Purchase Orders',  '~/Suppliers/PurchaseOrders', 1, 40, 2),
(3, 'Clinical',      'Patients',         '~/Clinical/Patients',        1, 70, 1),
(3, 'Clinical',      'Prescriptions',    '~/Clinical/Prescriptions',   1, 70, 2),
(3, 'DDA',           'Register',         '~/DDA/Register',             1, 75, 1),
(3, 'DDA',           'Report',           '~/DDA/Report',               1, 75, 2),
(3, 'Reports',       'Sales Report',     '~/Reports/Sales',            1, 60, 1),
(3, 'Reports',       'Stock Report',     '~/Reports/Stock',            1, 60, 2),
(3, 'Settings',      'Profile',          '~/Settings/Profile',         1, 80, 1);

-- ── STAFF (role_id = 4) ──
INSERT IGNORE INTO menu_access (role_id, main_menu_name, sub_menu_name, page_url, can_access, menu_order, sub_menu_order)
VALUES
(4, 'Dashboard',     'Overview',         '~/Dashboard/Index',          1, 1, 1),
(4, 'Inventory',     'Products',         '~/Products/Index',           1, 10, 1),
(4, 'Inventory',     'Batches',          '~/Stock/Batches',            1, 10, 3),
(4, 'Sales',         'POS',              '~/Sales/POS',                1, 20, 1),
(4, 'Sales',         'Sales History',    '~/Sales/History',            1, 20, 2),
(4, 'Customers',     'Retail',           '~/Customers/Retail',         1, 30, 1),
(4, 'Settings',      'Profile',          '~/Settings/Profile',         1, 80, 1);

-- ── CASHIER (role_id = 5) ──
INSERT IGNORE INTO menu_access (role_id, main_menu_name, sub_menu_name, page_url, can_access, menu_order, sub_menu_order)
VALUES
(5, 'Dashboard',     'Overview',         '~/Dashboard/Index',          1, 1, 1),
(5, 'Sales',         'POS',              '~/Sales/POS',                1, 20, 1),
(5, 'Sales',         'Sales History',    '~/Sales/History',            1, 20, 2),
(5, 'Customers',     'Retail',           '~/Customers/Retail',         1, 30, 1),
(5, 'Settings',      'Profile',          '~/Settings/Profile',         1, 80, 1);
