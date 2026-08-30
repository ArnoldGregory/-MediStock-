using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public ReportsController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet("sales")]
        public ActionResult GetSalesReport([FromQuery] string? from_date, [FromQuery] string? to_date)
        {
            iloggermanager.LogInfo("******* GET SALES REPORT REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                string p1 = pharmacyId.ToString();
                string p2 = from_date ?? "";
                string p3 = to_date ?? "";
                DataTable dt = dbhandler.GetRecords("report_sales", p1, p2, p3);
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSalesReport: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("stock")]
        public ActionResult GetStockReport()
        {
            iloggermanager.LogInfo("******* GET STOCK REPORT REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("report_stock", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetStockReport: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("financial")]
        public ActionResult GetFinancialReport([FromQuery] string? from_date, [FromQuery] string? to_date)
        {
            iloggermanager.LogInfo("******* GET FINANCIAL REPORT REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                string p1 = pharmacyId.ToString();
                string p2 = from_date ?? "";
                string p3 = to_date ?? "";
                DataTable dt = dbhandler.GetRecords("report_financial", p1, p2, p3);
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetFinancialReport: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("margins")]
        public ActionResult GetProductMargins()
        {
            iloggermanager.LogInfo("******* GET PRODUCT MARGINS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("report_product_margins", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetProductMargins: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("expensebreakdown")]
        public ActionResult GetExpenseBreakdown([FromQuery] string? from_date, [FromQuery] string? to_date)
        {
            iloggermanager.LogInfo("******* GET EXPENSE BREAKDOWN REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                string p2 = from_date ?? "";
                string p3 = to_date ?? "";
                DataTable dt = dbhandler.GetRecords("report_expense_by_category", pharmacyId.ToString(), p2, p3);
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetExpenseBreakdown: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("stock-performance")]
        public ActionResult GetStockPerformance()
        {
            iloggermanager.LogInfo("******* GET STOCK PERFORMANCE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                DataTable products = dbhandler.GetRecords("products", pharmacyId.ToString());
                DataTable demand = dbhandler.GetRecords("sales_demand", pharmacyId.ToString());

                var demandByProduct = new Dictionary<long, double>();
                foreach (DataRow r in demand.Rows)
                {
                    long pid = r["product_id"] != DBNull.Value ? Convert.ToInt64(r["product_id"]) : 0;
                    double units = r["units_30d"] != DBNull.Value ? Convert.ToDouble(r["units_30d"]) : 0;
                    if (pid > 0) demandByProduct[pid] = units;
                }

                var rows = new List<object>();
                decimal marginTotal = 0; int marginCount = 0; int slowCount = 0; int outCount = 0; decimal invValue = 0;
                decimal slowValue = 0;

                foreach (DataRow r in products.Rows)
                {
                    long pid = r["id"] != DBNull.Value ? Convert.ToInt64(r["id"]) : 0;
                    string name = r["name"]?.ToString() ?? "";
                    string sku = r["sku"]?.ToString() ?? "";
                    string cat = r["category_name"]?.ToString() ?? "";
                    int stock = r["stock_qty"] != DBNull.Value ? Convert.ToInt32(r["stock_qty"]) : 0;
                    decimal cost = r["cost_price"] != DBNull.Value ? Convert.ToDecimal(r["cost_price"]) : 0;
                    decimal sell = r["selling_price"] != DBNull.Value ? Convert.ToDecimal(r["selling_price"]) : 0;

                    demandByProduct.TryGetValue(pid, out double units30);

                    decimal marginKes = sell - cost;
                    decimal? marginPct = cost > 0 ? Math.Round(marginKes / cost * 100, 1) : (decimal?)null;
                    double avgDaily = units30 / 30.0;
                    double? daysOfStock = avgDaily > 0 ? Math.Round(stock / avgDaily, 1) : null;
                    decimal inv = Math.Round(stock * cost, 2);

                    string status;
                    if (stock <= 0) { status = "Out of Stock"; outCount++; }
                    else if (units30 <= 0) { status = "Slow"; slowCount++; slowValue += inv; }
                    else if (daysOfStock >= 120) { status = "Slow"; slowCount++; slowValue += inv; }
                    else { status = "Healthy"; }

                    if (marginPct.HasValue) { marginTotal += marginPct.Value; marginCount++; }
                    invValue += inv;

                    rows.Add(new
                    {
                        product_id = pid,
                        name, sku, category = cat,
                        stock, cost, sell,
                        margin_kes = Math.Round(marginKes, 2),
                        margin_pct = marginPct,
                        units_30d = units30,
                        avg_daily_sales = Math.Round(avgDaily, 2),
                        days_of_stock = daysOfStock,
                        inventory_value = inv,
                        status
                    });
                }

                iloggermanager.LogInfo($"GetStockPerformance: products={rows.Count}");
                return Ok(new
                {
                    success = true, message = "Success", action = "",
                    data = new
                    {
                        generated_at = DateTime.UtcNow,
                        summary = new
                        {
                            product_count = rows.Count,
                            avg_margin_pct = marginCount > 0 ? Math.Round(marginTotal / marginCount, 1) : (decimal?)null,
                            slow_count = slowCount,
                            out_count = outCount,
                            inventory_value = invValue,
                            slow_stock_value = Math.Round(slowValue, 2)
                        },
                        products = rows
                    }
                });
            }
            catch (Exception ex) { iloggermanager.LogError("GetStockPerformance: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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
                session_id = HttpContext.TraceIdentifier
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}