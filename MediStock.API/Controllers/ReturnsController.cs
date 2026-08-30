using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/returns")]
    public class ReturnsController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public ReturnsController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetReturns()
        {
            iloggermanager.LogInfo("******* GET RETURNS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                DataTable dt = dbhandler.GetSaleReturns(pharmacyId);
                iloggermanager.LogInfo($"GetReturns: rows={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetReturns: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("items")]
        public ActionResult GetReturnable([FromQuery] Int64 sale_id)
        {
            iloggermanager.LogInfo("******* GET RETURNABLE ITEMS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                if (sale_id <= 0) return Bad("sale_id is required");
                DataTable dt = dbhandler.GetSaleReturnableItems(sale_id);
                iloggermanager.LogInfo($"GetReturnable: sale_id={sale_id} rows={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetReturnable: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("detail")]
        public ActionResult GetReturnDetail([FromQuery] Int64 return_id)
        {
            iloggermanager.LogInfo("******* GET RETURN DETAIL REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                if (return_id <= 0) return Bad("return_id is required");
                DataTable dt = dbhandler.GetSaleReturnItems(return_id);
                iloggermanager.LogInfo($"GetReturnDetail: return_id={return_id} rows={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetReturnDetail: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost]
        public ActionResult CreateReturn([FromBody] CreateReturnRequest req)
        {
            iloggermanager.LogInfo("******* CREATE RETURN REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                if (req == null) return Bad("Invalid return data");
                if (req.sale_id <= 0) return Bad("sale_id is required");
                if (req.items == null || req.items.Count == 0) return Bad("Return must have at least one item");

                // Server-side safety: refund cannot exceed the value of the returned stock.
                decimal totalRefund = 0;
                foreach (var it in req.items)
                {
                    if (it.quantity <= 0) return Bad("Return quantity must be greater than zero");
                    decimal lineValue = Math.Round(it.quantity * it.unit_price, 2);
                    decimal refund = it.refund > 0 ? Math.Min(it.refund, lineValue) : lineValue;
                    it.refund = refund;
                    totalRefund += refund;
                }
                totalRefund = Math.Round(totalRefund, 2);

                string itemsJson = JsonConvert.SerializeObject(req.items);
                var created = dbhandler.CreateSaleReturn(pharmacyId, req.sale_id, req.customer_id,
                    req.reason, totalRefund, userId, itemsJson);

                if (created == null)
                    return Bad("Return failed — check that quantities do not exceed what was sold");

                iloggermanager.LogInfo($"CreateReturn: returnId={created.Value.id} number={created.Value.number}");
                CaptureAuditTrail(userId.ToString(), "Sales Return",
                    $"Recorded return {created.Value.number} for sale {req.sale_id} (refund {totalRefund:N2})");
                return Ok(new
                {
                    success = true,
                    message = "Return recorded and stock restored",
                    action = "",
                    data = new JObject { { "id", created.Value.id }, { "return_number", created.Value.number }, { "total_refund", totalRefund } }
                });
            }
            catch (Exception ex) { iloggermanager.LogError("CreateReturn: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        public class CreateReturnRequest
        {
            public Int64 sale_id { get; set; }
            public Int64? customer_id { get; set; }
            public string? reason { get; set; }
            public List<ReturnItemRequest>? items { get; set; }
        }

        public class ReturnItemRequest
        {
            public Int64 sale_item_id { get; set; }
            public Int64 product_id { get; set; }
            public Int64? batch_id { get; set; }
            public int quantity { get; set; }
            public decimal unit_price { get; set; }
            public decimal refund { get; set; }
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