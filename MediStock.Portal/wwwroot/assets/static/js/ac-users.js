// ============================================================
//  RIZIKI — ac-users.js  (Admin: Portal Users Management)
// ============================================================
var _usersTable;
var _rolesMap  = {};   // id -> role_name
var _hrRoleIds = [];   // role IDs whose name contains "HR"

$(document).ready(function () {
    App.init();

    _usersTable = $('#usersTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, row) {
                  return ((row.first_name || '') + ' ' + (row.last_name || '')).trim() || '—';
              }},
            { data: 'email',      sDefaultContent: '—' },
            { data: 'mobile',     sDefaultContent: '—' },
            { data: 'role_id',    sDefaultContent: '—',
              mRender: function (d) { return _rolesMap[d] || ('Role ' + d); }},
            { data: 'is_approved', sDefaultContent: '—',
              mRender: function (d) {
                  var ok = parseInt(d) === 1;
                  return '<span class="label label-' + (ok ? 'success' : 'warning') + '">' + (ok ? 'Yes' : 'Pending') + '</span>';
              }},
            { data: 'is_locked', sDefaultContent: '—',
              mRender: function (d) {
                  var locked = parseInt(d) === 1;
                  return '<span class="label label-' + (locked ? 'danger' : 'success') + '">' + (locked ? 'Locked' : 'Active') + '</span>';
              }},
            { bSortable: false,
              mRender: function (d, t, row) {
                  var btns = '<a href="#" class="btn btn-info btn-xs m-r-3 edit-user" '
                    + 'data-id="'     + row.id            + '" data-fn="' + (row.first_name||'') + '" '
                    + 'data-ln="'     + (row.last_name||'')  + '" data-email="' + (row.email||'') + '" '
                    + 'data-mobile="' + (row.mobile||'')  + '" data-role="' + (row.role_id||'') + '">'
                    + '<i class="fa fa-pencil"></i></a> ';
                  if (parseInt(row.is_approved) !== 1) {
                      btns += '<a href="#" class="btn btn-success btn-xs m-r-3 approve-user" data-id="' + row.id + '">'
                            + '<i class="fa fa-check"></i></a> ';
                  }
                  var locked = parseInt(row.is_locked) === 1;
                  btns += '<a href="#" class="btn btn-' + (locked ? 'default' : 'warning') + ' btn-xs m-r-3 lock-user" '
                        + 'data-id="' + row.id + '" data-locked="' + (locked ? 1 : 0) + '">'
                        + '<i class="fa fa-' + (locked ? 'unlock' : 'lock') + '"></i></a> ';
                  btns += '<a href="#" class="btn btn-danger btn-xs delete-user" data-id="' + row.id + '">'
                        + '<i class="fa fa-trash"></i></a>';
                  return btns;
              }}
        ]
    });

    // Load roles first, then load departments and users
    $.get('/AccessControl/GetRoles', function (roles) {
        (roles || []).forEach(function (r) {
            _rolesMap[r.id] = r.role_name;
            // Track which role IDs are HR
            if ((r.role_name || '').toUpperCase().indexOf('HR') !== -1) {
                _hrRoleIds.push(parseInt(r.id));
            }
            $('#usr_role_id').append('<option value="' + r.id + '">' + r.role_name + '</option>');
        });
        LoadUsers();
    });

    // Load departments into the HR modal select
    $.get('/Departments/GetList', function (depts) {
        (depts || []).forEach(function (d) {
            $('#usr_dept_id').append('<option value="' + d.id + '">' + (d.department_name || d.name || '') + '</option>');
        });
    });

    // Role change → toggle HR employee fields
    $('#usr_role_id').on('change', function () {
        var roleId = parseInt($(this).val());
        var isHR   = _hrRoleIds.indexOf(roleId) !== -1;
        $('#hr-employee-fields').toggle(isHR);
        if (isHR) {
            // Set default hire date to today
            if (!$('#usr_hire_date').val()) {
                $('#usr_hire_date').val(new Date().toISOString().split('T')[0]);
            }
            // Auto-fetch next staff number if blank
            if (!$('#usr_staff_number').val()) {
                $.get('/AccessControl/GetNextStaffNumber', function (num) {
                    $('#usr_staff_number').val(num || 'EMP001');
                });
            }
        }
    });

    // Reset modal on close
    $('#add-user-modal').on('hidden.bs.modal', function () {
        $('#usr_id').val('0');
        $('#usr_first_name, #usr_last_name, #usr_email, #usr_mobile, #usr_password').val('');
        $('#usr_staff_number, #usr_job_title, #usr_hire_date, #usr_salary').val('');
        $('#usr_role_id').val('');
        $('#usr_dept_id').val('0');
        $('#pwd-group').show();
        $('#hr-employee-fields').hide();
        $('#user-error').hide();
        $('#userModalTitle').html('<i class="fa fa-user-plus"></i> Add User');
        ResetSaveBtn();
    });
});

