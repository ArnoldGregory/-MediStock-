using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public CustomerController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetCustomers()
        {
            iloggermanager.LogInfo("******* GET CUSTOMERS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("customers", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetCustomers: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("wholesale")]
        public ActionResult GetWholesaleCustomers()
        {
            iloggermanager.LogInfo("******* GET WHOLESALE CUSTOMERS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("wholesale_customers", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetWholesaleCustomers: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("{id}")]
        public ActionResult GetCustomerById(Int64 id)
        {
            iloggermanager.LogInfo("******* GET CUSTOMER BY ID REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecordsById("customer", id);
                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "Customer not found", action = "", data = new JObject() });
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetCustomerById: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddCustomer([FromBody] CustomerModel model)
        {
            iloggermanager.LogInfo("******* ADD CUSTOMER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (model == null)
                    return Bad("Invalid customer data");

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddCustomer(model);
                if (ok && model.id > 0)
                {
                    iloggermanager.LogInfo($"AddCustomer: customerId={model.id}");
                    CaptureAuditTrail(userId.ToString(), "Add Customer", $"Added customer {model.id}");
                    return Ok(new { success = true, message = "Customer added successfully", action = "", data = new JObject { { "id", model.id } } });
                }
                return Bad("Failed to add customer");
            }
            catch (Exception ex) { iloggermanager.LogError("AddCustomer: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPut("{id}")]
        public ActionResult UpdateCustomer(Int64 id, [FromBody] CustomerModel model)
        {
            iloggermanager.LogInfo("******* UPDATE CUSTOMER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (model == null)
                    return Bad("Invalid customer data");

                model.id = id;
                model.pharmacy_id = pharmacyId;

                string sql = $"UPDATE customers SET first_name='{(model.first_name ?? "").Replace("'", "''")}', " +
                    $"last_name='{(model.last_name ?? "").Replace("'", "''")}', " +
                    $"phone='{(model.phone ?? "").Replace("'", "''")}', " +
                    $"email='{(model.email ?? "").Replace("'", "''")}', " +
                    $"address='{(model.address ?? "").Replace("'", "''")}', " +
                    $"customer_type='{(model.customer_type).Replace("'", "''")}', " +
                    $"credit_limit={model.credit_limit}, " +
                    $"outstanding_balance={model.outstanding_balance}, " +
                    $"payment_terms='{(model.payment_terms).Replace("'", "''")}', " +
                    $"is_active={model.is_active} " +
                    $"WHERE id={id} AND pharmacy_id={pharmacyId}";

                dbhandler.ExecuteNonQuery(sql);

                iloggermanager.LogInfo($"UpdateCustomer: customerId={id}");
                CaptureAuditTrail(userId.ToString(), "Update Customer", $"Updated customer {id}");
                return Ok(new { success = true, message = "Customer updated successfully", action = "", data = new JObject { { "id", id } } });
            }
            catch (Exception ex) { iloggermanager.LogError("UpdateCustomer: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult DeleteCustomer(Int64 id)
        {
            iloggermanager.LogInfo("******* DELETE CUSTOMER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                bool ok = dbhandler.DeleteRecord(id, userId, "customer");
                if (ok)
                {
                    iloggermanager.LogInfo($"DeleteCustomer: customerId={id}");
                    CaptureAuditTrail(userId.ToString(), "Delete Customer", $"Deleted customer {id}");
                    return Ok(new { success = true, message = "Customer deleted successfully", action = "", data = new JObject() });
                }
                return Bad("Failed to delete customer");
            }
            catch (Exception ex) { iloggermanager.LogError("DeleteCustomer: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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
