using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/menus")]
    public class MenuController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public MenuController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetMenu([FromQuery] string pageaccessed = "")
        {
            _logger.LogInfo("******* GET MENU REQUEST **********");
            try
            {
                var roleId = GetCallerRoleId();
                var userId = GetCallerUserId();

                if (roleId == 0)
                    return Unauthorized(new ApiResponse<object> { success = false, message = "Invalid or missing role_id in token" });

                if (userId == 0)
                    return Unauthorized(new ApiResponse<object> { success = false, message = "Invalid or missing user_id in token" });

                DataTable dt = dbhandler.GetRecords("menu_access", roleId.ToString(), pageaccessed);

                var menus = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    menus.Add(new
                    {
                        menu_id = row["menu_id"] != DBNull.Value ? Convert.ToInt64(row["menu_id"]) : 0,
                        menu_name = row["menu_name"]?.ToString() ?? "",
                        menu_url = row["menu_url"]?.ToString() ?? "",
                        icon = row["icon"]?.ToString() ?? "",
                        parent_id = row["parent_id"] != DBNull.Value ? Convert.ToInt64(row["parent_id"]) : 0,
                        sort_order = row["sort_order"] != DBNull.Value ? Convert.ToInt32(row["sort_order"]) : 0,
                        is_visible = row["is_visible"] != DBNull.Value ? Convert.ToBoolean(row["is_visible"]) : true
                    });
                }

                _logger.LogInfo($"GetMenu: roleId={roleId} pageaccessed={pageaccessed} menus={menus.Count}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    data = new
                    {
                        user_id = userId,
                        role_id = roleId,
                        pharmacy_id = GetCallerPharmacyId(),
                        email = GetCallerEmail(),
                        menu = menus
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetMenu: " + ex.Message + " - " + ex.StackTrace);
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
