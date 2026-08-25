// ============================================================
//  RIZIKI — employees.js
//  Employees list + Add Employee modal
// ============================================================

$(document).ready(function () {
    App.init();
    EmployeesTable.init();
    LoadDepartments();
    LoadBanks();

    // datepickers
    $('#dob_picker').datepicker({ format: 'dd-mm-yyyy', autoclose: true, todayHighlight: true });
    $('#hire_date_picker').datepicker({ format: 'dd-mm-yyyy', autoclose: true, todayHighlight: true });


    // payment method toggle
    // Bootstrap 3 data-toggle="buttons" swallows the native 'change' event,
    // so we bind to click on the label instead.
    $('#add-employee-modal .btn-group[data-toggle="buttons"] label').on('click', function () {
        // The checked state updates after the click, so defer by one tick
        setTimeout(TogglePaymentMethod, 0);
    });

    // clear modal on close
    $('#add-employee-modal').on('hidden.bs.modal', function () {
        ClearAddForm();
    });
});

function TogglePaymentMethod() {
    var pm = $('input[name="emp_payment_method"]:checked').val();
    if (pm === 'MOBILE_MONEY') {
        $('#bank_fields_section').hide();
        $('#mobile_money_section').show();
    } else {
        $('#bank_fields_section').show();
        $('#mobile_money_section').hide();
    }
}

// ── Load banks into dropdown ─────────────────────────────────────────────────
function LoadBanks() {
    $.get('/Employees/GetBanks', function (data) {
        var sel = $('#emp_bank_id');
        sel.find('option:not(:first)').remove();
        if (data && data.length) {
            $.each(data, function (i, b) {
                sel.append('<option value="' + b.id + '">' + b.bank_name + (b.abbreviation ? ' (' + b.abbreviation + ')' : '') + '</option>');
            });
        }
    });
}

// ── DataTable ────────────────────────────────────────────────────────────────
var EmployeesTable = function () {
    return {
        init: function () {
            var oTable = $('#employeesTable').dataTable({
                responsive: true,
                createdRow: function (row, data) {
                    $(row).attr('recid', data.id);
                },
                aoColumns: [
                    {
                        data: null, autoWidth: true, sDefaultContent: '',
                        render: function (data, type, row) {
                            var fn = row.first_name || '';
                            var mn = row.middle_name ? ' ' + row.middle_name : '';
                            var ln = row.last_name || '';
                            return (fn + mn + ' ' + ln).trim() || 'N/A';
                        }
                    },
                    { data: 'id_number',       autoWidth: true, sDefaultContent: 'N/A' },
                    { data: 'email',           autoWidth: true, sDefaultContent: 'N/A' },
                    { data: 'mobile',          autoWidth: true, sDefaultContent: 'N/A' },
                    { data: 'job_title',       autoWidth: true, sDefaultContent: 'N/A' },
                    { data: 'department_name', autoWidth: true, sDefaultContent: 'N/A' },
                    {
                        data: 'employment_type', autoWidth: true, sDefaultContent: 'N/A',
                        render: function (data) {
                            if (!data) return 'N/A';
                            var cls = data === 'PERMANENT' ? 'success' : data === 'CONTRACT' ? 'warning' : 'default';
                            return '<span class="label label-' + cls + '">' + data + '</span>';
                        }
                    },
                    { data: null, autoWidth: true, sDefaultContent: 'N/A',
                      render: function (data, type, row) {
                          var v = row.hire_date || '';
                          return v ? v.toString().split('T')[0] : 'N/A';
                      }},
                    {
                        bSortable: false,
                        sDefaultContent: '<a href="#" class="btn btn-info btn-xs view-emp"><i class="fa fa-eye"></i> View</a>'
                    },
                    {
                        bSortable: false,
                        sDefaultContent: '<a href="#" class="btn btn-default btn-xs docs-emp"><i class="fa fa-folder-open"></i> Docs</a>'
                    },
                    {
                        bSortable: false,
                        sDefaultContent: '<a href="#" class="btn btn-warning btn-xs ded-emp"><i class="fa fa-minus-circle"></i> Deductions</a>'
                    }
                ]
            });

            // View button click
            $('#employeesTable').on('click', 'a.view-emp', function (e) {
                e.preventDefault();
                var nRow = $(this).parents('tr')[0];
                var data = oTable.fnGetData(nRow);
                var d = JSON.parse(JSON.stringify(data));

                var full = ((d.first_name || '') + ' ' + (d.middle_name || '') + ' ' + (d.last_name || '')).trim();
                $('#vw_name').val(full);
                $('#vw_email').val(d.email || '');
                $('#vw_mobile').val(d.mobile || '');
                $('#vw_id_number').val(d.id_number || '');
                $('#vw_job_title').val(d.job_title || '');
                $('#vw_department').val(d.department_name || '');
                $('#vw_emp_type').val(d.employment_type || '');
                $('#vw_hire_date').val(d.hire_date || '');
                $('#vw_salary').val(d.basic_salary ? 'KES ' + parseFloat(d.basic_salary).toLocaleString() : '');
                $('#vw_kra_pin').val(d.kra_pin || '');
                $('#vw_nssf').val(d.nssf_number || '');
                $('#vw_nhif').val(d.nhif_number || '');

                // HOD toggle — treat undefined/null/0 as not-HOD
                var isHod = d.is_hod === 1 || d.is_hod === '1' || d.is_hod === true;
                $('#vw_employee_id').val(d.id);
                $('#vw_is_hod').val(isHod ? 1 : 0);
                $('#hodBtnLabel').text(isHod ? 'Remove HOD' : 'Make HOD');
                $('#btnToggleHod').toggleClass('btn-warning', !isHod).toggleClass('btn-danger', isHod);

                $('#view-employee-modal').appendTo('body').modal('show');
            });

            LoadEmployees(oTable);
        }
    };
}();

