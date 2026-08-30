using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;

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
        [HttpPost("reorder-po")]
        public ActionResult CreateReorderPo([FromBody] Newtonsoft.Json.Linq.JObject jobject)
        {
            iloggermanager.LogInfo("******* AI REORDER->PO REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (jobject == null)
                    return Bad("Request body is required");

                long supplierId = jobject["supplier_id"] != null ? Convert.ToInt64(jobject["supplier_id"].ToString()) : 0;
                if (supplierId <= 0)
                    return Bad("Select a supplier for the purchase order");

                var rawLines = jobject["lines"] as JArray;
                if (rawLines == null || rawLines.Count == 0)
                    return Bad("No items to order");

                // product cost lookup for this pharmacy only
                var costs = new Dictionary<long, decimal>();
                DataTable products = dbhandler.GetRecords("products", pharmacyId.ToString());
                foreach (DataRow r in products.Rows)
                {
                    long pid = r["id"] != DBNull.Value ? Convert.ToInt64(r["id"]) : 0;
                    if (pid > 0)
                        costs[pid] = r["cost_price"] != DBNull.Value ? Convert.ToDecimal(r["cost_price"]) : 0;
                }

                var lines = new List<(long productId, int quantity, decimal unitCost)>();
                foreach (var raw in rawLines)
                {
                    long productId = raw["product_id"] != null ? Convert.ToInt64(raw["product_id"].ToString()) : 0;
                    int qty = raw["quantity"] != null ? Convert.ToInt32(raw["quantity"].ToString()) : 0;
                    if (productId <= 0 || qty <= 0)
                    {
                        iloggermanager.LogInfo($"reorder-po: skipped invalid line product_id={productId} qty={qty}");
                        continue;
                    }
                    if (!costs.ContainsKey(productId))
                        return Bad($"Product ID {productId} does not belong to this pharmacy");
                    lines.Add((productId, qty, costs[productId]));
                }
                if (lines.Count == 0)
                    return Bad("No valid order lines");

                DateTime? expectedDate = null;
                if (jobject["expected_date"] != null
                    && DateTime.TryParse(jobject["expected_date"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ed))
                    expectedDate = ed;

                decimal total = lines.Sum(l => l.quantity * l.unitCost);

                long poId = 0; string poNumber = "";
                for (int attempt = 0; attempt < 3 && poId <= 0; attempt++)
                {
                    poNumber = $"PO-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
                    poId = dbhandler.ExecuteInsertReturnId(
                        "INSERT INTO purchase_orders (pharmacy_id, supplier_id, po_number, status, total, expected_date, created_by, created_on) " +
                        "VALUES (@pharmacy_id, @supplier_id, @po_number, 'Pending', @total, @expected_date, @created_by, NOW())",
                        new { pharmacy_id = pharmacyId, supplier_id = supplierId, po_number = poNumber, total = total, expected_date = expectedDate, created_by = userId });
                }
                if (poId <= 0)
                    return Bad("Failed to save purchase order");

                foreach (var l in lines)
                {
                    _ = dbhandler.ExecuteNonQuery(
                        "INSERT INTO po_items (po_id, product_id, quantity, received_qty, unit_cost, total) " +
                        "VALUES (@po_id, @product_id, @quantity, 0, @unit_cost, @total)",
                        new { po_id = poId, product_id = l.productId, quantity = l.quantity, unit_cost = l.unitCost, total = l.quantity * l.unitCost });
                }

                CaptureAuditTrail(userId.ToString(), "CREATE", $"Draft PO {poNumber} created from AI reorder ({lines.Count} items, total KES {total:F2})");

                iloggermanager.LogInfo($"CreateReorderPo: poId={poId} poNumber={poNumber} lines={lines.Count} total={total}");
                return Ok(new
                {
                    success = true, message = "Draft purchase order created", action = "",
                    data = new { po_id = poId, po_number = poNumber, total = total, line_count = lines.Count, status = "Pending" }
                });
            }
            catch (Exception ex) { iloggermanager.LogError("CreateReorderPo: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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

                var raw = jobject["medications"]?.ToObject<List<string>>() ?? new List<string>();
                var medications = raw
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Select(m => m.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (medications.Count == 0)
                    return Bad("Medications list is required");

                var interactions = new List<object>();
                foreach (var def in KnownInteractions)
                {
                    bool hasA = medications.Contains(def.A, StringComparer.OrdinalIgnoreCase);
                    bool hasB = medications.Contains(def.B, StringComparer.OrdinalIgnoreCase);
                    if (hasA && hasB)
                    {
                        interactions.Add(new
                        {
                            medication_a = def.A,
                            medication_b = def.B,
                            severity = def.Severity,
                            recommendation = def.Recommendation
                        });
                    }
                }

                iloggermanager.LogInfo($"CheckDrugInteractions: medications={medications.Count} interactions={interactions.Count}");
                return Ok(new
                {
                    success = true, message = "Success", action = "",
                    data = new
                    {
                        interactions = interactions,
                        count = interactions.Count,
                        checked_at = DateTime.UtcNow,
                        disclaimer = "Automated interaction screening cannot replace clinical judgement. Always verify against a pharmacist or physician."
                    }
                });
            }
            catch (Exception ex) { iloggermanager.LogError("CheckDrugInteractions: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        private sealed class InteractionDef
        {
            public string A { get; }
            public string B { get; }
            public string Severity { get; }
            public string Recommendation { get; }

            public InteractionDef(string a, string b, string severity, string recommendation)
            {
                A = a; B = b; Severity = severity; Recommendation = recommendation;
            }
        }

        private static readonly InteractionDef[] KnownInteractions =
        {
            // Anticoagulants / antiplatelets
            new("Warfarin", "Aspirin", "Severe", "Markedly increased bleeding risk. Avoid combined use."),
            new("Warfarin", "Ibuprofen", "Severe", "Additive bleeding risk and GI injury. Use paracetamol instead (with monitoring)."),
            new("Warfarin", "Diclofenac", "Severe", "Additive bleeding risk. Avoid; consider alternative analgesia."),
            new("Warfarin", "Ciprofloxacin", "Severe", "Antibiotic increases INR and bleeding risk. Monitor INR closely."),
            new("Warfarin", "Metronidazole", "Severe", "Strongly increases warfarin effect. Reduce dose and monitor INR."),
            new("Warfarin", "Fluconazole", "Severe", "Inhibits warfarin metabolism — elevated INR and bleeding risk."),
            new("Warfarin", "Simvastatin", "Moderate", "Additive effect on INR. Monitor and adjust warfarin dose."),
            new("Warfarin", "Paracetamol", "Moderate", "Frequent high doses may increase INR. Keep paracetamol low and monitor."),

            // Statins
            new("Simvastatin", "Clarithromycin", "Severe", "Risk of rhabdomyolysis. Do not combine; hold statin during therapy."),
            new("Simvastatin", "Itraconazole", "Severe", "Severe myopathy risk. Avoid combined use."),
            new("Simvastatin", "Gemfibrozil", "Severe", "Increased myopathy/rhabdomyolysis risk. Prefer fenofibrate or avoid."),
            new("Simvastatin", "Fluconazole", "Moderate", "Elevated statin levels. Limit simvastatin dose to 20 mg/day."),

            // Metformin
            new("Metformin", "Alcohol", "Moderate", "Excessive alcohol raises lactic acidosis risk. Limit intake, especially on empty stomach."),
            new("Metformin", "Iodinated Contrast", "Moderate", "Increased lactic acidosis risk with contrast studies. Temporarily hold metformin."),

            // ACE inhibitors / diuretics
            new("Lisinopril", "Spironolactone", "Severe", "Risk of dangerous hyperkalemia. Monitor potassium frequently."),
            new("Lisinopril", "Potassium", "Moderate", "Potassium supplements raise hyperkalemia risk when combined."),
            new("Lisinopril", "Ibuprofen", "Moderate", "NSAIDs reduce antihypertensive effect and impair renal function."),
            new("Lisinopril", "Diclofenac", "Moderate", "NSAIDs reduce antihypertensive effect and impair renal function."),

            // Antibiotics
            new("Amoxicillin", "Methotrexate", "Severe", "May elevate methotrexate levels to toxic range. Avoid if possible."),
            new("Ciprofloxacin", "Tizanidine", "Severe", "Dangerous drop in blood pressure and sedation. Contraindicated."),
            new("Ciprofloxacin", "Theophylline", "Moderate", "Raises theophylline levels — risk of seizures. Monitor levels."),
            new("Ciprofloxacin", "Antacids", "Moderate", "Calcium/magnesium/iron reduce ciprofloxacin absorption. Separate by 2–4 hours."),
            new("Doxycycline", "Antacids", "Moderate", "Calcium/iron/magnesium bind doxycycline. Separate by 2 hours."),
            new("Doxycycline", "Warfarin", "Moderate", "May increase INR. Monitor."),
            new("Clarithromycin", "Digoxin", "Severe", "Raises digoxin levels. Monitor and reduce digoxin dose."),
            new("Clarithromycin", "Colchicine", "Severe", "Risk of severe colchicine toxicity. Avoid combined use."),
            new("Metronidazole", "Alcohol", "Moderate", "Disulfiram-like reaction: flushing, palpitations, nausea. Avoid alcohol during and 48h after."),

            // Cardiac
            new("Digoxin", "Amiodarone", "Severe", "Doubles digoxin levels — toxicity risk. Reduce digoxin dose and monitor."),
            new("Digoxin", "Furosemide", "Moderate", "Low potassium from furosemide increases digoxin toxicity. Monitor potassium."),
            new("Clopidogrel", "Omeprazole", "Moderate", "Omeprazole may reduce clopidogrel activation. Consider pantoprazole."),
            new("Digoxin", "Omeprazole", "Moderate", "Reduced digoxin absorption. Monitor levels."),

            // Respiratory / CNS
            new("Salbutamol", "Propranolol", "Moderate", "Non-selective beta-blockers antagonize salbutamol in airways. Use cardioselective agent."),
            new("Tramadol", "Sertraline", "Severe", "Serotonin syndrome risk. Avoid; choose alternative analgesia."),
            new("Tramadol", "Fluoxetine", "Severe", "Serotonin syndrome risk. Avoid combined use."),
            new("Prednisone", "Ibuprofen", "Moderate", "Increased risk of GI bleeding when combined."),
            new("Prednisone", "Diclofenac", "Moderate", "Increased risk of GI bleeding when combined.")
        };

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
