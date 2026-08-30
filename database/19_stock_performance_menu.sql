-- ============================================================
--  MediStock — Reports / Stock Performance menu
--  Idempotent: adds one menu item under Reports (sub 4) for roles 2,3.
-- ============================================================

USE medistock;

DELETE FROM menu_access_data WHERE main_menu_name = 'Reports' AND sub_menu_name = 'Stock Performance';
INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type) VALUES
('Reports','Stock Performance','fa-area-chart',60,4,'~/Reports/StockPerformance','ALL');

DELETE FROM menu_access WHERE main_menu_name = 'Reports' AND sub_menu_name = 'Stock Performance';
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(2,'Reports','Stock Performance','fa-area-chart','~/Reports/StockPerformance',1,60,4),
(3,'Reports','Stock Performance','fa-area-chart','~/Reports/StockPerformance',1,60,4);