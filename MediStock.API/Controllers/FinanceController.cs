using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/finance")]
    public class FinanceController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public FinanceController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet("expenses")]
        public ActionResult GetExpenses()
        {
            iloggermanager.LogInfo("******* GET EXPENSES REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("expenses", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetExpenses: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("expenses/{id}")]
        public ActionResult GetExpenseById(Int64 id)
        {
            iloggermanager.LogInfo("******* GET EXPENSE BY ID REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecordsById("expense", id);
                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "Expense not found", action = "", data = new JObject() });
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetExpenseById: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("expenses")]
        public ActionResult AddExpense([FromBody] ExpenseModel model)
        {
            iloggermanager.LogInfo("******* ADD EXPENSE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null || string.IsNullOrEmpty(model.description)) return Bad("Description is required");

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddExpense(model);
                if (ok && model.id > 0)
                {
                    iloggermanager.LogInfo($"AddExpense: expenseId={model.id}");
                    CaptureAuditTrail(userId.ToString(), "Add Expense", $"Added expense: {model.description}");
                    return Ok(new { success = true, message = "Expense added successfully", action = "", data = new JObject { { "id", model.id } } });
                }
                return Bad("Failed to add expense");
            }
            catch (Exception ex) { iloggermanager.LogError("AddExpense: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpDelete("expenses/{id}")]
        public ActionResult DeleteExpense(Int64 id)
        {
            iloggermanager.LogInfo("******* DELETE EXPENSE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                bool ok = dbhandler.DeleteRecord(id, userId, "expenses");
                if (ok)
                {
                    iloggermanager.LogInfo($"DeleteExpense: expenseId={id}");
                    CaptureAuditTrail(userId.ToString(), "Delete Expense", $"Deleted expense {id}");
                    return Ok(new { success = true, message = "Expense deleted successfully", action = "", data = new JObject { { "id", id } } });
                }
                return Bad("Failed to delete expense");
            }
            catch (Exception ex) { iloggermanager.LogError("DeleteExpense: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("categories")]
        public ActionResult GetExpenseCategories()
        {
            iloggermanager.LogInfo("******* GET EXPENSE CATEGORIES REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("expense_categories", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetExpenseCategories: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("categories")]
        public ActionResult AddExpenseCategory([FromBody] ExpenseCategoryModel model)
        {
            iloggermanager.LogInfo("******* ADD EXPENSE CATEGORY REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (model == null || string.IsNullOrEmpty(model.name)) return Bad("Category name is required");

                model.pharmacy_id = pharmacyId;

                string sql = $"INSERT INTO expense_categories (pharmacy_id, name, is_active) VALUES ({pharmacyId}, '{model.name.Replace("'", "''")}', 1)";
                dbhandler.ExecuteNonQuery(sql);

                DataTable dt = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dt.Rows.Count > 0 ? Convert.ToInt64(dt.Rows[0]["id"]) : 0;

                iloggermanager.LogInfo($"AddExpenseCategory: categoryId={id}");
                CaptureAuditTrail(userId.ToString(), "Add Expense Category", $"Added expense category: {model.name}");
                return Ok(new { success = true, message = "Expense category added", action = "", data = new JObject { { "id", id } } });
            }
            catch (Exception ex) { iloggermanager.LogError("AddExpenseCategory: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
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
                session_id = "TODO"
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}