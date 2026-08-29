using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public AdminController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet("users")]
        public ActionResult GetUsers([FromQuery] long pharmacyId = 0)
        {
            iloggermanager.LogInfo("******* GET USERS REQUEST **********");
            try
            {
                var (userId, callerPharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={callerPharmacyId}, role={roleId}");
                if (roleId == 1) pharmacyId = callerPharmacyId;
                else if (pharmacyId == 0) pharmacyId = callerPharmacyId;

                iloggermanager.LogInfo($"GetUsers: pharmacyId={pharmacyId}");

                DataTable dtPortal = dbhandler.GetUsersByPharmacy(pharmacyId);
                DataTable dtExternal = dbhandler.GetExternalUsersByPharmacy(pharmacyId);

                var users = new List<Dictionary<string, object?>>();
                foreach (DataRow row in dtPortal.Rows)
                    users.Add(RowToDict(row));
                foreach (DataRow row in dtExternal.Rows)
                    users.Add(RowToDict(row));

                CaptureAuditTrail(userId.ToString(), "View Users", $"Viewed user list for pharmacy {pharmacyId}");
                return Ok(new { success = true, message = "Success", action = "", data = users });
            }
            catch (Exception ex) { iloggermanager.LogError("GetUsers: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("users/{id}")]
        public ActionResult GetUserById(long id)
        {
            iloggermanager.LogInfo("******* GET USER BY ID REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (id <= 0) return Bad("id is required");

                DataTable dt = dbhandler.GetUserById(id);
                if (dt.Rows.Count == 0)
                {
                    dt = dbhandler.GetExternalUserById(id);
                }

                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "User not found", action = "", data = new JObject() });

                return Ok(new { success = true, message = "Success", action = "", data = RowToDict(dt.Rows[0]) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetUserById: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("getuser")]
        public ActionResult GetUser([FromQuery] long id)
        {
            return GetUserById(id);
        }

        [Authorize]
        [HttpPost("users")]
        public ActionResult CreateUser([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            iloggermanager.LogInfo("******* CREATE USER REQUEST **********");
            iloggermanager.LogInfo($"CreateUser: body={body}");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                string firstName = body["firstName"]?.ToString() ?? body["first_name"]?.ToString() ?? "";
                string lastName = body["lastName"]?.ToString() ?? body["last_name"]?.ToString() ?? "";
                string email = body["email"]?.ToString() ?? "";
                string? phone = body["phone"]?.ToString();
                int newRoleId = body["roleId"] != null ? Convert.ToInt32(body["roleId"]) : (body["role_id"] != null ? Convert.ToInt32(body["role_id"]) : 3);
                string password = body["password"]?.ToString() ?? "password";

                if (string.IsNullOrEmpty(email)) return Bad("Email is required");

                string storedPassword = newRoleId is 1 or 2
                    ? new CryptoHelper.MediSecurity.Rijndael().Encrypt(password)
                    : BCrypt.Net.BCrypt.HashPassword(password);

                var user = new PharmacyUserModel
                {
                    pharmacy_id = pharmacyId,
                    role_id = newRoleId,
                    first_name = firstName,
                    last_name = lastName,
                    email = email,
                    mobile = phone,
                    phone = phone,
                    password = storedPassword,
                    created_by = userId
                };

                bool created = dbhandler.AddUser(user);
                if (!created) return Bad("Failed to create user. Email may already exist.");

                CaptureAuditTrail(userId.ToString(), "Create User", $"Created user: {email} (role: {newRoleId})");
                return Ok(new { success = true, message = "User created", action = "", data = (object?)null });
            }
            catch (Exception ex) { iloggermanager.LogError("CreateUser: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("adduser")]
        public ActionResult AddUser([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            return CreateUser(body);
        }

        [Authorize]
        [HttpPut("users/{id}")]
        public ActionResult UpdateUser(long id, [FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            iloggermanager.LogInfo("******* UPDATE USER REQUEST **********");
            iloggermanager.LogInfo($"UpdateUser: id={id}, body={body}");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (id <= 0) return Bad("id is required");

                string? firstName = body["firstName"]?.ToString() ?? body["first_name"]?.ToString();
                string? lastName = body["lastName"]?.ToString() ?? body["last_name"]?.ToString();
                string? email = body["email"]?.ToString();
                string? phone = body["phone"]?.ToString();
                int? newRoleId = body["roleId"] != null ? Convert.ToInt32(body["roleId"]) : (body["role_id"] != null ? Convert.ToInt32(body["role_id"]) : null);
                bool isActive = body["isActive"] != null ? Convert.ToBoolean(body["isActive"]) : (body["is_active"] != null ? Convert.ToBoolean(body["is_active"]) : true);

                bool updated = dbhandler.AdminUpdateUser(id, firstName, lastName, email, phone, newRoleId, isActive);
                if (!updated) return Bad("Failed to update user");

                CaptureAuditTrail(userId.ToString(), "Update User", $"Updated user {id}: {email}");
                return Ok(new { success = true, message = "User updated", action = "", data = (object?)null });
            }
            catch (Exception ex) { iloggermanager.LogError("UpdateUser: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("updateuser")]
        public ActionResult UpdateUserLegacy([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            long id = body["id"] != null ? Convert.ToInt64(body["id"]) : 0;
            return UpdateUser(id, body);
        }

        [Authorize]
        [HttpDelete("users/{id}")]
        public ActionResult DeleteUser(long id)
        {
            iloggermanager.LogInfo("******* DELETE USER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (id <= 0) return Bad("id is required");

                bool deleted = dbhandler.DeleteRecord(id, userId, "pharmacy_user");
                if (!deleted)
                    deleted = dbhandler.DeleteRecord(id, userId, "portal_user");
                if (!deleted)
                    deleted = dbhandler.DeleteRecord(id, userId, "p_external_portal_user");

                CaptureAuditTrail(userId.ToString(), "Delete User", $"Deleted user {id}");
                return Ok(new { success = deleted, message = deleted ? "User deleted" : "Failed to delete user", action = "", data = new JObject { { "id", id } } });
            }
            catch (Exception ex) { iloggermanager.LogError("DeleteUser: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("deleteuser")]
        public ActionResult DeleteUserLegacy([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            long id = body["id"] != null ? Convert.ToInt64(body["id"]) : 0;
            return DeleteUser(id);
        }

        [Authorize]
        [HttpPost("resetpassword")]
        public ActionResult ResetPassword([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            iloggermanager.LogInfo("******* RESET PASSWORD REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                long targetUserId = body["user_id"] != null ? Convert.ToInt64(body["user_id"]) : 0;
                string newPassword = body["new_password"]?.ToString() ?? "password";

                if (targetUserId <= 0) return Bad("user_id is required");

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                bool reset = dbhandler.AdminResetPassword(targetUserId, hashedPassword);
                if (!reset) return Bad("Failed to reset password");

                CaptureAuditTrail(userId.ToString(), "Reset Password", $"Reset password for user {targetUserId}");
                return Ok(new { success = true, message = "Password reset", action = "", data = (object?)null });
            }
            catch (Exception ex) { iloggermanager.LogError("ResetPassword: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("roles")]
        public ActionResult GetRoles()
        {
            iloggermanager.LogInfo("******* GET ROLES REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
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
                    return Ok(new { success = true, message = "Success", action = "", data = roles });
                }

                var roleList = new List<Dictionary<string, object?>>();
                foreach (DataRow row in dt.Rows)
                    roleList.Add(RowToDict(row));
                return Ok(new { success = true, message = "Success", action = "", data = roleList });
            }
            catch (Exception ex) { iloggermanager.LogError("GetRoles: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("stats")]
        public ActionResult GetStats()
        {
            iloggermanager.LogInfo("******* GET ADMIN STATS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
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

                return Ok(new { success = true, message = "Success", action = "", data = stats });
            }
            catch (Exception ex) { iloggermanager.LogError("GetStats: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("system-info")]
        public ActionResult GetSystemInfo()
        {
            try
            {
                var info = new Dictionary<string, object?>
                {
                    ["version"] = "1.0.0",
                    ["databaseStatus"] = "Connected",
                    ["serverTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                return Ok(new { success = true, message = "Success", action = "", data = info });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSystemInfo: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("recent-logins")]
        public ActionResult GetRecentLogins()
        {
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetUsersByPharmacy(pharmacyId);
                var users = DataTableToList(dt);
                return Ok(new { success = true, message = "Success", action = "", data = users.Take(10) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetRecentLogins: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [NonAction]
        private List<Dictionary<string, object?>> DataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object?>>();
            foreach (DataRow row in dt.Rows)
                list.Add(RowToDict(row));
            return list;
        }

        [NonAction]
        private static Dictionary<string, object?> RowToDict(DataRow row)
        {
            var dict = new Dictionary<string, object?>();
            foreach (DataColumn col in row.Table.Columns)
                dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            return dict;
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
                client_ip_address = Request.HttpContext.Connection.RemoteIpAddress!.ToString(),
                session_id = "TODO"
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}
