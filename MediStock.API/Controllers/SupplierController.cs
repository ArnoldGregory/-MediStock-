using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
using MediStock.API.Services;
using Newtonsoft.Json.Linq;
using System.Data;

namespace MediStock.API.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    public class SupplierController : Controller
    {
        private readonly IConfiguration iconfiguration;
        private readonly IWebHostEnvironment ihostingenvironment;
        private readonly ILoggerManager iloggermanager;
        private readonly DBHandler dbhandler;

        public SupplierController(ILoggerManager logger, IWebHostEnvironment environment, IConfiguration configuration, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            iconfiguration = configuration;
            dbhandler = mydbhandler;
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetSuppliers()
        {
            iloggermanager.LogInfo("******* GET SUPPLIERS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("suppliers", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSuppliers: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("{id}")]
        public ActionResult GetSupplierById(Int64 id)
        {
            iloggermanager.LogInfo("******* GET SUPPLIER BY ID REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecordsById("supplier", id);
                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "Supplier not found", action = "", data = new JObject() });
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSupplierById: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddSupplier([FromBody] SupplierModel model)
        {
            iloggermanager.LogInfo("******* ADD SUPPLIER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (model == null || string.IsNullOrEmpty(model.name))
                    return Bad("Supplier name is required");

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddSupplier(model);
                if (ok && model.id > 0)
                {
                    iloggermanager.LogInfo($"AddSupplier: supplierId={model.id}");
                    CaptureAuditTrail(userId.ToString(), "Add Supplier", $"Added supplier: {model.name}");
                    return Ok(new { success = true, message = "Supplier added successfully", action = "", data = new JObject { { "id", model.id } } });
                }
                return Bad("Failed to add supplier");
            }
            catch (Exception ex) { iloggermanager.LogError("AddSupplier: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPut("{id}")]
        public ActionResult UpdateSupplier(Int64 id, [FromBody] SupplierModel model)
        {
            iloggermanager.LogInfo("******* UPDATE SUPPLIER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (model == null || string.IsNullOrEmpty(model.name))
                    return Bad("Supplier name is required");

                model.id = id;
                model.pharmacy_id = pharmacyId;

                bool ok = dbhandler.UpdateSupplier(model);
                if (ok)
                {
                    iloggermanager.LogInfo($"UpdateSupplier: supplierId={id}");
                    CaptureAuditTrail(userId.ToString(), "Update Supplier", $"Updated supplier: {model.name}");
                    return Ok(new { success = true, message = "Supplier updated successfully", action = "", data = new JObject() });
                }
                return Bad("Failed to update supplier");
            }
            catch (Exception ex) { iloggermanager.LogError("UpdateSupplier: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult DeleteSupplier(Int64 id)
        {
            iloggermanager.LogInfo("******* DELETE SUPPLIER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                bool ok = dbhandler.DeleteRecord(id, userId, "supplier");
                if (ok)
                {
                    iloggermanager.LogInfo($"DeleteSupplier: supplierId={id}");
                    CaptureAuditTrail(userId.ToString(), "Delete Supplier", $"Deleted supplier ID {id}");
                    return Ok(new { success = true, message = "Supplier deleted successfully", action = "", data = new JObject() });
                }
                return Bad("Failed to delete supplier");
            }
            catch (Exception ex) { iloggermanager.LogError("DeleteSupplier: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("po")]
        public ActionResult GetPurchaseOrders()
        {
            iloggermanager.LogInfo("******* GET PURCHASE ORDERS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("purchase_orders", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetPurchaseOrders: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("po/{id}")]
        public ActionResult GetPurchaseOrderById(Int64 id)
        {
            iloggermanager.LogInfo("******* GET PURCHASE ORDER BY ID REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecordsById("purchase_order", id);
                if (dt.Rows.Count == 0)
                    return StatusCode(StatusCodes.Status404NotFound, new { success = false, message = "Purchase order not found", action = "", data = new JObject() });
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetPurchaseOrderById: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("po/{id}/items")]
        public ActionResult GetPOItems(Int64 id)
        {
            iloggermanager.LogInfo("******* GET PO ITEMS REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("po_items", id.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetPOItems: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("po")]
        public ActionResult AddPurchaseOrder([FromBody] PurchaseOrderModel model)
        {
            iloggermanager.LogInfo("******* ADD PURCHASE ORDER REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (model == null || model.supplier_id <= 0)
                    return Bad("Supplier ID is required");

                model.pharmacy_id = pharmacyId;
                model.created_by = userId;

                bool ok = dbhandler.AddPurchaseOrder(model);
                if (ok && model.id > 0)
                {
                    iloggermanager.LogInfo($"AddPurchaseOrder: poId={model.id}");
                    CaptureAuditTrail(userId.ToString(), "Add Purchase Order", $"Created PO {model.id} for supplier {model.supplier_id}");
                    return Ok(new { success = true, message = "Purchase order created", action = "", data = new JObject { { "id", model.id } } });
                }
                return Bad("Failed to create purchase order");
            }
            catch (Exception ex) { iloggermanager.LogError("AddPurchaseOrder: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("po/{id}/receive")]
        public ActionResult ReceiveStock(Int64 id, [FromBody] ReceiveStockModel model)
        {
            iloggermanager.LogInfo("******* RECEIVE STOCK REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (model == null)
                    return Bad("Invalid receive stock data");

                var receiveStock = new ReceiveStockModel
                {
                    quantity_received = model.quantity_received,
                    notes             = model.notes,
                    received_by       = userId,
                    items = model.items ?? new List<ReceiveStockItemModel>()
                };

                bool ok = dbhandler.ReceiveStock(id, receiveStock);
                if (ok)
                {
                    iloggermanager.LogInfo($"ReceiveStock: poId={id}");
                    CaptureAuditTrail(userId.ToString(), "Receive Stock", $"Received stock for PO {id}");
                    return Ok(new { success = true, message = "Stock received successfully", action = "", data = new JObject() });
                }
                return Bad("Failed to receive stock");
            }
            catch (Exception ex) { iloggermanager.LogError("ReceiveStock: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpGet("price-history")]
        public ActionResult GetSupplierPriceHistory()
        {
            iloggermanager.LogInfo("******* GET SUPPLIER PRICE HISTORY REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");
                DataTable dt = dbhandler.GetRecords("supplier_price_history", pharmacyId.ToString());
                iloggermanager.LogInfo($"Result: dt.Rows.Count={dt.Rows.Count}");
                return Ok(new { success = true, message = "Success", action = "", data = ToRows(dt) });
            }
            catch (Exception ex) { iloggermanager.LogError("GetSupplierPriceHistory: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("import-invoice")]
        public ActionResult ImportInvoice(IFormFile file)
        {
            iloggermanager.LogInfo("******* IMPORT INVOICE REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}, file={file?.FileName}");

                if (file == null || file.Length == 0)
                    return Bad("No file uploaded");

                using var ms = new MemoryStream();
                file.CopyTo(ms);

                var parsed = InvoiceParsingService.Parse(ms.ToArray(), file.FileName);
                iloggermanager.LogInfo($"ImportInvoice: type={parsed.document_type} lines={parsed.lines.Count} invoice#={parsed.invoice_number}");

                return Ok(new { success = true, message = "Invoice parsed", action = "", data = parsed });
            }
            catch (Exception ex) { iloggermanager.LogError("ImportInvoice: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [Authorize]
        [HttpPost("import-confirm")]
        public ActionResult ImportConfirm([FromBody] ImportConfirmRequest req)
        {
            iloggermanager.LogInfo("******* IMPORT CONFIRM REQUEST **********");
            try
            {
                var (userId, pharmacyId, roleId) = GetCaller();
                iloggermanager.LogInfo($"REQUEST: user_id={userId}, pharmacy_id={pharmacyId}, role={roleId}");

                if (req == null || req.supplier_id <= 0)
                    return Bad("Supplier is required");

                var lines = req.lines?.Where(l => !l.skip && !string.IsNullOrWhiteSpace(l.product_name) && l.quantity > 0).ToList()
                           ?? new List<ImportConfirmLineModel>();
                if (lines.Count == 0)
                    return Bad("No valid line items to import");

                string poNumber = EnsureUniquePoNumber(req.po_number ?? "");
                decimal total = lines.Sum(l => l.quantity * l.unit_cost);
                decimal markup = req.markup_percent > 0 ? req.markup_percent : 25m;

                // 1. PO header (status Received â€” this order already arrived)
                long poId = dbhandler.ExecuteInsertReturnId(
                    "INSERT INTO purchase_orders (pharmacy_id, supplier_id, po_number, status, total, expected_date, received_date, created_by, created_on) " +
                    "VALUES (@pharmacy_id, @supplier_id, @po_number, 'Received', @total, @received_date, @received_date, @created_by, NOW())",
                    new { pharmacy_id = pharmacyId, supplier_id = req.supplier_id, po_number = poNumber, total = total, received_date = DateTime.Today, created_by = userId });
                if (poId <= 0)
                    return Bad("Failed to create purchase order header");

                // 2. Existing products (fresh snapshot for matching)
                var existing = new List<(long id, string name, decimal sell, int reorder)>();
                foreach (DataRow row in dbhandler.GetRecords("products", pharmacyId.ToString()).Rows)
                {
                    existing.Add((Convert.ToInt64(row["id"]), row["name"]?.ToString() ?? "", 
                        row["selling_price"] != DBNull.Value ? Convert.ToDecimal(row["selling_price"]) : 0m,
                        row["reorder_level"] != DBNull.Value ? Convert.ToInt32(row["reorder_level"]) : 5));
                }

                int created = 0, matched = 0;
                var categories = new List<(long id, string name)>();
                foreach (DataRow row in dbhandler.GetRecords("product_categories", pharmacyId.ToString()).Rows)
                {
                    categories.Add((Convert.ToInt64(row["id"]), row["name"]?.ToString() ?? ""));
                }

                foreach (var line in lines)
                {
                    long productId = MatchProduct(existing, line.product_name);
                    if (productId <= 0)
                    {
                        // New product â€” create it with sensible defaults + auto category
                        var pm = new ProductModel
                        {
                            pharmacy_id = pharmacyId,
                            category_id = MatchCategory(categories, line.product_name),
                            name = line.product_name,
                            sku = GenerateSku(pharmacyId, created),
                            cost_price = line.unit_cost,
                            selling_price = line.unit_sell_price ?? Math.Round(line.unit_cost * (1 + markup / 100m), 2),
                            reorder_level = 5,
                            unit_of_measure = "pcs",
                            created_by = userId
                        };
                        bool ok = dbhandler.AddProduct(pm);
                        if (!ok || pm.id <= 0) continue;
                        productId = pm.id;
                        existing.Add((productId, line.product_name, pm.selling_price, 5));
                        created++;
                    }
                    else
                    {
                        var ex = existing.First(e => e.id == productId);
                        matched++;
                        string upd = line.unit_sell_price.HasValue && line.unit_sell_price > 0
                            ? $"UPDATE products SET stock_qty = stock_qty + {line.quantity}, cost_price = {line.unit_cost}, selling_price = {line.unit_sell_price} WHERE id = {productId} AND pharmacy_id = {pharmacyId}"
                            : $"UPDATE products SET stock_qty = stock_qty + {line.quantity}, cost_price = {line.unit_cost} WHERE id = {productId} AND pharmacy_id = {pharmacyId}";
                        _ = dbhandler.ExecuteNonQuery(upd);
                    }

                    if (productId <= 0) continue;

                    // 3. Batch (the actual inventory increase)
                    string expiry = string.IsNullOrWhiteSpace(line.expiry_date) ? "NULL" : $"'{line.expiry_date}'";
                    _ = dbhandler.ExecuteNonQuery(
                        "INSERT INTO product_batches (pharmacy_id, product_id, batch_number, expiry_date, cost_price, quantity, status, created_by) " +
                        $"VALUES ({pharmacyId}, {productId}, '{Escape(poNumber)}', {expiry}, {line.unit_cost}, {line.quantity}, 'Active', {userId})");

                    // 4. PO line item
                    _ = dbhandler.ExecuteNonQuery(
                        "INSERT INTO po_items (po_id, product_id, quantity, received_qty, unit_cost, total) " +
                        $"VALUES ({poId}, {productId}, {line.quantity}, {line.quantity}, {line.unit_cost}, {line.quantity * line.unit_cost})");

                    // 5. Price history
                    _ = dbhandler.ExecuteNonQuery(
                        "INSERT INTO supplier_price_history (pharmacy_id, supplier_id, product_id, unit_cost, recorded_on) " +
                        $"VALUES ({pharmacyId}, {req.supplier_id}, {productId}, {line.unit_cost}, NOW())");
                }

                iloggermanager.LogInfo($"ImportConfirm: poId={poId} created={created} matched={matched}");
                CaptureAuditTrail(userId.ToString(), "Import Invoice", $"Imported {lines.Count} items from invoice {poNumber} ({created} new, {matched} matched)");
                return Ok(new { success = true, message = "Stock imported successfully", action = "", data = new JObject { { "po_id", poId }, { "po_number", poNumber }, { "created", created }, { "matched", matched }, { "total", total } } });
            }
            catch (Exception ex) { iloggermanager.LogError("ImportConfirm: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException); return ServerError(); }
        }

        [NonAction]
        private string EnsureUniquePoNumber(string requested)
        {
            string po = string.IsNullOrWhiteSpace(requested)
                ? "IMP-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                : requested.Trim();
            string test = po;
            int n = 1;
            while (GetAdhocScalarInt($"SELECT COUNT(*) AS c FROM purchase_orders WHERE po_number = '{Escape(test)}'") > 0)
                test = $"{po}-{n++}";
            return test;
        }

        [NonAction]
        private int GetAdhocScalarInt(string sql)
        {
            DataTable dt = dbhandler.GetAdhocData(sql);
            return dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value ? Convert.ToInt32(dt.Rows[0][0]) : 0;
        }

        [NonAction]
        private string GenerateSku(long pharmacyId, int seq) => $"MED-{pharmacyId}-{DateTime.Now:yyMMdd}-{seq + 1:D3}";

        [NonAction]
        private static string Escape(string s) => s.Replace("'", "''");

        [NonAction]
        private long MatchProduct(List<(long id, string name, decimal sell, int reorder)> existing, string name)
        {
            string n = Normalize(name);
            foreach (var e in existing)
                if (Normalize(e.name) == n) return e.id;
            foreach (var e in existing)
                if (Normalize(e.name).Contains(n) || n.Contains(Normalize(e.name))) return e.id;
            return 0;
        }

        [NonAction]
        private static string Normalize(string s)
        {
            var t = (s ?? "").ToLowerInvariant().Trim();
            t = System.Text.RegularExpressions.Regex.Replace(t, @"\s+", " ");
            return t.Trim(' ', '|', '=', '\t');
        }

        [NonAction]
        private static long MatchCategory(List<(long id, string name)> categories, string productName)
        {
            string n = Normalize(productName);
            if (categories.Count == 0) return 0;

            // Explicit category name appearing in the product name wins.
            foreach (var c in categories)
            {
                string cn = Normalize(c.name);
                if (cn.Length > 2 && n.Contains(cn)) return c.id;
            }

            // Keyword lookup â€” only assigns if the matched category already exists.
            foreach (var kv in CategoryKeywords)
            {
                foreach (string kw in kv.Value)
                {
                    if (!n.Contains(kw)) continue;
                    foreach (var c in categories)
                        if (Normalize(c.name) == kv.Key) return c.id;
                }
            }
            return 0;
        }

        private static readonly Dictionary<string, string[]> CategoryKeywords = new()
        {
            ["Antibiotics"] = new[] { "ampiclox", "amoxi", "ampicil", "augmentin", "cloxa", "cephalexin", "cef", "cipro", "azithro", "erythro", "doxy", "tetra", "penicil", "metronidazole", "flagyl", "co-amoxiclav", "clavulanate", "mycin" },
            ["Pain & Inflammation"] = new[] { "paracetamol", "acetaminophen", "ibuprofen", "diclofenac", "naproxen", "aspirin", "tramadol", "morphine", "codeine", "panadol", "gesic", "ketorolac", "meloxicam", "diclof" },
            ["Antimalarials"] = new[] { "artemether", "lumefantrine", "artesunate", "coartem", "quinine", "malaria", "pyrimethamine", "fansidar" },
            ["Hypertension"] = new[] { "lisinopril", "enalapril", "losartan", "valsartan", "amlodipine", "nifedipine", "captopril", "atenolol", "propranolol", "metoprolol", "telmisartan", "bisoprolol" },
            ["Diabetes"] = new[] { "metformin", "glibenclamide", "glimepiride", "gliclazide", "insulin", "sitagliptin", "vildagliptin", "pioglitazone" },
            ["Gastrointestinal"] = new[] { "omeprazole", "pantoprazole", "esomeprazole", "lansoprazole", "ranitidine", "domperidone", "metoclopramide", "buscopan", "hyoscine", "loperamide", "antacid", "maalox" },
            ["Antihistamines"] = new[] { "cetirizine", "loratadine", "chlorpheniramine", "diphenhydramine", "fexofenadine", "desloratadine", "promethazine", "piriton" },
            ["Respiratory"] = new[] { "salbutamol", "ventolin", "beclomethasone", "budesonide", "montelukast", "aminophylline", "theophylline", "serevent", "prednisone" },
            ["Vitamins & Supplements"] = new[] { "vitamin", "vit b", "vit c", "multivitamin", "ferrous", "folic", "calcium", "zinc", "magnesium", "omega", "supplement" }
        };

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
                client_ip_address = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                session_id = HttpContext.TraceIdentifier
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }
    }
}
