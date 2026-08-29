// ============================================================
//  MediStock.Portal — ApiClient
//  Place in: Services/ApiClient.cs
// ============================================================

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;

namespace MediStock.Portal.Services
{
    public sealed class ApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ApiClient(IHttpClientFactory factory, IHttpContextAccessor ctx)
        {
            _factory = factory;
            _ctx = ctx;
        }

        // ── Token management ──────────────────────────────────────────────────
        public void SaveTokens(string accessToken, string refreshToken)
        {
            _ctx.HttpContext!.Session.SetString("AccessToken", accessToken);
            _ctx.HttpContext!.Session.SetString("RefreshToken", refreshToken);
        }

        public void SaveTempTokens(string accessToken, string refreshToken)
        {
            _ctx.HttpContext!.Session.SetString("TempAccessToken", accessToken);
            _ctx.HttpContext!.Session.SetString("TempRefreshToken", refreshToken);
        }

        public void ClearTempTokens()
        {
            _ctx.HttpContext?.Session.Remove("TempAccessToken");
            _ctx.HttpContext?.Session.Remove("TempRefreshToken");
        }

        public string? GetAccessToken() => _ctx.HttpContext?.Session.GetString("AccessToken");
        public string? GetRefreshToken() => _ctx.HttpContext?.Session.GetString("RefreshToken");
        public string? GetTempAccessToken() => _ctx.HttpContext?.Session.GetString("TempAccessToken");

        public void ClearTokens() => _ctx.HttpContext?.Session.Clear();

        // ── AUTH API ──────────────────────────────────────────────────────────
        public Task<ApiResult<T>> AuthPostAsync<T>(string endpoint, object body)
            => RawPostAsync<T>("AuthApi", endpoint, body, token: null);

        public Task<ApiResult<T>> AuthPostAuthAsync<T>(string endpoint, object body)
            => RawPostAsync<T>("AuthApi", endpoint, body, token: GetAccessToken());

        public Task<ApiResult<T>> AuthPostWithTokenAsync<T>(string endpoint, object body, string? token)
            => RawPostAsync<T>("AuthApi", endpoint, body, token: token);

        // ── MAIN API (silent refresh on 401) ──────────────────────────────────
        public Task<ApiResult<T>> GetAsync<T>(string endpoint)
            => SendWithRefreshAsync<T>("MainApi", HttpMethod.Get, endpoint, null);

        public Task<ApiResult<T>> PostAsync<T>(string endpoint, object body)
            => SendWithRefreshAsync<T>("MainApi", HttpMethod.Post, endpoint, body);

        // Multipart file upload to the Main API.
        public Task<ApiResult<T>> PostFileAsync<T>(string endpoint, string fileName, byte[] fileBytes)
            => SendFileWithRefreshAsync<T>("MainApi", endpoint, fileName, fileBytes);

        public Task<ApiResult<T>> PutAsync<T>(string endpoint, object body)
            => SendWithRefreshAsync<T>("MainApi", HttpMethod.Put, endpoint, body);

        public Task<ApiResult<T>> DeleteAsync<T>(string endpoint)
            => SendWithRefreshAsync<T>("MainApi", HttpMethod.Delete, endpoint, null);

        /// <summary>
        /// Downloads a file from the API.
        /// Returns (bytes, contentType, fileName) on success, or (null, null, null, error) on failure.
        /// </summary>
        public async Task<(byte[]? Bytes, string? ContentType, string? FileName, string? Error)> GetFileAsync(string endpoint)
        {
            var response = await DoSendAsync("MainApi", HttpMethod.Get, endpoint, null, GetAccessToken());

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshed = await TryRefreshAsync();
                if (!refreshed)
                {
                    await ForceLogoutAsync();
                    return (null, null, null, "Session expired");
                }
                response = await DoSendAsync("MainApi", HttpMethod.Get, endpoint, null, GetAccessToken());
            }

