using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/menus")]
    public class MenuController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public MenuController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetMenu([FromQuery] string pageaccessed = "")
        {
            iloggermanager.LogInfo("******* GET MENU REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (!int.TryParse(HttpContext.Items["profile_id"]?.ToString(), out int profileId) || profileId == 0)
                {
                    iloggermanager.LogError("GetMenu: Invalid or missing profile_id in token");
                    return StatusCode(StatusCodes.Status401Unauthorized, new { success = false, message = "Invalid or missing profile_id in token", action = "", data = new JObject() });
                }

                if (!int.TryParse(HttpContext.Items["user_id"]?.ToString(), out int menuUserId) || menuUserId == 0)
                {
                    iloggermanager.LogError("GetMenu: Invalid or missing user_id in token");
                    return StatusCode(StatusCodes.Status401Unauthorized, new { success = false, message = "Invalid or missing user_id in token", action = "", data = new JObject() });
                }

                var roleType = HttpContext.Items["role_type"]?.ToString() ?? "CLIENT";
                iloggermanager.LogInfo($"GetMenu: profileId={profileId}, roleType={roleType}, pageaccessed={pageaccessed}");

                MenuHandler handler = new(dbhandler, iloggermanager);
                IList<MenuModel> menuList = handler.GetMenu(profileId, pageaccessed);

                iloggermanager.LogInfo($"GetMenu: returned {menuList.Count} main menus");

                return Ok(new
                {
                    success = true,
                    user_id = menuUserId,
                    profile_id = profileId,
                    pharmacy_id = HttpContext.Items["pharmacy_id"]?.ToString(),
                    email = HttpContext.Items["email"]?.ToString(),
                    role_type = roleType,
                    menu = menuList
                });
            }
            catch (Exception ex) { iloggermanager.LogError("GetMenu: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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
        private ActionResult ServerError() =>
            StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error", action = "", data = new JObject() });
    }

    // ── Menu models (Riziki pattern) ──────────────────────────────────────

    public class MenuModel
    {
        public int menu_order { get; set; }
        public string menu_name { get; set; } = "";
        public string menu_icon { get; set; } = "";
        public string menu_url { get; set; } = "";
        public string menu_selected { get; set; } = "";
        public List<SubMenuModel> sub_menus { get; set; } = new();
    }

    public class SubMenuModel
    {
        public int sub_menu_order { get; set; }
        public string sub_menu_name { get; set; } = "";
        public string sub_menu_url { get; set; } = "";
        public string sub_menu_selected { get; set; } = "";
    }

    public class MenuHandler
    {
        private readonly DBHandler dbhandler;
        private readonly ILoggerManager _logger;

        public MenuHandler(DBHandler mydbhandler, ILoggerManager logger)
        {
            dbhandler = mydbhandler;
            _logger = logger;
        }

        public IList<MenuModel> GetMenu(int profileId, string pageaccessed)
        {
            DataTable mainData = dbhandler.GetMenu(profileId, "main", "");
            List<MenuModel> menuList = new();

            _logger.LogInfo($"MenuHandler.GetMenu: main menus returned {mainData.Rows.Count} rows");

            if (mainData.Rows.Count > 0)
            {
                for (int i = 0; i < mainData.Rows.Count; i++)
                {
                    string mainMenuName = mainData.Rows[i]["main_menu_name"].ToString() ?? "";
                    string menuIcon = mainData.Rows[i]["menu_icon"].ToString() ?? "fa-circle";
                    int menuOrder = Convert.ToInt32(mainData.Rows[i]["menu_order"]);

                    MenuModel menu = new MenuModel
                    {
                        menu_order = menuOrder,
                        menu_name = mainMenuName,
                        menu_icon = menuIcon
                    };

                    DataTable subData = dbhandler.GetMenu(profileId, "sub", mainMenuName);
                    List<SubMenuModel> subList = new();

                    _logger.LogInfo($"MenuHandler.GetMenu: '{mainMenuName}' has {subData.Rows.Count} sub-menus");

                    if (subData.Rows.Count > 0)
                    {
                        for (int j = 0; j < subData.Rows.Count; j++)
                        {
                            SubMenuModel sub = new SubMenuModel
                            {
                                sub_menu_order = Convert.ToInt32(subData.Rows[j]["sub_menu_order"]),
                                sub_menu_name = subData.Rows[j]["sub_menu_name"].ToString() ?? "",
                                sub_menu_url = subData.Rows[j]["page_url"].ToString() ?? ""
                            };

                            if (pageaccessed == sub.sub_menu_url.Replace("~", ""))
                            {
                                sub.sub_menu_selected = "active";
                                menu.menu_selected = "active";
                            }

                            subList.Add(sub);
                        }
                        menu.menu_url = "#";
                        menu.sub_menus = subList;
                    }
                    else
                    {
                        menu.menu_url = dbhandler.GetScalarItem(
                            $"call get_menu({profileId}, 'page_url', '{mainMenuName.Replace("'", "''")}')");

                        if (pageaccessed == menu.menu_url.Replace("~", ""))
                            menu.menu_selected = "active";
                    }

                    menuList.Add(menu);
                }
            }

            return menuList;
        }
    }
}
