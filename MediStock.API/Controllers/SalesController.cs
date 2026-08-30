using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public SalesController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetSales()
        {
            iloggermanager.LogInfo("******* GET SALES REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("sales", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSales: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("{id}")]
        public ActionResult GetSaleById(Int64 id)
        {
            iloggermanager.LogInfo("******* GET SALE BY ID REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecordsById("sale", id);
                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "Sale not found", action = "", data = new JObject() });
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSaleById: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("{id}/items")]
        public ActionResult GetSaleItems(Int64 id)
        {
            iloggermanager.LogInfo("******* GET SALE ITEMS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("sale_items", id.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSaleItems: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost]
        public ActionResult CreateSale([FromBody] SaleModel model)
        {
            iloggermanager.LogInfo("******* CREATE SALE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null)
                    return Bad("Invalid sale data");

                if (model.items == null || model.items.Count == 0)
                    return Bad("Sale must have at least one item");

                model.pharmacy_id = pharmacyId;
                model.sold_by = userId;

                bool saleOk = dbhandler.AddSale(model);
                if (!saleOk || model.id <= 0)
                    return Bad("Failed to create sale");

                bool itemsOk = dbhandler.AddSaleItems(model.id, model.pharmacy_id, model.items);
                if (!itemsOk)
                    return Bad("Sale created but failed to add items");

                iloggermanager.LogInfo($"CreateSale: saleId={model.id}");
                CaptureAuditTrail(userId.ToString(), "Create Sale", $"Created sale {model.id}");
                return Ok(new { success = true, message = "Sale created successfully", action = "", data = new JObject { { "id", model.id } } });
            }
            catch (Exception ex) { iloggermanager.LogError("CreateSale: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("voidsale")]
        public ActionResult VoidSale([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* VOID SALE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (jobject == null || !jobject.ContainsKey("id"))
                    return Bad("Sale id is required");

                long saleId = jobject["id"].Value<long>();
                if (saleId <= 0)
                    return Bad("Invalid sale id");

                var (ok, message) = dbhandler.VoidSale(saleId, pharmacyId);
                if (!ok)
                    return Bad(message);

                CaptureAuditTrail(userId.ToString(), "Void Sale", $"Voided sale {saleId}");
                dbhandler.AddNotification(pharmacyId, userId, "Sale voided", $"Sale {saleId} was voided by user {userId}. Stock restored.", "Info");

                return Ok(new { success = true, message = message, action = "", data = new JObject() });
            }
            catch (Exception ex) { iloggermanager.LogError("VoidSale: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("products")]
        public ActionResult GetProductsForPOS()
        {
            iloggermanager.LogInfo("******* GET PRODUCTS FOR POS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("products", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetProductsForPOS: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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