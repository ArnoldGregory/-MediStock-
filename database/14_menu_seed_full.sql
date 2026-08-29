-- ============================================================
--  MediStock — FULL MENU SEED (menu_access + menu_access_data)
--  Source of truth: Desktop/portal/port12.sql, port13.sql
--
--  Scope:
--    * menu_access_data  = master catalog (ALL 33 items)
--    * menu_access       = per-role assignment
--        role 1 (SuperAdmin): PLATFORM ONLY (5 items)
--        role 2 (Admin)     : full pharmacy menu (28)
--        role 3 (Pharmacist): 20
--        role 4 (Staff)     : 7
--        role 5 (Cashier)   : 5
--
--  SuperAdmin is a platform-wide administrator (pharmacy management,
--  users, audit). Pharmacy operations (add medicine, sales, ...) are
--  done by role 2 (Admin): add_pharmacy_platform auto-creates that
--  owner's role-2 account in portal_users when a pharmacy is added.
--
--  Idempotent: delete-then-insert (menu_access_data has no unique key).
-- ============================================================

USE medistock;

-- ------------------------------------------------------------
-- 1. MASTER CATALOG — menu_access_data (all 31)
-- ------------------------------------------------------------
DELETE FROM menu_access_data;
ALTER TABLE menu_access_data AUTO_INCREMENT = 1;

INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type) VALUES
('Dashboard','Overview','fa-dashboard',1,1,'~/Dashboard/Index','ALL'),
('Inventory','Products','fa-cube',10,1,'~/Products/Index','ALL'),
('Inventory','Categories','fa-cube',10,2,'~/Products/Categories','ALL'),
('Inventory','Batches','fa-cube',10,3,'~/Stock/Batches','ALL'),
('Inventory','Stock Adjustments','fa-cube',10,4,'~/Stock/Adjustments','ALL'),
('Inventory','Stock Take','fa-cube',10,5,'~/Stock/StockTake','ALL'),
('Sales','POS','fa-shopping-cart',20,1,'~/Sales/POS','ALL'),
('Sales','Sales History','fa-shopping-cart',20,2,'~/Sales/History','ALL'),
('Customers','Retail','fa-users',30,1,'~/Customers/Retail','ALL'),
('Customers','Wholesale','fa-users',30,2,'~/Customers/Wholesale','ALL'),
('Suppliers','Suppliers','fa-truck',40,1,'~/Suppliers/Index','ALL'),
('Suppliers','Purchase Orders','fa-truck',40,2,'~/Suppliers/PurchaseOrders','ALL'),
('Suppliers','Receive Stock','fa-truck',40,3,'~/Suppliers/ReceiveStock','ALL'),
('Suppliers','Import Invoice','fa-file-pdf-o',40,4,'~/Suppliers/ImportInvoice','ALL'),
('Finance','Expenses','fa-money',50,1,'~/Finance/Expenses','ALL'),
('Finance','Purchase Orders','fa-money',50,2,'~/Finance/PurchaseOrders','ALL'),
('Reports','Sales Report','fa-bar-chart',60,1,'~/Reports/Sales','ALL'),
('Reports','Stock Report','fa-bar-chart',60,2,'~/Reports/Stock','ALL'),
('Reports','Financial Report','fa-bar-chart',60,3,'~/Reports/Financial','ALL'),
('AI','Smart Reorder','fa-magic',65,1,'~/AI/Index','ALL'),
('Clinical','Patients','fa-heartbeat',70,1,'~/Clinical/Patients','ALL'),
('Clinical','Prescriptions','fa-heartbeat',70,2,'~/Clinical/Prescriptions','ALL'),
('DDA','Register','fa-balance-scale',75,1,'~/DDA/Register','ALL'),
('DDA','Report','fa-balance-scale',75,2,'~/DDA/Report','ALL'),
('Settings','Profile','fa-cog',80,1,'~/Settings/Profile','ALL'),
('Settings','Pharmacy','fa-cog',80,2,'~/Settings/Pharmacy','ALL'),
('Admin','Users','fa-user-secret',90,1,'~/Admin/Users','ALL'),
('Admin','Access Control','fa-shield',90,2,'~/Admin/AccessControl','ALL'),
('Platform','Overview','fa-globe',95,1,'~/SuperAdmin/Index','SUPERADMIN'),
('Platform','Pharmacies','fa-hospital-o',95,2,'~/SuperAdmin/Pharmacies','SUPERADMIN'),
('Platform','Users','fa-users',95,3,'~/SuperAdmin/Users','SUPERADMIN'),
('Platform','Audit Log','fa-history',95,4,'~/SuperAdmin/Audit','SUPERADMIN'),
('Platform','Access Control','fa-shield',95,5,'~/Admin/AccessControl','SUPERADMIN');

