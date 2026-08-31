-- ============================================================
-- 27_pharmacy_config.sql
-- Pharmacy settings config table used by SettingsController.SavePharmacySetting
-- (POST /api/settings/config). Missing from earlier migrations -> added here.
-- ============================================================
CREATE TABLE IF NOT EXISTS `pharmacy_config` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `pharmacy_id` bigint NOT NULL,
  `config_key` varchar(100) NOT NULL,
  `config_value` text,
  `created_by` bigint DEFAULT NULL,
  `created_on` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_by` bigint DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_pharmacy_config` (`pharmacy_id`, `config_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;