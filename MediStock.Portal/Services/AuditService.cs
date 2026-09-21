// ============================================================
//  MediStock.Portal — AuditService
//  Place in: Services/AuditService.cs
// ============================================================

using System.Security.Claims;
using NLog;

namespace MediStock.Portal.Services
{
    public sealed class AuditService
    {
        private static readonly Logger _log = LogManager.GetLogger("MediStock.Portal.Audit");
        private readonly IHttpContextAccessor _ctx;

        public AuditService(IHttpContextAccessor ctx) { _ctx = ctx; }

        private string User =>
            _ctx.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
            ?? _ctx.HttpContext?.User?.Identity?.Name
            ?? "anonymous";

        private string Ip =>
            _ctx.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

        // ── Auth events ──
        public Task LogLoginAsync(string username, bool success, string? detail = null)
        {
            _log.Info($"LOGIN | user={username} | success={success} | ip={Ip} | {detail}");
            return Task.CompletedTask;
        }

        public Task LogOtpAsync(string username, bool success, string? detail = null)
        {
            _log.Info($"OTP | user={username} | success={success} | ip={Ip} | {detail}");
            return Task.CompletedTask;
        }

        public Task LogLogoutAsync()
        {
            _log.Info($"LOGOUT | user={User} | ip={Ip}");
            return Task.CompletedTask;
        }

        // ── CRUD/view events ──
        public Task LogViewAsync(string entity, string? detail = null)
        {
            _log.Info($"VIEW | {entity} | user={User} | {detail}");
            return Task.CompletedTask;
        }
    }
}
