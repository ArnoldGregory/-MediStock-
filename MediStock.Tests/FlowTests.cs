using System.Text.Json;
using Xunit;

namespace MediStock.Tests;

[Collection("api")]
public abstract class FlowTestBase
{
    protected ApiFixture Fx { get; }
    protected HttpClient C => Fx.Client;
    protected string Admin => Fx.AdminToken;
    protected string Super => Fx.SuperToken;

    protected FlowTestBase(ApiFixture fx) => Fx = fx;

    protected Task<(int status, JsonDocument? doc)> Get(string url, string? token = null)
        => Fx.SendAsync(HttpMethod.Get, url, null, token ?? Admin);
    protected Task<(int status, JsonDocument? doc)> Post(string url, object body, string? token = null)
        => Fx.SendAsync(HttpMethod.Post, url, body, token ?? Admin);
    protected Task<(int status, JsonDocument? doc)> Put(string url, object body, string? token = null)
        => Fx.SendAsync(HttpMethod.Put, url, body, token ?? Admin);
    protected Task<(int status, JsonDocument? doc)> Delete(string url, string? token = null)
        => Fx.SendAsync(HttpMethod.Delete, url, null, token ?? Admin);

    protected static void AssertOk((int status, JsonDocument? doc) r)
    {
        Assert.True(r.status == 200, $"status={r.status} body={r.doc?.RootElement.GetRawText()}");
        Assert.True(r.doc!.RootElement.GetProperty("success").GetBoolean(), r.doc.RootElement.GetRawText());
    }

    protected static void AssertRejected((int status, JsonDocument? doc) r)
    {
        Assert.InRange(r.status, 400, 599);
        Assert.False(r.doc!.RootElement.GetProperty("success").GetBoolean());
    }

    protected static long DataId(JsonDocument? doc)
        => doc!.RootElement.GetProperty("data").GetProperty("id").GetInt64();

    protected static JsonElement[] DataArray(JsonDocument? doc)
        => doc!.RootElement.GetProperty("data").EnumerateArray().ToArray();

    protected static string Rand(string prefix) => prefix + "_" + Guid.NewGuid().ToString("N")[..8];
}

// ============================================================
// AUTH
// ============================================================
[Collection("api")]
public class AuthFlowTests : FlowTestBase
{
    public AuthFlowTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Login_WrongPassword_IsRejected()
        => AssertRejected(await Post("api/auth/clientlogin", new { username = TestDatabase.AdminEmail, password = "WrongPass999" }));

    [Fact]
    public async Task Login_UnknownEmail_IsRejected()
        => AssertRejected(await Post("api/auth/clientlogin", new { username = Rand("ghost") + "@test.co", password = "x" }));

    [Fact]
    public async Task ResendOtp_UnknownEmail_404()
        => AssertRejected(await Post("api/auth/resendotp", new { username = Rand("ghost") + "@test.co" }));

    [Fact]
    public async Task ResendOtp_ValidUser_Ok()
        => AssertOk(await Post("api/auth/resendotp", new { username = TestDatabase.AdminEmail }));

    [Fact]
    public async Task ResetPassword_UnknownEmail_404()
        => AssertRejected(await Post("api/auth/resetpassword", new { email = Rand("ghost") + "@test.co" }));

    [Fact]
    public async Task ChangePassword_WrongCurrent_Rejected()
        => AssertRejected(await Post("api/auth/changepassword",
            new { email = TestDatabase.AdminEmail, password = "WrongOld99", newpassword = "NewPass123", confirmpassword = "NewPass123" }, Admin));

    [Fact]
    public async Task RegisterPharmacy_ThenLoginOk()
    {
        string email = Rand("owner") + "@test.co";
        var r = await Post("api/auth/register-pharmacy", new
        {
            pharmacy_name = Rand("Pharmacy"),
            pharmacy_email = email,
            pharmacy_phone = "0711000000",
            pharmacy_address = "Test Street",
            admin_first_name = "Owner",
            admin_last_name = "One",
            admin_email = email,
            admin_phone = "0711000001",
            password = "Passw0rd!",
            confirm_password = "Passw0rd!"
        });
        AssertOk(r);
        AssertOk(await Fx.SendAsync(HttpMethod.Post, "api/auth/clientlogin", new { username = email, password = "Passw0rd!" }));

        TestDatabase.Cleanup(
            $"DELETE FROM portal_users WHERE email = '{email}'",
            $"DELETE FROM pharmacies WHERE email = '{email}'");
    }
}

