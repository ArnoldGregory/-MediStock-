// ============================================================
//  RIZIKI — payroll-payslips.js  (Payslips page)
// ============================================================
var _psTable;

$(document).ready(function () {
    App.init();

    _psTable = $('#payslipsTable').dataTable({
        responsive: false,
        bPaginate: false,
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; }},
            { data: 'employee_name',     sDefaultContent: '—' },
            { data: 'basic_salary',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'total_allowances',  sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'overtime_pay',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'gross_pay',         sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'paye',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'nssf',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'shif',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'housing_levy',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'advance_deduction', sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'net_pay',           sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { bSortable: false,
              mRender: function (d, t, row) {
                  var pid = row.payslip_id || row.id;
                  return '<a href="/Payroll/PayslipPdf?payslip_id=' + pid + '" target="_blank" class="btn btn-default btn-xs">'
                       + '<i class="fa fa-file-pdf-o"></i> PDF</a>';
              }}
        ]
    });

    // Load periods into selector
    $.get('/Payroll/GetPeriods', function (data) {
        var sel = $('#ps_period_id');
        (data || []).forEach(function (p) {
            var id   = p.period_id || p.id;
            var name = p.period_name || ('Period ' + id);
            var processed = (p.status || '').toUpperCase() === 'PROCESSED';
            sel.append('<option value="' + id + '" data-name="' + name + '" data-processed="' + processed + '">'
                + name + (processed ? ' ✓' : ' (pending)') + '</option>');
        });

        // If period_id passed in query string, auto-select it
        var qs = new URLSearchParams(window.location.search);
        var preId = qs.get('period_id');
        if (preId) { sel.val(preId).trigger('change'); }
    });

    // Period change → load payslips
    $('#ps_period_id').on('change', function () {
        var id = $(this).val();
        if (!id) { $('#payslips-panel').hide(); $('#btnBankFile').hide(); return; }
        var opt = $(this).find('option:selected');
        var name = opt.data('name') || opt.text();
        LoadPayslips(id, name);
    });
});

function LoadPayslips(periodId, periodName) {
    $('#ps_period_name').text(periodName);
    $('#btnBankFile').attr('href', '/Payroll/BankFile?period_id=' + periodId).show();
    $('#payslips-panel').show();

    $.get('/Payroll/GetPayslips', { period_id: periodId }, function (data) {
        var t = _psTable, s = t.fnSettings();
        t.fnClearTable(true);

        var totAllowances = 0, totOvertime = 0, totGross = 0;
        var totPaye = 0, totNssf = 0, totShif = 0, totHousing = 0, totAdv = 0, totNet = 0;

        if (data && data.length) {
            for (var i = 0; i < data.length; i++) {
                t.oApi._fnAddData(s, data[i]);
                totAllowances += parseFloat(data[i].total_allowances  || 0);
                totOvertime   += parseFloat(data[i].overtime_pay       || 0);
                totGross      += parseFloat(data[i].gross_pay          || 0);
                totPaye       += parseFloat(data[i].paye               || 0);
                totNssf       += parseFloat(data[i].nssf               || 0);
                totShif       += parseFloat(data[i].shif               || 0);
                totHousing    += parseFloat(data[i].housing_levy       || 0);
                totAdv        += parseFloat(data[i].advance_deduction  || 0);
                totNet        += parseFloat(data[i].net_pay            || 0);
            }
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();

        $('#ft_allowances').text(Fmt(totAllowances));
        $('#ft_overtime').text(Fmt(totOvertime));
        $('#ft_gross').text(Fmt(totGross));
        $('#ft_paye').text(Fmt(totPaye));
        $('#ft_nssf').text(Fmt(totNssf));
        $('#ft_shif').text(Fmt(totShif));
        $('#ft_housing').text(Fmt(totHousing));
        $('#ft_advance').text(Fmt(totAdv));
        $('#ft_net').text(Fmt(totNet));
    });
}

function Fmt(n) {
    return parseFloat(n || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 });
}
