using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public ProductController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        public class AddProductRequest
        {
            public long category_id { get; set; }
            public string name { get; set; } = "";
            public string? sku { get; set; }
            public string? barcode { get; set; }
            public string? description { get; set; }
            public decimal cost_price { get; set; }
            public decimal selling_price { get; set; }
            public int reorder_level { get; set; }
            public string? unit_of_measure { get; set; }
            public bool is_controlled_drug { get; set; }
        }
        public class UpdateProductRequest
        {
            public long id { get; set; }
            public long category_id { get; set; }
            public string name { get; set; } = "";
            public string? sku { get; set; }
            public string? barcode { get; set; }
            public string? description { get; set; }
            public decimal cost_price { get; set; }
            public decimal selling_price { get; set; }
            public int reorder_level { get; set; }
            public string? unit_of_measure { get; set; }
            public bool is_controlled_drug { get; set; }
        }
        public class DeleteRequest { public long id { get; set; } }
        public class AddCategoryRequest { public string name { get; set; } = ""; public string? description { get; set; } }
        public class DeleteCategoryRequest { public long id { get; set; } }

        [Authorize]
        [HttpGet]
        public ActionResult GetProducts()
        {
            iloggermanager.LogInfo("******* GET PRODUCTS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("products", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetProducts: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("getproduct")]
        public ActionResult GetProduct(int id)
        {
            iloggermanager.LogInfo("******* GET PRODUCT REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: id={id}, user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (id <= 0) return Bad("id is required");
                DataTable dt = dbhandler.GetRecordsById("product", id);
                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "Product not found", action = "", data = new JObject() });
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetProduct: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("addproduct")]
        public ActionResult AddProduct([FromBody] AddProductRequest req)
        {
            iloggermanager.LogInfo("******* ADD PRODUCT REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (req == null || string.IsNullOrEmpty(req.name)) return Bad("name is required");

                ProductModel model = new()
                {
                    pharmacy_id = pharmacyId,
                    category_id = req.category_id,
                    name = req.name,
                    sku = req.sku,
                    barcode = req.barcode,
                    description = req.description,
                    cost_price = req.cost_price,
                    selling_price = req.selling_price,
                    reorder_level = req.reorder_level,
                    unit_of_measure = req.unit_of_measure,
                    is_controlled_drug = req.is_controlled_drug,
                    created_by = userId
                };

                bool ok = dbhandler.AddProduct(model);
                if (ok && model.id > 0)
                {
                    iloggermanager.LogInfo($"AddProduct: productId={model.id}");
                    CaptureAuditTrail(userId.ToString(), "Add Product", $"Added product: {model.name}");
                    return Ok(new { success = true, message = "Product added successfully", action = "", data = new JObject { { "id", model.id } } });
                }
                return Bad("Failed to add product");
            }
            catch (Exception ex) { iloggermanager.LogError("AddProduct: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("updateproduct")]
        public ActionResult UpdateProduct([FromBody] UpdateProductRequest req)
        {
            iloggermanager.LogInfo("******* UPDATE PRODUCT REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: id={req?.id}, user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (req == null || req.id <= 0) return Bad("id is required");
                if (string.IsNullOrEmpty(req.name)) return Bad("name is required");

                ProductModel model = new()
                {
                    id = req.id,
                    pharmacy_id = pharmacyId,
                    category_id = req.category_id,
                    name = req.name,
                    sku = req.sku,
                    barcode = req.barcode,
                    description = req.description,
                    cost_price = req.cost_price,
                    selling_price = req.selling_price,
                    reorder_level = req.reorder_level,
                    unit_of_measure = req.unit_of_measure,
                    is_controlled_drug = req.is_controlled_drug
                };

                bool ok = dbhandler.UpdateProduct(model);
                if (ok)
                {
                    iloggermanager.LogInfo($"UpdateProduct: productId={req.id}");
                    CaptureAuditTrail(userId.ToString(), "Update Product", $"Updated product {req.id}");
                    return Ok(new { success = true, message = "Product updated successfully", action = "", data = new JObject { { "id", req.id } } });
                }
                return Bad("Failed to update product");
            }
            catch (Exception ex) { iloggermanager.LogError("UpdateProduct: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("deleteproduct")]
        public ActionResult DeleteProduct([FromBody] DeleteRequest req)
        {
            iloggermanager.LogInfo("******* DELETE PRODUCT REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: id={req?.id}, user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (req == null || req.id <= 0) return Bad("id is required");
                bool ok = dbhandler.DeleteRecord(req.id, userId, "product");
                if (ok)
                {
                    iloggermanager.LogInfo($"DeleteProduct: productId={req.id}");
                    CaptureAuditTrail(userId.ToString(), "Delete Product", $"Deleted product {req.id}");
                    return Ok(new { success = true, message = "Product deleted successfully", action = "", data = new JObject { { "id", req.id } } });
                }
                return Bad("Failed to delete product");
            }
            catch (Exception ex) { iloggermanager.LogError("DeleteProduct: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("categories")]
        public ActionResult GetCategories()
        {
            iloggermanager.LogInfo("******* GET CATEGORIES REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("product_categories", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetCategories: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("addcategory")]
        public ActionResult AddCategory([FromBody] AddCategoryRequest req)
        {
            iloggermanager.LogInfo("******* ADD CATEGORY REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (req == null || string.IsNullOrEmpty(req.name)) return Bad("name is required");

                ProductCategoryModel model = new()
                {
                    pharmacy_id = pharmacyId,
                    name = req.name,
                    description = req.description,
                    created_by = userId
                };

                bool ok = dbhandler.AddCategory(model);
                if (ok && model.id > 0)
                {
                    iloggermanager.LogInfo($"AddCategory: categoryId={model.id}");
                    CaptureAuditTrail(userId.ToString(), "Add Category", $"Added category: {model.name}");
                    return Ok(new { success = true, message = "Category added successfully", action = "", data = new JObject { { "id", model.id } } });
                }
                return Bad("Failed to add category");
            }
            catch (Exception ex) { iloggermanager.LogError("AddCategory: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("deletecategory")]
        public ActionResult DeleteCategory([FromBody] DeleteCategoryRequest req)
        {
            iloggermanager.LogInfo("******* DELETE CATEGORY REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: id={req?.id}, user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                if (req == null || req.id <= 0) return Bad("id is required");
                bool ok = dbhandler.DeleteRecord(req.id, userId, "category");
                if (ok)
                {
                    iloggermanager.LogInfo($"DeleteCategory: categoryId={req.id}");
                    CaptureAuditTrail(userId.ToString(), "Delete Category", $"Deleted category {req.id}");
                    return Ok(new { success = true, message = "Category deleted successfully", action = "", data = new JObject { { "id", req.id } } });
                }
                return Bad("Failed to delete category");
            }
            catch (Exception ex) { iloggermanager.LogError("DeleteCategory: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("low-stock")]
        public ActionResult GetLowStockProducts()
        {
            iloggermanager.LogInfo("******* GET LOW STOCK PRODUCTS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("low_stock_products", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetLowStockProducts: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("expiring")]
        public ActionResult GetExpiringBatches()
        {
            iloggermanager.LogInfo("******* GET EXPIRING BATCHES REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("expiring_batches", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetExpiringBatches: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [NonAction]
        private List<Dictionary<string, object>> ToRows(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns) row[col.ColumnName] = dr[col];
                rows.Add(row);
            }
            return rows;
        }

        [NonAction]
        private (Int64 userId, Int64 pharmacyId, Int64 roleId) GetCaller()
        {
            Int64 userId = Convert.ToInt64(HttpContext.Items["user_id"]?.ToString() ?? "0");
            Int64 pharmacyId = Convert.ToInt64(HttpContext.Items["pharmacy_id"]?.ToString() ?? "0");
            Int64 roleId = Convert.ToInt64(HttpContext.Items["profile_id"]?.ToString() ?? "0");
            return (userId, pharmacyId, roleId);
        }

        [NonAction]
        private ActionResult Bad(string msg) =>
            StatusCode(StatusCodes.Status400BadRequest, new { success = false, message = msg, action = "", data = new JObject() });

        [NonAction]
        private ActionResult Forbidden(string msg) =>
            StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = msg, action = "", data = new JObject() });

        [NonAction]
        private ActionResult ServerError() =>
            StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Server error", action = "", data = new JObject() });

        [NonAction]
        public bool CaptureAuditTrail(string user, string action_type, string action_description)
        {
            AuditTrailModel audittrailmodel = new()
            {
                user_name = user,
                action_type = action_type,
                action_description = action_description,
                page_accessed = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}",
                client_ip_address = Request.HttpContext.Connection.RemoteIpAddress!.ToString(),
                session_id = "TODO"
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}
