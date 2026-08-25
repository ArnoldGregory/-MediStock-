// ============================================================
//  RIZIKI — leave-pending.js  (HR Leave Approvals)
// ============================================================

var _leaveTable;
var _leaveData = [];

$(document).ready(function () {
    App.init();
    _leaveTable = InitLeaveTable();
    LoadLeave('PENDING');

    // Load department dropdown
    $.get('/Employees/GetDepartments', function (data) {
        var sel = $('#leave_dept_filter');
        (data || []).forEach(function (d) {
            sel.append('<option value="' + (d.id || d.department_id) + '">' + (d.department_name || d.name) + '</option>');
        });
    });

    // Dept filter change → re-render
    $('#leave_dept_filter').on('change', function () { RenderLeaveTable(); });

    $('#view-leave-modal').on('hidden.bs.modal', function () {
        $('#leave_reject_reason').val('');
        $('#leave-action-error').hide();
    });
});

function FmtDate(v) { return v ? v.toString().split('T')[0] : '—'; }

function InitLeaveTable() {
    return $('#leaveTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.leave_id || data.id); },
        aoColumns: [
            { data: 'employee_name', autoWidth: true, sDefaultContent: '—' },
            { data: 'leave_type_name', autoWidth: true, sDefaultContent: '—' },
            { data: 'start_date', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) { return FmtDate(d); } },
            { data: 'end_date', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) { return FmtDate(d); } },
            { data: 'number_of_days', autoWidth: true, sDefaultContent: '—' },
            { data: 'reason', autoWidth: true, sDefaultContent: '—' },
            { data: 'status', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) {
                  var cls = d === 'APPROVED' ? 'success' : d === 'REJECTED' ? 'danger' : 'warning';
                  return '<span class="label label-' + cls + '">' + (d || '—') + '</span>';
              }},
            { data: 'hod_status', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) {
                  if (!d || d === '') return '<span class="text-muted">—</span>';
                  var cls = d === 'APPROVED' ? 'success' : d === 'REJECTED' ? 'danger' : 'warning';
                  return '<span class="label label-' + cls + '">' + d + '</span>';
              }},
            { data: 'created_on', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) { return FmtDate(d); } },
            { bSortable: false, sDefaultContent:
              '<a href="#" class="btn btn-info btn-xs view-leave"><i class="fa fa-eye"></i> View</a>' }
        ]
    });
}

var _currentLeaveStatus = 'PENDING';

function LoadLeave(status) {
    _currentLeaveStatus = status === undefined ? 'PENDING' : status;

    // highlight active filter button
    $('#btnFilterPending, #btnFilterAll').removeClass('active');
    if (_currentLeaveStatus === 'PENDING') $('#btnFilterPending').addClass('active');
    else $('#btnFilterAll').addClass('active');

    $.get('/Leave/GetAll', { status: _currentLeaveStatus }, function (data) {
        _leaveData = data || [];
        RenderLeaveTable();
    });
}

function RenderLeaveTable() {
    var deptId = $('#leave_dept_filter').val() || '';
    var rows = deptId
        ? _leaveData.filter(function (r) { return String(r.department_id) === deptId; })
        : _leaveData;

    var t = _leaveTable, s = t.fnSettings();
    t.fnClearTable(true);
    for (var i = 0; i < rows.length; i++) t.oApi._fnAddData(s, rows[i]);
    s.aiDisplay = s.aiDisplayMaster.slice();
    t.fnDraw();
}

// ── View click ───────────────────────────────────────────────────────────────
$('#leaveTable').on('click', 'a.view-leave', function (e) {
    e.preventDefault();
    var d = _leaveTable.fnGetData($(this).parents('tr')[0]);

    $('#view_leave_id').val(d.leave_id || d.id);
    $('#view_leave_emp').val(d.employee_name || '—');
    $('#view_leave_type').val(d.leave_type_name || '—');
    $('#view_leave_from').val(FmtDate(d.start_date));
    $('#view_leave_to').val(FmtDate(d.end_date));
    $('#view_leave_days').val(d.number_of_days || '—');
    $('#view_leave_reason').val(d.reason || '—');
    $('#view_leave_applied').val(FmtDate(d.created_on));
    $('#view_leave_status').val(d.status || '—');
    $('#leave_reject_reason').val('');
    $('#leave-action-error').hide();
    $('#hod-pending-warning').remove(); // clear any prior warning

    var isPending    = (d.status || '').toUpperCase() === 'PENDING';
    var hodStatus    = (d.hod_status || '').toUpperCase();
    var hodPending   = hodStatus === 'PENDING' || hodStatus === '';
    var hodRejected  = hodStatus === 'REJECTED';

    $('#reject-reason-row').toggle(isPending);
    $('#btnRejectLeave').toggle(isPending);
    // HR can approve even if HOD hasn't acted; only block if HOD explicitly rejected
    // (if HOD rejects, the overall status should already be REJECTED, so isPending=false anyway)
    $('#btnApproveLeave').toggle(isPending).prop('disabled', false);

    if (isPending && hodPending) {
        $('#btnApproveLeave').before(
            '<div id="hod-pending-warning" class="alert alert-info m-b-10" style="display:inline-block;padding:4px 10px;margin-right:8px;">' +
            '<i class="fa fa-info-circle"></i> HOD has not reviewed yet — HR can still approve</div>'
        );
    }

    $('#view-leave-modal').appendTo('body').modal('show');
});

// ── Approve ──────────────────────────────────────────────────────────────────
$('#btnApproveLeave').on('click', function (e) {
    e.preventDefault();
    var id = parseInt($('#view_leave_id').val());
    Swal.fire({
        title: 'Approve Leave?',
        text: 'This will approve the leave request.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        confirmButtonText: 'Yes, Approve',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Leave/Approve', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ request_id: id }),
            success: function (res) {
                if (res.success) {
                    $('#view-leave-modal').modal('hide');
                    LoadLeave('PENDING');
                    Swal.fire('Approved!', res.message, 'success');
                } else {
                    $('#leave-action-error').text(res.message).show();
                }
            },
            error: function () { $('#leave-action-error').text('Request failed.').show(); }
        });
    });
});

// ── Reject ───────────────────────────────────────────────────────────────────
$('#btnRejectLeave').on('click', function (e) {
    e.preventDefault();
    var id     = parseInt($('#view_leave_id').val());
    var reason = $('#leave_reject_reason').val().trim();
    if (!reason) { $('#leave-action-error').text('Please provide a rejection reason.').show(); return; }

    Swal.fire({
        title: 'Reject Leave?',
        text: 'This will reject the leave request.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Reject',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Leave/Reject', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ request_id: id, reject_reason: reason }),
            success: function (res) {
                if (res.success) {
                    $('#view-leave-modal').modal('hide');
                    LoadLeave('PENDING');
                    Swal.fire('Rejected', res.message, 'info');
                } else {
                    $('#leave-action-error').text(res.message).show();
                }
            },
            error: function () { $('#leave-action-error').text('Request failed.').show(); }
        });
    });
});
