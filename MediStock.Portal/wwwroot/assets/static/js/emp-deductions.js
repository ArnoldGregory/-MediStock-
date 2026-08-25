/**
 * emp-deductions.js — HR management of custom employee deductions
 * Attached to Employees/Index.cshtml alongside employees.js
 */

var _dedTable;

// Open modal when "Deductions" button is clicked in the employees table
$(document).on('click', '#employeesTable a.ded-emp', function (e) {
    e.preventDefault();
    var oTable = $('#employeesTable').dataTable();
    var nRow   = $(this).closest('tr')[0];
    var d      = oTable.fnGetData(nRow);

    var empId   = d.id || d.employee_id;
    var empName = ((d.first_name || '') + ' ' + (d.last_name || '')).trim();

    $('#ded_employee_id').val(empId);
    $('#ded_emp_name').text(empName);
    $('#ded_name').val(''); $('#ded_amount').val('');

    if (!_dedTable) {
        _dedTable = $('#deductionsTable').dataTable({
            bPaginate: false,
            aoColumns: [
                { mRender: function (d, t, r) { return r.deduction_name || ''; } },
                { mRender: function (d, t, r) { return parseFloat(r.amount || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 }); } },
                { mRender: function (d, t, r) { return r.is_recurring ? '<span class="label label-success">Yes</span>' : '<span class="label label-default">No</span>'; } },
                { mRender: function (d, t, r) { return r.created_on ? new Date(r.created_on).toLocaleDateString('en-GB') : ''; } },
                { mRender: function (d, t, r) {
                    return '<a href="javascript:DeleteDeduction(' + r.id + ');" class="btn btn-danger btn-xs"><i class="fa fa-trash"></i></a>';
                }}
            ]
        });
    } else {
        _dedTable.fnClearTable();
    }

    LoadDeductions(empId);
    $('#deductions-modal').appendTo('body').modal('show');
});

function LoadDeductions(empId) {
    var id = empId || $('#ded_employee_id').val();
    $.get('/Employees/GetDeductions?employee_id=' + id, function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        _dedTable.fnClearTable();
        if (data.length) data.forEach(function (r) { _dedTable.fnAddData(r); });
    });
}

function AddDeduction(btn) {
    var empId     = parseInt($('#ded_employee_id').val());
    var name      = $('#ded_name').val().trim();
    var amount    = parseFloat($('#ded_amount').val());
    var recurring = $('#ded_recurring').val() === 'true';

    if (!name)            { toastr.warning('Deduction name is required'); return; }
    if (!amount || amount <= 0) { toastr.warning('Please enter a valid amount'); return; }

    btnLoad(btn, 'Saving...');
    $.ajax({
        url: '/Employees/AddDeduction', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ employee_id: empId, deduction_name: name, amount: amount, is_recurring: recurring }),
        success: function (res) {
            if (res.success) {
                toastr.success('Deduction added');
                $('#ded_name').val(''); $('#ded_amount').val('');
                LoadDeductions(empId);
            } else { toastr.error(res.message || 'Failed'); }
        },
        error: function () { toastr.error('Request failed'); },
        complete: function () { btnStop(btn); }
    });
}

function DeleteDeduction(id) {
    swal({ title: 'Remove Deduction?', text: 'This will remove the deduction from future payrolls.', type: 'warning', showCancelButton: true, confirmButtonText: 'Yes, remove' }, function (c) {
        if (!c) return;
        pageBlock('Removing...');
        $.ajax({
            url: '/Employees/DeleteDeduction', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { toastr.success('Removed'); LoadDeductions(); }
                else { toastr.error(res.message || 'Failed'); }
            },
            complete: function () { pageUnblock(); }
        });
    });
}
