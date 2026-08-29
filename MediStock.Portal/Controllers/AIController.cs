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

        public class PredictRequest
        {
            public int lead_days { get; set; } = 7;
        }
    }
}