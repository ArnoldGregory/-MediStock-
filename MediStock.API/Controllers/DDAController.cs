using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/dda")]
    public class DDAController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public DDAController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetDDARegister()
        {
            _logger.LogInfo("******* GET DDA REGISTER REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("dda_register", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetDDARegister: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetDDAEntryById(Int64 id)
        {
            _logger.LogInfo("******* GET DDA ENTRY BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("dda_entry", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "DDA entry not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetDDAEntryById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddDDAEntry([FromBody] DDAModel model)
        {
            _logger.LogInfo("******* ADD DDA ENTRY REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || model.product_id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Product ID is required" });

                if (string.IsNullOrEmpty(model.entry_type))
                    return BadRequest(new ApiResponse<object> { success = false, message = "Entry type is required" });

                if (model.quantity <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Quantity must be greater than zero" });

                model.pharmacy_id = pharmacyId;
                model.recorded_by = userId;

                bool ok = dbhandler.AddDDAEntry(model);
                if (ok && model.id > 0)
                {
                    _logger.LogInfo($"AddDDAEntry: ddaId={model.id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "DDA entry recorded",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to add DDA entry" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddDDAEntry: " + ex.Message + " - " + ex.StackTrace);
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