// ============================================================
// SUPPLIERS — add, list, get, update, delete
// ============================================================
[Collection("api")]
public class SupplierFlowTests : FlowTestBase
{
    public SupplierFlowTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Add_List_Get_Update_Delete()
    {
        string name = Rand("Supplier");
        var added = await Post("api/suppliers", new { name, phone = "0712000000", email = name + "@sup.co", address = "Nairobi" });
        AssertOk(added);
        long id = DataId(added.doc);

        var list = await Get("api/suppliers");
        AssertOk(list);
        Assert.Contains(DataArray(list.doc), r => r.GetProperty("name").GetString() == name);

        var byId = await Get($"api/suppliers/{id}");
        AssertOk(byId);
        Assert.Equal(name, DataArray(byId.doc)[0].GetProperty("name").GetString());

        string updated = name + " UPDATED";
        var upd = await Put($"api/suppliers/{id}", new { name = updated, phone = "0713000000", email = name + "@sup.co", address = "Mombasa" });
        AssertOk(upd);

        var after = await Get($"api/suppliers/{id}");
        Assert.Equal(updated, DataArray(after.doc)[0].GetProperty("name").GetString());

        AssertOk(await Delete($"api/suppliers/{id}"));
        var gone = await Get($"api/suppliers/{id}");
        AssertRejected(gone); // soft-deleted => 404

        TestDatabase.Cleanup($"DELETE FROM suppliers WHERE id = {id}");
    }
}

// ============================================================
// PRODUCTS & INVENTORY — categories, products, batches, adjustments
// ============================================================
[Collection("api")]
public class ProductFlowTests : FlowTestBase
{
    public ProductFlowTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Category_Product_Update_Delete()
    {
        string catName = Rand("Cat");
        var cat = await Post("api/products/addcategory", new { name = catName });
        AssertOk(cat);
        long catId = DataId(cat.doc);

        string sku = Rand("SKU");
        var prod = await Post("api/products/addproduct", new
        {
            name = Rand("Med"),
            sku,
            category_id = catId,
            cost_price = 40.00m,
            selling_price = 55.00m,
            reorder_level = 5,
            unit_of_measure = "pcs",
            is_controlled_drug = false
        });
        AssertOk(prod);
        long prodId = DataId(prod.doc);

        var byList = await Get($"api/products?pharmacyId={TestDatabase.PharmacyId}");
        AssertOk(byList);
        Assert.Contains(DataArray(byList.doc), r => r.GetProperty("id").GetInt64() == prodId);

        var upd = await Post("api/products/updateproduct", new
        {
            id = prodId,
            name = "Renamed Med",
            sku,
            category_id = catId,
            cost_price = 42.00m,
            selling_price = 58.00m,
            reorder_level = 5,
            unit_of_measure = "pcs",
            is_controlled_drug = false
        });
        AssertOk(upd);

        AssertOk(await Post("api/products/deleteproduct", new { id = prodId }));
        AssertOk(await Post("api/products/deletecategory", new { id = catId }));

        TestDatabase.Cleanup($"DELETE FROM product_categories WHERE id = {catId}");
    }

    [Fact]
    public async Task Batch_And_StockAdjustment()
    {
        string catName = Rand("CatB");
        var cat = await Post("api/products/addcategory", new { name = catName });
        long catId = DataId(cat.doc);
        var prod = await Post("api/products/addproduct", new
        {
            name = Rand("MedB"), sku = Rand("SKUB"), category_id = catId,
            cost_price = 30.00m, selling_price = 45.00m, reorder_level = 5, unit_of_measure = "pcs", is_controlled_drug = false
        });
        long prodId = DataId(prod.doc);

        var batch = await Post("api/stock/batches", new
        {
            product_id = prodId,
            batch_number = Rand("BATCH"),
            expiry_date = "2027-12-31",
            cost_price = 30.00m,
            quantity = 20
        });
        AssertOk(batch);
        long batchId = DataId(batch.doc);

        var batches = await Get("api/stock/batches");
        AssertOk(batches);
        Assert.Contains(DataArray(batches.doc), r => r.GetProperty("id").GetInt64() == batchId);

        var adj = await Post("api/stock/adjustments", new
        {
            product_id = prodId,
            batch_id = batchId,
            adjustment_type = "StockIn",
            quantity = 5,
            reason = "test adjustment"
        });
        AssertOk(adj);

        var adjustments = await Get("api/stock/adjustments");
        AssertOk(adjustments);

        TestDatabase.Cleanup(
            $"DELETE FROM stock_adjustments WHERE product_id = {prodId}",
            $"DELETE FROM product_batches WHERE id = {batchId}",
            $"DELETE FROM products WHERE id = {prodId}",
            $"DELETE FROM product_categories WHERE id = {catId}");
    }
}

