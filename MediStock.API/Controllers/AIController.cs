using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public AIController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpPost("predict-reorder")]
        public ActionResult PredictReorder([FromBody] Newtonsoft.Json.Linq.JObject jobject)
        {
            iloggermanager.LogInfo("******* AI PREDICT REORDER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

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

                iloggermanager.LogInfo($"PredictReorder: pharmacyId={pharmacyId} predictions={predictions.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = new { predictions = predictions, expiring_soon_count = expiringSoon.Rows.Count, generated_at = DateTime.UtcNow } });
            }
            catch (Exception ex) { iloggermanager.LogError("PredictReorder: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("drug-interactions")]
        public ActionResult CheckDrugInteractions([FromBody] Newtonsoft.Json.Linq.JObject jobject)
        {
            iloggermanager.LogInfo("******* AI DRUG INTERACTIONS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (jobject == null || !jobject.ContainsKey("medications"))
                    return Bad("Medications list is required");

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

                iloggermanager.LogInfo($"CheckDrugInteractions: medications={medications.Count} interactions={interactions.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = new { interactions = interactions, checked_at = DateTime.UtcNow, disclaimer = "This is a basic interaction check. Always consult a pharmacist or physician." } });
            }
            catch (Exception ex) { iloggermanager.LogError("CheckDrugInteractions: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [NonAction]
        private List<Dictionary<string, object>> ToRows(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns) row[col.ColumnName] = dr[col];
                rows.Add(row);
            }
            return rows;
        }

        [NonAction]
        private (Int64 userId, Int64 pharmacyId, Int64 roleId) GetCaller()
        {
            Int64 userId = Convert.ToInt64(HttpContext.Items["user_id"]?.ToString() ?? "0");
            Int64 pharmacyId = Convert.ToInt64(HttpContext.Items["pharmacy_id"]?.ToString() ?? "0");
            Int64 roleId = Convert.ToInt64(HttpContext.Items["profile_id"]?.ToString() ?? "0");
            return (userId, pharmacyId, roleId);
        }

        [NonAction]
        private ActionResult Bad(string msg) =>
            StatusCode(StatusCodes.Status400BadRequest, new { success = false, message = msg, action = "", data = new JObject() });

        [NonAction]
        private ActionResult Forbidden(string msg) =>
            StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = msg, action = "", data = new JObject() });

        [NonAction]
        private ActionResult ServerError() =>
            StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error", action = "", data = new JObject() });

        [NonAction]
        public bool CaptureAuditTrail(string user, string action_type, string action_description)
        {
            AuditTrailModel audittrailmodel = new()
            {
                user_name = user,
                action_type = action_type,
                action_description = action_description,
                page_accessed = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}",
                client_ip_address = Request.HttpContext.Connection.RemoteIpAddress!.ToString(),
                session_id = "TODO"
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}
