// ============================================================
//  RIZIKI — reports-leave.js  (Leave Report)
// ============================================================
var _leaveRptTable;

$(document).ready(function () {
    App.init();

    _leaveRptTable = $('#leaveRptTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return row.employee_name || row.full_name ||
                       ((row.first_name || '') + ' ' + (row.last_name || '')).trim() || '—';
            }},
            { data: 'leave_type_name', sDefaultContent: '—' },
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtDate(row.start_date || row.from_date || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtDate(row.end_date || row.to_date || '');
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                var days = row.days_requested || row.days_taken || row.num_days || row.days;
                if (days !== undefined && days !== null && days !== '') return days;
                var s = row.start_date || row.from_date;
                var e = row.end_date   || row.to_date;
                if (s && e) return Math.max(0, Math.round((new Date(e) - new Date(s)) / 86400000) + 1);
                return '—';
            }},
            { data: 'status', sDefaultContent: '—', mRender: function (d) {
                var cls = d === 'APPROVED' ? 'success' : d === 'REJECTED' ? 'danger' : 'warning';
                return '<span class="label label-' + cls + '">' + (d || '—') + '</span>';
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return FmtDate(row.created_on || row.applied_on || '');
            }},
            { data: 'reason', sDefaultContent: '—' }
        ]
    });

    // Load employees into filter dropdown
    $.get('/Reports/GetEmployees', function (data) {
        var sel = $('#rpt_employee_id');
        (data || []).forEach(function (e) {
            var name = ((e.first_name || '') + ' ' + (e.last_name || '')).trim() || e.email;
            sel.append('<option value="' + (e.id || e.employee_id) + '">' + name + '</option>');
        });
    });

    $('#btnLoadLeave').on('click', function (e) {
        e.preventDefault();
        LoadLeaveReport();
    });

    $('#btnExportCsv').on('click', function (e) {
        e.preventDefault();
        ExportCsv();
    });
});

function LoadLeaveReport() {
    var status     = $('#rpt_status').val();
    var employeeId = $('#rpt_employee_id').val();
    var url = '/Reports/GetLeaveReport?';
    if (status)     url += 'status=' + status + '&';
    if (employeeId) url += 'employee_id=' + employeeId + '&';

    $.get(url, function (data) {
        var t = _leaveRptTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();

        var label = status ? status : 'All';
        $('#rpt_summary').text((data ? data.length : 0) + ' record(s) — ' + label);
        $('#results-panel').show();
    });
}

function ExportCsv() {
    var settings = _leaveRptTable.fnSettings();
    var rows = settings.aoData;
    if (!rows || !rows.length) { Swal.fire('No Data', 'Load the report first.', 'info'); return; }

    var headers = ['Employee','Leave Type','From','To','Days','Status','Applied On','Reason'];
    var lines = [headers.join(',')];
    rows.forEach(function (r) {
        var d = r._aData;
        var emp  = d.employee_name || d.full_name || ((d.first_name || '') + ' ' + (d.last_name || '')).trim();
        var from = FmtDate(d.start_date || d.from_date || '');
        var to   = FmtDate(d.end_date   || d.to_date   || '');
        var days = d.days_requested || d.days_taken || d.num_days || d.days || '';
        lines.push([
            '"' + emp + '"',
            '"' + (d.leave_type_name || '') + '"',
            from, to, days,
            d.status || '',
            FmtDate(d.created_on || d.applied_on || ''),
            '"' + (d.reason || '').replace(/"/g, "'") + '"'
        ].join(','));
    });

    var blob = new Blob([lines.join('\n')], { type: 'text/csv' });
    var a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'leave_report.csv';
    a.click();
}

function FmtDate(val) {
    if (!val) return '—';
    return val.toString().split('T')[0];
}
