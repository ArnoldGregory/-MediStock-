// ============================================================
//  RIZIKI — ss-attendance.js  (Employee: Attendance)
// ============================================================
var _attTable;

$(document).ready(function () {
    App.init();
    UpdateClock();
    setInterval(UpdateClock, 1000);

    _attTable = $('#attendanceTable').dataTable({
        responsive: true,
        order: [[0, 'desc']],
        aoColumns: [
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtDate(row.attendance_date || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtTime(row.check_in_time || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtTime(row.check_out_time || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return CalcDuration(row.check_in_time, row.check_out_time);
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                var present = parseInt(row.is_present);
                if (isNaN(present)) return '<span class="label label-default">—</span>';
                return present === 1
                    ? '<span class="label label-success">PRESENT</span>'
                    : '<span class="label label-warning">ABSENT</span>';
            }}
        ]
    });
    LoadAttendance();
});

function UpdateClock() {
    var now = new Date();
    $('#currentTime').text(now.toLocaleTimeString('en-KE'));
    $('#todayDate').text(now.toLocaleDateString('en-KE', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }));
}

function LoadAttendance() {
    $.get('/SelfService/GetMyAttendance', function (data) {
        var t = _attTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// Strip T00:00:00 from date strings
function FmtDate(val) {
    if (!val) return '—';
    return val.toString().split('T')[0];
}

// Show only HH:MM from a datetime or time string
function FmtTime(val) {
    if (!val) return '—';
    var s = val.toString();
    if (s.indexOf('T') > -1) return s.split('T')[1].substring(0, 5);
    if (s.indexOf(':') > -1) return s.substring(0, 5);
    return s;
}

// Calculate duration between two time/datetime strings → "Xh Ym"
function CalcDuration(inVal, outVal) {
    if (!inVal || !outVal) return '—';
    try {
        var parseTime = function (v) {
            var s = v.toString();
            var t = s.indexOf('T') > -1 ? s.split('T')[1] : s;
            var parts = t.split(':');
            return parseInt(parts[0]) * 60 + parseInt(parts[1]);
        };
        var diff = parseTime(outVal) - parseTime(inVal);
        if (diff <= 0) return '—';
        return Math.floor(diff / 60) + 'h ' + (diff % 60) + 'm';
    } catch (e) { return '—'; }
}

$('#btnClockIn').on('click', function (e) {
    e.preventDefault();
    Swal.fire({
        title: 'Clock In?', text: 'Record your clock-in time now?',
        icon: 'question', showCancelButton: true,
        confirmButtonColor: '#28a745', confirmButtonText: 'Yes, Clock In', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.post('/SelfService/ClockIn', function (res) {
            if (res.success) {
                $('#clockInTime').text(new Date().toLocaleTimeString('en-KE'));
                LoadAttendance();
                Swal.fire('Clocked In!', res.message, 'success');
            } else { Swal.fire('Error', res.message, 'error'); }
        }).fail(function () { Swal.fire('Error', 'Request failed.', 'error'); });
    });
});

$('#btnClockOut').on('click', function (e) {
    e.preventDefault();
    Swal.fire({
        title: 'Clock Out?', text: 'Record your clock-out time now?',
        icon: 'question', showCancelButton: true,
        confirmButtonColor: '#d33', confirmButtonText: 'Yes, Clock Out', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.post('/SelfService/ClockOut', function (res) {
            if (res.success) {
                $('#clockOutTime').text(new Date().toLocaleTimeString('en-KE'));
                LoadAttendance();
                Swal.fire('Clocked Out!', res.message, 'success');
            } else { Swal.fire('Error', res.message, 'error'); }
        }).fail(function () { Swal.fire('Error', 'Request failed.', 'error'); });
    });
});
