namespace MediStock.API.Models
{
    // ── Pharmacy / Tenant ──
    public class PharmacyModel
    {
        public Int64 id { get; set; }
        public string name { get; set; } = "";
        public string slug { get; set; } = "";
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? license_number { get; set; }
        public string? license_no { get; set; }
        public string? owner_name { get; set; }
        public DateTime? license_expiry { get; set; }
        public string currency { get; set; } = "KES";
        public string? vat_number { get; set; }
        public string? receipt_footer { get; set; }
        public string subscription_plan { get; set; } = "Starter";
        public DateTime? subscription_expiry { get; set; }
        public bool is_active { get; set; } = true;
        public Int64 created_by { get; set; }
    }

    // ── Users ──
    public class PharmacyUserModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public int role_id { get; set; } = 3;
        public string? first_name { get; set; }
        public string? middle_name { get; set; }
        public string? last_name { get; set; }
        public string email { get; set; } = "";
        public string? mobile { get; set; }
        public string? phone { get; set; }
        public string password { get; set; } = "";
        public string? avatar { get; set; }
        public bool is_active { get; set; } = true;
        public bool locked { get; set; }
        public bool change_password { get; set; }
        public int failed_login_attempts { get; set; }
        public bool google_authenticate { get; set; }
        public string? sec_key { get; set; }
        public Int64 created_by { get; set; }
    }

    // ── Product Category ──
    public class ProductCategoryModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public string name { get; set; } = "";
        public string? description { get; set; }
        public bool is_active { get; set; } = true;
        public Int64 created_by { get; set; }
    }

    // ── Product ──
    public class ProductModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public Int64? category_id { get; set; }
        public string name { get; set; } = "";
        public string? sku { get; set; }
        public string? barcode { get; set; }
        public string? description { get; set; }
        public decimal cost_price { get; set; }
        public decimal selling_price { get; set; }
        public decimal vat_rate { get; set; } = 16.00m;
        public int reorder_level { get; set; }
        public int stock_qty { get; set; }
        public string unit { get; set; } = "pcs";
        public string? unit_of_measure { get; set; }
        public bool is_controlled_drug { get; set; }
        public bool is_active { get; set; } = true;
        public Int64 created_by { get; set; }
    }

    // ── Product Batch ──
    public class ProductBatchModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public Int64 product_id { get; set; }
        public string batch_number { get; set; } = "";
        public DateTime expiry_date { get; set; }
        public decimal cost_price { get; set; }
        public int quantity { get; set; }
        public int quantity_sold { get; set; }
        public string status { get; set; } = "Active";
        public Int64 created_by { get; set; }
    }

    // ── Customer ──
    public class CustomerModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public string customer_type { get; set; } = "Retail";
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public DateTime? date_of_birth { get; set; }
        public string? gender { get; set; }
        public decimal credit_limit { get; set; }
        public decimal outstanding_balance { get; set; }
        public string payment_terms { get; set; } = "Cash";
        public bool is_active { get; set; } = true;
        public Int64 created_by { get; set; }
    }

    // ── Supplier ──
    public class SupplierModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public string name { get; set; } = "";
        public string? contact_person { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public string? country { get; set; }
        public bool is_active { get; set; } = true;
        public Int64 created_by { get; set; }
    }

    // ── Sale ──
    public class SaleModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public Int64? customer_id { get; set; }
        public Int64 user_id { get; set; }
        public string sale_number { get; set; } = "";
        public string sale_type { get; set; } = "Retail";
        public decimal subtotal { get; set; }
        public decimal total_amount { get; set; }
        public decimal vat_amount { get; set; }
        public decimal tax { get; set; }
        public decimal discount { get; set; }
        public decimal total { get; set; }
        public decimal net_amount { get; set; }
        public decimal amount_paid { get; set; }
        public string payment_method { get; set; } = "Cash";
        public string? payment_reference { get; set; }
        public string? notes { get; set; }
        public string status { get; set; } = "Completed";
        public Int64 sold_by { get; set; }
        public string sale_mode { get; set; } = "POS";
        public Int64? prescription_id { get; set; }
        public Int64? dispensed_by { get; set; }
        public List<SaleItemModel>? items { get; set; }
    }

    // ── Sale Item ──
    public class SaleItemModel
    {
        public Int64 id { get; set; }
        public Int64 sale_id { get; set; }
        public Int64 product_id { get; set; }
        public Int64? batch_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        public decimal cost_price { get; set; }
        public decimal vat_rate { get; set; }
        public decimal vat_amount { get; set; }
        public decimal discount { get; set; }
        public decimal total { get; set; }
    }

    // ── Purchase Order ──
    public class PurchaseOrderModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public Int64 supplier_id { get; set; }
        public Int64 product_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_cost { get; set; }
        public decimal total_cost { get; set; }
        public string po_number { get; set; } = "";
        public string status { get; set; } = "Pending";
        public decimal total { get; set; }
        public DateTime? expected_date { get; set; }
        public string? notes { get; set; }
        public Int64 created_by { get; set; }
    }

    // ── PO Item ──
    public class POItemModel
    {
        public Int64 id { get; set; }
        public Int64 po_id { get; set; }
        public Int64 product_id { get; set; }
        public int quantity { get; set; }
        public int received_qty { get; set; }
        public decimal unit_cost { get; set; }
        public decimal total { get; set; }
    }

    // ── Receive Stock ──
    public class ReceiveStockModel
    {
        public Int64 pharmacy_id { get; set; }
        public Int64 received_by { get; set; }
        public int quantity_received { get; set; }
        public string? notes { get; set; }
        public List<ReceiveStockItemModel> items { get; set; } = new();
    }

    public class ReceiveStockItemModel
    {
        public Int64 product_id { get; set; }
        public string batch_number { get; set; } = "";
        public DateTime expiry_date { get; set; }
        public decimal unit_cost { get; set; }
        public int quantity { get; set; }
    }

    // ── Expense Category ──
    public class ExpenseCategoryModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public string name { get; set; } = "";
        public bool is_active { get; set; } = true;
    }

    // ── Expense ──
    public class ExpenseModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public Int64? category_id { get; set; }
        public string? category { get; set; }
        public string description { get; set; } = "";
        public decimal amount { get; set; }
        public DateTime? expense_date { get; set; }
        public string payment_method { get; set; } = "Cash";
        public string? reference { get; set; }
        public string? notes { get; set; }
        public Int64 created_by { get; set; }
    }

    // ── Patient ──
    public class PatientModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public string first_name { get; set; } = "";
        public string? last_name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public DateTime? date_of_birth { get; set; }
        public string? gender { get; set; }
        public string? address { get; set; }
        public string? nhif_number { get; set; }
        public string? allergies { get; set; }
        public string? medical_history { get; set; }
        public bool is_active { get; set; } = true;
        public Int64 created_by { get; set; }
    }

    // ── Prescription ──
    public class PrescriptionModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public Int64 patient_id { get; set; }
        public string prescription_number { get; set; } = "";
        public string? doctor_name { get; set; }
        public string? hospital { get; set; }
        public DateTime? prescription_date { get; set; }
        public string? notes { get; set; }
        public string status { get; set; } = "Pending";
        public List<PrescriptionItemModel> items { get; set; } = new();
        public Int64 created_by { get; set; }
    }

    // ── Prescription Item ──
    public class PrescriptionItemModel
    {
        public Int64 id { get; set; }
        public Int64 prescription_id { get; set; }
        public Int64? product_id { get; set; }
        public string medication_name { get; set; } = "";
        public string? dosage { get; set; }
        public string? frequency { get; set; }
        public string? duration { get; set; }
        public int quantity { get; set; }
        public string? notes { get; set; }
    }

    // ── DDA Register ──
    public class DDAModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public Int64? patient_id { get; set; }
        public Int64? prescription_id { get; set; }
        public Int64 product_id { get; set; }
        public Int64? batch_id { get; set; }
        public string entry_type { get; set; } = "";
        public int quantity { get; set; }
        public DateTime? dispensed_date { get; set; }
        public string? reference_number { get; set; }
        public string? patient_name { get; set; }
        public string? prescriber_name { get; set; }
        public string? notes { get; set; }
        public int balance_after { get; set; }
        public Int64 created_by { get; set; }
        public Int64 recorded_by { get; set; }
    }

    // ── Stock Adjustment ──
    public class StockAdjustmentModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public Int64 product_id { get; set; }
        public Int64? batch_id { get; set; }
        public string adjustment_type { get; set; } = "";
        public int quantity { get; set; }
        public string? reason { get; set; }
        public Int64 adjusted_by { get; set; }
    }

    // ── Stock Take ──
    public class StockTakeSessionModel
    {
        public Int64 id { get; set; }
        public Int64 pharmacy_id { get; set; }
        public string session_name { get; set; } = "";
        public string status { get; set; } = "Open";
        public Int64 started_by { get; set; }
    }

    public class StockTakeItemModel
    {
        public Int64 id { get; set; }
        public Int64 session_id { get; set; }
        public Int64 product_id { get; set; }
        public Int64? batch_id { get; set; }
        public int system_qty { get; set; }
        public int counted_qty { get; set; }
        public int variance { get; set; }
        public string? notes { get; set; }
    }

    // ── Dashboard ──
    public class DashboardSummary
    {
        public int total_products { get; set; }
        public int total_customers { get; set; }
        public int total_suppliers { get; set; }
        public decimal today_sales { get; set; }
        public decimal month_sales { get; set; }
        public decimal month_expenses { get; set; }
        public int low_stock_count { get; set; }
        public int expiring_soon_count { get; set; }
        public decimal total_inventory_value { get; set; }
    }

    // ── Audit Trail ──
    public class AuditTrailModel
    {
        public string user_name { get; set; } = "";
        public string action_type { get; set; } = "";
        public string action_description { get; set; } = "";
        public string page_accessed { get; set; } = "";
        public string client_ip_address { get; set; } = "";
        public string session_id { get; set; } = "";
        public DateTime created_on { get; set; } = DateTime.UtcNow;
    }

    // ── API Response Wrapper ──
    public class ApiResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public string? action { get; set; }
        public T? data { get; set; }
    }

    // ── Pagination ──
    public class PaginatedResult<T>
    {
        public List<T> items { get; set; } = new();
        public int total_count { get; set; }
        public int page { get; set; }
        public int page_size { get; set; }
    }

    // ── Invoice Import ──
    public class ImportConfirmRequest
    {
        public Int64 supplier_id { get; set; }
        public string? po_number { get; set; }
        public decimal markup_percent { get; set; } = 25m;
        public List<ImportConfirmLineModel>? lines { get; set; }
    }

    public class ImportConfirmLineModel
    {
        public string product_name { get; set; } = "";
        public int quantity { get; set; }
        public decimal unit_cost { get; set; }
        public decimal? unit_sell_price { get; set; }
        public string? expiry_date { get; set; }
        public bool skip { get; set; }
    }
}
