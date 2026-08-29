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

                int leadDays = 7;
                if (jobject?["lead_days"] != null && int.TryParse(jobject["lead_days"]?.ToString(), out int ld) && ld > 0)
                    leadDays = ld;

                DataTable lowStock = dbhandler.GetRecords("low_stock_products", pharmacyId.ToString());
                DataTable expiringSoon = dbhandler.GetRecords("expiring_batches", pharmacyId.ToString());
                DataTable demand = dbhandler.GetRecords("sales_demand", pharmacyId.ToString());

                // product_id -> average daily units sold over the last 30 days
                var demandByProduct = new Dictionary<long, double>();
                foreach (DataRow row in demand.Rows)
                {
                    long pid = row["product_id"] != DBNull.Value ? Convert.ToInt64(row["product_id"]) : 0;
                    double units = row["units_30d"] != DBNull.Value ? Convert.ToDouble(row["units_30d"]) : 0;
                    if (pid > 0) demandByProduct[pid] = units / 30.0;
                }

                var predictions = new List<object>();
                foreach (DataRow row in lowStock.Rows)
                {
                    long productId = row["product_id"] != DBNull.Value ? Convert.ToInt64(row["product_id"]) : 0;
                    string productName = row["product_name"]?.ToString() ?? "";
                    string sku = row["sku"]?.ToString() ?? "";
                    int stock = row["stock_qty"] != DBNull.Value ? Convert.ToInt32(row["stock_qty"]) : 0;
                    int reorder = row["reorder_level"] != DBNull.Value ? Convert.ToInt32(row["reorder_level"]) : 0;
                    decimal cost = row["cost_price"] != DBNull.Value ? Convert.ToDecimal(row["cost_price"]) : 0;
                    decimal sell = row["selling_price"] != DBNull.Value ? Convert.ToDecimal(row["selling_price"]) : 0;
                    string unit = row["unit"]?.ToString() ?? "";
                    string category = row["category_name"]?.ToString() ?? "";

                    demandByProduct.TryGetValue(productId, out double avgDaily);

                    int suggested;
                    double? daysOfStock = avgDaily > 0 ? Math.Round(stock / avgDaily, 1) : null;
                    if (avgDaily > 0)
                    {
                        // forecast: (avg daily demand * lead time) + reorder-level safety stock
                        double target = (avgDaily * leadDays) + reorder;
                        suggested = Math.Max(0, (int)Math.Ceiling(target - stock));
                    }
                    else
                    {
                        // no sales history yet: fall back to 2x reorder level
                        suggested = Math.Max(reorder * 2, reorder == 0 ? 10 : reorder);
                    }

                    string priority = stock <= 0 ? "Critical"
                        : (avgDaily > 0 && stock < reorder ? "High" : "Medium");

                    predictions.Add(new
                    {
                        product_id = productId,
                        product_name = productName,
                        sku = sku,
                        category = category,
                        unit = unit,
                        current_stock = stock,
                        reorder_level = reorder,
                        avg_daily_sales = Math.Round(avgDaily, 2),
                        days_of_stock = daysOfStock,
                        lead_days = leadDays,
                        suggested_quantity = suggested,
                        estimated_cost = suggested * cost,
                        selling_price = sell,
                        priority = priority
                    });
                }

                var expiring = new List<object>();
                foreach (DataRow row in expiringSoon.Rows)
                {
                    DateTime? expiry = row["expiry_date"] != DBNull.Value ? Convert.ToDateTime(row["expiry_date"]) : (DateTime?)null;
                    double? daysToExpiry = expiry.HasValue ? Math.Round((expiry.Value - DateTime.Today).TotalDays, 0) : null;
                    expiring.Add(new
                    {
                        batch_id = row["batch_id"] != DBNull.Value ? Convert.ToInt64(row["batch_id"]) : 0,
                        product_id = row["product_id"] != DBNull.Value ? Convert.ToInt64(row["product_id"]) : 0,
                        product_name = row["product_name"]?.ToString() ?? "",
                        batch_number = row["batch_number"]?.ToString() ?? "",
                        expiry_date = expiry?.ToString("yyyy-MM-dd") ?? null,
                        days_to_expiry = daysToExpiry,
                        quantity = row["quantity"] != DBNull.Value ? Convert.ToInt32(row["quantity"]) : 0
                    });
                }

                iloggermanager.LogInfo($"PredictReorder: pharmacyId={pharmacyId} predictions={predictions.Count} expiring={expiring.Count}");
                return Ok(new
                {
                    success = true, message = "Success", action = "",
                    data = new
                    {
                        generated_at = DateTime.UtcNow,
                        lead_days = leadDays,
                        predictions = predictions,
                        expiring_soon = expiring,
                        expiring_soon_count = expiring.Count,
                        disclaimer = "Forecast uses the last 30 days of sales. Without sales history, suggestions fall back to 2x reorder level."
                    }
                });
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
