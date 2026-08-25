// ============================================================
//  RIZIKI — settings-devices.js
//  Biometric device registration + employee fingerprint IDs
// ============================================================
var _devTable;
var _empTable;

$(document).ready(function () {
    App.init();

    // ── Devices table ────────────────────────────────────────
    _devTable = $('#devicesTable').dataTable({
        responsive: true,
        order: [[4, 'asc']],
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; } },
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) { return r.device_name || '—'; } },
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) { return r.device_serial || '—'; } },
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) { return r.location || '—'; } },
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) {
                return r.is_active
                    ? '<span class="label label-success">ACTIVE</span>'
                    : '<span class="label label-default">INACTIVE</span>';
            }},
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) {
                // Show only last 8 chars for security; full key only shown once at registration
                var k = r.api_key || '';
                return '<code>…' + k.slice(-8) + '</code>';
            }},
            { data: null, sDefaultContent: '', mRender: function (d, t, r) {
                return '<a href="javascript:RemoveDevice(' + r.id + ');" class="btn btn-xs btn-danger">'
                     + '<i class="fa fa-trash"></i> Remove</a>';
            }}
        ]
    });

    // ── Employee fingerprint table ───────────────────────────
    _empTable = $('#empTable').dataTable({
        responsive: true,
        order: [[2, 'asc']],
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; } },
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) { return r.staff_number || '—'; } },
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) { return r.employee_name || '—'; } },
            { data: null, sDefaultContent: '—', mRender: function (d, t, r) { return r.department_name || '—'; } },
            { data: null, sDefaultContent: '', mRender: function (d, t, r) {
                var val = r.fingerprint_id || '';
                return '<input type="text" class="form-control input-sm fp-input" style="width:100px;"'
                     + ' data-emp-id="' + r.id + '" value="' + val + '" placeholder="e.g. 001" />';
            }},
            { data: null, sDefaultContent: '', mRender: function (d, t, r) {
                return '<a href="javascript:;" class="btn btn-xs btn-success btn-save-fp" data-emp-id="' + r.id + '">'
                     + '<i class="fa fa-save"></i> Save</a>';
            }}
        ]
    });

    // Save fingerprint on button click (delegate — DataTables redraws rows)
    $('#empTable').on('click', '.btn-save-fp', function () {
        var empId = $(this).data('emp-id');
        var fpVal = $('.fp-input[data-emp-id="' + empId + '"]').val().trim();
        SaveFingerprint(this, empId, fpVal);
    });

    LoadDevices();
    LoadEmployees();
});

// ── Load devices ─────────────────────────────────────────────
function LoadDevices() {
    _devTable.fnClearTable();
    $.get('/Settings/GetDevices', function (data) {
        if (data && data.length) _devTable.fnAddData(data);
    });
}

// ── Load employees ───────────────────────────────────────────
function LoadEmployees() {
    _empTable.fnClearTable();
    $.get('/Settings/GetDeviceEmployees', function (data) {
        if (data && data.length) _empTable.fnAddData(data);
    });
}

// ── Register new device ──────────────────────────────────────
function RegisterDevice(btn) {
    var name   = $('#txtDeviceName').val().trim();
    var serial = $('#txtDeviceSerial').val().trim();
    var loc    = $('#txtDeviceLocation').val().trim();

    if (!name)   { Swal.fire({ icon: 'warning', title: 'Required', text: 'Device name is required.' }); return; }
    if (!serial) { Swal.fire({ icon: 'warning', title: 'Required', text: 'Serial number is required.' }); return; }

    RizikiUtils.setButtonLoading(btn, 'Registering…');

    $.ajax({
        url: '/Settings/RegisterDevice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ device_name: name, device_serial: serial, location: loc }),
        success: function (res) {
            RizikiUtils.resetButton(btn, '<i class="fa fa-save"></i> Register');
            if (res.success) {
                $('#mdAddDevice').modal('hide');
                $('#txtDeviceName, #txtDeviceSerial, #txtDeviceLocation').val('');
                LoadDevices();

                // Show API key (only time it's shown in full)
                var apiKey   = res.data && res.data.api_key ? res.data.api_key : '';
                var punchUrl = window.location.origin + '/api/attendance/punch';
                $('#txtApiKey').val(apiKey);
                $('#txtPunchUrl').text('POST ' + punchUrl);
                $('#mdApiKey').modal('show');
            } else {
                Swal.fire({ icon: 'error', title: 'Error', text: res.message || 'Failed to register device.' });
            }
        },
        error: function () {
            RizikiUtils.resetButton(btn, '<i class="fa fa-save"></i> Register');
            Swal.fire({ icon: 'error', title: 'Error', text: 'An error occurred. Please try again.' });
        }
    });
}

// ── Remove (deactivate) device ───────────────────────────────
function RemoveDevice(id) {
    document.activeElement && document.activeElement.blur();
    Swal.fire({
        title: 'Remove device?',
        text: 'The device will be deactivated and its API key will stop working.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, remove it'
    }).then(function (result) {
        if (!result.isConfirmed) return;
        $.ajax({
            url: '/Settings/DeleteDevice',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) {
                    Swal.fire({ icon: 'success', title: 'Removed', text: res.message, timer: 1500, showConfirmButton: false });
                    LoadDevices();
                } else {
                    Swal.fire({ icon: 'error', title: 'Error', text: res.message || 'Failed.' });
                }
            }
        });
    });
}

// ── Save fingerprint ID for an employee ──────────────────────
function SaveFingerprint(btn, empId, fpVal) {
    RizikiUtils.setButtonLoading(btn, '…');
    $.ajax({
        url: '/Settings/UpdateFingerprintId',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ employee_id: empId, fingerprint_id: fpVal }),
        success: function (res) {
            RizikiUtils.resetButton(btn, '<i class="fa fa-save"></i> Save');
            if (res.success) {
                Swal.fire({ icon: 'success', title: 'Saved', text: res.message, timer: 1200, showConfirmButton: false });
            } else {
                Swal.fire({ icon: 'error', title: 'Error', text: res.message || 'Failed.' });
            }
        },
        error: function () {
            RizikiUtils.resetButton(btn, '<i class="fa fa-save"></i> Save');
            Swal.fire({ icon: 'error', title: 'Error', text: 'Request failed.' });
        }
    });
}

// ── Copy API key to clipboard ────────────────────────────────
function CopyApiKey() {
    var input = document.getElementById('txtApiKey');
    input.select();
    document.execCommand('copy');
    Swal.fire({ icon: 'success', title: 'Copied!', timer: 1000, showConfirmButton: false });
}