// ============================================================
// SALES → RETURN → VOID (the full money/stock cycle)
// ============================================================
[Collection("api")]
public class SaleFlowTests : FlowTestBase
{
    public SaleFlowTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Sale_ThenReturn_ThenVoid_FullCycle()
    {
        // ---- seed a product + batch with stock = 10
        long catId;
        long prodId;
        long batchId;
        TestDatabase.Cleanup(
            "INSERT INTO product_categories (pharmacy_id, name, is_deleted, created_by, created_on) VALUES (9001, '" + Rand("CatS") + "', 0, 9001, NOW())");
        catId = Convert.ToInt64(TestDatabase.Scalar(
            "SELECT COALESCE(MAX(id),0) FROM product_categories WHERE pharmacy_id = 9001 AND is_deleted = 0"));
        TestDatabase.Cleanup(
            $"INSERT INTO products (pharmacy_id, category_id, name, sku, cost_price, selling_price, stock_qty, reorder_level, vat_rate, is_active, is_deleted, created_by, created_on) " +
            $"VALUES (9001, {catId}, '{"SaleMed " + Guid.NewGuid().ToString("N")[..8]}', 'SKUSALE', 45.00, 56.00, 10, 5, 0, 1, 0, 9001, NOW())");
        prodId = Convert.ToInt64(TestDatabase.Scalar("SELECT MAX(id) FROM products WHERE pharmacy_id = 9001 AND sku = 'SKUSALE'"));
        TestDatabase.Cleanup(
            $"INSERT INTO product_batches (pharmacy_id, product_id, batch_number, expiry_date, cost_price, quantity, quantity_sold, status, created_by) " +
            $"VALUES (9001, {prodId}, 'BATCHSALE', '2027-12-31', 45.00, 10, 0, 'Active', 9001)");
        batchId = Convert.ToInt64(TestDatabase.Scalar("SELECT MIN(id) FROM product_batches WHERE product_id = " + prodId));

        try
        {
            // ---- create a sale of 2 units
            var sale = await Post("api/sales", new
            {
                customer_id = (long?)null,
                sale_type = "Retail",
                subtotal = 112.00m,
                total_amount = 112.00m,
                discount = 0,
                tax = 0,
                net_amount = 112.00m,
                amount_paid = 112.00m,
                payment_method = "Cash",
                notes = "flow test",
                items = new[] { new { product_id = prodId, quantity = 2, unit_price = 56.00m, discount = 0, total = 112.00m, batch_id = batchId } }
            });
            AssertOk(sale);
            long saleId = DataId(sale.doc);

            Assert.Equal(8, Convert.ToInt32(TestDatabase.Scalar($"SELECT stock_qty FROM products WHERE id = {prodId}")));

            var saleById = await Get($"api/sales/{saleId}");
            AssertOk(saleById);
            Assert.Equal("Completed", DataArray(saleById.doc)[0].GetProperty("status").GetString());

            var items = await Get($"api/sales/{saleId}/items");
            AssertOk(items);
            var itemArr = DataArray(items.doc);
            Assert.Single(itemArr);
            long saleItemId = itemArr[0].GetProperty("id").GetInt64();

            // ---- return 1 unit → stock 9, returned_qty = 1
            var ret = await Post("api/returns", new
            {
                sale_id = saleId,
                customer_id = (long?)null,
                reason = "test return",
                items = new[]
                {
                    new { sale_item_id = saleItemId, product_id = prodId, batch_id = (long?)null,
                          quantity = 1, unit_price = 56.00m, refund = 56.00m }
                }
            });
            AssertOk(ret);
            Assert.Equal(9, Convert.ToInt32(TestDatabase.Scalar($"SELECT stock_qty FROM products WHERE id = {prodId}")));
            Assert.Equal(1.00m, Convert.ToDecimal(TestDatabase.Scalar($"SELECT returned_qty FROM sale_items WHERE id = {saleItemId}")));

            // ---- over-return: only 1 left, ask for 2 → rejected
            var over = await Post("api/returns", new
            {
                sale_id = saleId,
                customer_id = (long?)null,
                reason = "over",
                items = new[]
                {
                    new { sale_item_id = saleItemId, product_id = prodId, batch_id = (long?)null,
                          quantity = 2, unit_price = 56.00m, refund = 112.00m }
                }
            });
            AssertRejected(over);

            // ---- void the sale → status Voided, stock back to 10
            var voided = await Post("api/sales/voidsale", new { id = saleId, pharmacy_id = 9001 });
            AssertOk(voided);
            Assert.Equal("Voided", Convert.ToString(TestDatabase.Scalar($"SELECT status FROM sales WHERE id = {saleId}")));
            Assert.Equal(10, Convert.ToInt32(TestDatabase.Scalar($"SELECT stock_qty FROM products WHERE id = {prodId}")));

            // ---- second void → rejected
            AssertRejected(await Post("api/sales/voidsale", new { id = saleId, pharmacy_id = 9001 }));
        }
        finally
        {
            TestDatabase.Cleanup(
                $"DELETE FROM sales_return_items WHERE sale_item_id IN (SELECT id FROM sale_items WHERE sale_id IN (SELECT id FROM sales WHERE pharmacy_id = 9001 AND notes = 'flow test'))",
                $"DELETE FROM sales_returns WHERE sale_id IN (SELECT id FROM sales WHERE pharmacy_id = 9001 AND notes = 'flow test')",
                $"DELETE FROM sale_items WHERE sale_id IN (SELECT id FROM sales WHERE pharmacy_id = 9001 AND notes = 'flow test')",
                $"DELETE FROM sales WHERE pharmacy_id = 9001 AND notes = 'flow test'",
                $"DELETE FROM product_batches WHERE id = {batchId}",
                $"DELETE FROM products WHERE id = {prodId}",
                $"DELETE FROM product_categories WHERE id = {catId}");
        }
    }
}

