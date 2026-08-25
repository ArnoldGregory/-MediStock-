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

        private static readonly Dictionary<string, string> _menuIcons = new()
        {
            { "Dashboard",  "fa-dashboard" },
            { "Inventory",  "fa-cube" },
            { "Sales",      "fa-shopping-cart" },
            { "Customers",  "fa-users" },
            { "Suppliers",  "fa-truck" },
            { "Finance",    "fa-money" },
            { "Reports",    "fa-bar-chart" },
            { "Clinical",   "fa-heartbeat" },
            { "DDA",        "fa-balance-scale" },
            { "Settings",   "fa-cog" },
            { "Admin",      "fa-user-secret" },
        };

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
                var pharmacyId = GetCallerPharmacyId();
                var email = GetCallerEmail();

                if (roleId == 0)
                    return Unauthorized(new ApiResponse<object> { success = false, message = "Invalid or missing role_id in token" });

                DataTable dt = dbhandler.GetMenuRecords(roleId);

                var mainMenus = new Dictionary<string, MenuMainNode>();

                foreach (DataRow row in dt.Rows)
                {
                    string mainMenuName = row["main_menu_name"]?.ToString() ?? "";
                    string subMenuName = row["sub_menu_name"]?.ToString() ?? "";
                    string pageUrl = row["page_url"]?.ToString() ?? "";
                    int menuOrder = row["menu_order"] != DBNull.Value ? Convert.ToInt32(row["menu_order"]) : 0;
                    int subMenuOrder = row["sub_menu_order"] != DBNull.Value ? Convert.ToInt32(row["sub_menu_order"]) : 0;

                    if (!mainMenus.ContainsKey(mainMenuName))
                    {
                        string icon = _menuIcons.ContainsKey(mainMenuName) ? _menuIcons[mainMenuName] : "fa-circle";
                        bool isSelected = pageUrl.TrimStart('~', '/') == pageaccessed.TrimStart('~', '/');

                        mainMenus[mainMenuName] = new MenuMainNode
                        {
                            MenuOrder = menuOrder,
                            MenuName = mainMenuName,
                            MenuIcon = icon,
                            MenuUrl = "",
                            MenuSelected = isSelected ? "active" : "",
                            SubMenus = new List<MenuSubNode>()
                        };
                    }

                    bool subSelected = pageUrl.TrimStart('~', '/') == pageaccessed.TrimStart('~', '/');
                    mainMenus[mainMenuName].SubMenus.Add(new MenuSubNode
                    {
                        SubMenuOrder = subMenuOrder,
                        SubMenuName = subMenuName,
                        SubMenuUrl = pageUrl,
                        SubMenuSelected = subSelected ? "active" : ""
                    });

                    if (subSelected)
                        mainMenus[mainMenuName].MenuSelected = "active";
                }

                var menuList = mainMenus.Values.OrderBy(m => m.MenuOrder).ToList();

                _logger.LogInfo($"GetMenu: roleId={roleId} pageaccessed={pageaccessed} menus={menuList.Count}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    data = new
                    {
                        user_id = userId,
                        role_id = roleId,
                        pharmacy_id = pharmacyId,
                        email = email,
                        menu = menuList
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

    public class MenuMainNode
    {
        public int MenuOrder { get; set; }
        public string MenuName { get; set; } = "";
        public string MenuIcon { get; set; } = "";
        public string MenuUrl { get; set; } = "";
        public string MenuSelected { get; set; } = "";
        public List<MenuSubNode> SubMenus { get; set; } = new();
    }

    public class MenuSubNode
    {
        public int SubMenuOrder { get; set; }
        public string SubMenuName { get; set; } = "";
        public string SubMenuUrl { get; set; } = "";
        public string SubMenuSelected { get; set; } = "";
    }
}
