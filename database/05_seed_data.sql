-- ============================================================
--  MediStock — Seed Data
--  Run AFTER 01_tables.sql through 04_menu_seed.sql
--  Creates default pharmacy + super admin user
-- ============================================================

USE medistock;

-- ── DEFAULT PHARMACY ──
INSERT IGNORE INTO pharmacies (id, name, slug, phone, email, address, license_number, currency, subscription_plan, is_active, created_on)
VALUES (1, 'Demo Pharmacy', 'demo', '+254700000000', 'admin@demo pharmacy.co.ke', 'Nairobi, Kenya', 'PH-001', 'KES', 'Enterprise', 1, NOW());

-- ── SUPER ADMIN USER ──
-- Password: password (encrypted with Rijndael/AES — same key as CryptoHelper.cs)
INSERT IGNORE INTO pharmacy_users (id, pharmacy_id, role_id, first_name, middle_name, last_name, email, mobile, password, avatar, locked, change_password, is_deleted, created_on)
VALUES (1, 1, 1, 'Arnold', 'Gregory', 'Omondi', 'omondiarnold06@gmail.com', '+254700000000', 'tTAwzY33mxj3Ie0XEmq4xQ==', 'user-default.svg', 0, 0, 0, NOW());
