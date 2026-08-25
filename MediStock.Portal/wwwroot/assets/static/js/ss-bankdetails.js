// ============================================================
//  RIZIKI — ss-bankdetails.js  (Self-Service: Payment Details)
// ============================================================

$(document).ready(function () {
    App.init();
    LoadBankOptions();
    LoadPaymentDetails();

    // payment method toggle
    $('input[name="ss_payment_method"]').on('change', function () {
        ToggleSsPaymentMethod();
    });

    $('#ss_bank_id').select2({ placeholder: '-- Select Bank --' });

    $('#btnSaveBank').on('click', function (e) {
        e.preventDefault();

        var pm = $('input[name="ss_payment_method"]:checked').val() || 'BANK';

        if (pm === 'BANK') {
            if (!$('#ss_bank_id').val()) {
                Swal.fire('Required', 'Please select your bank.', 'warning'); return;
            }
            if (!$('#ss_bank_account_no').val().trim()) {
                Swal.fire('Required', 'Account number is required.', 'warning'); return;
            }
        } else {
            if (!$('#ss_mobile_money_number').val().trim()) {
                Swal.fire('Required', 'M-Pesa number is required.', 'warning'); return;
            }
        }

        Swal.fire({
            title: 'Save payment details?',
            text: 'This will update how your salary is paid.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, save'
        }).then(function (r) {
            if (!r.isConfirmed) return;

            var payload = {
                payment_method:       pm,
                bank_id:              pm === 'BANK' ? (parseInt($('#ss_bank_id').val()) || 0) : 0,
                bank_account_no:      pm === 'BANK' ? $('#ss_bank_account_no').val().trim() : '',
                bank_branch:          pm === 'BANK' ? $('#ss_bank_branch').val().trim() : '',
                mobile_money_number:  pm === 'MOBILE_MONEY' ? $('#ss_mobile_money_number').val().trim() : ''
            };

            $.ajax({
                url: '/SelfService/UpdateBankDetails', type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (res) {
                    if (res.success) Swal.fire('Saved!', res.message, 'success');
                    else Swal.fire('Error', res.message, 'error');
                },
                error: function () { Swal.fire('Error', 'Request failed. Please try again.', 'error'); }
            });
        });
    });
});

function ToggleSsPaymentMethod() {
    var pm = $('input[name="ss_payment_method"]:checked').val();
    if (pm === 'MOBILE_MONEY') {
        $('#ss_bank_section').hide();
        $('#ss_mobile_section').show();
    } else {
        $('#ss_bank_section').show();
        $('#ss_mobile_section').hide();
    }
}

function LoadBankOptions() {
    $.get('/SelfService/GetBanks', function (data) {
        var sel = $('#ss_bank_id');
        sel.find('option:not(:first)').remove();
        if (data && data.length) {
            $.each(data, function (i, b) {
                sel.append('<option value="' + b.id + '">' + b.bank_name + (b.abbreviation ? ' (' + b.abbreviation + ')' : '') + '</option>');
            });
            sel.trigger('change');
        }
    });
}

function LoadPaymentDetails() {
    $.get('/SelfService/GetMyProfile', function (data) {
        if (data) {
            var pm = (data.payment_method || 'BANK').toUpperCase();

            // set radio
            if (pm === 'MOBILE_MONEY') {
                $('#ss_pm_mpesa').prop('checked', true);
                $('#lbl_ss_mpesa').addClass('active');
                $('#lbl_ss_bank').removeClass('active');
                $('#ss_bank_section').hide();
                $('#ss_mobile_section').show();
                $('#ss_mobile_money_number').val(data.mobile_money_number || '');
            } else {
                $('#ss_pm_bank').prop('checked', true);
                $('#lbl_ss_bank').addClass('active');
                $('#lbl_ss_mpesa').removeClass('active');
                $('#ss_bank_section').show();
                $('#ss_mobile_section').hide();

                // set bank dropdown after options load
                if (data.bank_id && data.bank_id != 0) {
                    var waitForSelect = setInterval(function () {
                        if ($('#ss_bank_id option[value="' + data.bank_id + '"]').length) {
                            $('#ss_bank_id').val(data.bank_id).trigger('change');
                            clearInterval(waitForSelect);
                        }
                    }, 100);
                }
                $('#ss_bank_account_no').val(data.bank_account_no || '');
                $('#ss_bank_branch').val(data.bank_branch || '');
            }
        }
        $('#bank-loading').hide();
        $('#bank-form').show();
    }).fail(function () {
        $('#bank-loading').html('<p class="text-danger">Failed to load payment details.</p>');
    });
}
