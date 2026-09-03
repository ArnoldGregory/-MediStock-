using System.Text.Json;
using Xunit;

namespace MediStock.Tests;

// ============================================================
// DISPENSING-FIRST + FAST SHOP MODE
//  Dispense (Rx) sales link a prescription and record the
//  dispenser; POS sales default to 'POS' for walk-in shop.
// ============================================================
[Collection("api")]
public class DispensingSaleTests : FlowTestBase
{
    public DispensingSaleTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task DispenseSale_LinksPrescription_AndRecordsDispenser()
    {
        long catId = 0, prodId = 0, batchId = 0, rxId = 0, patientId = 0, saleId = 0;
        string med = "DispMed " + Guid.NewGuid().ToString("N")[..8];
        try
        {
            // ---- patient
            var pt = await Post("api/clinical/patients", new
            {
                first_name = Rand("DxP"),
                last_name = "Patient",
                phone = "0733333333",
                gender = "Female"
            });
            AssertOk(pt);
            patientId = DataId(pt.doc);

            // ---- prescription (Pending → supplies a prescription_id for the sale)
            var rx = await Post("api/clinical/prescriptions", new
            {
                patient_id = patientId,
                prescription_number = "RX-" + Rand(""),
                doctor_name = "Dr. Dispense",
                hospital = "Test Hospital",
                prescription_date = "2026-08-31",
                notes = "dispense flow test",
                status = "Pending",
                items = new[]
                {
                    new { medication_name = med, dosage = "500mg", frequency = "TDS", duration = "5 days", quantity = 5, notes = "" }
                }
            });
            AssertOk(rx);
            rxId = DataId(rx.doc);

            // ---- product + batch (stock 10)
            TestDatabase.Cleanup(
                "INSERT INTO product_categories (pharmacy_id, name, is_deleted, created_by, created_on) VALUES (9001, '" + Rand("CatD") + "', 0, 9001, NOW())");
            catId = Convert.ToInt64(TestDatabase.Scalar(
                "SELECT COALESCE(MAX(id),0) FROM product_categories WHERE pharmacy_id = 9001 AND is_deleted = 0"));
            TestDatabase.Cleanup(
                $"INSERT INTO products (pharmacy_id, category_id, name, sku, cost_price, selling_price, stock_qty, reorder_level, vat_rate, is_active, is_deleted, created_by, created_on) " +
                $"VALUES (9001, {catId}, '{med}', 'SKUDISP', 45.00, 56.00, 10, 5, 0, 1, 0, 9001, NOW())");
            prodId = Convert.ToInt64(TestDatabase.Scalar("SELECT MAX(id) FROM products WHERE pharmacy_id = 9001 AND sku = 'SKUDISP'"));
            TestDatabase.Cleanup(
                $"INSERT INTO product_batches (pharmacy_id, product_id, batch_number, expiry_date, cost_price, quantity, quantity_sold, status, created_by) " +
                $"VALUES (9001, {prodId}, 'BATCHDISP', '2027-12-31', 45.00, 10, 0, 'Active', 9001)");
            batchId = Convert.ToInt64(TestDatabase.Scalar("SELECT MIN(id) FROM product_batches WHERE product_id = " + prodId));

            // ---- dispense-mode sale (customer_id null → walk-in; dispensed_by null → sold_by=9001)
            var sale = await Post("api/sales", new
            {
                customer_id = (long?)null,
                sale_type = "Retail",
                sale_mode = "DISPENSE",
                prescription_id = rxId,
                dispensed_by = (long?)null,
                subtotal = 112.00m,
                total_amount = 112.00m,
                discount = 0,
                tax = 0,
                net_amount = 112.00m,
                amount_paid = 112.00m,
                payment_method = "Cash",
                notes = "dispense flow test",
                items = new[] { new { product_id = prodId, quantity = 2, unit_price = 56.00m, discount = 0, total = 112.00m, batch_id = batchId } }
            });
            AssertOk(sale);
            saleId = DataId(sale.doc);

            Assert.Equal("DISPENSE", Convert.ToString(
                TestDatabase.Scalar($"SELECT sale_mode FROM sales WHERE id = {saleId}")));
            Assert.Equal(rxId, Convert.ToInt64(
                TestDatabase.Scalar($"SELECT prescription_id FROM sales WHERE id = {saleId}")));
            Assert.Equal(9001, Convert.ToInt64(
                TestDatabase.Scalar($"SELECT dispensed_by FROM sales WHERE id = {saleId}")));
            Assert.Equal("Completed", Convert.ToString(
                TestDatabase.Scalar($"SELECT status FROM sales WHERE id = {saleId}")));
            Assert.Equal(8, Convert.ToInt32(
                TestDatabase.Scalar($"SELECT stock_qty FROM products WHERE id = {prodId}")));
        }
        finally
        {
            if (saleId > 0)  TestDatabase.Cleanup($"DELETE FROM sale_items WHERE sale_id = {saleId}");
            if (saleId > 0)  TestDatabase.Cleanup($"DELETE FROM sales WHERE id = {saleId}");
            if (rxId > 0)    TestDatabase.Cleanup($"DELETE FROM prescription_items WHERE prescription_id = {rxId}");
            if (rxId > 0)    TestDatabase.Cleanup($"DELETE FROM prescriptions WHERE id = {rxId}");
            if (patientId > 0) TestDatabase.Cleanup($"DELETE FROM patients WHERE id = {patientId}");
            if (batchId > 0) TestDatabase.Cleanup($"DELETE FROM product_batches WHERE id = {batchId}");
            if (prodId > 0)  TestDatabase.Cleanup($"DELETE FROM products WHERE id = {prodId}");
            if (catId > 0)   TestDatabase.Cleanup($"DELETE FROM product_categories WHERE id = {catId}");
        }
    }
}

