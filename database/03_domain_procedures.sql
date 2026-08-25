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
    IN  p_name           VARCHAR(200),
    IN  p_slug           VARCHAR(100),
    IN  p_phone          VARCHAR(50),
    IN  p_email          VARCHAR(200),
    IN  p_address        TEXT,
    IN  p_license_number VARCHAR(100),
    IN  p_currency       VARCHAR(10),
    IN  p_vat_number     VARCHAR(100),
    OUT p_id             BIGINT,
    OUT p_error_code     VARCHAR(2),
    OUT p_error_desc     VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF EXISTS (SELECT 1 FROM pharmacies WHERE slug = p_slug AND is_deleted = 0) THEN
        SET p_error_desc = 'Pharmacy slug already exists';
    ELSE
        INSERT INTO pharmacies
            (name, slug, phone, email, address, license_number, currency, vat_number, created_on)
        VALUES
            (p_name, p_slug, p_phone, p_email, p_address, p_license_number,
             COALESCE(p_currency, 'KES'), p_vat_number, NOW());
        SET p_id = LAST_INSERT_ID();
        SET p_error_code = '00'; SET p_error_desc = 'Pharmacy added';
    END IF;
END$$

-- ============================================================
-- 1. AUTH — add_user
-- ============================================================
DROP PROCEDURE IF EXISTS `add_user`$$
CREATE PROCEDURE `add_user`(
    IN  p_pharmacy_id  BIGINT,
    IN  p_role_id      INT,
    IN  p_first_name   VARCHAR(100),
    IN  p_middle_name  VARCHAR(100),
    IN  p_last_name    VARCHAR(100),
    IN  p_email        VARCHAR(200),
    IN  p_mobile       VARCHAR(50),
    IN  p_password     VARCHAR(200),
    IN  p_created_by   BIGINT,
    OUT p_id           BIGINT,
    OUT p_error_code   VARCHAR(2),
    OUT p_error_desc   VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF EXISTS (SELECT 1 FROM pharmacy_users WHERE email = p_email AND is_deleted = 0) THEN
        SET p_error_desc = 'Email already registered';
    ELSE
        INSERT INTO pharmacy_users
            (pharmacy_id, role_id, first_name, middle_name, last_name,
             email, mobile, password, created_by, created_on)
        VALUES
            (p_pharmacy_id, p_role_id, p_first_name, p_middle_name, p_last_name,
             p_email, p_mobile, p_password, p_created_by, NOW());
        SET p_id = LAST_INSERT_ID();
        SET p_error_code = '00'; SET p_error_desc = 'User added';
    END IF;
END$$

-- ============================================================
-- 1. AUTH — validate_login
-- ============================================================
DROP PROCEDURE IF EXISTS `validate_login`$$
CREATE PROCEDURE `validate_login`(
    IN p_email    VARCHAR(200),
    IN p_password VARCHAR(200)
)
BEGIN
    SELECT id, pharmacy_id, role_id, first_name, middle_name, last_name,
           email, mobile, avatar, locked, change_password, failed_login_attempts,
           google_authenticate, sec_key
    FROM pharmacy_users
    WHERE email = p_email AND password = p_password AND is_deleted = 0
    LIMIT 1;
END$$

-- ============================================================
-- 1. AUTH — add_refresh_token
-- ============================================================
DROP PROCEDURE IF EXISTS `add_refresh_token`$$
CREATE PROCEDURE `add_refresh_token`(
    IN p_user_id     BIGINT,
    IN p_token       VARCHAR(500),
    IN p_expires_at  DATETIME
)
BEGIN
    INSERT INTO refresh_tokens (user_id, token, expires_at, created_on)
    VALUES (p_user_id, p_token, p_expires_at, NOW());
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
    IN  p_user_id    BIGINT,
    IN  p_user_type  VARCHAR(20),
    IN  p_email      VARCHAR(200),
    IN  p_mobile     VARCHAR(50),
    IN  p_otp_code   VARCHAR(10),
    IN  p_purpose    VARCHAR(50),
    IN  p_otp_ref    VARCHAR(100),
    OUT p_id         BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO otp_records
        (user_id, user_type, email, mobile, otp_code, purpose, otp_ref, verified, expires_at, created_on)
    VALUES
        (p_user_id, p_user_type, p_email, p_mobile, p_otp_code, p_purpose, p_otp_ref, 0,
         DATE_ADD(NOW(), INTERVAL 15 MINUTE), NOW());
    SET p_id = LAST_INSERT_ID();
END$$

-- ============================================================
-- 1. AUTH — riziki_verify_otp
-- ============================================================
DROP PROCEDURE IF EXISTS `riziki_verify_otp`$$
CREATE PROCEDURE `riziki_verify_otp`(
    IN p_email    VARCHAR(200),
    IN p_otp_code VARCHAR(10),
    IN p_otp_ref  VARCHAR(100),
    IN p_purpose  VARCHAR(50)
)
BEGIN
    SELECT '1' AS valid, user_id, user_type, email
    FROM otp_records
    WHERE email = p_email
      AND otp_code = p_otp_code
      AND otp_ref = p_otp_ref
      AND purpose = p_purpose
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
    IN p_profiletype VARCHAR(50)
)
BEGIN
    IF p_profiletype = 'pharmacy' OR p_profiletype IS NULL THEN
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
    IN  p_pharmacy_id       BIGINT,
    IN  p_category_id       BIGINT,
    IN  p_name              VARCHAR(300),
    IN  p_sku               VARCHAR(100),
    IN  p_barcode           VARCHAR(100),
    IN  p_description       TEXT,
    IN  p_cost_price        DECIMAL(15,2),
    IN  p_selling_price     DECIMAL(15,2),
    IN  p_vat_rate          DECIMAL(5,2),
    IN  p_reorder_level     INT,
    IN  p_stock_qty         INT,
    IN  p_unit              VARCHAR(50),
    IN  p_is_controlled_drug TINYINT,
    IN  p_created_by        BIGINT,
    OUT p_id                BIGINT,
    OUT p_error_code        VARCHAR(2),
    OUT p_error_desc        VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF EXISTS (SELECT 1 FROM products WHERE pharmacy_id = p_pharmacy_id AND (sku = p_sku OR barcode = p_barcode) AND is_deleted = 0
               AND (p_sku IS NOT NULL OR p_barcode IS NOT NULL)) THEN
        SET p_error_desc = 'Product with same SKU or barcode already exists';
    ELSE
        INSERT INTO products
            (pharmacy_id, category_id, name, sku, barcode, description,
             cost_price, selling_price, vat_rate, reorder_level, stock_qty,
             unit, is_controlled_drug, created_by, created_on)
        VALUES
            (p_pharmacy_id, p_category_id, p_name, p_sku, p_barcode, p_description,
             p_cost_price, p_selling_price, COALESCE(p_vat_rate, 16.00),
             COALESCE(p_reorder_level, 0), COALESCE(p_stock_qty, 0),
             COALESCE(p_unit, 'pcs'), COALESCE(p_is_controlled_drug, 0),
             p_created_by, NOW());
        SET p_id = LAST_INSERT_ID();
        SET p_error_code = '00'; SET p_error_desc = 'Product added';
    END IF;
