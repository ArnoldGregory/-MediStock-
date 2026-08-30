using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/superadmin")]
    public class SuperAdminController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public SuperAdminController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        public class AddPharmacyRequest
        {
            public string name { get; set; } = "";
            public string slug { get; set; } = "";
            public string? phone { get; set; }
            public string? email { get; set; }
            public string? address { get; set; }
            public string? license_number { get; set; }
            public string currency { get; set; } = "KES";
            public string? owner_first_name { get; set; }
            public string? owner_last_name { get; set; }
            public string owner_email { get; set; } = "";
            public string? owner_mobile { get; set; }
            public string password { get; set; } = "";
        }

        public class UpdatePharmacyStatusRequest
        {
            public long id { get; set; }
            public bool is_active { get; set; }
        }

        // =====================================================================
        // Guard — superadmin only (role_id = 1)
        // =====================================================================
        private bool IsSuperAdmin(out Int64 userId)
        {
            userId = Convert.ToInt64(HttpContext.Items["user_id"]?.ToString() ?? "0");
            Int64 roleId = Convert.ToInt64(HttpContext.Items["profile_id"]?.ToString() ?? "0");
            return roleId == 1;
        }

        // =====================================================================
        // GET api/superadmin/pharmacies
        // =====================================================================
        [Authorize]
        [HttpGet("pharmacies")]
        public ActionResult GetPharmacies()
        {
            iloggermanager.LogInfo("******* GET ALL PHARMACIES REQUEST **********");
            try
            {
                if (!IsSuperAdmin(out long userId))
                    return Forbidden("Only SuperAdmin can access this resource");
                DataTable dt = dbhandler.GetAllPharmacies();
                iloggermanager.LogInfo($"GetPharmacies: returned {dt.Rows.Count} rows");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetPharmacies: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // GET api/superadmin/users
        // =====================================================================
        [Authorize]
        [HttpGet("users")]
        public ActionResult GetUsers()
        {
            iloggermanager.LogInfo("******* GET ALL USERS REQUEST **********");
            try
            {
                if (!IsSuperAdmin(out long userId))
                    return Forbidden("Only SuperAdmin can access this resource");
                DataTable dt = dbhandler.GetAllUsers(userId);
                iloggermanager.LogInfo($"GetUsers: returned {dt.Rows.Count} rows");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetUsers: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // GET api/superadmin/stats
        // =====================================================================
        [Authorize]
        [HttpGet("stats")]
        public ActionResult GetStats()
        {
            iloggermanager.LogInfo("******* GET PLATFORM STATS REQUEST **********");
            try
            {
                if (!IsSuperAdmin(out long userId))
                    return Forbidden("Only SuperAdmin can access this resource");
                DataTable dt = dbhandler.GetPlatformStats();
                var stats = new Dictionary<string, object?>();
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    foreach (DataColumn col in dt.Columns)
                        stats[col.ColumnName] = row[col] == DBNull.Value ? 0 : row[col];
                }
                return Ok(new { success = true, message = "Success", action = "", data = stats });
            }
            catch (Exception ex) { iloggermanager.LogError("GetStats: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // GET api/superadmin/audit?limit=50
        // =====================================================================
        [Authorize]
        [HttpGet("audit")]
        public ActionResult GetAudit([FromQuery] int limit = 50)
        {
            iloggermanager.LogInfo("******* GET PLATFORM AUDIT REQUEST **********");
            try
            {
                if (!IsSuperAdmin(out long userId))
                    return Forbidden("Only SuperAdmin can access this resource");
                if (limit <= 0 || limit > 500) limit = 50;
                DataTable dt = dbhandler.GetPlatformAudit(limit);
                iloggermanager.LogInfo($"GetAudit: returned {dt.Rows.Count} rows");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetAudit: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // POST api/superadmin/addpharmacy
        // =====================================================================
        [Authorize]
        [HttpPost("addpharmacy")]
        public ActionResult AddPharmacy([FromBody] AddPharmacyRequest req)
        {
            iloggermanager.LogInfo("******* ADD PHARMACY (PLATFORM) REQUEST **********");
            try
            {
                if (!IsSuperAdmin(out long userId))
                    return Forbidden("Only SuperAdmin can create pharmacies");
                if (req == null) return Bad("Invalid request");
                if (string.IsNullOrEmpty(req.name) || string.IsNullOrEmpty(req.owner_email) || string.IsNullOrEmpty(req.password))
                    return Bad("Pharmacy name, owner email and password are required");

                string slug = string.IsNullOrEmpty(req.slug)
                    ? req.name.Trim().ToLowerInvariant().Replace(" ", "-").Replace("'", "").Replace("\"", "")
                    : req.slug.Trim().ToLowerInvariant().Replace(" ", "-").Replace("'", "").Replace("\"", "");
                if (string.IsNullOrEmpty(slug)) return Bad("A valid pharmacy slug is required");

                var (pharmacyId, newUserId, errorCode, errorDesc) = dbhandler.AddPharmacyPlatform(
                    req.name.Trim(),
                    slug,
                    req.phone,
                    req.email,
                    req.address,
                    req.license_number,
                    req.currency,
                    req.owner_first_name,
                    req.owner_last_name,
                    req.owner_email.Trim(),
                    req.owner_mobile,
                    BCrypt.Net.BCrypt.HashPassword(req.password),
                    new CryptoHelper.MediSecurity.Rijndael().Encrypt(req.password),
                    userId);

                if (pharmacyId > 0 && newUserId > 0)
                {
                    iloggermanager.LogInfo($"AddPharmacy: pharmacyId={pharmacyId} userId={newUserId}");
                    CaptureAuditTrail(userId.ToString(), "Add Pharmacy", $"Created pharmacy '{req.name}' (id={pharmacyId}) with owner '{req.owner_email}'");
                    return Ok(new { success = true, message = "Pharmacy created successfully. Owner can now log in.", action = "", data = new JObject { { "pharmacy_id", pharmacyId }, { "user_id", newUserId } } });
                }

                string err = errorDesc;
                return Bad(string.IsNullOrEmpty(err) || err == "OK" ? "Failed to create pharmacy" : err);
            }
            catch (Exception ex) { iloggermanager.LogError("AddPharmacy: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        // =====================================================================
        // POST api/superadmin/updatepharmacystatus
        // =====================================================================
        [Authorize]
        [HttpPost("updatepharmacystatus")]
        public ActionResult UpdatePharmacyStatus([FromBody] UpdatePharmacyStatusRequest req)
        {
            iloggermanager.LogInfo("******* UPDATE PHARMACY STATUS REQUEST **********");
            try
            {
                if (!IsSuperAdmin(out long userId))
                    return Forbidden("Only SuperAdmin can update pharmacy status");
                if (req == null || req.id <= 0) return Bad("id is required");

                var (success, message) = dbhandler.UpdatePharmacyStatus(req.id, req.is_active);
                if (success)
                {
                    iloggermanager.LogInfo($"UpdatePharmacyStatus: pharmacyId={req.id} active={req.is_active}");
                    CaptureAuditTrail(userId.ToString(), "Update Pharmacy Status", $"Set pharmacy {req.id} active={req.is_active}");
                    return Ok(new { success = true, message = "Pharmacy status updated", action = "", data = new JObject { { "id", req.id }, { "is_active", req.is_active } } });
                }
                return Bad(string.IsNullOrEmpty(message) ? "Failed to update pharmacy status" : message);
            }
            catch (Exception ex) { iloggermanager.LogError("UpdatePharmacyStatus: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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
                session_id = HttpContext.TraceIdentifier
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}
