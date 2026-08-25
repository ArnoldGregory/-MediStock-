var cyclesTable;

$(document).ready(function () {
    cyclesTable = $('#cyclesTable').dataTable({
        aoColumns: [
            { mRender: function (d, t, r) { return r.cycle_name || ''; } },
            { mRender: function (d, t, r) { return r.start_date ? new Date(r.start_date).toLocaleDateString('en-GB') : ''; } },
            { mRender: function (d, t, r) { return r.end_date   ? new Date(r.end_date).toLocaleDateString('en-GB')   : ''; } },
            { mRender: function (d, t, r) {
                var s = (r.status || '').toUpperCase();
                var cls = s === 'ACTIVE' ? 'success' : s === 'CLOSED' ? 'default' : 'warning';
                return '<span class="label label-' + cls + '">' + s + '</span>';
            }},
            { mRender: function (d, t, r) {
                var btns = '<a href="/Appraisal/Review?cycle_id=' + r.id + '" class="btn btn-primary btn-xs m-r-5"><i class="fa fa-eye"></i> Review</a>';
                if ((r.status || '').toUpperCase() === 'ACTIVE') {
                    btns += '<a href="javascript:CloseCycle(' + r.id + ',\'' + (r.cycle_name || '') + '\');" class="btn btn-warning btn-xs"><i class="fa fa-lock"></i> Close</a>';
                }
                return btns;
            }}
        ]
    });
    LoadCycles();
});

function LoadCycles() {
    $.get('/Appraisal/GetCycles', function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        cyclesTable.fnClearTable();
        if (data.length) data.forEach(function (r) { cyclesTable.fnAddData(r); });
    });
}

function CreateCycle(btn) {
    var name  = $('#txtCycleName').val().trim();
    var start = $('#txtStartDate').val();
    var end   = $('#txtEndDate').val();
    if (!name)  { toastr.warning('Cycle name is required'); return; }
    if (!start) { toastr.warning('Start date is required');  return; }
    if (!end)   { toastr.warning('End date is required');    return; }
    if (end <= start) { toastr.warning('End date must be after start date'); return; }

    btnLoad(btn, 'Creating...');
    $.ajax({
        url: '/Appraisal/CreateCycle', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ cycle_name: name, start_date: start, end_date: end }),
        success: function (res) {
            if (res.success) {
                toastr.success('Cycle created');
                $('#txtCycleName').val(''); $('#txtStartDate').val(''); $('#txtEndDate').val('');
                LoadCycles();
            } else { toastr.error(res.message || 'Failed to create cycle'); }
        },
        error: function () { toastr.error('Request failed'); },
        complete: function () { btnStop(btn); }
    });
}

function CloseCycle(id, name) {
    swal({
        title: 'Close Cycle?',
        text: 'Close "' + name + '"? No more goals or ratings will be accepted.',
        type: 'warning', showCancelButton: true, confirmButtonText: 'Yes, close it'
    }, function (c) {
        if (!c) return;
        pageBlock('Closing cycle...');
        $.ajax({
            url: '/Appraisal/CloseCycle', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ cycle_id: id }),
            success: function (res) {
                if (res.success) { toastr.success('Cycle closed'); LoadCycles(); }
                else { toastr.error(res.message || 'Failed'); }
            },
            complete: function () { pageUnblock(); }
        });
    });
}
