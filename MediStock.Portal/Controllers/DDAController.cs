// ============================================================
//  MediStock.Portal — DDAController
//  Drug and Drug Authority (DDA) compliance register.
//  Routes:
//    GET  /DDA/Register        → DDA register view
//    GET  /DDA/Report          → DDA report view (client-side filter)
//    GET  /DDA/GetRegister     → JSON DDA register entries
//    POST /DDA/AddEntry        → proxy → api/dda
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class DDAController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public DDAController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Register()
        {
            await _audit.LogViewAsync("DDA/Register");
            return View();
        }

        public async Task<IActionResult> Report()
        {
            await _audit.LogViewAsync("DDA/Report");
            return View();
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetRegister()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/dda?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEntry([FromBody] AddDdaEntryRequest model)
        {
            if (model == null || model.product_id <= 0)
                return Json(new { success = false, message = "Product is required" });
            if (model.quantity <= 0)
                return Json(new { success = false, message = "Quantity must be greater than 0" });

            var result = await _api.PostAsync<object>("api/dda", new
            {
                product_id     = model.product_id,
                patient_id     = model.patient_id,
                quantity       = model.quantity,
                dispensed_date = model.dispensed_date,
                notes          = model.notes
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "DDA entry added" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add DDA entry" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class AddDdaEntryRequest
        {
            public long?   product_id     { get; set; }
            public long?   patient_id     { get; set; }
            public int     quantity       { get; set; }
            public string? dispensed_date { get; set; }
            public string? notes          { get; set; }
        }
    }
}