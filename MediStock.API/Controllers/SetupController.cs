using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/setup")]
    public class SetupController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public SetupController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet("checklist")]
        public ActionResult GetChecklist()
        {
            iloggermanager.LogInfo("******* GET SETUP CHECKLIST REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (pharmacyId <= 0) return Bad("pharmacy_id required");

                var checks = new List<SetupCheckItem>
                {
                    Check("pharmacy_profile", "Pharmacy profile", Scalar(pharmacyId, "SELECT COUNT(*) FROM pharmacies WHERE id = {p} AND is_deleted = 0") > 0,
                        "Fill in your pharmacy name, phone and licence details under Settings → Pharmacy.",
                        "start"),
                    Check("suppliers", "Add at least one supplier", Scalar(pharmacyId, "SELECT COUNT(*) FROM suppliers WHERE pharmacy_id = {p} AND is_deleted = 0") > 0,
                        "You need suppliers to raise purchase orders and import invoices.",
                        "suppliers"),
                    Check("products", "Add products to your catalogue", Scalar(pharmacyId, "SELECT COUNT(*) FROM products WHERE pharmacy_id = {p} AND is_deleted = 0") > 0,
                        "Add your stock or import a supplier invoice to create products automatically.",
                        "products"),
                    Check("categories", "Categorise your products", Scalar(pharmacyId, "SELECT COUNT(*) FROM products WHERE pharmacy_id = {p} AND category_id IS NULL AND is_deleted = 0") == 0,
                        $"{Scalar(pharmacyId, "SELECT COUNT(*) FROM products WHERE pharmacy_id = {p} AND category_id IS NULL AND is_deleted = 0")} product(s) have no category — this makes reports harder to read.",
                        "categories"),
                    Check("pricing", "Ensure products have a buying & selling price", Scalar(pharmacyId, "SELECT COUNT(*) FROM products WHERE pharmacy_id = {p} AND (cost_price <= 0 OR selling_price <= 0) AND is_deleted = 0") == 0,
                        $"{Scalar(pharmacyId, "SELECT COUNT(*) FROM products WHERE pharmacy_id = {p} AND (cost_price <= 0 OR selling_price <= 0) AND is_deleted = 0")} product(s) are missing a cost or selling price.",
                        "products"),
                    Check("batches", "Your stock is batch-tracked", Scalar(pharmacyId, "SELECT COUNT(DISTINCT p.id) FROM products p WHERE p.pharmacy_id = {p} AND p.is_deleted = 0 AND NOT EXISTS (SELECT 1 FROM product_batches b WHERE b.product_id = p.id AND b.is_deleted = 0)") == 0,
                        "Stock without a batch can't be tracked for expiry — add batches under Inventory → Batches.",
                        "batches"),
                    Check("no_expired", "No expired batches on the shelf", Scalar(pharmacyId, "SELECT COUNT(*) FROM product_batches WHERE pharmacy_id = {p} AND status = 'Active' AND is_deleted = 0 AND expiry_date < CURDATE()") == 0,
                        $"{Scalar(pharmacyId, "SELECT COUNT(*) FROM product_batches WHERE pharmacy_id = {p} AND status = 'Active' AND is_deleted = 0 AND expiry_date < CURDATE()")} active batch(es) have expired — review under Inventory → Batches.",
                        "batches"),
                    Check("expiring", "No batch expiring within 90 days", Scalar(pharmacyId, "SELECT COUNT(*) FROM product_batches WHERE pharmacy_id = {p} AND status = 'Active' AND is_deleted = 0 AND expiry_date BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 90 DAY)") == 0,
                        $"{Scalar(pharmacyId, "SELECT COUNT(*) FROM product_batches WHERE pharmacy_id = {p} AND status = 'Active' AND is_deleted = 0 AND expiry_date BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 90 DAY)")} batch(es) expire within 90 days — plan your sales or returns.",
                        "batches"),
                    Check("license", "Pharmacy licence not expiring soon", Scalar(pharmacyId, "SELECT COUNT(*) FROM pharmacies WHERE id = {p} AND license_expiry IS NOT NULL AND license_expiry < DATE_ADD(CURDATE(), INTERVAL 90 DAY)") == 0,
                        "Your pharmacy licence expires soon — renew under Settings → Pharmacy.",
                        "pharmacy")
                };

                int done = checks.Count(c => c.ok);
                iloggermanager.LogInfo($"GetChecklist: pharmacyId={pharmacyId} done={done}/{checks.Count}");
                return Ok(new
                {
                    success = true,
                    message = "Success",
                    action = "",
                    data = new
                    {
                        checked_at = DateTime.UtcNow,
                        total = checks.Count,
                        done,
                        pending = checks.Count - done,
                        ready = done == checks.Count,
                        checks
                    }
                });
            }
            catch (Exception ex) { iloggermanager.LogError("GetChecklist: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [NonAction]
        private SetupCheckItem Check(string key, string label, bool ok, string hint, string jumpTo)
            => new() { key = key, label = label, ok = ok, hint = hint, jump_to = jumpTo };

        public class SetupCheckItem
        {
            public string key { get; set; } = "";
            public string label { get; set; } = "";
            public bool ok { get; set; }
            public string hint { get; set; } = "";
            public string jump_to { get; set; } = "";
        }

        [NonAction]
        private long Scalar(Int64 pharmacyId, string sql)
        {
            string q = sql.Replace("{p}", pharmacyId.ToString());
            DataTable dt = dbhandler.GetAdhocData(q);
            return dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value ? Convert.ToInt64(dt.Rows[0][0]) : 0;
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
                session_id = HttpContext.TraceIdentifier
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}