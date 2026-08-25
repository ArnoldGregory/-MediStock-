// ============================================================
//  RIZIKI — reports-employees.js  (Employee Report)
// ============================================================

var _empRptTable;

$(document).ready(function () {
    App.init();
    $('#emp_rpt_date').text('Generated: ' + new Date().toLocaleDateString('en-KE'));
    _empRptTable = InitEmpRptTable();
    LoadEmpReport();
});

function InitEmpRptTable() {
    return $('#empRptTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: null, bSortable: false, mRender: function (d, t, r, m) { return m.row + 1; } },
            { data: null, sDefaultContent: '', mRender: function (d) { return (d.first_name || '') + ' ' + (d.last_name || ''); } },
            { data: 'staff_number',   sDefaultContent: '—' },
            { data: 'id_number',      sDefaultContent: '—' },
            { data: 'email',          sDefaultContent: '—' },
            { data: 'mobile',         sDefaultContent: '—' },
            { data: 'department_name',sDefaultContent: '—' },
            { data: 'job_title',      sDefaultContent: '—' },
            { data: 'employment_type',sDefaultContent: '—' },
            { data: 'hire_date',      sDefaultContent: '—' }
        ]
    });
}

function LoadEmpReport() {
    $.get('/Reports/GetEmployees', function (data) {
        var t = _empRptTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}
