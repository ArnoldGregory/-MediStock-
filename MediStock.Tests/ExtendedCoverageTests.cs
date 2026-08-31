using System.Text.Json;
using Xunit;

namespace MediStock.Tests;

// ============================================================
// CLINICAL — patients + prescriptions (real DB rows asserted)
// ============================================================
[Collection("api")]
public class ClinicalFlowTests : FlowTestBase
{
    public ClinicalFlowTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Patient_Add_List_Get_Allergies_Conditions_WithDbRow()
    {
        string fname = Rand("Pat");
        var added = await Post("api/clinical/patients", new
        {
            first_name = fname,
            last_name = "Test",
            phone = "0720000000",
            email = fname + "@pt.co",
            date_of_birth = "1990-05-10",
            gender = "Male",
            address = "Nairobi",
            nhif_number = "NHIF-" + fname,
            allergies = "Penicillin",
            medical_history = "None"
        });
        AssertOk(added);
        long id = DataId(added.doc);

        Assert.Equal(1, Convert.ToInt32(TestDatabase.Scalar($"SELECT COUNT(*) FROM patients WHERE id={id} AND pharmacy_id=9001")));
        Assert.Equal(1, Convert.ToInt32(TestDatabase.Scalar($"SELECT COUNT(*) FROM patients WHERE pharmacy_id=9001 AND first_name='{fname.Replace("'", "''")}'")));

        var list = await Get("api/clinical/patients");
        AssertOk(list);
        Assert.Contains(DataArray(list.doc), r => r.GetProperty("id").GetInt64() == id);

        AssertOk(await Get($"api/clinical/patients/{id}"));
        AssertOk(await Get($"api/clinical/patients/{id}/allergies"));
        AssertOk(await Get($"api/clinical/patients/{id}/conditions"));

        TestDatabase.Cleanup($"DELETE FROM patients WHERE id = {id}");
    }

    [Fact]
    public async Task Prescription_Add_Get_Items_WithDbRows()
    {
        var pt = await Post("api/clinical/patients", new
        {
            first_name = Rand("RXP"),
            last_name = "Patient",
            phone = "0721111111",
            gender = "Female"
        });
        AssertOk(pt);
        long patientId = DataId(pt.doc);

        long rxId = 0;
        try
        {
            var add = await Post("api/clinical/prescriptions", new
            {
                patient_id = patientId,
                prescription_number = "RX-" + Rand(""),
                doctor_name = "Dr. Test",
                hospital = "Test Hospital",
                prescription_date = "2026-08-30",
                notes = "flow test",
                status = "Pending",
                items = new[]
                {
                    new { medication_name = "Amoxicillin", dosage = "500mg", frequency = "TDS", duration = "5 days", quantity = 15, notes = "" }
                }
            });
            AssertOk(add);
            rxId = DataId(add.doc);

            Assert.Equal(1, Convert.ToInt32(TestDatabase.Scalar($"SELECT COUNT(*) FROM prescriptions WHERE id={rxId} AND patient_id={patientId}")));
            Assert.Equal(1, Convert.ToInt32(TestDatabase.Scalar($"SELECT COUNT(*) FROM prescription_items WHERE prescription_id={rxId} AND medication_name='Amoxicillin'")));

            AssertOk(await Get($"api/clinical/prescriptions/{rxId}"));
            AssertOk(await Get($"api/clinical/prescriptions/{rxId}/items"));
            AssertOk(await Get($"api/clinical/prescriptions?patientId={patientId}"));
        }
        finally
        {
            if (rxId > 0) TestDatabase.Cleanup($"DELETE FROM prescription_items WHERE prescription_id = {rxId}");
            if (rxId > 0) TestDatabase.Cleanup($"DELETE FROM prescriptions WHERE id = {rxId}");
            TestDatabase.Cleanup($"DELETE FROM patients WHERE id = {patientId}");
        }
    }
}

// ============================================================
// STOCK TAKE — session + item + commit (real DB rows asserted)
// ============================================================
[Collection("api")]
public class StockTakeTests : FlowTestBase
{
    public StockTakeTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task StockTake_SessionItemCommit_WithDbRows()
    {
        TestDatabase.Cleanup(
            "INSERT INTO product_categories (pharmacy_id, name, is_deleted, created_by, created_on) VALUES (9001, '" + Rand("CatST") + "', 0, 9001, NOW())");
        long catId = Convert.ToInt64(TestDatabase.Scalar("SELECT COALESCE(MAX(id),0) FROM product_categories WHERE pharmacy_id = 9001 AND is_deleted = 0"));
        TestDatabase.Cleanup(
            $"INSERT INTO products (pharmacy_id, category_id, name, sku, cost_price, selling_price, stock_qty, reorder_level, vat_rate, is_active, is_deleted, created_by, created_on) " +
            $"VALUES (9001, {catId}, '{"STMed " + Guid.NewGuid().ToString("N")[..8]}', 'SKUST', 10.00, 15.00, 5, 2, 0, 1, 0, 9001, NOW())");
        long prodId = Convert.ToInt64(TestDatabase.Scalar("SELECT MAX(id) FROM products WHERE pharmacy_id = 9001 AND sku = 'SKUST'"));

        long sessionId = 0;
        long itemId = 0;
        string sessionName = Rand("Count");
        try
        {
            var session = await Post("api/stock/stocktake", new { session_name = sessionName });
            AssertOk(session);
            sessionId = Convert.ToInt64(TestDatabase.Scalar($"SELECT MAX(id) FROM stock_take_sessions WHERE pharmacy_id = 9001 AND session_name = '{sessionName}'"));
            Assert.Equal("Open", Convert.ToString(TestDatabase.Scalar($"SELECT status FROM stock_take_sessions WHERE id = {sessionId}")));

            var item = await Post("api/stock/stocktake/items", new
            {
                session_id = sessionId,
                product_id = prodId,
                system_qty = 5,
                counted_qty = 6,
                notes = "test count"
            });
            AssertOk(item);
            itemId = Convert.ToInt64(TestDatabase.Scalar($"SELECT MAX(id) FROM stock_take_items WHERE session_id = {sessionId}"));
            Assert.True(itemId > 0, "stock_take_items row was not created");
            Assert.Equal(1, Convert.ToInt32(TestDatabase.Scalar($"SELECT COUNT(*) FROM stock_take_items WHERE id = {itemId} AND variance = 1")));

            AssertOk(await Get("api/stock/stocktake"));
            AssertOk(await Post($"api/stock/stocktake/commit/{sessionId}", new { }));
            Assert.Equal("Committed", Convert.ToString(TestDatabase.Scalar($"SELECT status FROM stock_take_sessions WHERE id = {sessionId}")));
        }
        finally
        {
            if (itemId > 0) TestDatabase.Cleanup($"DELETE FROM stock_take_items WHERE id = {itemId}");
            if (sessionId > 0) TestDatabase.Cleanup($"DELETE FROM stock_take_sessions WHERE id = {sessionId}");
            TestDatabase.Cleanup($"DELETE FROM products WHERE id = {prodId}");
            TestDatabase.Cleanup($"DELETE FROM product_categories WHERE id = {catId}");
        }
    }
}

// ============================================================
// SETTINGS — profile update + config upsert (DB writes, restored)
// ============================================================
[Collection("api")]
public class SettingsFlowTests : FlowTestBase
{
    public SettingsFlowTests(ApiFixture fx) : base(fx) { }

    private static string Sql(string? v) =>
        v == null ? "NULL" : "'" + v.Replace("'", "''") + "'";

    [Fact]
    public async Task Settings_Get_ProfileUpdate_ConfigUpsert_WithDbWrites()
    {
        string originalName = Convert.ToString(TestDatabase.Scalar("SELECT name FROM pharmacies WHERE id = 9001")) ?? "";
        string? originalPhone = TestDatabase.Scalar("SELECT phone FROM pharmacies WHERE id = 9001") as string;

        string newName = Rand("Pharm");
        string cfgKey = Rand("CFG");

        try
        {
            AssertOk(await Post("api/settings/profile", new
            {
                name = newName,
                phone = "0701111111",
                email = "pharm@test.co",
                address = "New Address",
                license_number = "TST-0001",
                vat_number = "VAT-1",
                receipt_footer = "Thanks",
                currency = "KES"
            }));
            Assert.Equal(newName, Convert.ToString(TestDatabase.Scalar($"SELECT name FROM pharmacies WHERE id = 9001")));

            AssertOk(await Post("api/settings/config", new { key = cfgKey, value = "v1" }));
            Assert.Equal(1, Convert.ToInt32(TestDatabase.Scalar($"SELECT COUNT(*) FROM pharmacy_config WHERE pharmacy_id = 9001 AND config_key = '{cfgKey}'")));
            AssertOk(await Post("api/settings/config", new { key = cfgKey, value = "v2" }));
            Assert.Equal("v2", Convert.ToString(TestDatabase.Scalar($"SELECT config_value FROM pharmacy_config WHERE pharmacy_id = 9001 AND config_key = '{cfgKey}'")));

            AssertOk(await Get("api/settings"));
        }
        finally
        {
            TestDatabase.Cleanup($"DELETE FROM pharmacy_config WHERE config_key = '{cfgKey}'");
            TestDatabase.Cleanup($"UPDATE pharmacies SET name = {Sql(originalName)}, phone = {Sql(originalPhone)} WHERE id = 9001");
        }
    }
}

// ============================================================
// M-PESA — read endpoints + callback lifecycle through the DB
// ============================================================
[Collection("api")]
public class MpesaFlowTests : FlowTestBase
{
    public MpesaFlowTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Status_And_Payments_Endpoints()
    {
        AssertOk(await Get("api/mpesa/status?checkout_request_id=" + Rand("WSX")));
        AssertOk(await Get("api/mpesa/payments"));
    }

    [Fact]
    public async Task Callback_Success_UpdatesPaymentRow_AndNotifies()
    {
        string checkout = "WS_TEST_" + Guid.NewGuid().ToString("N")[..8];
        string receipt = "NAN" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        SeedPayment(checkout, receipt[..8]);

        try
        {
            using var body = JsonDocument.Parse($@"{{""Body"":{{""stkCallback"":{{""MerchantRequestID"":""MR_{checkout}"",""CheckoutRequestID"":""{checkout}"",""ResultCode"":0,""ResultDesc"":""The service request is processed successfully."",""CallbackMetadata"":{{""Item"":[{{""Name"":""Amount"",""Value"":50}},{{""Name"":""MpesaReceiptNumber"",""Value"":""{receipt}""}},{{""Name"":""TransactionDate"",""Value"":20260830205359}}]}}}}}}}}");
            var (status, _) = await Fx.SendAsync(HttpMethod.Post, "api/mpesa/callback", body.RootElement, null);

            Assert.Equal(200, status);
            Assert.Equal("Success", Convert.ToString(TestDatabase.Scalar($"SELECT status FROM mpesa_payments WHERE checkout_request_id = '{checkout}'")));
            Assert.Equal(receipt, Convert.ToString(TestDatabase.Scalar($"SELECT mpesa_receipt FROM mpesa_payments WHERE checkout_request_id = '{checkout}'")));
            Assert.Equal(50m, Convert.ToDecimal(TestDatabase.Scalar($"SELECT paid_amount FROM mpesa_payments WHERE checkout_request_id = '{checkout}'")));
            Assert.True(Convert.ToInt64(TestDatabase.Scalar("SELECT COUNT(*) FROM notifications WHERE title = 'M-Pesa payment received'")) >= 1);
        }
        finally
        {
            TestDatabase.Cleanup(
                $"DELETE FROM mpesa_payments WHERE checkout_request_id = '{checkout}'",
                "DELETE FROM notifications WHERE title = 'M-Pesa payment received'");
        }
    }

    [Fact]
    public async Task Callback_Failure_SetsPaymentFailed()
    {
        string checkout = "WS_FAIL_" + Guid.NewGuid().ToString("N")[..8];
        SeedPayment(checkout, "FAIL1");

        try
        {
            using var body = JsonDocument.Parse($@"{{""Body"":{{""stkCallback"":{{""MerchantRequestID"":""MR_{checkout}"",""CheckoutRequestID"":""{checkout}"",""ResultCode"":1032,""ResultDesc"":""Request cancelled by user.""}}}}}}");
            var (status, _) = await Fx.SendAsync(HttpMethod.Post, "api/mpesa/callback", body.RootElement, null);

            Assert.Equal(200, status);
            Assert.Equal("Failed", Convert.ToString(TestDatabase.Scalar($"SELECT status FROM mpesa_payments WHERE checkout_request_id = '{checkout}'")));
            Assert.Equal("1032", Convert.ToString(TestDatabase.Scalar($"SELECT result_code FROM mpesa_payments WHERE checkout_request_id = '{checkout}'")));
        }
        finally
        {
            TestDatabase.Cleanup($"DELETE FROM mpesa_payments WHERE checkout_request_id = '{checkout}'");
        }
    }

    private static void SeedPayment(string checkout, string suffix)
    {
        TestDatabase.Cleanup(
            $"INSERT INTO mpesa_payments (pharmacy_id, user_id, phone, amount, account_reference, transaction_desc, checkout_request_id, merchant_request_id, status) " +
            $"VALUES (9001, 9001, '254711111111', 50.00, 'TEST', 'test {suffix}', '{checkout}', 'MR_{checkout}', 'Pending')");
    }
}

// ============================================================
// READ-ONLY SMOKE — dashboard, menus, auth helpers, inventory
// ============================================================
[Collection("api")]
public class DashboardAndMenuTests : FlowTestBase
{
    public DashboardAndMenuTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Dashboard_AllEndpoints()
    {
        foreach (var route in new[] { "summary", "stocksummary", "salesstats", "expiringitems", "alerts", "mysales", "pendingorders" })
            AssertOk(await Get("api/dashboard/" + route));
    }

    [Fact]
    public async Task Menu_And_Auth_Helpers()
    {
        AssertOk(await Get("api/menus"));
        AssertOk(await Get("api/auth/check-slug?slug=" + Rand("slug")));
        AssertOk(await Get("api/auth/check-email?email=" + Rand("mail") + "@test.co"));
    }

    [Fact]
    public async Task Inventory_Helper_Endpoints()
    {
        AssertOk(await Get("api/sales/products"));
        AssertOk(await Get("api/products/low-stock"));
        AssertOk(await Get("api/products/expiring"));
    }
}

// ============================================================
// RETURNS / FINANCE / ACCESS / ADMIN read + validation paths
// ============================================================
[Collection("api")]
public class FinanceAccessReturnsTests : FlowTestBase
{
    public FinanceAccessReturnsTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Returns_List_And_Validation()
    {
        AssertOk(await Get("api/returns"));
        AssertRejected(await Get("api/returns/items?sale_id=0"));
        AssertRejected(await Get("api/returns/detail?return_id=0"));
    }

    [Fact]
    public async Task Finance_Expense_GetById()
    {
        string cat = Rand("ExpCat2");
        var c = await Post("api/finance/categories", new { name = cat });
        AssertOk(c);
        long catId = DataId(c.doc);

        var e = await Post("api/finance/expenses", new
        {
            category_id = catId,
            description = "smoke test expense",
            amount = 5.00m,
            expense_date = "2026-08-30",
            payment_method = "Cash",
            notes = "smoke"
        });
        AssertOk(e);
        long expId = DataId(e.doc);

        AssertOk(await Get($"api/finance/expenses/{expId}"));

        TestDatabase.Cleanup(
            $"DELETE FROM expenses WHERE id = {expId}",
            $"DELETE FROM expense_categories WHERE id = {catId}");
    }

    [Fact]
    public async Task Access_And_Admin_ReadHelpers()
    {
        var roles = await Get("api/access/roles");
        AssertOk(roles);
        var first = DataArray(roles.doc).FirstOrDefault();
        Assert.NotEqual(default, first);
        long roleId = first.GetProperty("id").GetInt64();
        AssertOk(await Get($"api/access/roles/{roleId}"));

        AssertOk(await Get("api/access/menu-access?roleId=2"));
        AssertOk(await Get("api/admin/users/9001"));
    }
}