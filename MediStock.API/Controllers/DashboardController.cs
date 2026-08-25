using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public DashboardController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("summary")]
        public IActionResult GetDashboardSummary()
        {
            _logger.LogInfo("******* GET DASHBOARD SUMMARY REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetDashboardSummary(pharmacyId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        data = new DashboardSummary()
                    });
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

                _logger.LogInfo($"GetDashboardSummary: pharmacyId={pharmacyId}");
                return Ok(new ApiResponse<DashboardSummary>
                {
                    success = true,
                    data = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetDashboardSummary: " + ex.Message + " - " + ex.StackTrace);
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
