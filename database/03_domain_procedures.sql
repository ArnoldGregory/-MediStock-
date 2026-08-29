-- ============================================================
--  MediStock — Domain-Specific Stored Procedures
--  Run AFTER 01_tables.sql and 02_generic_procedures.sql
--  Covers: Auth, Products, Sales, Customers, Suppliers,
--          Finance, Clinical, DDA, Stock, Dashboard,
--          Settings, Notifications
-- ============================================================

USE medistock;

DELIMITER $$

-- ============================================================
-- 1. AUTH — add_pharmacy
-- ============================================================
DROP PROCEDURE IF EXISTS `add_pharmacy`$$
CREATE PROCEDURE `add_pharmacy`(
    IN  in_name        VARCHAR(200),
    IN  in_slug        VARCHAR(100),
    IN  in_address     TEXT,
    IN  in_phone       VARCHAR(50),
    IN  in_email       VARCHAR(200),
    IN  in_license_no  VARCHAR(100),
    IN  in_owner_name  VARCHAR(200),
    IN  in_created_by  BIGINT,
    OUT p_id           BIGINT
)
BEGIN
    SET p_id = 0;
    IF EXISTS (SELECT 1 FROM pharmacies WHERE slug = in_slug AND is_deleted = 0) THEN
        SET p_id = 0;
    ELSE
        INSERT INTO pharmacies
            (name, slug, phone, email, address, license_number, owner_name, created_on)
        VALUES
            (in_name, in_slug, in_phone, in_email, in_address, in_license_no, in_owner_name, NOW());
        SET p_id = LAST_INSERT_ID();
    END IF;
END$$

-- ============================================================
-- 1. AUTH — add_user
-- ============================================================
DROP PROCEDURE IF EXISTS `add_user`$$
CREATE PROCEDURE `add_user`(
    IN  in_pharmacy_id  BIGINT,
    IN  in_role_id      INT,
    IN  in_first_name   VARCHAR(100),
    IN  in_last_name    VARCHAR(100),
    IN  in_email        VARCHAR(200),
    IN  in_password     VARCHAR(200),
    IN  in_phone        VARCHAR(50),
    IN  in_is_active    TINYINT,
    IN  in_created_by   BIGINT,
    OUT p_id            BIGINT
)
BEGIN
    DECLARE v_locked TINYINT DEFAULT 0;
    SET p_id = 0;
    SET v_locked = IF(COALESCE(in_is_active, 1) = 1, 0, 1);

    IF COALESCE(in_role_id, 3) IN (1, 2) THEN
        IF EXISTS (SELECT 1 FROM portal_users WHERE email = in_email AND is_deleted = 0) THEN
            SET p_id = 0;
        ELSE
            INSERT INTO portal_users
                (pharmacy_id, role_id, first_name, last_name,
                 email, mobile, PASSWORD, locked, approved, created_by, created_on)
            VALUES
                (in_pharmacy_id, in_role_id, in_first_name, in_last_name,
                 in_email, in_phone, in_password, v_locked, 1, in_created_by, NOW());
            SET p_id = LAST_INSERT_ID();
        END IF;
    ELSE
        IF EXISTS (SELECT 1 FROM p_external_portal_user WHERE email = in_email AND is_deleted = 0) THEN
            SET p_id = 0;
        ELSE
            INSERT INTO p_external_portal_user
                (pharmacy_id, role_id, first_name, last_name,
                 email, mobile, PASSWORD, locked, created_by, created_on)
            VALUES
                (in_pharmacy_id, in_role_id, in_first_name, in_last_name,
                 in_email, in_phone, in_password, v_locked, in_created_by, NOW());
            SET p_id = LAST_INSERT_ID();
        END IF;
    END IF;
END$$

-- ============================================================
-- 1. AUTH — validate_login
-- ============================================================
DROP PROCEDURE IF EXISTS `validate_login`$$
CREATE PROCEDURE `validate_login`(
    IN username    VARCHAR(200),
    IN profiletype VARCHAR(50)
)
BEGIN
    IF LOWER(profiletype) = 'admin' THEN
        SELECT id, pharmacy_id, role_id, first_name, middle_name, last_name,
               email, mobile, password, avatar, locked,
               0 AS change_password, 0 AS failed_login_attempts,
               google_authenticate, sec_key
        FROM portal_users
        WHERE email = username AND is_deleted = 0
        LIMIT 1;
    ELSE
        SELECT id, pharmacy_id, role_id, first_name, middle_name, last_name,
               email, mobile, password, avatar, locked,
               change_password, failed_login_attempts,
               google_authenticate, sec_key
        FROM p_external_portal_user
        WHERE email = username AND is_deleted = 0
        LIMIT 1;
    END IF;
END$$

-- ============================================================
-- 1. AUTH — add_refresh_token
-- ============================================================
DROP PROCEDURE IF EXISTS `add_refresh_token`$$
CREATE PROCEDURE `add_refresh_token`(
    IN p_user_id     BIGINT,
    IN p_hashed_token VARCHAR(500),
    IN p_expires_at  DATETIME
)
BEGIN
    INSERT INTO refresh_tokens (user_id, token, expires_at, created_on)
    VALUES (p_user_id, p_hashed_token, p_expires_at, NOW());
END$$

-- ============================================================
-- 1. AUTH — revoke_refresh_token
-- ============================================================
DROP PROCEDURE IF EXISTS `revoke_refresh_token`$$
CREATE PROCEDURE `revoke_refresh_token`(
    IN p_token VARCHAR(500),
    IN p_ip    VARCHAR(100)
)
BEGIN
    UPDATE refresh_tokens
    SET revoked_at = NOW(), revoked_by_ip = p_ip
    WHERE token = p_token AND revoked_at IS NULL;
END$$

-- ============================================================
-- 1. AUTH — revoke_all_user_refresh_tokens
-- ============================================================
DROP PROCEDURE IF EXISTS `revoke_all_user_refresh_tokens`$$
CREATE PROCEDURE `revoke_all_user_refresh_tokens`(
    IN p_user_id BIGINT
)
BEGIN
    UPDATE refresh_tokens
    SET revoked_at = NOW()
    WHERE user_id = p_user_id AND revoked_at IS NULL;
