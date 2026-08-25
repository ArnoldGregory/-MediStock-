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
            // Public routes that don't require auth
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
                        ValidateAudience = true,
                        ValidAudience = _config["Jwt:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    var principal = tokenHandler.ValidateToken(token, validationParams, out var validatedToken);
                    context.User = principal;
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
    }
}
