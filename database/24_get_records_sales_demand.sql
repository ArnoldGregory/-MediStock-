-- ============================================================
--  MediStock — 24. get_records: restore missing branches
--  A fresh install's final get_records (file 12) lacked the
--  branches the API depends on, most importantly 'sales_demand'
--  (used by the stock-performance report) as well as 'roles',
--  'pharmacy_settings' and the report_* modules.
--  This re-creates get_records as a superset of file 12 with
--  those branches re-added (SQL copied from file 02).
--  NOTE: not applied to the live DB (its get_records already
--  carries the equivalent branches).
-- ============================================================

USE medistock;

DROP PROCEDURE IF EXISTS get_records;

DELIMITER //
CREATE PROCEDURE `get_records`(
    IN p_module VARCHAR(100),
    IN p_param1 VARCHAR(200),
    IN p_param2 VARCHAR(200),
    IN p_param3 VARCHAR(200),
    IN p_param4 VARCHAR(200)
)
BEGIN
    DECLARE v_limit INT DEFAULT 50;

    CASE p_module
        WHEN 'roles' THEN
            SELECT id, role_name, description
            FROM roles
            WHERE COALESCE(is_deleted, 0) = 0
            ORDER BY id;

        WHEN 'menus' THEN
            SELECT id, main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type
            FROM menu_access_data
            ORDER BY menu_order, sub_menu_order;

        WHEN 'menu_access' THEN
            SELECT id, role_id, main_menu_name, sub_menu_name, page_url, can_access, menu_order, sub_menu_order
            FROM menu_access
            WHERE role_id = CAST(p_param1 AS UNSIGNED) AND can_access = 1
            ORDER BY menu_order, sub_menu_order;

        WHEN 'product_categories' THEN
            SELECT id, name, description, is_active, created_on
            FROM product_categories
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
            ORDER BY name;

        WHEN 'products' THEN
            SELECT p.id, p.category_id, p.name, p.sku, p.barcode, p.description,
                   p.cost_price, p.selling_price, p.vat_rate, p.reorder_level,
                   p.stock_qty, p.unit, p.is_controlled_drug, p.is_active, p.created_on,
                   c.name AS category_name
            FROM products p
            LEFT JOIN product_categories c ON c.id = p.category_id
            WHERE p.pharmacy_id = p_param1 AND p.is_deleted = 0
            ORDER BY p.name;

        WHEN 'product_batches' THEN
            SELECT b.id, b.product_id, b.batch_number, b.expiry_date, b.cost_price,
                   b.quantity, b.quantity_sold, b.status, b.created_on,
                   p.name AS product_name
            FROM product_batches b
            JOIN products p ON p.id = b.product_id
            WHERE b.pharmacy_id = p_param1 AND b.is_deleted = 0
            ORDER BY b.expiry_date;

        WHEN 'customers' THEN
            SELECT id, customer_type, first_name, last_name, phone, email,
                   credit_limit, outstanding_balance, payment_terms, is_active, created_on
            FROM customers
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
            ORDER BY first_name;

        WHEN 'wholesale_customers' THEN
            SELECT id, first_name, last_name, phone, email, credit_limit,
                   outstanding_balance, payment_terms, is_active, created_on
            FROM customers
            WHERE pharmacy_id = p_param1 AND customer_type = 'Wholesale' AND is_deleted = 0
            ORDER BY first_name;

        WHEN 'suppliers' THEN
            SELECT id, name, contact_person, phone, email, address, is_active, created_on
            FROM suppliers
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
            ORDER BY name;

        WHEN 'sales' THEN
            SELECT s.id, s.sale_number, s.sale_type, s.subtotal, s.vat_amount,
                   s.discount, s.total, s.amount_paid, s.payment_method, s.status,
                   s.created_on,
                   CONCAT(c.first_name, ' ', COALESCE(c.last_name, '')) AS customer_name
            FROM sales s
            LEFT JOIN customers c ON c.id = s.customer_id
            WHERE s.pharmacy_id = p_param1 AND s.is_deleted = 0
            ORDER BY s.created_on DESC;

        WHEN 'sale_items' THEN
            SELECT si.id, si.product_id, si.quantity, si.unit_price, si.cost_price,
                   si.vat_amount, si.discount, si.total,
                   p.name AS product_name
            FROM sale_items si
            JOIN products p ON p.id = si.product_id
            WHERE si.sale_id = p_param1
            ORDER BY si.id;

        WHEN 'purchase_orders' THEN
            SELECT po.id, po.po_number, po.status, po.total, po.expected_date,
                   po.received_date, po.created_on,
                   sp.name AS supplier_name
            FROM purchase_orders po
            JOIN suppliers sp ON sp.id = po.supplier_id
            WHERE po.pharmacy_id = p_param1 AND po.is_deleted = 0
            ORDER BY po.created_on DESC;

        WHEN 'po_items' THEN
            SELECT poi.id, poi.product_id, poi.quantity, poi.received_qty,
                   poi.unit_cost, poi.total,
                   p.name AS product_name
            FROM po_items poi
            JOIN products p ON p.id = poi.product_id
            WHERE poi.po_id = p_param1
            ORDER BY poi.id;

        WHEN 'expense_categories' THEN
            SELECT id, name, is_active, created_on
            FROM expense_categories
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
            ORDER BY name;

        WHEN 'expenses' THEN
            SELECT e.id, e.description, e.amount, e.expense_date, e.payment_method,
                   e.reference, e.created_on,
                   ec.name AS category_name
            FROM expenses e
            LEFT JOIN expense_categories ec ON ec.id = e.category_id
            WHERE e.pharmacy_id = p_param1 AND e.is_deleted = 0
            ORDER BY e.expense_date DESC;

        WHEN 'patients' THEN
            SELECT id, first_name, last_name, phone, email, date_of_birth,
                   gender, nhif_number, is_active, created_on
            FROM patients
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
            ORDER BY first_name;

        WHEN 'prescriptions' THEN
            SELECT pr.id, pr.prescription_number, pr.doctor_name, pr.prescription_date,
                   pr.status, pr.created_on,
                   CONCAT(pt.first_name, ' ', COALESCE(pt.last_name, '')) AS patient_name
            FROM prescriptions pr
            JOIN patients pt ON pt.id = pr.patient_id
            WHERE pr.pharmacy_id = p_param1 AND pr.is_deleted = 0
            ORDER BY pr.created_on DESC;

        WHEN 'prescription_items' THEN
            SELECT pri.id, pri.product_id, pri.medication_name, pri.dosage,
                   pri.frequency, pri.duration, pri.quantity, pri.notes,
                   p.name AS product_name
            FROM prescription_items pri
            LEFT JOIN products p ON p.id = pri.product_id
            WHERE pri.prescription_id = p_param1
            ORDER BY pri.id;

        WHEN 'dda_register' THEN
            SELECT d.id, d.entry_type, d.quantity, d.reference_number,
                   d.patient_name, d.prescriber_name, d.balance_after,
                   d.created_on,
                   p.name AS product_name
            FROM dda_register d
            JOIN products p ON p.id = d.product_id
            WHERE d.pharmacy_id = p_param1 AND d.is_deleted = 0
            ORDER BY d.created_on DESC;

        WHEN 'stock_adjustments' THEN
            SELECT sa.id, sa.adjustment_type, sa.quantity, sa.reason, sa.created_on,
                   p.name AS product_name
            FROM stock_adjustments sa
            JOIN products p ON p.id = sa.product_id
            WHERE sa.pharmacy_id = p_param1 AND sa.is_deleted = 0
            ORDER BY sa.created_on DESC;

        WHEN 'stock_take_sessions' THEN
            SELECT id, session_name, status, started_on, committed_on
            FROM stock_take_sessions
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
            ORDER BY started_on DESC;

        WHEN 'pharmacy_users' THEN
            SELECT id, role_id, first_name, last_name, email, mobile,
                   avatar, locked, is_active, created_on
            FROM pharmacy_users
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
            ORDER BY first_name;

        WHEN 'notifications' THEN
            SELECT id, title, message, notification_type, is_read, created_on
            FROM notifications
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
              AND (user_id = p_param2 OR p_param2 = '' OR p_param2 IS NULL)
            ORDER BY created_on DESC
            LIMIT 50;

        WHEN 'supplier_price_history' THEN
            SELECT sph.id, sph.unit_cost, sph.recorded_on,
                   p.name AS product_name, sp.name AS supplier_name
            FROM supplier_price_history sph
            JOIN products p ON p.id = sph.product_id
            JOIN suppliers sp ON sp.id = sph.supplier_id
            WHERE sph.pharmacy_id = p_param1
            ORDER BY sph.recorded_on DESC;

        WHEN 'patient_allergies' THEN
            SELECT id, allergen, severity, notes
            FROM patient_allergies
            WHERE patient_id = p_param1 AND is_deleted = 0
            ORDER BY allergen;

        WHEN 'patient_conditions' THEN
            SELECT id, condition_name, diagnosed_date, notes, is_active
            FROM patient_conditions
            WHERE patient_id = p_param1 AND is_deleted = 0
            ORDER BY condition_name;

        WHEN 'expiring_batches' THEN
            SELECT b.id, b.batch_number, b.expiry_date, b.quantity, b.cost_price,
                   p.name AS product_name
            FROM product_batches b
            JOIN products p ON p.id = b.product_id
            WHERE b.pharmacy_id = p_param1 AND b.is_deleted = 0
              AND b.expiry_date BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 90 DAY)
              AND b.quantity > 0
            ORDER BY b.expiry_date;

        WHEN 'sales_demand' THEN
            SELECT si.product_id, p.name AS product_name,
                   SUM(si.quantity) AS units_30d, COUNT(DISTINCT s.id) AS sale_count
            FROM sale_items si
            JOIN sales s ON s.id = si.sale_id
            JOIN products p ON p.id = si.product_id
            WHERE s.pharmacy_id = p_param1 AND s.is_deleted = 0
              AND s.status IN ('Completed', 'Paid')
              AND s.created_on >= DATE_SUB(NOW(), INTERVAL 30 DAY)
            GROUP BY si.product_id, p.name;

        WHEN 'low_stock_products' THEN
            SELECT id, name, sku, stock_qty, reorder_level
            FROM products
            WHERE pharmacy_id = p_param1 AND is_deleted = 0
              AND stock_qty <= reorder_level
            ORDER BY stock_qty ASC;

        WHEN 'pharmacy_settings' THEN
            SELECT setting_key, setting_value
            FROM pharmacy_settings
            WHERE pharmacy_id = p_param1;

        WHEN 'audit_trail' THEN
            SET v_limit = CAST(COALESCE(NULLIF(p_param2, ''), '50') AS UNSIGNED);
            SELECT id, user_name, action_type, action_description,
                   page_accessed, client_ip_address, created_on
            FROM audit_trail
            ORDER BY created_on DESC
            LIMIT v_limit;

        WHEN 'report_sales' THEN
            SELECT s.sale_number, s.sale_type, s.created_on,
                   s.subtotal, s.discount, s.total, s.payment_method, s.status,
                   CONCAT(c.first_name, ' ', COALESCE(c.last_name, '')) AS customer_name
            FROM sales s
            LEFT JOIN customers c ON c.id = s.customer_id
            WHERE s.pharmacy_id = p_param1 AND s.is_deleted = 0
              AND (p_param2 = '' OR s.created_on >= p_param2)
              AND (p_param3 = '' OR s.created_on <= p_param3)
            ORDER BY s.created_on DESC;

        WHEN 'report_stock' THEN
            SELECT p.id AS product_id, p.name, p.sku, p.stock_qty,
                   p.cost_price, p.selling_price,
                   (p.stock_qty * p.cost_price) AS total_cost_value,
                   p.reorder_level, c.name AS category_name
            FROM products p
            LEFT JOIN product_categories c ON c.id = p.category_id
            WHERE p.pharmacy_id = p_param1 AND p.is_deleted = 0
            ORDER BY p.name;

        WHEN 'report_financial' THEN
            SELECT s.created_on, s.sale_number,
                   s.total AS sales_amount, s.payment_method,
                   e.description, e.amount AS expense_amount,
                   (COALESCE(s.total, 0) - COALESCE(e.amount, 0)) AS net
            FROM sales s
            LEFT JOIN expenses e ON 1 = 0
            WHERE s.pharmacy_id = p_param1 AND s.is_deleted = 0
            ORDER BY s.created_on DESC
            LIMIT 50;

        WHEN 'report_expense_by_category' THEN
            SELECT ec.name AS category_name, SUM(e.amount) AS total
            FROM expenses e
            JOIN expense_categories ec ON ec.id = e.category_id
            WHERE e.pharmacy_id = p_param1 AND e.is_deleted = 0
            GROUP BY ec.name
            ORDER BY total DESC;

        WHEN 'report_product_margins' THEN
            SELECT p.id AS product_id, p.name, p.sku,
                   p.cost_price, p.selling_price,
                   (p.selling_price - p.cost_price) AS margin,
                   ROUND((p.selling_price - p.cost_price) / NULLIF(p.cost_price, 0) * 100, 2) AS margin_pct
            FROM products p
            WHERE p.pharmacy_id = p_param1 AND p.is_deleted = 0
            ORDER BY margin_pct DESC;

        ELSE
            SELECT 'Unknown module' AS error;
    END CASE;
END //
DELIMITER ;