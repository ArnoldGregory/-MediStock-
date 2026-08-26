using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public ProductController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetProducts()
        {
            _logger.LogInfo("******* GET PRODUCTS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("products", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProducts: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetProductById(Int64 id)
        {
            _logger.LogInfo("******* GET PRODUCT BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("product", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Product not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProductById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddProduct([FromBody] ProductModel model)
        {
            _logger.LogInfo("******* ADD PRODUCT REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || string.IsNullOrEmpty(model.name))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Product name is required" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddProduct(model);
                if (ok && model.id > 0)
                {
                    _logger.LogInfo($"AddProduct: productId={model.id}");
                    CaptureAuditTrail(GetCallerEmail(), "Add Product", $"Added product: {model.name}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Product added successfully",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to add product" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddProduct: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(Int64 id, [FromBody] ProductModel model)
        {
            _logger.LogInfo("******* UPDATE PRODUCT REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();

                if (model == null || string.IsNullOrEmpty(model.name))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Product name is required" });

                model.id = id;
                model.pharmacy_id = pharmacyId;

                bool ok = dbhandler.UpdateProduct(model);
                if (ok)
                {
                    _logger.LogInfo($"UpdateProduct: productId={id}");
                    CaptureAuditTrail(GetCallerEmail(), "Update Product", $"Updated product {id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Product updated successfully",
                        data = new { id = id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to update product" });
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateProduct: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(Int64 id)
        {
            _logger.LogInfo("******* DELETE PRODUCT REQUEST **********");
            try
            {
                var userId = GetCallerUserId();
                bool ok = dbhandler.DeleteRecord(id, userId, "products");
                if (ok)
                {
                    _logger.LogInfo($"DeleteProduct: productId={id}");
                    CaptureAuditTrail(GetCallerEmail(), "Delete Product", $"Deleted product {id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Product deleted successfully"
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to delete product" });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteProduct: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            _logger.LogInfo("******* GET CATEGORIES REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("product_categories", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCategories: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("categories")]
        public IActionResult AddCategory([FromBody] ProductCategoryModel model)
        {
            _logger.LogInfo("******* ADD CATEGORY REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || string.IsNullOrEmpty(model.name))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Category name is required" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddCategory(model);
                if (ok && model.id > 0)
                {
                    _logger.LogInfo($"AddCategory: categoryId={model.id}");
                    CaptureAuditTrail(GetCallerEmail(), "Add Category", $"Added category: {model.name}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Category added successfully",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to add category" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddCategory: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("low-stock")]
        public IActionResult GetLowStockProducts()
        {
            _logger.LogInfo("******* GET LOW STOCK PRODUCTS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("low_stock_products", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetLowStockProducts: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("expiring")]
        public IActionResult GetExpiringBatches()
        {
            _logger.LogInfo("******* GET EXPIRING BATCHES REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("expiring_batches", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetExpiringBatches: " + ex.Message + " - " + ex.StackTrace);
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
