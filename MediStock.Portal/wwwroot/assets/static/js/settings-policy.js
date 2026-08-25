// ============================================================
//  RIZIKI — settings-policy.js  (Leave & Payroll Policy)
// ============================================================

$(document).ready(function () {
    App.init();
    LoadPolicy();

    $('#btnSavePolicy').on('click', function (e) {
        e.preventDefault();
        SavePolicy(this);
    });
});

function LoadPolicy() {
    $.get('/Settings/GetPolicy', function (data) {
        if (data) {
            $('#pol_annual').val(data.annual_leave_days   || 21);
            $('#pol_sick').val(data.sick_leave_days       || 14);
            $('#pol_maternity').val(data.maternity_leave_days || 90);
            $('#pol_paternity').val(data.paternity_leave_days || 14);
            $('#pol_cf').val(data.carry_forward_limit     || 5);
            $('#pol_ot_rate').val(data.overtime_rate      || 1.50);
            $('#pol_approval_levels').val(data.approval_levels || 1);

            var req = parseInt(data.require_payroll_approval) || 0;
            if (req === 1) {
                $('#pol_approval_yes').prop('checked', true);
                $('#lbl_pol_approval_yes').addClass('active');
                $('#lbl_pol_approval_no').removeClass('active');
            } else {
                $('#pol_approval_no').prop('checked', true);
                $('#lbl_pol_approval_no').addClass('active');
                $('#lbl_pol_approval_yes').removeClass('active');
            }
        }
        $('#pol-loading').hide();
        $('#pol-form').show();
    }).fail(function () {
        $('#pol-loading').html('<p class="text-danger">Failed to load policy settings.</p>');
    });
}

function SavePolicy(btn) {
    var annual    = parseInt($('#pol_annual').val())    || 0;
    var sick      = parseInt($('#pol_sick').val())      || 0;
    var maternity = parseInt($('#pol_maternity').val()) || 0;
    var paternity = parseInt($('#pol_paternity').val()) || 0;

    if (annual < 1 || sick < 1 || maternity < 1 || paternity < 1) {
        Swal.fire('Required', 'Leave entitlement days must be at least 1.', 'warning');
        return;
    }

    var payload = {
        annual_leave_days:        annual,
        sick_leave_days:          sick,
        maternity_leave_days:     maternity,
        paternity_leave_days:     paternity,
        carry_forward_limit:      parseInt($('#pol_cf').val())          || 0,
        overtime_rate:            parseFloat($('#pol_ot_rate').val())   || 1.50,
        approval_levels:          parseInt($('#pol_approval_levels').val()) || 1,
        require_payroll_approval: parseInt($('input[name="pol_payroll_approval"]:checked').val()) || 0
    };

    riziki.setLoading(btn, true);
    $.ajax({
        url: '/Settings/SavePolicy', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            riziki.setLoading(btn, false);
            if (res.success) Swal.fire('Saved!', res.message, 'success');
            else Swal.fire('Error', res.message, 'error');
        },
        error: function () {
            riziki.setLoading(btn, false);
            Swal.fire('Error', 'Request failed. Please try again.', 'error');
        }
    });
}
