// ============================================================
//  MediStock.Portal — DashboardController
//  Place in: Controllers/DashboardController.cs
//  Role-based dashboard: renders different views based on role_id.
//  1=ADMIN, 2=PHARMACIST, 3=CLERK.
// ============================================================

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AuditService _audit;
        private readonly ApiClient _api;

        public DashboardController(AuditService audit, ApiClient api)
        {
            _audit = audit;
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            await _audit.LogViewAsync("Dashboard");

            var roleId = User.FindFirst("profile_id")?.Value ?? "";
            var name = User.FindFirstValue(ClaimTypes.Name) ?? "";
            var pharmacy = User.FindFirst("pharmacy_id")?.Value ?? "";

            ViewBag.Name = name;
            ViewBag.PharmacyId = pharmacy;
            ViewBag.RoleId = roleId;
            ViewBag.IsAdmin = roleId == "1" || roleId == "2";

            ViewData["Title"] = "Dashboard";
            return View("Index");
        }

        // ── GET /Dashboard/Summary ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/dashboard/summary?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch
            {
                return Json(null);
            }
        }

        // ── GET /Dashboard/ExpiringItems ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ExpiringItems()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/dashboard/expiringitems?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        // ── GET /Dashboard/Alerts ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Alerts()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/dashboard/alerts?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }
    }
}
