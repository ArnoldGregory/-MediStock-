// ============================================================
//  RIZIKI — ac-role-permissions.js  (Admin: Assign Permissions to Roles)
// ============================================================
var _assignedTable, _availableTable;
var _allPermissions = [];
var _assignedPerms  = [];

$(document).ready(function () {
    App.init();

    _assignedTable = $('#assignedTable').dataTable({
        bPaginate: false, bFilter: false,
        aoColumns: [
            { data: 'permission_name', sDefaultContent: '—' },
            { bSortable: false, mRender: function (d, t, row) {
                // SP may return the mapping PK as 'mapping_id' or 'id' — handle both
                var mappingId = row.mapping_id || row.id || 0;
                return '<a href="#" class="btn btn-danger btn-xs unassign-perm" data-id="' + mappingId + '">'
                     + '<i class="fa fa-minus"></i> Remove</a>';
            }}
        ]
    });

    _availableTable = $('#availableTable').dataTable({
        bPaginate: false, bFilter: false,
        aoColumns: [
            { data: 'permission_name', sDefaultContent: '—' },
            { bSortable: false, mRender: function (d, t, row) {
                // Available permissions come from GetPermissions — PK is 'id'
                return '<a href="#" class="btn btn-success btn-xs assign-perm" data-id="' + row.id + '">'
                     + '<i class="fa fa-plus"></i> Assign</a>';
            }}
        ]
    });

    // Load roles into dropdown
    $.get('/AccessControl/GetRoles', function (data) {
        var sel = $('#rp_role_id');
        (data || []).forEach(function (r) {
            sel.append('<option value="' + r.id + '">' + r.role_name + '</option>');
        });
    });

    // Load all permissions once
    $.get('/AccessControl/GetPermissions', function (data) {
        _allPermissions = data || [];
    });

    // Role change → load assigned/available
    $('#rp_role_id').on('change', function () {
        var roleId = $(this).val();
        if (!roleId) { $('#rp-content').hide(); return; }
        LoadRolePermissions(roleId);
    });

    // Assign
    $('#availableTable').on('click', 'a.assign-perm', function (e) {
        e.preventDefault();
        var permId = parseInt($(this).data('id'));
        var roleId = parseInt($('#rp_role_id').val());
        $.ajax({ url: '/AccessControl/AssignPermission', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ mode: 'allocate', role_id: roleId, permission_id: permId, id: 0 }),
            success: function (res) {
                if (res.success) LoadRolePermissions(roleId);
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });

    // Remove
    $('#assignedTable').on('click', 'a.unassign-perm', function (e) {
        e.preventDefault();
        var mappingId = parseInt($(this).data('id'));
        var roleId    = parseInt($('#rp_role_id').val());
        Swal.fire({ title: 'Remove Permission?', icon: 'warning',
            showCancelButton: true, confirmButtonText: 'Yes, Remove', confirmButtonColor: '#d33', reverseButtons: true
        }).then(function (r) {
            if (!r.isConfirmed) return;
            $.ajax({ url: '/AccessControl/AssignPermission', type: 'POST', contentType: 'application/json',
                data: JSON.stringify({ mode: 'unallocate', role_id: roleId, permission_id: 0, id: mappingId }),
                success: function (res) {
                    if (res.success) LoadRolePermissions(roleId);
                    else Swal.fire('Error', res.message, 'error');
                }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
            });
        });
    });
});

function LoadRolePermissions(roleId) {
    $.get('/AccessControl/GetRolePermissions', { role_id: roleId }, function (assigned) {
        _assignedPerms = assigned || [];
        // SP returns permission_id; fall back to id in case schema differs
        var assignedIds = _assignedPerms.map(function (p) {
            return parseInt(p.permission_id !== undefined ? p.permission_id : p.id);
        });
        var available = _allPermissions.filter(function (p) {
            return assignedIds.indexOf(parseInt(p.id)) === -1;
        });

        RefreshTable(_assignedTable, _assignedPerms);
        RefreshTable(_availableTable, available);
        $('#rp-content').show();
    });
}

function RefreshTable(t, rows) {
    var s = t.fnSettings();
    t.fnClearTable(true);
    (rows || []).forEach(function (r) { t.oApi._fnAddData(s, r); });
    s.aiDisplay = s.aiDisplayMaster.slice();
    t.fnDraw();
}
