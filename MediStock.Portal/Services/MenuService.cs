// ============================================================
//  MediStock.Portal — MenuService
//  Place in: Services/MenuService.cs
//  Builds sidebar menu based on user role claims.
// ============================================================

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace MediStock.Portal.Services
{
    public sealed class MenuService
    {
        private readonly IHttpClientFactory _http;
        private readonly ApiClient _api;
        private readonly IHttpContextAccessor _ctx;

        public MenuService(IHttpClientFactory http, ApiClient api, IHttpContextAccessor ctx)
        {
            _http = http;
            _api = api;
            _ctx = ctx;
        }

        public string GetUserRole()
        {
            return _ctx.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        public string GetPharmacyId()
        {
            return _ctx.HttpContext?.User?.FindFirst("pharmacy_id")?.Value ?? "0";
        }

        public string GetUserName()
        {
            return _ctx.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? "";
        }

        public string GetUserEmail()
        {
            return _ctx.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? "";
        }

        public async Task<List<MenuItem>> GetMenuAsync(string pageAccessed)
        {
            try
            {
                var token = _api.GetAccessToken();
                var client = _http.CreateClient("MainApi");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await client.GetAsync($"api/menus?pageaccessed={Uri.EscapeDataString(pageAccessed)}");
                if (!resp.IsSuccessStatusCode) return new List<MenuItem>();

                var json = await resp.Content.ReadAsStringAsync();
                var env = JsonSerializer.Deserialize<MenuEnvelope>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return env?.Data?.Menu ?? new List<MenuItem>();
            }
            catch
            {
                return new List<MenuItem>();
            }
        }
    }

    public class MenuEnvelope
    {
        public bool Success { get; set; }
        public MenuDataEnvelope? Data { get; set; }
    }

    public class MenuDataEnvelope
    {
        public List<MenuItem> Menu { get; set; } = new();
    }

    public class MenuItem
    {
        public int MenuOrder { get; set; }
        public string MenuName { get; set; } = "";
        public string MenuIcon { get; set; } = "";
        public string MenuUrl { get; set; } = "";
        public string MenuSelected { get; set; } = "";
        public List<SubMenuItem> SubMenus { get; set; } = new();
    }

    public class SubMenuItem
    {
        public int SubMenuOrder { get; set; }
        public string SubMenuName { get; set; } = "";
        public string SubMenuUrl { get; set; } = "";
        public string SubMenuSelected { get; set; } = "";
    }
}