-- ------------------------------------------------------------
-- 2. PER-ROLE — menu_access
-- ------------------------------------------------------------
DELETE FROM menu_access;
ALTER TABLE menu_access AUTO_INCREMENT = 1;

-- Role 1 (SuperAdmin): Platform only
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(1,'Platform','Overview','fa-globe','~/SuperAdmin/Index',1,95,1),
(1,'Platform','Pharmacies','fa-hospital-o','~/SuperAdmin/Pharmacies',1,95,2),
(1,'Platform','Users','fa-users','~/SuperAdmin/Users',1,95,3),
(1,'Platform','Audit Log','fa-history','~/SuperAdmin/Audit',1,95,4),
(1,'Platform','Access Control','fa-shield','~/Admin/AccessControl',1,95,5);

-- Role 2 (Admin): full pharmacy menu
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(2,'Dashboard','Overview','fa-dashboard','~/Dashboard/Index',1,1,1),
(2,'Inventory','Products','fa-cube','~/Products/Index',1,10,1),
(2,'Inventory','Categories','fa-cube','~/Products/Categories',1,10,2),
(2,'Inventory','Batches','fa-cube','~/Stock/Batches',1,10,3),
(2,'Inventory','Stock Adjustments','fa-cube','~/Stock/Adjustments',1,10,4),
(2,'Inventory','Stock Take','fa-cube','~/Stock/StockTake',1,10,5),
(2,'Sales','POS','fa-shopping-cart','~/Sales/POS',1,20,1),
(2,'Sales','Sales History','fa-shopping-cart','~/Sales/History',1,20,2),
(2,'Customers','Retail','fa-users','~/Customers/Retail',1,30,1),
(2,'Customers','Wholesale','fa-users','~/Customers/Wholesale',1,30,2),
(2,'Suppliers','Suppliers','fa-truck','~/Suppliers/Index',1,40,1),
(2,'Suppliers','Purchase Orders','fa-truck','~/Suppliers/PurchaseOrders',1,40,2),
(2,'Suppliers','Receive Stock','fa-truck','~/Suppliers/ReceiveStock',1,40,3),
(2,'Suppliers','Import Invoice','fa-file-pdf-o','~/Suppliers/ImportInvoice',1,40,4),
(2,'Finance','Expenses','fa-money','~/Finance/Expenses',1,50,1),
(2,'Finance','Purchase Orders','fa-money','~/Finance/PurchaseOrders',1,50,2),
(2,'Reports','Sales Report','fa-bar-chart','~/Reports/Sales',1,60,1),
(2,'Reports','Stock Report','fa-bar-chart','~/Reports/Stock',1,60,2),
(2,'Reports','Financial Report','fa-bar-chart','~/Reports/Financial',1,60,3),
(2,'AI','Smart Reorder','fa-magic','~/AI/Index',1,65,1),
(2,'Clinical','Patients','fa-heartbeat','~/Clinical/Patients',1,70,1),
(2,'Clinical','Prescriptions','fa-heartbeat','~/Clinical/Prescriptions',1,70,2),
(2,'DDA','Register','fa-balance-scale','~/DDA/Register',1,75,1),
(2,'DDA','Report','fa-balance-scale','~/DDA/Report',1,75,2),
(2,'Settings','Profile','fa-cog','~/Settings/Profile',1,80,1),
(2,'Settings','Pharmacy','fa-cog','~/Settings/Pharmacy',1,80,2),
(2,'Admin','Users','fa-user-secret','~/Admin/Users',1,90,1),
(2,'Admin','Access Control','fa-shield','~/Admin/AccessControl',1,90,2);

