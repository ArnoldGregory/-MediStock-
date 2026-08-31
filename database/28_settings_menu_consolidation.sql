-- ============================================================
-- 28_settings_menu_consolidation.sql
-- Consolidate the duplicate 'Profile' and 'Pharmacy' settings
-- menu items into a single 'Settings' entry pointing at the
-- merged view (~/Settings/Index).
-- ============================================================

-- 1) menu_access_data: rename Profile -> Settings (Index), drop Pharmacy.
UPDATE menu_access_data
SET sub_menu_name = 'Settings',
    page_url      = '~/Settings/Index'
WHERE main_menu_name = 'Settings'
  AND page_url = '~/Settings/Profile';

DELETE FROM menu_access_data
WHERE main_menu_name = 'Settings'
  AND page_url = '~/Settings/Pharmacy';

-- 2) menu_access: same consolidation for the per-role access rows.
UPDATE menu_access
SET sub_menu_name = 'Settings',
    page_url      = '~/Settings/Index'
WHERE main_menu_name = 'Settings'
  AND sub_menu_name = 'Profile';

DELETE FROM menu_access
WHERE main_menu_name = 'Settings'
  AND sub_menu_name = 'Pharmacy';
