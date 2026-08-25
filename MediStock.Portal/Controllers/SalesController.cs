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
                var result = await _api.GetAsync<object>($"api/sales/getsale?id={id}");
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
                var result = await _api.GetAsync<object>($"api/sales/saleitems?sale_id={sale_id}");
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
                var result = await _api.GetAsync<object>("api/sales/todaysummary?pharmacyId=" + GetPharmacyId());
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

            var result = await _api.PostAsync<object>("api/sales/processsale", new
            {
                pharmacy_id   = GetPharmacyId(),
                customer_id   = model.customer_id,
                items         = model.items,
                payment_method = model.payment_method,
                amount_paid   = model.amount_paid,
                notes         = model.notes
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
            public long?          customer_id    { get; set; }
            public List<object>?  items          { get; set; }
            public string?        payment_method { get; set; }
            public decimal        amount_paid    { get; set; }
            public string?        notes          { get; set; }
        }
    }
}
