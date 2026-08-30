using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using MediStock.API.Helpers;
using MediStock.API.Models;
using MediStock.API.Services;
using System.Data;
using System.Security.Claims;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;
        private readonly EmailService emailservice;

        public AuthController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler, EmailService email)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
            emailservice = email;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [HttpPost("clientlogin")]
        public ActionResult Login([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* LOGIN REQUEST **********");
            iloggermanager.LogInfo(jobject.ToString());

            try
            {
                string email = "";
                if (jobject.ContainsKey("email"))
                    email = jobject["email"]!.ToString().Trim();
                else if (jobject.ContainsKey("username"))
                    email = jobject["username"]!.ToString().Trim();

                string password = jobject["password"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return Bad("Email and password are required");

                DataTable dt = dbhandler.ValidateUserLogin("ADMIN", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("CLIENT", email);

                if (dt.Rows.Count == 0)
                {
                    iloggermanager.LogInfo($"Login failed: User not found for email: {email}");
                    CaptureAuditTrail(email, "Invalid Login", "User not found");
                    return StatusCode(StatusCodes.Status401Unauthorized, new { success = false, message = "Invalid credentials", action = "", data = new JObject() });
                }

                string? storedPassword = dt.Rows[0]["password"]?.ToString() ?? "";

                var crypto = new CryptoHelper.MediSecurity.Rijndael();
                string decryptedPassword = "";
                try { decryptedPassword = crypto.Decrypt(storedPassword); }
                catch { decryptedPassword = storedPassword; }

                bool validPassword = password == decryptedPassword;
                if (!validPassword)
                {
                    try { validPassword = BCrypt.Net.BCrypt.Verify(password, storedPassword); }
                    catch { validPassword = false; }
                }

                if (!validPassword)
                {
                    iloggermanager.LogInfo($"Login failed: Wrong password for email: {email}");
                    CaptureAuditTrail(email, "Invalid Login", "Wrong password");
                    return StatusCode(StatusCodes.Status401Unauthorized, new { success = false, message = "Invalid credentials", action = "", data = new JObject() });
                }

                int locked = dt.Rows[0]["locked"] == DBNull.Value ? 0 : Convert.ToInt16(dt.Rows[0]["locked"]);

                if (locked == 1)
                    return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Account is locked", action = "", data = new JObject() });

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

                iloggermanager.LogInfo($"Login: userId={userId} pharmacyId={pharmacyId} roleId={roleId}");

                var userJobject = new JObject
                {
                    { "user_id", userId.ToString() },
                    { "userid", userId.ToString() },
                    { "email", userEmail },
                    { "role_type", roleType },
                    { "role_id", roleId.ToString() },
                    { "profile_id", roleId.ToString() },
                    { "pharmacy_id", pharmacyId.ToString() },
                    { "name", name },
                    { "mobile", dt.Rows[0]["mobile"]?.ToString() ?? "" },
                    { "avatar", dt.Rows[0]["avatar"]?.ToString() ?? "user-default.svg" },
                    { "change_password", dt.Rows[0]["change_password"] != DBNull.Value && Convert.ToBoolean(dt.Rows[0]["change_password"]) }
                };

                string otp = "1000";
                string otpRef = Guid.NewGuid().ToString("N");
                dbhandler.RizikiSaveOtp(userId, "CLIENT", userEmail, dt.Rows[0]["mobile"]?.ToString(), otp, "LOGIN", otpRef);
                emailservice.SendOtp(userEmail, name, otp, "login");

                var jwtUtils = new JwtUtilsHelper.JwtUtilsHandler(iloggermanager, iconfiguration);
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

                iloggermanager.LogInfo($"RESPONSE: OTP sent for {email}");
                return Ok(new { success = true, message = "OTP sent to your device", action = "VerifyOTP", data = userJobject });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("Login: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while processing your request", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public ActionResult Register([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* REGISTER REQUEST **********");

            try
            {
                if (jobject == null) return Bad("Invalid request");

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

                if (string.IsNullOrEmpty(pharmacyName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return Bad("Pharmacy name, email and password are required");

                if (!string.IsNullOrEmpty(slug))
                {
                    DataTable existingSlug = dbhandler.GetPharmacyIdBySlug(slug);
                    if (existingSlug.Rows.Count > 0) return Bad("Slug already taken");
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
                if (!pharmacyCreated || pharmacy.id <= 0) return Bad("Failed to create pharmacy");

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
                if (!userCreated) return Bad("Pharmacy created but failed to create admin user");

                iloggermanager.LogInfo($"Register: pharmacyId={pharmacy.id} userId={user.id}");
                return Ok(new { success = true, message = "Registration successful. You can now log in.", action = "login", data = (object?)null });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("Register: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Registration failed. Please try again.", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        [HttpPost("otpclientlogin")]
        public ActionResult VerifyOTP([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* VERIFY OTP REQUEST **********");
            iloggermanager.LogInfo(jobject.ToString());

            try
            {
                if (jobject == null) return Bad("Invalid request");

                string username = jobject["username"]?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(username))
                    username = jobject["email"]?.ToString()?.Trim() ?? "";
                string otp = jobject["otp"]?.ToString()?.Trim() ?? "";

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(otp)) return Bad("OTP and username are required");

                DataTable dtUser = dbhandler.ValidateUserLogin("ADMIN", username);
                if (dtUser.Rows.Count == 0)
                    dtUser = dbhandler.ValidateUserLogin("CLIENT", username);

                if (dtUser.Rows.Count == 0) return Bad("User not found");

                string userEmail = dtUser.Rows[0]["email"]?.ToString() ?? "";

                DataTable dt = dbhandler.RizikiVerifyOtp(userEmail, otp, "LOGIN");
                if (dt.Rows.Count == 0) return Bad("Invalid or expired OTP");

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
                    { "user_id", userId.ToString() },
                    { "userid", userId.ToString() },
                    { "email", userEmail },
                    { "role_type", roleType },
                    { "role_id", roleId.ToString() },
                    { "name", name },
                    { "mobile", mobile },
                    { "profile_id", roleId.ToString() },
                    { "pharmacy_id", pharmacyId.ToString() },
                    { "avatar", avatar }
                };

                var jwtUtils = new JwtUtilsHelper.JwtUtilsHandler(iloggermanager, iconfiguration);
                string accessToken = jwtUtils.GenerateAccessToken(userJobject);
                string refreshToken = jwtUtils.GenerateRefreshToken();
                DateTime expiresAt = DateTime.UtcNow.AddDays(iconfiguration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 14));

                string hashedRefresh = BCrypt.Net.BCrypt.HashPassword(refreshToken);
                dbhandler.AddRefreshToken(userId, hashedRefresh, expiresAt);

                CaptureAuditTrail(userEmail, "OTP Verified", "Login complete for user: " + userEmail);

                userJobject.Add("accessToken", accessToken);
                userJobject.Add("refreshToken", refreshToken);
                userJobject.Add("change_password", changePassword);

                iloggermanager.LogInfo($"RESPONSE: OTP verified for {userEmail}, login complete");
                return Ok(new { success = true, message = "OTP verified - login complete", action = changePassword ? "ChangePassword" : "Dashboard", data = userJobject });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("VerifyOTP: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "OTP verification failed", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public ActionResult RefreshToken([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* REFRESH TOKEN REQUEST **********");

            try
            {
                if (jobject == null) return Bad("Invalid request");

                string refreshToken = jobject["refreshToken"]?.ToString() ?? "";
                string email = jobject["email"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(email)) return Bad("Refresh token and email are required");

                long userId = dbhandler.GetUserIdFromRefreshToken(refreshToken);
                if (userId == 0)
                    return StatusCode(StatusCodes.Status401Unauthorized, new { success = false, message = "Invalid or expired refresh token", action = "", data = new JObject() });

                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                dbhandler.RevokeRefreshToken(refreshToken, ipAddress);

                DataTable dt = dbhandler.ValidateUserLogin("ADMIN", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("CLIENT", email);

                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status401Unauthorized, new { success = false, message = "User not found", action = "", data = new JObject() });

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

                var jwtUtils = new JwtUtilsHelper.JwtUtilsHandler(iloggermanager, iconfiguration);
                string newAccessToken = jwtUtils.GenerateAccessToken(userJobject);
                string newRefreshToken = jwtUtils.GenerateRefreshToken();
                DateTime newExpires = DateTime.UtcNow.AddDays(iconfiguration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 14));

                string hashedRefresh = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
                dbhandler.AddRefreshToken(userId, hashedRefresh, newExpires);

                userJobject.Add("accessToken", newAccessToken);
                userJobject.Add("refreshToken", newRefreshToken);

                iloggermanager.LogInfo("RESPONSE: Token refreshed");
                return Ok(new { success = true, message = "Token refreshed successfully", action = "TokenRefreshed", data = userJobject });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("RefreshToken: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error during token refresh", action = "", data = new JObject() });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public ActionResult Logout([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* LOGOUT REQUEST **********");

            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (userId == 0) return Bad("Invalid user");

                bool revoked = dbhandler.RevokeAllUserRefreshTokens(userId);

                iloggermanager.LogInfo($"Logout: userId={userId} revoked={revoked}");
                return Ok(new { success = true, message = "Logged out from all devices", action = "Logout", data = (object?)null });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("Logout: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error during logout", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public ActionResult ForgotPassword([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* FORGOT PASSWORD REQUEST **********");

            try
            {
                if (jobject == null || !jobject.ContainsKey("email")) return Bad("Email is required");

                string email = jobject["email"]!.ToString().Trim();

                DataTable dt = dbhandler.ValidateUserLogin("ADMIN", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("CLIENT", email);

                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "No account found with that email", action = "", data = new JObject() });

                string otp = new Random().Next(100000, 999999).ToString();
                string otpRef = Guid.NewGuid().ToString("N");
                long userId = Convert.ToInt64(dt.Rows[0]["id"]);
                string? mobile = dt.Rows[0]["mobile"]?.ToString();
                string firstName = dt.Rows[0]["first_name"]?.ToString() ?? "";

                dbhandler.RizikiSaveOtp(userId, "CLIENT", email, mobile, otp, "PASSWORD_RESET", otpRef);
                emailservice.SendOtp(email, firstName, otp, "password reset");

                iloggermanager.LogInfo($"ForgotPassword: OTP generated for {email}");
                return Ok(new { success = true, message = "OTP sent to your email", action = "", data = new JObject { { "otp_ref", otpRef } } });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("ForgotPassword: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Failed to process forgot password request", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public ActionResult ResetPassword([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* RESET PASSWORD REQUEST **********");

            try
            {
                if (jobject == null) return Bad("Invalid request");

                string email = jobject["email"]?.ToString()?.Trim() ?? "";
                string otp = jobject["otp"]?.ToString()?.Trim() ?? "";
                string newPassword = jobject["new_password"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(newPassword)) return Bad("Email, OTP and new password are required");

                DataTable dt = dbhandler.RizikiVerifyOtp(email, otp, "PASSWORD_RESET");
                if (dt.Rows.Count == 0) return Bad("Invalid or expired OTP");

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                bool reset = dbhandler.PortalPasswordReset(email, hashedPassword, "ADMIN");

                if (!reset) return Bad("Failed to reset password");

                iloggermanager.LogInfo($"ResetPassword: Password reset for {email}");
                return Ok(new { success = true, message = "Password reset successfully. You can now log in with your new password.", action = "", data = (object?)null });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("ResetPassword: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Password reset failed", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpPost("resendotp")]
        public ActionResult ResendOtp([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* RESEND OTP REQUEST **********");
            try
            {
                if (jobject == null) return Bad("Invalid request");

                string email = jobject["username"]?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(email))
                    email = jobject["email"]?.ToString()?.Trim() ?? "";

                if (string.IsNullOrEmpty(email)) return Bad("Email is required");

                DataTable dt = dbhandler.ValidateUserLogin("ADMIN", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("CLIENT", email);

                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "No account found with that email", action = "", data = new JObject() });

                long userId = Convert.ToInt64(dt.Rows[0]["id"]);
                string userEmail = dt.Rows[0]["email"]?.ToString() ?? email;
                string mobile = dt.Rows[0]["mobile"]?.ToString() ?? "";

                string otp = "1000";
                string otpRef = Guid.NewGuid().ToString("N");
                dbhandler.RizikiSaveOtp(userId, "CLIENT", userEmail, mobile, otp, "LOGIN", otpRef);
                emailservice.SendOtp(userEmail, dt.Rows[0]["first_name"]?.ToString() ?? "", otp, "login");

                iloggermanager.LogInfo($"ResendOtp: OTP regenerated for {userEmail}");
                return Ok(new { success = true, message = "A new OTP has been sent", action = "", data = new JObject { { "otp", otp }, { "otp_ref", otpRef } } });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("ResendOtp: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Failed to resend OTP", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpPost("resetpassword")]
        public ActionResult ResetPasswordByEmail([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* RESET PASSWORD (EMAIL) REQUEST **********");
            try
            {
                if (jobject == null || !jobject.ContainsKey("email"))
                    return Bad("Email is required");

                string email = jobject["email"]!.ToString().Trim();

                DataTable dt = dbhandler.ValidateUserLogin("ADMIN", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("CLIENT", email);

                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "No account found with that email", action = "", data = new JObject() });

                string tempPassword = GenerateTempPassword();
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                bool reset = dbhandler.PortalPasswordReset(email, hashedPassword, "ADMIN");

                if (!reset) return Bad("Failed to reset password");

                iloggermanager.LogInfo($"ResetPasswordByEmail: Password reset for {email}");
                CaptureAuditTrail(email, "Password Reset", "Email-initiated password reset");
                return Ok(new { success = true, message = "Password reset. Use the temporary password to log in.", action = "", data = new JObject { { "temp_password", tempPassword } } });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("ResetPasswordByEmail: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Password reset failed", action = "", data = new JObject() });
            }
        }

        [Authorize]
        [HttpPost("changepassword")]
        public ActionResult ChangePassword([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* CHANGE PASSWORD REQUEST **********");
            try
            {
                if (jobject == null) return Bad("Invalid request");

                string email = jobject["email"]?.ToString()?.Trim() ?? "";
                string currentPassword = jobject["password"]?.ToString() ?? "";
                string newPassword = jobject["newpassword"]?.ToString() ?? "";
                string confirmPassword = jobject["confirmpassword"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                    return Bad("Email, current password and new password are required");

                if (newPassword != confirmPassword)
                    return Bad("New password and confirm password do not match");

                DataTable dt = dbhandler.ValidateUserLogin("ADMIN", email);
                if (dt.Rows.Count == 0)
                    dt = dbhandler.ValidateUserLogin("CLIENT", email);

                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "No account found with that email", action = "", data = new JObject() });

                string storedPassword = dt.Rows[0]["password"]?.ToString() ?? "";

                var crypto = new CryptoHelper.MediSecurity.Rijndael();
                bool validPassword = false;
                try { validPassword = currentPassword == crypto.Decrypt(storedPassword); }
                catch { validPassword = false; }
                if (!validPassword)
                {
                    try { validPassword = BCrypt.Net.BCrypt.Verify(currentPassword, storedPassword); }
                    catch { validPassword = false; }
                }

                if (!validPassword)
                    return Bad("Current password is incorrect");

                string hashedNew = BCrypt.Net.BCrypt.HashPassword(newPassword);
                bool updated = dbhandler.PortalPasswordReset(email, hashedNew, "ADMIN");
                if (!updated) return Bad("Failed to change password");

                iloggermanager.LogInfo($"ChangePassword: Password changed for {email}");
                CaptureAuditTrail(email, "Change Password", "User changed their password");
                return Ok(new { success = true, message = "Password changed successfully", action = "", data = (object?)null });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("ChangePassword: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Failed to change password", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpPost("register-pharmacy")]
        public ActionResult RegisterPharmacy([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* REGISTER PHARMACY (PORTAL) REQUEST **********");
            try
            {
                if (jobject == null) return Bad("Invalid request");

                string pharmacyName = jobject["pharmacy_name"]?.ToString()?.Trim() ?? "";
                string pharmacyEmail = jobject["pharmacy_email"]?.ToString()?.Trim() ?? "";
                string pharmacyPhone = jobject["pharmacy_phone"]?.ToString()?.Trim() ?? "";
                string pharmacyAddress = jobject["pharmacy_address"]?.ToString()?.Trim() ?? "";
                string firstName = jobject["admin_first_name"]?.ToString()?.Trim() ?? "";
                string lastName = jobject["admin_last_name"]?.ToString()?.Trim() ?? "";
                string email = jobject["admin_email"]?.ToString()?.Trim() ?? "";
                string password = jobject["password"]?.ToString() ?? "";
                string confirmPassword = jobject["confirm_password"]?.ToString() ?? "";
                string phone = jobject["admin_phone"]?.ToString()?.Trim() ?? "";

                if (string.IsNullOrEmpty(pharmacyName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return Bad("Pharmacy name, email and password are required");
                if (password != confirmPassword) return Bad("Passwords do not match");

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var pharmacy = new PharmacyModel
                {
                    name = pharmacyName,
                    slug = "",
                    phone = string.IsNullOrEmpty(pharmacyPhone) ? null : pharmacyPhone,
                    email = string.IsNullOrEmpty(pharmacyEmail) ? null : pharmacyEmail,
                    address = string.IsNullOrEmpty(pharmacyAddress) ? null : pharmacyAddress,
                    license_no = null,
                    created_by = 0
                };

                bool pharmacyCreated = dbhandler.AddPharmacy(pharmacy);
                if (!pharmacyCreated || pharmacy.id <= 0) return Bad("Failed to create pharmacy");

                var user = new PharmacyUserModel
                {
                    pharmacy_id = pharmacy.id,
                    role_id = 1,
                    first_name = firstName,
                    last_name = lastName,
                    email = email,
                    password = hashedPassword,
                    phone = string.IsNullOrEmpty(phone) ? null : phone,
                    created_by = 0
                };

                bool userCreated = dbhandler.AddUser(user);
                if (!userCreated) return Bad("Pharmacy created but failed to create admin user");

                iloggermanager.LogInfo($"RegisterPharmacy: pharmacyId={pharmacy.id} userId={user.id}");
                return Ok(new { success = true, message = "Registration successful. You can now log in.", action = "login", data = (object?)null });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("RegisterPharmacy: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Registration failed. Please try again.", action = "", data = new JObject() });
            }
        }

        [NonAction]
        private static string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var rnd = Random.Shared;
            int len = 8;
            char[] pw = new char[len];
            for (int i = 0; i < len; i++) pw[i] = chars[rnd.Next(chars.Length)];
            return "Med" + new string(pw);
        }

        [AllowAnonymous]
        [HttpGet("check-slug")]
        public ActionResult CheckSlug([FromQuery] string slug)
        {
            try
            {
                if (string.IsNullOrEmpty(slug)) return Bad("Slug is required");

                DataTable dt = dbhandler.GetPharmacyIdBySlug(slug.Trim().ToLower());
                bool available = dt.Rows.Count == 0;

                return Ok(new { success = true, message = "Success", action = "", data = new JObject { { "available", available } } });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("CheckSlug: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error", action = "", data = new JObject() });
            }
        }

        [AllowAnonymous]
        [HttpGet("check-email")]
        public ActionResult CheckEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email)) return Bad("Email is required");

                DataTable dt1 = dbhandler.ValidateUserLogin("ADMIN", email.Trim());
                DataTable dt2 = dbhandler.ValidateUserLogin("CLIENT", email.Trim());
                bool available = dt1.Rows.Count == 0 && dt2.Rows.Count == 0;

                return Ok(new { success = true, message = "Success", action = "", data = new JObject { { "available", available } } });
            }
            catch (Exception ex)
            {
                iloggermanager.LogError("CheckEmail: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error", action = "", data = new JObject() });
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
        private ActionResult Forbidden(string msg) =>
            StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = msg, action = "", data = new JObject() });

        [NonAction]
        private ActionResult ServerError() =>
            StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error", action = "", data = new JObject() });

        [NonAction]
        public bool CaptureAuditTrail(string user, string action_type, string action_description)
        {
            AuditTrailModel audittrailmodel = new()
            {
                user_name = user,
                action_type = action_type,
                action_description = action_description,
                page_accessed = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}",
                client_ip_address = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                session_id = HttpContext.TraceIdentifier
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}
