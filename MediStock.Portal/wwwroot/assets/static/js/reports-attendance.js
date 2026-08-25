// ============================================================
//  RIZIKI — reports-attendance.js  (HR: Attendance Report)
// ============================================================
var _rattTable;

$(document).ready(function () {
    App.init();

    _rattTable = $('#rattTable').dataTable({
        responsive: true,
        order: [[2, 'desc']],
        aoColumns: [
            { data: null, mRender: function (d, t, r, meta) { return meta.row + 1; }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return row.employee_name || row.full_name ||
                       ((row.first_name || '') + ' ' + (row.last_name || '')).trim() || '—';
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtDate(row.attendance_date || row.work_date || row.date || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtTime(row.clock_in || row.clock_in_time || row.time_in || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtTime(row.clock_out || row.clock_out_time || row.time_out || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return row.duration || row.hours_worked || row.total_hours || '—';
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                var s = row.status || row.attendance_status || '';
                if (!s) return '<span class="label label-default">—</span>';
                var cls = s.toLowerCase() === 'present' ? 'success' : 'warning';
                return '<span class="label label-' + cls + '">' + s.toUpperCase() + '</span>';
            }}
        ]
    });

    // Employee dropdown
    $.get('/Reports/GetEmployees', function (data) {
        var sel = $('#ratt_employee_id');
        (data || []).forEach(function (e) {
            var name = ((e.first_name || '') + ' ' + (e.last_name || '')).trim() || e.email;
            sel.append('<option value="' + (e.id || e.employee_id) + '">' + name + '</option>');
        });
    });

    // Default: this week
    var today = new Date();
    var mon = new Date(today);
    mon.setDate(today.getDate() - ((today.getDay() + 6) % 7));
    $('#ratt_from').val(FmtInputDate(mon));
    $('#ratt_to').val(FmtInputDate(today));

    $('#btnLoadRatt').on('click', function (e) { e.preventDefault(); LoadRatt(); });
    $('#btnRattToday').on('click', function (e) {
        e.preventDefault();
        var t = new Date();
        $('#ratt_from').val(FmtInputDate(t));
        $('#ratt_to').val(FmtInputDate(t));
        LoadRatt();
    });
    $('#btnRattCsv').on('click', function (e) { e.preventDefault(); ExportCsv(); });

    LoadRatt();
});

function LoadRatt() {
    var empId = $('#ratt_employee_id').val() || 0;
    var from  = $('#ratt_from').val() || '';
    var to    = $('#ratt_to').val()   || '';
    var url   = '/Reports/GetAttendanceReport?employee_id=' + empId
              + '&from_date=' + from + '&to_date=' + to;

    $.get(url, function (data) {
        var t = _rattTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
        var label = (from || 'All') + (to && to !== from ? ' → ' + to : '');
        $('#ratt_summary').text((data ? data.length : 0) + ' record(s) — ' + label);
    });
}

function ExportCsv() {
    var rows = [['Employee','Date','Clock In','Clock Out','Duration','Status']];
    _rattTable.fnSettings().aoData.forEach(function (d) {
        var r = d._aData;
        rows.push([
            r.employee_name || r.full_name || ((r.first_name||'') + ' ' + (r.last_name||'')).trim(),
            FmtDate(r.attendance_date || r.work_date || r.date || ''),
            FmtTime(r.clock_in || r.clock_in_time || r.time_in || ''),
            FmtTime(r.clock_out || r.clock_out_time || r.time_out || ''),
            r.duration || r.hours_worked || '',
            r.status || r.attendance_status || ''
        ]);
    });
    var csv = rows.map(function (r) { return r.map(function (c) { return '"' + (c || '').toString().replace(/"/g, '""') + '"'; }).join(','); }).join('\r\n');
    var a = document.createElement('a');
    a.href = 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv);
    a.download = 'attendance_' + $('#ratt_from').val() + '_' + $('#ratt_to').val() + '.csv';
    a.click();
}

function FmtDate(val) { if (!val) return '—'; return val.toString().split('T')[0]; }
function FmtTime(val) {
    if (!val) return '—';
    var s = val.toString();
    if (s.indexOf('T') > -1) return s.split('T')[1].substring(0, 5);
    if (s.indexOf(':') > -1) return s.substring(0, 5);
    return s;
}
function FmtInputDate(d) {
    return d.getFullYear() + '-' + String(d.getMonth()+1).padStart(2,'0') + '-' + String(d.getDate()).padStart(2,'0');
}
