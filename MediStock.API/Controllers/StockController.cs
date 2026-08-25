using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public StockController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("batches")]
        public IActionResult GetBatches()
        {
            _logger.LogInfo("******* GET BATCHES REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("product_batches", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetBatches: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("batches/{id}")]
        public IActionResult GetBatchById(Int64 id)
        {
            _logger.LogInfo("******* GET BATCH BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("batch", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Batch not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetBatchById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("batches")]
        public IActionResult AddBatch([FromBody] ProductBatchModel model)
        {
            _logger.LogInfo("******* ADD BATCH REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || model.product_id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Product ID is required" });

                if (string.IsNullOrEmpty(model.batch_number))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Batch number is required" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                string sql = "INSERT INTO product_batches (pharmacy_id, product_id, batch_number, expiry_date, cost_price, quantity, status, created_by) " +
                    $"VALUES ({pharmacyId}, {model.product_id}, '{model.batch_number}', '{model.expiry_date:yyyy-MM-dd}', {model.cost_price}, {model.quantity}, 'Active', {userId})";
                dbhandler.ExecuteNonQuery(sql);

                DataTable dtId = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dtId.Rows.Count > 0 ? Convert.ToInt64(dtId.Rows[0]["id"]) : 0;

                _logger.LogInfo($"AddBatch: batchId={id}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Batch added successfully",
                    data = new { id = id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddBatch: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("adjustments")]
        public IActionResult AddStockAdjustment([FromBody] StockAdjustmentModel model)
        {
            _logger.LogInfo("******* ADD STOCK ADJUSTMENT REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || model.product_id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Product ID is required" });

                if (string.IsNullOrEmpty(model.adjustment_type))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Adjustment type is required" });

                model.pharmacy_id = pharmacyId;
                model.adjusted_by = userId;

                string sql = $"INSERT INTO stock_adjustments (pharmacy_id, product_id, batch_id, adjustment_type, quantity, reason, adjusted_by) " +
                    $"VALUES ({pharmacyId}, {model.product_id}, {(model.batch_id > 0 ? model.batch_id.ToString() : "NULL")}, '{model.adjustment_type}', {model.quantity}, '{(model.reason ?? "").Replace("'", "''")}', {userId})";

                dbhandler.ExecuteNonQuery(sql);
                DataTable dtAdj = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dtAdj.Rows.Count > 0 ? Convert.ToInt64(dtAdj.Rows[0]["id"]) : 0;

                _logger.LogInfo($"AddStockAdjustment: adjustmentId={id}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Stock adjustment recorded",
                    data = new { id = id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddStockAdjustment: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("adjustments")]
        public IActionResult GetAdjustments()
        {
            _logger.LogInfo("******* GET ADJUSTMENTS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("stock_adjustments", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAdjustments: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("stocktake")]
        public IActionResult AddStockTakeSession([FromBody] StockTakeSessionModel model)
        {
            _logger.LogInfo("******* ADD STOCK TAKE SESSION REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || string.IsNullOrEmpty(model.session_name))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Session name is required" });

                model.pharmacy_id = pharmacyId;
                model.started_by = userId;

                string sql = $"INSERT INTO stock_take_sessions (pharmacy_id, session_name, status, started_by) " +
                    $"VALUES ({pharmacyId}, '{model.session_name.Replace("'", "''")}', 'Open', {userId})";

                dbhandler.ExecuteNonQuery(sql);
                DataTable dt = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dt.Rows.Count > 0 ? Convert.ToInt64(dt.Rows[0]["id"]) : 0;

                _logger.LogInfo($"AddStockTakeSession: sessionId={id}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Stock take session created",
                    data = new { id = id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddStockTakeSession: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("stocktake/items")]
        public IActionResult AddStockTakeItem([FromBody] StockTakeItemModel model)
        {
            _logger.LogInfo("******* ADD STOCK TAKE ITEM REQUEST **********");
            try
            {
                if (model == null || model.session_id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Session ID is required" });

                if (model.product_id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Product ID is required" });

                int variance = model.counted_qty - model.system_qty;

                string sql = $"INSERT INTO stock_take_items (session_id, product_id, batch_id, system_qty, counted_qty, variance, notes) " +
                    $"VALUES ({model.session_id}, {model.product_id}, {(model.batch_id > 0 ? model.batch_id.ToString() : "NULL")}, {model.system_qty}, {model.counted_qty}, {variance}, '{(model.notes ?? "").Replace("'", "''")}')";

                dbhandler.ExecuteNonQuery(sql);

                DataTable dt = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dt.Rows.Count > 0 ? Convert.ToInt64(dt.Rows[0]["id"]) : 0;

                _logger.LogInfo($"AddStockTakeItem: itemId={id}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Stock take item recorded",
                    data = new { id = id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddStockTakeItem: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("stocktake/commit/{sessionId}")]
        public IActionResult CommitStockTake(Int64 sessionId)
        {
            _logger.LogInfo("******* COMMIT STOCK TAKE REQUEST **********");
            try
            {
                var userId = GetCallerUserId();

                string sql = $"UPDATE stock_take_sessions SET status = 'Committed', committed_at = NOW(), committed_by = {userId} WHERE id = {sessionId}";
                dbhandler.ExecuteNonQuery(sql);

                _logger.LogInfo($"CommitStockTake: sessionId={sessionId}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Stock take committed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("CommitStockTake: " + ex.Message + " - " + ex.StackTrace);
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
