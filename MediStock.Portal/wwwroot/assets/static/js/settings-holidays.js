var holidaysTable;

$(document).ready(function () {
    holidaysTable = $('#holidaysTable').dataTable({
        aoColumns: [
            { mRender: function (d, t, r, m) { return m.row + 1; } },
            { mRender: function (d, t, r) { return r.holiday_name || ''; } },
            { mRender: function (d, t, r) { return r.holiday_date ? new Date(r.holiday_date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : ''; } },
            { mRender: function (d, t, r) { return r.holiday_date ? new Date(r.holiday_date).toLocaleDateString('en-GB', { weekday: 'long' }) : ''; } },
            {
                mRender: function (d, t, r) {
                    return '<a href="javascript:DeleteHoliday(' + r.id + ',\'' + (r.holiday_name || '') + '\');" class="btn btn-danger btn-xs"><i class="fa fa-trash"></i></a>';
                }
            }
        ]
    });
    LoadHolidays();
});

function LoadHolidays() {
    $.get('/Settings/GetHolidays', function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        holidaysTable.fnClearTable();
        if (data.length) data.forEach(function (r) { holidaysTable.fnAddData(r); });
    });
}

function AddHoliday(btn) {
    var name = $('#txtHolidayName').val().trim();
    var date = $('#txtHolidayDate').val();
    if (!name) { toastr.warning('Please enter a holiday name'); return; }
    if (!date)  { toastr.warning('Please select a date'); return; }

    btnLoad(btn, 'Saving...');
    $.ajax({
        url: '/Settings/AddHoliday', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ holiday_name: name, holiday_date: date }),
        success: function (res) {
            if (res.success) {
                toastr.success('Holiday added');
                $('#txtHolidayName').val(''); $('#txtHolidayDate').val('');
                LoadHolidays();
            } else { toastr.error(res.message || 'Failed'); }
        },
        error: function () { toastr.error('Request failed'); },
        complete: function () { btnStop(btn); }
    });
}

function DeleteHoliday(id, name) {
    swal({
        title: 'Remove Holiday?',
        text: 'Remove "' + name + '" from the holiday list?',
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, remove it'
    }, function (confirmed) {
        if (!confirmed) return;
        pageBlock('Removing...');
        $.ajax({
            url: '/Settings/DeleteHoliday', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { toastr.success('Holiday removed'); LoadHolidays(); }
                else { toastr.error(res.message || 'Failed'); }
            },
            complete: function () { pageUnblock(); }
        });
    });
}
