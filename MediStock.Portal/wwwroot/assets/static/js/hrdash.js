// ============================================================
//  RIZIKI — hrdash.js  (HR / Admin Dashboard)
//  Stat cards: /Dashboard/HrStats  (fast, reliable path)
//  Charts:     /Dashboard/HrAnalytics → api/dashboard/hrstats
// ============================================================

var _chartPayrollTrend     = null;
var _chartPayrollBreakdown = null;
var _chartDept             = null;
var _chartLeaveType        = null;
var _chartLeaveStatus      = null;
var _chartLeaveMonthly     = null;
var _chartAdvanceStatus    = null;

var _kes = function (v, decimals) {
    decimals = decimals === undefined ? 0 : decimals;
    return 'KES ' + Number(v).toLocaleString('en-KE', {
        minimumFractionDigits: decimals, maximumFractionDigits: decimals
    });
};

$(document).ready(function () {
    LoadHrDashboard();
});

function LoadHrDashboard() {
    // Stat cards from HrStats (direct payroll lookup, always reliable)
    $.getJSON('/Dashboard/HrStats').done(function (s) {
        if (!s) return;
        $('#statEmployees').text(s.employees !== undefined ? s.employees : '—');
        $('#statLeave').text(s.leave !== undefined ? s.leave : '—');
        $('#statAdvances').text(s.advances !== undefined ? s.advances : '—');
        $('#statPayroll').text(s.lastNet || '—');
    }).fail(function () {
        $('#statEmployees, #statLeave, #statAdvances, #statPayroll').text('—');
    });

    // Charts from HrAnalytics
    $.getJSON('/Dashboard/HrAnalytics').done(function (d) {
        if (!d) { ShowChartError(); return; }
        RenderPayrollTrend(d.payroll_trend || []);
        RenderPayrollBreakdown(d.payroll_breakdown || []);
        RenderDeptHeadcount(d.dept_headcount || []);
        RenderLeaveType(d.leave_type_summary || []);
        RenderLeaveStatus(d.leave_breakdown || []);
        RenderLeaveMonthly(d.leave_monthly || []);
        RenderAdvanceStatus(d.advance_status || []);
    }).fail(function () {
        ShowChartError();
    });
}

