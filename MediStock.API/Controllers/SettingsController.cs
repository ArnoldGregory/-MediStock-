using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
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
                DataTable pt = dbhandler.GetAdhocData($"SELECT id, name, slug, phone, email, address, license_number, license_expiry, vat_number, receipt_footer, currency FROM pharmacies WHERE id={pharmacyId} AND is_deleted=0");
                if (pt.Rows.Count > 0)
                {
                    DataRow r = pt.Rows[0];
                    foreach (DataColumn col in pt.Columns)
                        profile[col.ColumnName] = r[col] == DBNull.Value ? JValue.CreateNull() : JToken.FromObject(r[col]);
                }

                DataTable ct = dbhandler.GetAdhocData($"SELECT config_key, config_value FROM pharmacy_config WHERE pharmacy_id={pharmacyId}");
                foreach (DataRow r in ct.Rows)
                    profile[Convert.ToString(r["config_key"])] = r["config_value"] == DBNull.Value ? JValue.CreateNull() : JToken.FromObject(r["config_value"]);

                iloggermanager.LogInfo($"Result: profile_keys={string.Join(",", ((JObject)profile).Properties().Select(p => p.Name))}");
                return Ok(new { success = true, message = "Success", action = "", data = profile });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSettings: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("profile")]
        public ActionResult UpdatePharmacyProfile([FromBody] PharmacyModel model)
        {
            iloggermanager.LogInfo("******* UPDATE PHARMACY PROFILE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null || string.IsNullOrEmpty(model.name)) return Bad("Pharmacy name is required");

                string sql = $"UPDATE pharmacies SET " +
                    $"name='{model.name.Replace("'", "''")}', " +
                    $"phone='{(model.phone ?? "").Replace("'", "''")}', " +
                    $"email='{(model.email ?? "").Replace("'", "''")}', " +
                    $"address='{(model.address ?? "").Replace("'", "''")}', " +
                    $"license_number='{(model.license_number ?? "").Replace("'", "''")}', " +
                    $"vat_number='{(model.vat_number ?? "").Replace("'", "''")}', " +
                    $"receipt_footer='{(model.receipt_footer ?? "").Replace("'", "''")}', " +
                    $"currency='{(model.currency).Replace("'", "''")}' " +
                    $"WHERE id={pharmacyId}";

                dbhandler.ExecuteNonQuery(sql);

                iloggermanager.LogInfo($"UpdatePharmacyProfile: pharmacyId={pharmacyId}");
                CaptureAuditTrail(userId.ToString(), "Update Pharmacy Profile", $"Updated pharmacy profile {pharmacyId}");
                return Ok(new { success = true, message = "Pharmacy profile updated successfully", action = "", data = (object?)null });
            }
            catch (Exception ex) { iloggermanager.LogError("UpdatePharmacyProfile: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("config")]
        public ActionResult SavePharmacySetting([FromBody] Newtonsoft.Json.Linq.JObject jobject)
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

                string escapedKey = key.Replace("'", "''");
                string escapedValue = value.Replace("'", "''");

                string checkSql = $"SELECT id FROM pharmacy_config WHERE pharmacy_id={pharmacyId} AND config_key='{escapedKey}'";
                DataTable existing = dbhandler.GetAdhocData(checkSql);

                if (existing.Rows.Count > 0)
                {
                    string updateSql = $"UPDATE pharmacy_config SET config_value='{escapedValue}', updated_at=NOW(), updated_by={userId} WHERE pharmacy_id={pharmacyId} AND config_key='{escapedKey}'";
                    dbhandler.ExecuteNonQuery(updateSql);
                }
                else
                {
                    string insertSql = $"INSERT INTO pharmacy_config (pharmacy_id, config_key, config_value, created_by) VALUES ({pharmacyId}, '{escapedKey}', '{escapedValue}', {userId})";
                    dbhandler.ExecuteNonQuery(insertSql);
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