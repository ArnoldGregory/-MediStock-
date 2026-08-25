var p9Table;

$(document).ready(function () {
    // Populate year selector (last 5 years)
    var curYear = new Date().getFullYear();
    for (var y = curYear; y >= curYear - 4; y--) {
        $('#selYear').append('<option value="' + y + '"' + (y === curYear ? ' selected' : '') + '>' + y + '</option>');
    }

    // Load employees
    $.get('/Reports/GetEmployees', function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        data.forEach(function (e) {
            var name = (e.first_name || '') + ' ' + (e.last_name || e.middle_name || '');
            $('#selEmployee').append('<option value="' + (e.id || e.employee_id) + '">' + name.trim() + '</option>');
        });
    });

    p9Table = $('#p9Table').dataTable({
        bPaginate: false, bFilter: false, bInfo: false,
        aoColumns: [
            { mRender: function (d, t, r) { return r.period_name || ''; } },
            { mRender: function (d, t, r) { return fmt(r.basic_salary); } },
            { mRender: function (d, t, r) { return fmt(r.total_allowances); } },
            { mRender: function (d, t, r) { return fmt(r.gross_pay); } },
            { mRender: function (d, t, r) { return fmt(r.nssf); } },
            { mRender: function (d, t, r) { return fmt(r.shif); } },
            { mRender: function (d, t, r) { return fmt(r.housing_levy); } },
            { mRender: function (d, t, r) { return fmt(r.taxable_income); } },
            { mRender: function (d, t, r) { return fmt(r.paye); } },
            { mRender: function (d, t, r) { return fmt(r.personal_relief); } },
            { mRender: function (d, t, r) { return fmt(r.net_paye); } }
        ]
    });
});

function fmt(v) { return parseFloat(v || 0).toLocaleString('en-KE', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }

function LoadP9(btn) {
    var empId = $('#selEmployee').val();
    var year  = $('#selYear').val();
    if (!empId) { toastr.warning('Please select an employee'); return; }

    btnLoad(btn, 'Loading...');
    $.get('/Reports/GetP9Data?employee_id=' + empId + '&year=' + year, function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        p9Table.fnClearTable();

        if (!data.length) {
            $('#p9Report').hide(); $('#p9Empty').show().text('No payslip data found for the selected employee and year.');
            return;
        }

        var first = data[0];
        $('#lblEmpName').text((first.first_name || '') + ' ' + (first.last_name || ''));
        $('#lblKraPin').text(first.kra_pin || 'N/A');
        $('#lblEmployerPin').text(first.company_kra_pin || 'N/A');
        $('#lblYear').text(year);

        var totals = { basic: 0, allow: 0, gross: 0, nssf: 0, shif: 0, housing: 0, taxable: 0, paye: 0, relief: 0, netpaye: 0 };
        data.forEach(function (r) {
            p9Table.fnAddData(r);
            totals.basic   += parseFloat(r.basic_salary    || 0);
            totals.allow   += parseFloat(r.total_allowances|| 0);
            totals.gross   += parseFloat(r.gross_pay       || 0);
            totals.nssf    += parseFloat(r.nssf            || 0);
            totals.shif    += parseFloat(r.shif            || 0);
            totals.housing += parseFloat(r.housing_levy    || 0);
            totals.taxable += parseFloat(r.taxable_income  || 0);
            totals.paye    += parseFloat(r.paye            || 0);
            totals.relief  += parseFloat(r.personal_relief || 0);
            totals.netpaye += parseFloat(r.net_paye        || 0);
        });

        $('#tBasic').text(fmt(totals.basic)); $('#tAllowances').text(fmt(totals.allow));
        $('#tGross').text(fmt(totals.gross)); $('#tNssf').text(fmt(totals.nssf));
        $('#tShif').text(fmt(totals.shif));   $('#tHousing').text(fmt(totals.housing));
        $('#tTaxable').text(fmt(totals.taxable)); $('#tPaye').text(fmt(totals.paye));
        $('#tRelief').text(fmt(totals.relief)); $('#tNetPaye').text(fmt(totals.netpaye));

        $('#p9Empty').hide(); $('#p9Report').show();
    }).always(function () { btnStop(btn); });
}

function PrintP9() {
    if ($('#p9Report').is(':hidden')) { toastr.warning('Load a P9 report first'); return; }
    window.print();
}
