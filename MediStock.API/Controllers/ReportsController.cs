using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public ReportsController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("sales")]
        public IActionResult GetSalesReport([FromQuery] string? from_date, [FromQuery] string? to_date)
        {
            _logger.LogInfo("******* GET SALES REPORT REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                string p1 = pharmacyId.ToString();
                string p2 = from_date ?? "";
                string p3 = to_date ?? "";
                DataTable dt = dbhandler.GetRecords("report_sales", p1, p2, p3);
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSalesReport: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("stock")]
        public IActionResult GetStockReport()
        {
            _logger.LogInfo("******* GET STOCK REPORT REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("report_stock", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetStockReport: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("financial")]
        public IActionResult GetFinancialReport([FromQuery] string? from_date, [FromQuery] string? to_date)
        {
            _logger.LogInfo("******* GET FINANCIAL REPORT REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                string p1 = pharmacyId.ToString();
                string p2 = from_date ?? "";
                string p3 = to_date ?? "";
                DataTable dt = dbhandler.GetRecords("report_financial", p1, p2, p3);
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetFinancialReport: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("margins")]
        public IActionResult GetProductMargins()
        {
            _logger.LogInfo("******* GET PRODUCT MARGINS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("report_product_margins", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProductMargins: " + ex.Message + " - " + ex.StackTrace);
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
