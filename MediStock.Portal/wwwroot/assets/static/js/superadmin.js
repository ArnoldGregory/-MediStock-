// ============================================================
//  MediStock — superadmin.js (Riziki BFF pattern)
//  Platform-wide administration (SuperAdmin role only)
// ============================================================

var _pharmTable;
var _userTable;
var _auditTable;

$(document).ready(function () {
    App.init();

    if ($('#pharmaciesTable').length) {
        _pharmTable = InitPharmacyTable();
        LoadPharmacies(_pharmTable);
    }
    if ($('#usersTable').length) {
        _userTable = InitUserTable();
        LoadUsers(_userTable);
    }
    if ($('#auditTable').length) {
        _auditTable = InitAuditTable();
        LoadAudit(_auditTable);
    }
});

// ── Pharmacies ───────────────────────────────────────────────────────────────
function InitPharmacyTable() {
    return $('#pharmaciesTable').dataTable({
        responsive: true,
        createdRow: function (row, data) { $(row).attr('recid', data.id); },
        aoColumns: [
            { data: 'name', autoWidth: true, sDefaultContent: '—' },
            {
                data: null,
                autoWidth: true,
                sDefaultContent: '—',
                render: function (d) {
                    var n = (d.owner_first_name || '') + ' ' + (d.owner_last_name || '');
                    return (n.trim() ? n.trim() : '-') + (d.owner_email ? ' <small class="text-muted">(' + d.owner_email + ')</small>' : '');
                }
            },
            {
                data: null,
                autoWidth: true,
                sDefaultContent: '—',
                render: function (d) {
                    return (d.phone || '-') + (d.email ? ' <small class="text-muted">' + d.email + '</small>' : '');
                }
            },
            { data: 'user_count', autoWidth: true, sDefaultContent: '0' },
            { data: 'subscription_plan', autoWidth: true, sDefaultContent: 'Starter' },
            { data: 'created_on', autoWidth: true, sDefaultContent: '—', render: function (d) { return d ? formatDate(d) : '—'; } },
            {
                data: 'is_active',
                autoWidth: true,
                sDefaultContent: 'Active',
                render: function (d) { return (d == 1 || d === true) ? '<span class="label label-success">Active</span>' : '<span class="label label-default">Inactive</span>'; }
            },
            {
                data: null,
                bSortable: false,
                sDefaultContent: '',
                render: function (d) {
                    var active = (d.is_active == 1 || d.is_active === true);
                    var label = active ? 'Deactivate' : 'Activate';
                    var btnClass = active ? 'btn-danger' : 'btn-success';
                    return '<a href="#" class="btn btn-xs ' + btnClass + ' toggle-status" data-active="' + (active ? 1 : 0) + '"><i class="fa fa-power-off"></i> ' + label + '</a>';
                }
            }
        ]
    });
}

function LoadPharmacies(oTable) {
    ajaxGet('/SuperAdmin/GetPharmacies', function (data) {
        var t = oTable || _pharmTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

$('#pharmaciesTable').on('click', 'a.toggle-status', function (e) {
    e.preventDefault();
    var d = _pharmTable.fnGetData($(this).parents('tr')[0]);
    var isActive = ($(this).data('active') === 1) ? true : false;
    var targetActive = !isActive;
    Swal.fire({
        title: targetActive ? 'Activate Pharmacy?' : 'Deactivate Pharmacy?',
        text: 'Set "' + d.name + '" ' + (targetActive ? 'active' : 'inactive') + '?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes',
        reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        ajaxPost('/SuperAdmin/UpdatePharmacyStatus', { id: d.id, is_active: targetActive }, function (res) {
            if (res.success) {
                LoadPharmacies(_pharmTable);
                Swal.fire('Updated', res.message, 'success');
            } else {
                Swal.fire('Error', res.message, 'error');
            }
        });
    });
});

function resetPharmacyForm() {
    $('#pharmacy-modal input').val('');
}

$('#btnAddPharmacy').on('click', function (e) {
    e.preventDefault();
    var name = $('#phName').val().trim();
    var owEmail = $('#owEmail').val().trim();
    var pw = $('#owPassword').val();
    if (!name) { Swal.fire('Validation', 'Pharmacy name is required.', 'warning'); return; }
    if (!owEmail) { Swal.fire('Validation', 'Owner email is required.', 'warning'); return; }
    if (!pw || pw.length < 6) { Swal.fire('Validation', 'Password must be at least 6 characters.', 'warning'); return; }

    var payload = {
        name: name,
        slug: $('#phSlug').val().trim() || null,
        phone: $('#phPhone').val().trim() || null,
        email: $('#phEmail').val().trim() || null,
        address: $('#phAddress').val().trim() || null,
        license_number: $('#phLicense').val().trim() || null,
        currency: 'KES',
        owner_first_name: $('#owFirst').val().trim() || null,
        owner_last_name: $('#owLast').val().trim() || null,
        owner_email: owEmail,
        owner_mobile: $('#owMobile').val().trim() || null,
        password: pw
    };

    var btn = this;
    btnLoad(btn, 'Creating...');
    ajaxPost('/SuperAdmin/AddPharmacy', payload, function (res) {
        btnStop(btn);
        if (res.success) {
            $('#pharmacy-modal').modal('hide');
            LoadPharmacies(_pharmTable);
            Swal.fire('Created', res.message, 'success');
        } else {
            Swal.fire('Error', res.message, 'error');
        }
    }, function () { btnStop(btn); Swal.fire('Error', 'Request failed.', 'error'); });
});

// ── Users ────────────────────────────────────────────────────────────────────
function InitUserTable() {
    return $('#usersTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: 'pharmacy_name', autoWidth: true, sDefaultContent: '—' },
            { data: null, autoWidth: true, sDefaultContent: '—', render: function (d) { return (d.first_name || '') + ' ' + (d.last_name || ''); } },
            { data: 'email', autoWidth: true, sDefaultContent: '—' },
            { data: 'role_name', autoWidth: true, sDefaultContent: '—' },
            { data: 'mobile', autoWidth: true, sDefaultContent: '—' },
            { data: 'created_on', autoWidth: true, sDefaultContent: '—', render: function (d) { return d ? formatDate(d) : '—'; } },
            {
                data: 'is_deleted',
                autoWidth: true,
                sDefaultContent: 'Active',
                render: function (d) {
                    var active = (d == 0 || d === false || d === null || d === undefined);
                    return active ? '<span class="label label-success">Active</span>' : '<span class="label label-default">Deleted</span>';
                }
            }
        ]
    });
}

function LoadUsers(oTable) {
    ajaxGet('/SuperAdmin/GetUsers', function (data) {
        var t = oTable || _userTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// ── Audit ────────────────────────────────────────────────────────────────────
function InitAuditTable() {
    return $('#auditTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: 'created_on', autoWidth: true, sDefaultContent: '—', render: function (d) { return d ? formatDateTime(d) : '—'; } },
            { data: 'user_name', autoWidth: true, sDefaultContent: '—' },
            { data: 'action_type', autoWidth: true, sDefaultContent: '—' },
            { data: 'action_description', autoWidth: true, sDefaultContent: '—' },
            { data: 'page_accessed', autoWidth: true, sDefaultContent: '—' },
            { data: 'client_ip_address', autoWidth: true, sDefaultContent: '—' }
        ]
    });
}

function LoadAudit(oTable) {
    ajaxGet('/SuperAdmin/GetAudit', function (data) {
        var t = oTable || _auditTable;
        var s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) {
            for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        }
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}
