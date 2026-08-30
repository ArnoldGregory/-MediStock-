using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public NotificationsController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetNotifications([FromQuery] long? pharmacyId)
        {
            iloggermanager.LogInfo("******* GET NOTIFICATIONS REQUEST **********");
            try
            {
                var (userId, callerPharmacyId, roleId) = GetCaller();
                long pid = pharmacyId ?? callerPharmacyId;
                if (pid <= 0) pid = callerPharmacyId;
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pid}, role={roleId}");
                DataTable dt = dbhandler.GetNotifications(pid);
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetNotifications: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("count")]
        public ActionResult GetCount([FromQuery] long? pharmacyId)
        {
            iloggermanager.LogInfo("******* GET NOTIFICATION COUNT REQUEST **********");
            try
            {
                var (userId, callerPharmacyId, roleId) = GetCaller();
                long pid = pharmacyId ?? callerPharmacyId;
                if (pid <= 0) pid = callerPharmacyId;
                int count = dbhandler.GetNotificationCount(pid);
                return Ok(new { success = true, message = "Success", action = "", data = new JObject { { "count", count } } });
            }
            catch (Exception ex) { iloggermanager.LogError("GetCount: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("dismiss")]
        public ActionResult Dismiss([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* DISMISS NOTIFICATION REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                if (jobject == null || !jobject.ContainsKey("id"))
                    return Bad("id is required");

                long id = jobject["id"].Value<long>();
                if (id <= 0) return Bad("Invalid id");

                bool ok = dbhandler.MarkNotificationRead(id);
                if (!ok) return Bad("Notification not found");

                return Ok(new { success = true, message = "Notification dismissed", action = "", data = new JObject() });
            }
            catch (Exception ex) { iloggermanager.LogError("Dismiss: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("markallread")]
        public ActionResult MarkAllRead([FromBody] JObject jobject)
        {
            iloggermanager.LogInfo("******* MARK ALL NOTIFICATIONS READ REQUEST **********");
            try
            {
                var (userId, callerPharmacyId, roleId) = GetCaller();
                long pharmacyId = callerPharmacyId;
                if (jobject != null && jobject.ContainsKey("pharmacy_id") && jobject["pharmacy_id"]?.Value<long?>() is long pid && pid > 0)
                    pharmacyId = pid;

                bool ok = dbhandler.MarkAllNotificationsRead(pharmacyId);
                if (!ok) return Bad("Failed to mark notifications as read");

                return Ok(new { success = true, message = "All notifications marked as read", action = "", data = new JObject() });
            }
            catch (Exception ex) { iloggermanager.LogError("MarkAllRead: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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
        private ActionResult Bad(string msg) =>
            StatusCode(StatusCodes.Status400BadRequest, new { success = false, message = msg, action = "", data = new JObject() });

        [NonAction]
        private ActionResult ServerError() =>
            StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error", action = "", data = new JObject() });
    }
}