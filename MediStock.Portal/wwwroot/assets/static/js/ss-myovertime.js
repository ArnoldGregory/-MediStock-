// ============================================================
//  RIZIKI — ss-myovertime.js  (Employee: My Overtime)
// ============================================================
var _myOtTable;

$(document).ready(function () {
    App.init();

    _myOtTable = $('#myOtTable').dataTable({
        responsive: true,
        order: [[8, 'desc']],
        createdRow: function (row, data) { $(row).attr('recid', data.id); },
        aoColumns: [
            { data: 'overtime_date', sDefaultContent: '—', mRender: function (d) { return FmtDate(d); } },
            { data: 'start_time',    sDefaultContent: '—', mRender: function (d) { return FmtTime(d); } },
            { data: 'end_time',      sDefaultContent: '—', mRender: function (d) { return FmtTime(d); } },
            { data: 'hours_worked',  sDefaultContent: '0' },
            { data: 'hourly_rate',   sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'total_amount',  sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'reason',        sDefaultContent: '—' },
            { data: 'status', sDefaultContent: '—',
              mRender: function (d) {
                  var cls = d === 'APPROVED' ? 'success' : d === 'REJECTED' ? 'danger' : 'warning';
                  return '<span class="label label-' + cls + '">' + (d || 'PENDING') + '</span>';
              }},
            { data: 'created_on', sDefaultContent: '—', mRender: function (d) { return FmtDate(d); } }
        ]
    });

    LoadMyOvertime();

    $('#submit-ot-modal').on('hidden.bs.modal', function () {
        $('#ot_date, #ot_start, #ot_end, #ot_rate, #ot_reason').val('');
        $('#ot-submit-error').hide();
    });
});

function LoadMyOvertime() {
    $.get('/SelfService/GetMyOvertime', function (data) {
        var t = _myOtTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

$('#btnSubmitOt').on('click', function (e) {
    e.preventDefault();
    var date  = $('#ot_date').val();
    var start = $('#ot_start').val();
    var end   = $('#ot_end').val();
    var rate  = parseFloat($('#ot_rate').val() || '0');
    var reason= $('#ot_reason').val().trim();

    if (!date)  { $('#ot-submit-error').text('Date is required.').show(); return; }
    if (!start) { $('#ot-submit-error').text('Start time is required.').show(); return; }
    if (!end)   { $('#ot-submit-error').text('End time is required.').show(); return; }
    if (end <= start) { $('#ot-submit-error').text('End time must be after start time.').show(); return; }
    if (!rate || rate <= 0) { $('#ot-submit-error').text('Hourly rate is required.').show(); return; }
    $('#ot-submit-error').hide();

    $.ajax({
        url: '/SelfService/SubmitOvertime', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ overtime_date: date, start_time: start, end_time: end, hourly_rate: rate, reason: reason }),
        success: function (res) {
            if (res.success) {
                $('#submit-ot-modal').modal('hide');
                LoadMyOvertime();
                Swal.fire('Submitted!', res.message, 'success');
            } else {
                $('#ot-submit-error').text(res.message).show();
            }
        },
        error: function () { $('#ot-submit-error').text('Request failed.').show(); }
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
