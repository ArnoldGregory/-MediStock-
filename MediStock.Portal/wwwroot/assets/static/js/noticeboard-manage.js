var noticesTable;

$(document).ready(function () {
    noticesTable = $('#noticesTable').dataTable({
        aoColumns: [
            { mRender: function (d, t, r) { return '<strong>' + (r.title || '') + '</strong>'; } },
            { mRender: function (d, t, r) {
                var map = { ALL: 'Everyone', HR: 'HR Only', EMPLOYEE: 'Employees' };
                return map[r.target_role] || r.target_role || 'Everyone';
            }},
            { mRender: function (d, t, r) { return r.created_on ? new Date(r.created_on).toLocaleDateString('en-GB') : ''; } },
            { mRender: function (d, t, r) { return r.expiry_date ? new Date(r.expiry_date).toLocaleDateString('en-GB') : '<span class="text-muted">No expiry</span>'; } },
            { mRender: function (d, t, r) {
                return '<a href="javascript:DeleteNotice(' + r.id + ');" class="btn btn-danger btn-xs"><i class="fa fa-trash"></i></a>';
            }}
        ]
    });
    LoadNotices();
});

function LoadNotices() {
    $.get('/NoticeBoard/GetNotices', function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        noticesTable.fnClearTable();
        if (data.length) data.forEach(function (r) { noticesTable.fnAddData(r); });
    });
}

function PostNotice(btn) {
    var title   = $('#txtTitle').val().trim();
    var content = $('#txtContent').val().trim();
    var target  = $('#selTarget').val();
    var expiry  = $('#txtExpiry').val() || null;
    if (!title)   { toastr.warning('Title is required'); return; }
    if (!content) { toastr.warning('Message content is required'); return; }

    btnLoad(btn, 'Posting...');
    $.ajax({
        url: '/NoticeBoard/AddNotice', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ title: title, content: content, target_role: target, expiry_date: expiry }),
        success: function (res) {
            if (res.success) {
                toastr.success('Notice posted');
                $('#txtTitle').val(''); $('#txtContent').val('');
                LoadNotices();
            } else { toastr.error(res.message || 'Failed'); }
        },
        error: function () { toastr.error('Request failed'); },
        complete: function () { btnStop(btn); }
    });
}

function DeleteNotice(id) {
    swal({ title: 'Delete Notice?', text: 'This will remove the notice for all users.', type: 'warning', showCancelButton: true, confirmButtonText: 'Yes, delete' }, function (c) {
        if (!c) return;
        pageBlock('Deleting...');
        $.ajax({
            url: '/NoticeBoard/DeleteNotice', type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { toastr.success('Deleted'); LoadNotices(); }
                else { toastr.error(res.message || 'Failed'); }
            },
            complete: function () { pageUnblock(); }
        });
    });
}
