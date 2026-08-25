// ============================================================
//  RIZIKI — reports-statutory.js  (Monthly Statutory Returns)
// ============================================================

var _statTable;

$(document).ready(function () {
    App.init();

    // Populate year dropdown (last 5 years)
    var curYear = new Date().getFullYear();
    for (var y = curYear; y >= curYear - 4; y--) {
        var sel = y === curYear ? ' selected' : '';
        $('#selStatYear').append('<option value="' + y + '"' + sel + '>' + y + '</option>');
    }

    _statTable = InitStatutoryTable();
    LoadStatutory();

    $('#btnDownloadStatutory').on('click', function (e) {
        e.preventDefault();
        var year = $('#selStatYear').val() || new Date().getFullYear();
        window.location.href = '/Reports/DownloadStatutoryReturns?year=' + year;
    });
});

function InitStatutoryTable() {
    return $('#statutoryTable').dataTable({
        bPaginate: false,
        bFilter: false,
        aoColumns: [
            { data: 'month_name',        sDefaultContent: '—' },
            { data: 'employee_count',    sDefaultContent: '0' },
            { data: 'gross_pay',         sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'nssf',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'shif',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'housing_levy',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'paye',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'total_deductions',  sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
            { data: 'net_pay',           sDefaultContent: '0', mRender: function (d) { return Fmt(d); } }
        ]
    });
}

function LoadStatutory() {
    var year = $('#selStatYear').val() || new Date().getFullYear();

    $.get('/Reports/GetStatutoryReturns', { year: year }, function (res) {
        var data = res.data || res || [];
        var t = _statTable;
        var s = t.fnSettings();
        t.fnClearTable(true);

        var totEmp = 0, totGross = 0, totNssf = 0, totShif = 0,
            totHousing = 0, totPaye = 0, totDed = 0, totNet = 0;

        if (data && data.length) {
            for (var i = 0; i < data.length; i++) {
                t.oApi._fnAddData(s, data[i]);
                totEmp     += parseInt(data[i].employee_count || 0);
                totGross   += parseFloat(data[i].gross_pay || 0);
                totNssf    += parseFloat(data[i].nssf || 0);
                totShif    += parseFloat(data[i].shif || 0);
                totHousing += parseFloat(data[i].housing_levy || 0);
                totPaye    += parseFloat(data[i].paye || 0);
                totDed     += parseFloat(data[i].total_deductions || 0);
                totNet     += parseFloat(data[i].net_pay || 0);
            }
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();

        $('#ft_emp').text(totEmp);
        $('#ft_gross').text(Fmt(totGross));
        $('#ft_nssf').text(Fmt(totNssf));
        $('#ft_shif').text(Fmt(totShif));
        $('#ft_housing').text(Fmt(totHousing));
        $('#ft_paye').text(Fmt(totPaye));
        $('#ft_deductions').text(Fmt(totDed));
        $('#ft_net').text(Fmt(totNet));
    }).fail(function () {
        Swal.fire('Error', 'Failed to load statutory returns.', 'error');
    });
}

function Fmt(n) {
    return parseFloat(n || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 });
}
