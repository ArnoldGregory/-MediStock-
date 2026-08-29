// ============================================================
//  MediStock.Portal — ClinicalController
//  Routes:
//    GET  /Clinical/Patients             → patients list view
//    GET  /Clinical/PatientDetail/{id}   → patient detail view
//    GET  /Clinical/Prescriptions        → prescriptions view
//    GET  /Clinical/GetPatients          → JSON patients list
//    GET  /Clinical/GetPatient?id=       → JSON single patient
//    GET  /Clinical/GetPatientAllergies?id=  → JSON allergies
//    GET  /Clinical/GetPatientConditions?id= → JSON conditions
//    GET  /Clinical/GetPrescriptions     → JSON prescriptions list
//    GET  /Clinical/GetPrescriptionItems?id= → JSON prescription items
//    POST /Clinical/AddPatient           → proxy → api/clinical/patients
//    POST /Clinical/AddPrescription      → proxy → api/clinical/prescriptions
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class ClinicalController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public ClinicalController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Patients()
        {
            await _audit.LogViewAsync("Clinical/Patients");
            return View();
        }

        public async Task<IActionResult> PatientDetail(long id)
        {
            await _audit.LogViewAsync("Clinical/PatientDetail", $"id={id}");
            ViewBag.PatientId = id;
            return View();
        }

        public async Task<IActionResult> Prescriptions()
        {
            await _audit.LogViewAsync("Clinical/Prescriptions");
            return View();
        }

        // ── Patient Data ──────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/clinical/patients?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPatient(long id)
        {
            if (id <= 0) return Json(new { success = false, message = "id required" });
            try
            {
                var result = await _api.GetAsync<object>("api/clinical/patients/" + id);
                return Json(result.IsSuccess ? result.Data : new { success = false, message = result.Error });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientAllergies(long id)
        {
            if (id <= 0) return Json(new List<object>());
            try
            {
                var result = await _api.GetAsync<object>("api/clinical/patients/" + id + "/allergies");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientConditions(long id)
        {
            if (id <= 0) return Json(new List<object>());
            try
            {
                var result = await _api.GetAsync<object>("api/clinical/patients/" + id + "/conditions");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPatient([FromBody] AddPatientRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.first_name))
                return Json(new { success = false, message = "First name is required" });

            var result = await _api.PostAsync<object>("api/clinical/patients", new
            {
                first_name     = model.first_name,
                last_name      = model.last_name,
                date_of_birth  = model.date_of_birth,
                gender         = model.gender,
                phone          = model.phone,
                email          = model.email,
                address        = model.address,
                allergies      = model.allergies,
                medical_history = model.medical_history
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Patient added", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add patient" : result.Error, data = (object?)null });
        }

        // ── Prescription Data ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPrescriptions(long? patient_id, string? status)
        {
            try
            {
                var qs = "api/clinical/prescriptions?pharmacyId=" + GetPharmacyId();
                if (patient_id.HasValue && patient_id > 0) qs += $"&patient_id={patient_id}";
                if (!string.IsNullOrWhiteSpace(status)) qs += $"&status={status}";
                var result = await _api.GetAsync<object>(qs);
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPrescriptionItems(long id)
        {
            if (id <= 0) return Json(new List<object>());
            try
            {
                var result = await _api.GetAsync<object>("api/clinical/prescriptions/" + id + "/items");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPrescription([FromBody] AddPrescriptionRequest model)
        {
            if (model == null || model.patient_id <= 0)
                return Json(new { success = false, message = "Patient is required" });

            var result = await _api.PostAsync<object>("api/clinical/prescriptions", new
            {
                patient_id        = model.patient_id,
                doctor_name       = model.doctor_name,
                hospital          = model.hospital,
                prescription_date = model.prescription_date,
                notes             = model.notes,
                items             = model.items
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Prescription added", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add prescription" : result.Error, data = (object?)null });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class AddPatientRequest
        {
            public string? first_name      { get; set; }
            public string? last_name       { get; set; }
            public string? date_of_birth   { get; set; }
            public string? gender          { get; set; }
            public string? phone           { get; set; }
            public string? email           { get; set; }
            public string? address         { get; set; }
            public string? allergies       { get; set; }
            public string? medical_history { get; set; }
        }

        public class AddPrescriptionRequestItem
        {
            public long?   product_id      { get; set; }
            public string? medication_name { get; set; }
            public string? dosage          { get; set; }
            public string? frequency       { get; set; }
            public string? duration        { get; set; }
            public int     quantity        { get; set; }
            public string? notes           { get; set; }
        }

        public class AddPrescriptionRequest
        {
            public long?                          patient_id        { get; set; }
            public string?                        doctor_name       { get; set; }
            public string?                        hospital          { get; set; }
            public string?                        prescription_date { get; set; }
            public string?                        notes             { get; set; }
            public List<AddPrescriptionRequestItem>? items          { get; set; }
        }
    }
}