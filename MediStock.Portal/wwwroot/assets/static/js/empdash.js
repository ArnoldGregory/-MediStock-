// ============================================================
//  RIZIKI — empdash.js  (Employee Dashboard)
//  Calls /Dashboard/EmpAnalytics which proxies api/dashboard/empstats
// ============================================================

var _chartPayslip = null;
var _chartLeave   = null;

$(document).ready(function () {
    LoadEmpDashboard();
});

function LoadEmpDashboard() {
    $.getJSON('/Dashboard/EmpAnalytics', function (d) {
        if (!d) { ShowEmpError(); return; }

        // ── Stat cards ──────────────────────────────────────────
        var bal = d.stat_leave_balance !== undefined ? d.stat_leave_balance + ' days' : '—';
        $('#statBalance').text(bal);
        $('#statMyLeave').text(d.stat_pending_leave !== undefined ? d.stat_pending_leave : '—');
        $('#statMyAdvances').text(d.stat_pending_adv !== undefined ? d.stat_pending_adv : '—');
        $('#statMyPayslip').text(d.stat_latest_net || '—');

        // ── Charts ──────────────────────────────────────────────
        RenderPayslipTrend(d.payslip_trend || []);
        RenderLeaveBalance(d.leave_balance || []);

        // ── HOD panel ───────────────────────────────────────────
        if (d.is_hod === true || d.is_hod === 'true') {
            $('#hodApprovalsRow').show();
            LoadHodPending();
        }

    }).fail(function () {
        ShowEmpError();
    });
}

// ── HOD dept approvals table ─────────────────────────────────────
var _hodTable = null;

function LoadHodPending() {
    $.get('/SelfService/GetHodPending', function (data) {
        var tbody = $('#hodLeaveBody');
        tbody.empty();

        if (!_hodTable) {
            _hodTable = $('#hodLeaveTable').dataTable({
                responsive: true,
                bDestroy: true,
                aoColumns: [
                    { data: 'employee_name', autoWidth: true, sDefaultContent: '—' },
                    { data: 'leave_type_name', autoWidth: true, sDefaultContent: '—' },
                    { data: 'start_date', autoWidth: true, sDefaultContent: '—' },
                    { data: 'end_date', autoWidth: true, sDefaultContent: '—' },
                    { data: 'days_requested', autoWidth: true, sDefaultContent: '—' },
                    { data: 'reason', autoWidth: true, sDefaultContent: '—' },
                    { data: 'created_on', autoWidth: true, sDefaultContent: '—' },
                    { bSortable: false, sDefaultContent:
                      '<a href="#" class="btn btn-success btn-xs hod-approve m-r-5"><i class="fa fa-check"></i> Approve</a>' +
                      '<a href="#" class="btn btn-danger btn-xs hod-reject"><i class="fa fa-ban"></i> Reject</a>' }
                ]
            });
        }

        var t = _hodTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();

        if (!data || data.length === 0) {
            $('#hodLeaveBody').html('<tr><td colspan="8" class="text-muted text-center">No pending approvals from your team.</td></tr>');
        }
    });
}

