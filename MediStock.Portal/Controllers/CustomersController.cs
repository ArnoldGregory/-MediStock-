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
                var qs = "api/customers";
                if (type == "Wholesale") qs = "api/customers/wholesale";
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
                var result = await _api.GetAsync<object>($"api/customers/{id}");
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
                var result = await _api.GetAsync<object>("api/sales");
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

            var (first, last) = SplitName(model.customer_name);

            var result = await _api.PostAsync<object>("api/customers", new
            {
                pharmacy_id        = GetPharmacyId(),
                customer_type      = model.customer_type,
                first_name         = first,
                last_name          = last,
                phone              = model.phone,
                email              = model.email,
                address            = model.address,
                credit_limit       = model.credit_limit,
                payment_terms      = model.payment_terms,
                is_active          = true
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

            var (first, last) = SplitName(model.customer_name);

            var result = await _api.PutAsync<object>($"api/customers/{model.id}", new
            {
                customer_type      = model.customer_type,
                first_name         = first,
                last_name          = last,
                phone              = model.phone,
                email              = model.email,
                address            = model.address,
                credit_limit       = model.credit_limit,
                outstanding_balance= model.outstanding_balance,
                payment_terms      = model.payment_terms,
                is_active          = model.is_active
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

            var result = await _api.DeleteAsync<object>($"api/customers/{model.id}");
            return Json(result.IsSuccess
                ? new { success = true, message = "Customer deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete customer" : result.Error });
        }

        private static (string first, string last) SplitName(string? full)
        {
            if (string.IsNullOrWhiteSpace(full)) return ("", "");
            var parts = full.Trim().Split(' ', 2);
            return (parts[0], parts.Length > 1 ? parts[1] : "");
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddCustomerRequest
        {
            public string? customer_name    { get; set; }
            public string? customer_type    { get; set; }
            public string? phone            { get; set; }
            public string? email            { get; set; }
            public string? address          { get; set; }
            public decimal credit_limit     { get; set; }
            public string? payment_terms    { get; set; }
        }

        public class UpdateCustomerRequest
        {
            public long    id                   { get; set; }
            public string? customer_name        { get; set; }
            public string? customer_type        { get; set; }
            public string? phone                { get; set; }
            public string? email                { get; set; }
            public string? address              { get; set; }
            public decimal credit_limit         { get; set; }
            public decimal outstanding_balance  { get; set; }
            public string? payment_terms        { get; set; }
            public bool    is_active            { get; set; } = true;
        }
    }
}
