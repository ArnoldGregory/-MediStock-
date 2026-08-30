using System.Net;
using System.Net.Mail;
using MediStock.API.Helpers;

namespace MediStock.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public EmailService(IConfiguration config, ILoggerManager logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_config["Email:Password"]) &&
            !string.IsNullOrWhiteSpace(_config["Email:Host"]);

        public void SendOtp(string to, string recipientName, string otp, string purpose)
        {
            string subject = $"Your MediStock {purpose} code is {otp}";
            string bodyHtml =
                "<div style='font-family:Arial,sans-serif;max-width:520px;margin:auto;border:1px solid #e3e6ec;border-radius:10px;padding:24px;'>" +
                "<h2 style='color:#1b7a5a;margin:0 0 8px;'>MediStock</h2>" +
                $"<p>Hi {WebUtility.HtmlEncode(recipientName)},</p>" +
                $"<p>Use the code below to complete your <b>{WebUtility.HtmlEncode(purpose)}</b>:</p>" +
                $"<div style='background:#f2f7f5;border-radius:8px;padding:14px;font-size:28px;font-weight:bold;letter-spacing:6px;text-align:center;color:#1b7a5a;'>{otp}</div>" +
                "<p style='color:#6b7280;font-size:12px;'>This code expires shortly. If you did not request it, ignore this email.</p>" +
                "</div>";
            string bodyText = $"MediStock {purpose} code for {recipientName}: {otp}. It expires shortly. If you did not request it, ignore this email.";

            _ = Task.Run(async () => await TrySendAsync(to, subject, bodyHtml, bodyText));
        }

        private async Task TrySendAsync(string to, string subject, string bodyHtml, string bodyText)
        {
            try
            {
                string mode = _config["Email:Mode"] ?? "Screen";
                if (mode.Equals("Screen", StringComparison.OrdinalIgnoreCase) || !IsConfigured)
                {
                    _logger.LogInfo($"[EmailService] Screen mode: to={to} subject='{subject}' body='{bodyText}'");
                    return;
                }

                using var smtp = new SmtpClient(_config["Email:Host"], _config.GetValue<int>("Email:Port", 587))
                {
                    EnableSsl = _config.GetValue<bool>("Email:EnableSsl", true),
                    Credentials = new NetworkCredential(_config["Email:UserName"], _config["Email:Password"]),
                    Timeout = 20000
                };

                var from = _config["Email:From"] ?? _config["Email:UserName"];
                using var message = new MailMessage(from!, to, subject, bodyText)
                {
                    IsBodyHtml = true,
                    Body = bodyHtml
                };

                await smtp.SendMailAsync(message);
                _logger.LogInfo($"[EmailService] Sent email to {to} subject='{subject}'");
            }
            catch (Exception ex)
            {
                _logger.LogError("EmailService.TrySend: to='" + to + "' " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
        }
    }
}