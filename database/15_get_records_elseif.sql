DELIMITER $$

USE `medistock`$$

DROP PROCEDURE IF EXISTS `get_records`$$

CREATE DEFINER=`RizikiDev`@`%` PROCEDURE `get_records`(
    IN module  VARCHAR(100),
    IN param1  VARCHAR(2500),
    IN param2  VARCHAR(100),
    IN param3  VARCHAR(100),
    IN param4  VARCHAR(100)
)
BEGIN
	DECLARE v_limit INT DEFAULT 50;

	IF module = 'roles' THEN
		SELECT id, role_name, description
		FROM roles
		WHERE COALESCE(is_deleted, 0) = 0
		ORDER BY id;

	ELSEIF module = 'menus' THEN
		SELECT id, main_menu_name, sub_menu_name, menu_icon, menu_order, sub_menu_order, page_url, menu_type
		FROM menu_access_data
		ORDER BY menu_order, sub_menu_order;

	ELSEIF module = 'menu_access' THEN
		SELECT id, role_id, main_menu_name, sub_menu_name, page_url, can_access, menu_order, sub_menu_order
		FROM menu_access
		WHERE role_id = CAST(param1 AS UNSIGNED) AND can_access = 1
		ORDER BY menu_order, sub_menu_order;

	ELSEIF module = 'product_categories' THEN
		SELECT id, name, description, is_active, created_on
		FROM product_categories
		WHERE pharmacy_id = param1 AND is_deleted = 0
		ORDER BY name;

	ELSEIF module = 'products' THEN
		SELECT p.id, p.category_id, p.name, p.sku, p.barcode, p.description,
		       p.cost_price, p.selling_price, p.vat_rate, p.reorder_level,
		       p.stock_qty, p.unit, p.is_controlled_drug, p.is_active, p.created_on,
		       c.name AS category_name
		FROM products p
		LEFT JOIN product_categories c ON c.id = p.category_id
		WHERE p.pharmacy_id = param1 AND p.is_deleted = 0
		ORDER BY p.name;

	ELSEIF module = 'product_batches' THEN
		SELECT b.id, b.product_id, b.batch_number, b.expiry_date, b.cost_price,
		       b.quantity, b.quantity_sold, b.status, b.created_on,
		       p.name AS product_name
		FROM product_batches b
		JOIN products p ON p.id = b.product_id
		WHERE b.pharmacy_id = param1 AND b.is_deleted = 0
		ORDER BY b.expiry_date;

	ELSEIF module = 'customers' THEN
		SELECT id, customer_type, first_name, last_name, phone, email,
		       credit_limit, outstanding_balance, payment_terms, is_active, created_on
		FROM customers
		WHERE pharmacy_id = param1 AND is_deleted = 0
		ORDER BY first_name;

	ELSEIF module = 'wholesale_customers' THEN
		SELECT id, first_name, last_name, phone, email, credit_limit,
		       outstanding_balance, payment_terms, is_active, created_on
		FROM customers
		WHERE pharmacy_id = param1 AND customer_type = 'Wholesale' AND is_deleted = 0
		ORDER BY first_name;

	ELSEIF module = 'suppliers' THEN
		SELECT id, name, contact_person, phone, email, address, is_active, created_on
		FROM suppliers
		WHERE pharmacy_id = param1 AND is_deleted = 0
		ORDER BY name;

	ELSEIF module = 'sales' THEN
		SELECT s.id, s.sale_number, s.sale_type, s.subtotal, s.vat_amount,
		       s.discount, s.total, s.amount_paid, s.payment_method, s.status,
		       s.created_on,
		       CONCAT(c.first_name, ' ', COALESCE(c.last_name, '')) AS customer_name
		FROM sales s
		LEFT JOIN customers c ON c.id = s.customer_id
		WHERE s.pharmacy_id = param1 AND s.is_deleted = 0
		ORDER BY s.created_on DESC;

	ELSEIF module = 'sale_items' THEN
		SELECT si.id, si.product_id, si.quantity, si.unit_price, si.cost_price,
		       si.vat_amount, si.discount, si.total,
		       p.name AS product_name
		FROM sale_items si
		JOIN products p ON p.id = si.product_id
		WHERE si.sale_id = param1
		ORDER BY si.id;

	ELSEIF module = 'purchase_orders' THEN
		SELECT po.id, po.po_number, po.status, po.total, po.expected_date,
		       po.received_date, po.created_on,
		       sp.name AS supplier_name
		FROM purchase_orders po
		JOIN suppliers sp ON sp.id = po.supplier_id
		WHERE po.pharmacy_id = param1 AND po.is_deleted = 0
		ORDER BY po.created_on DESC;

	ELSEIF module = 'po_items' THEN
		SELECT poi.id, poi.product_id, poi.quantity, poi.received_qty,
		       poi.unit_cost, poi.total,
		       p.name AS product_name
		FROM po_items poi
		JOIN products p ON p.id = poi.product_id
		WHERE poi.po_id = param1
		ORDER BY poi.id;

	ELSEIF module = 'expense_categories' THEN
		SELECT id, name, is_active, created_on
		FROM expense_categories
		WHERE pharmacy_id = param1 AND is_deleted = 0
		ORDER BY name;

	ELSEIF module = 'expenses' THEN
		SELECT e.id, e.description, e.amount, e.expense_date, e.payment_method,
		       e.reference, e.created_on,
		       ec.name AS category_name
		FROM expenses e
		LEFT JOIN expense_categories ec ON ec.id = e.category_id
		WHERE e.pharmacy_id = param1 AND e.is_deleted = 0
		ORDER BY e.expense_date DESC;

	ELSEIF module = 'patients' THEN
		SELECT id, first_name, last_name, phone, email, date_of_birth,
		       gender, nhif_number, is_active, created_on
		FROM patients
		WHERE pharmacy_id = param1 AND is_deleted = 0
		ORDER BY first_name;

	ELSEIF module = 'prescriptions' THEN
		SELECT pr.id, pr.prescription_number, pr.doctor_name, pr.prescription_date,
		       pr.status, pr.created_on,
		       CONCAT(pt.first_name, ' ', COALESCE(pt.last_name, '')) AS patient_name
		FROM prescriptions pr
		JOIN patients pt ON pt.id = pr.patient_id
		WHERE pr.pharmacy_id = param1 AND pr.is_deleted = 0
		ORDER BY pr.created_on DESC;

	ELSEIF module = 'prescription_items' THEN
		SELECT pri.id, pri.product_id, pri.medication_name, pri.dosage,
		       pri.frequency, pri.duration, pri.quantity, pri.notes,
		       p.name AS product_name
		FROM prescription_items pri
		LEFT JOIN products p ON p.id = pri.product_id
		WHERE pri.prescription_id = param1
		ORDER BY pri.id;

	ELSEIF module = 'dda_register' THEN
		SELECT d.id, d.entry_type, d.quantity, d.reference_number,
		       d.patient_name, d.prescriber_name, d.balance_after,
		       d.created_on,
		       p.name AS product_name
		FROM dda_register d
		JOIN products p ON p.id = d.product_id
		WHERE d.pharmacy_id = param1 AND d.is_deleted = 0
		ORDER BY d.created_on DESC;

	ELSEIF module = 'stock_adjustments' THEN
		SELECT sa.id, sa.adjustment_type, sa.quantity, sa.reason, sa.created_on,
		       p.name AS product_name
		FROM stock_adjustments sa
		JOIN products p ON p.id = sa.product_id
		WHERE sa.pharmacy_id = param1 AND sa.is_deleted = 0
		ORDER BY sa.created_on DESC;

	ELSEIF module = 'stock_take_sessions' THEN
		SELECT id, session_name, status, started_on, committed_on
		FROM stock_take_sessions
		WHERE pharmacy_id = param1 AND is_deleted = 0
		ORDER BY started_on DESC;

	ELSEIF module = 'pharmacy_users' THEN
		SELECT id, role_id, first_name, last_name, email, mobile,
		       avatar, locked, is_active, created_on
		FROM pharmacy_users
		WHERE pharmacy_id = param1 AND is_deleted = 0
		ORDER BY first_name;

	ELSEIF module = 'notifications' THEN
		SELECT id, title, message, notification_type, is_read, created_on
		FROM notifications
		WHERE pharmacy_id = param1 AND is_deleted = 0
		  AND (user_id = param2 OR param2 = '' OR param2 IS NULL)
		ORDER BY created_on DESC
		LIMIT 50;

	ELSEIF module = 'supplier_price_history' THEN
		SELECT sph.id, sph.unit_cost, sph.recorded_on,
		       p.name AS product_name, sp.name AS supplier_name
		FROM supplier_price_history sph
		JOIN products p ON p.id = sph.product_id
		JOIN suppliers sp ON sp.id = sph.supplier_id
		WHERE sph.pharmacy_id = param1
		ORDER BY sph.recorded_on DESC;

	ELSEIF module = 'patient_allergies' THEN
		SELECT id, allergen, severity, notes
		FROM patient_allergies
		WHERE patient_id = param1 AND is_deleted = 0
		ORDER BY allergen;

	ELSEIF module = 'patient_conditions' THEN
		SELECT id, condition_name, diagnosed_date, notes, is_active
		FROM patient_conditions
		WHERE patient_id = param1 AND is_deleted = 0
		ORDER BY condition_name;

	ELSEIF module = 'expiring_batches' THEN
		SELECT b.id, b.batch_number, b.expiry_date, b.quantity, b.cost_price,
		       p.name AS product_name
		FROM product_batches b
		JOIN products p ON p.id = b.product_id
		WHERE b.pharmacy_id = param1 AND b.is_deleted = 0
		  AND b.expiry_date BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 90 DAY)
		  AND b.quantity > 0
		ORDER BY b.expiry_date;

	ELSEIF module = 'low_stock_products' THEN
		SELECT id, name, sku, stock_qty, reorder_level
		FROM products
		WHERE pharmacy_id = param1 AND is_deleted = 0
		  AND stock_qty <= reorder_level
		ORDER BY stock_qty ASC;

	ELSEIF module = 'audit_trail' THEN
		SET v_limit = CAST(COALESCE(NULLIF(param2, ''), '50') AS UNSIGNED);
		SELECT id, user_name, action_type, action_description,
		       page_accessed, client_ip_address, created_on
		FROM audit_trail
		ORDER BY created_on DESC
		LIMIT v_limit;

	ELSE
		SELECT 'Unknown module' AS `error`;
	END IF;
