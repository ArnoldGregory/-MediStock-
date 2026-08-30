using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using MediStock.API.Helpers;
using MediStock.API.Models;
using MediStock.API.Services;
using System.Data;
using System.Text.Json;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/mpesa")]
    public class MpesaController : Controller
    {
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;
        private readonly MpesaService mpesa;

        public MpesaController(ILoggerManager logger, DBHandler mydbhandler, MpesaService mpesaService)
        {
            iloggermanager = logger;
            dbhandler = mydbhandler;
            mpesa = mpesaService;
        }

        [Authorize]
        [HttpPost("stkpush")]
        public async Task<ActionResult> StkPush([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* M-PESA STK PUSH REQUEST **********");
            try
            {
                if (jobject == null) return Bad("Invalid request");

                var (userId, pharmacyId, _) = GetCaller();
                string phone = jobject["phone"]?.ToString()?.Trim() ?? "";
                decimal amount = jobject["amount"] != null ? Convert.ToDecimal(jobject["amount"]) : 0m;
                string accountRef = jobject["account_reference"]?.ToString()?.Trim() ?? "";
                if (accountRef.Length > 12) accountRef = accountRef[..12];
                string description = jobject["transaction_desc"]?.ToString()?.Trim() ?? "Pharmacy payment";
                if (description.Length > 128) description = description[..128];

                if (string.IsNullOrEmpty(phone)) return Bad("Phone number is required");
                if (amount <= 0) return Bad("Amount must be greater than 0");
                if (pharmacyId <= 0) return Bad("Pharmacy context is required");

                var creds = mpesa.GetCredentials();
                if (!creds.IsConfigured)
                    return Bad("M-Pesa is not configured. Add Daraja credentials in Settings.");

                string phone254 = MpesaService.FormatPhone(phone);
                long paymentId = dbhandler.AddMpesaPayment(pharmacyId, userId, phone254, amount, accountRef, description);
                if (paymentId <= 0) return Bad("Could not log the payment. Try again.");

                var result = await mpesa.StkPushAsync(phone254, amount, accountRef, description);

                if (!result.Success)
                {
                    dbhandler.SetMpesaCheckoutStatus(paymentId, null, null, "Failed");
                    return Ok(new { success = false, message = result.ErrorMessage ?? "STK push failed.", action = "", data = new JObject() });
                }

                dbhandler.SetMpesaCheckoutStatus(paymentId, result.CheckoutRequestId, result.MerchantRequestId, "Pending");

                return Ok(new
                {
                    success = true,
                    message = "M-Pesa prompt sent. Check your phone and enter your PIN.",
                    action = "VerifyPayment",
                    data = new JObject
                    {
                        { "payment_id", paymentId.ToString() },
                        { "checkout_request_id", result.CheckoutRequestId },
                        { "merchant_request_id", result.MerchantRequestId }
                    }
                });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("MpesaController.StkPush: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return ServerError();
            }
        }

        [Authorize]
        [HttpGet("status")]
        public ActionResult Status([FromQuery] string checkout_request_id)
        {
            iloggermanager.LogInfo("******* M-PESA STATUS REQUEST **********");
            try
            {
                if (string.IsNullOrWhiteSpace(checkout_request_id)) return Bad("checkout_request_id is required");

                DataTable dt = dbhandler.GetMpesaByCheckout(checkout_request_id.Trim());
                if (dt.Rows.Count == 0)
                    return Ok(new { success = true, message = "Payment not found", action = "", data = new JObject() });

                return Ok(new { success = true, message = "Success", action = "", data = JToken.FromObject(dt) });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("MpesaController.Status: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return ServerError();
            }
        }

        [Authorize]
        [HttpGet("payments")]
        public ActionResult Payments()
        {
            iloggermanager.LogInfo("******* M-PESA PAYMENTS LIST REQUEST **********");
            try
            {
                var (_, pharmacyId, _) = GetCaller();
                DataTable dt = dbhandler.ListMpesaPayments(pharmacyId);
                return Ok(new { success = true, message = "Success", action = "", data = JToken.FromObject(dt) });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("MpesaController.Payments: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return ServerError();
            }
        }

        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<ActionResult> Callback()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                string body = await reader.ReadToEndAsync();
                iloggermanager.LogInfo("M-Pesa Callback body: " + body);

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (!root.TryGetProperty("Body", out var b) || !b.TryGetProperty("stkCallback", out var stk))
                    return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });

                string checkoutId = stk.TryGetProperty("CheckoutRequestID", out var cid) ? cid.GetString() ?? "" : "";
                int stkCode = stk.TryGetProperty("ResultCode", out var rCode) ? rCode.GetInt32() : -1;
                string stkDesc = stk.TryGetProperty("ResultDesc", out var rDesc) ? rDesc.GetString() ?? "" : "";

                if (string.IsNullOrEmpty(checkoutId))
                    return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });

                string? receipt = null;
                decimal paidAmount = 0m;

                if (stkCode == 0 &&
                    stk.TryGetProperty("CallbackMetadata", out var meta) &&
                    meta.TryGetProperty("Item", out var items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        string name = item.TryGetProperty("Name", out var n) ? n.GetString() ?? "" : "";
                        if (name == "MpesaReceiptNumber" && item.TryGetProperty("Value", out var rv))
                            receipt = rv.GetString();
                        if (name == "Amount" && item.TryGetProperty("Value", out var av))
                            paidAmount = av.GetDecimal();
                    }
                }

                DataTable row = dbhandler.GetMpesaByCheckout(checkoutId);
                dbhandler.UpdateMpesaFromCallback(checkoutId, stkCode, stkDesc, receipt, paidAmount);

                if (row.Rows.Count > 0 && stkCode == 0)
                {
                    dbhandler.AddNotification(
                        Convert.ToInt64(row.Rows[0]["pharmacy_id"]),
                        0,
                        "M-Pesa payment received",
                        $"KES {paidAmount:N0} received. Receipt: {receipt}.",
                        "Payment");
                }

                return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("MpesaController.Callback: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });
            }
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
        private ActionResult Bad(string msg) =>
            StatusCode(StatusCodes.Status400BadRequest, new { success = false, message = msg, action = "", data = new JObject() });

        [NonAction]
        private ActionResult ServerError() =>
            StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error", action = "", data = new JObject() });
    }
}