// ============================================================
// REPORTS
// ============================================================
[Collection("api")]
public class ReportTests : FlowTestBase
{
    public ReportTests(ApiFixture fx) : base(fx) { }

    [Theory]
    [InlineData("sales")]
    [InlineData("stock")]
    [InlineData("financial")]
    [InlineData("margins")]
    [InlineData("expensebreakdown")]
    [InlineData("stock-performance")]
    public async Task Report_Endpoints_ReturnSuccess(string report)
        => AssertOk(await Get($"api/reports/{report}?pharmacyId={TestDatabase.PharmacyId}"));

    [Fact]
    public async Task Export_ReturnsXlsx()
    {
        using var req = Fx.Build(HttpMethod.Get, $"api/reports/export?report=stock", null, Admin);
        using var resp = await C.SendAsync(req);
        Assert.Equal(200, (int)resp.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", resp.Content.Headers.ContentType?.MediaType);
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0);
        Assert.Equal(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, bytes[..4]);
    }
}

// ============================================================
// NOTIFICATIONS
// ============================================================
[Collection("api")]
public class NotificationTests : FlowTestBase
{
    public NotificationTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Count_List_Dismiss_MarkAllRead()
    {
        string marker = Rand("NOTIF");
        TestDatabase.Cleanup(
            $"INSERT INTO notifications (pharmacy_id, user_id, title, message, notification_type, is_read, is_deleted, created_on) " +
            $"VALUES (9001, 9001, '{marker}', 'test', 'Info', 0, 0, NOW())");

        var count = await Get($"api/notifications/count?pharmacyId={TestDatabase.PharmacyId}");
        AssertOk(count);
        Assert.True(count.doc!.RootElement.GetProperty("data").GetProperty("count").GetInt32() >= 1);

        var list = await Get($"api/notifications?pharmacyId={TestDatabase.PharmacyId}");
        AssertOk(list);
        var n = DataArray(list.doc).FirstOrDefault(r => r.GetProperty("title").GetString() == marker);
        Assert.NotEqual(default, n);
        long id = n.GetProperty("id").GetInt64();

        AssertOk(await Post("api/notifications/dismiss", new { id = id }));
        AssertOk(await Post("api/notifications/markallread", new { pharmacy_id = 9001 }));

        TestDatabase.Cleanup($"DELETE FROM notifications WHERE title = '{marker}'");
    }
}

