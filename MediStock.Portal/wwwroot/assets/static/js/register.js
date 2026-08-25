// ============================================================
//  register.js — Company self-registration page
// ============================================================
$(document).ready(function () {
    App.init();
    if (typeof FormPlugins !== 'undefined') { FormPlugins.init(); }

    // Eye toggle — show/hide password
    $(document).on('click', '.reg-eye', function () {
        var input = $(this).prev('input');
        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            $(this).removeClass('glyphicon-eye-open').addClass('glyphicon-eye-close');
        } else {
            input.attr('type', 'password');
            $(this).removeClass('glyphicon-eye-close').addClass('glyphicon-eye-open');
        }
    });

    // Password match validation on submit
    $('#registerForm').on('submit', function (e) {
        var pwd     = $('input[name="Password"]').val();
        var confirm = $('input[name="ConfirmPassword"]').val();

        if (pwd.length < 6) {
            e.preventDefault();
            Swal.fire({ icon: 'warning', title: 'Password too short', text: 'Password must be at least 6 characters.', confirmButtonColor: '#348fe2' });
            return;
        }
        if (pwd !== confirm) {
            e.preventDefault();
            Swal.fire({ icon: 'warning', title: 'Passwords do not match', text: 'Please make sure both password fields match.', confirmButtonColor: '#348fe2' });
            return;
        }
        if (this.checkValidity()) {
            $.blockUI({ message: '<h4><img src="/assets/static/img/loading1.gif" /> Registering your organisation, please wait...</h4>' });
        }
    });
});