// ── Load employees into table ────────────────────────────────────────────────
function LoadEmployees(oTable) {
    $.get('/Employees/GetList', function (data) {
        var table = oTable || $('#employeesTable').dataTable();
        var settings = table.fnSettings();
        table.fnClearTable(true);
        if (data && data.length > 0) {
            for (var i = 0; i < data.length; i++) {
                table.oApi._fnAddData(settings, data[i]);
            }
        }
        settings.aiDisplay = settings.aiDisplayMaster.slice();
        table.fnDraw();
    });
}

// ── Load departments for dropdown ────────────────────────────────────────────
function LoadDepartments() {
    $.get('/Employees/GetDepartments', function (data) {
        var sel = $('#emp_department_id');
        sel.find('option:not(:first)').remove();
        if (data && data.length > 0) {
            $.each(data, function (i, d) {
                sel.append('<option value="' + d.id + '">' + (d.department_name || d.name) + '</option>');
            });
        }
    });
}

// ── Save new employee ────────────────────────────────────────────────────────
$('#btnSaveEmployee').on('click', function (e) {
    e.preventDefault();

    // basic validation
    if (!$('#emp_first_name').val() || !$('#emp_last_name').val()) {
        ShowError('First name and last name are required.'); return;
    }
    if (!$('#emp_email').val()) { ShowError('Email is required.'); return; }
    if (!$('#emp_mobile').val()) { ShowError('Mobile number is required.'); return; }
    if (!$('#emp_id_number').val()) { ShowError('ID number is required.'); return; }
    if (!$('#emp_job_title').val()) { ShowError('Job title is required.'); return; }
    if (!$('#emp_employment_type').val()) { ShowError('Employment type is required.'); return; }
    if (!$('#emp_department_id').val()) { ShowError('Department is required.'); return; }
    if (!$('#emp_basic_salary').val() || parseFloat($('#emp_basic_salary').val()) <= 0) {
        ShowError('Basic salary must be greater than 0.'); return;
    }
    // password is optional — API auto-generates if blank

    var pm = $('input[name="emp_payment_method"]:checked').val() || 'BANK';

    // payment method validation
    if (pm === 'BANK' && !$('#emp_bank_id').val()) {
        ShowError('Please select a bank for Bank Transfer.'); return;
    }
    if (pm === 'MOBILE_MONEY' && !$('#emp_mobile_money_number').val().trim()) {
        ShowError('M-Pesa number is required for Mobile Money.'); return;
    }

    var payload = {
        first_name:           $('#emp_first_name').val().trim(),
        middle_name:          $('#emp_middle_name').val().trim(),
        last_name:            $('#emp_last_name').val().trim(),
        id_number:            $('#emp_id_number').val().trim(),
        email:                $('#emp_email').val().trim(),
        mobile:               $('#emp_mobile').val().trim(),
        gender:               $('#emp_gender').val(),
        date_of_birth:        ParseDate($('#emp_dob').val()),
        job_title:            $('#emp_job_title').val().trim(),
        employment_type:      $('#emp_employment_type').val(),
        hire_date:            ParseDate($('#emp_hire_date').val()),
        department_id:        parseInt($('#emp_department_id').val()) || 0,
        basic_salary:         parseFloat($('#emp_basic_salary').val()) || 0,
        kra_pin:              $('#emp_kra_pin').val().trim(),
        nssf_number:          $('#emp_nssf_number').val().trim(),
        nhif_number:          $('#emp_nhif_number').val().trim(),
        payment_method:       pm,
        bank_id:              pm === 'BANK' ? (parseInt($('#emp_bank_id').val()) || 0) : 0,
        bank_account_no:      pm === 'BANK' ? $('#emp_bank_account_no').val().trim() : '',
        bank_branch:          pm === 'BANK' ? $('#emp_bank_branch').val().trim() : '',
        mobile_money_number:  pm === 'MOBILE_MONEY' ? $('#emp_mobile_money_number').val().trim() : '',
        password:             $('#emp_password').val()
    };

    Swal.fire({
        title: 'Add Employee?',
        text: 'Save ' + payload.first_name + ' ' + payload.last_name + ' as a new employee?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes, Save',
        reverseButtons: true
    }).then(function (result) {
        if (!result.isConfirmed) return;

        BlockPanel();
        $.ajax({
            url: '/Employees/AddEmployee',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (res) {
                UnblockPanel();
                if (res.success) {
                    $('#add-employee-modal').modal('hide');
                    LoadEmployees(null);
                    Swal.fire({ title: 'Success', text: res.message, icon: 'success', confirmButtonText: 'OK' });
                } else {
                    ShowError(res.message || 'Failed to save employee.');
                }
            },
            error: function (xhr) {
                UnblockPanel();
                ShowError('An error occurred. Please try again.');
            }
        });
    });
});

