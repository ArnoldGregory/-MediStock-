// ============================================================
//  MediStock.Portal — AccountController
//  Wired to the MediStockAPI AuthController routes (all api/auth/...):
//    clientlogin -> otpclientlogin -> resendotp / resetpassword / changepassword
//  Routes on profile_id (role_id): 1=ADMIN, 2=PHARMACIST, 3=CLERK.
// ============================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Models.Auth;
using MediStock.Portal.Services;
using LoginRequest = MediStock.Portal.Models.Auth.LoginRequest;
using OtpRequest = MediStock.Portal.Models.Auth.OtpRequest;

namespace MediStock.Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;
        private readonly IWebHostEnvironment _env;
        private readonly AuditService _audit;

        public AccountController(ApiClient api, IWebHostEnvironment env, AuditService audit)
        {
            _api = api;
            _env = env;
            _audit = audit;
        }

        // ── GET /Account/Login ───────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl, bool expired = false)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();
            if (expired)
                ViewBag.ExpiredMessage = "Your session expired. Please log in again.";
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // ── POST /Account/Login  -> api/auth/clientlogin ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _api.AuthPostAsync<LoginResponse>(
                "api/auth/clientlogin",
                new LoginRequest { Username = model.username, Password = model.password });

            if (!result.IsSuccess || result.Data is null)
            {
                await _audit.LogLoginAsync(model.username, false, result.Error);
                model.errormessage = string.IsNullOrEmpty(result.Error) ? "Invalid credentials" : result.Error;
                return View(model);
            }

            var data = result.Data;
            var action = result.Action ?? string.Empty;

            if (action.Equals("VerifyOTP", StringComparison.OrdinalIgnoreCase))
            {
                _api.SaveTempTokens(data.AccessToken, data.RefreshToken);
                HttpContext.Session.SetString("TempUserId", data.UserId);
                HttpContext.Session.SetString("TempEmail", data.Email);
                HttpContext.Session.SetString("TempName", data.Name);
                HttpContext.Session.SetString("TempMobile", data.Mobile);
                HttpContext.Session.SetString("TempRoleType", data.RoleType);
                HttpContext.Session.SetString("TempPharmacyId", data.PharmacyId);

                var otpVm = new OtpViewModel { UserId = data.UserId };
                if (!string.IsNullOrEmpty(data.Otp))
                    otpVm.DevOtpHint = $"Your OTP: {data.Otp}";

                TempData["ReturnUrl"] = returnUrl;
                return View("VerifyOtp", otpVm);
            }

            await _audit.LogLoginAsync(model.username, false, $"Unexpected action: {action}");
            model.errormessage = "Unexpected response from server.";
            return View(model);
        }

        // ── GET /Account/VerifyOtp ───────────────────────────────────────────
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var userId = HttpContext.Session.GetString("TempUserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction(nameof(Login));
            return View(new OtpViewModel { UserId = userId });
        }

        // ── POST /Account/VerifyOtp  -> api/auth/otpclientlogin ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tempEmail = HttpContext.Session.GetString("TempEmail") ?? model.UserId;

            var result = await _api.AuthPostWithTokenAsync<OtpResponse>(
                "api/auth/otpclientlogin",
                new OtpRequest { Username = tempEmail, Otp = model.Otp },
                _api.GetTempAccessToken());

            if (!result.IsSuccess || result.Data is null)
            {
                await _audit.LogOtpAsync(tempEmail, false, result.Error);
                model.ErrorMessage = string.IsNullOrEmpty(result.Error) ? "Invalid or expired OTP" : result.Error;
                return View(model);
            }

            var data = result.Data;

            _api.ClearTempTokens();
            _api.SaveTokens(data.AccessToken, data.RefreshToken);

            var userId = !string.IsNullOrEmpty(data.UserId) ? data.UserId : HttpContext.Session.GetString("TempUserId") ?? "";
            var email = !string.IsNullOrEmpty(data.Email) ? data.Email : HttpContext.Session.GetString("TempEmail") ?? "";
            var name = !string.IsNullOrEmpty(data.Name) ? data.Name : HttpContext.Session.GetString("TempName") ?? "";
            var mobile = !string.IsNullOrEmpty(data.Mobile) ? data.Mobile : HttpContext.Session.GetString("TempMobile") ?? "";
            var roleType = !string.IsNullOrEmpty(data.RoleType) ? data.RoleType : HttpContext.Session.GetString("TempRoleType") ?? "";
            var profileId = data.ProfileId ?? "";
            var pharmacyId = !string.IsNullOrEmpty(data.PharmacyId) ? data.PharmacyId : HttpContext.Session.GetString("TempPharmacyId") ?? "";

            var avatar = data.Avatar ?? "user-default.svg";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email,          email),
                new(ClaimTypes.Name,           name),
                new("mobile",                  mobile),
                new(ClaimTypes.Role,           roleType),
                new("profile_id",              profileId),
                new("pharmacy_id",             pharmacyId),
                new("avatar",                  avatar),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            foreach (var k in new[] { "TempUserId", "TempEmail", "TempName", "TempMobile", "TempRoleType", "TempPharmacyId" })
                HttpContext.Session.Remove(k);

            await _audit.LogOtpAsync(email, true, $"Logged in as role_id {profileId}");

            if (result.Data.ChangePassword)
                return RedirectToAction(nameof(ChangePassword), new { forced = "true" });

            return RedirectToDashboard();
        }

        // ── POST /Account/ResendOtp  -> api/auth/resendotp ───────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp()
        {
            var email = HttpContext.Session.GetString("TempEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            var result = await _api.AuthPostAsync<OtpResponse>(
                "api/auth/resendotp",
                new { username = email });

            var vm = new OtpViewModel { UserId = HttpContext.Session.GetString("TempUserId") ?? "" };
            if (!result.IsSuccess)
                vm.ErrorMessage = string.IsNullOrEmpty(result.Error) ? "Could not resend OTP" : result.Error;
            else
                TempData["Success"] = "A new OTP has been sent.";

            await _audit.LogOtpAsync(email, result.IsSuccess, "Resend OTP");
            return View("VerifyOtp", vm);
        }

        // ── GET/POST Reset Password  -> api/auth/resetpassword ───────────────
        [HttpGet]
        public IActionResult ResetPassword() => View(new ResetPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.email))
            {
                model.errormessage = "Email is required";
                return View(model);
            }

            var result = await _api.AuthPostAsync<object>(
                "api/auth/resetpassword",
                new { email = model.email });

            if (result.IsSuccess)
                TempData["Success"] = "Password reset. Check your email/SMS for the new password.";
            else
                model.errormessage = string.IsNullOrEmpty(result.Error) ? "The email does not exist" : result.Error;

            await _audit.LogViewAsync("ResetPassword", $"{model.email} success={result.IsSuccess}");
            return View(model);
        }

        // ── GET/POST Change Password  -> api/auth/changepassword ─────────────
        [HttpGet]
        public IActionResult ChangePassword(string? forced)
        {
            ViewBag.Forced = forced == "true";
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (model.newpassword != model.confirmpassword)
            {
                model.errormessage = "New password and confirm password do not match";
                return View(model);
            }

            var email = User.FindFirstValue(ClaimTypes.Email) ?? model.email;

            var result = await _api.AuthPostAsync<object>(
                "api/auth/changepassword",
                new
                {
                    email,
                    password = model.password,
                    newpassword = model.newpassword,
                    confirmpassword = model.confirmpassword
                });

            if (result.IsSuccess)
            {
                TempData["Success"] = "Password changed successfully. Please log in again.";
                _api.ClearTokens();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            model.errormessage = string.IsNullOrEmpty(result.Error) ? "Failed to change password" : result.Error;
            return View(model);
        }

        // ── POST /Account/Logout ─────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _audit.LogLogoutAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                await _api.AuthPostAuthAsync<object>("api/auth/logout", new { user_id = userId });

            _api.ClearTokens();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // ── Company self-registration ─────────────────────────────────────────
        [HttpGet]
        public IActionResult PharmacyRegister() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PharmacyRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            var payload = new
            {
                pharmacy_name     = model.PharmacyName,
                pharmacy_email    = model.PharmacyEmail,
                pharmacy_phone    = model.PharmacyPhone,
                pharmacy_address  = model.PharmacyAddress,
                admin_first_name  = model.FirstName,
                admin_last_name   = model.LastName,
                admin_email       = model.AdminEmail,
                admin_phone       = model.AdminPhone,
                password          = model.Password,
                confirm_password  = model.ConfirmPassword
            };

            var r = await _api.AuthPostAsync<object>("api/auth/register-pharmacy", payload);
            if (r.IsSuccess)
                return RedirectToAction(nameof(Login), new { registered = "true" });

            ModelState.AddModelError("", string.IsNullOrEmpty(r.Error) ? "Registration failed. Please try again." : r.Error);
            return View(model);
        }

        // ── role-based landing ───────────────────────────────────────────────
        private IActionResult RedirectToDashboard()
            => RedirectToAction("Index", "Dashboard");
    }
}
