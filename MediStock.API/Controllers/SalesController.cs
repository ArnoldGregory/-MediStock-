using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public SalesController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetSales()
        {
            _logger.LogInfo("******* GET SALES REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("sales", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSales: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetSaleById(Int64 id)
        {
            _logger.LogInfo("******* GET SALE BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("sale", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Sale not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSaleById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("{id}/items")]
        public IActionResult GetSaleItems(Int64 id)
        {
            _logger.LogInfo("******* GET SALE ITEMS REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("sale_items", id.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSaleItems: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateSale([FromBody] SaleModel model)
        {
            _logger.LogInfo("******* CREATE SALE REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid sale data" });

                if (model.items == null || model.items.Count == 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Sale must have at least one item" });

                model.pharmacy_id = pharmacyId;
                model.sold_by = userId;

                bool saleOk = dbhandler.AddSale(model);
                if (!saleOk || model.id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Failed to create sale" });

                bool itemsOk = dbhandler.AddSaleItems(model.id, model.items);
                if (!itemsOk)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Sale created but failed to add items" });

                _logger.LogInfo($"CreateSale: saleId={model.id}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Sale created successfully",
                    data = new { id = model.id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateSale: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("products")]
        public IActionResult GetProductsForPOS()
        {
            _logger.LogInfo("******* GET PRODUCTS FOR POS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("products", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProductsForPOS: " + ex.Message + " - " + ex.StackTrace);
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
