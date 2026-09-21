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

        // ── GET /Dashboard/StartHere ──────────────────────────────────────────
        // Guided, novice-friendly journey: back office setup → stocking → selling.
        public async Task<IActionResult> StartHere()
        {
            await _audit.LogViewAsync("Dashboard/StartHere");

            var roleId = User.FindFirst("profile_id")?.Value ?? "";
            var name = User.FindFirstValue(ClaimTypes.Name) ?? "";

            int role = int.TryParse(roleId, out var r) ? r : 0;
            ViewBag.Name = name;
            ViewBag.RoleId = roleId;
            ViewBag.IsAdmin = role == 1 || role == 2;
            ViewBag.IsSuperAdmin = role == 1;
            ViewBag.IsPharmacist = role == 3;
            ViewBag.IsStaff = role >= 4;
            ViewBag.CanSell = role is 2 or 3 or 4 or 5;
            ViewBag.CanStock = role is 1 or 2 or 3 or 4;

            ViewData["Title"] = "Start Here — Guided Tour";
            return View();
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
