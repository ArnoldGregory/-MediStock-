// ============================================================
//  MediStock.Portal — DDAController
//  Drug and Drug Authority (DDA) compliance reporting.
//  Routes:
//    GET  /DDA/Register        → DDA register view
//    GET  /DDA/Report          → DDA report view
//    GET  /DDA/GetRegister     → JSON DDA register entries
//    GET  /DDA/GetReport       → JSON DDA report data
//    POST /DDA/AddEntry        → proxy → api/dda/addentry
//    POST /DDA/UpdateEntry     → proxy → api/dda/updateentry
//    GET  /DDA/DownloadReport  → Excel export DDA report
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
        public async Task<IActionResult> GetRegister(string? search)
        {
            try
            {
                var qs = "api/dda/register?pharmacyId=" + GetPharmacyId();
                if (!string.IsNullOrWhiteSpace(search)) qs += $"&search={search}";
                var result = await _api.GetAsync<object>(qs);
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEntry(long id)
        {
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/dda/getentry?id={id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReport(string? from_date, string? to_date)
        {
            try
            {
                var qs = "api/dda/report?pharmacyId=" + GetPharmacyId();
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

        [HttpPost]
        public async Task<IActionResult> AddEntry([FromBody] AddDdaEntryRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/dda/addentry", new
            {
                pharmacy_id    = GetPharmacyId(),
                product_id     = model.product_id,
                dda_number     = model.dda_number,
                drug_name      = model.drug_name,
                strength       = model.strength,
                form           = model.form,
                schedule       = model.schedule,
                manufacturer   = model.manufacturer,
                expiry_date    = model.expiry_date
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "DDA entry added" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add DDA entry" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEntry([FromBody] UpdateDdaEntryRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/dda/updateentry", new
            {
                id             = model.id,
                dda_number     = model.dda_number,
                drug_name      = model.drug_name,
                strength       = model.strength,
                form           = model.form,
                schedule       = model.schedule,
                manufacturer   = model.manufacturer,
                expiry_date    = model.expiry_date
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "DDA entry updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update DDA entry" : result.Error });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReport(string? from_date, string? to_date)
        {
            var qs = $"api/dda/report/excel?pharmacyId={GetPharmacyId()}";
            if (!string.IsNullOrWhiteSpace(from_date)) qs += $"&from_date={from_date}";
            if (!string.IsNullOrWhiteSpace(to_date)) qs += $"&to_date={to_date}";

            var (bytes, contentType, fileName, error) = await _api.GetFileAsync(qs);
            if (bytes == null) return BadRequest(error ?? "Failed to generate DDA report");
            return File(bytes, contentType ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName ?? "dda_report.xlsx");
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class AddDdaEntryRequest
        {
            public long?   product_id  { get; set; }
            public string? dda_number  { get; set; }
            public string? drug_name   { get; set; }
            public string? strength    { get; set; }
            public string? form        { get; set; }
            public string? schedule    { get; set; }
            public string? manufacturer { get; set; }
            public string? expiry_date { get; set; }
        }

        public class UpdateDdaEntryRequest
        {
            public long    id           { get; set; }
            public string? dda_number   { get; set; }
            public string? drug_name    { get; set; }
            public string? strength     { get; set; }
            public string? form         { get; set; }
            public string? schedule     { get; set; }
            public string? manufacturer { get; set; }
            public string? expiry_date  { get; set; }
        }
    }
}
