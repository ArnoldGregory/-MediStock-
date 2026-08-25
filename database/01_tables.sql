-- ============================================================
--  MediStock — Database Schema
--  MySQL Database for Pharmaceutical Inventory Management
--  Follows Riziki pattern: multi-tenant, stored procedures only
-- ============================================================

CREATE DATABASE IF NOT EXISTS medistock CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE medistock;

-- ============================================================
-- CORE TABLES
-- ============================================================

CREATE TABLE IF NOT EXISTS `pharmacies` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `slug` varchar(100) NOT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `email` varchar(200) DEFAULT NULL,
  `address` text,
  `license_number` varchar(100) DEFAULT NULL,
  `license_expiry` date DEFAULT NULL,
  `currency` varchar(10) DEFAULT 'KES',
  `vat_number` varchar(100) DEFAULT NULL,
  `receipt_footer` text,
  `subscription_plan` varchar(20) DEFAULT 'Starter',
  `subscription_expiry` date DEFAULT NULL,
  `is_active` tinyint DEFAULT 1,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_pharmacy_slug` (`slug`),
  KEY `idx_pharmacy_active` (`is_active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `pharmacy_users` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `role_id` int NOT NULL DEFAULT 3,
  `first_name` varchar(100) DEFAULT NULL,
  `middle_name` varchar(100) DEFAULT NULL,
  `last_name` varchar(100) DEFAULT NULL,
  `email` varchar(200) NOT NULL,
  `mobile` varchar(50) DEFAULT NULL,
  `password` varchar(200) NOT NULL,
  `avatar` varchar(500) DEFAULT NULL,
  `locked` tinyint DEFAULT 0,
  `change_password` tinyint DEFAULT 0,
  `failed_login_attempts` int DEFAULT 0,
  `google_authenticate` tinyint DEFAULT 0,
  `sec_key` varchar(200) DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_user_email` (`email`),
  KEY `idx_user_pharmacy` (`pharmacy_id`),
  KEY `idx_user_role` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `refresh_tokens` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL,
  `token` varchar(500) NOT NULL,
  `expires_at` datetime NOT NULL,
  `revoked_at` datetime DEFAULT NULL,
  `revoked_by_ip` varchar(100) DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_refresh_user` (`user_id`),
  KEY `idx_refresh_token` (`token`(100))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `otp_records` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL,
  `user_type` varchar(20) NOT NULL,
  `email` varchar(200) NOT NULL,
  `mobile` varchar(50) DEFAULT NULL,
  `otp_code` varchar(10) NOT NULL,
  `purpose` varchar(50) NOT NULL,
  `otp_ref` varchar(100) NOT NULL,
  `verified` tinyint DEFAULT 0,
  `expires_at` datetime NOT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_otp_email` (`email`),
  KEY `idx_otp_ref` (`otp_ref`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `menu_access` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `role_id` int NOT NULL,
  `main_menu_name` varchar(100) NOT NULL,
  `sub_menu_name` varchar(100) NOT NULL,
  `page_url` varchar(500) NOT NULL,
  `can_access` tinyint DEFAULT 1,
  `menu_order` int DEFAULT 0,
  `sub_menu_order` int DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_menu_access` (`role_id`, `main_menu_name`, `sub_menu_name`),
  KEY `idx_menu_role` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `audit_trail` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_name` varchar(200) DEFAULT NULL,
  `action_type` varchar(50) DEFAULT NULL,
  `action_description` text,
  `page_accessed` varchar(500) DEFAULT NULL,
  `client_ip_address` varchar(100) DEFAULT NULL,
  `session_id` varchar(200) DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_audit_user` (`user_name`),
  KEY `idx_audit_date` (`created_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- INVENTORY TABLES
-- ============================================================

CREATE TABLE IF NOT EXISTS `product_categories` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `name` varchar(200) NOT NULL,
  `description` text,
  `is_active` tinyint DEFAULT 1,
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_cat_pharmacy` (`pharmacy_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `products` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `category_id` bigint DEFAULT NULL,
  `name` varchar(300) NOT NULL,
  `sku` varchar(100) DEFAULT NULL,
  `barcode` varchar(100) DEFAULT NULL,
  `description` text,
  `cost_price` decimal(15,2) DEFAULT 0.00,
  `selling_price` decimal(15,2) DEFAULT 0.00,
  `vat_rate` decimal(5,2) DEFAULT 16.00,
  `reorder_level` int DEFAULT 0,
  `stock_qty` int DEFAULT 0,
  `unit` varchar(50) DEFAULT 'pcs',
  `is_controlled_drug` tinyint DEFAULT 0,
  `is_active` tinyint DEFAULT 1,
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_prod_pharmacy` (`pharmacy_id`),
  KEY `idx_prod_category` (`category_id`),
  KEY `idx_prod_sku` (`sku`),
  KEY `idx_prod_barcode` (`barcode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `product_batches` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `batch_number` varchar(100) NOT NULL,
  `expiry_date` date NOT NULL,
  `cost_price` decimal(15,2) DEFAULT 0.00,
  `quantity` int DEFAULT 0,
  `quantity_sold` int DEFAULT 0,
  `status` varchar(20) DEFAULT 'Active',
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_batch_pharmacy` (`pharmacy_id`),
  KEY `idx_batch_product` (`product_id`),
  KEY `idx_batch_expiry` (`expiry_date`),
  KEY `idx_batch_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_adjustments` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `batch_id` bigint DEFAULT NULL,
  `adjustment_type` varchar(50) NOT NULL,
  `quantity` int NOT NULL,
  `reason` text,
  `adjusted_by` bigint DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_adj_pharmacy` (`pharmacy_id`),
  KEY `idx_adj_product` (`product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_take_sessions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `session_name` varchar(200) NOT NULL,
  `status` varchar(20) DEFAULT 'Open',
  `started_by` bigint DEFAULT NULL,
  `committed_by` bigint DEFAULT NULL,
  `started_on` datetime DEFAULT NULL,
  `committed_on` datetime DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `idx_st_pharmacy` (`pharmacy_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_take_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `session_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `batch_id` bigint DEFAULT NULL,
  `system_qty` int DEFAULT 0,
  `counted_qty` int DEFAULT 0,
  `variance` int DEFAULT 0,
  `notes` text,
  PRIMARY KEY (`id`),
  KEY `idx_sti_session` (`session_id`),
  KEY `idx_sti_product` (`product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- CUSTOMER & SALES TABLES
-- ============================================================

CREATE TABLE IF NOT EXISTS `customers` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `customer_type` varchar(20) DEFAULT 'Retail',
  `first_name` varchar(100) DEFAULT NULL,
  `last_name` varchar(100) DEFAULT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `email` varchar(200) DEFAULT NULL,
  `address` text,
  `credit_limit` decimal(15,2) DEFAULT 0.00,
  `outstanding_balance` decimal(15,2) DEFAULT 0.00,
  `payment_terms` varchar(50) DEFAULT 'Cash',
  `is_active` tinyint DEFAULT 1,
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_cust_pharmacy` (`pharmacy_id`),
  KEY `idx_cust_phone` (`phone`),
  KEY `idx_cust_type` (`customer_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `sales` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `customer_id` bigint DEFAULT NULL,
  `sale_number` varchar(50) NOT NULL,
  `sale_type` varchar(20) DEFAULT 'Retail',
  `subtotal` decimal(15,2) DEFAULT 0.00,
  `vat_amount` decimal(15,2) DEFAULT 0.00,
  `discount` decimal(15,2) DEFAULT 0.00,
  `total` decimal(15,2) DEFAULT 0.00,
  `amount_paid` decimal(15,2) DEFAULT 0.00,
  `payment_method` varchar(50) DEFAULT 'Cash',
  `payment_reference` varchar(200) DEFAULT NULL,
  `status` varchar(20) DEFAULT 'Completed',
  `sold_by` bigint DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_sale_number` (`sale_number`),
  KEY `idx_sale_pharmacy` (`pharmacy_id`),
  KEY `idx_sale_customer` (`customer_id`),
  KEY `idx_sale_date` (`created_on`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `sale_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `sale_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `batch_id` bigint DEFAULT NULL,
  `quantity` int NOT NULL,
  `unit_price` decimal(15,2) DEFAULT 0.00,
  `cost_price` decimal(15,2) DEFAULT 0.00,
  `vat_rate` decimal(5,2) DEFAULT 0.00,
  `vat_amount` decimal(15,2) DEFAULT 0.00,
  `discount` decimal(15,2) DEFAULT 0.00,
  `total` decimal(15,2) DEFAULT 0.00,
  PRIMARY KEY (`id`),
  KEY `idx_si_sale` (`sale_id`),
  KEY `idx_si_product` (`product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- SUPPLIER & PROCUREMENT TABLES
-- ============================================================

CREATE TABLE IF NOT EXISTS `suppliers` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `name` varchar(200) NOT NULL,
  `contact_person` varchar(200) DEFAULT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `email` varchar(200) DEFAULT NULL,
  `address` text,
  `is_active` tinyint DEFAULT 1,
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_supp_pharmacy` (`pharmacy_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `purchase_orders` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `supplier_id` bigint NOT NULL,
  `po_number` varchar(50) NOT NULL,
  `status` varchar(20) DEFAULT 'Pending',
  `total` decimal(15,2) DEFAULT 0.00,
  `expected_date` date DEFAULT NULL,
  `received_date` date DEFAULT NULL,
  `created_by` bigint DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_po_number` (`po_number`),
  KEY `idx_po_pharmacy` (`pharmacy_id`),
  KEY `idx_po_supplier` (`supplier_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `po_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `po_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `quantity` int NOT NULL,
  `received_qty` int DEFAULT 0,
  `unit_cost` decimal(15,2) DEFAULT 0.00,
  `total` decimal(15,2) DEFAULT 0.00,
  PRIMARY KEY (`id`),
  KEY `idx_poi_po` (`po_id`),
  KEY `idx_poi_product` (`product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `supplier_price_history` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `supplier_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `unit_cost` decimal(15,2) DEFAULT 0.00,
  `recorded_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_sph_pharmacy` (`pharmacy_id`),
  KEY `idx_sph_supplier` (`supplier_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- FINANCE TABLES
-- ============================================================

CREATE TABLE IF NOT EXISTS `expense_categories` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `name` varchar(200) NOT NULL,
  `is_active` tinyint DEFAULT 1,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_exp_cat_pharmacy` (`pharmacy_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `expenses` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `category_id` bigint DEFAULT NULL,
  `description` varchar(500) NOT NULL,
  `amount` decimal(15,2) DEFAULT 0.00,
  `expense_date` date DEFAULT NULL,
  `payment_method` varchar(50) DEFAULT 'Cash',
  `reference` varchar(200) DEFAULT NULL,
  `created_by` bigint DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_exp_pharmacy` (`pharmacy_id`),
  KEY `idx_exp_category` (`category_id`),
  KEY `idx_exp_date` (`expense_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- CLINICAL TABLES
-- ============================================================

CREATE TABLE IF NOT EXISTS `patients` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `first_name` varchar(100) NOT NULL,
  `last_name` varchar(100) DEFAULT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `email` varchar(200) DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  `gender` varchar(20) DEFAULT NULL,
  `address` text,
  `nhif_number` varchar(50) DEFAULT NULL,
  `is_active` tinyint DEFAULT 1,
  `is_deleted` tinyint DEFAULT 0,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_pat_pharmacy` (`pharmacy_id`),
  KEY `idx_pat_phone` (`phone`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `patient_allergies` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `patient_id` bigint NOT NULL,
  `allergen` varchar(200) NOT NULL,
  `severity` varchar(50) DEFAULT 'Mild',
  `notes` text,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_pa_patient` (`patient_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `patient_conditions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `patient_id` bigint NOT NULL,
  `condition_name` varchar(200) NOT NULL,
  `diagnosed_date` date DEFAULT NULL,
  `notes` text,
  `is_active` tinyint DEFAULT 1,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_pc_patient` (`patient_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `prescriptions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `patient_id` bigint NOT NULL,
  `prescription_number` varchar(50) NOT NULL,
  `doctor_name` varchar(200) DEFAULT NULL,
  `prescription_date` date DEFAULT NULL,
  `notes` text,
  `status` varchar(20) DEFAULT 'Pending',
  `created_by` bigint DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_rx_number` (`prescription_number`),
  KEY `idx_rx_pharmacy` (`pharmacy_id`),
  KEY `idx_rx_patient` (`patient_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `prescription_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `prescription_id` bigint NOT NULL,
  `product_id` bigint DEFAULT NULL,
  `medication_name` varchar(300) NOT NULL,
  `dosage` varchar(100) DEFAULT NULL,
  `frequency` varchar(100) DEFAULT NULL,
  `duration` varchar(100) DEFAULT NULL,
  `quantity` int DEFAULT 0,
  `notes` text,
  PRIMARY KEY (`id`),
  KEY `idx_rxi_prescription` (`prescription_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- DDA (CONTROLLED DRUGS) TABLE
-- ============================================================

CREATE TABLE IF NOT EXISTS `dda_register` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `batch_id` bigint DEFAULT NULL,
  `entry_type` varchar(50) NOT NULL,
  `quantity` int NOT NULL,
  `reference_number` varchar(100) DEFAULT NULL,
  `patient_name` varchar(200) DEFAULT NULL,
  `prescriber_name` varchar(200) DEFAULT NULL,
  `balance_after` int DEFAULT 0,
  `recorded_by` bigint DEFAULT NULL,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_dda_pharmacy` (`pharmacy_id`),
  KEY `idx_dda_product` (`product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- SETTINGS TABLE
-- ============================================================

CREATE TABLE IF NOT EXISTS `pharmacy_settings` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `setting_key` varchar(100) NOT NULL,
  `setting_value` text,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_setting` (`pharmacy_id`, `setting_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- NOTIFICATIONS TABLE
-- ============================================================

CREATE TABLE IF NOT EXISTS `notifications` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `user_id` bigint DEFAULT NULL,
  `title` varchar(200) NOT NULL,
  `message` text,
  `notification_type` varchar(50) DEFAULT 'Info',
  `is_read` tinyint DEFAULT 0,
  `is_deleted` tinyint DEFAULT 0,
  `created_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_notif_pharmacy` (`pharmacy_id`),
  KEY `idx_notif_user` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