// ============================================================
// CUSTOMERS
// ============================================================
[Collection("api")]
public class CustomerFlowTests : FlowTestBase
{
    public CustomerFlowTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Add_List_WholesaleList()
    {
        string first = Rand("Buyer");
        var added = await Post("api/customers", new
        {
            customer_type = "Wholesale",
            first_name = first,
            last_name = "Test",
            phone = "0714000000",
            email = first + "@cust.co",
            credit_limit = 10000.00m,
            payment_terms = "Net 30"
        });
        AssertOk(added);
        long id = DataId(added.doc);

        var list = await Get("api/customers");
        AssertOk(list);
        Assert.Contains(DataArray(list.doc), r => r.GetProperty("id").GetInt64() == id);

        var wholesale = await Get("api/customers/wholesale");
        AssertOk(wholesale);
        Assert.Contains(DataArray(wholesale.doc), r => r.GetProperty("id").GetInt64() == id);

        TestDatabase.Cleanup($"DELETE FROM customers WHERE id = {id}");
    }
}

// ============================================================
// FINANCE, ADMIN, DDA, SETUP, SUPERADMIN, ACCESS
// ============================================================
[Collection("api")]
public class AdminAndFinanceTests : FlowTestBase
{
    public AdminAndFinanceTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task ExpenseCategory_AddAndDelete()
    {
        string name = Rand("ExpCat");
        var cat = await Post("api/finance/categories", new { name = name });
        AssertOk(cat);
        long catId = DataId(cat.doc);

        var exp = await Post("api/finance/expenses", new
        {
            category_id = catId,
            description = "test expense",
            amount = 1500.00m,
            expense_date = "2026-08-30",
            payment_method = "Cash",
            notes = "test"
        });
        AssertOk(exp);
        long expId = DataId(exp.doc);

        var list = await Get("api/finance/expenses");
        AssertOk(list);
        Assert.Contains(DataArray(list.doc), r => r.GetProperty("id").GetInt64() == expId);

        AssertOk(await Delete($"api/finance/expenses/{expId}"));

        TestDatabase.Cleanup(
            $"DELETE FROM expenses WHERE id = {expId}",
            $"DELETE FROM expense_categories WHERE id = {catId}");
    }

    [Fact]
    public async Task Admin_StatsUsersRolesAndAudit()
    {
        AssertOk(await Get("api/admin/users?pharmacyId=" + TestDatabase.PharmacyId));
        AssertOk(await Get("api/admin/stats"));
        AssertOk(await Get("api/admin/system-info"));
        AssertOk(await Get("api/admin/recent-logins"));
        AssertOk(await Get("api/access/roles"));
        AssertOk(await Get("api/access/menus"));
    }

    [Fact]
    public async Task DDA_Add_Get_List()
    {
        // seed a product to attach the DDA entry to
        TestDatabase.Cleanup(
            "INSERT INTO product_categories (pharmacy_id, name, is_deleted, created_by, created_on) VALUES (9001, '" + Rand("CatD") + "', 0, 9001, NOW())");
        long catId = Convert.ToInt64(TestDatabase.Scalar("SELECT MAX(id) FROM product_categories WHERE pharmacy_id = 9001"));
        TestDatabase.Cleanup(
            $"INSERT INTO products (pharmacy_id, category_id, name, sku, cost_price, selling_price, stock_qty, reorder_level, vat_rate, is_active, is_deleted, created_by, created_on) " +
            $"VALUES (9001, {catId}, '{"DDAMed " + Guid.NewGuid().ToString("N")[..8]}', 'SKUDDA', 20.00, 30.00, 5, 3, 0, 1, 0, 9001, NOW())");
        long prodId = Convert.ToInt64(TestDatabase.Scalar("SELECT MAX(id) FROM products WHERE pharmacy_id = 9001 AND sku = 'SKUDDA'"));

        try
        {
            var add = await Post("api/dda", new
            {
                product_id = prodId,
                entry_type = "Dispensed",
                quantity = 2,
                recipient_name = "Test Patient",
                notes = "test"
            });
            AssertOk(add);
            long ddaId = DataId(add.doc);

            AssertOk(await Get($"api/dda/{ddaId}"));
            AssertOk(await Get("api/dda"));
        }
        finally
        {
            TestDatabase.Cleanup(
                "DELETE FROM dda_register where 1=1",
                $"DELETE FROM products WHERE id = {prodId}",
                $"DELETE FROM product_categories WHERE id = {catId}");
        }
    }

    [Fact]
    public async Task SetupChecklist_Returns9Checks()
    {
        var r = await Get("api/setup/checklist");
        AssertOk(r);
        Assert.Equal(9, r.doc!.RootElement.GetProperty("data").GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task SuperAdmin_Endpoints()
    {
        AssertOk(await Get("api/superadmin/pharmacies", Super));
        AssertOk(await Get("api/superadmin/users", Super));
        AssertOk(await Get("api/superadmin/stats", Super));
    }
}