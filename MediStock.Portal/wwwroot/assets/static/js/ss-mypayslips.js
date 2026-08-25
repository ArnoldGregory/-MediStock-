// ============================================================
//  RIZIKI — ss-mypayslips.js  (Employee: My Payslips)
// ============================================================
var _psTable;

$(document).ready(function () {
    App.init();
    _psTable = $('#payslipsTable').dataTable({
        responsive: true,
        order: [[0, 'desc']],
        aoColumns: [
            { data: 'period_name',      sDefaultContent: '—' },
            { data: 'basic_salary',     sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'gross_pay',        sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'total_deductions', sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'net_pay',          sDefaultContent: '0',
              mRender: function (d) { return '<strong>' + Fmt(d) + '</strong>'; } },
            { data: 'payment_date',     sDefaultContent: '—' },
            { bSortable: false,
              mRender: function (d, t, row) {
                  var id = row.payslip_id || row.id;
                  return '<a href="/SelfService/PayslipPdf?payslip_id=' + id + '" target="_blank" class="btn btn-default btn-xs">'
                       + '<i class="fa fa-file-pdf-o"></i> PDF</a>';
              }}
        ]
    });
    LoadMyPayslips();
});

function LoadMyPayslips() {
    $.get('/SelfService/GetMyPayslips', function (data) {
        var t = _psTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

function Fmt(n) {
    return 'KES ' + parseFloat(n || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 });
}