END$$

-- ============================================================
-- 1. AUTH — get_active_refresh_tokens
-- ============================================================
DROP PROCEDURE IF EXISTS `get_active_refresh_tokens`$$
CREATE PROCEDURE `get_active_refresh_tokens`()
BEGIN
    SELECT id, user_id, token, expires_at, created_on
    FROM refresh_tokens
    WHERE revoked_at IS NULL AND expires_at > NOW();
END$$

-- ============================================================
-- 1. AUTH — riziki_save_otp
-- ============================================================
DROP PROCEDURE IF EXISTS `riziki_save_otp`$$
CREATE PROCEDURE `riziki_save_otp`(
    IN  in_user_id    BIGINT,
    IN  in_user_type  VARCHAR(20),
    IN  in_email      VARCHAR(200),
    IN  in_mobile     VARCHAR(50),
    IN  in_otp_code   VARCHAR(10),
    IN  in_purpose    VARCHAR(50),
    IN  in_otp_ref    VARCHAR(100),
    OUT out_id         BIGINT
)
BEGIN
    SET out_id = 0;
    INSERT INTO otp_records
        (user_id, user_type, email, mobile, otp_code, purpose, otp_ref, verified, expires_at, created_on)
    VALUES
        (in_user_id, in_user_type, in_email, in_mobile, in_otp_code, in_purpose, in_otp_ref, 0,
         DATE_ADD(NOW(), INTERVAL 15 MINUTE), NOW());
    SET out_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 1. AUTH — riziki_verify_otp
-- ============================================================
DROP PROCEDURE IF EXISTS `riziki_verify_otp`$$
CREATE PROCEDURE `riziki_verify_otp`(
    IN in_email    VARCHAR(200),
    IN in_otp_code VARCHAR(10),
    IN in_otp_ref  VARCHAR(100),
    IN in_purpose  VARCHAR(50)
)
BEGIN
    SELECT '1' AS valid, user_id, user_type, email
    FROM otp_records
    WHERE email = in_email
      AND otp_code = in_otp_code
      AND otp_ref = in_otp_ref
      AND purpose = in_purpose
      AND verified = 0
      AND expires_at > NOW()
    ORDER BY id DESC
    LIMIT 1;
END$$

-- ============================================================
-- 1. AUTH — client_password_reset
-- ============================================================
DROP PROCEDURE IF EXISTS `client_password_reset`$$
CREATE PROCEDURE `client_password_reset`(
    IN p_email       VARCHAR(200),
    IN p_password    VARCHAR(200),
    IN profiletype VARCHAR(50)
)
BEGIN
    IF profiletype = 'pharmacy' OR profiletype IS NULL THEN
        UPDATE pharmacy_users
        SET password = p_password, change_password = 0, failed_login_attempts = 0
        WHERE email = p_email AND is_deleted = 0;
    END IF;
END$$

-- ============================================================
-- 1. AUTH — external_portal_password_reset
-- ============================================================
DROP PROCEDURE IF EXISTS `external_portal_password_reset`$$
CREATE PROCEDURE `external_portal_password_reset`(
    IN p_email       VARCHAR(200),
    IN p_new_password VARCHAR(200),
    IN p_reset_type  VARCHAR(50)
)
BEGIN
    UPDATE pharmacy_users
    SET password = p_new_password, change_password = 0, failed_login_attempts = 0
    WHERE email = p_email AND is_deleted = 0;
END$$

-- ============================================================
-- 1. AUTH — update_jwt_token
-- ============================================================
DROP PROCEDURE IF EXISTS `update_jwt_token`$$
CREATE PROCEDURE `update_jwt_token`(
    IN p_id  BIGINT,
    IN p_jwt VARCHAR(500)
)
BEGIN
    UPDATE pharmacy_users
    SET sec_key = p_jwt
    WHERE id = p_id AND is_deleted = 0;
END$$

-- ============================================================
-- 2. PRODUCTS — add_product
-- ============================================================
DROP PROCEDURE IF EXISTS `add_product`$$
CREATE PROCEDURE `add_product`(
    IN  in_pharmacy_id      BIGINT,
    IN  in_category_id      BIGINT,
    IN  in_name             VARCHAR(300),
    IN  in_description      TEXT,
    IN  in_sku              VARCHAR(100),
    IN  in_barcode          VARCHAR(100),
    IN  in_cost_price       DECIMAL(15,2),
    IN  in_selling_price    DECIMAL(15,2),
    IN  in_reorder_level    INT,
    IN  in_unit_of_measure  VARCHAR(50),
    IN  in_is_active        TINYINT,
    IN  in_created_by       BIGINT,
    OUT p_id                BIGINT
)
BEGIN
    SET p_id = 0;
    IF EXISTS (SELECT 1 FROM products WHERE pharmacy_id = in_pharmacy_id AND (sku = in_sku OR barcode = in_barcode) AND is_deleted = 0
               AND (in_sku IS NOT NULL OR in_barcode IS NOT NULL)) THEN
        SET p_id = 0;
    ELSE
        INSERT INTO products
            (pharmacy_id, category_id, name, sku, barcode, description,
             cost_price, selling_price, reorder_level, unit, is_active, created_by, created_on)
        VALUES
            (in_pharmacy_id, in_category_id, in_name, in_sku, in_barcode, in_description,
             COALESCE(in_cost_price, 0), COALESCE(in_selling_price, 0),
             COALESCE(in_reorder_level, 0), COALESCE(in_unit_of_measure, 'pcs'),
             COALESCE(in_is_active, 1), in_created_by, NOW());
        SET p_id = LAST_INSERT_ID();
    END IF;
END$$