END$$

-- ============================================================
-- 2. PRODUCTS — update_product
-- ============================================================
DROP PROCEDURE IF EXISTS `update_product`$$
CREATE PROCEDURE `update_product`(
    IN  p_id                BIGINT,
    IN  p_category_id       BIGINT,
    IN  p_name              VARCHAR(300),
    IN  p_sku               VARCHAR(100),
    IN  p_barcode           VARCHAR(100),
    IN  p_description       TEXT,
    IN  p_cost_price        DECIMAL(15,2),
    IN  p_selling_price     DECIMAL(15,2),
    IN  p_vat_rate          DECIMAL(5,2),
    IN  p_reorder_level     INT,
    IN  p_unit              VARCHAR(50),
    IN  p_is_controlled_drug TINYINT,
    OUT p_error_code        VARCHAR(2),
    OUT p_error_desc        VARCHAR(500)
)
BEGIN
    SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF NOT EXISTS (SELECT 1 FROM products WHERE id = p_id AND is_deleted = 0) THEN
        SET p_error_desc = 'Product not found';
    ELSE
        UPDATE products
        SET category_id       = p_category_id,
            name              = p_name,
            sku               = p_sku,
            barcode           = p_barcode,
            description       = p_description,
            cost_price        = p_cost_price,
            selling_price     = p_selling_price,
            vat_rate          = COALESCE(p_vat_rate, 16.00),
            reorder_level     = COALESCE(p_reorder_level, 0),
            unit              = COALESCE(p_unit, 'pcs'),
            is_controlled_drug = COALESCE(p_is_controlled_drug, 0)
        WHERE id = p_id AND is_deleted = 0;
        SET p_error_code = '00'; SET p_error_desc = 'Product updated';
    END IF;
