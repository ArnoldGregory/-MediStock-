// ============================================================
//  MediStock.Portal — MenuService
//  Place in: Services/MenuService.cs
//  Builds sidebar menu based on user role claims.
// ============================================================

using System.Security.Claims;

namespace MediStock.Portal.Services
{
    public sealed class MenuService
    {
        private readonly IHttpContextAccessor _ctx;

        public MenuService(IHttpContextAccessor ctx) { _ctx = ctx; }

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
    }
}
