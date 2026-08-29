// ============================================================
//  MediStock — categories.js (Riziki BFF pattern)
// ============================================================

var _catTable;

$(document).ready(function () {
    App.init();
    _catTable = InitCatTable();
    LoadCats(_catTable);

    $('#category-modal').on('hidden.bs.modal', function () {
        $('#categoryName, #categoryDescription').val('');
    });
});

function InitCatTable() {
    return $('#categoriesTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.id); },
        aoColumns: [
            { data: 'id',              autoWidth: true, sDefaultContent: '' },
            { data: 'name',            autoWidth: true, sDefaultContent: '—' },
            { data: 'description',     autoWidth: true, sDefaultContent: '—' },
            { data: 'created_on',      autoWidth: true, sDefaultContent: '—', render: function (d) { return formatDate(d); } },
            { data: 'is_active',       autoWidth: true, sDefaultContent: 'Active', render: function (d) { return (d == 1 || d === true) ? '<span class="label label-success">Active</span>' : '<span class="label label-default">Inactive</span>'; } },
            { bSortable: false, sDefaultContent: '<a href="#" class="btn btn-danger btn-xs delete-cat"><i class="fa fa-trash"></i> Delete</a>' }
        ]
    });
}

function LoadCats(oTable) {
    ajaxGet('/Products/GetCategories', function (data) {
        var t = oTable || _catTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── Delete ───────────────────────────────────────────────────────────────────
$('#categoriesTable').on('click', 'a.delete-cat', function (e) {
    e.preventDefault();
    var d = _catTable.fnGetData($(this).parents('tr')[0]);
    Swal.fire({
        title: 'Delete Category?',
        text: 'Delete "' + d.name + '"? Products in this category will be left without a category.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Delete',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        ajaxPost('/Products/DeleteCategory', { id: d.id }, function (res) {
            if (res.success) {
                LoadCats(_catTable);
                Swal.fire('Deleted', res.message, 'success');
            } else {
                Swal.fire('Error', res.message, 'error');
            }
        });
    });
});

// ── Add Save ─────────────────────────────────────────────────────────────────
$('#btnAddCat').on('click', function (e) {
    e.preventDefault();
    var name = $('#categoryName').val().trim();
    if (!name) { Swal.fire('Validation', 'Category name is required.', 'warning'); return; }
    var btn = this;
    btnLoad(btn, 'Saving...');
    ajaxPost('/Products/AddCategory', { name: name, description: $('#categoryDescription').val().trim() || null }, function (res) {
        btnStop(btn);
        if (res.success) {
            $('#category-modal').modal('hide');
            LoadCats(_catTable);
            Swal.fire('Saved', res.message, 'success');
        } else {
            Swal.fire('Error', res.message, 'error');
        }
    }, function () { btnStop(btn); Swal.fire('Error', 'Request failed.', 'error'); });
});
