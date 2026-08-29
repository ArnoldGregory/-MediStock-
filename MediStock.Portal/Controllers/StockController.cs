// ============================================================
//  MediStock.Portal — StockController
//  Routes:
//    GET  /Stock/Batches          → batches view
//    GET  /Stock/Adjustments      → adjustments view
//    GET  /Stock/StockTake        → stock take view
//    GET  /Stock/GetBatches       → JSON batches
//    GET  /Stock/GetAdjustments   → JSON adjustments
//    GET  /Stock/GetStockTake     → JSON stock take sessions
//    GET  /Stock/GetExpiringItems → JSON expiring batches
//    POST /Stock/AddBatch         → proxy → api/stock/batches
//    POST /Stock/AddAdjustment    → proxy → api/stock/adjustments
//    POST /Stock/SaveStockTake    → proxy → api/stock/stocktake (session → items → commit)
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
        public async Task<IActionResult> GetExpiringItems()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/dashboard/expiringitems?pharmacyId=" + GetPharmacyId());
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
            try
            {
                if (model == null || model.product_id <= 0)
                    return Json(new { success = false, message = "Product is required" });
                if (string.IsNullOrEmpty(model.batch_number))
                    return Json(new { success = false, message = "Batch number is required" });

                var result = await _api.PostAsync<object>("api/stock/batches", new
                {
                    product_id   = model.product_id,
                    batch_number = model.batch_number,
                    expiry_date  = model.expiry_date,
                    cost_price   = model.cost_price,
                    quantity     = model.quantity
                });

                return Json(result.IsSuccess
                    ? new { success = true, message = "Batch added successfully" }
                    : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add batch" : result.Error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAdjustment([FromBody] AdjustmentRequest model)
        {
            try
            {
                if (model == null || model.product_id <= 0)
                    return Json(new { success = false, message = "Product is required" });
                if (string.IsNullOrEmpty(model.adjustment_type))
                    return Json(new { success = false, message = "Adjustment type is required" });

                var result = await _api.PostAsync<object>("api/stock/adjustments", new
                {
                    product_id      = model.product_id,
                    batch_id        = model.batch_id,
                    adjustment_type = model.adjustment_type,
                    quantity        = model.quantity,
                    reason          = model.reason
                });

                return Json(result.IsSuccess
                    ? new { success = true, message = "Adjustment recorded" }
                    : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to record adjustment" : result.Error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveStockTake([FromBody] StockTakeRequest model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.session_name))
                    return Json(new { success = false, message = "Session name is required" });
                if (model.items == null || model.items.Count == 0)
                    return Json(new { success = false, message = "No items entered" });

                var session = await _api.PostAsync<StockTakeSessionResponse>("api/stock/stocktake", new { session_name = model.session_name });
                if (!session.IsSuccess)
                    return Json(new { success = false, message = string.IsNullOrEmpty(session.Error) ? "Failed to create session" : session.Error });

                long sessionId = session.Data?.id ?? 0;
                if (sessionId <= 0)
                    return Json(new { success = false, message = "Failed to create session" });

                foreach (var item in model.items)
                {
                    int system = item.system_qty;
                    int counted = item.counted_qty;
                    var res = await _api.PostAsync<object>("api/stock/stocktake/items", new
                    {
                        session_id  = sessionId,
                        product_id  = item.product_id,
                        batch_id    = item.batch_id,
                        system_qty  = system,
                        counted_qty = counted,
                        notes       = item.notes
                    });
                    if (!res.IsSuccess)
                        return Json(new { success = false, message = "Item failed: " + (res.Error ?? "unknown error") });
                }

                var commit = await _api.PostAsync<object>("api/stock/stocktake/commit/" + sessionId, new { });
                return Json(commit.IsSuccess
                    ? new { success = true, message = "Stock take saved and committed" }
                    : new { success = false, message = string.IsNullOrEmpty(commit.Error) ? "Failed to commit stock take" : commit.Error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
        }

        public class AdjustmentRequest
        {
            public long    product_id      { get; set; }
            public long    batch_id        { get; set; }
            public int     quantity        { get; set; }
            public string? adjustment_type { get; set; }
            public string? reason          { get; set; }
        }

        public class StockTakeRequestItem
        {
            public long    product_id { get; set; }
            public long    batch_id   { get; set; }
            public int     system_qty { get; set; }
            public int     counted_qty { get; set; }
            public string? notes      { get; set; }
        }

        public class StockTakeRequest
        {
            public string?                    session_name { get; set; }
            public List<StockTakeRequestItem>? items       { get; set; }
        }

        public class StockTakeSessionResponse
        {
            public long id { get; set; }
        }
    }
}