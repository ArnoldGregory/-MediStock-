// ============================================================
//  RIZIKI — ac-users-unapproved.js  (Admin: Approve Users)
// ============================================================
var _unapprovedUsersTable;
var _rolesMapU = {};

$(document).ready(function () {
    App.init();

    _unapprovedUsersTable = $('#unapprovedUsersTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; }},
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, row) {
                  return ((row.first_name || '') + ' ' + (row.last_name || '')).trim() || '—';
              }},
            { data: 'email',  sDefaultContent: '—' },
            { data: 'mobile', sDefaultContent: '—' },
            { data: 'role_id', sDefaultContent: '—',
              mRender: function (d) { return _rolesMapU[d] || ('Role ' + d); }},
            { bSortable: false,
              mRender: function (d, t, row) {
                  return '<a href="#" class="btn btn-success btn-sm approve-user-u" data-id="' + row.id + '">'
                       + '<i class="fa fa-check"></i> Approve</a>';
              }}
        ]
    });

    // Populate role map first, then load users
    $.get('/AccessControl/GetRoles', function (roles) {
        (roles || []).forEach(function (r) { _rolesMapU[r.id] = r.role_name; });
        LoadUnapprovedUsers();
    });

    // Approve
    $('#unapprovedUsersTable').on('click', 'a.approve-user-u', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        Swal.fire({ title: 'Approve User?', icon: 'question',
            showCancelButton: true, confirmButtonText: 'Yes, Approve', reverseButtons: true
        }).then(function (r) {
            if (!r.isConfirmed) return;
            $.ajax({ url: '/AccessControl/ApproveUser', type: 'POST', contentType: 'application/json',
                data: JSON.stringify({ id: id }),
                success: function (res) {
                    if (res.success) { LoadUnapprovedUsers(); Swal.fire('Approved!', res.message, 'success'); }
                    else Swal.fire('Error', res.message, 'error');
                }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
            });
        });
    });
});

function LoadUnapprovedUsers() {
    $.get('/AccessControl/GetUsers', function (data) {
        var pending = (data || []).filter(function (u) { return parseInt(u.is_approved) !== 1; });
        var t = _unapprovedUsersTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (pending.length) for (var i = 0; i < pending.length; i++) t.oApi._fnAddData(s, pending[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}
