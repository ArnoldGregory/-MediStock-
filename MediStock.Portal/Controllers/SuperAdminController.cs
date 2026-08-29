// ============================================================
//  MediStock.Portal — SuperAdminController (Riziki BFF pattern)
//  Platform-wide administration, SuperAdmin (role_id=1) only.
//  Routes:
//    GET  /SuperAdmin/Index           → platform overview dashboard view
//    GET  /SuperAdmin/Pharmacies      → all pharmacies view
//    GET  /SuperAdmin/Users           → all users view
//    GET  /SuperAdmin/Audit           → platform audit trail view
//    GET  /SuperAdmin/GetPharmacies   → JSON proxy → api/superadmin/pharmacies
//    GET  /SuperAdmin/GetUsers        → JSON proxy → api/superadmin/users
//    GET  /SuperAdmin/GetStats        → JSON proxy → api/superadmin/stats
//    GET  /SuperAdmin/GetAudit        → JSON proxy → api/superadmin/audit
//    POST /SuperAdmin/AddPharmacy     → proxy → api/superadmin/addpharmacy
//    POST /SuperAdmin/UpdatePharmacyStatus → proxy → api/superadmin/updatepharmacystatus
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class SuperAdminController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public SuperAdminController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");
            await _audit.LogViewAsync("SuperAdmin/Index");
            return View();
        }

        public async Task<IActionResult> Pharmacies()
        {
            if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");
            await _audit.LogViewAsync("SuperAdmin/Pharmacies");
            return View();
        }

        public async Task<IActionResult> Users()
        {
            if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");
            await _audit.LogViewAsync("SuperAdmin/Users");
            return View();
        }

        public async Task<IActionResult> Audit()
        {
            if (!IsSuperAdmin()) return RedirectToAction("AccessDenied", "Account");
            await _audit.LogViewAsync("SuperAdmin/Audit");
            return View();
        }

        // ── Data proxies ──────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPharmacies()
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });
            var result = await _api.GetAsync<object>("api/superadmin/pharmacies");
            return Json(result.IsSuccess ? result.Data : new List<object>());
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });
            var result = await _api.GetAsync<object>("api/superadmin/users");
            return Json(result.IsSuccess ? result.Data : new List<object>());
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });
            var result = await _api.GetAsync<object>("api/superadmin/stats");
            return Json(result.IsSuccess ? result.Data : new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetAudit()
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });
            var result = await _api.GetAsync<object>("api/superadmin/audit?limit=100");
            return Json(result.IsSuccess ? result.Data : new List<object>());
        }

        [HttpPost]
        public async Task<IActionResult> AddPharmacy([FromBody] AddPharmacyRequest model)
        {
            if (!IsSuperAdmin())
                return Json(new { success = false, message = "Unauthorized" });
            if (model == null || string.IsNullOrWhiteSpace(model.name) || string.IsNullOrWhiteSpace(model.owner_email) || string.IsNullOrWhiteSpace(model.password))
                return Json(new { success = false, message = "Pharmacy name, owner email and password are required" });

            var result = await _api.PostAsync<object>("api/superadmin/addpharmacy", new
            {
                name = model.name,
                slug = model.slug,
                phone = model.phone,
                email = model.email,
                address = model.address,
                license_number = model.license_number,
                currency = model.currency,
                owner_first_name = model.owner_first_name,
                owner_last_name = model.owner_last_name,
                owner_email = model.owner_email,
                owner_mobile = model.owner_mobile,
                password = model.password
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Pharmacy created. Owner can now log in." }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to create pharmacy" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePharmacyStatus([FromBody] UpdatePharmacyStatusRequest model)
        {
            if (!IsSuperAdmin())
                return Json(new { success = false, message = "Unauthorized" });
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/superadmin/updatepharmacystatus", new
            {
                id = model.id,
                is_active = model.is_active
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Pharmacy status updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update pharmacy status" : result.Error });
        }

        private bool IsSuperAdmin()
        {
            var roleId = User.Claims.FirstOrDefault(c => c.Type == "profile_id")?.Value ?? "0";
            return roleId == "1";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class AddPharmacyRequest
        {
            public string? name { get; set; }
            public string? slug { get; set; }
            public string? phone { get; set; }
            public string? email { get; set; }
            public string? address { get; set; }
            public string? license_number { get; set; }
            public string? currency { get; set; }
            public string? owner_first_name { get; set; }
            public string? owner_last_name { get; set; }
            public string? owner_email { get; set; }
            public string? owner_mobile { get; set; }
            public string? password { get; set; }
        }

        public class UpdatePharmacyStatusRequest
        {
            public long id { get; set; }
            public bool is_active { get; set; }
        }
    }
}
