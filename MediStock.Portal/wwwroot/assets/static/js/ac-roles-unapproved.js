// ============================================================
//  RIZIKI — ac-roles-unapproved.js  (Admin: Roles Approval)
// ============================================================
var _ruTable;

$(document).ready(function () {
    App.init();
    _ruTable = $('#rolesUnapprovedTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: 'id',         sDefaultContent: '—' },
            { data: 'role_name',  sDefaultContent: '—' },
            { data: 'created_on', sDefaultContent: '—' },
            { bSortable: false,
              mRender: function (d, t, row) {
                  return '<a href="#" class="btn btn-success btn-xs approve-role" data-id="' + row.id + '">'
                       + '<i class="fa fa-check"></i> Approve</a>';
              }}
        ]
    });
    LoadUnapprovedRoles();
});

function LoadUnapprovedRoles() {
    $.get('/AccessControl/GetRoles', function (data) {
        var pending = (data || []).filter(function (r) { return parseInt(r.is_approved) !== 1; });
        var t = _ruTable, s = t.fnSettings();
        t.fnClearTable(true);
        for (var i = 0; i < pending.length; i++) t.oApi._fnAddData(s, pending[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

$('#rolesUnapprovedTable').on('click', 'a.approve-role', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Approve Role?', icon: 'question', showCancelButton: true,
        confirmButtonText: 'Yes, Approve', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/AccessControl/ApproveRole', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadUnapprovedRoles(); Swal.fire('Approved!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});
