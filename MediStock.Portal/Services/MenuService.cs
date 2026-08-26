// ============================================================
//  MediStock.Portal — MenuService
//  Matches Riziki pattern exactly.
// ============================================================

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediStock.Portal.Services
{
    public sealed class MenuService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ApiClient _api;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public MenuService(IHttpClientFactory factory, ApiClient api)
        {
            _factory = factory;
            _api = api;
        }

        public async Task<List<MenuItem>> GetMenuAsync(string pageAccessed = "")
        {
            try
            {
                var token = _api.GetAccessToken();
                if (string.IsNullOrEmpty(token)) return new();

                var client = _factory.CreateClient("MainApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await client.GetAsync($"api/menus?pageaccessed={Uri.EscapeDataString(pageAccessed)}");
                if (!resp.IsSuccessStatusCode) return new();

                var raw = await resp.Content.ReadAsStringAsync();
                var env = JsonSerializer.Deserialize<MenuEnvelope>(raw, _json);
                return env?.Menu ?? new();
            }
            catch
            {
                return new();
            }
        }
    }

    public sealed class MenuEnvelope
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("menu")] public List<MenuItem>? Menu { get; set; }
    }

    public sealed class MenuItem
    {
        [JsonPropertyName("menu_order")] public int MenuOrder { get; set; }
        [JsonPropertyName("menu_name")] public string? MenuName { get; set; }
        [JsonPropertyName("menu_icon")] public string? MenuIcon { get; set; }
        [JsonPropertyName("menu_url")] public string? MenuUrl { get; set; }
        [JsonPropertyName("menu_selected")] public string? MenuSelected { get; set; }
        [JsonPropertyName("sub_menus")] public List<SubMenuItem>? SubMenus { get; set; }
    }

    public sealed class SubMenuItem
    {
        [JsonPropertyName("sub_menu_order")] public int SubMenuOrder { get; set; }
        [JsonPropertyName("sub_menu_name")] public string? SubMenuName { get; set; }
        [JsonPropertyName("sub_menu_url")] public string? SubMenuUrl { get; set; }
        [JsonPropertyName("sub_menu_selected")] public string? SubMenuSelected { get; set; }
    }
}
