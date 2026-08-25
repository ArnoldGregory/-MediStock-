using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public SettingsController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetSettings()
        {
            _logger.LogInfo("******* GET SETTINGS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("pharmacy_settings", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                if (dt.Rows.Count == 0)
                    return Ok(new ApiResponse<object> { success = true, data = new { } });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSettings: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("profile")]
        public IActionResult UpdatePharmacyProfile([FromBody] PharmacyModel model)
        {
            _logger.LogInfo("******* UPDATE PHARMACY PROFILE REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || string.IsNullOrEmpty(model.name))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Pharmacy name is required" });

                string sql = $"UPDATE pharmacies SET " +
                    $"name='{model.name.Replace("'", "''")}', " +
                    $"phone='{(model.phone ?? "").Replace("'", "''")}', " +
                    $"email='{(model.email ?? "").Replace("'", "''")}', " +
                    $"address='{(model.address ?? "").Replace("'", "''")}', " +
                    $"license_no='{(model.license_number ?? "").Replace("'", "''")}', " +
                    $"vat_number='{(model.vat_number ?? "").Replace("'", "''")}', " +
                    $"receipt_footer='{(model.receipt_footer ?? "").Replace("'", "''")}', " +
                    $"currency='{(model.currency).Replace("'", "''")}' " +
                    $"WHERE id={pharmacyId}";

                dbhandler.ExecuteNonQuery(sql);

                _logger.LogInfo($"UpdatePharmacyProfile: pharmacyId={pharmacyId}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Pharmacy profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdatePharmacyProfile: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("config")]
        public IActionResult SavePharmacySetting([FromBody] Newtonsoft.Json.Linq.JObject jobject)
        {
            _logger.LogInfo("******* SAVE PHARMACY SETTING REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (jobject == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid request" });

                string key = jobject["key"]?.ToString() ?? "";
                string value = jobject["value"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(key))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Setting key is required" });

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

                _logger.LogInfo($"SavePharmacySetting: pharmacyId={pharmacyId} key={key}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Setting saved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("SavePharmacySetting: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
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
    }
}
