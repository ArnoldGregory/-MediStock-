// ============================================================
//  MediStock.Portal — SuppliersController
//  Routes:
//    GET  /Suppliers/Index             → suppliers list view
//    GET  /Suppliers/PurchaseOrders    → purchase orders view
//    GET  /Suppliers/ReceiveStock      → receive stock view
//    GET  /Suppliers/GetSuppliers      → JSON suppliers list
//    POST /Suppliers/AddSupplier       → proxy → POST api/suppliers
//    POST /Suppliers/UpdateSupplier    → proxy → PUT api/suppliers/{id}
//    POST /Suppliers/DeleteSupplier    → proxy → DELETE api/suppliers/{id}
//    GET  /Suppliers/GetPurchaseOrders → JSON purchase orders
//    POST /Suppliers/CreatePurchaseOrder → proxy → POST api/suppliers/po
//    POST /Suppliers/ReceivePurchaseOrder → proxy → POST api/suppliers/po/{id}/receive
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

        public async Task<IActionResult> ReceiveStock(long? id)
        {
            ViewBag.PoId = id ?? 0;
            await _audit.LogViewAsync("Suppliers/ReceiveStock");
            return View();
        }

        public async Task<IActionResult> ImportInvoice()
        {
            await _audit.LogViewAsync("Suppliers/ImportInvoice");
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
                var result = await _api.GetAsync<object>($"api/suppliers/{id}");
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

            var result = await _api.PostAsync<object>("api/suppliers", new
            {
                pharmacy_id    = GetPharmacyId(),
                name           = model.name,
                contact_person = model.contact_person,
                phone          = model.phone,
                email          = model.email,
                address        = model.address,
                city           = model.city,
                country        = model.country,
                is_active      = model.is_active
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

            var result = await _api.PutAsync<object>($"api/suppliers/{model.id}", new
            {
                name           = model.name,
                contact_person = model.contact_person,
                phone          = model.phone,
                email          = model.email,
                address        = model.address,
                city           = model.city,
                country        = model.country,
                is_active      = model.is_active
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

            var result = await _api.DeleteAsync<object>($"api/suppliers/{model.id}");
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
                var qs = "api/suppliers/po";
                if (!string.IsNullOrWhiteSpace(status)) qs += $"?status={status}";
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
                var result = await _api.GetAsync<object>($"api/suppliers/po/{id}");
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
            if (model == null || model.supplier_id <= 0 || model.product_id <= 0)
                return Json(new { success = false, message = "Supplier and product are required" });

            var result = await _api.PostAsync<object>("api/suppliers/po", new
            {
                supplier_id   = model.supplier_id,
                product_id    = model.product_id,
                quantity      = model.quantity,
                unit_cost     = model.unit_cost,
                total_cost    = model.quantity * model.unit_cost,
                expected_date = model.expected_date,
                notes         = model.notes
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

            var result = await _api.PostAsync<object>($"api/suppliers/po/{model.id}/receive", new
            {
                quantity_received = model.quantity_received,
                notes             = model.notes
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Stock received" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to receive stock" : result.Error });
        }

        // ── Invoice Import ─────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ImportInvoiceUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "No file selected" });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var result = await _api.PostFileAsync<object>("api/suppliers/import-invoice", file.FileName, ms.ToArray());
            return Json(result.IsSuccess
                ? new { success = true, message = "Invoice parsed", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to parse invoice" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> ImportConfirm([FromBody] ImportConfirmRequest model)
        {
            if (model == null || model.supplier_id <= 0)
                return Json(new { success = false, message = "Supplier is required" });

            var lines = (model.lines ?? new List<ImportConfirmLineRequest>())
                .Where(l => !l.skip && !string.IsNullOrWhiteSpace(l.product_name) && l.quantity > 0)
                .ToList();
            if (lines.Count == 0)
                return Json(new { success = false, message = "No line items to import" });

            var result = await _api.PostAsync<object>("api/suppliers/import-confirm", new
            {
                supplier_id    = model.supplier_id,
                po_number      = model.po_number,
                markup_percent = model.markup_percent,
                lines          = lines.Select(l => new
                {
                    product_name    = l.product_name,
                    quantity        = l.quantity,
                    unit_cost       = l.unit_cost,
                    unit_sell_price = l.unit_sell_price,
                    expiry_date     = l.expiry_date,
                    skip            = l.skip
                }).ToList()
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Stock imported", data = result.Data }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to import" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddSupplierRequest
        {
            public string? name           { get; set; }
            public string? contact_person { get; set; }
            public string? phone          { get; set; }
            public string? email          { get; set; }
            public string? address        { get; set; }
            public string? city           { get; set; }
            public string? country        { get; set; }
            public bool    is_active      { get; set; } = true;
        }

        public class UpdateSupplierRequest
        {
            public long    id              { get; set; }
            public string? name            { get; set; }
            public string? contact_person  { get; set; }
            public string? phone           { get; set; }
            public string? email           { get; set; }
            public string? address         { get; set; }
            public string? city            { get; set; }
            public string? country         { get; set; }
            public bool    is_active       { get; set; } = true;
        }

        public class CreatePoRequest
        {
            public long     supplier_id     { get; set; }
            public long     product_id      { get; set; }
            public int      quantity        { get; set; }
            public decimal  unit_cost       { get; set; }
            public string?  expected_date   { get; set; }
            public string?  notes           { get; set; }
        }

        public class ReceivePoRequest
        {
            public long    id                 { get; set; }
            public int     quantity_received  { get; set; }
            public string? notes              { get; set; }
        }

        public class ImportConfirmRequest
        {
            public long     supplier_id      { get; set; }
            public string?  po_number        { get; set; }
            public decimal  markup_percent   { get; set; } = 25m;
            public List<ImportConfirmLineRequest>? lines { get; set; }
        }

        public class ImportConfirmLineRequest
        {
            public string   product_name     { get; set; } = "";
            public int      quantity         { get; set; }
            public decimal  unit_cost        { get; set; }
            public decimal? unit_sell_price  { get; set; }
            public string?  expiry_date      { get; set; }
            public bool     skip             { get; set; }
        }
    }
}
