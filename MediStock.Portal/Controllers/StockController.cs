// ============================================================
//  MediStock.Portal — StockController
//  Routes:
//    GET  /Stock/Batches          → batches view
//    GET  /Stock/Adjustments      → adjustments view
//    GET  /Stock/StockTake        → stock take view
//    GET  /Stock/GetBatches       → JSON batches
//    GET  /Stock/GetAdjustments   → JSON adjustments
//    GET  /Stock/GetStockTake     → JSON stock take items
//    POST /Stock/AddBatch         → proxy → api/stock/addbatch
//    POST /Stock/AddAdjustment    → proxy → api/stock/addadjustment
//    POST /Stock/SaveStockTake    → proxy → api/stock/savestocktake
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class StockController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public StockController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Batches()
        {
            await _audit.LogViewAsync("Stock/Batches");
            return View();
        }

        public async Task<IActionResult> Adjustments()
        {
            await _audit.LogViewAsync("Stock/Adjustments");
            return View();
        }

        public async Task<IActionResult> StockTake()
        {
            await _audit.LogViewAsync("Stock/StockTake");
            return View();
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetBatches()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/stock/batches?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAdjustments()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/stock/adjustments?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStockTake()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/stock/stocktake?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetExpiringItems(int days = 30)
        {
            try
            {
                var result = await _api.GetAsync<object>($"api/stock/expiring?pharmacyId={GetPharmacyId()}&days={days}");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBatch([FromBody] AddBatchRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/stock/addbatch", new
            {
                pharmacy_id   = GetPharmacyId(),
                product_id    = model.product_id,
                batch_number  = model.batch_number,
                quantity      = model.quantity,
                cost_price    = model.cost_price,
                expiry_date   = model.expiry_date,
                supplier_id   = model.supplier_id
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Batch added successfully" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add batch" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> AddAdjustment([FromBody] AdjustmentRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/stock/addadjustment", new
            {
                pharmacy_id  = GetPharmacyId(),
                product_id   = model.product_id,
                batch_id     = model.batch_id,
                quantity     = model.quantity,
                adjustment_type = model.adjustment_type,
                reason       = model.reason
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Adjustment recorded" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to record adjustment" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> SaveStockTake([FromBody] StockTakeRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/stock/savestocktake", new
            {
                pharmacy_id = GetPharmacyId(),
                items       = model.items
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Stock take saved" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to save stock take" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class AddBatchRequest
        {
            public long    product_id   { get; set; }
            public string? batch_number { get; set; }
            public int     quantity     { get; set; }
            public decimal cost_price   { get; set; }
            public string? expiry_date  { get; set; }
            public long    supplier_id  { get; set; }
        }

        public class AdjustmentRequest
        {
            public long    product_id      { get; set; }
            public long    batch_id        { get; set; }
            public int     quantity        { get; set; }
            public string? adjustment_type { get; set; }
            public string? reason          { get; set; }
        }

        public class StockTakeRequest
        {
            public List<object>? items { get; set; }
        }
    }
}