// ── Helpers ──────────────────────────────────────────────────────────────────
function ShowError(msg) {
    $('#emp-error-msg').text(msg).show();
}

function ClearAddForm() {
    $('#add-employee-modal input, #add-employee-modal select, #add-employee-modal textarea').val('');
    $('#emp-error-msg').hide();
    $('#emp_department_id').val('').trigger('change');
    $('#emp_bank_id').val('').trigger('change');
    // Reset payment method to Bank Transfer
    $('#pm_bank').prop('checked', true);
    $('#lbl_bank_transfer').addClass('active');
    $('#lbl_mobile_money').removeClass('active');
    $('#bank_fields_section').show();
    $('#mobile_money_section').hide();
}

// Parse DD-MM-YYYY → YYYY-MM-DD for API
function ParseDate(val) {
    if (!val) return null;
    var parts = val.split('-');
    if (parts.length === 3) return parts[2] + '-' + parts[1] + '-' + parts[0];
    return val;
}

// ── Documents modal ───────────────────────────────────────────────────────────
var _docsTable;

$('#employeesTable').on('click', 'a.docs-emp', function (e) {
    e.preventDefault();
    var oTable = $('#employeesTable').dataTable();
    var d = oTable.fnGetData($(this).parents('tr')[0]);
    var empId = d.id || d.employee_id;
    var empName = ((d.first_name || '') + ' ' + (d.last_name || '')).trim();

    $('#docs_employee_id').val(empId);
    $('#docs_emp_name').text(empName);
    $('#doc-upload-error, #doc-upload-success').hide();
    $('#doc_file').val('');

    if (_docsTable) { _docsTable.fnClearTable(true); _docsTable.fnDraw(); }
    else {
        _docsTable = $('#docsTable').dataTable({
            responsive: false, bPaginate: false,
            aoColumns: [
                { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                    return row.document_name || row.file_name || '—';
                }},
                { data: 'document_type', sDefaultContent: '—' },
                { data: 'created_on',    sDefaultContent: '—' },
                { bSortable: false, mRender: function (d, t, row) {
                    var docId = row.id || row.document_id;
                    if (!docId) return '—';
                    return '<a href="/Employees/DownloadDocument?doc_id=' + docId + '" class="btn btn-default btn-xs"><i class="fa fa-download"></i> Download</a>';
                }}
            ]
        });
    }

    LoadEmployeeDocs(empId);
    $('#docs-modal').appendTo('body').modal('show');
});

