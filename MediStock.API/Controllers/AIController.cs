using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public AIController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("predict-reorder")]
        public IActionResult PredictReorder([FromBody] Newtonsoft.Json.Linq.JObject jobject)
        {
            _logger.LogInfo("******* AI PREDICT REORDER REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();

                DataTable lowStock = dbhandler.GetRecords("low_stock_products", pharmacyId.ToString());
                DataTable expiringSoon = dbhandler.GetRecords("expiring_batches", pharmacyId.ToString());

                var predictions = new List<object>();
                foreach (DataRow row in lowStock.Rows)
                {
                    predictions.Add(new
                    {
                        product_id = row["product_id"] != DBNull.Value ? Convert.ToInt64(row["product_id"]) : 0,
                        product_name = row["product_name"]?.ToString() ?? "",
                        current_stock = row["stock_qty"] != DBNull.Value ? Convert.ToInt32(row["stock_qty"]) : 0,
                        reorder_level = row["reorder_level"] != DBNull.Value ? Convert.ToInt32(row["reorder_level"]) : 0,
                        suggested_quantity = row["reorder_level"] != DBNull.Value ? Convert.ToInt32(row["reorder_level"]) * 2 : 0,
                        priority = row["stock_qty"] != DBNull.Value && Convert.ToInt32(row["stock_qty"]) == 0 ? "Critical" : "High"
                    });
                }

                _logger.LogInfo($"PredictReorder: pharmacyId={pharmacyId} predictions={predictions.Count}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    data = new
                    {
                        predictions = predictions,
                        expiring_soon_count = expiringSoon.Rows.Count,
                        generated_at = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("PredictReorder: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("drug-interactions")]
        public IActionResult CheckDrugInteractions([FromBody] Newtonsoft.Json.Linq.JObject jobject)
        {
            _logger.LogInfo("******* AI DRUG INTERACTIONS REQUEST **********");
            try
            {
                if (jobject == null || !jobject.ContainsKey("medications"))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Medications list is required" });

                var medications = jobject["medications"]?.ToObject<List<string>>() ?? new List<string>();

                var interactions = new List<object>();
                var knownInteractions = new Dictionary<string, string[]>
                {
                    { "Warfarin", new[] { "Aspirin", "Ibuprofen", "Paracetamol" } },
                    { "Metformin", new[] { "Alcohol", "Contrast Dye" } },
                    { "Lisinopril", new[] { "Potassium", "NSAIDs" } }
                };

                foreach (var med in medications)
                {
                    if (knownInteractions.ContainsKey(med))
                    {
                        var conflicting = knownInteractions[med].Where(m => medications.Contains(m, StringComparer.OrdinalIgnoreCase)).ToArray();
                        if (conflicting.Length > 0)
                        {
                            interactions.Add(new
                            {
                                medication = med,
                                interacts_with = conflicting,
                                severity = "Moderate",
                                recommendation = "Consult a healthcare professional before concurrent use"
                            });
                        }
                    }
                }

                _logger.LogInfo($"CheckDrugInteractions: medications={medications.Count} interactions={interactions.Count}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    data = new
                    {
                        interactions = interactions,
                        checked_at = DateTime.UtcNow,
                        disclaimer = "This is a basic interaction check. Always consult a pharmacist or physician."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("CheckDrugInteractions: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        private Int64 GetCallerPharmacyId()
        {
            var claim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id");
            return claim != null ? Convert.ToInt64(claim.Value) : 0;
        }

        private Int64 GetCallerUserId()
        {
            var claim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id");
            return claim != null ? Convert.ToInt64(claim.Value) : 0;
        }

        private string GetCallerEmail()
        {
            return HttpContext.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
        }

        private int GetCallerRoleId()
        {
            var claim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "role_id");
            return claim != null ? Convert.ToInt32(claim.Value) : 0;
        }
    }
}
