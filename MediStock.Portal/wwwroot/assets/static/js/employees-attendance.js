// ============================================================
//  RIZIKI — employees-attendance.js  (HR: Attendance view)
// ============================================================
var _attTable;
var _allEmployees = [];
var _attData = [];

$(document).ready(function () {
    App.init();

    _attTable = $('#attTable').dataTable({
        responsive: true,
        order: [[2, 'desc']],
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return row.employee_name || '—';
            }},
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

    // Load departments
    $.get('/Employees/GetDepartments', function (data) {
        var sel = $('#att_dept_id');
        (data || []).forEach(function (d) {
            sel.append('<option value="' + (d.id || d.department_id) + '">' + (d.department_name || d.name) + '</option>');
        });
    });

    // Load employees; keep full list for dept filtering
    $.get('/Employees/GetList', function (data) {
        _allEmployees = data || [];
        PopulateEmployeeDropdown('');
    });

    // Dept change → filter employee dropdown, reload
    $('#att_dept_id').on('change', function () {
        PopulateEmployeeDropdown($(this).val());
    });

    // Default dates: this week (Mon → today)
    var today = new Date();
    var mon = new Date(today);
    mon.setDate(today.getDate() - ((today.getDay() + 6) % 7));
    $('#att_from').val(FmtInputDate(mon));
    $('#att_to').val(FmtInputDate(today));

    $('#btnLoadAtt').on('click', function (e) { e.preventDefault(); LoadAttendance(); });
    $('#btnTodayAtt').on('click', function (e) {
        e.preventDefault();
        var t = new Date();
        $('#att_from').val(FmtInputDate(t));
        $('#att_to').val(FmtInputDate(t));
        LoadAttendance();
    });

    LoadAttendance();
});

function PopulateEmployeeDropdown(deptId) {
    var sel = $('#att_employee_id');
    var current = sel.val();
    sel.empty().append('<option value="">— All Employees —</option>');
    _allEmployees.forEach(function (e) {
        if (deptId && String(e.department_id) !== deptId) return;
        var name = (e.employee_name || ((e.first_name || '') + ' ' + (e.last_name || '')).trim() || e.email);
        sel.append('<option value="' + (e.id || e.employee_id) + '">' + name + '</option>');
    });
    // restore selection if still valid
    if (current && sel.find('option[value="' + current + '"]').length) sel.val(current);
}

function LoadAttendance() {
    var empId  = $('#att_employee_id').val() || '';
    var deptId = $('#att_dept_id').val() || '';
    var from   = $('#att_from').val() || '';
    var to     = $('#att_to').val()   || '';
    var url    = '/Employees/GetAttendance?employee_id=' + empId
               + '&from_date=' + from + '&to_date=' + to;

    $.get(url, function (data) {
        _attData = data || [];
        RenderAttTable(deptId, from, to);
    });
}

function RenderAttTable(deptId, from, to) {
    // If a dept is selected but no specific employee, filter by department_id
    var rows = (deptId && !$('#att_employee_id').val())
        ? _attData.filter(function (r) { return String(r.department_id) === deptId; })
        : _attData;

    var t = _attTable, s = t.fnSettings();
    t.fnClearTable(true);
    for (var i = 0; i < rows.length; i++) t.oApi._fnAddData(s, rows[i]);
    s.aiDisplay = s.aiDisplayMaster.slice();
    t.fnDraw();

    var label = (from || 'All') + (to && to !== from ? ' → ' + to : '');
    $('#att_summary').text(rows.length + ' record(s) — ' + label);
}

function FmtDate(val) {
    if (!val) return '—';
    return val.toString().split('T')[0];
}

function FmtTime(val) {
    if (!val) return '—';
    var s = val.toString();
    if (s.indexOf('T') > -1) return s.split('T')[1].substring(0, 5);
    if (s.indexOf(':') > -1) return s.substring(0, 5);
    return s;
}

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

function FmtInputDate(d) {
    return d.getFullYear() + '-'
         + String(d.getMonth() + 1).padStart(2, '0') + '-'
         + String(d.getDate()).padStart(2, '0');
}
