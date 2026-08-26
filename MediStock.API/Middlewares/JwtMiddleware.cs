using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediStock.API.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace MediStock.API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public JwtMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var publicRoutes = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/verify-otp",
                "/api/auth/otpclientlogin",
                "/api/auth/forgot-password",
                "/api/auth/reset-password",
                "/api/auth/refresh",
                "/api/auth/check-slug",
                "/api/auth/check-email",
                "/swagger",
                "/swagger/index.html",
                "/swagger/v1/swagger.json"
            };

            if (publicRoutes.Any(r => path.StartsWith(r)) || path.StartsWith("/swagger"))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                try
                {
                    var jwtKey = _config["Jwt:Key"]!;
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                    var tokenHandler = new JwtSecurityTokenHandler();

                    var validationParams = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = true,
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    var principal = tokenHandler.ValidateToken(token, validationParams, out var validatedToken);
                    context.User = principal;

                    // Riziki pattern: extract claims to HttpContext.Items
                    ExtractClaimsToContext(context, token);
                }
                catch (SecurityTokenExpiredException)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
                catch (Exception)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
            }

            await _next(context);
        }

        /// <summary>
        /// Reads JWT claims and stores them in HttpContext.Items
        /// so any controller can access them via HttpContext.Items["user_id"] etc.
        /// </summary>
        private void ExtractClaimsToContext(HttpContext context, string token)
        {
            try
            {
                JwtSecurityTokenHandler handler = new();
                JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

                string? userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
                string? profileId = jwtToken.Claims.FirstOrDefault(c => c.Type == "role_id")?.Value
                                 ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "profile_id")?.Value;
                string? email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                string? pharmacyId = jwtToken.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value;

                // Derive role_type from profile_id (role_id)
                string roleType = "CLIENT";
                if (int.TryParse(profileId, out int rid))
                {
                    roleType = rid switch
                    {
                        1 => "SuperAdmin",
                        2 => "Admin",
                        3 => "Pharmacist",
                        4 => "Staff",
                        5 => "Cashier",
                        _ => "CLIENT"
                    };
                }

                if (!string.IsNullOrEmpty(userId)) context.Items["user_id"] = userId;
                if (!string.IsNullOrEmpty(profileId)) context.Items["profile_id"] = profileId;
                if (!string.IsNullOrEmpty(email)) context.Items["email"] = email;
                if (!string.IsNullOrEmpty(pharmacyId)) context.Items["pharmacy_id"] = pharmacyId;
                if (!string.IsNullOrEmpty(roleType)) context.Items["role_type"] = roleType;
            }
            catch (Exception)
            {
                // silently fail — context items stay unset
            }
        }
    }
}
