// ============================================================
//  MediStock.Portal — SettingsController
//  Routes:
//    GET  /Settings/Profile         → profile view
//    GET  /Settings/Pharmacy        → pharmacy settings view
//    GET  /Settings/GetProfile      → JSON current user profile
//    POST /Settings/UpdateProfile   → proxy → api/settings/updateprofile
//    GET  /Settings/GetPharmacy     → JSON pharmacy settings
//    POST /Settings/UpdatePharmacy  → proxy → api/settings/updatepharmacy
//    GET  /Settings/GetUsers        → JSON pharmacy users (admin)
//    POST /Settings/AddUser         → proxy → api/settings/adduser
//    POST /Settings/DeleteUser      → proxy → api/settings/deleteuser
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
        public async Task<IActionResult> Profile()
        {
            await _audit.LogViewAsync("Settings/Profile");
            return View();
        }

        public async Task<IActionResult> Pharmacy()
        {
            await _audit.LogViewAsync("Settings/Pharmacy");
            return View();
        }

        // ── Profile data ──────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/settings/profile");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/settings/updateprofile", new
            {
                first_name = model.first_name,
                last_name  = model.last_name,
                phone      = model.phone,
                avatar     = model.avatar
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Profile updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update profile" : result.Error });
        }

        // ── Pharmacy data ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPharmacy()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/settings/pharmacy?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : null);
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

            var result = await _api.PostAsync<object>("api/settings/updatepharmacy", new
            {
                pharmacy_id     = GetPharmacyId(),
                pharmacy_name   = model.pharmacy_name,
                pharmacy_phone  = model.pharmacy_phone,
                pharmacy_email  = model.pharmacy_email,
                pharmacy_address = model.pharmacy_address,
                license_number  = model.license_number,
                kra_pin         = model.kra_pin
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Pharmacy settings updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update pharmacy" : result.Error });
        }

        // ── Users data (admin only) ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/settings/users?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/settings/adduser", new
            {
                pharmacy_id  = GetPharmacyId(),
                first_name   = model.first_name,
                last_name    = model.last_name,
                email        = model.email,
                phone        = model.phone,
                role_id      = model.role_id,
                password     = model.password
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "User added" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add user" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/settings/deleteuser", new { id = model.id });
            return Json(result.IsSuccess
                ? new { success = true, message = "User deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete user" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class UpdateProfileRequest
        {
            public string? first_name { get; set; }
            public string? last_name  { get; set; }
            public string? phone      { get; set; }
            public string? avatar     { get; set; }
        }

        public class UpdatePharmacyRequest
        {
            public string? pharmacy_name   { get; set; }
            public string? pharmacy_phone  { get; set; }
            public string? pharmacy_email  { get; set; }
            public string? pharmacy_address { get; set; }
            public string? license_number  { get; set; }
            public string? kra_pin         { get; set; }
        }

        public class AddUserRequest
        {
            public string? first_name { get; set; }
            public string? last_name  { get; set; }
            public string? email      { get; set; }
            public string? phone      { get; set; }
            public long    role_id    { get; set; }
            public string? password   { get; set; }
        }
    }
}
