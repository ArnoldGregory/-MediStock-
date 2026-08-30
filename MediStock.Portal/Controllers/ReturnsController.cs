// ============================================================
//  MediStock.Portal — ReturnsController
//  Routes:
//    GET  /Sales/Returns         → sales returns view
//    GET  /Returns/GetReturns    → JSON returns list
//    GET  /Returns/GetReturnable → JSON returnable items for a sale
//    GET  /Returns/GetReturnDetail → JSON line items of a return
//    POST /Returns/CreateReturn  → proxy → api/returns
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class ReturnsController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public ReturnsController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            await _audit.LogViewAsync("Sales/Returns");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetReturns()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/returns");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReturnable(long sale_id)
        {
            if (sale_id <= 0) return Json(new List<object>());
            try
            {
                var result = await _api.GetAsync<object>($"api/returns/items?sale_id={sale_id}");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReturnDetail(long return_id)
        {
            if (return_id <= 0) return Json(new List<object>());
            try
            {
                var result = await _api.GetAsync<object>($"api/returns/detail?return_id={return_id}");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReturn([FromBody] CreateReturnRequest model)
        {
            if (model == null || model.sale_id <= 0 || model.items == null || model.items.Count == 0)
                return Json(new { success = false, message = "Select a sale and at least one item" });

            var result = await _api.PostAsync<object>("api/returns", new
            {
                sale_id     = model.sale_id,
                customer_id = model.customer_id,
                reason      = model.reason,
                items       = model.items
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Return recorded and stock restored", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to record return" : result.Error, data = (object?)null });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        public class CreateReturnRequest
        {
            public long sale_id { get; set; }
            public long? customer_id { get; set; }
            public string? reason { get; set; }
            public List<ReturnItemViewModel>? items { get; set; }
        }

        public class ReturnItemViewModel
        {
            public long sale_item_id { get; set; }
            public long product_id { get; set; }
            public long? batch_id { get; set; }
            public int quantity { get; set; }
            public decimal unit_price { get; set; }
            public decimal refund { get; set; }
        }
    }
}