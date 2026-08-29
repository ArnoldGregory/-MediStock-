// ============================================================
//  MediStock.Portal — ProductsController (Riziki BFF pattern)
//  Routes:
//    GET  /Products/Index          → product register view
//    GET  /Products/Categories     → categories view
//    GET  /Products/GetProducts    → JSON proxy → api/products
//    GET  /Products/GetCategories  → JSON proxy → api/products/categories
//    GET  /Products/GetProduct?id= → JSON proxy → api/products/getproduct
//    POST /Products/AddProduct     → proxy → api/products/addproduct
//    POST /Products/UpdateProduct  → proxy → api/products/updateproduct
//    POST /Products/DeleteProduct  → proxy → api/products/deleteproduct
//    POST /Products/AddCategory    → proxy → api/products/addcategory
//    POST /Products/DeleteCategory → proxy → api/products/deletecategory
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

        // ── Data proxies ──────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _api.GetAsync<object>("api/products");
            return Json(result.IsSuccess ? result.Data : new List<object>());
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _api.GetAsync<object>("api/products/categories");
            return Json(result.IsSuccess ? result.Data : new List<object>());
        }

        [HttpGet]
        public async Task<IActionResult> GetProduct(long id)
        {
            if (id <= 0) return Json(new { success = false, message = "id required" });
            var result = await _api.GetAsync<object>("api/products/getproduct?id=" + id);
            return Json(result.IsSuccess ? result.Data : new { success = false, message = result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] AddProductRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.name))
                return Json(new { success = false, message = "Product name is required" });

            var result = await _api.PostAsync<object>("api/products/addproduct", new
            {
                name = model.name,
                sku = model.sku,
                barcode = model.barcode,
                description = model.description,
                category_id = model.category_id,
                cost_price = model.cost_price,
                selling_price = model.selling_price,
                reorder_level = model.reorder_level,
                unit_of_measure = model.unit_of_measure,
                is_controlled_drug = model.is_controlled_drug
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Product added" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to add product" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromBody] AddProductRequest model)
        {
            if (model == null || model.id <= 0 || string.IsNullOrWhiteSpace(model.name))
                return Json(new { success = false, message = "id and name are required" });

            var result = await _api.PostAsync<object>("api/products/updateproduct", new
            {
                id = model.id,
                name = model.name,
                sku = model.sku,
                barcode = model.barcode,
                description = model.description,
                category_id = model.category_id,
                cost_price = model.cost_price,
                selling_price = model.selling_price,
                reorder_level = model.reorder_level,
                unit_of_measure = model.unit_of_measure,
                is_controlled_drug = model.is_controlled_drug
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Product updated" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to update product" : result.Error });
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
            if (model == null || string.IsNullOrWhiteSpace(model.name))
                return Json(new { success = false, message = "Category name is required" });

            var result = await _api.PostAsync<object>("api/products/addcategory", new
            {
                name = model.name.Trim(),
                description = model.description
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

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddProductRequest
        {
            public long    id                 { get; set; }
            public long    category_id        { get; set; }
            public string? name               { get; set; }
            public string? sku                { get; set; }
            public string? barcode            { get; set; }
            public string? description        { get; set; }
            public decimal cost_price         { get; set; }
            public decimal selling_price      { get; set; }
            public int     reorder_level      { get; set; }
            public string? unit_of_measure    { get; set; }
            public bool    is_controlled_drug { get; set; }
        }

        public class CategoryRequest
        {
            public string? name        { get; set; }
            public string? description { get; set; }
        }
    }
}
