// ============================================================
//  RIZIKI — advances-pending.js  (HR Advances Approvals)
// ============================================================

var _advTable;

$(document).ready(function () {
    App.init();
    _advTable = InitAdvTable();
    LoadAdvances('PENDING');

    $('#view-advance-modal').on('hidden.bs.modal', function () {
        $('#adv_reject_reason').val('');
        $('#adv-action-error').hide();
    });
});

function InitAdvTable() {
    return $('#advancesTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.advance_id || data.id); },
        aoColumns: [
            { data: null, autoWidth: true, sDefaultContent: '',
              mRender: function (d) { return (d.first_name || '') + ' ' + (d.last_name || ''); } },
            { data: 'amount', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) { return d ? parseFloat(d).toLocaleString('en-KE', { minimumFractionDigits: 2 }) : '—'; } },
            { data: 'reason',           autoWidth: true, sDefaultContent: '—' },
            { data: 'repayment_months', autoWidth: true, sDefaultContent: '—' },
            { data: 'status', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) {
                  var cls = d === 'APPROVED' ? 'success' : d === 'REJECTED' ? 'danger' : 'warning';
                  return '<span class="label label-' + cls + '">' + (d || '—') + '</span>';
              }},
            { data: 'created_on', autoWidth: true, sDefaultContent: '—' },
            { bSortable: false, sDefaultContent:
              '<a href="#" class="btn btn-info btn-xs view-advance"><i class="fa fa-eye"></i> View</a>' }
        ]
    });
}

function LoadAdvances(status) {
    status = status === undefined ? 'PENDING' : status;

    $('#btnAdvFilterPending, #btnAdvFilterAll').removeClass('active');
    if (status === 'PENDING') $('#btnAdvFilterPending').addClass('active');
    else $('#btnAdvFilterAll').addClass('active');

    $.get('/Advances/GetAll', { status: status }, function (data) {
        var t = _advTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── View click ───────────────────────────────────────────────────────────────
$('#advancesTable').on('click', 'a.view-advance', function (e) {
    e.preventDefault();
    var d = _advTable.fnGetData($(this).parents('tr')[0]);

    $('#view_adv_id').val(d.advance_id || d.id);
    $('#view_adv_emp').val((d.first_name || '') + ' ' + (d.last_name || ''));
    $('#view_adv_amount').val(d.amount ? parseFloat(d.amount).toLocaleString('en-KE', { minimumFractionDigits: 2 }) : '—');
    $('#view_adv_months').val(d.repayment_months || '—');
    $('#view_adv_reason').val(d.reason || '—');
    $('#view_adv_status').val(d.status || '—');
    $('#view_adv_date').val(d.created_on || '—');
    $('#adv_reject_reason').val('');
    $('#adv-action-error').hide();

    var isPending = (d.status || '').toUpperCase() === 'PENDING';
    $('#adv-reject-reason-row').toggle(isPending);
    $('#btnApproveAdvance, #btnRejectAdvance').toggle(isPending);

    $('#view-advance-modal').appendTo('body').modal('show');
});

// ── Approve ──────────────────────────────────────────────────────────────────
$('#btnApproveAdvance').on('click', function (e) {
    e.preventDefault();
    var id = parseInt($('#view_adv_id').val());
    Swal.fire({
        title: 'Approve Advance?',
        text: 'This will approve the salary advance request.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        confirmButtonText: 'Yes, Approve',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Advances/Approve', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ request_id: id }),
            success: function (res) {
                if (res.success) {
                    $('#view-advance-modal').modal('hide');
                    LoadAdvances('PENDING');
                    Swal.fire('Approved!', res.message, 'success');
                } else {
                    $('#adv-action-error').text(res.message).show();
                }
            },
            error: function () { $('#adv-action-error').text('Request failed.').show(); }
        });
    });
});

// ── Reject ───────────────────────────────────────────────────────────────────
$('#btnRejectAdvance').on('click', function (e) {
    e.preventDefault();
    var id     = parseInt($('#view_adv_id').val());
    var reason = $('#adv_reject_reason').val().trim();
    if (!reason) { $('#adv-action-error').text('Please provide a rejection reason.').show(); return; }

    Swal.fire({
        title: 'Reject Advance?',
        text: 'This will reject the salary advance request.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Reject',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Advances/Reject', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ request_id: id, reject_reason: reason }),
            success: function (res) {
                if (res.success) {
                    $('#view-advance-modal').modal('hide');
                    LoadAdvances('PENDING');
                    Swal.fire('Rejected', res.message, 'info');
                } else {
                    $('#adv-action-error').text(res.message).show();
                }
            },
            error: function () { $('#adv-action-error').text('Request failed.').show(); }
        });
    });
});