function LoadUsers() {
    $.get('/AccessControl/GetUsers', function (data) {
        var t = _usersTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── Edit ──────────────────────────────────────────────────────────────────────
$('#usersTable').on('click', 'a.edit-user', function (e) {
    e.preventDefault();
    var $a = $(this);
    $('#usr_id').val($a.data('id'));
    $('#usr_first_name').val($a.data('fn'));
    $('#usr_last_name').val($a.data('ln'));
    $('#usr_email').val($a.data('email'));
    $('#usr_mobile').val($a.data('mobile'));
    $('#usr_role_id').val($a.data('role')).trigger('change');
    $('#pwd-group').hide();
    $('#userModalTitle').html('<i class="fa fa-pencil"></i> Edit User');
    $('#add-user-modal').modal('show');
});

// ── Approve ───────────────────────────────────────────────────────────────────
$('#usersTable').on('click', 'a.approve-user', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Approve User?', icon: 'question',
        showCancelButton: true, confirmButtonText: 'Yes, Approve', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/AccessControl/ApproveUser', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadUsers(); Swal.fire('Approved!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// ── Lock / Unlock ──────────────────────────────────────────────────────────────
$('#usersTable').on('click', 'a.lock-user', function (e) {
    e.preventDefault();
    var id     = $(this).data('id');
    var locked = parseInt($(this).data('locked')) === 1;
    var action = locked ? 'Unlock' : 'Lock';
    Swal.fire({ title: action + ' User?', icon: 'warning',
        showCancelButton: true, confirmButtonText: 'Yes, ' + action, reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/AccessControl/LockUser', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id, locked: !locked }),
            success: function (res) {
                if (res.success) { LoadUsers(); Swal.fire('Done!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// ── Delete ─────────────────────────────────────────────────────────────────────
$('#usersTable').on('click', 'a.delete-user', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Delete User?', text: 'This cannot be undone.', icon: 'warning',
        showCancelButton: true, confirmButtonText: 'Yes, Delete', confirmButtonColor: '#d33', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/AccessControl/DeleteUser', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadUsers(); Swal.fire('Deleted!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// ── Save ───────────────────────────────────────────────────────────────────────
$('#btnSaveUser').on('click', function (e) {
    e.preventDefault();
    var id    = parseInt($('#usr_id').val()) || 0;
    var email = $('#usr_email').val().trim();
    var fn    = $('#usr_first_name').val().trim();
    var ln    = $('#usr_last_name').val().trim();
    var roleId = parseInt($('#usr_role_id').val()) || 0;
    var pwd   = $('#usr_password').val();

    if (!fn || !ln) { ShowErr('First and last name are required.'); return; }
    if (!email)     { ShowErr('Email is required.'); return; }
    if (!roleId)    { ShowErr('Role is required.'); return; }
    if (id === 0 && !pwd) { ShowErr('Password is required for new users.'); return; }

    var isHR = _hrRoleIds.indexOf(roleId) !== -1;

    // If editing existing user, use normal update regardless of role
    if (id > 0) {
        SetSaveBtn(true);
        $.ajax({ url: '/AccessControl/SaveUser', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id, first_name: fn, last_name: ln, email: email,
                mobile: $('#usr_mobile').val().trim(), role_id: roleId, password: '' }),
            success: function (res) {
                SetSaveBtn(false);
                if (res.success) { $('#add-user-modal').modal('hide'); LoadUsers(); Swal.fire('Saved!', res.message, 'success'); }
                else ShowErr(res.message);
            }, error: function () { SetSaveBtn(false); ShowErr('Request failed.'); }
        });
        return;
    }

    // New user — HR role goes through SaveHRUser (creates employee record too)
    if (isHR) {
        var hireDate = $('#usr_hire_date').val();
        var salary   = parseFloat($('#usr_salary').val()) || 0;
        if (!hireDate) { ShowErr('Hire date is required for HR users.'); return; }
        if (salary <= 0) { ShowErr('Basic salary is required for HR users.'); return; }

        SetSaveBtn(true);
        $.ajax({ url: '/AccessControl/SaveHRUser', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({
                first_name:    fn,
                last_name:     ln,
                email:         email,
                mobile:        $('#usr_mobile').val().trim(),
                password:      pwd,
                role_id:       roleId,
                department_id: parseInt($('#usr_dept_id').val()) || 0,
                staff_number:  $('#usr_staff_number').val().trim(),
                job_title:     $('#usr_job_title').val().trim(),
                hire_date:     hireDate,
                basic_salary:  salary
            }),
            success: function (res) {
                SetSaveBtn(false);
                if (res.success) { $('#add-user-modal').modal('hide'); LoadUsers(); Swal.fire('Saved!', res.message, 'success'); }
                else ShowErr(res.message);
            }, error: function () { SetSaveBtn(false); ShowErr('Request failed.'); }
        });
        return;
    }

    // New Admin or other non-HR user
    SetSaveBtn(true);
    $.ajax({ url: '/AccessControl/SaveUser', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ id: 0, first_name: fn, last_name: ln, email: email,
            mobile: $('#usr_mobile').val().trim(), role_id: roleId, password: pwd }),
        success: function (res) {
            SetSaveBtn(false);
            if (res.success) { $('#add-user-modal').modal('hide'); LoadUsers(); Swal.fire('Saved!', res.message, 'success'); }
            else ShowErr(res.message);
        }, error: function () { SetSaveBtn(false); ShowErr('Request failed.'); }
    });
});

// ── Helpers ────────────────────────────────────────────────────────────────────
function ShowErr(msg) { $('#user-error').text(msg).show(); }
function SetSaveBtn(loading) {
    var $b = $('#btnSaveUser');
    if (loading) $b.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');
    else ResetSaveBtn();
}
function ResetSaveBtn() {
    $('#btnSaveUser').prop('disabled', false).html('<i class="fa fa-save"></i> Save');
}
