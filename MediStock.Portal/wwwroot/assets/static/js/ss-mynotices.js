$(document).ready(function () { LoadMyNotices(); });

function LoadMyNotices() {
    $.get('/NoticeBoard/GetNotices?role=ALL', function (res) {
        var data = res && res.data ? res.data : (Array.isArray(res) ? res : []);
        var container = $('#noticesContainer');
        container.empty();

        if (!data.length) {
            $('#noNotices').show();
            return;
        }
        $('#noNotices').hide();

        data.forEach(function (n) {
            var posted = n.created_on ? new Date(n.created_on).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : '';
            var expiry = n.expiry_date ? '<small class="text-muted"> — Expires ' + new Date(n.expiry_date).toLocaleDateString('en-GB') + '</small>' : '';
            var card = '<div class="col-md-6 m-b-15">' +
                '<div class="panel panel-inverse">' +
                '<div class="panel-heading"><h4 class="panel-title"><i class="fa fa-bullhorn m-r-5"></i>' + (n.title || '') + '</h4></div>' +
                '<div class="panel-body">' +
                '<p>' + (n.content || '') + '</p>' +
                '<small class="text-muted">Posted ' + posted + expiry + '</small>' +
                '</div></div></div>';
            container.append(card);
        });
    });
}
