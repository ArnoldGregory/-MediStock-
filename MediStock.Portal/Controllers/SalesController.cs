// ============================================================
//  MediStock.Portal — SalesController
//  Routes:
//    GET  /Sales/POS              → point-of-sale view
//    GET  /Sales/History          → sales history view
//    GET  /Sales/GetSales         → JSON sales list
//    GET  /Sales/GetSale?id=      → JSON single sale detail
//    POST /Sales/ProcessSale      → proxy → api/sales/processsale
//    POST /Sales/VoidSale         → proxy → api/sales/voidsale
//    GET  /Sales/GetSaleItems?id= → JSON sale line items
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public SalesController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> POS()
        {
            await _audit.LogViewAsync("Sales/POS");
            return View();
        }

        public async Task<IActionResult> History()
        {
            await _audit.LogViewAsync("Sales/History");
            return View();
        }

        public async Task<IActionResult> Receipt(long id)
        {
            if (id <= 0) return NotFound();

            var sale = await _api.GetAsync<List<SaleDto>>($"api/sales/{id}");
            var items = await _api.GetAsync<List<SaleItemDto>>($"api/sales/{id}/items");
            if (!sale.IsSuccess || sale.Data == null || sale.Data.Count == 0)
                return NotFound();

            return View(new ReceiptViewModel
            {
                Sale = sale.Data[0],
                Items = items.IsSuccess ? (items.Data ?? new List<SaleItemDto>()) : new List<SaleItemDto>()
            });
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSales(string? from_date, string? to_date)
        {
            try
            {
                var qs = "api/sales?pharmacyId=" + GetPharmacyId();
                if (!string.IsNullOrWhiteSpace(from_date)) qs += $"&from_date={from_date}";
                if (!string.IsNullOrWhiteSpace(to_date)) qs += $"&to_date={to_date}";
                var result = await _api.GetAsync<object>(qs);
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSale(long id)
        {
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/sales/{id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSaleItems(long sale_id)
        {
            if (sale_id <= 0) return Json(new List<object>());
            try
            {
                var result = await _api.GetAsync<object>($"api/sales/{sale_id}/items");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPOSProducts()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/sales/products");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/customers");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTodaySummary()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/dashboard/salesstats");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessSale([FromBody] ProcessSaleRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/sales", new
            {
                pharmacy_id     = GetPharmacyId(),
                customer_id     = model.customer_id,
                sale_type       = model.sale_type,
                subtotal        = model.subtotal,
                total_amount    = model.subtotal,
                discount        = model.discount,
                tax             = model.tax,
                net_amount      = model.net_amount,
                amount_paid     = model.amount_paid,
                payment_method  = model.payment_method,
                payment_reference = model.payment_reference,
                notes           = model.notes,
                items           = model.items
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Sale processed", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to process sale" : result.Error, data = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> VoidSale([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/sales/voidsale", new { id = model.id, pharmacy_id = GetPharmacyId() });
            return Json(result.IsSuccess
                ? new { success = true, message = "Sale voided" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to void sale" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class ProcessSaleRequest
        {
            public long?          customer_id       { get; set; }
            public string?        sale_type         { get; set; }
            public List<object>?  items             { get; set; }
            public decimal        subtotal          { get; set; }
            public decimal        discount          { get; set; }
            public decimal        tax               { get; set; }
            public decimal        net_amount        { get; set; }
            public decimal        amount_paid       { get; set; }
            public string?        payment_method    { get; set; }
            public string?        payment_reference { get; set; }
            public string?        notes             { get; set; }
        }
    }

    public class ReceiptViewModel
    {
        public SaleDto? Sale { get; set; }
        public List<SaleItemDto> Items { get; set; } = new();
    }

    public class SaleDto
    {
        public long id { get; set; }
        public string? sale_number { get; set; }
        public string? sale_type { get; set; }
        public decimal subtotal { get; set; }
        public decimal discount { get; set; }
        public decimal total { get; set; }
        public decimal amount_paid { get; set; }
        public string? payment_method { get; set; }
        public string? status { get; set; }
        public string? customer_name { get; set; }
        public DateTime created_on { get; set; }
    }

    public class SaleItemDto
    {
        public long id { get; set; }
        public long sale_id { get; set; }
        public long product_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        public decimal discount { get; set; }
        public decimal total { get; set; }
        public string? product_name { get; set; }
    }
}
