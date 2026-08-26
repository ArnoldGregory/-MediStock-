using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/access")]
    public class AccessController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public AccessController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        // =====================================================================
        // ROLES — GET ALL
        // =====================================================================
        [Authorize]
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            _logger.LogInfo("******* GET ROLES REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("roles", "0");
                _logger.LogInfo($"GetRoles: returned {dt.Rows.Count} rows");

                var list = DataTableToList(dt);
                return Ok(new ApiResponse<object> { success = true, message = "Roles retrieved", data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetRoles: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // ROLES — GET BY ID
        // =====================================================================
        [Authorize]
        [HttpGet("roles/{id}")]
        public IActionResult GetRoleById(int id)
        {
            _logger.LogInfo("******* GET ROLE BY ID REQUEST **********");
            try
            {
                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                DataTable dt = dbhandler.GetRecordsById("roles", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Role not found" });

                return Ok(new ApiResponse<object> { success = true, message = "Role retrieved", data = RowToDict(dt.Rows[0]) });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetRoleById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // ROLES — CREATE
        // =====================================================================
        [Authorize]
        [HttpPost("roles")]
        public IActionResult CreateRole([FromBody] JObject body)
        {
            _logger.LogInfo("******* CREATE ROLE REQUEST **********");
            try
            {
                int callerRoleId = GetCallerRoleId();
                if (callerRoleId != 1 && callerRoleId != 2)
                    return StatusCode(403, new ApiResponse<object> { success = false, message = "Only Admin or SuperAdmin can create roles" });

                string roleName = body["role_name"]?.ToString()?.Trim() ?? "";
                string? description = body["description"]?.ToString()?.Trim();

                if (string.IsNullOrEmpty(roleName))
                    return BadRequest(new ApiResponse<object> { success = false, message = "role_name is required" });

                string sql = "INSERT INTO roles (role_name, description, created_by, created_on) VALUES ('" +
                    roleName.Replace("'", "''") + "', '" +
                    (description ?? "").Replace("'", "''") + "', " +
                    GetCallerUserId() + ", NOW())";

                dbhandler.GetAdhocData(sql);

                long newId = 0;
                DataTable dtId = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                if (dtId.Rows.Count > 0)
                    newId = Convert.ToInt64(dtId.Rows[0]["id"]);

                CaptureAuditTrail(GetCallerEmail(), "Create Role", $"Created role: {roleName}");
                _logger.LogInfo($"CreateRole: id={newId} name={roleName}");

                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Role created",
                    data = new { id = newId, role_name = roleName }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateRole: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // ROLES — UPDATE
        // =====================================================================
        [Authorize]
        [HttpPut("roles/{id}")]
        public IActionResult UpdateRole(int id, [FromBody] JObject body)
        {
            _logger.LogInfo("******* UPDATE ROLE REQUEST **********");
            try
            {
                int callerRoleId = GetCallerRoleId();
                if (callerRoleId != 1 && callerRoleId != 2)
                    return StatusCode(403, new ApiResponse<object> { success = false, message = "Only Admin or SuperAdmin can update roles" });

                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                string? roleName = body["role_name"]?.ToString()?.Trim();
                string? description = body["description"]?.ToString()?.Trim();

                string updates = "";
                if (!string.IsNullOrEmpty(roleName))
                    updates += "role_name = '" + roleName.Replace("'", "''") + "'";
                if (description != null)
                    updates += (updates.Length > 0 ? ", " : "") + "description = '" + description.Replace("'", "''") + "'";
                updates += ", updated_by = " + GetCallerUserId() + ", updated_on = NOW()";

                if (string.IsNullOrEmpty(roleName) && description == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "No fields to update" });

                string sql = "UPDATE roles SET " + updates + " WHERE id = " + id;
                dbhandler.GetAdhocData(sql);

                CaptureAuditTrail(GetCallerEmail(), "Update Role", $"Updated role {id}: {roleName}");
                _logger.LogInfo($"UpdateRole: id={id} name={roleName}");

                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Role updated",
                    data = new { id = id, role_name = roleName }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateRole: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // ROLES — DELETE
        // =====================================================================
        [Authorize]
        [HttpDelete("roles/{id}")]
        public IActionResult DeleteRole(int id)
        {
            _logger.LogInfo("******* DELETE ROLE REQUEST **********");
            try
            {
                int callerRoleId = GetCallerRoleId();
                if (callerRoleId != 1)
                    return StatusCode(403, new ApiResponse<object> { success = false, message = "Only SuperAdmin can delete roles" });

                if (id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "id is required" });

                bool deleted = dbhandler.DeleteRecord(id, GetCallerUserId(), "roles");
                if (!deleted)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Failed to delete role. Role may be in use." });

                CaptureAuditTrail(GetCallerEmail(), "Delete Role", $"Deleted role {id}");
                _logger.LogInfo($"DeleteRole: id={id}");

                return Ok(new ApiResponse<object> { success = true, message = "Role deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteRole: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // MENUS — GET ALL
        // =====================================================================
        [Authorize]
        [HttpGet("menus")]
        public IActionResult GetMenus()
        {
            _logger.LogInfo("******* GET MENUS REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("menus", "0");
                _logger.LogInfo($"GetMenus: returned {dt.Rows.Count} rows");

                var list = DataTableToList(dt);
                return Ok(new ApiResponse<object> { success = true, message = "Menus retrieved", data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetMenus: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // MENU ACCESS — GET FOR ROLE
        // =====================================================================
        [Authorize]
        [HttpGet("menu-access")]
        public IActionResult GetMenuAccess([FromQuery] int roleId)
        {
            _logger.LogInfo("******* GET MENU ACCESS REQUEST **********");
            try
            {
                if (roleId <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "roleId is required" });

                DataTable dt = dbhandler.GetRecords("menu_access", roleId.ToString());
                _logger.LogInfo($"GetMenuAccess: roleId={roleId} returned {dt.Rows.Count} rows");

                var list = DataTableToList(dt);
                return Ok(new ApiResponse<object> { success = true, message = "Menu access retrieved", data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetMenuAccess: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // MENU ACCESS — SAVE FOR ROLE
        // =====================================================================
        [Authorize]
        [HttpPost("menu-access")]
        public IActionResult SaveMenuAccess([FromBody] JObject body)
        {
            _logger.LogInfo("******* SAVE MENU ACCESS REQUEST **********");
            try
            {
                int callerRoleId = GetCallerRoleId();
                if (callerRoleId != 1 && callerRoleId != 2)
                    return StatusCode(403, new ApiResponse<object> { success = false, message = "Only Admin or SuperAdmin can manage menu access" });

                int roleId = body["role_id"] != null ? Convert.ToInt32(body["role_id"]) : 0;
                if (roleId <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "role_id is required" });

                JArray? menuIds = body["menu_ids"] as JArray;
                if (menuIds == null || menuIds.Count == 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "menu_ids array is required" });

                // Delete existing access for this role, then re-insert
                string deleteSql = "DELETE FROM menu_access WHERE role_id = " + roleId;
                dbhandler.GetAdhocData(deleteSql);

                int count = 0;
                foreach (var item in menuIds)
                {
                    int menuId = Convert.ToInt32(item);
                    string insertSql = "INSERT INTO menu_access (role_id, menu_id, created_by, created_on) VALUES (" +
                        roleId + ", " + menuId + ", " + GetCallerUserId() + ", NOW())";
                    dbhandler.GetAdhocData(insertSql);
                    count++;
                }

                CaptureAuditTrail(GetCallerEmail(), "Save Menu Access", $"Saved {count} menu items for role {roleId}");
                _logger.LogInfo($"SaveMenuAccess: roleId={roleId} saved {count} items");

                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Menu access saved",
                    data = new { role_id = roleId, count = count }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("SaveMenuAccess: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // AUDIT TRAIL — GET
        // =====================================================================
        [Authorize]
        [HttpGet("audit")]
        public IActionResult GetAuditTrail([FromQuery] int pageSize = 50)
        {
            _logger.LogInfo("******* GET AUDIT TRAIL REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("audit_trail", "", pageSize.ToString());
                _logger.LogInfo($"GetAuditTrail: returned {dt.Rows.Count} rows");

                var list = DataTableToList(dt);
                return Ok(new ApiResponse<object> { success = true, message = "Audit trail retrieved", data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAuditTrail: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object> { success = false, message = "Server error" });
            }
        }

        // =====================================================================
        // HELPERS
        // =====================================================================
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
