// ============================================================
//  MediStock.Portal — SuppliersController
//  Routes:
//    GET  /Suppliers/Index             → suppliers list view
//    GET  /Suppliers/PurchaseOrders    → purchase orders view
//    GET  /Suppliers/ReceiveStock      → receive stock view
//    GET  /Suppliers/GetSuppliers      → JSON suppliers list
//    POST /Suppliers/AddSupplier       → proxy → api/suppliers/addsupplier
//    POST /Suppliers/UpdateSupplier    → proxy → api/suppliers/updatesupplier
//    POST /Suppliers/DeleteSupplier    → proxy → api/suppliers/deletesupplier
//    GET  /Suppliers/GetPurchaseOrders → JSON purchase orders
//    POST /Suppliers/CreatePurchaseOrder → proxy → api/suppliers/createpo
//    POST /Suppliers/ReceivePurchaseOrder → proxy → api/suppliers/receivepo
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public SuppliersController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            await _audit.LogViewAsync("Suppliers");
            return View();
        }

        public async Task<IActionResult> PurchaseOrders()
        {
            await _audit.LogViewAsync("Suppliers/PurchaseOrders");
            return View();
        }

        public async Task<IActionResult> ReceiveStock()
        {
            await _audit.LogViewAsync("Suppliers/ReceiveStock");
            return View();
        }

        // ── Supplier Data ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/suppliers?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplier(long id)
        {
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/suppliers/getsupplier?id={id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSupplier([FromBody] AddSupplierRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/suppliers/addsupplier", new
            {
                pharmacy_id    = GetPharmacyId(),
                supplier_name  = model.supplier_name,
                contact_person = model.contact_person,
                phone          = model.phone,
                email          = model.email,
                address        = model.address,
                tax_number     = model.tax_number
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Supplier added successfully" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add supplier" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSupplier([FromBody] UpdateSupplierRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/suppliers/updatesupplier", new
            {
                id             = model.id,
                supplier_name  = model.supplier_name,
                contact_person = model.contact_person,
                phone          = model.phone,
                email          = model.email,
                address        = model.address,
                tax_number     = model.tax_number
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Supplier updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update supplier" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSupplier([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/suppliers/deletesupplier", new { id = model.id });
            return Json(result.IsSuccess
                ? new { success = true, message = "Supplier deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete supplier" : result.Error });
        }

        // ── Purchase Orders Data ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders(string? status)
        {
            try
            {
                var qs = "api/suppliers/purchaseorders?pharmacyId=" + GetPharmacyId();
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
        public async Task<IActionResult> GetPurchaseOrder(long id)
        {
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/suppliers/getpo?id={id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePoRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/suppliers/createpo", new
            {
                pharmacy_id  = GetPharmacyId(),
                supplier_id  = model.supplier_id,
                items        = model.items,
                notes        = model.notes
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Purchase order created", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to create PO" : result.Error, data = (object?)null });
        }

        [HttpPost]
        public async Task<IActionResult> ReceivePurchaseOrder([FromBody] ReceivePoRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/suppliers/receivepo", new
            {
                id    = model.id,
                items = model.items
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Stock received" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to receive stock" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddSupplierRequest
        {
            public string? supplier_name  { get; set; }
            public string? contact_person { get; set; }
            public string? phone          { get; set; }
            public string? email          { get; set; }
            public string? address        { get; set; }
            public string? tax_number     { get; set; }
        }

        public class UpdateSupplierRequest
        {
            public long    id              { get; set; }
            public string? supplier_name   { get; set; }
            public string? contact_person  { get; set; }
            public string? phone           { get; set; }
            public string? email           { get; set; }
            public string? address         { get; set; }
            public string? tax_number      { get; set; }
        }

        public class CreatePoRequest
        {
            public long?          supplier_id { get; set; }
            public List<object>?  items       { get; set; }
            public string?        notes       { get; set; }
        }

        public class ReceivePoRequest
        {
            public long          id    { get; set; }
            public List<object>? items { get; set; }
        }
    }
}
