using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.API.Helpers;
using MediStock.API.Models;
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
