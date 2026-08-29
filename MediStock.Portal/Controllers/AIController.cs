// ============================================================
//  MediStock.Portal — AIController
//  Routes:
//    GET  /AI/Index             → smart reorder view
//    POST /AI/PredictReorder    → proxy → POST api/ai/predict-reorder
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public AIController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            await _audit.LogViewAsync("AI/Index");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PredictReorder([FromBody] PredictRequest? model)
        {
            try
            {
                var result = await _api.PostAsync<object>("api/ai/predict-reorder", new
                {
                    lead_days = model?.lead_days ?? 7
                });

                return Json(result.IsSuccess
                    ? new { success = true, message = "Success", data = result.Data }
                    : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to generate forecast" : result.Error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReorderPo([FromBody] CreatePoRequest? model)
        {
            try
            {
                if (model == null || model.lines == null || model.lines.Count == 0)
                    return Json(new { success = false, message = "No items to order" });

                var result = await _api.PostAsync<object>("api/ai/reorder-po", new
                {
                    supplier_id = model.supplier_id,
                    expected_date = model.expected_date,
                    lines = model.lines.Select(l => new { product_id = l.product_id, quantity = l.quantity }).ToList()
                });

                return Json(result.IsSuccess
                    ? new { success = true, message = "Success", data = result.Data }
                    : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to create purchase order" : result.Error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class PredictRequest
        {
            public int lead_days { get; set; } = 7;
        }

        public class CreatePoRequest
        {
            public long supplier_id { get; set; }
            public string? expected_date { get; set; }
            public List<CreatePoLine> lines { get; set; } = new();
        }

        public class CreatePoLine
        {
            public long product_id { get; set; }
            public int quantity { get; set; }
        }
    }
}