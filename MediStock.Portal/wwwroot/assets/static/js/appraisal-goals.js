var goalsTable;

$(document).ready(function () {
    goalsTable = $('#goalsTable').dataTable({
        aoColumns: [
            { mRender: function (d, t, r) { return r.goal_title || ''; } },
            { mRender: function (d, t, r) { return (r.weight_percent || 0) + '%'; } },
            { mRender: function (d, t, r) { return r.self_rating    ? r.self_rating    + '/5' : '<span class="text-muted">—</span>'; } },
            { mRender: function (d, t, r) { return r.manager_rating ? r.manager_rating + '/5' : '<span class="text-muted">—</span>'; } },
            { mRender: function (d, t, r) {
                var s = (r.status || '').toUpperCase();
                var cls = s === 'RATED' ? 'success' : s === 'SUBMITTED' ? 'info' : 'default';
                return '<span class="label label-' + cls + '">' + s + '</span>';
            }},
            { mRender: function (d, t, r) {
                if ((r.status || '').toUpperCase() === 'PENDING' || !r.self_rating) {
                    return '<a href="javascript:OpenSelfRate(' + r.id + ',\'' + escape(r.goal_title || '') + '\');" class="btn btn-primary btn-xs"><i class="fa fa-pencil"></i> Self-Rate</a>';
                }
                return '<span class="text-muted text-sm">Submitted</span>';
            }}
        ]
    });

    // Load active cycles into selector
    $.get('/Appraisal/GetCycles', function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        data.forEach(function (c) {
            if ((c.status || '').toUpperCase() === 'ACTIVE') {
                $('#selCycle').append('<option value="' + c.id + '">' + (c.cycle_name || '') + '</option>');
            }
        });
        if ($('#selCycle option').length > 1) {
            $('#selCycle option:eq(1)').prop('selected', true);
            LoadMyGoals();
        }
    });
});

function LoadMyGoals() {
    var cycleId = $('#selCycle').val();
    if (!cycleId) return;

    $.get('/Appraisal/GetGoals?cycle_id=' + cycleId, function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        goalsTable.fnClearTable();
        if (data.length) data.forEach(function (r) { goalsTable.fnAddData(r); });
    });
}

function OpenSelfRate(appraisalId, title) {
    $('#hAppraisalId').val(appraisalId);
    $('#lblGoalTitle').text(unescape(title));
    $('#txtSelfRating').val('');
    $('#txtSelfComments').val('');
    $('#selfRateModal').modal('show');
}

function SubmitSelf(btn) {
    var id      = $('#hAppraisalId').val();
    var rating  = parseFloat($('#txtSelfRating').val());
    var comment = $('#txtSelfComments').val().trim();

    if (!rating || rating < 1 || rating > 5) { toastr.warning('Rating must be between 1 and 5'); return; }
    if (!comment) { toastr.warning('Please add a comment describing your performance'); return; }

    btnLoad(btn, 'Submitting...');
    $.ajax({
        url: '/Appraisal/SelfRate', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ appraisal_id: parseInt(id), self_rating: rating, self_comments: comment }),
        success: function (res) {
            if (res.success) {
                toastr.success('Self-assessment submitted');
                $('#selfRateModal').modal('hide');
                LoadMyGoals();
            } else { toastr.error(res.message || 'Failed'); }
        },
        error: function () { toastr.error('Request failed'); },
        complete: function () { btnStop(btn); }
    });
}
