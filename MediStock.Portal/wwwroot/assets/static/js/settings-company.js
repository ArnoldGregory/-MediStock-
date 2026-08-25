// ============================================================
//  RIZIKI — settings-company.js
// ============================================================
$(document).ready(function () {
    App.init();
    LoadCompany();

    $('#btnSaveCompany').on('click', function (e) {
        e.preventDefault();
        var name = $('#co_name').val().trim();
        if (!name) { $('#co-error').text('Company name is required.').show(); return; }
        $('#co-error').hide();

        var btn = this;
        btnLoad(btn, 'Saving...');
        $.ajax({
            url: '/Settings/SaveCompany', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({
                company_name:    name,
                company_email:   $('#co_email').val().trim(),
                company_phone:   $('#co_phone').val().trim(),
                company_address: $('#co_address').val().trim(),
                kra_pin:         $('#co_kra').val().trim(),
                nssf_no:         $('#co_nssf').val().trim(),
                nhif_no:         $('#co_nhif').val().trim()
            }),
            success: function (res) {
                if (res.success) Swal.fire('Saved!', res.message, 'success');
                else $('#co-error').text(res.message).show();
            },
            error: function () { $('#co-error').text('Request failed.').show(); },
            complete: function () { btnStop(btn); }
        });
    });
});

function LoadCompany() {
    $('#co-loading').show(); $('#co-form').hide();
    $.get('/Settings/GetCompany', function (data) {
        $('#co-loading').hide();
        if (!data) { $('#co-form').show(); return; }
        $('#co_name').val(data.company_name    || '');
        $('#co_email').val(data.company_email  || '');
        $('#co_phone').val(data.company_phone  || '');
        $('#co_address').val(data.company_address || '');
        $('#co_kra').val(data.kra_pin          || '');
        $('#co_nssf').val(data.nssf_no         || '');
        $('#co_nhif').val(data.nhif_no         || '');
        $('#co-form').show();
    }).fail(function () {
        $('#co-loading').hide();
        $('#co-form').show();
    });
}
