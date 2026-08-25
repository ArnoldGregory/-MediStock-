// ============================================================
//  RIZIKI — ac-permissions.js  (Admin: Permissions)
// ============================================================
var _permTable;

$(document).ready(function () {
    App.init();
    _permTable = $('#permissionsTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: 'id',              sDefaultContent: '—' },
            { data: 'permission_name', sDefaultContent: '—' },
            { data: 'created_on',      sDefaultContent: '—' },
            { bSortable: false,
              mRender: function (d, t, row) {
                  return '<a href="#" class="btn btn-danger btn-xs delete-perm" data-id="' + row.id + '">'
                       + '<i class="fa fa-trash"></i> Delete</a>';
              }}
        ]
    });
    LoadPermissions();

    $('#add-perm-modal').on('hidden.bs.modal', function () {
        $('#perm_name').val('');
        $('#perm-error').hide();
    });
});

function LoadPermissions() {
    $.get('/AccessControl/GetPermissions', function (data) {
        var t = _permTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

$('#permissionsTable').on('click', 'a.delete-perm', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Delete Permission?', text: 'This cannot be undone.', icon: 'warning',
        showCancelButton: true, confirmButtonText: 'Yes, Delete', confirmButtonColor: '#d33', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/AccessControl/DeletePermission', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadPermissions(); Swal.fire('Deleted!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

$('#btnSavePerm').on('click', function (e) {
    e.preventDefault();
    var name = $('#perm_name').val().trim();
    if (!name) { $('#perm-error').text('Permission name is required.').show(); return; }

    $.ajax({ url: '/AccessControl/SavePermission', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ permission_name: name }),
        success: function (res) {
            if (res.success) {
                $('#add-perm-modal').modal('hide');
                LoadPermissions();
                Swal.fire('Saved!', res.message, 'success');
            } else { $('#perm-error').text(res.message).show(); }
        }, error: function () { $('#perm-error').text('Request failed.').show(); }
    });
});
