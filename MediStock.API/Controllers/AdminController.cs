using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public AdminController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("users")]
        public IActionResult GetUsers([FromQuery] long pharmacyId = 0)
        {
            _logger.LogInfo("******* GET USERS REQUEST **********");
            try
            {
                var callerPharmacyId = GetCallerPharmacyId();
                var callerRoleId = GetCallerRoleId();
                if (callerRoleId == 1) pharmacyId = callerPharmacyId;
                else if (pharmacyId == 0) pharmacyId = callerPharmacyId;

                _logger.LogInfo($"GetUsers: pharmacyId={pharmacyId}");

                DataTable dtPortal = dbhandler.GetUsersByPharmacy(pharmacyId);
                DataTable dtExternal = dbhandler.GetExternalUsersByPharmacy(pharmacyId);

                var users = new List<Dictionary<string, object?>>();
                foreach (DataRow row in dtPortal.Rows)
                    users.Add(RowToDict(row));
                foreach (DataRow row in dtExternal.Rows)
                    users.Add(RowToDict(row));

                CaptureAuditTrail(GetCallerEmail(), "View Users", $"Viewed user list for pharmacy {pharmacyId}");
                return Ok(new ApiResponse<object> { success = true, data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetUsers: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("users/{id}")]
        public IActionResult GetUserById(long id)
        {
            _logger.LogInfo("******* GET USER BY ID REQUEST **********");
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                DataTable dt = dbhandler.GetUserById(id);
                if (dt.Rows.Count == 0)
                {
                    dt = dbhandler.GetExternalUserById(id);
                }

                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "User not found" });

                return Ok(new ApiResponse<object> { success = true, data = RowToDict(dt.Rows[0]) });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetUserById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("getuser")]
        public IActionResult GetUser([FromQuery] long id)
        {
            return GetUserById(id);
        }

        [Authorize]
        [HttpPost("users")]
        public IActionResult CreateUser([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            _logger.LogInfo("******* CREATE USER REQUEST **********");
            _logger.LogInfo($"CreateUser: body={body}");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var callerRoleId = GetCallerRoleId();

                string firstName = body["firstName"]?.ToString() ?? body["first_name"]?.ToString() ?? "";
                string lastName = body["lastName"]?.ToString() ?? body["last_name"]?.ToString() ?? "";
                string email = body["email"]?.ToString() ?? "";
                string? phone = body["phone"]?.ToString();
                int roleId = body["roleId"] != null ? Convert.ToInt32(body["roleId"]) : (body["role_id"] != null ? Convert.ToInt32(body["role_id"]) : 3);
                string password = body["password"]?.ToString() ?? "password";

                if (string.IsNullOrEmpty(email))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Email is required" });

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                var user = new PharmacyUserModel
                {
                    pharmacy_id = pharmacyId,
                    role_id = roleId,
                    first_name = firstName,
                    last_name = lastName,
                    email = email,
                    mobile = phone,
                    password = hashedPassword,
                    created_by = GetCallerUserId()
                };

                bool created = dbhandler.AddUser(user);
                if (!created)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Failed to create user. Email may already exist." });

                CaptureAuditTrail(GetCallerEmail(), "Create User", $"Created user: {email} (role: {roleId})");
                return Ok(new ApiResponse<object> { success = true, message = "User created" });
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateUser: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpPost("adduser")]
        public IActionResult AddUser([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            return CreateUser(body);
        }

        [Authorize]
        [HttpPut("users/{id}")]
        public IActionResult UpdateUser(long id, [FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            _logger.LogInfo("******* UPDATE USER REQUEST **********");
            _logger.LogInfo($"UpdateUser: id={id}, body={body}");
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                string? firstName = body["firstName"]?.ToString() ?? body["first_name"]?.ToString();
                string? lastName = body["lastName"]?.ToString() ?? body["last_name"]?.ToString();
                string? email = body["email"]?.ToString();
                string? phone = body["phone"]?.ToString();
                int? roleId = body["roleId"] != null ? Convert.ToInt32(body["roleId"]) : (body["role_id"] != null ? Convert.ToInt32(body["role_id"]) : null);
                bool isActive = body["isActive"] != null ? Convert.ToBoolean(body["isActive"]) : (body["is_active"] != null ? Convert.ToBoolean(body["is_active"]) : true);

                bool updated = dbhandler.AdminUpdateUser(id, firstName, lastName, email, phone, roleId, isActive);
                if (!updated)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Failed to update user" });

                CaptureAuditTrail(GetCallerEmail(), "Update User", $"Updated user {id}: {email}");
                return Ok(new ApiResponse<object> { success = true, message = "User updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateUser: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpPost("updateuser")]
        public IActionResult UpdateUserLegacy([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            long id = body["id"] != null ? Convert.ToInt64(body["id"]) : 0;
            return UpdateUser(id, body);
        }

        [Authorize]
        [HttpDelete("users/{id}")]
        public IActionResult DeleteUser(long id)
        {
            _logger.LogInfo("******* DELETE USER REQUEST **********");
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                bool deleted = dbhandler.DeleteRecord(id, GetCallerUserId(), "pharmacy_user");
                if (!deleted)
                    deleted = dbhandler.DeleteRecord(id, GetCallerUserId(), "p_external_portal_user");

                CaptureAuditTrail(GetCallerEmail(), "Delete User", $"Deleted user {id}");
                return Ok(new ApiResponse<object>
                {
                    success = deleted,
                    message = deleted ? "User deleted" : "Failed to delete user"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteUser: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpPost("deleteuser")]
        public IActionResult DeleteUserLegacy([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            long id = body["id"] != null ? Convert.ToInt64(body["id"]) : 0;
            return DeleteUser(id);
        }

        [Authorize]
        [HttpPost("resetpassword")]
        public IActionResult ResetPassword([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            _logger.LogInfo("******* RESET PASSWORD REQUEST **********");
            try
            {
                long userId = body["user_id"] != null ? Convert.ToInt64(body["user_id"]) : 0;
                string newPassword = body["new_password"]?.ToString() ?? "password";

                if (userId <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "user_id is required" });

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                bool reset = dbhandler.AdminResetPassword(userId, hashedPassword);
                if (!reset)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Failed to reset password" });

                CaptureAuditTrail(GetCallerEmail(), "Reset Password", $"Reset password for user {userId}");
                return Ok(new ApiResponse<object> { success = true, message = "Password reset" });
            }
            catch (Exception ex)
            {
                _logger.LogError("ResetPassword: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            _logger.LogInfo("******* GET ROLES REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("roles", "0");
                if (dt.Rows.Count == 0)
                {
                    var roles = new[]
                    {
                        new { roleId = 1, roleName = "SuperAdmin" },
                        new { roleId = 2, roleName = "Admin" },
                        new { roleId = 3, roleName = "Pharmacist" },
                        new { roleId = 4, roleName = "Staff" },
                        new { roleId = 5, roleName = "Cashier" },
                    };
                    return Ok(new ApiResponse<object> { success = true, data = roles });
                }

                var roleList = new List<Dictionary<string, object?>>();
                foreach (DataRow row in dt.Rows)
                    roleList.Add(RowToDict(row));
                return Ok(new ApiResponse<object> { success = true, data = roleList });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetRoles: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            _logger.LogInfo("******* GET ADMIN STATS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetDashboardSummary(pharmacyId);

                var stats = new Dictionary<string, object?>();
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    stats["totalUsers"] = dbhandler.GetUsersByPharmacy(pharmacyId).Rows.Count + dbhandler.GetExternalUsersByPharmacy(pharmacyId).Rows.Count;
                    stats["pharmacyName"] = "MediStock";
                    stats["totalProducts"] = row["total_products"] != DBNull.Value ? Convert.ToInt32(row["total_products"]) : 0;
                    stats["alertCount"] = row["low_stock_count"] != DBNull.Value ? Convert.ToInt32(row["low_stock_count"]) : 0;
                }

                return Ok(new ApiResponse<object> { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetStats: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("system-info")]
        public IActionResult GetSystemInfo()
        {
            try
            {
                var info = new Dictionary<string, object?>
                {
                    ["version"] = "1.0.0",
                    ["databaseStatus"] = "Connected",
                    ["serverTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                return Ok(new ApiResponse<object> { success = true, data = info });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSystemInfo: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("recent-logins")]
        public IActionResult GetRecentLogins()
        {
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetUsersByPharmacy(pharmacyId);
                var users = DataTableToList(dt);
                return Ok(new ApiResponse<object> { success = true, data = users.Take(10) });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetRecentLogins: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("auditlog")]
        public IActionResult GetAuditLog([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            _logger.LogInfo("******* GET AUDIT LOG REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("audit_trail", "", pageSize.ToString());
                var log = DataTableToList(dt);
                return Ok(new ApiResponse<object> { success = true, data = log });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAuditLog: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [NonAction]
        private void CaptureAuditTrail(string email, string actionType, string description)
        {
            try
            {
                var model = new AuditTrailModel
                {
                    user_name = email,
                    action_type = actionType,
                    action_description = description,
                    page_accessed = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}",
                    client_ip_address = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    session_id = HttpContext.Session?.Id ?? "",
                    created_on = DateTime.UtcNow
                };
                dbhandler.AddAuditTrail(model);
            }
            catch (Exception ex)
            {
                _logger.LogError("CaptureAuditTrail: " + ex.Message);
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

        private static List<Dictionary<string, object?>> DataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object?>>();
            foreach (DataRow row in dt.Rows)
                list.Add(RowToDict(row));
            return list;
        }

        private static Dictionary<string, object?> RowToDict(DataRow row)
        {
            var dict = new Dictionary<string, object?>();
            foreach (DataColumn col in row.Table.Columns)
                dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            return dict;
        }
    }
}
