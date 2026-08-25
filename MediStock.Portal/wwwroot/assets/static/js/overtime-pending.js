// ============================================================
//  RIZIKI — overtime-pending.js  (HR: Overtime Approvals)
// ============================================================
var _otTable;

$(document).ready(function () {
    App.init();
    _otTable = InitOtTable();
    LoadOvertime('PENDING');

    $('#view-ot-modal').on('hidden.bs.modal', function () {
        $('#ot_reject_reason').val('');
        $('#ot-action-error').hide();
    });
});

function FmtDate(v) { return v ? v.toString().split('T')[0] : '—'; }
function FmtTime(v) {
    if (!v) return '—';
    var s = v.toString();
    if (s.indexOf('T') > -1) return s.split('T')[1].substring(0, 5);
    return s.substring(0, 5);
}
function Fmt(n) { return parseFloat(n || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 }); }

function InitOtTable() {
    return $('#otTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.id); },
        aoColumns: [
            { data: 'employee_name',  autoWidth: true, sDefaultContent: '—' },
            { data: 'department_name',autoWidth: true, sDefaultContent: '—' },
            { data: 'overtime_date',  autoWidth: true, sDefaultContent: '—', mRender: function (d) { return FmtDate(d); } },
            { data: 'start_time',     autoWidth: true, sDefaultContent: '—', mRender: function (d) { return FmtTime(d); } },
            { data: 'end_time',       autoWidth: true, sDefaultContent: '—', mRender: function (d) { return FmtTime(d); } },
            { data: 'hours_worked',   autoWidth: true, sDefaultContent: '0' },
            { data: 'hourly_rate',    autoWidth: true, sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'total_amount',   autoWidth: true, sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'reason',         autoWidth: true, sDefaultContent: '—' },
            { data: 'status', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) {
                  var cls = d === 'APPROVED' ? 'success' : d === 'REJECTED' ? 'danger' : 'warning';
                  return '<span class="label label-' + cls + '">' + (d || '—') + '</span>';
              }},
            { bSortable: false, sDefaultContent:
              '<a href="#" class="btn btn-info btn-xs view-ot"><i class="fa fa-eye"></i> View</a>' }
        ]
    });
}

function LoadOvertime(status) {
    status = status === undefined ? 'PENDING' : status;
    $('#btnFilterPending, #btnFilterAll').removeClass('active');
    if (status === 'PENDING') $('#btnFilterPending').addClass('active');
    else $('#btnFilterAll').addClass('active');

    $.get('/Overtime/GetAll', { status: status }, function (data) {
        var t = _otTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── View click ────────────────────────────────────────────────────────────────
$('#otTable').on('click', 'a.view-ot', function (e) {
    e.preventDefault();
    var d = _otTable.fnGetData($(this).parents('tr')[0]);

    $('#view_ot_id').val(d.id);
    $('#view_ot_emp').val(d.employee_name || '—');
    $('#view_ot_dept').val(d.department_name || '—');
    $('#view_ot_date').val(FmtDate(d.overtime_date));
    $('#view_ot_start').val(FmtTime(d.start_time));
    $('#view_ot_end').val(FmtTime(d.end_time));
    $('#view_ot_hours').val(d.hours_worked || '0');
    $('#view_ot_rate').val(Fmt(d.hourly_rate));
    $('#view_ot_amount').val(Fmt(d.total_amount));
    $('#view_ot_status').val(d.status || '—');
    $('#view_ot_reason').val(d.reason || '—');
    $('#ot_reject_reason').val('');
    $('#ot-action-error').hide();

    var isPending = (d.status || '').toUpperCase() === 'PENDING';
    $('#ot-reject-row').toggle(isPending);
    $('#btnRejectOt').toggle(isPending);
    $('#btnApproveOt').toggle(isPending);

    $('#view-ot-modal').appendTo('body').modal('show');
});

// ── Approve ───────────────────────────────────────────────────────────────────
$('#btnApproveOt').on('click', function (e) {
    e.preventDefault();
    var id = parseInt($('#view_ot_id').val());
    Swal.fire({
        title: 'Approve Overtime?', text: 'This will approve the overtime claim.',
        icon: 'question', showCancelButton: true,
        confirmButtonColor: '#28a745', confirmButtonText: 'Yes, Approve', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Overtime/Approve', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ request_id: id }),
            success: function (res) {
                if (res.success) {
                    $('#view-ot-modal').modal('hide');
                    LoadOvertime('PENDING');
                    Swal.fire('Approved!', res.message, 'success');
                } else { $('#ot-action-error').text(res.message).show(); }
            },
            error: function () { $('#ot-action-error').text('Request failed.').show(); }
        });
    });
});

// ── Reject ────────────────────────────────────────────────────────────────────
$('#btnRejectOt').on('click', function (e) {
    e.preventDefault();
    var id     = parseInt($('#view_ot_id').val());
    var reason = $('#ot_reject_reason').val().trim();
    if (!reason) { $('#ot-action-error').text('Please provide a rejection reason.').show(); return; }

    Swal.fire({
        title: 'Reject Overtime?', text: 'This will reject the overtime claim.',
        icon: 'warning', showCancelButton: true,
        confirmButtonColor: '#d33', confirmButtonText: 'Yes, Reject', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Overtime/Reject', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ request_id: id, reject_reason: reason }),
            success: function (res) {
                if (res.success) {
                    $('#view-ot-modal').modal('hide');
                    LoadOvertime('PENDING');
                    Swal.fire('Rejected', res.message, 'info');
                } else { $('#ot-action-error').text(res.message).show(); }
            },
            error: function () { $('#ot-action-error').text('Request failed.').show(); }
        });
    });
});
