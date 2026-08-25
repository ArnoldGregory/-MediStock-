// ============================================================
//  RIZIKI — payroll-salaries.js
// ============================================================

var _salTable;

$(document).ready(function () {
    App.init();
    _salTable = InitSalaryTable();
    LoadSalaries();

    // live gross preview
    $('#sal_basic, #sal_house, #sal_transport, #sal_medical, #sal_other').on('input', UpdateGrossPreview);

    $('#salary-modal').on('hidden.bs.modal', function () {
        $('#sal-error').hide();
        $('#sal_basic, #sal_house, #sal_transport, #sal_medical, #sal_other').val('');
        $('#sal_effective').val('');
        $('#sal_gross_preview').val('');
    });
});

function InitSalaryTable() {
    return $('#salaryTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.employee_id || data.id); },
        aoColumns: [
            { data: null, autoWidth: true, sDefaultContent: '',
              mRender: function (d) { return (d.first_name || '') + ' ' + (d.last_name || ''); } },
            { data: 'staff_number',      autoWidth: true, sDefaultContent: '—' },
            { data: 'department_name',   autoWidth: true, sDefaultContent: '—' },
            { data: 'job_title',         autoWidth: true, sDefaultContent: '—' },
            { data: 'employment_type',   autoWidth: true, sDefaultContent: '—' },
            { bSortable: false, sDefaultContent:
              '<a href="#" class="btn btn-primary btn-xs manage-salary"><i class="fa fa-money"></i> Manage Salary</a>' }
        ]
    });
}

function LoadSalaries() {
    $.get('/Payroll/GetEmployees', function (data) {
        var t = _salTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── Manage Salary click ──────────────────────────────────────────────────────
$('#salaryTable').on('click', 'a.manage-salary', function (e) {
    e.preventDefault();
    var d = _salTable.fnGetData($(this).parents('tr')[0]);
    var empId = d.employee_id || d.id;

    $('#sal_employee_id').val(empId);
    $('#sal_emp_name').text((d.first_name || '') + ' ' + (d.last_name || ''));
    $('#sal_basic, #sal_house, #sal_transport, #sal_medical, #sal_other').val(0);
    $('#sal_effective').val(new Date().toISOString().split('T')[0]);
    $('#sal_gross_preview').val('0.00');
    $('#sal-error').hide();

    // load current salary structure
    $.get('/Payroll/GetSalaryStructure', { employee_id: empId }, function (data) {
        if (data && data.length > 0) {
            var s = data[0];
            $('#sal_basic').val(s.basic_salary || 0);
            $('#sal_house').val(s.house_allowance || 0);
            $('#sal_transport').val(s.transport_allowance || 0);
            $('#sal_medical').val(s.medical_allowance || 0);
            $('#sal_other').val(s.other_allowance || 0);
            if (s.effective_date) $('#sal_effective').val(s.effective_date.split('T')[0]);
            UpdateGrossPreview();
        }
    });

    $('#salary-modal').appendTo('body').modal('show');
});

function UpdateGrossPreview() {
    var total = parseFloat($('#sal_basic').val() || 0)
              + parseFloat($('#sal_house').val() || 0)
              + parseFloat($('#sal_transport').val() || 0)
              + parseFloat($('#sal_medical').val() || 0)
              + parseFloat($('#sal_other').val() || 0);
    $('#sal_gross_preview').val('KES ' + total.toLocaleString('en-KE', { minimumFractionDigits: 2 }));
}

// ── Save Salary ──────────────────────────────────────────────────────────────
$('#btnSaveSalary').on('click', function (e) {
    e.preventDefault();
    var basic = parseFloat($('#sal_basic').val()) || 0;
    if (basic <= 0) { $('#sal-error').text('Basic salary must be greater than 0.').show(); return; }

    Swal.fire({
        title: 'Save Salary Structure?',
        text: 'This will update the salary for ' + $('#sal_emp_name').text(),
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        confirmButtonText: 'Yes, Save',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Payroll/SetSalary', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                employee_id:         parseInt($('#sal_employee_id').val()),
                basic_salary:        parseFloat($('#sal_basic').val()) || 0,
                house_allowance:     parseFloat($('#sal_house').val()) || 0,
                transport_allowance: parseFloat($('#sal_transport').val()) || 0,
                medical_allowance:   parseFloat($('#sal_medical').val()) || 0,
                other_allowance:     parseFloat($('#sal_other').val()) || 0,
                effective_date:      $('#sal_effective').val() || null
            }),
            success: function (res) {
                if (res.success) {
                    $('#salary-modal').modal('hide');
                    Swal.fire('Saved!', res.message, 'success');
                } else {
                    $('#sal-error').text(res.message).show();
                }
            },
            error: function () { $('#sal-error').text('Request failed.').show(); }
        });
    });
});