-- ============================================================
-- 2. PRODUCTS — update_product
-- ============================================================
DROP PROCEDURE IF EXISTS `update_product`$$
CREATE PROCEDURE `update_product`(
    IN  in_id                BIGINT,
    IN  in_pharmacy_id       BIGINT,
    IN  in_category_id       BIGINT,
    IN  in_name              VARCHAR(300),
    IN  in_description       TEXT,
    IN  in_sku               VARCHAR(100),
    IN  in_barcode           VARCHAR(100),
    IN  in_cost_price        DECIMAL(15,2),
    IN  in_selling_price     DECIMAL(15,2),
    IN  in_reorder_level     INT,
    IN  in_unit_of_measure   VARCHAR(50),
    IN  in_is_active         TINYINT
)
BEGIN
    UPDATE products
    SET category_id      = in_category_id,
        name             = in_name,
        sku              = in_sku,
        barcode          = in_barcode,
        description      = in_description,
        cost_price       = in_cost_price,
        selling_price    = in_selling_price,
        reorder_level    = COALESCE(in_reorder_level, 0),
        unit             = COALESCE(in_unit_of_measure, 'pcs'),
        is_active        = COALESCE(in_is_active, 1)
    WHERE id = in_id AND pharmacy_id = in_pharmacy_id AND is_deleted = 0;
END$$

-- ============================================================
-- 2. PRODUCTS — add_category
-- ============================================================
DROP PROCEDURE IF EXISTS `add_category`$$
CREATE PROCEDURE `add_category`(
    IN  in_pharmacy_id  BIGINT,
    IN  in_name         VARCHAR(200),
    IN  in_description  TEXT,
    IN  in_created_by   BIGINT,
    OUT p_id            BIGINT
)
BEGIN
    SET p_id = 0;
    IF EXISTS (SELECT 1 FROM product_categories WHERE pharmacy_id = in_pharmacy_id AND name = in_name AND is_deleted = 0) THEN
        SET p_id = 0;
    ELSE
        INSERT INTO product_categories
            (pharmacy_id, name, description, created_by, created_on)
        VALUES
            (in_pharmacy_id, in_name, in_description, in_created_by, NOW());
        SET p_id = LAST_INSERT_ID();
    END IF;
END$$

