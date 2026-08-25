// ============================================================
//  RIZIKI — payroll-run.js  (Run Payroll page)
// ============================================================
var _runTable;

$(document).ready(function () {
    App.init();

    _runTable = $('#runPeriodsTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: 'period_name',  sDefaultContent: '—' },
            { data: 'start_date',   sDefaultContent: '—' },
            { data: 'end_date',     sDefaultContent: '—' },
            { data: 'payment_date', sDefaultContent: '—' },
            { data: 'status', sDefaultContent: '—',
              mRender: function (d) {
                  var s = (d || 'OPEN').toUpperCase();
                  var cls = s === 'PROCESSED' ? 'success' : 'warning';
                  return '<span class="label label-' + cls + '">' + s + '</span>';
              }},
            { bSortable: false,
              mRender: function (d, t, row) {
                  var id     = row.period_id || row.id;
                  var name   = row.period_name || '';
                  var status = (row.status || '').toUpperCase();
                  if (status === 'PROCESSED') {
                      return '<span class="text-muted"><i class="fa fa-check"></i> Processed</span>'
                           + ' &nbsp; <a href="/Payroll/Payslips?period_id=' + id + '" class="btn btn-info btn-xs">'
                           + '<i class="fa fa-list"></i> View Payslips</a>';
                  }
                  return '<a href="#" class="btn btn-warning btn-sm run-payroll-btn" data-id="' + id + '" data-name="' + name + '">'
                       + '<i class="fa fa-play"></i> Run Payroll</a>';
              }}
        ]
    });

    LoadRunPeriods();
});

function LoadRunPeriods() {
    $.get('/Payroll/GetPeriods', function (data) {
        var t = _runTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

$('#runPeriodsTable').on('click', 'a.run-payroll-btn', function (e) {
    e.preventDefault();
    var id   = $(this).data('id');
    var name = $(this).data('name');
    var $btn = $(this);

    Swal.fire({
        title: 'Run Payroll?',
        html: 'This will process payroll for all active employees in period<br><strong>' + name + '</strong>.<br>This cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#f59c1a',
        confirmButtonText: 'Yes, Run Payroll',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;

        Swal.fire({ title: 'Processing…', text: 'Please wait while payroll is being processed.',
            allowOutsideClick: false, didOpen: function () { Swal.showLoading(); } });

        $btn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Processing…');

        $.ajax({
            url: '/Payroll/RunPayroll', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ period_id: id }),
            success: function (res) {
                Swal.close();
                $btn.prop('disabled', false).html('<i class="fa fa-play"></i> Run Payroll');
                if (res.success) {
                    LoadRunPeriods();
                    Swal.fire({ title: 'Payroll Complete!', text: res.message || 'Payroll processed successfully.', icon: 'success' });
                } else {
                    Swal.fire('Error', res.message || 'Payroll failed.', 'error');
                }
            },
            error: function () {
                Swal.close();
                $btn.prop('disabled', false).html('<i class="fa fa-play"></i> Run Payroll');
                Swal.fire('Error', 'Request failed.', 'error');
            }
        });
    });
});

function Fmt(n) {
    return parseFloat(n || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 });
}