// ============================================================
// PROCUREMENT — PO → Receive Stock (per-line partial receive)
//  Receiving less than ordered bumps the right batch + stock
//  and marks the PO Partial; finishing it marks it Received.
// ============================================================
[Collection("api")]
public class PartialReceiveTests : FlowTestBase
{
    public PartialReceiveTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task PartialReceive_ThenFinalReceive_UpdatesStockAndStatus()
    {
        long supId = 0, catId = 0, prodId = 0, poId = 0;
        try
        {
            // ---- supplier
            var sup = await Post("api/suppliers", new { name = Rand("SupR"), phone = "0744444444", email = Rand("s") + "@sup.co" });
            AssertOk(sup);
            supId = DataId(sup.doc);

            // ---- category + product (stock 0)
            TestDatabase.Cleanup(
                "INSERT INTO product_categories (pharmacy_id, name, is_deleted, created_by, created_on) VALUES (9001, '" + Rand("CatR") + "', 0, 9001, NOW())");
            catId = Convert.ToInt64(TestDatabase.Scalar(
                "SELECT COALESCE(MAX(id),0) FROM product_categories WHERE pharmacy_id = 9001 AND is_deleted = 0"));
            TestDatabase.Cleanup(
                $"INSERT INTO products (pharmacy_id, category_id, name, sku, cost_price, selling_price, stock_qty, reorder_level, vat_rate, is_active, is_deleted, created_by, created_on) " +
                $"VALUES (9001, {catId}, '{"RecvMed " + Guid.NewGuid().ToString("N")[..8]}', 'SKURECV', 40.00, 55.00, 0, 5, 0, 1, 0, 9001, NOW())");
            prodId = Convert.ToInt64(TestDatabase.Scalar("SELECT MAX(id) FROM products WHERE pharmacy_id = 9001 AND sku = 'SKURECV'"));

            // ---- PO for 10 units of the product
            var po = await Post("api/suppliers/po", new
            {
                supplier_id = supId,
                product_id = prodId,
                quantity = 10,
                unit_cost = 40.00m,
                total_cost = 400.00m,
                expected_date = "2026-09-15",
                notes = "receive flow test"
            });
            AssertOk(po);
            poId = DataId(po.doc);

            // ---- partial receive: 4 of 10
            var partial = await Post($"api/suppliers/po/{poId}/receive", new
            {
                quantity_received = 4,
                notes = "first delivery",
                items = new[]
                {
                    new { product_id = prodId, batch_number = "RECVB1", expiry_date = "2027-12-31", unit_cost = 40.00m, quantity = 4 }
                }
            });
            AssertOk(partial);

            Assert.Equal(4, Convert.ToInt32(TestDatabase.Scalar($"SELECT stock_qty FROM products WHERE id = {prodId}")));
            Assert.Equal(4, Convert.ToInt32(TestDatabase.Scalar(
                $"SELECT received_qty FROM po_items WHERE po_id = {poId} AND product_id = {prodId}")));
            Assert.Equal(4, Convert.ToInt32(TestDatabase.Scalar(
                $"SELECT COALESCE(SUM(quantity),0) FROM product_batches WHERE product_id = {prodId} AND batch_number = 'RECVB1'")));
            Assert.Equal("Partial", Convert.ToString(
                TestDatabase.Scalar($"SELECT status FROM purchase_orders WHERE id = {poId}")));

            // ---- final receive: 6 more → Received
            var final = await Post($"api/suppliers/po/{poId}/receive", new
            {
                quantity_received = 6,
                notes = "final delivery",
                items = new[]
                {
                    new { product_id = prodId, batch_number = "RECVB2", expiry_date = "2027-12-31", unit_cost = 40.00m, quantity = 6 }
                }
            });
            AssertOk(final);

            Assert.Equal(10, Convert.ToInt32(TestDatabase.Scalar($"SELECT stock_qty FROM products WHERE id = {prodId}")));
            Assert.Equal("Received", Convert.ToString(
                TestDatabase.Scalar($"SELECT status FROM purchase_orders WHERE id = {poId}")));
        }
        finally
        {
            if (poId > 0) TestDatabase.Cleanup(
                $"DELETE FROM supplier_price_history WHERE pharmacy_id = 9001 AND supplier_id = {supId}",
                $"DELETE FROM po_items WHERE po_id = {poId}");
            if (poId > 0) TestDatabase.Cleanup($"DELETE FROM purchase_orders WHERE id = {poId}");
            if (prodId > 0) TestDatabase.Cleanup($"DELETE FROM product_batches WHERE product_id = {prodId}");
            if (prodId > 0) TestDatabase.Cleanup($"DELETE FROM products WHERE id = {prodId}");
            if (catId > 0) TestDatabase.Cleanup($"DELETE FROM product_categories WHERE id = {catId}");
            if (supId > 0) TestDatabase.Cleanup($"DELETE FROM suppliers WHERE id = {supId}");
        }
    }
}