function HodAction(leaveId, action) {
    var label = action === 'approve' ? 'Approve' : 'Reject';
    Swal.fire({
        title: label + ' Leave?',
        text: 'You are about to ' + label.toLowerCase() + ' this leave request.',
        icon: action === 'approve' ? 'question' : 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, ' + label,
        confirmButtonColor: action === 'approve' ? '#28a745' : '#d33',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/SelfService/HodReview', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ leave_id: leaveId, action: action }),
            success: function (res) {
                if (res.success) {
                    Swal.fire('Done', res.message, 'success').then(function () { LoadHodPending(); });
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
}

// Delegate click handlers for HOD table
$(document).on('click', 'a.hod-approve', function (e) {
    e.preventDefault();
    if (!_hodTable) return;
    var d = _hodTable.fnGetData($(this).parents('tr')[0]);
    HodAction(d.id || d.leave_id, 'approve');
});
$(document).on('click', 'a.hod-reject', function (e) {
    e.preventDefault();
    if (!_hodTable) return;
    var d = _hodTable.fnGetData($(this).parents('tr')[0]);
    HodAction(d.id || d.leave_id, 'reject');
});

// ── Chart 1: My Net Pay bar chart ───────────────────────────────
function RenderPayslipTrend(rows) {
    // Rows come newest-first from API (ORDER BY payment_date DESC LIMIT 6)
    // Reverse so chart reads left=oldest → right=newest
    rows = rows.slice().reverse();

    var labels  = rows.map(function (r) { return r.month_label || ''; });
    var netPay  = rows.map(function (r) { return parseFloat(r.net_pay)   || 0; });
    var grossPay = rows.map(function (r) { return parseFloat(r.gross_pay) || 0; });

    if (_chartPayslip) { _chartPayslip.destroy(); }

    var opts = {
        chart:  { type: 'bar', height: 260, toolbar: { show: false }, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: [
            { name: 'Gross Pay', data: grossPay },
            { name: 'Net Pay',   data: netPay   }
        ],
        plotOptions: { bar: { borderRadius: 3, columnWidth: '65%', grouped: true } },
        xaxis:  { categories: labels, labels: { style: { colors: '#b6c2cf' } } },
        yaxis:  {
            labels: {
                style: { colors: '#b6c2cf' },
                formatter: function (v) { return 'KES ' + (v / 1000).toFixed(0) + 'k'; }
            }
        },
        colors:  ['#348fe2', '#00acac'],
        dataLabels: { enabled: false },
        grid:    { borderColor: '#3d4a56' },
        legend:  { labels: { colors: '#b6c2cf' } },
        tooltip: {
            theme: 'dark',
            y: { formatter: function (v) { return 'KES ' + v.toLocaleString('en-KE', { minimumFractionDigits: 2 }); } }
        },
        noData:  { text: 'No payslip data yet', style: { color: '#b6c2cf' } }
    };

    _chartPayslip = new ApexCharts(document.querySelector('#chartPayslipTrend'), opts);
    _chartPayslip.render();
}

// ── Chart 2: Leave balance donut ────────────────────────────────
function RenderLeaveBalance(rows) {
    var labels     = rows.map(function (r) { return r.type_name || 'Leave'; });
    var remaining  = rows.map(function (r) { return Math.max(parseInt(r.remaining_days) || 0, 0); });

    if (_chartLeave) { _chartLeave.destroy(); }

    var palette = ['#00acac', '#348fe2', '#f59c1a', '#ff5b57', '#727cb6', '#49b6d6'];

    var opts = {
        chart:  { type: 'donut', height: 260, background: 'transparent' },
        theme:  { mode: 'dark' },
        series: remaining,
        labels: labels,
        colors: palette.slice(0, labels.length),
        legend: { position: 'bottom', labels: { colors: '#b6c2cf' } },
        dataLabels: { enabled: true, style: { colors: ['#fff'] } },
        plotOptions: { pie: { donut: { size: '60%', labels: {
            show: true,
            total: { show: true, label: 'Total Days', color: '#b6c2cf',
                formatter: function (w) {
                    return w.globals.seriesTotals.reduce(function (a, b) { return a + b; }, 0);
                }
            }
        } } } },
        tooltip: {
            theme: 'dark',
            y: { formatter: function (v) { return v + ' days'; } }
        },
        noData: { text: 'No leave balance data', style: { color: '#b6c2cf' } }
    };

    _chartLeave = new ApexCharts(document.querySelector('#chartLeaveBalance'), opts);
    _chartLeave.render();
}

function ShowEmpError() {
    $('#statBalance, #statMyLeave, #statMyAdvances, #statMyPayslip').text('—');
    $('#chartPayslipTrend, #chartLeaveBalance')
        .html('<p class="text-muted text-center p-t-20">Could not load data.</p>');
}
