// ============================================================
//  RIZIKI — settings-bank-branches.js
// ============================================================
var _branchesTable;
var _banks = [];

$(document).ready(function () {
    App.init();

    _branchesTable = $('#branchesTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; }},
            { data: 'bank_name',    sDefaultContent: '—' },
            { data: 'branch_code',  sDefaultContent: '—' },
            { data: 'branch_name',  sDefaultContent: '—' },
            { data: 'location',     sDefaultContent: '—' },
            { data: 'created_on',   sDefaultContent: '—' },
            { bSortable: false,
              mRender: function (d, t, row) {
                  return '<a href="#" class="btn btn-info btn-xs m-r-3 edit-branch" '
                    + 'data-id="' + row.id + '" data-bankid="' + (row.bank_id||'') + '" '
                    + 'data-code="' + (row.branch_code||'') + '" data-name="' + (row.branch_name||'') + '" '
                    + 'data-location="' + (row.location||'').replace(/"/g,'&quot;') + '">'
                    + '<i class="fa fa-pencil"></i></a> '
                    + '<a href="#" class="btn btn-danger btn-xs delete-branch" data-id="' + row.id + '">'
                    + '<i class="fa fa-trash"></i></a>';
              }}
        ]
    });

    // Load banks for filter + modal dropdown
    $.get('/Settings/GetBanks', function (data) {
        _banks = data || [];
        _banks.forEach(function (b) {
            $('#filter_bank_id').append('<option value="' + b.id + '">' + b.bank_name + '</option>');
            $('#branch_bank_id').append('<option value="' + b.id + '">' + b.bank_name + '</option>');
        });
        LoadBranches();
    });

    $('#filter_bank_id').on('change', function () {
        LoadBranches($(this).val());
    });

    $('#branch-modal').on('hidden.bs.modal', function () {
        $('#branch_id').val('0');
        $('#branch_bank_id').val('');
        $('#branch_code, #branch_name, #branch_location').val('');
        $('#branch-error').hide();
        $('#branchModalTitle').html('<i class="fa fa-building"></i> Add Branch');
    });
});

function LoadBranches(bankId) {
    var bid = bankId || 0;
    $.get('/Settings/GetBankBranches', { bank_id: bid }, function (data) {
        var t = _branchesTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// Edit
$('#branchesTable').on('click', 'a.edit-branch', function (e) {
    e.preventDefault();
    var $a = $(this);
    $('#branch_id').val($a.data('id'));
    $('#branch_bank_id').val($a.data('bankid'));
    $('#branch_code').val($a.data('code'));
    $('#branch_name').val($a.data('name'));
    $('#branch_location').val($a.data('location'));
    $('#branchModalTitle').html('<i class="fa fa-pencil"></i> Edit Branch');
    $('#branch-modal').modal('show');
});

// Delete
$('#branchesTable').on('click', 'a.delete-branch', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Delete Branch?', text: 'This cannot be undone.', icon: 'warning',
        showCancelButton: true, confirmButtonText: 'Yes, Delete', confirmButtonColor: '#d33', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/Settings/DeleteBankBranch', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadBranches($('#filter_bank_id').val()); Swal.fire('Deleted!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// Save
$('#btnSaveBranch').on('click', function (e) {
    e.preventDefault();
    var id     = parseInt($('#branch_id').val()) || 0;
    var bankId = parseInt($('#branch_bank_id').val()) || 0;
    var code   = $('#branch_code').val().trim();
    var name   = $('#branch_name').val().trim();

    if (!bankId) { $('#branch-error').text('Bank is required.').show(); return; }
    if (!code)   { $('#branch-error').text('Branch code is required.').show(); return; }
    if (!name)   { $('#branch-error').text('Branch name is required.').show(); return; }

    $.ajax({ url: '/Settings/SaveBankBranch', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ id: id, bank_id: bankId, branch_code: code, branch_name: name,
            location: $('#branch_location').val().trim() }),
        success: function (res) {
            if (res.success) {
                $('#branch-modal').modal('hide');
                LoadBranches($('#filter_bank_id').val());
                Swal.fire('Saved!', res.message, 'success');
            } else { $('#branch-error').text(res.message).show(); }
        }, error: function () { $('#branch-error').text('Request failed.').show(); }
    });
});
