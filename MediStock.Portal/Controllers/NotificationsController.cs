// ============================================================
//  MediStock.Portal — NotificationsController
//  Routes:
//    GET  /Notifications/GetNotifications → JSON notifications list
//    POST /Notifications/Dismiss          → dismiss a notification
//    GET  /Notifications/GetCount         → JSON unread count
//    POST /Notifications/MarkAllRead      → mark all as read
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public NotificationsController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/notifications?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/notifications/count?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new { count = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { count = 0, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dismiss([FromBody] DismissRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/notifications/dismiss", new { id = model.id });
            return Json(result.IsSuccess
                ? new { success = true, message = "Notification dismissed" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to dismiss" : result.Error });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var result = await _api.PostAsync<object>("api/notifications/markallread", new
            {
                pharmacy_id = GetPharmacyId()
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "All notifications marked as read" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class DismissRequest { public long id { get; set; } }
    }
}