            if (!response.IsSuccessStatusCode)
                return (null, null, null, response.ReasonPhrase ?? "Request failed");

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                        ?? response.Content.Headers.ContentDisposition?.FileNameStar
                        ?? "download";
            return (bytes, contentType, fileName, null);
        }

        private async Task<ApiResult<T>> SendWithRefreshAsync<T>(
            string clientName, HttpMethod method, string endpoint, object? body)
        {
            var response = await DoSendAsync(clientName, method, endpoint, body, GetAccessToken());

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshed = await TryRefreshAsync();
                if (!refreshed)
                {
                    await ForceLogoutAsync();
                    return ApiResult<T>.Fail("Session expired. Please log in again.", 401);
                }
                response = await DoSendAsync(clientName, method, endpoint, body, GetAccessToken());
            }

            return await ParseAsync<T>(response);
        }

        private async Task<ApiResult<T>> SendFileWithRefreshAsync<T>(
            string clientName, string endpoint, string fileName, byte[] fileBytes)
        {
            var response = await DoSendFileAsync(clientName, endpoint, fileName, fileBytes, GetAccessToken());

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshed = await TryRefreshAsync();
                if (!refreshed)
                {
                    await ForceLogoutAsync();
                    return ApiResult<T>.Fail("Session expired. Please log in again.", 401);
                }
                response = await DoSendFileAsync(clientName, endpoint, fileName, fileBytes, GetAccessToken());
            }

            return await ParseAsync<T>(response);
        }

        private async Task<HttpResponseMessage> DoSendFileAsync(
            string clientName, string endpoint, string fileName, byte[] fileBytes, string? token)
        {
            var client = BuildClient(clientName, token);
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(fileBytes), "file", fileName);
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            return await client.SendAsync(request);
        }

        private async Task<bool> TryRefreshAsync()
        {
            var refreshToken = GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            try
            {
                var client = _factory.CreateClient("AuthApi");
                var body = JsonSerializer.Serialize(new { refreshToken });
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/auth/refresh", content);
                if (!response.IsSuccessStatusCode) return false;

                var raw = await response.Content.ReadAsStringAsync();
                var wrapper = JsonSerializer.Deserialize<ApiResponse<RefreshTokenResponse>>(raw, _json);
                if (wrapper?.Data is null) return false;

                SaveTokens(wrapper.Data.AccessToken, wrapper.Data.RefreshToken);
                return true;
            }
            catch { return false; }
        }

        private async Task ForceLogoutAsync()
        {
            ClearTokens();
            var ctx = _ctx.HttpContext;
            if (ctx is null) return;
            await ctx.SignOutAsync("Cookies");
            ctx.Items["SessionExpired"] = true;
        }

        private async Task<HttpResponseMessage> DoSendAsync(
            string clientName, HttpMethod method, string endpoint, object? body, string? token)
        {
            var client = BuildClient(clientName, token);
            var request = new HttpRequestMessage(method, endpoint);
            if (body is not null)
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            return await client.SendAsync(request);
        }

        private async Task<ApiResult<T>> RawPostAsync<T>(
            string clientName, string endpoint, object body, string? token)
        {
            try
            {
                var client = BuildClient(clientName, token);
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                return await ParseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResult<T>.Fail($"API connection failed: {ex.Message}", 0);
            }
        }

        private HttpClient BuildClient(string name, string? token)
        {
            var client = _factory.CreateClient(name);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static async Task<ApiResult<T>> ParseAsync<T>(HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var err = JsonSerializer.Deserialize<ApiErrorResponse>(raw, _json);
                    return ApiResult<T>.Fail(
                        err?.Message ?? response.ReasonPhrase ?? "Request failed",
                        (int)response.StatusCode);
                }
                catch
                {
                    return ApiResult<T>.Fail(response.ReasonPhrase ?? "Request failed", (int)response.StatusCode);
                }
            }

            try
            {
                var wrapper = JsonSerializer.Deserialize<ApiResponse<T>>(raw, _json);
                return ApiResult<T>.Ok(wrapper!.Data, wrapper.Action);
            }
            catch
            {
                return ApiResult<T>.Fail("Failed to parse API response.", (int)response.StatusCode);
            }
        }
    }

    // ── Envelope ──────────────────────────────────────────────────────────────
    public sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Action { get; set; }
        public T? Data { get; set; }
    }

    public sealed class ApiErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string[]? Errors { get; set; }
        public int StatusCode { get; set; }
    }

    public sealed class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }

    public sealed class ApiResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? Action { get; private set; }
        public string Error { get; private set; } = string.Empty;
        public int StatusCode { get; private set; }

        public static ApiResult<T> Ok(T? data, string? action = null)
            => new() { IsSuccess = true, Data = data, Action = action };

        public static ApiResult<T> Fail(string error, int statusCode = 500)
            => new() { IsSuccess = false, Error = error, StatusCode = statusCode };
    }
}