-- Role 3 (Pharmacist)
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(3,'Dashboard','Overview','fa-dashboard','~/Dashboard/Index',1,1,1),
(3,'Inventory','Products','fa-cube','~/Products/Index',1,10,1),
(3,'Inventory','Categories','fa-cube','~/Products/Categories',1,10,2),
(3,'Inventory','Batches','fa-cube','~/Stock/Batches',1,10,3),
(3,'Inventory','Stock Adjustments','fa-cube','~/Stock/Adjustments',1,10,4),
(3,'Sales','POS','fa-shopping-cart','~/Sales/POS',1,20,1),
(3,'Sales','Sales History','fa-shopping-cart','~/Sales/History',1,20,2),
(3,'Customers','Retail','fa-users','~/Customers/Retail',1,30,1),
(3,'Customers','Wholesale','fa-users','~/Customers/Wholesale',1,30,2),
(3,'Suppliers','Suppliers','fa-truck','~/Suppliers/Index',1,40,1),
(3,'Suppliers','Purchase Orders','fa-truck','~/Suppliers/PurchaseOrders',1,40,2),
(3,'Suppliers','Import Invoice','fa-file-pdf-o','~/Suppliers/ImportInvoice',1,40,4),
(3,'Reports','Sales Report','fa-bar-chart','~/Reports/Sales',1,60,1),
(3,'Reports','Stock Report','fa-bar-chart','~/Reports/Stock',1,60,2),
(3,'AI','Smart Reorder','fa-magic','~/AI/Index',1,65,1),
(3,'Clinical','Patients','fa-heartbeat','~/Clinical/Patients',1,70,1),
(3,'Clinical','Prescriptions','fa-heartbeat','~/Clinical/Prescriptions',1,70,2),
(3,'DDA','Register','fa-balance-scale','~/DDA/Register',1,75,1),
(3,'DDA','Report','fa-balance-scale','~/DDA/Report',1,75,2),
(3,'Settings','Profile','fa-cog','~/Settings/Profile',1,80,1);

-- Role 4 (Staff)
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(4,'Dashboard','Overview','fa-dashboard','~/Dashboard/Index',1,1,1),
(4,'Inventory','Products','fa-cube','~/Products/Index',1,10,1),
(4,'Inventory','Batches','fa-cube','~/Stock/Batches',1,10,3),
(4,'Sales','POS','fa-shopping-cart','~/Sales/POS',1,20,1),
(4,'Sales','Sales History','fa-shopping-cart','~/Sales/History',1,20,2),
(4,'Customers','Retail','fa-users','~/Customers/Retail',1,30,1),
(4,'Settings','Profile','fa-cog','~/Settings/Profile',1,80,1);

-- Role 5 (Cashier)
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(5,'Dashboard','Overview','fa-dashboard','~/Dashboard/Index',1,1,1),
(5,'Sales','POS','fa-shopping-cart','~/Sales/POS',1,20,1),
(5,'Sales','Sales History','fa-shopping-cart','~/Sales/History',1,20,2),
(5,'Customers','Retail','fa-users','~/Customers/Retail',1,30,1),
(5,'Settings','Profile','fa-cog','~/Settings/Profile',1,80,1);

-- Verify: catalog = 33; access per role 1=5, 2=28, 3=20, 4=7, 5=5
