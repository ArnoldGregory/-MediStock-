using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;
using System.Security.Claims;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public AuthController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [HttpPost("clientlogin")]
        public IActionResult Login([FromBody] JObject jobject)
        {
            _logger.LogInfo("******* LOGIN REQUEST **********");
            _logger.LogInfo(jobject.ToString());

            try
            {
                string email = "";
                if (jobject.ContainsKey("email"))
                    email = jobject["email"]!.ToString().Trim();
                else if (jobject.ContainsKey("username"))
                    email = jobject["username"]!.ToString().Trim();

                string password = jobject["password"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Email and password are required" });

                DataTable dt = dbhandler.ValidateUserLogin("PHARMACY", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("USER", email);

                if (dt.Rows.Count == 0)
                {
                    _logger.LogInfo($"Login failed: User not found for email: {email}");
                    CaptureAuditTrail(email, "Invalid Login", "User not found");
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiResponse<object>
                    {
                        success = false,
                        message = "Invalid credentials"
                    });
                }

                string? storedPassword = dt.Rows[0]["password"]?.ToString() ?? "";

                var crypto = new CryptoHelper.MediSecurity.Rijndael();
                string decryptedPassword = "";
                try { decryptedPassword = crypto.Decrypt(storedPassword); }
                catch { decryptedPassword = storedPassword; }

                if (password != decryptedPassword)
                {
                    _logger.LogInfo($"Login failed: Wrong password for email: {email}");
                    CaptureAuditTrail(email, "Invalid Login", "Wrong password");
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiResponse<object>
                    {
                        success = false,
                        message = "Invalid credentials"
                    });
                }

                int locked = dt.Rows[0]["locked"] == DBNull.Value ? 0 : Convert.ToInt16(dt.Rows[0]["locked"]);

                if (locked == 1)
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<object> { success = false, message = "Account is locked" });

                CaptureAuditTrail(email, "Login Attempt", "OTP sent to user: " + email);

                long userId = Convert.ToInt64(dt.Rows[0]["id"]);
                Int64 pharmacyId = dt.Rows[0]["pharmacy_id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["pharmacy_id"]) : 0;
                int roleId = dt.Rows[0]["role_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["role_id"]) : 3;
                string name = dt.Rows[0]["first_name"]?.ToString() ?? "";
                string userEmail = dt.Rows[0]["email"]?.ToString() ?? "";

                string roleType = roleId switch
                {
                    1 => "SuperAdmin",
                    2 => "Admin",
                    3 => "Pharmacist",
                    4 => "Staff",
                    5 => "Cashier",
                    _ => "Staff"
                };

                _logger.LogInfo($"Login: userId={userId} pharmacyId={pharmacyId} roleId={roleId}");

                var userJobject = new JObject
                {
                    { "userid", userId.ToString() },
                    { "email", userEmail },
                    { "role_type", roleType },
                    { "profile_id", roleId.ToString() },
                    { "pharmacy_id", pharmacyId.ToString() },
                    { "name", name },
                    { "mobile", dt.Rows[0]["mobile"]?.ToString() ?? "" },
                    { "avatar", dt.Rows[0]["avatar"]?.ToString() ?? "user-default.svg" },
                    { "change_password", dt.Rows[0]["change_password"] != DBNull.Value && Convert.ToBoolean(dt.Rows[0]["change_password"]) }
                };

                // string otp = new Helpers.RandomKeyGeneratorManagement().GenerateOtp(4);
                string otp = "1000";
                string otpRef = Guid.NewGuid().ToString("N");
                dbhandler.RizikiSaveOtp(userId, "USER", userEmail, dt.Rows[0]["mobile"]?.ToString(), otp, "LOGIN", otpRef);

                var jwtUtils = new JwtUtilsHelper.JwtUtilsHandler(_logger, _config);
                string tempToken = jwtUtils.GenerateAccessToken(new JObject
                {
                    { "user_id", userId.ToString() },
                    { "email", userEmail },
                    { "role_id", roleId.ToString() },
                    { "pharmacy_id", pharmacyId.ToString() }
                });

                userJobject.Add("accessToken", tempToken);
                userJobject.Add("refreshToken", "");
                userJobject.Add("otp", otp);

                _logger.LogInfo($"RESPONSE: OTP sent for {email}");
                return Ok(new ApiResponse<JObject>
                {
                    success = true,
                    message = "OTP sent to your device",
                    action = "VerifyOTP",
                    data = userJobject
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Login: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "An error occurred while processing your request"
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] JObject jobject)
        {
            _logger.LogInfo("******* REGISTER REQUEST **********");

            try
            {
                if (jobject == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid request" });

                string pharmacyName = jobject["pharmacy_name"]?.ToString()?.Trim() ?? "";
                string slug = jobject["slug"]?.ToString()?.Trim() ?? "";
                string? pharmacyPhone = jobject["pharmacy_phone"]?.ToString()?.Trim();
                string? pharmacyEmail = jobject["pharmacy_email"]?.ToString()?.Trim();
                string? pharmacyAddress = jobject["pharmacy_address"]?.ToString()?.Trim();
                string? licenseNo = jobject["license_number"]?.ToString()?.Trim();
                string firstName = jobject["first_name"]?.ToString()?.Trim() ?? "";
                string lastName = jobject["last_name"]?.ToString()?.Trim() ?? "";
                string email = jobject["email"]?.ToString()?.Trim() ?? "";
                string password = jobject["password"]?.ToString() ?? "";
                string? phone = jobject["phone"]?.ToString()?.Trim();

                if (string.IsNullOrEmpty(pharmacyName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Pharmacy name, email and password are required" });

                if (!string.IsNullOrEmpty(slug))
                {
                    DataTable existingSlug = dbhandler.GetPharmacyIdBySlug(slug);
                    if (existingSlug.Rows.Count > 0)
                        return BadRequest(new ApiResponse<object> { success = false, message = "Slug already taken" });
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var pharmacy = new PharmacyModel
                {
                    name = pharmacyName,
                    slug = slug,
                    phone = pharmacyPhone,
                    email = pharmacyEmail,
                    address = pharmacyAddress,
                    license_no = licenseNo,
                    created_by = 0
                };

                bool pharmacyCreated = dbhandler.AddPharmacy(pharmacy);
                if (!pharmacyCreated || pharmacy.id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Failed to create pharmacy" });

                var user = new PharmacyUserModel
                {
                    pharmacy_id = pharmacy.id,
                    role_id = 1,
                    first_name = firstName,
                    last_name = lastName,
                    email = email,
                    password = hashedPassword,
                    phone = phone,
                    created_by = 0
                };

                bool userCreated = dbhandler.AddUser(user);
                if (!userCreated)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Pharmacy created but failed to create admin user" });

                _logger.LogInfo($"Register: pharmacyId={pharmacy.id} userId={user.id}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Registration successful. You can now log in.",
                    action = "login"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Register: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "Registration failed. Please try again."
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        [HttpPost("otpclientlogin")]
        public IActionResult VerifyOTP([FromBody] JObject jobject)
        {
            _logger.LogInfo("******* VERIFY OTP REQUEST **********");
            _logger.LogInfo(jobject.ToString());

            try
            {
                if (jobject == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid request" });

                string username = jobject["username"]?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(username))
                    username = jobject["email"]?.ToString()?.Trim() ?? "";
                string otp = jobject["otp"]?.ToString()?.Trim() ?? "";

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(otp))
                    return BadRequest(new ApiResponse<object> { success = false, message = "OTP and username are required" });

                // Step 1: Look up user by username (Riziki pattern)
                DataTable dtUser = dbhandler.ValidateUserLogin("PHARMACY", username);
                if (dtUser.Rows.Count == 0)
                    dtUser = dbhandler.ValidateUserLogin("USER", username);

                if (dtUser.Rows.Count == 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "User not found" });

                string userEmail = dtUser.Rows[0]["email"]?.ToString() ?? "";

                // Step 2: Verify OTP by email + otp + purpose (no otp_ref)
                DataTable dt = dbhandler.RizikiVerifyOtp(userEmail, otp, "LOGIN");
                if (dt.Rows.Count == 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid or expired OTP" });

                long userId = Convert.ToInt64(dtUser.Rows[0]["id"]);
                Int64 pharmacyId = dtUser.Rows[0]["pharmacy_id"] != DBNull.Value ? Convert.ToInt64(dtUser.Rows[0]["pharmacy_id"]) : 0;
                int roleId = dtUser.Rows[0]["role_id"] != DBNull.Value ? Convert.ToInt32(dtUser.Rows[0]["role_id"]) : 3;
                string name = dtUser.Rows[0]["first_name"]?.ToString() ?? "";
                string mobile = dtUser.Rows[0]["mobile"]?.ToString() ?? "";
                string avatar = dtUser.Rows[0]["avatar"]?.ToString() ?? "user-default.svg";

                string roleType = roleId switch
                {
                    1 => "SuperAdmin",
                    2 => "Admin",
                    3 => "Pharmacist",
                    4 => "Staff",
                    5 => "Cashier",
                    _ => "Staff"
                };

                bool changePassword = dtUser.Rows[0]["change_password"] != DBNull.Value && Convert.ToBoolean(dtUser.Rows[0]["change_password"]);

                JObject userJobject = new JObject
                {
                    { "userid", userId.ToString() },
                    { "email", userEmail },
                    { "role_type", roleType },
                    { "name", name },
                    { "mobile", mobile },
                    { "profile_id", roleId.ToString() },
                    { "pharmacy_id", pharmacyId.ToString() },
                    { "avatar", avatar }
                };

                var jwtUtils = new JwtUtilsHelper.JwtUtilsHandler(_logger, _config);
                string accessToken = jwtUtils.GenerateAccessToken(userJobject);
                string refreshToken = jwtUtils.GenerateRefreshToken();
                DateTime expiresAt = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 14));

                string hashedRefresh = BCrypt.Net.BCrypt.HashPassword(refreshToken);
                dbhandler.AddRefreshToken(userId, hashedRefresh, expiresAt);

                CaptureAuditTrail(userEmail, "OTP Verified", "Login complete for user: " + userEmail);

                userJobject.Add("accessToken", accessToken);
                userJobject.Add("refreshToken", refreshToken);
                userJobject.Add("change_password", changePassword);

                _logger.LogInfo($"RESPONSE: OTP verified for {userEmail}, login complete");
                return Ok(new ApiResponse<JObject>
                {
                    success = true,
                    message = "OTP verified - login complete",
                    action = changePassword ? "ChangePassword" : "Dashboard",
                    data = userJobject
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("VerifyOTP: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "OTP verification failed"
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult RefreshToken([FromBody] JObject jobject)
        {
            _logger.LogInfo("******* REFRESH TOKEN REQUEST **********");

            try
            {
                if (jobject == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid request" });

                string refreshToken = jobject["refreshToken"]?.ToString() ?? "";
                string email = jobject["email"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(email))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Refresh token and email are required" });

                long userId = dbhandler.GetUserIdFromRefreshToken(refreshToken);
                if (userId == 0)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiResponse<object>
                    {
                        success = false,
                        message = "Invalid or expired refresh token"
                    });

                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                dbhandler.RevokeRefreshToken(refreshToken, ipAddress);

                DataTable dt = dbhandler.ValidateUserLogin("PHARMACY", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("USER", email);

                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiResponse<object>
                    {
                        success = false,
                        message = "User not found"
                    });

                string userEmail = dt.Rows[0]["email"]?.ToString() ?? "";
                int roleId = dt.Rows[0]["role_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["role_id"]) : 3;
                Int64 pharmacyId = dt.Rows[0]["pharmacy_id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["pharmacy_id"]) : 0;

                var userJobject = new JObject
                {
                    { "user_id", userId.ToString() },
                    { "email", userEmail },
                    { "role_id", roleId.ToString() },
                    { "pharmacy_id", pharmacyId.ToString() }
                };

                var jwtUtils = new JwtUtilsHelper.JwtUtilsHandler(_logger, _config);
                string newAccessToken = jwtUtils.GenerateAccessToken(userJobject);
                string newRefreshToken = jwtUtils.GenerateRefreshToken();
                DateTime newExpires = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 14));

                string hashedRefresh = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
                dbhandler.AddRefreshToken(userId, hashedRefresh, newExpires);

                userJobject.Add("accessToken", newAccessToken);
                userJobject.Add("refreshToken", newRefreshToken);

                _logger.LogInfo("RESPONSE: Token refreshed");
                return Ok(new ApiResponse<JObject>
                {
                    success = true,
                    message = "Token refreshed successfully",
                    action = "TokenRefreshed",
                    data = userJobject
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("RefreshToken: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "Server error during token refresh"
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout([FromBody] JObject jobject)
        {
            _logger.LogInfo("******* LOGOUT REQUEST **********");

            try
            {
                long userId = GetCallerUserId();
                if (userId == 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid user" });

                bool revoked = dbhandler.RevokeAllUserRefreshTokens(userId);

                _logger.LogInfo($"Logout: userId={userId} revoked={revoked}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Logged out from all devices",
                    action = "Logout"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Logout: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "Server error during logout"
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] JObject jobject)
        {
            _logger.LogInfo("******* FORGOT PASSWORD REQUEST **********");

            try
            {
                if (jobject == null || !jobject.ContainsKey("email"))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Email is required" });

                string email = jobject["email"]!.ToString().Trim();

                DataTable dt = dbhandler.ValidateUserLogin("PHARMACY", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("USER", email);

                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "No account found with that email" });

                string otp = new Random().Next(100000, 999999).ToString();
                string otpRef = Guid.NewGuid().ToString("N");
                long userId = Convert.ToInt64(dt.Rows[0]["id"]);
                string? mobile = dt.Rows[0]["mobile"]?.ToString();

                dbhandler.RizikiSaveOtp(userId, "USER", email, mobile, otp, "PASSWORD_RESET", otpRef);

                _logger.LogInfo($"ForgotPassword: OTP generated for {email}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "OTP sent to your email",
                    data = new JObject { { "otp_ref", otpRef } }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("ForgotPassword: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "Failed to process forgot password request"
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] JObject jobject)
        {
            _logger.LogInfo("******* RESET PASSWORD REQUEST **********");

            try
            {
                if (jobject == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid request" });

                string email = jobject["email"]?.ToString()?.Trim() ?? "";
                string otp = jobject["otp"]?.ToString()?.Trim() ?? "";
                string newPassword = jobject["new_password"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(newPassword))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Email, OTP and new password are required" });

                DataTable dt = dbhandler.RizikiVerifyOtp(email, otp, "PASSWORD_RESET");
                if (dt.Rows.Count == 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid or expired OTP" });

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                bool reset = dbhandler.PortalPasswordReset(email, hashedPassword, "PHARMACY");

                if (!reset)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Failed to reset password" });

                _logger.LogInfo($"ResetPassword: Password reset for {email}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Password reset successfully. You can now log in with your new password."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("ResetPassword: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "Password reset failed"
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("check-slug")]
        public IActionResult CheckSlug([FromQuery] string slug)
        {
            try
            {
                if (string.IsNullOrEmpty(slug))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Slug is required" });

                DataTable dt = dbhandler.GetPharmacyIdBySlug(slug.Trim().ToLower());
                bool available = dt.Rows.Count == 0;

                return Ok(new ApiResponse<object>
                {
                    success = true,
                    data = new JObject { { "available", available } }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("CheckSlug: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "Server error"
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("check-email")]
        public IActionResult CheckEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Email is required" });

                DataTable dt1 = dbhandler.ValidateUserLogin("PHARMACY", email.Trim());
                DataTable dt2 = dbhandler.ValidateUserLogin("USER", email.Trim());
                bool available = dt1.Rows.Count == 0 && dt2.Rows.Count == 0;

                return Ok(new ApiResponse<object>
                {
                    success = true,
                    data = new JObject { { "available", available } }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("CheckEmail: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    success = false,
                    message = "Server error"
                });
            }
        }

        private Int64 GetCallerPharmacyId()
        {
            var claim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id");
            return claim != null ? Convert.ToInt64(claim.Value) : 0;
        }

        private Int64 GetCallerUserId()
        {
            var claim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id");
            return claim != null ? Convert.ToInt64(claim.Value) : 0;
        }

        private string GetCallerEmail()
        {
            return HttpContext.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
        }

        private int GetCallerRoleId()
        {
            var claim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "role_id");
            return claim != null ? Convert.ToInt32(claim.Value) : 0;
        }

        private void CaptureAuditTrail(string email, string actionType, string description)
        {
            try
            {
                var model = new AuditTrailModel
                {
                    user_name = email,
                    action_type = actionType,
                    action_description = description,
                    page_accessed = HttpContext.Request.Path,
                    client_ip_address = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    session_id = HttpContext.Session.Id,
                    created_on = DateTime.UtcNow
                };
                dbhandler.AddAuditTrail(model);
            }
            catch (Exception ex)
            {
                _logger.LogError("CaptureAuditTrail: " + ex.Message);
            }
        }
    }
}
