// ============================================================
//  RIZIKI — ac-roles.js  (Admin: Roles Management)
// ============================================================
var _rolesTable;

$(document).ready(function () {
    App.init();
    _rolesTable = $('#rolesTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: 'id',         sDefaultContent: '—' },
            { data: 'role_name',  sDefaultContent: '—' },
            { data: 'is_approved', sDefaultContent: '—',
              mRender: function (d) {
                  var approved = parseInt(d) === 1;
                  return '<span class="label label-' + (approved ? 'success' : 'warning') + '">'
                       + (approved ? 'Approved' : 'Pending') + '</span>';
              }},
            { data: 'created_on', sDefaultContent: '—' },
            { bSortable: false,
              mRender: function (d, t, row) {
                  var btns = '<a href="#" class="btn btn-info btn-xs m-r-3 edit-role" '
                           + 'data-id="' + row.id + '" data-name="' + (row.role_name||'') + '">'
                           + '<i class="fa fa-pencil"></i> Edit</a> ';
                  if (parseInt(row.is_approved) !== 1) {
                      btns += '<a href="#" class="btn btn-success btn-xs m-r-3 approve-role" data-id="' + row.id + '">'
                            + '<i class="fa fa-check"></i> Approve</a> ';
                  }
                  btns += '<a href="#" class="btn btn-danger btn-xs delete-role" data-id="' + row.id + '">'
                        + '<i class="fa fa-trash"></i> Delete</a>';
                  return btns;
              }}
        ]
    });
    LoadRoles();

    $('#add-role-modal').on('hidden.bs.modal', function () {
        $('#role_id').val('0');
        $('#role_name').val('');
        $('#role-error').hide();
        $('#roleModalTitle').html('<i class="fa fa-user-secret"></i> Add Role');
    });
});

function LoadRoles() {
    $.get('/AccessControl/GetRoles', function (data) {
        var t = _rolesTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// Edit click
$('#rolesTable').on('click', 'a.edit-role', function (e) {
    e.preventDefault();
    $('#role_id').val($(this).data('id'));
    $('#role_name').val($(this).data('name'));
    $('#roleModalTitle').html('<i class="fa fa-pencil"></i> Edit Role');
    $('#add-role-modal').modal('show');
});

// Approve click
$('#rolesTable').on('click', 'a.approve-role', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Approve Role?', text: 'Approve this role?', icon: 'question',
        showCancelButton: true, confirmButtonText: 'Yes, Approve', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/AccessControl/ApproveRole', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadRoles(); Swal.fire('Approved!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// Delete click
$('#rolesTable').on('click', 'a.delete-role', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Delete Role?', text: 'This cannot be undone.', icon: 'warning',
        showCancelButton: true, confirmButtonText: 'Yes, Delete', confirmButtonColor: '#d33', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/AccessControl/DeleteRole', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadRoles(); Swal.fire('Deleted!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// Save role
$('#btnSaveRole').on('click', function (e) {
    e.preventDefault();
    var name = $('#role_name').val().trim();
    if (!name) { $('#role-error').text('Role name is required.').show(); return; }

    $.ajax({ url: '/AccessControl/SaveRole', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ id: parseInt($('#role_id').val()) || 0, role_name: name }),
        success: function (res) {
            if (res.success) {
                $('#add-role-modal').modal('hide');
                LoadRoles();
                Swal.fire('Saved!', res.message, 'success');
            } else { $('#role-error').text(res.message).show(); }
        }, error: function () { $('#role-error').text('Request failed.').show(); }
    });
});
