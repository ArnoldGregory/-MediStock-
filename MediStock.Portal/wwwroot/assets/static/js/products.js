// ============================================================
//  MediStock — products.js (Riziki BFF pattern)
// ============================================================

var _prodTable;

$(document).ready(function () {
    App.init();
    _prodTable = InitProdTable();
    LoadCategories();
    LoadProducts(_prodTable);

    $('#product-modal').on('hidden.bs.modal', function () {
        ResetProductForm();
    });
});

function InitProdTable() {
    return $('#productsTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.id); },
        aoColumns: [
            { data: 'name',              autoWidth: true, sDefaultContent: '—' },
            { data: 'sku',               autoWidth: true, sDefaultContent: '—' },
            { data: 'category_name',     autoWidth: true, sDefaultContent: '—' },
            { data: 'selling_price',     autoWidth: true, sDefaultContent: '0.00', render: function (d) { return formatCurrency(d); } },
            { data: 'cost_price',        autoWidth: true, sDefaultContent: '0.00', render: function (d) { return formatCurrency(d); } },
            { data: 'stock_qty',         autoWidth: true, sDefaultContent: '0' },
            { data: 'reorder_level',     autoWidth: true, sDefaultContent: '0' },
            { data: 'is_controlled_drug', autoWidth: true, sDefaultContent: 'No', render: function (d) { return (d == 1 || d === true) ? 'Yes' : 'No'; } },
            { data: 'is_active',         autoWidth: true, sDefaultContent: 'Active', render: function (d) { return (d == 1 || d === true) ? '<span class="label label-success">Active</span>' : '<span class="label label-default">Inactive</span>'; } },
            { bSortable: false, sDefaultContent: '<a href="#" class="btn btn-warning btn-xs edit-prod"><i class="fa fa-edit"></i> Edit</a>' },
            { bSortable: false, sDefaultContent: '<a href="#" class="btn btn-danger btn-xs delete-prod"><i class="fa fa-trash"></i> Delete</a>' }
        ]
    });
}

function LoadProducts(oTable) {
    ajaxGet('/Products/GetProducts', function (data) {
        var t = oTable || _prodTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

function LoadCategories() {
    ajaxGet('/Products/GetCategories', function (data) {
        var $sel = $('#categoryId');
        var opts = ['<option value="">-- Select --</option>'];
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) {
                opts.push('<option value="' + data[i].id + '">' + data[i].name + '</option>');
            }
        }
        $sel.html(opts.join(''));
    });
}

function ResetProductForm() {
    $('#productForm')[0].reset();
    $('#productId').val('');
    $('#productModalLabel').html('<i class="fa fa-pills"></i> Add Product');
    $('#categoryId').prop('disabled', false);
}

function GetProductForm() {
    return {
        id: parseInt($('#productId').val()) || 0,
        name: $('#productName').val().trim(),
        sku: $('#productSku').val().trim() || null,
        barcode: $('#productBarcode').val().trim() || null,
        description: $('#productDescription').val().trim() || null,
        category_id: parseInt($('#categoryId').val()) || 0,
        cost_price: parseFloat($('#costPrice').val()) || 0,
        selling_price: parseFloat($('#sellingPrice').val()) || 0,
        reorder_level: parseInt($('#reorderLevel').val()) || 0,
        unit_of_measure: $('#unitOfMeasure').val().trim() || null,
        is_controlled_drug: $('#isDDA').val() === 'true'
    };
}

// ── Edit ─────────────────────────────────────────────────────────────────────
$('#productsTable').on('click', 'a.edit-prod', function (e) {
    e.preventDefault();
    var d = _prodTable.fnGetData($(this).parents('tr')[0]);
    $('#productId').val(d.id);
    $('#productName').val(d.name);
    $('#productSku').val(d.sku || '');
    $('#productBarcode').val(d.barcode || '');
    $('#productDescription').val(d.description || '');
    $('#categoryId').val(d.category_id).trigger('change');
    $('#costPrice').val(d.cost_price);
    $('#sellingPrice').val(d.selling_price);
    $('#reorderLevel').val(d.reorder_level);
    $('#unitOfMeasure').val(d.unit || d.unit_of_measure || '');
    var controlled = (d.is_controlled_drug == 1 || d.is_controlled_drug === true);
    $('#isDDA').val(controlled ? 'true' : 'false');
    $('#productModalLabel').html('<i class="fa fa-edit"></i> Edit Product');
    $('#product-modal').appendTo('body').modal('show');
});

// ── Delete ───────────────────────────────────────────────────────────────────
$('#productsTable').on('click', 'a.delete-prod', function (e) {
    e.preventDefault();
    var d = _prodTable.fnGetData($(this).parents('tr')[0]);
    Swal.fire({
        title: 'Delete Product?',
        text: 'Delete "' + d.name + '"? This cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Delete',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        ajaxPost('/Products/DeleteProduct', { id: d.id }, function (res) {
            if (res.success) {
                LoadProducts(_prodTable);
                Swal.fire('Deleted', res.message, 'success');
            } else {
                Swal.fire('Error', res.message, 'error');
            }
        });
    });
});

// ── Save (add / update) ──────────────────────────────────────────────────────
$('#btnSaveProduct').on('click', function (e) {
    e.preventDefault();
    var name = $('#productName').val().trim();
    if (!name) { Swal.fire('Validation', 'Product name is required.', 'warning'); return; }
    var id = parseInt($('#productId').val()) || 0;
    var url = id ? '/Products/UpdateProduct' : '/Products/AddProduct';
    var payload = GetProductForm();
    var btn = this;
    btnLoad(btn, 'Saving...');
    ajaxPost(url, payload, function (res) {
        btnStop(btn);
        if (res.success) {
            $('#product-modal').modal('hide');
            LoadProducts(_prodTable);
            Swal.fire(id ? 'Updated' : 'Saved', res.message, 'success');
        } else {
            Swal.fire('Error', res.message, 'error');
        }
    }, function () { btnStop(btn); Swal.fire('Error', 'Request failed.', 'error'); });
});
