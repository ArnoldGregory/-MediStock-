// ============================================================
//  MediStock.Portal — SettingsController
//  Routes:
//    GET  /Settings/Profile         → profile view
//    GET  /Settings/Pharmacy        → pharmacy settings view
//    GET  /Settings/GetPharmacy     → JSON pharmacy info (api/settings)
//    POST /Settings/UpdatePharmacy  → proxy → api/settings/profile
//    POST /Settings/SavePharmacyConfig → proxy → api/settings/config
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public SettingsController(ApiClient api, AuditService audit)
        {
            _api   = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            await _audit.LogViewAsync("Settings");
            ViewBag.FirstName = User.FindFirst("first_name")?.Value ?? "";
            ViewBag.LastName  = User.FindFirst("last_name")?.Value ?? "";
            ViewBag.Email     = User.Identity?.Name ?? "";
            ViewBag.Phone     = User.FindFirst("phone")?.Value ?? "";
            ViewBag.RoleId    = User.FindFirst("profile_id")?.Value ?? "";
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Pharmacy()
        {
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Setup()
        {
            await _audit.LogViewAsync("Settings/Setup");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSetupChecklist()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/setup/checklist");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // ── Pharmacy data ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPharmacy()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/settings/");
                if (result.IsSuccess && result.Data != null)
                    return Json(result.Data);
                return Json((object?)null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePharmacy([FromBody] UpdatePharmacyRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/settings/profile", new
            {
                name            = model.pharmacy_name,
                phone           = model.pharmacy_phone,
                email           = model.pharmacy_email,
                address         = model.pharmacy_address,
                license_number  = model.license_number,
                vat_number      = model.vat_number,
                receipt_footer  = model.receipt_footer,
                currency        = model.currency
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Pharmacy settings updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update pharmacy" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> SavePharmacyConfig([FromBody] SaveConfigRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.key))
                return Json(new { success = false, message = "key is required" });

            var result = await _api.PostAsync<object>("api/settings/config", new
            {
                key   = model.key,
                value = model.value ?? ""
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Setting saved" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to save setting" : result.Error });
        }

        // ── Request models ────────────────────────────────────────────────────
        public class UpdatePharmacyRequest
        {
            public string? pharmacy_name   { get; set; }
            public string? pharmacy_phone  { get; set; }
            public string? pharmacy_email  { get; set; }
            public string? pharmacy_address { get; set; }
            public string? license_number  { get; set; }
            public string? vat_number      { get; set; }
            public string? receipt_footer  { get; set; }
            public string? currency        { get; set; }
        }

        public class SaveConfigRequest
        {
            public string? key   { get; set; }
            public string? value { get; set; }
        }
    }
}