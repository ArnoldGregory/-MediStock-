/**
 * leave-yearend.js — Year-end leave carry forward
 */

var _cfTable;

$(document).ready(function () {
    App.init();

    // Populate year dropdown: last 2 years + current year
    var cy = new Date().getFullYear();
    for (var y = cy; y >= cy - 2; y--) {
        $('#cf_year').append('<option value="' + y + '">' + y + '</option>');
    }

    _cfTable = $('#cfLogTable').dataTable({
        bPaginate: false,
        aoColumns: [
            { mRender: function (d, t, r) { return r.processed_year || ''; } },
            { mRender: function (d, t, r) { return r.max_carry_days || '0'; } },
            { mRender: function (d, t, r) {
                return r.processed_on ? new Date(r.processed_on).toLocaleString('en-GB') : '';
            }}
        ]
    });

    LoadCFLog();
});

function LoadCFLog() {
    $.get('/Leave/GetCFLog', function (res) {
        var data = (res && res.data) ? res.data : (Array.isArray(res) ? res : []);
        _cfTable.fnClearTable();
        if (data.length) data.forEach(function (r) { _cfTable.fnAddData(r); });
    });
}

function RunCarryForward(btn) {
    var year   = parseInt($('#cf_year').val());
    var maxDays = parseFloat($('#cf_max').val());

    if (!year)          { toastr.warning('Select a year'); return; }
    if (isNaN(maxDays) || maxDays < 0) { toastr.warning('Enter a valid max carry days'); return; }

    Swal.fire({
        title: 'Run Carry Forward for ' + year + '?',
        text: 'This will create ' + (year + 1) + ' leave balances for all active employees. This cannot be reversed.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Run It',
        reverseButtons: true
    }).then(function (result) {
        if (!result.isConfirmed) return;

        btnLoad(btn, 'Processing...');
        $.ajax({
            url: '/Leave/RunCarryForward', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ year: year, max_carry: maxDays }),
            success: function (res) {
                if (res.success) {
                    Swal.fire('Done!', res.message || 'Carry forward completed', 'success');
                    LoadCFLog();
                } else {
                    Swal.fire('Error', res.message || 'Failed', 'error');
                }
            },
            error: function () { Swal.fire('Error', 'Request failed', 'error'); },
            complete: function () { btnStop(btn); }
        });
    });
}
