// ============================================================
//  RIZIKI — ss-myleave.js  (Employee: My Leave Requests)
// ============================================================
var _leaveTable;

$(document).ready(function () {
    App.init();
    _leaveTable = InitLeaveTable();
    LoadMyLeave();
    LoadLeaveTypes();

    // Half-day toggle
    $('input[name="apply_half_day"]').on('change', function () {
        var isHalf = $(this).val() === 'yes';
        $('#apply_half_day_period_row').toggle(isHalf);
        $('#apply_end_date_col').toggle(!isHalf);
        if (isHalf) {
            var start = $('#apply_start_date').val();
            if (start) $('#apply_end_date').val(start);
        }
    });

    // When start date changes on half-day mode, sync end date
    $('#apply_start_date').on('change', function () {
        if ($('#apply_half_day_yes').is(':checked')) {
            $('#apply_end_date').val($(this).val());
        }
    });

    $('#apply-leave-modal').on('hidden.bs.modal', function () {
        $('#apply_leave_type').val('');
        $('#apply_start_date, #apply_end_date, #apply_reason').val('');
        $('#apply-leave-error').hide();
        $('input[name="apply_half_day"][value="no"]').prop('checked', true);
        $('input[name="apply_half_period"][value="AM"]').prop('checked', true);
        $('#apply_half_day_period_row').hide();
        $('#apply_end_date_col').show();
    });

    // Cancel (Recall) leave
    $(document).on('click', '.btn-recall-leave', function () {
        var reqId = $(this).data('id');
        Swal.fire({
            title: 'Cancel Leave Request?',
            text: 'This will cancel the request and restore your leave balance.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, cancel it',
            cancelButtonText: 'No'
        }).then(function (result) {
            if (!result.value) return;
            $.ajax({
                url: '/SelfService/RecallLeave', type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ request_id: reqId }),
                success: function (res) {
                    if (res.success) {
                        LoadMyLeave();
                        Swal.fire('Cancelled', res.message, 'success');
                    } else {
                        Swal.fire('Error', res.message, 'error');
                    }
                },
                error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
            });
        });
    });
});

function InitLeaveTable() {
    return $('#myLeaveTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: 'leave_type_name',  sDefaultContent: '—' },
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtDate(row.start_date || row.from_date || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtDate(row.end_date || row.to_date || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                var n = row.days_requested || row.days_taken || row.num_days || row.days || row.number_of_days;
                if (n !== undefined && n !== null && n !== '') return n;
                var s = row.start_date || row.from_date;
                var e = row.end_date   || row.to_date;
                if (s && e) {
                    var diff = (new Date(e) - new Date(s)) / 86400000;
                    return Math.max(0, Math.round(diff) + 1);
                }
                return '—';
            }},
            { data: 'reason',           sDefaultContent: '—' },
            { data: 'status', sDefaultContent: '—',
              mRender: function (d) {
                  var cls = d === 'APPROVED'  ? 'success'
                          : d === 'REJECTED'  ? 'danger'
                          : d === 'RECALLED'  ? 'default'
                          : 'warning';
                  return '<span class="label label-' + cls + '">' + (d || '—') + '</span>';
              }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtDate(row.created_on || row.applied_on || '');
            }},
            { data: 'reject_reason',    sDefaultContent: '—' },
            { data: null, sDefaultContent: '', mRender: function (d, t, row) {
                if (row.status === 'PENDING') {
                    return '<button class="btn btn-xs btn-danger btn-recall-leave" data-id="' + row.id + '">' +
                           '<i class="fa fa-times"></i> Cancel</button>';
                }
                return '';
            }}
        ]
    });
}

function FmtDate(val) {
    if (!val) return '—';
    return val.toString().split('T')[0];
}

function LoadMyLeave() {
    $.get('/SelfService/GetMyLeave', function (data) {
        var t = _leaveTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

function LoadLeaveTypes() {
    $.get('/SelfService/GetLeaveTypes', function (data) {
        var sel = $('#apply_leave_type');
        sel.find('option:not(:first)').remove();
        if (data && data.length) {
            $.each(data, function (i, t) {
                sel.append($('<option>', { value: t.id || t.leave_type_id, text: t.leave_type_name + ' (' + (t.annual_days || 0) + ' days/yr)' }));
            });
        }
    });
}

$('#btnSubmitLeave').on('click', function (e) {
    e.preventDefault();
    var typeId  = $('#apply_leave_type').val();
    var start   = $('#apply_start_date').val();
    var isHalf  = $('#apply_half_day_yes').is(':checked');
    var period  = $('input[name="apply_half_period"]:checked').val();
    var end     = isHalf ? start : $('#apply_end_date').val();

    if (!typeId) { $('#apply-leave-error').text('Please select a leave type.').show(); return; }
    if (!start)  { $('#apply-leave-error').text('Start date is required.').show(); return; }
    if (!isHalf && !end) { $('#apply-leave-error').text('End date is required.').show(); return; }
    if (!isHalf && end < start) { $('#apply-leave-error').text('End date cannot be before start date.').show(); return; }

    $.ajax({
        url: '/SelfService/ApplyLeave', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            leave_type_id:   parseInt(typeId),
            start_date:      start,
            end_date:        end,
            reason:          $('#apply_reason').val().trim(),
            is_half_day:     isHalf,
            half_day_period: isHalf ? period : null
        }),
        success: function (res) {
            if (res.success) {
                $('#apply-leave-modal').modal('hide');
                LoadMyLeave();
                Swal.fire('Submitted!', res.message, 'success');
            } else {
                $('#apply-leave-error').text(res.message).show();
            }
        },
        error: function () { $('#apply-leave-error').text('Request failed.').show(); }
    });
});
