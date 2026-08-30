using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public DashboardController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet("summary")]
        public ActionResult GetDashboardSummary()
        {
            iloggermanager.LogInfo("******* GET DASHBOARD SUMMARY REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetDashboardSummary(pharmacyId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(new { success = true, message = "Success", action = "", data = new DashboardSummary() });
                }

                DataRow row = dt.Rows[0];
                var summary = new DashboardSummary
                {
                    total_products = row["total_products"] != DBNull.Value ? Convert.ToInt32(row["total_products"]) : 0,
                    total_customers = row["total_customers"] != DBNull.Value ? Convert.ToInt32(row["total_customers"]) : 0,
                    total_suppliers = row["total_suppliers"] != DBNull.Value ? Convert.ToInt32(row["total_suppliers"]) : 0,
                    today_sales = row["today_sales"] != DBNull.Value ? Convert.ToDecimal(row["today_sales"]) : 0,
                    month_sales = row["month_sales"] != DBNull.Value ? Convert.ToDecimal(row["month_sales"]) : 0,
                    month_expenses = row["month_expenses"] != DBNull.Value ? Convert.ToDecimal(row["month_expenses"]) : 0,
                    low_stock_count = row["low_stock_count"] != DBNull.Value ? Convert.ToInt32(row["low_stock_count"]) : 0,
                    expiring_soon_count = row["expiring_soon_count"] != DBNull.Value ? Convert.ToInt32(row["expiring_soon_count"]) : 0,
                    total_inventory_value = row["total_inventory_value"] != DBNull.Value ? Convert.ToDecimal(row["total_inventory_value"]) : 0
                };

                iloggermanager.LogInfo($"GetDashboardSummary: pharmacyId={pharmacyId}");
                return Ok(new { success = true, message = "Success", action = "", data = summary });
            }
            catch (Exception ex) { iloggermanager.LogError("GetDashboardSummary: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("stocksummary")]
        public ActionResult GetStockSummary()
        {
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetStockSummary(pharmacyId);
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetStockSummary: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("salesstats")]
        public ActionResult GetSalesStats()
        {
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetSalesStats(pharmacyId);
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSalesStats: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("expiringitems")]
        public ActionResult GetExpiringItems()
        {
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetExpiringItems(pharmacyId);
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetExpiringItems: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("alerts")]
        public ActionResult GetAlerts()
        {
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetAlerts(pharmacyId);
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetAlerts: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("mysales")]
        public ActionResult GetMySales()
        {
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetMySales(pharmacyId, userId);
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetMySales: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("pendingorders")]
        public ActionResult GetPendingOrders()
        {
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetPendingOrders(pharmacyId);
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetPendingOrders: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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
                client_ip_address = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                session_id = HttpContext.TraceIdentifier
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}
