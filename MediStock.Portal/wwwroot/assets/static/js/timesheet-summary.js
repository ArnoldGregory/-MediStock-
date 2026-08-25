// ============================================================
//  RIZIKI — timesheet-summary.js
//  HR view: per-employee timesheet totals with date filter
// ============================================================
var _summaryTable;

$(document).ready(function () {
    App.init();

    _summaryTable = $('#summaryTable').dataTable({
        responsive: true,
        order: [[4, 'desc']],
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; } },
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) { return r.employee_name || '—'; } },
            { data: null, sDefaultContent: '0', mRender: function (d, t, r) { return r.entry_count || 0; } },
            { data: null, sDefaultContent: '0', mRender: function (d, t, r) { return r.days_active || 0; } },
            { data: null, sDefaultContent: '0m', mRender: function (d, t, r) {
                var mins = r.total_minutes || 0;
                if (mins <= 0) return '<span class="text-muted">0m</span>';
                var h = Math.floor(mins / 60), m = mins % 60;
                return h > 0 ? h + 'h ' + m + 'm' : m + 'm';
            }}
        ]
    });

    // Default date range: current month
    var now   = new Date();
    var first = new Date(now.getFullYear(), now.getMonth(), 1);
    var last  = new Date(now.getFullYear(), now.getMonth() + 1, 0);

    $('#txtFrom').val(formatDate(first));
    $('#txtTo').val(formatDate(last));

    LoadSummary();
});

function LoadSummary() {
    var from = $('#txtFrom').val();
    var to   = $('#txtTo').val();

    _summaryTable.fnClearTable();

    var url = '/Timesheet/GetSummary';
    var params = [];
    if (from) params.push('from=' + from);
    if (to)   params.push('to='   + to);
    if (params.length) url += '?' + params.join('&');

    $.get(url, function (data) {
        if (data && data.length) {
            _summaryTable.fnAddData(data);
        }
    });
}

function formatDate(d) {
    var mm = ('0' + (d.getMonth() + 1)).slice(-2);
    var dd = ('0' + d.getDate()).slice(-2);
    return d.getFullYear() + '-' + mm + '-' + dd;
}
