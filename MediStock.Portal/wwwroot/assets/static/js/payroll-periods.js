// ============================================================
//  RIZIKI — payroll-periods.js
// ============================================================

var _perTable, _payslipTable;

$(document).ready(function () {
    App.init();
    _perTable = InitPeriodsTable();
    LoadPeriods();

    $('#create-period-modal').on('hidden.bs.modal', function () {
        $('#new_period_name, #new_period_start, #new_period_end, #new_period_payment').val('');
        $('#create-period-error').hide();
    });

    $('#payslips-modal').on('shown.bs.modal', function () {
        if (_payslipTable) _payslipTable.fnDraw();
    });
});

function InitPeriodsTable() {
    return $('#periodsTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.period_id || data.id); },
        aoColumns: [
            { data: 'period_name',   autoWidth: true, sDefaultContent: '—' },
            { data: 'start_date',    autoWidth: true, sDefaultContent: '—' },
            { data: 'end_date',      autoWidth: true, sDefaultContent: '—' },
            { data: 'payment_date',  autoWidth: true, sDefaultContent: '—' },
            { data: 'status', autoWidth: true, sDefaultContent: '—',
              mRender: function (d) {
                  var s = (d || '').toUpperCase();
                  var cls = s === 'PROCESSED' ? 'success'
                          : s === 'LOCKED'    ? 'default'
                          : s === 'AWAITING_APPROVAL' ? 'primary'
                          : 'warning';
                  var label = s === 'AWAITING_APPROVAL' ? 'AWAITING APPROVAL' : (s || 'OPEN');
                  return '<span class="label label-' + cls + '">' + label + '</span>';
              }},
            { bSortable: false, autoWidth: true,
              mRender: function (d, t, row) {
                  var id     = row.period_id || row.id;
                  var name   = row.period_name || '';
                  var status = (row.status || '').toUpperCase();
                  var isAdmin = (typeof RIZIKI_PROFILE_ID !== 'undefined' && parseInt(RIZIKI_PROFILE_ID) === 1);
                  var btns   = '';
                  // Run Payroll: only on OPEN (not PROCESSED, LOCKED, or AWAITING_APPROVAL)
                  if (status === 'OPEN' || status === '') {
                      btns += '<a href="#" class="btn btn-warning btn-xs run-payroll m-r-3" data-id="' + id + '" data-name="' + name + '">'
                           +  '<i class="fa fa-play"></i> Run Payroll</a> ';
                  }
                  // AWAITING_APPROVAL: show Review button for ADMIN
                  if (status === 'AWAITING_APPROVAL' && isAdmin) {
                      btns += '<a href="#" class="btn btn-primary btn-xs review-payroll m-r-3" data-id="' + id + '" data-name="' + name + '">'
                           +  '<i class="fa fa-check-circle"></i> Review</a> ';
                  }
                  btns += '<a href="#" class="btn btn-info btn-xs view-payslips m-r-3" data-id="' + id + '" data-name="' + name + '">'
                       +  '<i class="fa fa-list"></i> Payslips</a>';
                  if (status === 'PROCESSED') {
                      btns += ' <a href="/Payroll/BankFile?period_id=' + id + '" class="btn btn-default btn-xs m-r-3" target="_blank">'
                           +  '<i class="fa fa-download"></i> Bank File</a>';
                      btns += ' <a href="#" class="btn btn-danger btn-xs lock-period" data-id="' + id + '" data-name="' + name + '">'
                           +  '<i class="fa fa-lock"></i> Lock</a>';
                  }
                  if (status === 'LOCKED') {
                      btns += ' <a href="/Payroll/BankFile?period_id=' + id + '" class="btn btn-default btn-xs" target="_blank">'
                           +  '<i class="fa fa-download"></i> Bank File</a>';
                  }
                  return btns;
              }}
        ]
    });
}

