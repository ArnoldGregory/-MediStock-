using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public StockController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet("batches")]
        public ActionResult GetBatches()
        {
            iloggermanager.LogInfo("******* GET BATCHES REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("product_batches", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetBatches: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("batches/{id}")]
        public ActionResult GetBatchById(Int64 id)
        {
            iloggermanager.LogInfo("******* GET BATCH BY ID REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecordsById("batch", id);
                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "Batch not found", action = "", data = new JObject() });
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetBatchById: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("batches")]
        public ActionResult AddBatch([FromBody] ProductBatchModel model)
        {
            iloggermanager.LogInfo("******* ADD BATCH REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null || model.product_id <= 0)
                    return Bad("Product ID is required");

                if (string.IsNullOrEmpty(model.batch_number))
                    return Bad("Batch number is required");

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                string sql = "INSERT INTO product_batches (pharmacy_id, product_id, batch_number, expiry_date, cost_price, quantity, status, created_by) " +
                    $"VALUES ({pharmacyId}, {model.product_id}, '{model.batch_number}', '{model.expiry_date:yyyy-MM-dd}', {model.cost_price}, {model.quantity}, 'Active', {userId})";
                dbhandler.ExecuteNonQuery(sql);

                DataTable dtId = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dtId.Rows.Count > 0 ? Convert.ToInt64(dtId.Rows[0]["id"]) : 0;

                iloggermanager.LogInfo($"AddBatch: batchId={id}");
                CaptureAuditTrail(userId.ToString(), "Add Batch", $"Added batch {id} for product {model.product_id}");
                return Ok(new { success = true, message = "Batch added successfully", action = "", data = new JObject { { "id", id } } });
            }
            catch (Exception ex) { iloggermanager.LogError("AddBatch: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("adjustments")]
        public ActionResult AddStockAdjustment([FromBody] StockAdjustmentModel model)
        {
            iloggermanager.LogInfo("******* ADD STOCK ADJUSTMENT REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null || model.product_id <= 0)
                    return Bad("Product ID is required");

                if (string.IsNullOrEmpty(model.adjustment_type))
                    return Bad("Adjustment type is required");

                model.pharmacy_id = pharmacyId;
                model.adjusted_by = userId;

                string sql = $"INSERT INTO stock_adjustments (pharmacy_id, product_id, batch_id, adjustment_type, quantity, reason, adjusted_by) " +
                    $"VALUES ({pharmacyId}, {model.product_id}, {(model.batch_id > 0 ? model.batch_id.ToString() : "NULL")}, '{model.adjustment_type}', {model.quantity}, '{(model.reason ?? "").Replace("'", "''")}', {userId})";

                dbhandler.ExecuteNonQuery(sql);
                DataTable dtAdj = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dtAdj.Rows.Count > 0 ? Convert.ToInt64(dtAdj.Rows[0]["id"]) : 0;

                iloggermanager.LogInfo($"AddStockAdjustment: adjustmentId={id}");
                CaptureAuditTrail(userId.ToString(), "Stock Adjustment", $"Recorded adjustment {id} ({model.adjustment_type})");
                return Ok(new { success = true, message = "Stock adjustment recorded", action = "", data = new JObject { { "id", id } } });
            }
            catch (Exception ex) { iloggermanager.LogError("AddStockAdjustment: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("adjustments")]
        public ActionResult GetAdjustments()
        {
            iloggermanager.LogInfo("******* GET ADJUSTMENTS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("stock_adjustments", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetAdjustments: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("stocktake")]
        public ActionResult GetStockTakeSessions()
        {
            iloggermanager.LogInfo("******* GET STOCK TAKE SESSIONS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("stock_take_sessions", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetStockTakeSessions: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("stocktake")]
        public ActionResult AddStockTakeSession([FromBody] StockTakeSessionModel model)
        {
            iloggermanager.LogInfo("******* ADD STOCK TAKE SESSION REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null || string.IsNullOrEmpty(model.session_name))
                    return Bad("Session name is required");

                model.pharmacy_id = pharmacyId;
                model.started_by = userId;

                string sql = $"INSERT INTO stock_take_sessions (pharmacy_id, session_name, status, started_by) " +
                    $"VALUES ({pharmacyId}, '{model.session_name.Replace("'", "''")}', 'Open', {userId})";

                dbhandler.ExecuteNonQuery(sql);
                DataTable dt = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dt.Rows.Count > 0 ? Convert.ToInt64(dt.Rows[0]["id"]) : 0;

                iloggermanager.LogInfo($"AddStockTakeSession: sessionId={id}");
                CaptureAuditTrail(userId.ToString(), "Stock Take Session", $"Created stock take session: {model.session_name}");
                return Ok(new { success = true, message = "Stock take session created", action = "", data = new JObject { { "id", id } } });
            }
            catch (Exception ex) { iloggermanager.LogError("AddStockTakeSession: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("stocktake/items")]
        public ActionResult AddStockTakeItem([FromBody] StockTakeItemModel model)
        {
            iloggermanager.LogInfo("******* ADD STOCK TAKE ITEM REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null || model.session_id <= 0)
                    return Bad("Session ID is required");

                if (model.product_id <= 0)
                    return Bad("Product ID is required");

                int variance = model.counted_qty - model.system_qty;

                string sql = $"INSERT INTO stock_take_items (session_id, product_id, batch_id, system_qty, counted_qty, variance, notes) " +
                    $"VALUES ({model.session_id}, {model.product_id}, {(model.batch_id > 0 ? model.batch_id.ToString() : "NULL")}, {model.system_qty}, {model.counted_qty}, {variance}, '{(model.notes ?? "").Replace("'", "''")}')";

                dbhandler.ExecuteNonQuery(sql);

                DataTable dt = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dt.Rows.Count > 0 ? Convert.ToInt64(dt.Rows[0]["id"]) : 0;

                iloggermanager.LogInfo($"AddStockTakeItem: itemId={id}");
                CaptureAuditTrail(userId.ToString(), "Stock Take Item", $"Recorded stock take item {id} for session {model.session_id}");
                return Ok(new { success = true, message = "Stock take item recorded", action = "", data = new JObject { { "id", id } } });
            }
            catch (Exception ex) { iloggermanager.LogError("AddStockTakeItem: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("stocktake/commit/{sessionId}")]
        public ActionResult CommitStockTake(Int64 sessionId)
        {
            iloggermanager.LogInfo("******* COMMIT STOCK TAKE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                string sql = $"UPDATE stock_take_sessions SET status = 'Committed', committed_on = NOW(), committed_by = {userId} WHERE id = {sessionId}";
                dbhandler.ExecuteNonQuery(sql);

                iloggermanager.LogInfo($"CommitStockTake: sessionId={sessionId}");
                CaptureAuditTrail(userId.ToString(), "Commit Stock Take", $"Committed stock take session {sessionId}");
                return Ok(new { success = true, message = "Stock take committed successfully", action = "", data = (object?)null });
            }
            catch (Exception ex) { iloggermanager.LogError("CommitStockTake: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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