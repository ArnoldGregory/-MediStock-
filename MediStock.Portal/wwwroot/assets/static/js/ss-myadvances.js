// ============================================================
//  RIZIKI — ss-myadvances.js  (Employee: My Advances)
// ============================================================
var _advTable;

$(document).ready(function () {
    App.init();
    _advTable = $('#myAdvTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: 'amount', sDefaultContent: '0',
              mRender: function (d) { return 'KES ' + parseFloat(d || 0).toLocaleString('en-KE', { minimumFractionDigits: 2 }); } },
            { data: 'reason',        sDefaultContent: '—' },
            { data: 'status', sDefaultContent: '—',
              mRender: function (d) {
                  var cls = d === 'APPROVED'  ? 'success'
                          : d === 'REJECTED'  ? 'danger'
                          : d === 'PROCESSED' ? 'info'
                          : d === 'RECALLED'  ? 'default'
                          : 'warning';
                  return '<span class="label label-' + cls + '">' + (d || '—') + '</span>';
              }},
            { data: 'reject_reason', sDefaultContent: '—' },
            { data: 'created_on',    sDefaultContent: '—' },
            { data: null, sDefaultContent: '', mRender: function (d, t, row) {
                if (row.status === 'PENDING') {
                    return '<button class="btn btn-xs btn-danger btn-recall-advance" data-id="' + row.id + '">' +
                           '<i class="fa fa-times"></i> Cancel</button>';
                }
                return '';
            }}
        ]
    });
    LoadMyAdvances();

    $('#request-advance-modal').on('hidden.bs.modal', function () {
        $('#adv_amount, #adv_reason').val('');
        $('#adv-request-error').hide();
    });

    // Cancel (Recall) advance
    $(document).on('click', '.btn-recall-advance', function () {
        var reqId = $(this).data('id');
        Swal.fire({
            title: 'Cancel Advance Request?',
            text: 'This will cancel your pending advance request.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, cancel it',
            cancelButtonText: 'No'
        }).then(function (result) {
            if (!result.value) return;
            $.ajax({
                url: '/SelfService/RecallAdvance', type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ request_id: reqId }),
                success: function (res) {
                    if (res.success) {
                        LoadMyAdvances();
                        Swal.fire('Cancelled', res.message, 'success');
                    } else {
                        Swal.fire('Error', res.message, 'error');
                    }
                },
                error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
            });
        });
    });
});

function LoadMyAdvances() {
    $.get('/SelfService/GetMyAdvances', function (data) {
        var t = _advTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

$('#btnSubmitAdvance').on('click', function (e) {
    e.preventDefault();
    var amount = parseFloat($('#adv_amount').val()) || 0;
    if (amount <= 0) { $('#adv-request-error').text('Amount must be greater than 0.').show(); return; }

    $.ajax({
        url: '/SelfService/RequestAdvance', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ amount: amount, reason: $('#adv_reason').val().trim() }),
        success: function (res) {
            if (res.success) {
                $('#request-advance-modal').modal('hide');
                LoadMyAdvances();
                Swal.fire('Submitted!', res.message, 'success');
            } else {
                $('#adv-request-error').text(res.message).show();
            }
        },
        error: function () { $('#adv-request-error').text('Request failed.').show(); }
    });
});
