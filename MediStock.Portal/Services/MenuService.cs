// ============================================================
//  MediStock.Portal — MenuService
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
        private readonly ILogger<MenuService> _log;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public MenuService(IHttpClientFactory factory, ApiClient api, ILogger<MenuService> log)
        {
            _factory = factory;
            _api = api;
            _log = log;
        }

        public async Task<List<MenuItem>> GetMenuAsync(string pageAccessed = "")
        {
            try
            {
                var token = _api.GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    _log.LogWarning("GetMenuAsync: No access token in session");
                    return new();
                }

                _log.LogInformation("GetMenuAsync: Calling api/menus with token={tokenPrefix}...", token[..Math.Min(20, token.Length)]);

                var client = _factory.CreateClient("MainApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await client.GetAsync($"api/menus?pageaccessed={Uri.EscapeDataString(pageAccessed)}");

                _log.LogInformation("GetMenuAsync: API responded with {statusCode}", resp.StatusCode);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    _log.LogWarning("GetMenuAsync: API returned {statusCode}: {body}", resp.StatusCode, body[..Math.Min(500, body.Length)]);
                    return new();
                }

                var raw = await resp.Content.ReadAsStringAsync();
                _log.LogInformation("GetMenuAsync: Raw response length={len}", raw.Length);

                var env = JsonSerializer.Deserialize<MenuEnvelope>(raw, _json);
                if (env?.Menu is null)
                {
                    _log.LogWarning("GetMenuAsync: Deserialized envelope is null or Menu is null. Raw={raw}", raw[..Math.Min(500, raw.Length)]);
                    return new();
                }

                _log.LogInformation("GetMenuAsync: Got {count} menus", env.Menu.Count);
                return env.Menu;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "GetMenuAsync: Exception");
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
