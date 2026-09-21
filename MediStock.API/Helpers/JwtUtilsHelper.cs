using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MediStock.API.Helpers
{
    public class JwtUtilsHelper
    {
        public class JwtUtilsHandler
        {
            private readonly ILoggerManager iloggermanager;
            private readonly IConfiguration iconfiguration;

            public JwtUtilsHandler(ILoggerManager logger, IConfiguration configuration)
            {
                iloggermanager = logger;
                iconfiguration = configuration;
            }

            public string GenerateAccessToken(JObject jobject)
            {
                string? secret = iconfiguration["Jwt:Key"];
                if (string.IsNullOrEmpty(secret))
                {
                    iloggermanager.LogError("GenerateAccessToken: JWT Key is missing in configuration");
                    throw new InvalidOperationException("JWT Key is not configured");
                }

                int expiryMinutes = iconfiguration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);

                JwtSecurityTokenHandler tokenHandler = new();
                byte[] key = Encoding.UTF8.GetBytes(secret);

                List<Claim> claims = new()
                {
                    new Claim("pharmacy_id", jobject["pharmacy_id"]?.ToString() ?? "0"),
                    new Claim("user_id",     jobject["user_id"]!.ToString()),
                    new Claim("email",       jobject["email"]!.ToString()),
                    new Claim("role_id",     jobject["role_id"]!.ToString()),
                    new Claim("profile_id",  jobject["role_id"]!.ToString()),
                };

                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                    Issuer = iconfiguration["Jwt:Issuer"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature),
                };

                SecurityToken jwtToken = tokenHandler.CreateToken(tokenDescriptor);
                string token = tokenHandler.WriteToken(jwtToken);

                iloggermanager.LogInfo($"GenerateAccessToken: Generated token for email={jobject["email"]}, role_id={jobject["role_id"]}, pharmacy_id={jobject["pharmacy_id"]}");
                return token;
            }

            public string GenerateRefreshToken()
            {
                return Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();
            }
        }
    }
}