function LoadEmployeeDocs(empId) {
    $.get('/Employees/GetDocuments', { employee_id: empId }, function (data) {
        var t = _docsTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

$('#btnUploadDoc').on('click', function (e) {
    e.preventDefault();
    var file = $('#doc_file')[0].files[0];
    if (!file) { $('#doc-upload-error').text('Please select a file.').show(); return; }
    var maxSize = 5 * 1024 * 1024; // 5 MB
    if (file.size > maxSize) { $('#doc-upload-error').text('File size must be under 5 MB.').show(); return; }

    var reader = new FileReader();
    reader.onload = function (evt) {
        var base64 = evt.target.result; // includes data:...;base64, prefix — API strips it
        $('#doc-upload-error, #doc-upload-success').hide();
        $.ajax({
            url: '/Employees/UploadDocument', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                employee_id:   parseInt($('#docs_employee_id').val()),
                document_type: $('#doc_type').val(),
                file_name:     file.name,
                file_base64:   base64
            }),
            success: function (res) {
                if (res.success) {
                    $('#doc-upload-success').text(res.message).show();
                    $('#doc_file').val('');
                    LoadEmployeeDocs(parseInt($('#docs_employee_id').val()));
                } else {
                    $('#doc-upload-error').text(res.message).show();
                }
            },
            error: function () { $('#doc-upload-error').text('Upload failed.').show(); }
        });
    };
    reader.readAsDataURL(file);
});

// ── HOD toggle ───────────────────────────────────────────────────────────────
$('#btnToggleHod').on('click', function (e) {
    e.preventDefault();
    var empId  = parseInt($('#vw_employee_id').val());
    var isHod  = parseInt($('#vw_is_hod').val()) === 1;
    var newHod = !isHod;
    var label  = newHod ? 'designate this employee as Head of Department?' : 'remove HOD designation?';

    Swal.fire({
        title: newHod ? 'Make HOD?' : 'Remove HOD?',
        text: 'Are you sure you want to ' + label,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Employees/SetHod', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ employee_id: empId, is_hod: newHod }),
            success: function (res) {
                if (res.success) {
                    $('#vw_is_hod').val(newHod ? 1 : 0);
                    $('#hodBtnLabel').text(newHod ? 'Remove HOD' : 'Make HOD');
                    $('#btnToggleHod').toggleClass('btn-warning', !newHod).toggleClass('btn-danger', newHod);
                    Swal.fire('Done', res.message, 'success').then(function () {
                        $('#view-employee-modal').modal('hide');
                        LoadEmployees();
                    });
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

var _panel;
function BlockPanel() {
    _panel = $('#main_panel');
    if (!_panel.hasClass('panel-loading')) {
        _panel.addClass('panel-loading');
        _panel.find('.panel-body').prepend('<div class="panel-loader"><span class="spinner-small"></span></div>');
    }
}
function UnblockPanel() {
    if (_panel) { _panel.removeClass('panel-loading'); _panel.find('.panel-loader').remove(); }
}