END$$

DELIMITER ;

-- ============================================================================
-- get_records_by_id — matches DBHandler.GetRecordsById (@module, @record_id)
-- plus a 'roles' case so GetRoleById works.
-- ============================================================================
DELIMITER $$

USE `medistock`$$

DROP PROCEDURE IF EXISTS `get_records_by_id`$$

CREATE DEFINER=`RizikiDev`@`%` PROCEDURE `get_records_by_id`(
    IN module    VARCHAR(100),
    IN record_id VARCHAR(100)
)
BEGIN
    IF module = 'roles' THEN
        SELECT * FROM roles WHERE id = record_id AND COALESCE(is_deleted, 0) = 0;

    ELSEIF module = 'product' THEN
        SELECT * FROM products WHERE id = record_id AND is_deleted = 0;

    ELSEIF module = 'category' THEN
        SELECT * FROM product_categories WHERE id = record_id AND is_deleted = 0;

    ELSEIF module = 'batch' THEN
        SELECT b.*, p.name AS product_name
        FROM product_batches b
        JOIN products p ON p.id = b.product_id
        WHERE b.id = record_id AND b.is_deleted = 0;

    ELSEIF module = 'customer' THEN
        SELECT * FROM customers WHERE id = record_id AND is_deleted = 0;

    ELSEIF module = 'supplier' THEN
        SELECT * FROM suppliers WHERE id = record_id AND is_deleted = 0;

    ELSEIF module = 'sale' THEN
        SELECT s.*, CONCAT(c.first_name, ' ', COALESCE(c.last_name, '')) AS customer_name
        FROM sales s
        LEFT JOIN customers c ON c.id = s.customer_id
        WHERE s.id = record_id AND s.is_deleted = 0;

    ELSEIF module = 'purchase_order' THEN
        SELECT po.*, sp.name AS supplier_name
        FROM purchase_orders po
        JOIN suppliers sp ON sp.id = po.supplier_id
        WHERE po.id = record_id AND po.is_deleted = 0;

    ELSEIF module = 'patient' THEN
        SELECT * FROM patients WHERE id = record_id AND is_deleted = 0;

    ELSEIF module = 'prescription' THEN
        SELECT pr.*, CONCAT(pt.first_name, ' ', COALESCE(pt.last_name, '')) AS patient_name
        FROM prescriptions pr
        JOIN patients pt ON pt.id = pr.patient_id
        WHERE pr.id = record_id AND pr.is_deleted = 0;

    ELSEIF module = 'expense' THEN
        SELECT e.*, ec.name AS category_name
        FROM expenses e
        LEFT JOIN expense_categories ec ON ec.id = e.category_id
        WHERE e.id = record_id AND e.is_deleted = 0;

    ELSEIF module = 'user' THEN
        SELECT id, pharmacy_id, role_id, first_name, last_name, email, mobile,
               avatar, locked, is_active, created_on
        FROM pharmacy_users WHERE id = record_id AND is_deleted = 0;

    ELSEIF module = 'pharmacy' THEN
        SELECT * FROM pharmacies WHERE id = record_id AND is_deleted = 0;

    ELSEIF module = 'dda_entry' THEN
        SELECT d.*, p.name AS product_name
        FROM dda_register d
        JOIN products p ON p.id = d.product_id
        WHERE d.id = record_id AND d.is_deleted = 0;

    ELSEIF module = 'stock_take_session' THEN
        SELECT * FROM stock_take_sessions WHERE id = record_id AND is_deleted = 0;

    ELSE
        SELECT 'Unknown module' AS `error`;
    END IF;
