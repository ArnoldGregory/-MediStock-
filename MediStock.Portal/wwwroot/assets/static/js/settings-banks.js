// ============================================================
//  RIZIKI — settings-banks.js
// ============================================================
var _banksTable;

$(document).ready(function () {
    App.init();

    _banksTable = $('#banksTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; }},
            { data: 'bank_code',    sDefaultContent: '—' },
            { data: 'bank_name',    sDefaultContent: '—' },
            { data: 'abbreviation', sDefaultContent: '—' },
            { data: 'created_on',   sDefaultContent: '—' },
            { bSortable: false,
              mRender: function (d, t, row) {
                  return '<a href="#" class="btn btn-info btn-xs m-r-3 edit-bank" '
                    + 'data-id="' + row.id + '" data-code="' + (row.bank_code||'') + '" '
                    + 'data-name="' + (row.bank_name||'') + '" data-abbrev="' + (row.abbreviation||'') + '">'
                    + '<i class="fa fa-pencil"></i></a> '
                    + '<a href="#" class="btn btn-danger btn-xs delete-bank" data-id="' + row.id + '">'
                    + '<i class="fa fa-trash"></i></a>';
              }}
        ]
    });

    LoadBanks();

    $('#bank-modal').on('hidden.bs.modal', function () {
        $('#bank_id').val('0');
        $('#bank_code, #bank_name, #bank_abbrev').val('');
        $('#bank-error').hide();
        $('#bankModalTitle').html('<i class="fa fa-university"></i> Add Bank');
    });
});

function LoadBanks() {
    $.get('/Settings/GetBanks', function (data) {
        var t = _banksTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// Edit
$('#banksTable').on('click', 'a.edit-bank', function (e) {
    e.preventDefault();
    var $a = $(this);
    $('#bank_id').val($a.data('id'));
    $('#bank_code').val($a.data('code'));
    $('#bank_name').val($a.data('name'));
    $('#bank_abbrev').val($a.data('abbrev'));
    $('#bankModalTitle').html('<i class="fa fa-pencil"></i> Edit Bank');
    $('#bank-modal').modal('show');
});

// Delete
$('#banksTable').on('click', 'a.delete-bank', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Delete Bank?', text: 'This will fail if branches exist.', icon: 'warning',
        showCancelButton: true, confirmButtonText: 'Yes, Delete', confirmButtonColor: '#d33', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/Settings/DeleteBank', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadBanks(); Swal.fire('Deleted!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// Save
$('#btnSaveBank').on('click', function (e) {
    e.preventDefault();
    var id   = parseInt($('#bank_id').val()) || 0;
    var code = $('#bank_code').val().trim();
    var name = $('#bank_name').val().trim();

    if (!code) { $('#bank-error').text('Bank code is required.').show(); return; }
    if (!name) { $('#bank-error').text('Bank name is required.').show(); return; }

    $.ajax({ url: '/Settings/SaveBank', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ id: id, bank_code: code, bank_name: name, abbreviation: $('#bank_abbrev').val().trim() }),
        success: function (res) {
            if (res.success) {
                $('#bank-modal').modal('hide');
                LoadBanks();
                Swal.fire('Saved!', res.message, 'success');
            } else { $('#bank-error').text(res.message).show(); }
        }, error: function () { $('#bank-error').text('Request failed.').show(); }
    });
});
