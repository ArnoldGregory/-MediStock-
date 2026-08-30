// ============================================================
//  MediStock.Portal — ReportsController
//  Routes:
//    GET  /Reports/Sales            → sales report view
//    GET  /Reports/Stock            → stock report view
//    GET  /Reports/Financial        → financial report view
//    GET  /Reports/GetSalesReport   → JSON sales report data
//    GET  /Reports/GetStockReport   → JSON stock report data
//    GET  /Reports/GetFinancialReport → JSON financial report data
//    GET  /Reports/GetMargins       → JSON product margins
//    GET  /Reports/GetExpenseBreakdown → JSON expense by category
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public ReportsController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Sales()
        {
            await _audit.LogViewAsync("Reports/Sales");
            return View();
        }

        public async Task<IActionResult> Stock()
        {
            await _audit.LogViewAsync("Reports/Stock");
            return View();
        }

        public async Task<IActionResult> Financial()
        {
            await _audit.LogViewAsync("Reports/Financial");
            return View();
        }

        public async Task<IActionResult> StockPerformance()
        {
            await _audit.LogViewAsync("Reports/StockPerformance");
            return View();
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSalesReport(string? from_date, string? to_date)
        {
            try
            {
                var qs = "api/reports/sales?pharmacyId=" + GetPharmacyId();
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
        public async Task<IActionResult> GetStockReport()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/reports/stock?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFinancialReport(string? from_date, string? to_date)
        {
            try
            {
                var qs = "api/reports/financial?pharmacyId=" + GetPharmacyId();
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
        public async Task<IActionResult> GetMargins()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/reports/margins?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenseBreakdown(string? from_date, string? to_date)
        {
            try
            {
                var qs = "api/reports/expensebreakdown?pharmacyId=" + GetPharmacyId();
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
        public async Task<IActionResult> GetStockPerformance()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/reports/stock-performance?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string report, string? from_date = null, string? to_date = null)
        {
            try
            {
                var qs = $"api/reports/export?report={report}";
                if (!string.IsNullOrWhiteSpace(from_date)) qs += $"&from_date={Uri.EscapeDataString(from_date)}";
                if (!string.IsNullOrWhiteSpace(to_date)) qs += $"&to_date={Uri.EscapeDataString(to_date)}";

                var (bytes, contentType, fileName, error) = await _api.GetFileAsync(qs);
                if (bytes == null)
                    return Content(error ?? "Export failed");
                return File(bytes, contentType ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName ?? $"{report}_report.xlsx");
            }
            catch (Exception ex)
            {
                return Content("Export failed: " + ex.Message);
            }
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }
    }
}