END$$

-- ============================================================
-- 2. PRODUCTS — add_category
-- ============================================================
DROP PROCEDURE IF EXISTS `add_category`$$
CREATE PROCEDURE `add_category`(
    IN  p_pharmacy_id  BIGINT,
    IN  p_name         VARCHAR(200),
    IN  p_description  TEXT,
    IN  p_created_by   BIGINT,
    OUT p_id           BIGINT,
    OUT p_error_code   VARCHAR(2),
    OUT p_error_desc   VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF EXISTS (SELECT 1 FROM product_categories WHERE pharmacy_id = p_pharmacy_id AND name = p_name AND is_deleted = 0) THEN
        SET p_error_desc = 'Category already exists';
    ELSE
        INSERT INTO product_categories
            (pharmacy_id, name, description, created_by, created_on)
        VALUES
            (p_pharmacy_id, p_name, p_description, p_created_by, NOW());
        SET p_id = LAST_INSERT_ID();
        SET p_error_code = '00'; SET p_error_desc = 'Category added';
    END IF;
END$$

-- ============================================================
-- 3. SALES — create_sale
-- ============================================================
DROP PROCEDURE IF EXISTS `create_sale`$$
CREATE PROCEDURE `create_sale`(
    IN  p_pharmacy_id       BIGINT,
    IN  p_customer_id       BIGINT,
    IN  p_sale_number       VARCHAR(50),
    IN  p_sale_type         VARCHAR(20),
    IN  p_subtotal          DECIMAL(15,2),
    IN  p_vat_amount        DECIMAL(15,2),
    IN  p_discount          DECIMAL(15,2),
    IN  p_total             DECIMAL(15,2),
    IN  p_amount_paid       DECIMAL(15,2),
    IN  p_payment_method    VARCHAR(50),
    IN  p_payment_reference VARCHAR(200),
    IN  p_sold_by           BIGINT,
    OUT p_id                BIGINT,
    OUT p_error_code        VARCHAR(2),
    OUT p_error_desc        VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF EXISTS (SELECT 1 FROM sales WHERE sale_number = p_sale_number) THEN
        SET p_error_desc = 'Sale number already exists';
    ELSE
        INSERT INTO sales
            (pharmacy_id, customer_id, sale_number, sale_type, subtotal, vat_amount,
             discount, total, amount_paid, payment_method, payment_reference,
             sold_by, created_on)
        VALUES
            (p_pharmacy_id, p_customer_id, p_sale_number,
             COALESCE(p_sale_type, 'Retail'),
             COALESCE(p_subtotal, 0), COALESCE(p_vat_amount, 0),
             COALESCE(p_discount, 0), COALESCE(p_total, 0),
             COALESCE(p_amount_paid, 0), COALESCE(p_payment_method, 'Cash'),
             p_payment_reference, p_sold_by, NOW());
        SET p_id = LAST_INSERT_ID();
        SET p_error_code = '00'; SET p_error_desc = 'Sale created';
    END IF;
END$$

-- ============================================================
-- 3. SALES — add_sale_item
-- ============================================================
DROP PROCEDURE IF EXISTS `add_sale_item`$$
CREATE PROCEDURE `add_sale_item`(
    IN  p_sale_id      BIGINT,
    IN  p_product_id   BIGINT,
    IN  p_batch_id     BIGINT,
    IN  p_quantity     INT,
    IN  p_unit_price   DECIMAL(15,2),
    IN  p_cost_price   DECIMAL(15,2),
    IN  p_vat_rate     DECIMAL(5,2),
    IN  p_vat_amount   DECIMAL(15,2),
    IN  p_discount     DECIMAL(15,2),
    IN  p_total        DECIMAL(15,2),
    OUT p_id           BIGINT
)
BEGIN
    SET p_id = 0;
    INSERT INTO sale_items
        (sale_id, product_id, batch_id, quantity, unit_price, cost_price,
         vat_rate, vat_amount, discount, total)
    VALUES
        (p_sale_id, p_product_id, p_batch_id, p_quantity,
         COALESCE(p_unit_price, 0), COALESCE(p_cost_price, 0),
         COALESCE(p_vat_rate, 0), COALESCE(p_vat_amount, 0),
         COALESCE(p_discount, 0), COALESCE(p_total, 0));
    SET p_id = LAST_INSERT_ID();
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
    IN  p_pharmacy_id    BIGINT,
    IN  p_customer_type  VARCHAR(20),
    IN  p_first_name     VARCHAR(100),
    IN  p_last_name      VARCHAR(100),
    IN  p_phone          VARCHAR(50),
    IN  p_email          VARCHAR(200),
    IN  p_address        TEXT,
    IN  p_credit_limit   DECIMAL(15,2),
    IN  p_payment_terms  VARCHAR(50),
    IN  p_created_by     BIGINT,
    OUT p_id             BIGINT,
    OUT p_error_code     VARCHAR(2),
    OUT p_error_desc     VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    INSERT INTO customers
        (pharmacy_id, customer_type, first_name, last_name, phone, email,
         address, credit_limit, payment_terms, created_by, created_on)
    VALUES
        (p_pharmacy_id, COALESCE(p_customer_type, 'Retail'), p_first_name, p_last_name,
         p_phone, p_email, p_address, COALESCE(p_credit_limit, 0),
         COALESCE(p_payment_terms, 'Cash'), p_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
    SET p_error_code = '00'; SET p_error_desc = 'Customer added';
END$$

-- ============================================================
-- 5. SUPPLIERS — add_supplier
-- ============================================================
DROP PROCEDURE IF EXISTS `add_supplier`$$
CREATE PROCEDURE `add_supplier`(
    IN  p_pharmacy_id     BIGINT,
    IN  p_name            VARCHAR(200),
    IN  p_contact_person  VARCHAR(200),
    IN  p_phone           VARCHAR(50),
    IN  p_email           VARCHAR(200),
    IN  p_address         TEXT,
    IN  p_created_by      BIGINT,
    OUT p_id              BIGINT,
    OUT p_error_code      VARCHAR(2),
    OUT p_error_desc      VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    INSERT INTO suppliers
        (pharmacy_id, name, contact_person, phone, email, address, created_by, created_on)
    VALUES
        (p_pharmacy_id, p_name, p_contact_person, p_phone, p_email, p_address, p_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
    SET p_error_code = '00'; SET p_error_desc = 'Supplier added';
END$$

-- ============================================================
-- 5. SUPPLIERS — add_purchase_order
-- ============================================================
DROP PROCEDURE IF EXISTS `add_purchase_order`$$
CREATE PROCEDURE `add_purchase_order`(
    IN  p_pharmacy_id   BIGINT,
    IN  p_supplier_id   BIGINT,
    IN  p_po_number     VARCHAR(50),
    IN  p_total         DECIMAL(15,2),
    IN  p_expected_date DATE,
    IN  p_created_by    BIGINT,
    OUT p_id            BIGINT,
    OUT p_error_code    VARCHAR(2),
    OUT p_error_desc    VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF EXISTS (SELECT 1 FROM purchase_orders WHERE po_number = p_po_number) THEN
        SET p_error_desc = 'PO number already exists';
    ELSE
        INSERT INTO purchase_orders
            (pharmacy_id, supplier_id, po_number, total, expected_date, created_by, created_on)
        VALUES
            (p_pharmacy_id, p_supplier_id, p_po_number, COALESCE(p_total, 0),
             p_expected_date, p_created_by, NOW());
        SET p_id = LAST_INSERT_ID();
        SET p_error_code = '00'; SET p_error_desc = 'Purchase order created';
    END IF;
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
    IN  p_po_id          BIGINT,
    IN  p_pharmacy_id    BIGINT,
    IN  p_product_id     BIGINT,
    IN  p_batch_number   VARCHAR(100),
    IN  p_expiry_date    DATE,
    IN  p_unit_cost      DECIMAL(15,2),
    IN  p_quantity       INT,
    IN  p_created_by     BIGINT,
    OUT p_error_code     VARCHAR(2),
    OUT p_error_desc     VARCHAR(500)
)
BEGIN
    DECLARE v_batch_id BIGINT DEFAULT 0;
    SET p_error_code = '01'; SET p_error_desc = 'Failed';

    -- Create batch
    INSERT INTO product_batches
        (pharmacy_id, product_id, batch_number, expiry_date, cost_price, quantity,
         created_by, created_on)
    VALUES
        (p_pharmacy_id, p_product_id, p_batch_number, p_expiry_date, p_unit_cost,
         p_quantity, p_created_by, NOW());
    SET v_batch_id = LAST_INSERT_ID();

    -- Update product stock_qty
    UPDATE products
    SET stock_qty = stock_qty + p_quantity
    WHERE id = p_product_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;

    -- Update PO received_qty
    UPDATE po_items
    SET received_qty = received_qty + p_quantity
    WHERE po_id = p_po_id AND product_id = p_product_id;

    -- If all items on the PO are fully received, mark PO as Received
    IF NOT EXISTS (
        SELECT 1 FROM po_items
        WHERE po_id = p_po_id AND received_qty < quantity
    ) THEN
        UPDATE purchase_orders
        SET status = 'Received', received_date = CURDATE()
        WHERE id = p_po_id;
    END IF;

    SET p_error_code = '00'; SET p_error_desc = 'Stock received';
END$$

-- ============================================================
-- 6. FINANCE — add_expense
-- ============================================================
DROP PROCEDURE IF EXISTS `add_expense`$$
CREATE PROCEDURE `add_expense`(
    IN  p_pharmacy_id    BIGINT,
    IN  p_category_id    BIGINT,
    IN  p_description    VARCHAR(500),
    IN  p_amount         DECIMAL(15,2),
    IN  p_expense_date   DATE,
    IN  p_payment_method VARCHAR(50),
    IN  p_reference      VARCHAR(200),
    IN  p_created_by     BIGINT,
    OUT p_id             BIGINT,
    OUT p_error_code     VARCHAR(2),
    OUT p_error_desc     VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    INSERT INTO expenses
        (pharmacy_id, category_id, description, amount, expense_date,
         payment_method, reference, created_by, created_on)
    VALUES
        (p_pharmacy_id, p_category_id, p_description, COALESCE(p_amount, 0),
         COALESCE(p_expense_date, CURDATE()), COALESCE(p_payment_method, 'Cash'),
         p_reference, p_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
    SET p_error_code = '00'; SET p_error_desc = 'Expense added';
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
    IN  p_pharmacy_id      BIGINT,
    IN  p_first_name       VARCHAR(100),
    IN  p_last_name        VARCHAR(100),
    IN  p_phone            VARCHAR(50),
    IN  p_email            VARCHAR(200),
    IN  p_date_of_birth    DATE,
    IN  p_gender           VARCHAR(20),
    IN  p_address          TEXT,
    IN  p_nhif_number      VARCHAR(50),
    IN  p_created_by       BIGINT,
    OUT p_id               BIGINT,
    OUT p_error_code       VARCHAR(2),
    OUT p_error_desc       VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    INSERT INTO patients
        (pharmacy_id, first_name, last_name, phone, email, date_of_birth,
         gender, address, nhif_number, created_by, created_on)
    VALUES
        (p_pharmacy_id, p_first_name, p_last_name, p_phone, p_email,
         p_date_of_birth, p_gender, p_address, p_nhif_number, p_created_by, NOW());
    SET p_id = LAST_INSERT_ID();
    SET p_error_code = '00'; SET p_error_desc = 'Patient added';
END$$

-- ============================================================
-- 7. CLINICAL — add_prescription
-- ============================================================
DROP PROCEDURE IF EXISTS `add_prescription`$$
CREATE PROCEDURE `add_prescription`(
    IN  p_pharmacy_id         BIGINT,
    IN  p_patient_id          BIGINT,
    IN  p_prescription_number VARCHAR(50),
    IN  p_doctor_name         VARCHAR(200),
    IN  p_prescription_date   DATE,
    IN  p_notes               TEXT,
    IN  p_created_by          BIGINT,
    OUT p_id                  BIGINT,
    OUT p_error_code          VARCHAR(2),
    OUT p_error_desc          VARCHAR(500)
)
BEGIN
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';
    IF EXISTS (SELECT 1 FROM prescriptions WHERE prescription_number = p_prescription_number) THEN
        SET p_error_desc = 'Prescription number already exists';
    ELSE
        INSERT INTO prescriptions
            (pharmacy_id, patient_id, prescription_number, doctor_name,
             prescription_date, notes, created_by, created_on)
        VALUES
            (p_pharmacy_id, p_patient_id, p_prescription_number, p_doctor_name,
             COALESCE(p_prescription_date, CURDATE()), p_notes, p_created_by, NOW());
        SET p_id = LAST_INSERT_ID();
        SET p_error_code = '00'; SET p_error_desc = 'Prescription added';
    END IF;
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
    IN  p_pharmacy_id      BIGINT,
    IN  p_product_id       BIGINT,
    IN  p_batch_id         BIGINT,
    IN  p_entry_type       VARCHAR(50),
    IN  p_quantity         INT,
    IN  p_reference_number VARCHAR(100),
    IN  p_patient_name     VARCHAR(200),
    IN  p_prescriber_name  VARCHAR(200),
    IN  p_recorded_by      BIGINT,
    OUT p_id               BIGINT,
    OUT p_error_code       VARCHAR(2),
    OUT p_error_desc       VARCHAR(500)
)
BEGIN
    DECLARE v_current_stock INT DEFAULT 0;
    SET p_id = 0; SET p_error_code = '01'; SET p_error_desc = 'Failed';

    SELECT stock_qty INTO v_current_stock
    FROM products
    WHERE id = p_product_id AND pharmacy_id = p_pharmacy_id AND is_deleted = 0;

    INSERT INTO dda_register
        (pharmacy_id, product_id, batch_id, entry_type, quantity, reference_number,
         patient_name, prescriber_name, balance_after, recorded_by, created_on)
    VALUES
        (p_pharmacy_id, p_product_id, p_batch_id, p_entry_type, p_quantity,
         p_reference_number, p_patient_name, p_prescriber_name,
         COALESCE(v_current_stock, 0), p_recorded_by, NOW());
    SET p_id = LAST_INSERT_ID();
    SET p_error_code = '00'; SET p_error_desc = 'DDA entry recorded';
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

DELIMITER ;
