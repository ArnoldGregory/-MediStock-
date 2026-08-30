-- ============================================================
--  MediStock — AI / Drug Interactions menu
--  Idempotent: adds one menu item under AI (sub 2) for roles 2,3.
-- ============================================================

USE medistock;

DELETE FROM menu_access_data WHERE main_menu_name = 'AI' AND sub_menu_name = 'Drug Interactions';
INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type) VALUES
('AI','Drug Interactions','fa-medkit',65,2,'~/AI/DrugInteractions','ALL');

DELETE FROM menu_access WHERE main_menu_name = 'AI' AND sub_menu_name = 'Drug Interactions';
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) VALUES
(2,'AI','Drug Interactions','fa-medkit','~/AI/DrugInteractions',1,65,2),
(3,'AI','Drug Interactions','fa-medkit','~/AI/DrugInteractions',1,65,2);