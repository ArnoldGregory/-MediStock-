-- ============================================================
-- 30_dispensing_sale_mode.sql
-- Extend the sales table to support a "dispensing-first" model
-- while keeping the fast walk-in shop flow.
--   - sale_mode        ('POS' = quick walk-in sale, 'DISPENSE' = prescription-linked dispensing)
--   - prescription_id  (FK-ish link to prescriptions when DISPENSE mode)
--   - dispensed_by     (the dispensing pharmacist/user)
-- Default stays 'POS' so the existing shop flow is unchanged.
-- ============================================================

ALTER TABLE sales
    ADD COLUMN sale_mode       VARCHAR(20) NOT NULL DEFAULT 'POS' AFTER sold_by,
    ADD COLUMN prescription_id BIGINT      NULL DEFAULT NULL AFTER sale_mode,
    ADD COLUMN dispensed_by    BIGINT      NULL DEFAULT NULL AFTER prescription_id;

-- Index for lookups by dispensing mode / prescription.
ALTER TABLE sales
    ADD INDEX idx_sales_sale_mode (sale_mode),
    ADD INDEX idx_sales_prescription (prescription_id);
