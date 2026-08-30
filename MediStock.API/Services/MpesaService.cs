using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MediStock.API.Helpers;

namespace MediStock.API.Services
{
    public class MpesaCredentials
    {
        public string ConsumerKey { get; set; } = "";
        public string ConsumerSecret { get; set; } = "";
        public string PassKey { get; set; } = "";
        public string ShortCode { get; set; } = "";
        public string CallbackUrl { get; set; } = "";
        public string Environment { get; set; } = "sandbox";
        public string ShortCodeType { get; set; } = "Paybill";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ConsumerKey) &&
            !string.IsNullOrWhiteSpace(ConsumerSecret) &&
            !string.IsNullOrWhiteSpace(PassKey) &&
            !string.IsNullOrWhiteSpace(ShortCode) &&
            !string.IsNullOrWhiteSpace(CallbackUrl);

        public string BaseUrl => Environment == "production"
            ? "https://api.safaricom.co.ke"
            : "https://sandbox.safaricom.co.ke";
    }

    public class StkPushResult
    {
        public bool Success { get; set; }
        public string? CheckoutRequestId { get; set; }
        public string? MerchantRequestId { get; set; }
        public string? ResponseCode { get; set; }
        public string? ResponseDescription { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MpesaService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public MpesaService(IHttpClientFactory httpFactory, IConfiguration config, ILoggerManager logger)
        {
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;
        }

        public MpesaCredentials GetCredentials()
        {
            return new MpesaCredentials
            {
                ConsumerKey = _config["Mpesa:ConsumerKey"] ?? "",
                ConsumerSecret = _config["Mpesa:ConsumerSecret"] ?? "",
                PassKey = _config["Mpesa:PassKey"] ?? "",
                ShortCode = _config["Mpesa:ShortCode"] ?? "",
                CallbackUrl = _config["Mpesa:CallbackUrl"] ?? "",
                Environment = _config["Mpesa:Environment"] ?? "sandbox",
                ShortCodeType = _config["Mpesa:ShortCodeType"] ?? "Paybill"
            };
        }

        public async Task<string?> GetAccessTokenAsync(MpesaCredentials creds)
        {
            try
            {
                var client = _httpFactory.CreateClient("MpesaClient");
                var encoded = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{creds.ConsumerKey}:{creds.ConsumerSecret}"));

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", encoded);

                var response = await client.GetAsync(
                    $"{creds.BaseUrl}/oauth/v1/generate?grant_type=client_credentials");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"MpesaService.GetAccessToken: HTTP {(int)response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("access_token").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError("MpesaService.GetAccessToken: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return null;
            }
        }

        public async Task<StkPushResult> StkPushAsync(
            string phone,
            decimal amount,
            string accountRef,
            string description)
        {
            var creds = GetCredentials();
            return await StkPushWithCredsAsync(creds, phone, amount, accountRef, description);
        }

        public async Task<StkPushResult> StkPushWithCredsAsync(
            MpesaCredentials creds,
            string phone,
            decimal amount,
            string accountRef,
            string description)
        {
            try
            {
                if (!creds.IsConfigured)
                    return new StkPushResult
                    {
                        Success = false,
                        ErrorMessage = "M-Pesa is not configured. Please add Daraja credentials in Settings."
                    };

                var token = await GetAccessTokenAsync(creds);
                if (token == null)
                    return new StkPushResult
                    {
                        Success = false,
                        ErrorMessage = "Could not authenticate with M-Pesa. Check your Consumer Key and Secret."
                    };

                phone = FormatPhone(phone);

                var timestamp = NairobiNow().ToString("yyyyMMddHHmmss");
                var password = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    $"{creds.ShortCode}{creds.PassKey}{timestamp}"));

                var payload = new
                {
                    BusinessShortCode = creds.ShortCode,
                    Password = password,
                    Timestamp = timestamp,
                    TransactionType = creds.ShortCodeType == "Till" ? "CustomerBuyGoodsOnline" : "CustomerPayBillOnline",
                    Amount = (int)Math.Ceiling(amount),
                    PartyA = phone,
                    PartyB = creds.ShortCode,
                    PhoneNumber = phone,
                    CallBackURL = creds.CallbackUrl,
                    AccountReference = accountRef,
                    TransactionDesc = description
                };

                var client = _httpFactory.CreateClient("MpesaClient");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(
                    JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    $"{creds.BaseUrl}/mpesa/stkpush/v1/processrequest", content);

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("ResponseCode", out var rc) &&
                    rc.GetString() == "0")
                {
                    return new StkPushResult
                    {
                        Success = true,
                        CheckoutRequestId = doc.RootElement.GetProperty("CheckoutRequestID").GetString(),
                        MerchantRequestId = doc.RootElement.GetProperty("MerchantRequestID").GetString(),
                        ResponseCode = "0",
                        ResponseDescription = doc.RootElement.GetProperty("ResponseDescription").GetString()
                    };
                }

                var errMsg = doc.RootElement.TryGetProperty("errorMessage", out var em)
                    ? em.GetString()
                    : doc.RootElement.TryGetProperty("ResponseDescription", out var rd)
                        ? rd.GetString()
                        : "STK push failed.";

                return new StkPushResult { Success = false, ErrorMessage = errMsg };
            }
            catch (Exception ex)
            {
                _logger.LogError("MpesaService.StkPush: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return new StkPushResult
                {
                    Success = false,
                    ErrorMessage = "Server error initiating M-Pesa payment."
                };
            }
        }

        public static string FormatPhone(string phone)
        {
            phone = (phone ?? "").Trim().Replace(" ", "").Replace("-", "");
            if (phone.StartsWith("0")) phone = "254" + phone[1..];
            if (phone.StartsWith("+")) phone = phone[1..];
            return phone;
        }

        public static DateTime NairobiNow()
        {
            return DateTime.UtcNow.AddHours(3);
        }
    }
}