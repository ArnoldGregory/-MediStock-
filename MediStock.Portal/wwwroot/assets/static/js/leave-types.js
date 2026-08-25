// ============================================================
//  RIZIKI — leave-types.js
// ============================================================

var _typeTable;

$(document).ready(function () {
    App.init();
    _typeTable = InitTypeTable();
    LoadTypes();

    $('#add-type-modal').on('hidden.bs.modal', function () {
        $('#add_type_name, #add_type_days, #add_type_desc').val('');
        $('#add-type-error').hide();
    });
});

function InitTypeTable() {
    return $('#typeTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.id); },
        aoColumns: [
            { data: 'id',              autoWidth: true, sDefaultContent: '' },
            { data: 'leave_type_name', autoWidth: true, sDefaultContent: '—' },
            { data: 'annual_days',     autoWidth: true, sDefaultContent: '—' },
            { data: 'description',     autoWidth: true, sDefaultContent: '—' },
            { data: 'created_on',      autoWidth: true, sDefaultContent: '—' },
            { bSortable: false, sDefaultContent:
              '<a href="#" class="btn btn-danger btn-xs delete-type"><i class="fa fa-trash"></i> Delete</a>' }
        ]
    });
}

function LoadTypes() {
    $.get('/Leave/GetTypes', function (data) {
        var t = _typeTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── Delete click ─────────────────────────────────────────────────────────────
$('#typeTable').on('click', 'a.delete-type', function (e) {
    e.preventDefault();
    var d = _typeTable.fnGetData($(this).parents('tr')[0]);
    Swal.fire({
        title: 'Delete Leave Type?',
        text: 'Delete "' + d.leave_type_name + '"? This cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Delete',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Leave/DeleteType', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: d.id }),
            success: function (res) {
                if (res.success) {
                    LoadTypes();
                    Swal.fire('Deleted', res.message, 'success');
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// ── Add Save ─────────────────────────────────────────────────────────────────
$('#btnAddType').on('click', function (e) {
    e.preventDefault();
    var name = $('#add_type_name').val().trim();
    var days = parseInt($('#add_type_days').val()) || 0;
    if (!name) { $('#add-type-error').text('Leave type name is required.').show(); return; }
    if (days < 1) { $('#add-type-error').text('Annual days must be at least 1.').show(); return; }

    $.ajax({
        url: '/Leave/AddType', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ leave_type_name: name, annual_days: days, description: $('#add_type_desc').val().trim() }),
        success: function (res) {
            if (res.success) {
                $('#add-type-modal').modal('hide');
                LoadTypes();
                Swal.fire('Saved', res.message, 'success');
            } else {
                $('#add-type-error').text(res.message).show();
            }
        },
        error: function () { $('#add-type-error').text('Request failed.').show(); }
    });
});
