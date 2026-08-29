// ============================================================
//  MediStock.Portal — AdminController
//  Routes:
//    GET  /Admin/Users        → users management view
//    GET  /Admin/Dashboard    → admin overview dashboard view
//    GET  /Admin/GetUsers     → JSON all pharmacy users
//    GET  /Admin/GetUser?id=  → JSON single user
//    POST /Admin/AddUser      → proxy → api/admin/adduser
//    POST /Admin/UpdateUser   → proxy → api/admin/updateuser
//    POST /Admin/DeleteUser   → proxy → api/admin/deleteuser
//    POST /Admin/ResetPassword → proxy → api/admin/resetpassword
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public AdminController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Users()
        {
            await _audit.LogViewAsync("Admin/Users");
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            await _audit.LogViewAsync("Admin/Dashboard");
            return View();
        }

        public async Task<IActionResult> AccessControl()
        {
            await _audit.LogViewAsync("Admin/AccessControl");
            return View();
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/admin/users?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(long id)
        {
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/admin/getuser?id={id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/admin/roles");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/admin/adduser", new
            {
                pharmacy_id = GetPharmacyId(),
                first_name  = model.first_name,
                last_name   = model.last_name,
                email       = model.email,
                phone       = model.phone,
                role_id     = model.role_id,
                password    = model.password
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "User created" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to create user" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/admin/updateuser", new
            {
                id        = model.id,
                first_name = model.first_name,
                last_name  = model.last_name,
                email      = model.email,
                phone      = model.phone,
                role_id    = model.role_id,
                is_active  = model.is_active
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "User updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update user" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/admin/deleteuser", new { id = model.id });
            return Json(result.IsSuccess
                ? new { success = true, message = "User deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete user" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (model == null || model.user_id <= 0)
                return Json(new { success = false, message = "user_id is required" });

            var result = await _api.PostAsync<object>("api/admin/resetpassword", new
            {
                user_id     = model.user_id,
                new_password = model.new_password
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Password reset" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to reset password" : result.Error });
        }

        // ── Admin Dashboard Data ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/admin/stats");
                return Json(result.IsSuccess ? result.Data : new { totalUsers = 0, totalProducts = 0, alertCount = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemInfo()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/admin/system-info");
                return Json(result.IsSuccess ? result.Data : new { version = "", databaseStatus = "", serverTime = "" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentLogins()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/admin/recent-logins");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // ── Access Control Data ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetRolesList()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/access/roles");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/access/menus");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuAccess(int roleId)
        {
            try
            {
                var result = await _api.GetAsync<object>($"api/access/menu-access?roleId={roleId}");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveMenuAccess([FromBody] SaveMenuAccessRequest model)
        {
            if (model == null || model.role_id <= 0)
                return Json(new { success = false, message = "role_id is required" });

            var result = await _api.PostAsync<object>("api/access/menu-access", new
            {
                role_id  = model.role_id,
                menu_ids = model.menu_ids
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Menu access saved" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> SaveRole([FromBody] SaveRoleRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.role_name))
                return Json(new { success = false, message = "role_name is required" });

            if (model.id > 0)
            {
                var result = await _api.PostAsync<object>($"api/access/roles/{model.id}", new
                {
                    role_name  = model.role_name,
                    description = model.description
                });
                return Json(result.IsSuccess
                    ? new { success = true, message = "Role updated" }
                    : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed" : result.Error });
            }
            else
            {
                var result = await _api.PostAsync<object>("api/access/roles", new
                {
                    role_name  = model.role_name,
                    description = model.description
                });
                return Json(result.IsSuccess
                    ? new { success = true, message = "Role created" }
                    : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed" : result.Error });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.DeleteAsync<object>($"api/access/roles/{model.id}");
            return Json(result.IsSuccess
                ? new { success = true, message = "Role deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete role" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddUserRequest
        {
            public string? first_name { get; set; }
            public string? last_name  { get; set; }
            public string? email      { get; set; }
            public string? phone      { get; set; }
            public long    role_id    { get; set; }
            public string? password   { get; set; }
        }

        public class UpdateUserRequest
        {
            public long    id         { get; set; }
            public string? first_name { get; set; }
            public string? last_name  { get; set; }
            public string? email      { get; set; }
            public string? phone      { get; set; }
            public long    role_id    { get; set; }
            public bool    is_active  { get; set; } = true;
        }

        public class ResetPasswordRequest
        {
            public long    user_id      { get; set; }
            public string? new_password { get; set; }
        }

        public class SaveMenuAccessRequest
        {
            public int      role_id  { get; set; }
            public int[]?   menu_ids { get; set; }
        }

        public class SaveRoleRequest
        {
            public long    id          { get; set; }
            public string? role_name   { get; set; }
            public string? description { get; set; }
        }
    }
}
