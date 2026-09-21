using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public SettingsController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetSettings()
        {
            iloggermanager.LogInfo("******* GET SETTINGS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                var profile = new JObject();
                DataTable pt = dbhandler.GetAdhocData("SELECT id, name, slug, phone, email, address, license_number, license_expiry, vat_number, receipt_footer, currency FROM pharmacies WHERE id=@pharmacyId AND is_deleted=0", new[] { new MySqlParameter("@pharmacyId", pharmacyId) });
                if (pt.Rows.Count > 0)
                {
                    DataRow r = pt.Rows[0];
                    foreach (DataColumn col in pt.Columns)
                        profile[col.ColumnName] = r[col] == DBNull.Value ? JValue.CreateNull() : JToken.FromObject(r[col]);
                }

                DataTable ct = dbhandler.GetAdhocData("SELECT config_key, config_value FROM pharmacy_config WHERE pharmacy_id=@pharmacyId", new[] { new MySqlParameter("@pharmacyId", pharmacyId) });
                foreach (DataRow r in ct.Rows)
                    profile[Convert.ToString(r["config_key"])] = r["config_value"] == DBNull.Value ? JValue.CreateNull() : JToken.FromObject(r["config_value"]);

                iloggermanager.LogInfo($"Result: profile_keys={string.Join(",", ((JObject)profile).Properties().Select(p => p.Name))}");
                return Ok(new { success = true, message = "Success", action = "", data = profile });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSettings: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("profile")]
        public async Task<ActionResult> UpdatePharmacyProfile([FromBody] PharmacyModel model)
        {
            iloggermanager.LogInfo("******* UPDATE PHARMACY PROFILE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null || string.IsNullOrEmpty(model.name)) return Bad("Pharmacy name is required");

                string sql = "UPDATE pharmacies SET " +
                            "name=@name, " +
                            "phone=@phone, " +
                            "email=@email, " +
                            "address=@address, " +
                            "license_number=@licenseNo, " +
                            "vat_number=@vatNumber, " +
                            "receipt_footer=@receiptFooter, " +
                            "currency=@currency " +
                            "WHERE id=@pharmacyId";

                await dbhandler.ExecuteNonQuery(sql, new
                {
                    name = model.name ?? "",
                    phone = model.phone ?? "",
                    email = model.email ?? "",
                    address = model.address ?? "",
                    licenseNo = model.license_number ?? "",
                    vatNumber = model.vat_number ?? "",
                    receiptFooter = model.receipt_footer ?? "",
                    currency = model.currency ?? "",
                    pharmacyId
                });

                iloggermanager.LogInfo($"UpdatePharmacyProfile: pharmacyId={pharmacyId}");
                CaptureAuditTrail(userId.ToString(), "Update Pharmacy Profile", $"Updated pharmacy profile {pharmacyId}");
                return Ok(new { success = true, message = "Pharmacy profile updated successfully", action = "", data = (object?)null });
            }
            catch (Exception ex) { iloggermanager.LogError("UpdatePharmacyProfile: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("config")]
        public async Task<ActionResult> SavePharmacySetting([FromBody] Newtonsoft.Json.Linq.JObject jobject)
        {
            iloggermanager.LogInfo("******* SAVE PHARMACY SETTING REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (jobject == null)
                    return Bad("Invalid request");

                string key = jobject["key"]?.ToString() ?? "";
                string value = jobject["value"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(key))
                    return Bad("Setting key is required");

                string checkSql = "SELECT id FROM pharmacy_config WHERE pharmacy_id=@pharmacyId AND config_key=@key";
                DataTable existing = dbhandler.GetAdhocData(checkSql, new[] { new MySqlParameter("@pharmacyId", pharmacyId), new MySqlParameter("@key", key) });

                if (existing.Rows.Count > 0)
                {
                    string updateSql = "UPDATE pharmacy_config SET config_value=@value, updated_at=NOW(), updated_by=@userId WHERE pharmacy_id=@pharmacyId AND config_key=@key";
                    await dbhandler.ExecuteNonQuery(updateSql, new { value, userId, pharmacyId, key });
                }
                else
                {
                    string insertSql = "INSERT INTO pharmacy_config (pharmacy_id, config_key, config_value, created_by) VALUES (@pharmacyId, @key, @value, @userId)";
                    await dbhandler.ExecuteNonQuery(insertSql, new { pharmacyId, key, value, userId });
                }

                iloggermanager.LogInfo($"SavePharmacySetting: pharmacyId={pharmacyId} key={key}");
                CaptureAuditTrail(userId.ToString(), "Save Pharmacy Setting", $"Saved setting: {key}");
                return Ok(new { success = true, message = "Setting saved successfully", action = "", data = (object?)null });
            }
            catch (Exception ex) { iloggermanager.LogError("SavePharmacySetting: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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