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
            try
            {
                var callerPharmacyId = GetCallerPharmacyId();
                var callerRoleId = GetCallerRoleId();
                if (callerRoleId == 1) pharmacyId = callerPharmacyId;
                else if (pharmacyId == 0) pharmacyId = callerPharmacyId;

                DataTable dt = dbhandler.GetUsersByPharmacy(pharmacyId);
                var users = DataTableToList(dt);
                return Ok(new ApiResponse<object> { success = true, data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetUsers: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("getuser")]
        public IActionResult GetUser([FromQuery] long id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                DataTable dt = dbhandler.GetUserById(id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "User not found" });

                return Ok(new ApiResponse<object> { success = true, data = RowToDict(dt.Rows[0]) });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetUser: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpPost("adduser")]
        public IActionResult AddUser([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                string firstName = body["first_name"]?.ToString() ?? "";
                string lastName = body["last_name"]?.ToString() ?? "";
                string email = body["email"]?.ToString() ?? "";
                string? phone = body["phone"]?.ToString();
                int roleId = body["role_id"] != null ? Convert.ToInt32(body["role_id"]) : 3;
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

                return Ok(new ApiResponse<object> { success = true, message = "User created" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddUser: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpPost("updateuser")]
        public IActionResult UpdateUser([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            try
            {
                long id = body["id"] != null ? Convert.ToInt64(body["id"]) : 0;
                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                string? firstName = body["first_name"]?.ToString();
                string? lastName = body["last_name"]?.ToString();
                string? email = body["email"]?.ToString();
                string? phone = body["phone"]?.ToString();
                int? roleId = body["role_id"] != null ? Convert.ToInt32(body["role_id"]) : null;
                bool isActive = body["is_active"] != null ? Convert.ToBoolean(body["is_active"]) : true;

                bool updated = dbhandler.AdminUpdateUser(id, firstName, lastName, email, phone, roleId, isActive);
                if (!updated)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Failed to update user" });

                return Ok(new ApiResponse<object> { success = true, message = "User updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateUser: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpPost("deleteuser")]
        public IActionResult DeleteUser([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            try
            {
                long id = body["id"] != null ? Convert.ToInt64(body["id"]) : 0;
                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                bool deleted = dbhandler.DeleteRecord(id, GetCallerUserId(), "pharmacy_user");
                return Ok(new ApiResponse<object>
                {
                    success = deleted,
                    message = deleted ? "User deleted" : "Failed to delete user"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteUser: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpPost("resetpassword")]
        public IActionResult ResetPassword([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
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

                return Ok(new ApiResponse<object> { success = true, message = "Password reset" });
            }
            catch (Exception ex)
            {
                _logger.LogError("ResetPassword: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        [Authorize]
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = new[]
            {
                new { id = 1, name = "SuperAdmin" },
                new { id = 2, name = "Admin" },
                new { id = 3, name = "Pharmacist" },
                new { id = 4, name = "Staff" },
                new { id = 5, name = "Cashier" },
            };
            return Ok(new ApiResponse<object> { success = true, data = roles });
        }

        [Authorize]
        [HttpGet("auditlog")]
        public IActionResult GetAuditLog([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                DataTable dt = dbhandler.GetRecords("audit_trail", "", pageSize.ToString());
                var log = DataTableToList(dt);
                return Ok(new ApiResponse<object> { success = true, data = log });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAuditLog: " + ex.Message);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
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
