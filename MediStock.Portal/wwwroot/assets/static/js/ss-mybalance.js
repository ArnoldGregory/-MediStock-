// ============================================================
//  RIZIKI — ss-mybalance.js  (Employee: Leave Balance)
// ============================================================
var _balTable;

$(document).ready(function () {
    App.init();
    _balTable = $('#balanceTable').dataTable({
        responsive: true,
        bPaginate: false,
        aoColumns: [
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return row.leave_type_name || row.leave_name || row.type_name || '—';
            }},
            { data: null, sDefaultContent: '0', mRender: function (d, t, row) {
                return row.annual_days || row.annual_entitlement || row.entitlement_days || row.total_days || 0;
            }},
            { data: null, sDefaultContent: '0', mRender: function (d, t, row) {
                return row.used_days || row.days_used || row.days_taken || row.taken_days || 0;
            }},
            { data: null, sDefaultContent: '0', mRender: function (d, t, row) {
                var n = parseInt(row.remaining_days || row.days_remaining || row.balance_days || row.balance || 0);
                var cls = n <= 0 ? 'danger' : n <= 3 ? 'warning' : 'success';
                return '<span class="label label-' + cls + '">' + n + '</span>';
            }}
        ]
    });
    LoadBalance();
});

function LoadBalance() {
    $.get('/SelfService/GetMyBalance', function (data) {
        var t = _balTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}
