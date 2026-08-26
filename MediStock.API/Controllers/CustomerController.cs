using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public CustomerController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetCustomers()
        {
            _logger.LogInfo("******* GET CUSTOMERS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("customers", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCustomers: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("wholesale")]
        public IActionResult GetWholesaleCustomers()
        {
            _logger.LogInfo("******* GET WHOLESALE CUSTOMERS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("wholesale_customers", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetWholesaleCustomers: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetCustomerById(Int64 id)
        {
            _logger.LogInfo("******* GET CUSTOMER BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("customer", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Customer not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCustomerById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddCustomer([FromBody] CustomerModel model)
        {
            _logger.LogInfo("******* ADD CUSTOMER REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid customer data" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddCustomer(model);
                if (ok && model.id > 0)
                {
                    _logger.LogInfo($"AddCustomer: customerId={model.id}");
                    CaptureAuditTrail(GetCallerEmail(), "Add Customer", $"Added customer {model.id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Customer added successfully",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to add customer" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddCustomer: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(Int64 id, [FromBody] CustomerModel model)
        {
            _logger.LogInfo("******* UPDATE CUSTOMER REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();

                if (model == null)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Invalid customer data" });

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

                _logger.LogInfo($"UpdateCustomer: customerId={id}");
                CaptureAuditTrail(GetCallerEmail(), "Update Customer", $"Updated customer {id}");
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    message = "Customer updated successfully",
                    data = new { id = id }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateCustomer: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(Int64 id)
        {
            _logger.LogInfo("******* DELETE CUSTOMER REQUEST **********");
            try
            {
                var userId = GetCallerUserId();
                bool ok = dbhandler.DeleteRecord(id, userId, "customers");
                if (ok)
                {
                    _logger.LogInfo($"DeleteCustomer: customerId={id}");
                    CaptureAuditTrail(GetCallerEmail(), "Delete Customer", $"Deleted customer {id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Customer deleted successfully"
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to delete customer" });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteCustomer: " + ex.Message + " - " + ex.StackTrace);
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
