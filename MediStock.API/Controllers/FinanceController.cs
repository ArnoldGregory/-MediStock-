using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/finance")]
    public class FinanceController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public FinanceController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("expenses")]
        public IActionResult GetExpenses()
        {
            _logger.LogInfo("******* GET EXPENSES REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("expenses", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetExpenses: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("expenses/{id}")]
        public IActionResult GetExpenseById(Int64 id)
        {
            _logger.LogInfo("******* GET EXPENSE BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("expense", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Expense not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetExpenseById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("expenses")]
        public IActionResult AddExpense([FromBody] ExpenseModel model)
        {
            _logger.LogInfo("******* ADD EXPENSE REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || string.IsNullOrEmpty(model.description))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Description is required" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddExpense(model);
                if (ok && model.id > 0)
                {
                    _logger.LogInfo($"AddExpense: expenseId={model.id}");
                    CaptureAuditTrail(GetCallerEmail(), "Add Expense", $"Added expense: {model.description}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Expense added successfully",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to add expense" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddExpense: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpDelete("expenses/{id}")]
        public IActionResult DeleteExpense(Int64 id)
        {
            _logger.LogInfo("******* DELETE EXPENSE REQUEST **********");
            try
            {
                var userId = GetCallerUserId();
                bool ok = dbhandler.DeleteRecord(id, userId, "expenses");
                if (ok)
                {
                    _logger.LogInfo($"DeleteExpense: expenseId={id}");
                    CaptureAuditTrail(GetCallerEmail(), "Delete Expense", $"Deleted expense {id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Expense deleted successfully"
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to delete expense" });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteExpense: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("categories")]
        public IActionResult GetExpenseCategories()
        {
            _logger.LogInfo("******* GET EXPENSE CATEGORIES REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("expense_categories", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetExpenseCategories: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("categories")]
        public IActionResult AddExpenseCategory([FromBody] ExpenseCategoryModel model)
        {
            _logger.LogInfo("******* ADD EXPENSE CATEGORY REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();

                if (model == null || string.IsNullOrEmpty(model.name))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Category name is required" });

                model.pharmacy_id = pharmacyId;

                string sql = $"INSERT INTO expense_categories (pharmacy_id, name, is_active) VALUES ({pharmacyId}, '{model.name.Replace("'", "''")}', 1)";
                dbhandler.ExecuteNonQuery(sql);

                DataTable dt = dbhandler.GetAdhocData("SELECT LAST_INSERT_ID() AS id");
                Int64 id = dt.Rows.Count > 0 ? Convert.ToInt64(dt.Rows[0]["id"]) : 0;

                _logger.LogInfo($"AddExpenseCategory: categoryId={id}");
                CaptureAuditTrail(GetCallerEmail(), "Add Expense Category", $"Added expense category: {model.name}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Expense category added",
                    data = new { id = id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddExpenseCategory: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

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
    }
}