-- ============================================================
-- 3. SALES — create_sale
-- ============================================================
DROP PROCEDURE IF EXISTS `create_sale`$$
CREATE PROCEDURE `create_sale`(
    IN  in_pharmacy_id      BIGINT,
    IN  in_customer_id      BIGINT,
    IN  in_user_id          BIGINT,
    IN  in_total_amount     DECIMAL(15,2),
    IN  in_discount         DECIMAL(15,2),
    IN  in_tax              DECIMAL(15,2),
    IN  in_net_amount       DECIMAL(15,2),
    IN  in_amount_paid      DECIMAL(15,2),
    IN  in_payment_method   VARCHAR(50),
    IN  in_notes            TEXT,
    OUT p_sale_id           BIGINT
)
BEGIN
    DECLARE v_sale_number VARCHAR(50);
    SET p_sale_id = 0;
    SET v_sale_number = CONCAT('SAL-', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s'), '-', FLOOR(1000 + RAND() * 9000));
    INSERT INTO sales
        (pharmacy_id, customer_id, sale_number, sale_type, subtotal, vat_amount,
         discount, total, amount_paid, payment_method, notes, sold_by, created_on)
    VALUES
        (in_pharmacy_id, in_customer_id, v_sale_number, 'Retail',
         COALESCE(in_total_amount, 0), COALESCE(in_tax, 0),
         COALESCE(in_discount, 0), COALESCE(in_net_amount, 0),
         COALESCE(in_amount_paid, 0), COALESCE(in_payment_method, 'Cash'),
         in_notes, in_user_id, NOW());
    SET p_sale_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 3. SALES — add_sale_item
-- ============================================================
DROP PROCEDURE IF EXISTS `add_sale_item`$$
CREATE PROCEDURE `add_sale_item`(
    IN  in_sale_id    BIGINT,
    IN  in_product_id BIGINT,
    IN  in_quantity   INT,
    IN  in_unit_price DECIMAL(15,2),
    IN  in_discount   DECIMAL(15,2),
    IN  in_total      DECIMAL(15,2)
)
BEGIN
    INSERT INTO sale_items
        (sale_id, product_id, quantity, unit_price, discount, total)
    VALUES
        (in_sale_id, in_product_id, COALESCE(in_quantity, 0),
         COALESCE(in_unit_price, 0), COALESCE(in_discount, 0), COALESCE(in_total, 0));
END$$

-- ============================================================
-- 3. SALES — deduct_stock_on_sale
-- ============================================================
DROP PROCEDURE IF EXISTS `deduct_stock_on_sale`$$
CREATE PROCEDURE `deduct_stock_on_sale`(
    IN p_pharmacy_id BIGINT,
    IN p_product_id  BIGINT,
    IN p_batch_id    BIGINT,
    IN p_quantity    INT
)
BEGIN
    UPDATE products
    SET stock_qty = stock_qty - p_quantity
    WHERE id = p_product_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;

    IF p_batch_id IS NOT NULL AND p_batch_id > 0 THEN
        UPDATE product_batches
        SET quantity_sold = quantity_sold + p_quantity
        WHERE id = p_batch_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;
    END IF;
END$$

-- ============================================================
-- 4. CUSTOMERS — add_customer
-- ============================================================
DROP PROCEDURE IF EXISTS `add_customer`$$
CREATE PROCEDURE `add_customer`(
    IN  in_pharmacy_id    BIGINT,
    IN  in_first_name     VARCHAR(100),
    IN  in_last_name      VARCHAR(100),
    IN  in_email          VARCHAR(200),
    IN  in_phone          VARCHAR(50),
    IN  in_address        TEXT,
    IN  in_date_of_birth  DATE,
    IN  in_gender         VARCHAR(20),
    IN  in_customer_type  VARCHAR(20),
    IN  in_credit_limit   DECIMAL(15,2),
    IN  in_payment_terms  VARCHAR(50),
    IN  in_is_active      TINYINT,
    IN  in_created_by     BIGINT,
    OUT p_id              BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO customers
        (pharmacy_id, first_name, last_name, email, phone, address,
         date_of_birth, gender, customer_type, credit_limit, payment_terms,
         is_active, created_by, created_on)
    VALUES
        (in_pharmacy_id, in_first_name, in_last_name, in_email, in_phone, in_address,
         in_date_of_birth, in_gender, COALESCE(in_customer_type, 'Retail'),
         COALESCE(in_credit_limit, 0), COALESCE(in_payment_terms, 'Cash'),
         COALESCE(in_is_active, 1), in_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 5. SUPPLIERS — add_supplier
-- ============================================================
DROP PROCEDURE IF EXISTS `add_supplier`$$
CREATE PROCEDURE `add_supplier`(
    IN  in_pharmacy_id    BIGINT,
    IN  in_name           VARCHAR(200),
    IN  in_contact_person VARCHAR(200),
    IN  in_email          VARCHAR(200),
    IN  in_phone          VARCHAR(50),
    IN  in_address        TEXT,
    IN  in_city           VARCHAR(100),
    IN  in_country        VARCHAR(100),
    IN  in_created_by     BIGINT,
    OUT p_id              BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO suppliers
        (pharmacy_id, name, contact_person, email, phone, address, city, country,
         created_by, created_on)
    VALUES
        (in_pharmacy_id, in_name, in_contact_person, in_email, in_phone, in_address,
         in_city, in_country, in_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 4b. SUPPLIERS — update_supplier
-- ============================================================
DROP PROCEDURE IF EXISTS `update_supplier`$$
CREATE PROCEDURE `update_supplier`(
    IN  in_id             BIGINT,
    IN  in_pharmacy_id    BIGINT,
    IN  in_name           VARCHAR(200),
    IN  in_contact_person VARCHAR(200),
    IN  in_email          VARCHAR(200),
    IN  in_phone          VARCHAR(50),
    IN  in_address        TEXT,
    IN  in_city           VARCHAR(100),
    IN  in_country        VARCHAR(100),
    IN  in_is_active      TINYINT
)
BEGIN
    UPDATE suppliers
    SET name             = in_name,
        contact_person   = in_contact_person,
        email            = in_email,
        phone            = in_phone,
        address          = in_address,
        city             = in_city,
        country          = in_country,
        is_active        = COALESCE(in_is_active, 1)
    WHERE id = in_id AND pharmacy_id = in_pharmacy_id AND is_deleted = 0;
END$$

-- ============================================================
-- 5. SUPPLIERS — add_purchase_order
-- ============================================================
DROP PROCEDURE IF EXISTS `add_purchase_order`$$
CREATE PROCEDURE `add_purchase_order`(
    IN  in_pharmacy_id   BIGINT,
    IN  in_supplier_id   BIGINT,
    IN  in_product_id    BIGINT,
    IN  in_quantity      INT,
    IN  in_unit_cost     DECIMAL(15,2),
    IN  in_total_cost    DECIMAL(15,2),
    IN  in_expected_date DATE,
    IN  in_notes         TEXT,
    IN  in_created_by    BIGINT,
    OUT p_id             BIGINT
)
BEGIN
    DECLARE v_po_number VARCHAR(50);
    SET p_id = 0;
    SET v_po_number = CONCAT('PO-', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s'), '-', FLOOR(1000 + RAND() * 9000));
    INSERT INTO purchase_orders
        (pharmacy_id, supplier_id, po_number, product_id, quantity, unit_cost,
         total_cost, total, expected_date, notes, created_by, created_on)
    VALUES
        (in_pharmacy_id, in_supplier_id, v_po_number, in_product_id,
         COALESCE(in_quantity, 0), COALESCE(in_unit_cost, 0),
         COALESCE(in_total_cost, 0), COALESCE(in_total_cost, 0),
         in_expected_date, in_notes, in_created_by, NOW());
    SET p_id = LAST_INSERT_ID();

    INSERT INTO po_items
        (po_id, product_id, quantity, received_qty, unit_cost, total)
    VALUES
        (p_id, in_product_id, COALESCE(in_quantity, 0), 0,
         COALESCE(in_unit_cost, 0), COALESCE(in_total_cost, 0));
END$$

-- ============================================================
-- 5. SUPPLIERS — add_po_item
-- ============================================================
DROP PROCEDURE IF EXISTS `add_po_item`$$
CREATE PROCEDURE `add_po_item`(
    IN  p_po_id      BIGINT,
    IN  p_product_id BIGINT,
    IN  p_quantity   INT,
    IN  p_unit_cost  DECIMAL(15,2),
    IN  p_total      DECIMAL(15,2),
    OUT p_id         BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO po_items
        (po_id, product_id, quantity, unit_cost, total)
    VALUES
        (p_po_id, p_product_id, p_quantity, COALESCE(p_unit_cost, 0), COALESCE(p_total, 0));
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 5. SUPPLIERS — receive_stock
-- ============================================================
DROP PROCEDURE IF EXISTS `receive_stock`$$
CREATE PROCEDURE `receive_stock`(
    IN  in_po_id             BIGINT,
    IN  in_received_by       BIGINT,
    IN  in_quantity_received INT,
    IN  in_notes             TEXT
)
BEGIN
    UPDATE po_items
    SET received_qty = received_qty + COALESCE(in_quantity_received, 0)
    WHERE po_id = in_po_id AND received_qty < quantity;

    UPDATE purchase_orders po
    SET po.status = CASE WHEN (SELECT COUNT(*) FROM po_items WHERE po_id = in_po_id AND received_qty < quantity) = 0
                         THEN 'Received' ELSE 'Partial' END,
        po.received_date = CASE WHEN (SELECT COUNT(*) FROM po_items WHERE po_id = in_po_id AND received_qty < quantity) = 0
                                THEN CURDATE() ELSE po.received_date END
    WHERE po.id = in_po_id;

    UPDATE products p
    JOIN po_items pi ON pi.product_id = p.id AND pi.po_id = in_po_id
    SET p.stock_qty = p.stock_qty + COALESCE(in_quantity_received, 0)
    WHERE p.is_deleted = 0;
END$$

-- ============================================================
-- 6. FINANCE — add_expense
-- ============================================================
DROP PROCEDURE IF EXISTS `add_expense`$$
CREATE PROCEDURE `add_expense`(
    IN  in_pharmacy_id    BIGINT,
    IN  in_category       VARCHAR(200),
    IN  in_description    VARCHAR(500),
    IN  in_amount         DECIMAL(15,2),
    IN  in_expense_date   DATE,
    IN  in_payment_method VARCHAR(50),
    IN  in_notes          TEXT,
    IN  in_created_by     BIGINT,
    OUT p_id              BIGINT
)
BEGIN
    DECLARE v_category_id BIGINT DEFAULT 0;
    SET p_id = 0;
    IF in_category IS NOT NULL AND in_category <> '' THEN
        SELECT id INTO v_category_id
        FROM expense_categories
        WHERE pharmacy_id = in_pharmacy_id AND name = in_category AND is_deleted = 0
        LIMIT 1;
    END IF;
    INSERT INTO expenses
        (pharmacy_id, category_id, description, amount, expense_date,
         payment_method, notes, created_by, created_on)
    VALUES
        (in_pharmacy_id, IFNULL(v_category_id, 0), in_description, COALESCE(in_amount, 0),
         COALESCE(in_expense_date, CURDATE()), COALESCE(in_payment_method, 'Cash'),
         in_notes, in_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 6. FINANCE — add_expense_category
-- ============================================================
DROP PROCEDURE IF EXISTS `add_expense_category`$$
CREATE PROCEDURE `add_expense_category`(
    IN  p_pharmacy_id  BIGINT,
    IN  p_name         VARCHAR(200),
    IN  p_created_by   BIGINT,
    OUT p_id           BIGINT,
    OUT p_error_code   VARCHAR(2),
    OUT p_error_desc   VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF EXISTS (SELECT 1 FROM expense_categories WHERE pharmacy_id = p_pharmacy_id AND name = p_name AND is_deleted = 0) THEN
        SET p_error_desc = 'Expense category already exists';
    ELSE
        INSERT INTO expense_categories
            (pharmacy_id, name, created_on)
        VALUES
            (p_pharmacy_id, p_name, NOW());
        SET p_id = LAST_INSERT_ID();
        SET p_error_code = '00'; SET p_error_desc = 'Expense category added';
    END IF;
END$$

-- ============================================================
-- 7. CLINICAL — add_patient
-- ============================================================
DROP PROCEDURE IF EXISTS `add_patient`$$
CREATE PROCEDURE `add_patient`(
    IN  in_pharmacy_id      BIGINT,
    IN  in_first_name       VARCHAR(100),
    IN  in_last_name        VARCHAR(100),
    IN  in_date_of_birth    DATE,
    IN  in_gender           VARCHAR(20),
    IN  in_phone            VARCHAR(50),
    IN  in_email            VARCHAR(200),
    IN  in_address          TEXT,
    IN  in_allergies        TEXT,
    IN  in_medical_history  TEXT,
    IN  in_created_by       BIGINT,
    OUT p_id                BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO patients
        (pharmacy_id, first_name, last_name, date_of_birth, gender, phone, email,
         address, allergies, medical_history, created_by, created_on)
    VALUES
        (in_pharmacy_id, in_first_name, in_last_name, in_date_of_birth, in_gender,
         in_phone, in_email, in_address, in_allergies, in_medical_history,
         in_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 7. CLINICAL — add_prescription
-- ============================================================
DROP PROCEDURE IF EXISTS `add_prescription`$$
CREATE PROCEDURE `add_prescription`(
    IN  in_pharmacy_id         BIGINT,
    IN  in_patient_id          BIGINT,
    IN  in_doctor_name         VARCHAR(200),
    IN  in_hospital            VARCHAR(200),
    IN  in_prescription_date   DATE,
    IN  in_notes               TEXT,
    IN  in_created_by          BIGINT,
    OUT p_id                   BIGINT
)
BEGIN
    DECLARE v_rx_number VARCHAR(50);
    SET p_id = 0;
    SET v_rx_number = CONCAT('RX-', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s'), '-', FLOOR(1000 + RAND() * 9000));
    INSERT INTO prescriptions
        (pharmacy_id, patient_id, prescription_number, doctor_name, hospital,
         prescription_date, notes, created_by, created_on)
    VALUES
        (in_pharmacy_id, in_patient_id, v_rx_number, in_doctor_name, in_hospital,
         COALESCE(in_prescription_date, CURDATE()), in_notes, in_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 7. CLINICAL — add_prescription_item
-- ============================================================
DROP PROCEDURE IF EXISTS `add_prescription_item`$$
CREATE PROCEDURE `add_prescription_item`(
    IN  p_prescription_id  BIGINT,
    IN  p_product_id       BIGINT,
    IN  p_medication_name  VARCHAR(300),
    IN  p_dosage           VARCHAR(100),
    IN  p_frequency        VARCHAR(100),
    IN  p_duration         VARCHAR(100),
    IN  p_quantity         INT,
    IN  p_notes            TEXT,
    OUT p_id               BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO prescription_items
        (prescription_id, product_id, medication_name, dosage, frequency,
         duration, quantity, notes)
    VALUES
        (p_prescription_id, p_product_id, p_medication_name, p_dosage, p_frequency,
         p_duration, COALESCE(p_quantity, 0), p_notes);
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 8. DDA — add_dda_entry
-- ============================================================
DROP PROCEDURE IF EXISTS `add_dda_entry`$$
CREATE PROCEDURE `add_dda_entry`(
    IN  in_pharmacy_id      BIGINT,
    IN  in_patient_id       BIGINT,
    IN  in_prescription_id  BIGINT,
    IN  in_product_id       BIGINT,
    IN  in_quantity         INT,
    IN  in_dispensed_date   DATE,
    IN  in_notes            TEXT,
    IN  in_created_by       BIGINT,
    OUT p_id                BIGINT
)
BEGIN
    DECLARE v_balance INT DEFAULT 0;
    SET p_id = 0;
    SELECT stock_qty INTO v_balance
    FROM products
    WHERE id = in_product_id AND pharmacy_id = in_pharmacy_id AND is_deleted = 0;
    INSERT INTO dda_register
        (pharmacy_id, patient_id, prescription_id, product_id, entry_type, quantity,
         dispensed_date, notes, balance_after, recorded_by, created_on)
    VALUES
        (in_pharmacy_id, in_patient_id, in_prescription_id, in_product_id, 'Dispense',
         COALESCE(in_quantity, 0), in_dispensed_date, in_notes,
         COALESCE(v_balance, 0) - COALESCE(in_quantity, 0), in_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 9. STOCK — add_stock_adjustment
-- ============================================================
DROP PROCEDURE IF EXISTS `add_stock_adjustment`$$
CREATE PROCEDURE `add_stock_adjustment`(
    IN  p_pharmacy_id      BIGINT,
    IN  p_product_id       BIGINT,
    IN  p_batch_id         BIGINT,
    IN  p_adjustment_type  VARCHAR(50),
    IN  p_quantity         INT,
    IN  p_reason           TEXT,
    IN  p_adjusted_by      BIGINT,
    OUT p_id               BIGINT,
    OUT p_error_code       VARCHAR(2),
    OUT p_error_desc       VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    INSERT INTO stock_adjustments
        (pharmacy_id, product_id, batch_id, adjustment_type, quantity, reason,
         adjusted_by, created_on)
    VALUES
        (p_pharmacy_id, p_product_id, p_batch_id, p_adjustment_type, p_quantity,
         p_reason, p_adjusted_by, NOW());
    SET p_id = LAST_INSERT_ID();

    -- Apply adjustment to stock
    IF p_adjustment_type = 'Addition' THEN
        UPDATE products SET stock_qty = stock_qty + p_quantity
        WHERE id = p_product_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;
    ELSEIF p_adjustment_type = 'Subtraction' THEN
        UPDATE products SET stock_qty = stock_qty - p_quantity
        WHERE id = p_product_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;
    END IF;

    IF p_batch_id IS NOT NULL AND p_batch_id > 0 THEN
        IF p_adjustment_type = 'Addition' THEN
            UPDATE product_batches SET quantity = quantity + p_quantity
            WHERE id = p_batch_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;
        ELSEIF p_adjustment_type = 'Subtraction' THEN
            UPDATE product_batches SET quantity = quantity - p_quantity
            WHERE id = p_batch_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;
        END IF;
    END IF;

    SET p_error_code = '00'; SET p_error_desc = 'Stock adjustment recorded';
END$$

-- ============================================================
-- 9. STOCK — add_stock_take_session
-- ============================================================
DROP PROCEDURE IF EXISTS `add_stock_take_session`$$
CREATE PROCEDURE `add_stock_take_session`(
    IN  p_pharmacy_id  BIGINT,
    IN  p_session_name VARCHAR(200),
    IN  p_started_by   BIGINT,
    OUT p_id           BIGINT,
    OUT p_error_code   VARCHAR(2),
    OUT p_error_desc   VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    INSERT INTO stock_take_sessions
        (pharmacy_id, session_name, status, started_by, started_on)
    VALUES
        (p_pharmacy_id, p_session_name, 'Open', p_started_by, NOW());
    SET p_id = LAST_INSERT_ID();
    SET p_error_code = '00'; SET p_error_desc = 'Stock take session started';
END$$

-- ============================================================
-- 9. STOCK — add_stock_take_item
-- ============================================================
DROP PROCEDURE IF EXISTS `add_stock_take_item`$$
CREATE PROCEDURE `add_stock_take_item`(
    IN  p_session_id  BIGINT,
    IN  p_product_id  BIGINT,
    IN  p_batch_id    BIGINT,
    IN  p_system_qty  INT,
    IN  p_counted_qty INT,
    IN  p_notes       TEXT,
    OUT p_id          BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO stock_take_items
        (session_id, product_id, batch_id, system_qty, counted_qty,
         variance, notes)
    VALUES
        (p_session_id, p_product_id, p_batch_id,
         COALESCE(p_system_qty, 0), COALESCE(p_counted_qty, 0),
         COALESCE(p_counted_qty, 0) - COALESCE(p_system_qty, 0), p_notes);
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 9. STOCK — commit_stock_take
-- ============================================================
DROP PROCEDURE IF EXISTS `commit_stock_take`$$
CREATE PROCEDURE `commit_stock_take`(
    IN  p_session_id   BIGINT,
    IN  p_pharmacy_id  BIGINT,
    IN  p_committed_by BIGINT,
    OUT p_error_code   VARCHAR(2),
    OUT p_error_desc   VARCHAR(500)
)
BEGIN
    SET p_error_code = '01'; SET p_error_desc = 'Failed';

    IF NOT EXISTS (SELECT 1 FROM stock_take_sessions WHERE id = p_session_id AND status = 'Open' AND is_deleted = 0) THEN
        SET p_error_desc = 'Session not found or not open';
    ELSE
        -- Update product stock_qty based on counted_qty for each item
        UPDATE products p
        JOIN stock_take_items sti ON sti.product_id = p.id AND sti.session_id = p_session_id
        SET p.stock_qty = sti.counted_qty
        WHERE p.pharmacy_id = p_pharmacy_id AND p.is_deleted = 0;

        -- Update batch quantities if batch_id is present
        UPDATE product_batches pb
        JOIN stock_take_items sti ON sti.batch_id = pb.id AND sti.session_id = p_session_id
        SET pb.quantity = sti.counted_qty
        WHERE pb.pharmacy_id = p_pharmacy_id AND pb.is_deleted = 0;

        -- Close the session
        UPDATE stock_take_sessions
        SET status = 'Committed', committed_by = p_committed_by, committed_on = NOW()
        WHERE id = p_session_id;

        SET p_error_code = '00'; SET p_error_desc = 'Stock take committed';
    END IF;
END$$

-- ============================================================
-- 10. DASHBOARD — get_dashboard_summary
-- ============================================================
DROP PROCEDURE IF EXISTS `get_dashboard_summary`$$
CREATE PROCEDURE `get_dashboard_summary`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT
        (SELECT COUNT(*) FROM products WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0) AS total_products,
        (SELECT COUNT(*) FROM customers WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0) AS total_customers,
        (SELECT COUNT(*) FROM suppliers WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0) AS total_suppliers,
        (SELECT COALESCE(SUM(total), 0) FROM sales
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND DATE(created_on) = CURDATE()) AS today_sales,
        (SELECT COALESCE(SUM(total), 0) FROM sales
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND YEAR(created_on) = YEAR(CURDATE()) AND MONTH(created_on) = MONTH(CURDATE())) AS month_sales,
        (SELECT COALESCE(SUM(amount), 0) FROM expenses
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND YEAR(created_on) = YEAR(CURDATE()) AND MONTH(created_on) = MONTH(CURDATE())) AS month_expenses,
        (SELECT COUNT(*) FROM products
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND stock_qty <= reorder_level) AS low_stock_count,
        (SELECT COUNT(*) FROM product_batches
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND expiry_date BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 90 DAY)
           AND quantity > 0) AS expiring_soon_count,
        (SELECT COALESCE(SUM(stock_qty * cost_price), 0) FROM products
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0) AS total_inventory_value;
END$$

-- ============================================================
-- 11. SETTINGS — save_pharmacy_setting
-- ============================================================
DROP PROCEDURE IF EXISTS `save_pharmacy_setting`$$
CREATE PROCEDURE `save_pharmacy_setting`(
    IN p_pharmacy_id   BIGINT,
    IN p_setting_key   VARCHAR(100),
    IN p_setting_value TEXT
)
BEGIN
    INSERT INTO pharmacy_settings (pharmacy_id, setting_key, setting_value)
    VALUES (p_pharmacy_id, p_setting_key, p_setting_value)
    ON DUPLICATE KEY UPDATE setting_value = p_setting_value;
END$$

-- ============================================================
-- 11. SETTINGS — add_audit_trail
-- ============================================================
DROP PROCEDURE IF EXISTS `add_audit_trail`$$
CREATE PROCEDURE `add_audit_trail`(
    IN  p_user_name          VARCHAR(200),
    IN  p_action_type        VARCHAR(50),
    IN  p_action_description TEXT,
    IN  p_page_accessed      VARCHAR(500),
    IN  p_client_ip_address  VARCHAR(100),
    IN  p_session_id         VARCHAR(200),
    IN  p_created_on         DATETIME,
    OUT p_id                 BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO audit_trail
        (user_name, action_type, action_description, page_accessed,
         client_ip_address, session_id, created_on)
    VALUES
        (p_user_name, p_action_type, p_action_description, p_page_accessed,
         p_client_ip_address, p_session_id, COALESCE(p_created_on, NOW()));
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 12. NOTIFICATIONS — add_notification
-- ============================================================
DROP PROCEDURE IF EXISTS `add_notification`$$
CREATE PROCEDURE `add_notification`(
    IN  p_pharmacy_id        BIGINT,
    IN  p_user_id            BIGINT,
    IN  p_title              VARCHAR(200),
    IN  p_message            TEXT,
    IN  p_notification_type  VARCHAR(50),
    OUT p_id                 BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO notifications
        (pharmacy_id, user_id, title, message, notification_type, created_on)
    VALUES
        (p_pharmacy_id, p_user_id, p_title, p_message,
         COALESCE(p_notification_type, 'Info'), NOW());
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 12. NOTIFICATIONS — mark_notification_read
-- ============================================================
DROP PROCEDURE IF EXISTS `mark_notification_read`$$
CREATE PROCEDURE `mark_notification_read`(
    IN p_id BIGINT
)
BEGIN
    UPDATE notifications
    SET is_read = 1
    WHERE id = p_id AND is_deleted = 0;
END$$

-- ============================================================
-- 13. ADMIN — get_pharmacy_by_slug
-- ============================================================
DROP PROCEDURE IF EXISTS `get_pharmacy_by_slug`$$
CREATE PROCEDURE `get_pharmacy_by_slug`(
    IN p_slug VARCHAR(100)
)
BEGIN
    SELECT id, name, slug, phone, email, address, license_number,
           currency, subscription_plan, is_active
    FROM pharmacies
    WHERE slug = p_slug AND is_deleted = 0
    LIMIT 1;
END$$

-- ============================================================
-- 13. ADMIN — get_users_by_pharmacy
-- ============================================================
DROP PROCEDURE IF EXISTS `get_users_by_pharmacy`$$
CREATE PROCEDURE `get_users_by_pharmacy`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT id, pharmacy_id, role_id, first_name, last_name, email,
           mobile, avatar, locked, IF(locked=1,0,1) AS is_active, created_on
    FROM portal_users
    WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
    ORDER BY first_name;
END$$

-- ============================================================
-- 13. ADMIN — get_user_by_id
-- ============================================================
DROP PROCEDURE IF EXISTS `get_user_by_id`$$
CREATE PROCEDURE `get_user_by_id`(
    IN p_id BIGINT
)
BEGIN
    SELECT id, pharmacy_id, role_id, first_name, last_name, email,
           mobile, avatar, locked, IF(locked=1,0,1) AS is_active, created_on
    FROM portal_users
    WHERE id = p_id AND is_deleted = 0
    LIMIT 1;
END$$

-- ============================================================
-- 13. ADMIN — update_user
-- ============================================================
DROP PROCEDURE IF EXISTS `update_user`$$
CREATE PROCEDURE `update_user`(
    IN  p_id         BIGINT,
    IN  p_first_name VARCHAR(100),
    IN  p_last_name  VARCHAR(100),
    IN  p_email      VARCHAR(200),
    IN  p_mobile     VARCHAR(50),
    IN  p_role_id    INT,
    IN  p_is_active  TINYINT
)
BEGIN
    IF EXISTS (SELECT 1 FROM portal_users WHERE id = p_id AND is_deleted = 0) THEN
        UPDATE portal_users
        SET first_name = COALESCE(p_first_name, first_name),
            last_name  = COALESCE(p_last_name, last_name),
            email      = COALESCE(p_email, email),
            mobile     = COALESCE(p_mobile, mobile),
            role_id    = COALESCE(p_role_id, role_id),
            locked     = IF(p_is_active = 1, 0, 1)
        WHERE id = p_id AND is_deleted = 0;
    ELSE
        UPDATE p_external_portal_user
        SET first_name = COALESCE(p_first_name, first_name),
            last_name  = COALESCE(p_last_name, last_name),
            email      = COALESCE(p_email, email),
            mobile     = COALESCE(p_mobile, mobile),
            role_id    = COALESCE(p_role_id, role_id),
            locked     = IF(p_is_active = 1, 0, 1)
        WHERE id = p_id AND is_deleted = 0;
    END IF;
END$$

-- ============================================================
-- 13. ADMIN — admin_reset_password
-- ============================================================
DROP PROCEDURE IF EXISTS `admin_reset_password`$$
CREATE PROCEDURE `admin_reset_password`(
    IN  p_user_id      BIGINT,
    IN  p_new_password VARCHAR(200)
)
BEGIN
    IF EXISTS (SELECT 1 FROM portal_users WHERE id = p_user_id AND is_deleted = 0) THEN
        UPDATE portal_users
        SET PASSWORD = p_new_password, locked = 0
        WHERE id = p_user_id AND is_deleted = 0;
    ELSE
        UPDATE p_external_portal_user
        SET PASSWORD = p_new_password, change_password = 1, failed_login_attempts = 0
        WHERE id = p_user_id AND is_deleted = 0;
    END IF;
END$$

-- ============================================================
-- 14. DASHBOARD — get_stock_summary (top products by qty)
-- ============================================================
DROP PROCEDURE IF EXISTS `get_stock_summary`$$
CREATE PROCEDURE `get_stock_summary`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT id, name, sku, stock_qty, reorder_level, cost_price, selling_price
    FROM products
    WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
    ORDER BY stock_qty DESC
    LIMIT 20;
END$$

-- ============================================================
-- 14. DASHBOARD — get_sales_stats (today + recent totals)
-- ============================================================
DROP PROCEDURE IF EXISTS `get_sales_stats`$$
CREATE PROCEDURE `get_sales_stats`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT
        (SELECT COALESCE(SUM(total), 0) FROM sales
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND DATE(created_on) = CURDATE()) AS today_total,
        (SELECT COUNT(*) FROM sales
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND DATE(created_on) = CURDATE()) AS today_count,
        (SELECT COALESCE(SUM(total), 0) FROM sales
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND created_on >= DATE_SUB(CURDATE(), INTERVAL 7 DAY)) AS week_total,
        (SELECT COALESCE(SUM(total), 0) FROM sales
         WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
           AND YEAR(created_on) = YEAR(CURDATE()) AND MONTH(created_on) = MONTH(CURDATE())) AS month_total;
END$$

-- ============================================================
-- 14. DASHBOARD — get_expiring_items (batches expiring within 90 days)
-- ============================================================
DROP PROCEDURE IF EXISTS `get_expiring_items`$$
CREATE PROCEDURE `get_expiring_items`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT b.id, b.batch_number, b.expiry_date, b.quantity, b.cost_price,
           p.name AS product_name, p.sku
    FROM product_batches b
    JOIN products p ON p.id = b.product_id
    WHERE b.pharmacy_id = p_pharmacy_id AND b.is_deleted = 0
      AND b.expiry_date BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 90 DAY)
      AND b.quantity > 0
    ORDER BY b.expiry_date
    LIMIT 20;
END$$

-- ============================================================
-- 14. DASHBOARD — get_alerts (low stock + expiring + locked users)
-- ============================================================
DROP PROCEDURE IF EXISTS `get_alerts`$$
CREATE PROCEDURE `get_alerts`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT 'Low Stock' AS alert_type, name AS title,
           CONCAT('Stock: ', stock_qty, ' / Reorder: ', reorder_level) AS description,
           'warning' AS severity
    FROM products
    WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
      AND stock_qty <= reorder_level AND stock_qty > 0
    UNION ALL
    SELECT 'Out of Stock' AS alert_type, name AS title,
           CONCAT('Stock: ', stock_qty) AS description,
           'danger' AS severity
    FROM products
    WHERE pharmacy_id = p_pharmacy_id AND is_deleted = 0
      AND stock_qty = 0
    UNION ALL
    SELECT 'Expiring Soon' AS alert_type, p.name AS title,
           CONCAT(b.batch_number, ' expires ', DATE_FORMAT(b.expiry_date, '%d %b %Y')) AS description,
           'warning' AS severity
    FROM product_batches b
    JOIN products p ON p.id = b.product_id
    WHERE b.pharmacy_id = p_pharmacy_id AND b.is_deleted = 0
      AND b.expiry_date BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 30 DAY)
      AND b.quantity > 0
    ORDER BY severity DESC, title
    LIMIT 20;
END$$

-- ============================================================
-- 14. DASHBOARD — get_my_sales (today's sales for current user)
-- ============================================================
DROP PROCEDURE IF EXISTS `get_my_sales`$$
CREATE PROCEDURE `get_my_sales`(
    IN p_pharmacy_id BIGINT,
    IN p_user_id     BIGINT
)
BEGIN
    SELECT s.id, s.sale_number, s.total, s.payment_method, s.status, s.created_on
    FROM sales s
    WHERE s.pharmacy_id = p_pharmacy_id AND s.is_deleted = 0
      AND s.sold_by = p_user_id
      AND DATE(s.created_on) = CURDATE()
    ORDER BY s.created_on DESC;
END$$

-- ============================================================
-- 14. DASHBOARD — get_pending_orders (pending purchase orders)
-- ============================================================
DROP PROCEDURE IF EXISTS `get_pending_orders`$$
CREATE PROCEDURE `get_pending_orders`(
    IN p_pharmacy_id BIGINT
)
BEGIN
    SELECT po.id, po.po_number, po.total, po.expected_date, po.status, po.created_on,
           sp.name AS supplier_name
    FROM purchase_orders po
    JOIN suppliers sp ON sp.id = po.supplier_id
    WHERE po.pharmacy_id = p_pharmacy_id AND po.is_deleted = 0
      AND po.status = 'Pending'
    ORDER BY po.expected_date
    LIMIT 20;
END$$

DELIMITER ;
