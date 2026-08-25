// ============================================================
//  RIZIKI — reports-payroll.js  (Payroll Summary Report)
// ============================================================

var _rptTable;

$(document).ready(function () {
    App.init();
    LoadPeriodOptions();
});

function LoadPeriodOptions() {
    $.get('/Reports/GetPeriods', function (data) {
        var sel = $('#rpt_period_sel');
        sel.find('option:not(:first)').remove();
        if (data && data.length) {
            $.each(data, function (i, p) {
                sel.append($('<option>', { value: p.period_id || p.id, text: p.period_name }));
            });
        }
    });
}

$('#btnLoadReport').on('click', function (e) {
    e.preventDefault();
    var periodId   = $('#rpt_period_sel').val();
    var periodName = $('#rpt_period_sel option:selected').text();
    if (!periodId) { Swal.fire('Select a Period', 'Please select a payroll period first.', 'warning'); return; }

    $('#rpt_title').text('Payroll Summary Report');
    $('#rpt_subtitle').text(periodName + '  |  Generated: ' + new Date().toLocaleDateString('en-KE'));

    $.get('/Reports/GetPayslips', { period_id: periodId }, function (data) {
        if (!data || !data.length) {
            Swal.fire('No Data', 'No payslips found for the selected period. Run payroll first.', 'info');
            $('#report-area').hide();
            return;
        }

        // destroy and re-init table
        if (_rptTable) { _rptTable.fnDestroy(); $('#rptTable').find('tbody').empty(); }

        _rptTable = $('#rptTable').dataTable({
            responsive: false,
            bPaginate: false,
            aoColumns: [
                { data: null, mRender: function (d, t, r, m) { return m.row + 1; }, bSortable: false },
                { data: 'employee_name',     sDefaultContent: '—' },
                { data: 'staff_number',      sDefaultContent: '—' },
                { data: 'basic_salary',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'gross_pay',         sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'paye',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'nssf',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'shif',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'housing_levy',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'advance_deduction', sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'total_deductions',  sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'net_pay',           sDefaultContent: '0', mRender: function (d) { return Fmt(d); } }
            ]
        });

        var s = _rptTable.fnSettings();
        var totGross = 0, totPaye = 0, totNssf = 0, totShif = 0,
            totHousing = 0, totAdv = 0, totDed = 0, totNet = 0;

        for (var i = 0; i < data.length; i++) {
            _rptTable.oApi._fnAddData(s, data[i]);
            totGross   += parseFloat(data[i].gross_pay || 0);
            totPaye    += parseFloat(data[i].paye || 0);
            totNssf    += parseFloat(data[i].nssf || 0);
            totShif    += parseFloat(data[i].shif || 0);
            totHousing += parseFloat(data[i].housing_levy || 0);
            totAdv     += parseFloat(data[i].advance_deduction || 0);
            totDed     += parseFloat(data[i].total_deductions || 0);
            totNet     += parseFloat(data[i].net_pay || 0);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        _rptTable.fnDraw();

        // footer totals
        $('#ft2_gross').text(Fmt(totGross));
        $('#ft2_paye').text(Fmt(totPaye));
        $('#ft2_nssf').text(Fmt(totNssf));
        $('#ft2_shif').text(Fmt(totShif));
        $('#ft2_housing').text(Fmt(totHousing));
        $('#ft2_advance').text(Fmt(totAdv));
        $('#ft2_totalded').text(Fmt(totDed));
        $('#ft2_net').text(Fmt(totNet));

        // KPI widgets
        $('#rpt_count').text(data.length);
        $('#rpt_gross').text('KES ' + Fmt(totGross));
        $('#rpt_paye').text('KES ' + Fmt(totPaye));
        $('#rpt_net').text('KES ' + Fmt(totNet));

        $('#report-area').show();
        $('#btnPrintReport, #btnExportReport').show();
        $('#btnExportReport').attr('href', '/Payroll/BankFile?period_id=' + periodId);
    });
});

$('#btnPrintReport').on('click', function (e) { e.preventDefault(); window.print(); });

function Fmt(n) {
    return parseFloat(n || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 });
}
