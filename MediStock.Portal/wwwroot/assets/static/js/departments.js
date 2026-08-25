// ============================================================
//  RIZIKI — departments.js
// ============================================================

var _deptTable;

$(document).ready(function () {
    App.init();
    _deptTable = InitDeptTable();
    LoadDepts(_deptTable);

    $('#add-dept-modal').on('hidden.bs.modal', function () {
        $('#add_dept_name, #add_dept_desc').val('');
        $('#add-dept-error').hide();
    });
    $('#edit-dept-modal').on('hidden.bs.modal', function () {
        $('#edit-dept-error').hide();
    });
});

function InitDeptTable() {
    return $('#deptTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.id); },
        aoColumns: [
            { data: 'id',              autoWidth: true, sDefaultContent: '' },
            { data: 'department_name', autoWidth: true, sDefaultContent: 'N/A' },
            { data: 'description',     autoWidth: true, sDefaultContent: '—' },
            { data: 'created_on',      autoWidth: true, sDefaultContent: '—' },
            { bSortable: false, sDefaultContent: '<a href="#" class="btn btn-warning btn-xs edit-dept"><i class="fa fa-edit"></i> Edit</a>' },
            { bSortable: false, sDefaultContent: '<a href="#" class="btn btn-danger btn-xs delete-dept"><i class="fa fa-trash"></i> Delete</a>' }
        ]
    });
}

function LoadDepts(oTable) {
    $.get('/Departments/GetList', function (data) {
        var t = oTable || _deptTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── Edit click ───────────────────────────────────────────────────────────────
$('#deptTable').on('click', 'a.edit-dept', function (e) {
    e.preventDefault();
    var d = _deptTable.fnGetData($(this).parents('tr')[0]);
    $('#edit_dept_id').val(d.id);
    $('#edit_dept_name').val(d.department_name);
    $('#edit_dept_desc').val(d.description || '');
    $('#edit-dept-modal').appendTo('body').modal('show');
});

// ── Delete click ─────────────────────────────────────────────────────────────
$('#deptTable').on('click', 'a.delete-dept', function (e) {
    e.preventDefault();
    var d = _deptTable.fnGetData($(this).parents('tr')[0]);
    Swal.fire({
        title: 'Delete Department?',
        text: 'Delete "' + d.department_name + '"? This cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Delete',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Departments/DeleteDepartment', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: d.id }),
            success: function (res) {
                if (res.success) {
                    LoadDepts(_deptTable);
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
$('#btnAddDept').on('click', function (e) {
    e.preventDefault();
    var name = $('#add_dept_name').val().trim();
    if (!name) { $('#add-dept-error').text('Department name is required.').show(); return; }
    var btn = this;
    btnLoad(btn, 'Saving...');
    $.ajax({
        url: '/Departments/AddDepartment', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ department_name: name, description: $('#add_dept_desc').val().trim() }),
        success: function (res) {
            if (res.success) {
                $('#add-dept-modal').modal('hide');
                LoadDepts(_deptTable);
                Swal.fire('Saved', res.message, 'success');
            } else {
                $('#add-dept-error').text(res.message).show();
            }
        },
        error: function () { $('#add-dept-error').text('Request failed.').show(); },
        complete: function () { btnStop(btn); }
    });
});

// ── Edit Save ─────────────────────────────────────────────────────────────────
$('#btnEditDept').on('click', function (e) {
    e.preventDefault();
    var name = $('#edit_dept_name').val().trim();
    if (!name) { $('#edit-dept-error').text('Department name is required.').show(); return; }
    var btn = this;
    btnLoad(btn, 'Saving...');
    $.ajax({
        url: '/Departments/EditDepartment', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: parseInt($('#edit_dept_id').val()), department_name: name, description: $('#edit_dept_desc').val().trim() }),
        success: function (res) {
            if (res.success) {
                $('#edit-dept-modal').modal('hide');
                LoadDepts(_deptTable);
                Swal.fire('Updated', res.message, 'success');
            } else {
                $('#edit-dept-error').text(res.message).show();
            }
        },
        error: function () { $('#edit-dept-error').text('Request failed.').show(); },
        complete: function () { btnStop(btn); }
    });
});