END$$

DELIMITER ;

-- ============================================================================
-- delete_records — matches DBHandler.DeleteRecord (@recordid, @in_deleted_by, @module)
-- plus a 'roles' case so DeleteRole works (soft delete).
-- ============================================================================
DELIMITER $$

USE `medistock`$$

DROP PROCEDURE IF EXISTS `delete_records`$$

CREATE DEFINER=`RizikiDev`@`%` PROCEDURE `delete_records`(
    IN recordid     BIGINT,
    IN in_deleted_by BIGINT,
    IN module       VARCHAR(100)
)
BEGIN
    IF module = 'roles' THEN
        UPDATE roles SET is_deleted = 1, deleted_by = in_deleted_by, deleted_on = NOW()
        WHERE id = recordid;

    ELSEIF module = 'product' THEN
        UPDATE products SET is_deleted = 1, created_by = in_deleted_by WHERE id = recordid;
    ELSEIF module = 'category' THEN
        UPDATE product_categories SET is_deleted = 1, created_by = in_deleted_by WHERE id = recordid;
    ELSEIF module = 'batch' THEN
        UPDATE product_batches SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'customer' THEN
        UPDATE customers SET is_deleted = 1, created_by = in_deleted_by WHERE id = recordid;
    ELSEIF module = 'supplier' THEN
        UPDATE suppliers SET is_deleted = 1, created_by = in_deleted_by WHERE id = recordid;
    ELSEIF module = 'sale' THEN
        UPDATE sales SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'purchase_order' THEN
        UPDATE purchase_orders SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'expense' THEN
        UPDATE expenses SET is_deleted = 1, created_by = in_deleted_by WHERE id = recordid;
    ELSEIF module = 'expense_category' THEN
        UPDATE expense_categories SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'patient' THEN
        UPDATE patients SET is_deleted = 1, created_by = in_deleted_by WHERE id = recordid;
    ELSEIF module = 'prescription' THEN
        UPDATE prescriptions SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'dda_entry' THEN
        UPDATE dda_register SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'stock_adjustment' THEN
        UPDATE stock_adjustments SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'stock_take_session' THEN
        UPDATE stock_take_sessions SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'pharmacy_user' THEN
        UPDATE pharmacy_users SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'notification' THEN
        UPDATE notifications SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'supplier_price_history' THEN
        DELETE FROM supplier_price_history WHERE id = recordid;
    ELSEIF module = 'patient_allergy' THEN
        UPDATE patient_allergies SET is_deleted = 1 WHERE id = recordid;
    ELSEIF module = 'patient_condition' THEN
        UPDATE patient_conditions SET is_deleted = 1 WHERE id = recordid;

    ELSE
        SELECT 'Unknown module' AS `error`;
    END IF;
END$$

DELIMITER ;