// ── Chart 1: Payroll cost trend (area) ─────────────────────────
function RenderPayrollTrend(rows) {
    rows = rows.slice().reverse(); // API returns DESC; chart reads L→R

    var labels   = rows.map(function (r) { return r.month_label || ''; });
    var gross    = rows.map(function (r) { return parseFloat(r.total_gross) || 0; });
    var net      = rows.map(function (r) { return parseFloat(r.total_net)   || 0; });
    var paye     = rows.map(function (r) { return parseFloat(r.total_paye)  || 0; });

    if (_chartPayrollTrend) _chartPayrollTrend.destroy();

    _chartPayrollTrend = new ApexCharts(document.querySelector('#chartPayrollTrend'), {
        chart:  { type: 'area', height: 270, toolbar: { show: false }, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: [
            { name: 'Gross Pay', data: gross },
            { name: 'Net Pay',   data: net   },
            { name: 'PAYE',      data: paye  }
        ],
        colors:     ['#348fe2', '#00acac', '#f59c1a'],
        stroke:     { curve: 'smooth', width: 2 },
        fill:       { type: 'gradient', gradient: { opacityFrom: 0.3, opacityTo: 0.02 } },
        xaxis:      { categories: labels, labels: { style: { colors: '#b6c2cf' } } },
        yaxis:      {
            labels: {
                style: { colors: '#b6c2cf' },
                formatter: function (v) { return 'KES ' + (v / 1000000).toFixed(1) + 'M'; }
            }
        },
        dataLabels: { enabled: false },
        grid:       { borderColor: '#3d4a56' },
        legend:     { labels: { colors: '#b6c2cf' } },
        tooltip:    {
            theme: 'dark',
            y: { formatter: function (v) { return _kes(v, 2); } }
        },
        noData: { text: 'No payroll data', style: { color: '#b6c2cf' } }
    });
    _chartPayrollTrend.render();
}

// ── Chart 2: Latest payroll breakdown (donut) ───────────────────
function RenderPayrollBreakdown(rows) {
    if (_chartPayrollBreakdown) _chartPayrollBreakdown.destroy();

    if (!rows || rows.length === 0) {
        $('#chartPayrollBreakdown').html('<p class="text-muted text-center p-t-40">No payroll data</p>');
        return;
    }

    var r = rows[0];
    var gross = parseFloat(r.total_gross) || 0;
    var paye  = parseFloat(r.total_paye)  || 0;
    var nssf  = parseFloat(r.total_nssf)  || 0;
    var shif  = parseFloat(r.total_shif)  || 0;
    var net   = parseFloat(r.total_net)   || 0;

    // Show deductions breakdown + net
    var series = [net, paye, nssf, shif];
    var labels = ['Net Pay', 'PAYE', 'NSSF', 'SHIF'];
    var colors = ['#00acac', '#ff5b57', '#f59c1a', '#727cb6'];

    _chartPayrollBreakdown = new ApexCharts(document.querySelector('#chartPayrollBreakdown'), {
        chart:  { type: 'donut', height: 270, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: series,
        labels: labels,
        colors: colors,
        legend: { position: 'bottom', labels: { colors: '#b6c2cf' } },
        plotOptions: { pie: { donut: { size: '65%', labels: {
            show: true,
            name:  { show: true, color: '#b6c2cf' },
            value: { show: true, color: '#fff',
                formatter: function (v) { return _kes(parseFloat(v), 0); }
            },
            total: { show: true, label: 'Gross',  color: '#b6c2cf',
                formatter: function () { return _kes(gross, 0); }
            }
        } } } },
        dataLabels: { enabled: false },
        tooltip:  {
            theme: 'dark',
            y: { formatter: function (v) { return _kes(v, 2); } }
        },
        noData: { text: 'No data', style: { color: '#b6c2cf' } }
    });
    _chartPayrollBreakdown.render();
}

// ── Chart 3: Headcount by department (horizontal bar) ───────────
function RenderDeptHeadcount(rows) {
    if (_chartDept) _chartDept.destroy();

    var depts  = rows.map(function (r) { return r.dept || 'Unassigned'; });
    var counts = rows.map(function (r) { return parseInt(r.headcount) || 0; });

    _chartDept = new ApexCharts(document.querySelector('#chartDeptHeadcount'), {
        chart:  { type: 'bar', height: 270, toolbar: { show: false }, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: [{ name: 'Employees', data: counts }],
        plotOptions: { bar: { horizontal: true, borderRadius: 3, barHeight: '55%',
            dataLabels: { position: 'top' }
        } },
        xaxis:  { categories: depts, labels: { style: { colors: '#b6c2cf' } }, tickAmount: 5 },
        yaxis:  { labels: { style: { colors: '#b6c2cf' } } },
        colors: ['#49b6d6'],
        dataLabels: {
            enabled: true, offsetX: 18,
            style: { fontSize: '12px', fontWeight: 600, colors: ['#fff'] }
        },
        grid:   { borderColor: '#3d4a56' },
        tooltip: { theme: 'dark' },
        noData: { text: 'No department data', style: { color: '#b6c2cf' } }
    });
    _chartDept.render();
}

// ── Chart 4: Leave days by type (horizontal bar) ────────────────
function RenderLeaveType(rows) {
    if (_chartLeaveType) _chartLeaveType.destroy();

    var types  = rows.map(function (r) { return r.type_name || 'Unknown'; });
    var days   = rows.map(function (r) { return parseInt(r.total_days) || 0; });
    var reqs   = rows.map(function (r) { return parseInt(r.request_count) || 0; });

    _chartLeaveType = new ApexCharts(document.querySelector('#chartLeaveType'), {
        chart:  { type: 'bar', height: 270, toolbar: { show: false }, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: [
            { name: 'Days Taken', data: days },
            { name: 'Requests',   data: reqs }
        ],
        plotOptions: { bar: { horizontal: true, borderRadius: 3, barHeight: '65%',
            dataLabels: { position: 'top' }
        } },
        xaxis:  { categories: types, labels: { style: { colors: '#b6c2cf' } } },
        yaxis:  { labels: { style: { colors: '#b6c2cf' } } },
        colors: ['#00acac', '#348fe2'],
        dataLabels: {
            enabled: true, offsetX: 6,
            style: { fontSize: '11px', colors: ['#fff'] }
        },
        grid:   { borderColor: '#3d4a56' },
        legend: { labels: { colors: '#b6c2cf' } },
        tooltip: { theme: 'dark' },
        noData: { text: 'No leave type data', style: { color: '#b6c2cf' } }
    });
    _chartLeaveType.render();
}

// ── Chart 5: Leave requests by status (donut) ───────────────────
function RenderLeaveStatus(rows) {
    if (_chartLeaveStatus) _chartLeaveStatus.destroy();

    var labels = rows.map(function (r) { return r.status || 'Unknown'; });
    var counts = rows.map(function (r) { return parseInt(r.cnt) || 0; });

    var statusColors = {
        'APPROVED': '#00acac', 'PENDING': '#f59c1a',
        'REJECTED': '#ff5b57', 'CANCELLED': '#6c757d'
    };
    var colors = labels.map(function (s) {
        return statusColors[s.toUpperCase()] || '#348fe2';
    });

    var total = counts.reduce(function (a, b) { return a + b; }, 0);

    _chartLeaveStatus = new ApexCharts(document.querySelector('#chartLeaveStatus'), {
        chart:  { type: 'donut', height: 270, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: counts,
        labels: labels,
        colors: colors,
        legend: { position: 'bottom', labels: { colors: '#b6c2cf' } },
        plotOptions: { pie: { donut: { size: '65%', labels: {
            show: true,
            name:  { show: true, color: '#b6c2cf' },
            value: { show: true, color: '#fff' },
            total: { show: true, label: 'Total', color: '#b6c2cf',
                formatter: function () { return total; }
            }
        } } } },
        dataLabels: { enabled: true, style: { colors: ['#fff'], fontSize: '11px' } },
        tooltip:  { theme: 'dark' },
        noData:   { text: 'No leave requests', style: { color: '#b6c2cf' } }
    });
    _chartLeaveStatus.render();
}

// ── Chart 6: Leave applications over time (column) ──────────────
function RenderLeaveMonthly(rows) {
    rows = rows.slice().reverse();

    var labels = rows.map(function (r) { return r.month_label || ''; });
    var counts = rows.map(function (r) { return parseInt(r.cnt) || 0; });

    if (_chartLeaveMonthly) _chartLeaveMonthly.destroy();

    _chartLeaveMonthly = new ApexCharts(document.querySelector('#chartLeaveMonthly'), {
        chart:  { type: 'bar', height: 270, toolbar: { show: false }, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: [{ name: 'Leave Applications', data: counts }],
        plotOptions: { bar: { borderRadius: 4, columnWidth: '50%' } },
        xaxis:  { categories: labels, labels: { style: { colors: '#b6c2cf' } } },
        yaxis:  { labels: { style: { colors: '#b6c2cf' } }, tickAmount: 4, min: 0 },
        colors: ['#727cb6'],
        dataLabels: {
            enabled: true,
            style:   { fontSize: '11px', colors: ['#fff'] }
        },
        grid:   { borderColor: '#3d4a56' },
        tooltip: { theme: 'dark' },
        noData: { text: 'No leave data', style: { color: '#b6c2cf' } }
    });
    _chartLeaveMonthly.render();
}

// ── Chart 7: Advance requests by status (donut) ─────────────────
function RenderAdvanceStatus(rows) {
    if (_chartAdvanceStatus) _chartAdvanceStatus.destroy();

    var labels = rows.map(function (r) { return r.status || 'Unknown'; });
    var counts = rows.map(function (r) { return parseInt(r.cnt) || 0; });

    var statusColors = {
        'APPROVED': '#00acac', 'PENDING': '#f59c1a',
        'REJECTED': '#ff5b57', 'CANCELLED': '#6c757d'
    };
    var colors = labels.map(function (s) {
        return statusColors[s.toUpperCase()] || '#348fe2';
    });

    var total = counts.reduce(function (a, b) { return a + b; }, 0);

    _chartAdvanceStatus = new ApexCharts(document.querySelector('#chartAdvanceStatus'), {
        chart:  { type: 'donut', height: 270, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: counts,
        labels: labels,
        colors: colors,
        legend: { position: 'bottom', labels: { colors: '#b6c2cf' } },
        plotOptions: { pie: { donut: { size: '65%', labels: {
            show: true,
            name:  { show: true, color: '#b6c2cf' },
            value: { show: true, color: '#fff' },
            total: { show: true, label: 'Total', color: '#b6c2cf',
                formatter: function () { return total; }
            }
        } } } },
        dataLabels: { enabled: true, style: { colors: ['#fff'], fontSize: '11px' } },
        tooltip:    { theme: 'dark' },
        noData:     { text: 'No advance requests', style: { color: '#b6c2cf' } }
    });
    _chartAdvanceStatus.render();
}

function ShowChartError() {
    $('#chartPayrollTrend, #chartPayrollBreakdown, #chartDeptHeadcount, ' +
      '#chartLeaveType, #chartLeaveStatus, #chartLeaveMonthly, #chartAdvanceStatus')
        .html('<p class="text-muted text-center p-t-30">Could not load data.</p>');
}