// ============================================================
// MENU — single Settings entry per role (no duplicate)
//  Guarantees the consolidated menu has one 'Settings' child
//  per role and 'Setup Checklist' as its own top-level.
// ============================================================
[Collection("api")]
public class MenuConsolidationTests : FlowTestBase
{
    public MenuConsolidationTests(ApiFixture fx) : base(fx) { }

    [Fact]
    public async Task Settings_MenuHasNoDuplicate_PerRole()
    {
        AssertOk(await Get("api/access/menus"));
        AssertOk(await Get("api/access/roles"));

        // No role can have more than one Settings/Settings entry.
        int dup = Convert.ToInt32(TestDatabase.Scalar(
            "SELECT COUNT(*) FROM ( " +
            "  SELECT role_id FROM menu_access " +
            "  WHERE main_menu_name = 'Settings' AND sub_menu_name = 'Settings' " +
            "  GROUP BY role_id HAVING COUNT(*) > 1 " +
            ") t"));
        Assert.Equal(0, dup);

        // 'Setup Checklist' exists as its own top-level menu child.
        int setup = Convert.ToInt32(TestDatabase.Scalar(
            "SELECT COUNT(*) FROM menu_access WHERE main_menu_name = 'Setup Checklist' AND sub_menu_name = 'Setup Checklist'"));
        Assert.True(setup >= 1, "Setup Checklist should exist as its own top-level menu");
    }
}
