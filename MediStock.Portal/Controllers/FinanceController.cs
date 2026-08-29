// ============================================================
//  MediStock.Portal — FinanceController (Riziki BFF pattern)
//  Routes:
//    GET  /Finance/Expenses          → expenses view
//    GET  /Finance/PurchaseOrders    → purchase orders finance view
//    GET  /Finance/GetExpenses       → JSON expenses list        → api/finance/expenses
//    GET  /Finance/GetExpense?id=    → JSON single expense       → api/finance/expenses/{id}
//    GET  /Finance/GetExpenseCategories → JSON categories        → api/finance/categories
//    POST /Finance/AddExpense        → proxy → POST api/finance/expenses
//    GET  /Finance/GetPurchaseOrders → JSON purchase orders      → api/suppliers/po
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediStock.Portal.Services;

namespace MediStock.Portal.Controllers
{
    [Authorize]
    public class FinanceController : Controller
    {
        private readonly ApiClient _api;
        private readonly AuditService _audit;

        public FinanceController(ApiClient api, AuditService audit)
        {
            _api = api;
            _audit = audit;
        }

        // ── Views ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Expenses()
        {
            await _audit.LogViewAsync("Finance/Expenses");
            return View();
        }

        public async Task<IActionResult> PurchaseOrders()
        {
            await _audit.LogViewAsync("Finance/PurchaseOrders");
            return View();
        }

        // ── Data ──────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetExpenses(string? from_date, string? to_date)
        {
            try
            {
                var qs = "api/finance/expenses?pharmacyId=" + GetPharmacyId();
                if (!string.IsNullOrWhiteSpace(from_date)) qs += $"&from_date={from_date}";
                if (!string.IsNullOrWhiteSpace(to_date)) qs += $"&to_date={to_date}";
                var result = await _api.GetAsync<object>(qs);
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetExpense(long id)
        {
            if (id <= 0) return Json(new { error = "id required" });
            try
            {
                var result = await _api.GetAsync<object>($"api/finance/expenses/{id}");
                return Json(result.IsSuccess ? result.Data : null);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenseCategories()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/finance/categories");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders()
        {
            try
            {
                var result = await _api.GetAsync<object>("api/suppliers/po");
                return Json(result.IsSuccess ? result.Data : new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense([FromBody] AddExpenseRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _api.PostAsync<object>("api/finance/expenses", new
            {
                category_id     = model.category_id,
                description     = model.description,
                amount          = model.amount,
                expense_date    = model.expense_date,
                payment_method  = model.payment_method,
                receipt_number  = model.receipt_number
            });

            return Json(result.IsSuccess
                ? new { success = true, message = "Expense recorded" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to record expense" : result.Error });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExpense([FromBody] IdRequest model)
        {
            if (model == null || model.id <= 0)
                return Json(new { success = false, message = "id is required" });

            var result = await _api.DeleteAsync<object>($"api/finance/expenses/{model.id}");
            return Json(result.IsSuccess
                ? new { success = true, message = "Expense deleted" }
                : new { success = false, message = string.IsNullOrEmpty(result.Error) ? "Failed to delete expense" : result.Error });
        }

        private string GetPharmacyId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == "pharmacy_id")?.Value ?? "0";
        }

        // ── Request models ────────────────────────────────────────────────────
        public class IdRequest { public long id { get; set; } }

        public class AddExpenseRequest
        {
            public long    category_id    { get; set; }
            public string? description    { get; set; }
            public decimal amount         { get; set; }
            public string? expense_date   { get; set; }
            public string? payment_method { get; set; }
            public string? receipt_number { get; set; }
        }
    }
}
