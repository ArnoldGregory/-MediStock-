var goalsTable;

$(document).ready(function () {
    goalsTable = $('#goalsTable').dataTable({
        aoColumns: [
            { mRender: function (d, t, r) { return r.full_name || r.employee_name || ''; } },
            { mRender: function (d, t, r) { return r.goal_title || ''; } },
            { mRender: function (d, t, r) { return (r.weight_percent || 0) + '%'; } },
            { mRender: function (d, t, r) { return r.self_rating    ? r.self_rating    + '/5' : '<span class="text-muted">—</span>'; } },
            { mRender: function (d, t, r) { return r.self_comments  || '<span class="text-muted">—</span>'; } },
            { mRender: function (d, t, r) { return r.manager_rating ? r.manager_rating + '/5' : '<span class="text-muted">—</span>'; } },
            { mRender: function (d, t, r) {
                var s = (r.status || '').toUpperCase();
                var cls = s === 'RATED' ? 'success' : s === 'SUBMITTED' ? 'info' : 'default';
                return '<span class="label label-' + cls + '">' + s + '</span>';
            }},
            { mRender: function (d, t, r) {
                if ((r.status || '').toUpperCase() === 'SUBMITTED' || !(r.self_rating)) {
                    return '<a href="javascript:OpenRateModal(' + r.id + ');" class="btn btn-warning btn-xs"><i class="fa fa-star"></i> Rate</a>';
                }
                return '';
            }}
        ]
    });

    // Load employees
    $.get('/Appraisal/GetEmployees', function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        data.forEach(function (e) {
            var name = (e.first_name || '') + ' ' + (e.last_name || e.middle_name || '');
            $('#selEmployee').append('<option value="' + (e.id || e.employee_id) + '">' + name.trim() + '</option>');
        });
    });

    LoadGoals();
});

function LoadGoals() {
    var cycleId = $('#hCycleId').val();
    var empId   = $('#selEmployee').val();
    if (!cycleId) return;

    var url = '/Appraisal/GetGoals?cycle_id=' + cycleId;
    if (empId) url += '&employee_id=' + empId;

    $.get(url, function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        goalsTable.fnClearTable();
        if (data.length) data.forEach(function (r) { goalsTable.fnAddData(r); });
    });
}

function AddGoal(btn) {
    var cycleId = $('#hCycleId').val();
    var empId   = $('#selEmployee').val();
    var title   = $('#txtGoalTitle').val().trim();
    var desc    = $('#txtGoalDesc').val().trim();
    var weight  = $('#txtWeight').val();

    if (!cycleId) { toastr.warning('No cycle selected'); return; }
    if (!empId)   { toastr.warning('Please select an employee'); return; }
    if (!title)   { toastr.warning('Goal title is required'); return; }

    btnLoad(btn, 'Adding...');
    $.ajax({
        url: '/Appraisal/AddGoal', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ cycle_id: parseInt(cycleId), employee_id: parseInt(empId), goal_title: title, goal_description: desc, weight_percent: parseFloat(weight) || 0 }),
        success: function (res) {
            if (res.success) {
                toastr.success('Goal added');
                $('#txtGoalTitle').val(''); $('#txtGoalDesc').val(''); $('#txtWeight').val('0');
                LoadGoals();
            } else { toastr.error(res.message || 'Failed'); }
        },
        error: function () { toastr.error('Request failed'); },
        complete: function () { btnStop(btn); }
    });
}

function OpenRateModal(appraisalId) {
    $('#hAppraisalId').val(appraisalId);
    $('#txtManagerRating').val('');
    $('#txtManagerComments').val('');
    $('#rateModal').modal('show');
}

function SubmitRating(btn) {
    var id      = $('#hAppraisalId').val();
    var rating  = parseFloat($('#txtManagerRating').val());
    var comment = $('#txtManagerComments').val().trim();

    if (!rating || rating < 1 || rating > 5) { toastr.warning('Rating must be between 1 and 5'); return; }

    btnLoad(btn, 'Saving...');
    $.ajax({
        url: '/Appraisal/ManagerRate', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ appraisal_id: parseInt(id), manager_rating: rating, manager_comments: comment }),
        success: function (res) {
            if (res.success) {
                toastr.success('Rating saved');
                $('#rateModal').modal('hide');
                LoadGoals();
            } else { toastr.error(res.message || 'Failed'); }
        },
        error: function () { toastr.error('Request failed'); },
        complete: function () { btnStop(btn); }
    });
}
