using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/menus")]
    public class MenuController : ControllerBase
    {
        private readonly DBHandler dbhandler;

        public MenuController(IConfiguration config)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetMenu([FromQuery] string pageaccessed = "")
        {
            if (!int.TryParse(HttpContext.Items["profile_id"]?.ToString(), out int profileId) || profileId == 0)
                return Unauthorized(new { success = false, message = "Invalid or missing profile_id in token" });

            if (!int.TryParse(HttpContext.Items["user_id"]?.ToString(), out int userId) || userId == 0)
                return Unauthorized(new { success = false, message = "Invalid or missing user_id in token" });

            var roleType = HttpContext.Items["role_type"]?.ToString() ?? "CLIENT";

            MenuHandler handler = new(dbhandler);
            IList<MenuModel> menuList = handler.GetMenu(profileId, pageaccessed);

            return Ok(new
            {
                success = true,
                user_id = userId,
                profile_id = profileId,
                pharmacy_id = HttpContext.Items["pharmacy_id"]?.ToString(),
                email = HttpContext.Items["email"]?.ToString(),
                role_type = roleType,
                menu = menuList
            });
        }
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

        public MenuHandler(DBHandler mydbhandler)
        {
            dbhandler = mydbhandler;
        }

        public IList<MenuModel> GetMenu(int profileId, string pageaccessed)
        {
            // Step 1: Get main menus
            DataTable mainData = dbhandler.GetMenu(profileId, "main", "");
            List<MenuModel> menuList = new();

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

                    // Step 2: Get sub-menus for this main menu
                    DataTable subData = dbhandler.GetMenu(profileId, "sub", mainMenuName);
                    List<SubMenuModel> subList = new();

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
                        // Step 3: Standalone menu — get its page_url
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
