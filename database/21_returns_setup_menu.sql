-- ============================================================
--  MediStock — Sales Returns & Setup Checklist menu items
--  Idempotent: delete-then-insert.
-- ============================================================

USE medistock;

DELETE FROM menu_access_data WHERE (main_menu_name = 'Sales' AND sub_menu_name = 'Returns');
INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type) VALUES
('Sales','Returns','fa-undo',50,3,'~/Sales/Returns','ALL');

DELETE FROM menu_access WHERE main_menu_name = 'Sales' AND sub_menu_name = 'Returns';
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(2,'Sales','Returns','fa-undo','~/Sales/Returns',1,50,3),
(3,'Sales','Returns','fa-undo','~/Sales/Returns',1,50,3);

DELETE FROM menu_access_data WHERE (main_menu_name = 'Settings' AND sub_menu_name = 'Setup Checklist');
INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type) VALUES
('Settings','Setup Checklist','fa-check-square-o',70,3,'~/Settings/Setup','ALL');

DELETE FROM menu_access WHERE main_menu_name = 'Settings' AND sub_menu_name = 'Setup Checklist';
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(2,'Settings','Setup Checklist','fa-check-square-o','~/Settings/Setup',1,70,3),
(3,'Settings','Setup Checklist','fa-check-square-o','~/Settings/Setup',1,70,3);