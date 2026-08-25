// ============================================================
//  MediStock.Portal — ClinicalController
//  Routes:
//    GET  /Clinical/Patients          → patients list view
//    GET  /Clinical/PatientDetail/{id} → patient detail view
//    GET  /Clinical/Prescriptions     → prescriptions view
//    GET  /Clinical/GetPatients       → JSON patients list
//    GET  /Clinical/GetPatient?id=    → JSON single patient
//    POST /Clinical/AddPatient        → proxy → api/clinical/addpatient
//    POST /Clinical/UpdatePatient     → proxy → api/clinical/updatepatient
//    GET  /Clinical/GetPrescriptions  → JSON prescriptions list
//    POST /Clinical/AddPrescription   → proxy → api/clinical/addprescription
//    GET  /Clinical/GetPatientHistory?id= → JSON patient history
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
        public async Task<IActionResult> GetPatients(string? search)
        {
            try
            {
                var qs = "api/clinical/patients?pharmacyId=" + GetPharmacyId();
                if (!string.IsNullOrWhiteSpace(search)) qs += $"&search={search}";
                var result = await _api.GetAsync<object>(qs);
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
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/clinical/getpatient?id={id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientHistory(long id)
        {
            if (id <= 0) return Json(new List<object>());
            try
            {
                var result = await _api.GetAsync<object>($"api/clinical/patienthistory?id={id}");
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
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/clinical/addpatient", new
            {
                pharmacy_id  = GetPharmacyId(),
                first_name   = model.first_name,
                last_name    = model.last_name,
                date_of_birth = model.date_of_birth,
                gender       = model.gender,
                phone        = model.phone,
                email        = model.email,
                id_number    = model.id_number,
                allergies    = model.allergies,
                medical_notes = model.medical_notes
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Patient added", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add patient" : result.Error, data = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePatient([FromBody] UpdatePatientRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/clinical/updatepatient", new
            {
                id           = model.id,
                first_name   = model.first_name,
                last_name    = model.last_name,
                date_of_birth = model.date_of_birth,
                gender       = model.gender,
                phone        = model.phone,
                email        = model.email,
                id_number    = model.id_number,
                allergies    = model.allergies,
                medical_notes = model.medical_notes
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Patient updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update patient" : result.Error });
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

        [HttpPost]
        public async Task<IActionResult> AddPrescription([FromBody] AddPrescriptionRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/clinical/addprescription", new
            {
                pharmacy_id   = GetPharmacyId(),
                patient_id    = model.patient_id,
                doctor_name   = model.doctor_name,
                diagnosis     = model.diagnosis,
                items         = model.items,
                notes         = model.notes
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Prescription added", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add prescription" : result.Error, data = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> DispensePrescription([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/clinical/dispenseprescription", new { id = model.id });
            return Json(result.IsSuccess
                ? new { success = true, message = "Prescription dispensed" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to dispense" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddPatientRequest
        {
            public string? first_name    { get; set; }
            public string? last_name     { get; set; }
            public string? date_of_birth { get; set; }
            public string? gender        { get; set; }
            public string? phone         { get; set; }
            public string? email         { get; set; }
            public string? id_number     { get; set; }
            public string? allergies     { get; set; }
            public string? medical_notes { get; set; }
        }

        public class UpdatePatientRequest
        {
            public long    id             { get; set; }
            public string? first_name     { get; set; }
            public string? last_name      { get; set; }
            public string? date_of_birth  { get; set; }
            public string? gender         { get; set; }
            public string? phone          { get; set; }
            public string? email          { get; set; }
            public string? id_number      { get; set; }
            public string? allergies      { get; set; }
            public string? medical_notes  { get; set; }
        }

        public class AddPrescriptionRequest
        {
            public long?          patient_id  { get; set; }
            public string?        doctor_name { get; set; }
            public string?        diagnosis   { get; set; }
            public List<object>?  items       { get; set; }
            public string?        notes       { get; set; }
        }
    }
}
