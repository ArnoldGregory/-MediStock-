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
    public class AccessController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public AccessController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        // =====================================================================
        // ROLES — GET ALL
        // =====================================================================
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
                iloggermanager.LogInfo($"GetRoles: returned {dt.Rows.Count} rows");

                return Ok(new { success = true, message = "Roles retrieved", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetRoles: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // ROLES — GET BY ID
        // =====================================================================
        [Authorize]
        [HttpGet("roles/{id}")]
        public ActionResult GetRoleById(int id)
        {
            iloggermanager.LogInfo("******* GET ROLE BY ID REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (id <= 0)
                    return Bad("id is required");

                DataTable dt = dbhandler.GetRecordsById("roles", id);
                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "Role not found", action = "", data = new JObject() });

                return Ok(new { success = true, message = "Role retrieved", action = "", data = RowToDict(dt.Rows[0]) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetRoleById: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // ROLES — CREATE
        // =====================================================================
        [Authorize]
        [HttpPost("roles")]
        public ActionResult CreateRole([FromBody] JObject body)
        {
            iloggermanager.LogInfo("******* CREATE ROLE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (roleId != 1 && roleId != 2)
                    return Forbidden("Only Admin or SuperAdmin can create roles");

                string roleName = body["role_name"]?.ToString()?.Trim() ?? "";
                string? description = body["description"]?.ToString()?.Trim();

                if (string.IsNullOrEmpty(roleName))
                    return Bad("role_name is required");

                string sql = "INSERT INTO roles (role_name, description, created_by, created_on) VALUES ('" +
                    roleName.Replace("'", "''") + "', '" +
                    (description ?? "").Replace("'", "''") + "', " +
                    userId + ", NOW())";

                dbhandler.GetAdhocData(sql);

                long newId = 0;
                DataTable dtId = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                if (dtId.Rows.Count > 0)
                    newId = Convert.ToInt64(dtId.Rows[0]["id"]);

                CaptureAuditTrail(userId.ToString(), "Create Role", $"Created role: {roleName}");
                iloggermanager.LogInfo($"CreateRole: id={newId} name={roleName}");

                return Ok(new { success = true, message = "Role created", action = "", data = new JObject { { "id", newId }, { "role_name", roleName } } });
            }
            catch (Exception ex) { iloggermanager.LogError("CreateRole: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // ROLES — UPDATE
        // =====================================================================
        [Authorize]
        [HttpPut("roles/{id}")]
        public ActionResult UpdateRole(int id, [FromBody] JObject body)
        {
            iloggermanager.LogInfo("******* UPDATE ROLE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (roleId != 1 && roleId != 2)
                    return Forbidden("Only Admin or SuperAdmin can update roles");

                if (id <= 0)
                    return Bad("id is required");

                string? roleName = body["role_name"]?.ToString()?.Trim();
                string? description = body["description"]?.ToString()?.Trim();

                string updates = "";
                if (!string.IsNullOrEmpty(roleName))
                    updates += "role_name = '" + roleName.Replace("'", "''") + "'";
                if (description != null)
                    updates += (updates.Length > 0 ? ", " : "") + "description = '" + description.Replace("'", "''") + "'";
                updates += ", updated_by = " + userId + ", updated_on = NOW()";

                if (string.IsNullOrEmpty(roleName) && description == null)
                    return Bad("No fields to update");

                string sql = "UPDATE roles SET " + updates + " WHERE id = " + id;
                dbhandler.GetAdhocData(sql);

                CaptureAuditTrail(userId.ToString(), "Update Role", $"Updated role {id}: {roleName}");
                iloggermanager.LogInfo($"UpdateRole: id={id} name={roleName}");

                return Ok(new { success = true, message = "Role updated", action = "", data = new JObject { { "id", id }, { "role_name", roleName } } });
            }
            catch (Exception ex) { iloggermanager.LogError("UpdateRole: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // ROLES — DELETE
        // =====================================================================
        [Authorize]
        [HttpDelete("roles/{id}")]
        public ActionResult DeleteRole(int id)
        {
            iloggermanager.LogInfo("******* DELETE ROLE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (roleId != 1)
                    return Forbidden("Only SuperAdmin can delete roles");

                if (id <= 0)
                    return Bad("id is required");

                bool deleted = dbhandler.DeleteRecord(id, userId, "roles");
                if (!deleted)
                    return Bad("Failed to delete role. Role may be in use.");

                CaptureAuditTrail(userId.ToString(), "Delete Role", $"Deleted role {id}");
                iloggermanager.LogInfo($"DeleteRole: id={id}");

                return Ok(new { success = true, message = "Role deleted", action = "", data = (object?)null });
            }
            catch (Exception ex) { iloggermanager.LogError("DeleteRole: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // MENUS — GET ALL
        // =====================================================================
        [Authorize]
        [HttpGet("menus")]
        public ActionResult GetMenus()
        {
            iloggermanager.LogInfo("******* GET MENUS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("menus", "0");
                iloggermanager.LogInfo($"GetMenus: returned {dt.Rows.Count} rows");

                return Ok(new { success = true, message = "Menus retrieved", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetMenus: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // MENU ACCESS — GET FOR ROLE
        // =====================================================================
        [Authorize]
        [HttpGet("menu-access")]
        public ActionResult GetMenuAccess([FromQuery] int roleId)
        {
            iloggermanager.LogInfo("******* GET MENU ACCESS REQUEST **********");
            try
            {
                var (userId, pharmacyId, callerRoleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={callerRoleId}");
                if (roleId <= 0)
                    return Bad("roleId is required");

                DataTable dt = dbhandler.GetRecords("menu_access", roleId.ToString());
                iloggermanager.LogInfo($"GetMenuAccess: roleId={roleId} returned {dt.Rows.Count} rows");

                return Ok(new { success = true, message = "Menu access retrieved", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetMenuAccess: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // MENU ACCESS — SAVE FOR ROLE
        // =====================================================================
        [Authorize]
        [HttpPost("menu-access")]
        public ActionResult SaveMenuAccess([FromBody] JObject body)
        {
            iloggermanager.LogInfo("******* SAVE MENU ACCESS REQUEST **********");
            try
            {
                var (userId, pharmacyId, callerRoleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={callerRoleId}");
                if (callerRoleId != 1 && callerRoleId != 2)
                    return Forbidden("Only Admin or SuperAdmin can manage menu access");

                int roleId = body["role_id"] != null ? Convert.ToInt32(body["role_id"]) : 0;
                if (roleId <= 0)
                    return Bad("role_id is required");

                JArray? menuIds = body["menu_ids"] as JArray;
                if (menuIds == null || menuIds.Count == 0)
                    return Bad("menu_ids array is required");

                // Delete existing access for this role, then re-insert
                string deleteSql = "DELETE FROM menu_access WHERE role_id = " + roleId;
                dbhandler.GetAdhocData(deleteSql);

                int count = 0;
                foreach (var item in menuIds)
                {
                    int menuId = Convert.ToInt32(item);
                    string insertSql = "INSERT INTO menu_access (role_id, menu_id, created_by, created_on) VALUES (" +
                        roleId + ", " + menuId + ", " + userId + ", NOW())";
                    dbhandler.GetAdhocData(insertSql);
                    count++;
                }

                CaptureAuditTrail(userId.ToString(), "Save Menu Access", $"Saved {count} menu items for role {roleId}");
                iloggermanager.LogInfo($"SaveMenuAccess: roleId={roleId} saved {count} items");

                return Ok(new { success = true, message = "Menu access saved", action = "", data = new JObject { { "role_id", roleId }, { "count", count } } });
            }
            catch (Exception ex) { iloggermanager.LogError("SaveMenuAccess: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [NonAction]
        private List<Dictionary<string, object>> ToRows(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns) row[col.ColumnName] = dr[col];
                rows.Add(row);
            }
            return rows;
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
        private static Dictionary<string, object?> RowToDict(DataRow row)
        {
            var dict = new Dictionary<string, object?>();
            foreach (DataColumn col in row.Table.Columns)
                dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            return dict;
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
