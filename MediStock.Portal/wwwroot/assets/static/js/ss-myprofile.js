// ============================================================
//  RIZIKI — ss-myprofile.js  (Employee: My Profile)
// ============================================================
$(document).ready(function () {
    App.init();
    LoadProfile();

    // Click photo → open file picker
    $('#profilePhotoPreview').on('click', function () {
        $('#photoFileInput').trigger('click');
    });

    // File selected → preview + auto-upload
    $('#photoFileInput').on('change', function () {
        var file = this.files[0];
        if (!file) return;

        // Client-side preview
        var reader = new FileReader();
        reader.onload = function (e) { $('#profilePhotoPreview').attr('src', e.target.result); };
        reader.readAsDataURL(file);

        // Upload
        var form = new FormData();
        form.append('photo', file);

        $('#photo-upload-error').hide();
        Swal.fire({ title: 'Uploading...', allowOutsideClick: false, didOpen: function () { Swal.showLoading(); } });

        $.ajax({
            url: '/SelfService/UploadPhoto',
            type: 'POST',
            data: form,
            contentType: false,
            processData: false,
            success: function (res) {
                Swal.close();
                if (res.success) {
                    Swal.fire('Done!', res.message, 'success');
                    // Update nav/sidebar photo without full reload
                    $('img[src*="profile-pics"]').attr('src', '/assets/static/img/profile-pics/' + res.fileName);
                } else {
                    $('#photo-upload-error').text(res.message).show();
                    Swal.fire('Error', res.message, 'error');
                }
            },
            error: function () {
                Swal.close();
                Swal.fire('Error', 'Upload failed. Please try again.', 'error');
            }
        });
    });
});

function LoadProfile() {
    $.get('/SelfService/GetMyProfile', function (data) {
        if (!data) { $('#profile-loading').html('<p class="text-danger">Failed to load profile.</p>'); return; }
        $('#p_first_name').val(data.first_name || '');
        $('#p_last_name').val(data.last_name || '');
        $('#p_id_number').val(data.id_number || '');
        $('#p_staff_number').val(data.staff_number || '');
        $('#p_department').val(data.department_name || '');
        $('#p_job_title').val(data.job_title || '');
        $('#p_emp_type').val(data.employment_type || '');
        $('#p_hire_date').val(FmtDate(data.hire_date));
        $('#p_email').val(data.email || '');
        $('#p_mobile').val(data.mobile || '');
        $('#profile-loading').hide();
        $('#profile-form').show();
    }).fail(function () {
        $('#profile-loading').html('<p class="text-danger">Failed to load profile. Please refresh the page.</p>');
    });
}

$('#btnSaveProfile').on('click', function (e) {
    e.preventDefault();
    var email  = $('#p_email').val().trim();
    var mobile = $('#p_mobile').val().trim();
    if (!email)  { $('#profile-error').text('Email is required.').show(); return; }
    if (!mobile) { $('#profile-error').text('Mobile is required.').show(); return; }
    $('#profile-error').hide();

    var $btn = $(this);
    $btn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

    $.ajax({
        url: '/SelfService/UpdateProfile', type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email: email, mobile: mobile }),
        success: function (res) {
            $btn.prop('disabled', false).html('<i class="fa fa-save"></i> Save Contact Info');
            if (res.success) {
                Swal.fire('Saved!', res.message, 'success');
            } else {
                $('#profile-error').text(res.message || 'Failed to save.').show();
            }
        },
        error: function () {
            $btn.prop('disabled', false).html('<i class="fa fa-save"></i> Save Contact Info');
            $('#profile-error').text('Request failed. Please try again.').show();
        }
    });
});

function FmtDate(val) {
    if (!val) return '';
    return val.toString().split('T')[0];
}
