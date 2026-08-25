// ============================================================
//  MediStock.Portal — CustomersController
//  Routes:
//    GET  /Customers/Retail       → retail customers view
//    GET  /Customers/Wholesale    → wholesale customers view
//    GET  /Customers/GetCustomers → JSON customers list
//    GET  /Customers/GetCustomer?id= → JSON single customer
//    POST /Customers/AddCustomer  → proxy → api/customers/addcustomer
//    POST /Customers/UpdateCustomer → proxy → api/customers/updatecustomer
//    POST /Customers/DeleteCustomer → proxy → api/customers/deletecustomer
//    GET  /Customers/GetCustomerSales?id= → JSON customer purchase history
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public CustomersController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Retail()
        {
            await _audit.LogViewAsync("Customers/Retail");
            return View();
        }

        public async Task<IActionResult> Wholesale()
        {
            await _audit.LogViewAsync("Customers/Wholesale");
            return View();
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetCustomers(string? type)
        {
            try
            {
                var qs = "api/customers?pharmacyId=" + GetPharmacyId();
                if (!string.IsNullOrWhiteSpace(type)) qs += $"&type={type}";
                var result = await _api.GetAsync<object>(qs);
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomer(long id)
        {
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/customers/getcustomer?id={id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerSales(long customer_id)
        {
            if (customer_id <= 0) return Json(new List<object>());
            try
            {
                var result = await _api.GetAsync<object>($"api/customers/customersales?customer_id={customer_id}");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] AddCustomerRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/customers/addcustomer", new
            {
                pharmacy_id   = GetPharmacyId(),
                customer_name = model.customer_name,
                phone         = model.phone,
                email         = model.email,
                customer_type = model.customer_type,
                id_number     = model.id_number,
                address       = model.address
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Customer added successfully" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add customer" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/customers/updatecustomer", new
            {
                id            = model.id,
                customer_name = model.customer_name,
                phone         = model.phone,
                email         = model.email,
                customer_type = model.customer_type,
                id_number     = model.id_number,
                address       = model.address
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Customer updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update customer" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/customers/deletecustomer", new { id = model.id });
            return Json(result.IsSuccess
                ? new { success = true, message = "Customer deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete customer" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddCustomerRequest
        {
            public string? customer_name { get; set; }
            public string? phone         { get; set; }
            public string? email         { get; set; }
            public string? customer_type { get; set; }
            public string? id_number     { get; set; }
            public string? address       { get; set; }
        }

        public class UpdateCustomerRequest
        {
            public long    id             { get; set; }
            public string? customer_name  { get; set; }
            public string? phone          { get; set; }
            public string? email          { get; set; }
            public string? customer_type  { get; set; }
            public string? id_number      { get; set; }
            public string? address        { get; set; }
        }
    }
}
