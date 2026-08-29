-- ============================================================
--  MediStock — AI menus (Smart Reorder + Invoice Import)
--  Run AFTER 02_generic_procedures.sql (get_records sales_demand branch)
--  Adds "AI / Smart Reorder" and "Suppliers / Import Invoice" menus.
-- ============================================================

USE medistock;

DELETE FROM menu_access_data WHERE main_menu_name = 'AI' OR (main_menu_name = 'Suppliers' AND sub_menu_name = 'Import Invoice');
INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type) VALUES
('AI','Smart Reorder','fa-magic',65,1,'~/AI/Index','ALL'),
('Suppliers','Import Invoice','fa-file-pdf-o',40,4,'~/Suppliers/ImportInvoice','ALL');

DELETE FROM menu_access WHERE main_menu_name = 'AI' OR (main_menu_name = 'Suppliers' AND sub_menu_name = 'Import Invoice');
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(2,'AI','Smart Reorder','fa-magic','~/AI/Index',1,65,1),
(3,'AI','Smart Reorder','fa-magic','~/AI/Index',1,65,1),
(2,'Suppliers','Import Invoice','fa-file-pdf-o','~/Suppliers/ImportInvoice',1,40,4),
(3,'Suppliers','Import Invoice','fa-file-pdf-o','~/Suppliers/ImportInvoice',1,40,4);