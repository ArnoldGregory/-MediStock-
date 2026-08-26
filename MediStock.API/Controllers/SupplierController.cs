using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    public class SupplierController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public SupplierController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetSuppliers()
        {
            _logger.LogInfo("******* GET SUPPLIERS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("suppliers", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSuppliers: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetSupplierById(Int64 id)
        {
            _logger.LogInfo("******* GET SUPPLIER BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("supplier", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Supplier not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSupplierById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddSupplier([FromBody] SupplierModel model)
        {
            _logger.LogInfo("******* ADD SUPPLIER REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || string.IsNullOrEmpty(model.name))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Supplier name is required" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddSupplier(model);
                if (ok && model.id > 0)
                {
                    _logger.LogInfo($"AddSupplier: supplierId={model.id}");
                    CaptureAuditTrail(GetCallerEmail(), "Add Supplier", $"Added supplier: {model.name}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Supplier added successfully",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to add supplier" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddSupplier: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("po")]
        public IActionResult GetPurchaseOrders()
        {
            _logger.LogInfo("******* GET PURCHASE ORDERS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("purchase_orders", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPurchaseOrders: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("po/{id}")]
        public IActionResult GetPurchaseOrderById(Int64 id)
        {
            _logger.LogInfo("******* GET PURCHASE ORDER BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("purchase_order", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Purchase order not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPurchaseOrderById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("po/{id}/items")]
        public IActionResult GetPOItems(Int64 id)
        {
            _logger.LogInfo("******* GET PO ITEMS REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("po_items", id.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPOItems: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("po")]
        public IActionResult AddPurchaseOrder([FromBody] PurchaseOrderModel model)
        {
            _logger.LogInfo("******* ADD PURCHASE ORDER REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || model.supplier_id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Supplier ID is required" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddPurchaseOrder(model);
                if (ok && model.id > 0)
                {
                    _logger.LogInfo($"AddPurchaseOrder: poId={model.id}");
                    CaptureAuditTrail(GetCallerEmail(), "Add Purchase Order", $"Created PO {model.id} for supplier {model.supplier_id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Purchase order created",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to create purchase order" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddPurchaseOrder: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("po/{id}/receive")]
        public IActionResult ReceiveStock(Int64 id, [FromBody] ReceiveStockModel model)
        {
            _logger.LogInfo("******* RECEIVE STOCK REQUEST **********");
            try
            {
                var userId = GetCallerUserId();

                if (model == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid receive stock data" });

                var receiveStock = new ReceiveStockModel
                {
                    items = model.items ?? new List<ReceiveStockItemModel>()
                };

                foreach (var item in receiveStock.items)
                {
                    ReceiveStockItemModel itemModel = new ReceiveStockItemModel
                    {
                        product_id = item.product_id,
                        batch_number = item.batch_number,
                        expiry_date = item.expiry_date,
                        unit_cost = item.unit_cost,
                        quantity = item.quantity
                    };
                }

                bool ok = dbhandler.ReceiveStock(id, new ReceiveStockModel { items = receiveStock.items });
                if (ok)
                {
                    _logger.LogInfo($"ReceiveStock: poId={id}");
                    CaptureAuditTrail(GetCallerEmail(), "Receive Stock", $"Received stock for PO {id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Stock received successfully"
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to receive stock" });
            }
            catch (Exception ex)
            {
                _logger.LogError("ReceiveStock: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("price-history")]
        public IActionResult GetSupplierPriceHistory()
        {
            _logger.LogInfo("******* GET SUPPLIER PRICE HISTORY REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("supplier_price_history", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSupplierPriceHistory: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [NonAction]
        private void CaptureAuditTrail(string email, string actionType, string description)
        {
            try
            {
                var model = new AuditTrailModel
                {
                    user_name = email,
                    action_type = actionType,
                    action_description = description,
                    page_accessed = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}",
                    client_ip_address = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    session_id = HttpContext.Session?.Id ?? "",
                    created_on = DateTime.UtcNow
                };
                dbhandler.AddAuditTrail(model);
            }
            catch (Exception ex)
            {
                _logger.LogError("CaptureAuditTrail: " + ex.Message);
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
