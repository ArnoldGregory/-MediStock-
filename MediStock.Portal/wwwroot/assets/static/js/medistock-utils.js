// MediStock Utility Functions (Riziki pattern)

function btnLoad(btn, label) {
    var $b = $(btn);
    $b.data('rz-orig', $b.html()).prop('disabled', true)
      .html('<i class="fa fa-spinner fa-spin m-r-5"></i>' + (label || 'Processing...'));
}

function btnStop(btn) {
    var $b = $(btn);
    $b.prop('disabled', false).html($b.data('rz-orig') || $b.html());
}

function pageBlock(msg) {
    $.blockUI({
        message: '<h5><i class="fa fa-spinner fa-spin m-r-5"></i> ' + (msg || 'Processing...') + '</h5>',
        overlayCSS: { backgroundColor: '#000', opacity: 0.3, zIndex: 9998 },
        css: { border: 'none', padding: '15px', zIndex: 9999 }
    });
}

function pageUnblock() {
    $.unblockUI();
}

function formatDate(d) {
    if (!d) return '-';
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function formatDateTime(d) {
    if (!d) return '-';
    return new Date(d).toLocaleString('en-GB', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

function formatCurrency(amount) {
    return 'KES ' + parseFloat(amount || 0).toLocaleString('en-KE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function ajaxGet(url, success, error) {
    $.ajax({
        url: url, type: 'GET',
        success: success,
        error: error || function (xhr) {
            if (xhr.status === 401) { window.location.href = '/Account/Login'; }
            else { Swal.fire('Error', 'Request failed.', 'error'); }
        }
    });
}

function ajaxPost(url, data, success, error) {
    $.ajax({
        url: url, type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: success,
        error: error || function (xhr) {
            if (xhr.status === 401) { window.location.href = '/Account/Login'; }
            else { Swal.fire('Error', xhr.responseText || 'Request failed.', 'error'); }
        }
    });
}
