-- ============================================================
-- 29_remove_duplicate_settings_menu.sql
-- Collapse the 'Settings' menu to a single entry. After
-- migration 28 there is a 'Settings' main menu with two children:
--   - Settings        (~/Settings/Index)
--   - Setup Checklist (~/Settings/Setup)
-- The child literally named 'Settings' duplicated the parent label,
-- which looked like two 'Settings' items. This migration:
--   1) keeps a single 'Settings' entry -> ~/Settings/Index
--   2) moves 'Setup Checklist' into its own top-level menu so it
--      remains reachable.
--
-- Idempotent: safe to re-run.
-- ============================================================

-- 1) menu_access_data -------------------------------------------
--    a) Ensure exactly ONE single-child 'Settings' entry exists.
--       (INSERT IGNORE via explicit existence check so re-runs are safe.)
INSERT INTO menu_access_data (main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type)
SELECT 'Settings', 'Settings', 'fa-cog', 80, 1, '~/Settings/Index', 'ALL'
WHERE NOT EXISTS (
    SELECT 1 FROM menu_access_data
    WHERE main_menu_name = 'Settings' AND sub_menu_name = 'Settings' AND page_url = '~/Settings/Index'
);

--    b) Remove any duplicate Settings children left over (keep one).
DELETE d1 FROM menu_access_data d1
JOIN menu_access_data d2 ON d1.main_menu_name = d2.main_menu_name
    AND d1.sub_menu_name = d2.sub_menu_name
    AND d1.page_url = d2.page_url
    AND d1.id > d2.id
WHERE d1.main_menu_name = 'Settings' AND d1.sub_menu_name = 'Settings';

--    c) Promote 'Setup Checklist' into its own top-level main menu
--       with a single clickable child.
UPDATE menu_access_data
SET main_menu_name = 'Setup Checklist',
    sub_menu_name  = 'Setup Checklist',
    menu_icon      = 'fa-check-square-o',
    menu_order     = 85,
    sub_menu_order = 1
WHERE main_menu_name = 'Settings'
  AND sub_menu_name  = 'Setup Checklist';

-- 2) menu_access (per-role) -------------------------------------
--    a) Ensure each role with the old settings group gets a single
--       'Settings' entry.
INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order)
SELECT ma.role_id, 'Settings', 'Settings', 'fa-cog', '~/Settings/Index', 1, 80, 1
FROM menu_access ma
WHERE ma.main_menu_name = 'Settings'
  AND ma.sub_menu_name = 'Setup Checklist'
  AND NOT EXISTS (
      SELECT 1 FROM menu_access m2
      WHERE m2.role_id = ma.role_id
        AND m2.main_menu_name = 'Settings' AND m2.sub_menu_name = 'Settings'
  );

--    b) Dedupe Settings children per role.
DELETE a1 FROM menu_access a1
JOIN menu_access a2 ON a1.role_id = a2.role_id
    AND a1.main_menu_name = a2.main_menu_name
    AND a1.sub_menu_name = a2.sub_menu_name
    AND a1.page_url = a2.page_url
    AND a1.id > a2.id
WHERE a1.main_menu_name = 'Settings' AND a1.sub_menu_name = 'Settings';

--    c) Promote 'Setup Checklist' per-role to its own top-level
--       single-child entry.
UPDATE menu_access
SET sub_menu_name = 'Setup Checklist',
    page_url      = '~/Settings/Setup',
    menu_icon     = 'fa-check-square-o',
    menu_order     = 85,
    sub_menu_order = 1
WHERE main_menu_name = 'Setup Checklist'
  AND (sub_menu_name = 'Setup Checklist');

--    d) Move any stragglers still grouped under 'Settings' group name.
UPDATE menu_access
SET main_menu_name = 'Setup Checklist'
WHERE sub_menu_name = 'Setup Checklist'
  AND main_menu_name <> 'Setup Checklist';
