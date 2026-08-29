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

        // ── GET /Dashboard/PharmacistStats ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PharmacistStats()
        {
            try
            {
                var stockTask = _api.GetAsync<List<JsonElement>>("api/dashboard/stocksummary?pharmacyId=" + GetPharmacyId());
                var salesTask = _api.GetAsync<List<JsonElement>>("api/dashboard/salesstats?pharmacyId=" + GetPharmacyId());
                var expiryTask = _api.GetAsync<List<JsonElement>>("api/dashboard/expiringitems?pharmacyId=" + GetPharmacyId());
                var alertsTask = _api.GetAsync<List<JsonElement>>("api/dashboard/alerts?pharmacyId=" + GetPharmacyId());

                await Task.WhenAll(stockTask, salesTask, expiryTask, alertsTask);

                int totalProducts = stockTask.Result.IsSuccess && stockTask.Result.Data != null
                    ? stockTask.Result.Data.Count : 0;
                decimal todaySales = 0;
                if (salesTask.Result.IsSuccess && salesTask.Result.Data != null)
                {
                    foreach (var item in salesTask.Result.Data)
                    {
                        foreach (var prop in item.EnumerateObject())
                        {
                            if (prop.Name.Equals("today_total", StringComparison.OrdinalIgnoreCase))
                            {
                                if (prop.Value.TryGetDecimal(out decimal v)) todaySales = v;
                                break;
                            }
                        }
                    }
                }
                int expiringCount = expiryTask.Result.IsSuccess && expiryTask.Result.Data != null
                    ? expiryTask.Result.Data.Count : 0;
                int alertCount = alertsTask.Result.IsSuccess && alertsTask.Result.Data != null
                    ? alertsTask.Result.Data.Count : 0;

                return Json(new { totalProducts, todaySales, expiringCount, alertCount });
            }
            catch
            {
                return Json(new { totalProducts = 0, todaySales = 0m, expiringCount = 0, alertCount = 0 });
            }
        }

        // ── GET /Dashboard/ClerkStats ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ClerkStats()
        {
            try
            {
                var salesTask = _api.GetAsync<List<JsonElement>>("api/dashboard/mysales?pharmacyId=" + GetPharmacyId());
                var pendingTask = _api.GetAsync<List<JsonElement>>("api/dashboard/pendingorders?pharmacyId=" + GetPharmacyId());

                await Task.WhenAll(salesTask, pendingTask);

                int todaySales = salesTask.Result.IsSuccess && salesTask.Result.Data != null
                    ? salesTask.Result.Data.Count : 0;
                int pendingOrders = pendingTask.Result.IsSuccess && pendingTask.Result.Data != null
                    ? pendingTask.Result.Data.Count : 0;

                return Json(new { todaySales, pendingOrders });
            }
            catch
            {
                return Json(new { todaySales = 0, pendingOrders = 0 });
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