function LoadPeriods() {
    $.get('/Payroll/GetPeriods', function (data) {
        var t = _perTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── Create Period ─────────────────────────────────────────────────────────────
$('#btnCreatePeriod').on('click', function (e) {
    e.preventDefault();
    var name = $('#new_period_name').val().trim();
    if (!name) { $('#create-period-error').text('Period name is required.').show(); return; }

    $.ajax({
        url: '/Payroll/CreatePeriod', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            period_name:  name,
            start_date:   $('#new_period_start').val() || null,
            end_date:     $('#new_period_end').val() || null,
            payment_date: $('#new_period_payment').val() || null
        }),
        success: function (res) {
            if (res.success) {
                $('#create-period-modal').modal('hide');
                LoadPeriods();
                Swal.fire('Created!', res.message, 'success');
            } else {
                $('#create-period-error').text(res.message).show();
            }
        },
        error: function () { $('#create-period-error').text('Request failed.').show(); }
    });
});

// ── Run Payroll click ─────────────────────────────────────────────────────────
$('#periodsTable').on('click', 'a.run-payroll', function (e) {
    e.preventDefault();
    var id   = $(this).data('id');
    var name = $(this).data('name');

    Swal.fire({
        title: 'Run Payroll?',
        html:  'This will process payroll for all active employees in period<br><strong>' + name + '</strong>.<br>This cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#f59c1a',
        confirmButtonText: 'Yes, Run Payroll',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;

        // show loading
        Swal.fire({ title: 'Processing…', text: 'Please wait while payroll is being processed.', allowOutsideClick: false, didOpen: function () { Swal.showLoading(); } });

        $.ajax({
            url: '/Payroll/RunPayroll', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ period_id: id }),
            success: function (res) {
                Swal.close();
                if (res.success) {
                    LoadPeriods();
                    Swal.fire({ title: 'Payroll Complete!', text: res.message || 'Payroll processed successfully.', icon: 'success' });
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () { Swal.close(); Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// ── Lock Period click ─────────────────────────────────────────────────────────
$('#periodsTable').on('click', 'a.lock-period', function (e) {
    e.preventDefault();
    var id   = $(this).data('id');
    var name = $(this).data('name');

    Swal.fire({
        title: 'Lock Period?',
        html:  'Locking <strong>' + name + '</strong> will prevent any further payroll runs for this period. This cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Lock It',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;

        $.ajax({
            url: '/Payroll/LockPeriod', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ period_id: id }),
            success: function (res) {
                if (res.success) {
                    LoadPeriods();
                    Swal.fire('Locked!', res.message, 'success');
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// ── View Payslips click ───────────────────────────────────────────────────────
$('#periodsTable').on('click', 'a.view-payslips', function (e) {
    e.preventDefault();
    var id   = $(this).data('id');
    var name = $(this).data('name');

    $('#payslip_period_id').val(id);
    $('#payslip_period_name').text(name);
    $('#btnBankFile').attr('href', '/Payroll/BankFile?period_id=' + id);

    // init or reset payslips table
    if (_payslipTable) {
        _payslipTable.fnClearTable(true);
        _payslipTable.fnDraw();
    } else {
        _payslipTable = $('#payslipsTable').dataTable({
            responsive: false,
            bPaginate: false,
            aoColumns: [
                { data: 'employee_name',     sDefaultContent: '—' },
                { data: 'basic_salary',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'total_allowances',  sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'overtime_pay',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'gross_pay',         sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'paye',              sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'nssf',             sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'shif',             sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'housing_levy',      sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'advance_deduction', sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { data: 'net_pay',           sDefaultContent: '0', mRender: function (d) { return Fmt(d); } },
                { bSortable: false, mRender: function (d, t, row) {
                    return '<a href="/Payroll/PayslipPdf?payslip_id=' + (row.payslip_id || row.id) + '" target="_blank" class="btn btn-default btn-xs"><i class="fa fa-file-pdf-o"></i> PDF</a>';
                }}
            ]
        });
    }

    $.get('/Payroll/GetPayslips', { period_id: id }, function (data) {
        var t = _payslipTable;
        var s = t.fnSettings();
        t.fnClearTable(true);

        var totBasic = 0, totAllowances = 0, totOvertime = 0, totGross = 0,
            totPaye = 0, totNssf = 0, totShif = 0, totHousing = 0, totAdv = 0, totNet = 0;

        if (data && data.length) {
            for (var i = 0; i < data.length; i++) {
                t.oApi._fnAddData(s, data[i]);
                totBasic      += parseFloat(data[i].basic_salary || 0);
                totAllowances += parseFloat(data[i].total_allowances || 0);
                totOvertime   += parseFloat(data[i].overtime_pay || 0);
                totGross      += parseFloat(data[i].gross_pay || 0);
                totPaye       += parseFloat(data[i].paye || 0);
                totNssf       += parseFloat(data[i].nssf || 0);
                totShif       += parseFloat(data[i].shif || 0);
                totHousing    += parseFloat(data[i].housing_levy || 0);
                totAdv        += parseFloat(data[i].advance_deduction || 0);
                totNet        += parseFloat(data[i].net_pay || 0);
            }
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();

        $('#ft_basic').text(Fmt(totBasic));
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

    $('#payslips-modal').appendTo('body').modal('show');
});

// ── Quick Run ─────────────────────────────────────────────────────────────────
$('#btnQuickRun').on('click', function (e) {
    e.preventDefault();
    Swal.fire({
        title: 'Quick Run Payroll',
        html: '<p>This will automatically create a payroll period for <strong>this month</strong> (if not already open) and process all active employees.</p>'
            + '<div class="form-group m-t-10">'
            + '<label>Payment Day of Month</label>'
            + '<input id="qr_pay_day" type="number" min="1" max="28" value="28" class="form-control" style="width:120px;margin:auto;">'
            + '</div>',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#348fe2',
        confirmButtonText: '<i class="fa fa-bolt"></i> Run Now',
        reverseButtons: true,
        preConfirm: function () {
            var d = parseInt($('#qr_pay_day').val());
            if (!d || d < 1 || d > 28) { Swal.showValidationMessage('Enter a day between 1 and 28'); return false; }
            return d;
        }
    }).then(function (r) {
        if (!r.isConfirmed) return;
        var payDay = r.value;

        Swal.fire({ title: 'Processing…', text: 'Please wait while payroll is being processed.', allowOutsideClick: false, didOpen: function () { Swal.showLoading(); } });

        $.ajax({
            url: '/Payroll/QuickRun', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ payment_day: payDay }),
            success: function (res) {
                Swal.close();
                if (res.success) {
                    LoadPeriods();
                    Swal.fire({ title: 'Payroll Complete!', text: res.message || 'Payroll processed successfully.', icon: 'success' });
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () { Swal.close(); Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// ── Review Payroll (ADMIN) ────────────────────────────────────────────────────
$('#periodsTable').on('click', 'a.review-payroll', function (e) {
    e.preventDefault();
    var id   = $(this).data('id');
    var name = $(this).data('name');
    $('#approve_period_id').val(id);
    $('#approve_period_name').text(name);
    $('#approve_comments').val('');
    $('#approve-payroll-modal').appendTo('body').modal('show');
});

$('#btnApprovePayroll').on('click', function (e) {
    e.preventDefault();
    var id       = $('#approve_period_id').val();
    var comments = $('#approve_comments').val().trim();

    Swal.fire({
        title: 'Approve Payroll?',
        text: 'This will mark the payroll as PROCESSED.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#5cb85c',
        confirmButtonText: 'Yes, Approve',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Payroll/ApprovePayroll', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ period_id: parseInt(id), comments: comments }),
            success: function (res) {
                if (res.success) {
                    $('#approve-payroll-modal').modal('hide');
                    LoadPeriods();
                    Swal.fire('Approved!', res.message, 'success');
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

$('#btnRejectPayroll').on('click', function (e) {
    e.preventDefault();
    var id       = $('#approve_period_id').val();
    var comments = $('#approve_comments').val().trim();

    Swal.fire({
        title: 'Reject Payroll?',
        text: 'This will return the payroll period to OPEN so HR can make corrections.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d9534f',
        confirmButtonText: 'Yes, Reject',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({
            url: '/Payroll/RejectPayroll', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ period_id: parseInt(id), comments: comments }),
            success: function (res) {
                if (res.success) {
                    $('#approve-payroll-modal').modal('hide');
                    LoadPeriods();
                    Swal.fire('Rejected', res.message, 'info');
                } else {
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// ── Helpers ──────────────────────────────────────────────────────────────────
function Fmt(n) {
    return parseFloat(n || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 });
}
