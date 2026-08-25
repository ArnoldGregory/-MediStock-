using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/clinical")]
    public class ClinicalController : ControllerBase
    {
        private readonly DBHandler dbhandler;
        private readonly IConfiguration _config;
        private readonly ILoggerManager _logger;

        public ClinicalController(IConfiguration config, ILoggerManager logger)
        {
            dbhandler = new DBHandler(config.GetConnectionString("DefaultConnection")!);
            _config = config;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("patients")]
        public IActionResult GetPatients()
        {
            _logger.LogInfo("******* GET PATIENTS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("patients", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPatients: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("patients/{id}")]
        public IActionResult GetPatientById(Int64 id)
        {
            _logger.LogInfo("******* GET PATIENT BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("patient", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Patient not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPatientById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("patients")]
        public IActionResult AddPatient([FromBody] PatientModel model)
        {
            _logger.LogInfo("******* ADD PATIENT REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || string.IsNullOrEmpty(model.first_name))
                    return BadRequest(new ApiResponse<object> { success = false, message = "First name is required" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddPatient(model);
                if (ok && model.id > 0)
                {
                    _logger.LogInfo($"AddPatient: patientId={model.id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Patient added successfully",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to add patient" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddPatient: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("patients/{id}/allergies")]
        public IActionResult GetPatientAllergies(Int64 id)
        {
            _logger.LogInfo("******* GET PATIENT ALLERGIES REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("patient_allergies", id.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPatientAllergies: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("patients/{id}/conditions")]
        public IActionResult GetPatientConditions(Int64 id)
        {
            _logger.LogInfo("******* GET PATIENT CONDITIONS REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("patient_conditions", id.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPatientConditions: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("prescriptions")]
        public IActionResult GetPrescriptions()
        {
            _logger.LogInfo("******* GET PRESCRIPTIONS REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                DataTable dt = dbhandler.GetRecords("prescriptions", pharmacyId.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPrescriptions: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("prescriptions/{id}")]
        public IActionResult GetPrescriptionById(Int64 id)
        {
            _logger.LogInfo("******* GET PRESCRIPTION BY ID REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecordsById("prescription", id);
                if (dt.Rows.Count == 0)
                    return NotFound(new ApiResponse<object> { success = false, message = "Prescription not found" });
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPrescriptionById: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("prescriptions/{id}/items")]
        public IActionResult GetPrescriptionItems(Int64 id)
        {
            _logger.LogInfo("******* GET PRESCRIPTION ITEMS REQUEST **********");
            try
            {
                DataTable dt = dbhandler.GetRecords("prescription_items", id.ToString());
                _logger.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new ApiResponse<DataTable> { success = true, data = dt });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPrescriptionItems: " + ex.Message + " - " + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("prescriptions")]
        public IActionResult AddPrescription([FromBody] PrescriptionModel model)
        {
            _logger.LogInfo("******* ADD PRESCRIPTION REQUEST **********");
            try
            {
                var pharmacyId = GetCallerPharmacyId();
                var userId = GetCallerUserId();

                if (model == null || model.patient_id <= 0)
                    return BadRequest(new ApiResponse<object> { success = false, message = "Patient ID is required" });

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddPrescription(model);
                if (ok && model.id > 0)
                {
                    if (model.items != null && model.items.Count > 0)
                    {
                        foreach (var item in model.items)
                        {
                            string sql = $"INSERT INTO prescription_items (prescription_id, product_id, medication_name, dosage, frequency, duration, quantity, notes) " +
                                $"VALUES ({model.id}, {(item.product_id > 0 ? item.product_id.ToString() : "NULL")}, " +
                                $"'{item.medication_name.Replace("'", "''")}', " +
                                $"'{(item.dosage ?? "").Replace("'", "''")}', " +
                                $"'{(item.frequency ?? "").Replace("'", "''")}', " +
                                $"'{(item.duration ?? "").Replace("'", "''")}', " +
                                $"{item.quantity}, " +
                                $"'{(item.notes ?? "").Replace("'", "''")}'";
                            dbhandler.ExecuteNonQuery(sql);
                        }
                    }

                    _logger.LogInfo($"AddPrescription: prescriptionId={model.id}");
                    return Ok(new ApiResponse<object>
                    {
                        success = true,
                        message = "Prescription added successfully",
                        data = new { id = model.id }
                    });
                }
                return BadRequest(new ApiResponse<object> { success = false, message = "Failed to add prescription" });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddPrescription: " + ex.Message + " - " + ex.StackTrace);
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
