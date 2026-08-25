// ============================================================
//  MediStock.Portal — ProductsController
//  Routes:
//    GET  /Products/Index          → products list view
//    GET  /Products/Categories     → categories view
//    GET  /Products/GetProducts    → JSON proxy → api/products
//    GET  /Products/GetCategories  → JSON proxy → api/products/categories
//    GET  /Products/GetProduct?id= → JSON proxy → api/products/getproduct
//    POST /Products/AddProduct     → proxy → api/products/addproduct
//    POST /Products/UpdateProduct  → proxy → api/products/updateproduct
//    POST /Products/DeleteProduct  → proxy → api/products/deleteproduct
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public ProductsController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            await _audit.LogViewAsync("Products");
            return View();
        }

        public async Task<IActionResult> Categories()
        {
            await _audit.LogViewAsync("Products/Categories");
            return View();
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/products?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProduct(long id)
        {
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/products/getproduct?id={id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/products/categories?pharmacyId=" + GetPharmacyId());
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] AddProductRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/products/addproduct", new
            {
                pharmacy_id     = GetPharmacyId(),
                product_name    = model.product_name,
                generic_name    = model.generic_name,
                category_id     = model.category_id,
                unit_price      = model.unit_price,
                cost_price      = model.cost_price,
                quantity        = model.quantity,
                reorder_level   = model.reorder_level,
                expiry_date     = model.expiry_date,
                batch_number    = model.batch_number,
                manufacturer    = model.manufacturer,
                supplier_id     = model.supplier_id
            });

            if (result.IsSuccess)
                return Json(new { success = true, message = "Product added successfully" });

            return Json(new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add product" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/products/updateproduct", new
            {
                id              = model.id,
                product_name    = model.product_name,
                generic_name    = model.generic_name,
                category_id     = model.category_id,
                unit_price      = model.unit_price,
                cost_price      = model.cost_price,
                reorder_level   = model.reorder_level,
                manufacturer    = model.manufacturer
            });

            if (result.IsSuccess)
                return Json(new { success = true, message = "Product updated successfully" });

            return Json(new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update product" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/products/deleteproduct", new { id = model.id });
            return Json(result.IsSuccess
                ? new { success = true, message = "Product deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete product" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.category_name))
                return Json(new { success = false, message = "category_name is required" });

            var result = await _api.PostAsync<object>("api/products/addcategory", new
            {
                category_name = model.category_name.Trim(),
                description   = model.description
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Category added" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add category" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.PostAsync<object>("api/products/deletecategory", new { id = model.id });
            return Json(result.IsSuccess
                ? new { success = true, message = "Category deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete category" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddProductRequest
        {
            public string? product_name { get; set; }
            public string? generic_name { get; set; }
            public long    category_id  { get; set; }
            public decimal unit_price   { get; set; }
            public decimal cost_price   { get; set; }
            public int     quantity     { get; set; }
            public int     reorder_level { get; set; }
            public string? expiry_date  { get; set; }
            public string? batch_number { get; set; }
            public string? manufacturer { get; set; }
            public long    supplier_id  { get; set; }
        }

        public class UpdateProductRequest
        {
            public long    id              { get; set; }
            public string? product_name    { get; set; }
            public string? generic_name    { get; set; }
            public long    category_id     { get; set; }
            public decimal unit_price      { get; set; }
            public decimal cost_price      { get; set; }
            public int     reorder_level   { get; set; }
            public string? manufacturer    { get; set; }
        }

        public class CategoryRequest
        {
            public string? category_name { get; set; }
            public string? description   { get; set; }
        }
    }
}
