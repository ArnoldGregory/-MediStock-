// ============================================================
//  RIZIKI — settings-parameters.js
// ============================================================
var _paramsTable;

$(document).ready(function () {
    App.init();

    _paramsTable = $('#paramsTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: null, sDefaultContent: '—',
              mRender: function (d, t, r, meta) { return meta.row + 1; }},
            { data: 'item_key',   sDefaultContent: '—' },
            { data: 'item_value', sDefaultContent: '—' },
            { data: 'comments',   sDefaultContent: '—' },
            { data: 'created_on', sDefaultContent: '—' },
            { bSortable: false,
              mRender: function (d, t, row) {
                  return '<a href="#" class="btn btn-info btn-xs m-r-3 edit-param" '
                    + 'data-id="' + row.id + '" data-key="' + (row.item_key||'') + '" '
                    + 'data-value="' + (row.item_value||'').replace(/"/g,'&quot;') + '" '
                    + 'data-comments="' + (row.comments||'').replace(/"/g,'&quot;') + '">'
                    + '<i class="fa fa-pencil"></i></a> '
                    + '<a href="#" class="btn btn-danger btn-xs delete-param" data-id="' + row.id + '">'
                    + '<i class="fa fa-trash"></i></a>';
              }}
        ]
    });

    LoadParameters();

    $('#param-modal').on('hidden.bs.modal', function () {
        $('#param_id').val('0');
        $('#param_key, #param_value, #param_comments').val('');
        $('#param-error').hide();
        $('#paramModalTitle').html('<i class="fa fa-wrench"></i> Add Parameter');
    });
});

function LoadParameters() {
    $.get('/Settings/GetParameters', function (data) {
        var t = _paramsTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}

// Edit
$('#paramsTable').on('click', 'a.edit-param', function (e) {
    e.preventDefault();
    var $a = $(this);
    $('#param_id').val($a.data('id'));
    $('#param_key').val($a.data('key'));
    $('#param_value').val($a.data('value'));
    $('#param_comments').val($a.data('comments'));
    $('#paramModalTitle').html('<i class="fa fa-pencil"></i> Edit Parameter');
    $('#param-modal').modal('show');
});

// Delete
$('#paramsTable').on('click', 'a.delete-param', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    Swal.fire({ title: 'Delete Parameter?', text: 'This cannot be undone.', icon: 'warning',
        showCancelButton: true, confirmButtonText: 'Yes, Delete', confirmButtonColor: '#d33', reverseButtons: true
    }).then(function (r) {
        if (!r.isConfirmed) return;
        $.ajax({ url: '/Settings/DeleteParameter', type: 'POST', contentType: 'application/json',
            data: JSON.stringify({ id: id }),
            success: function (res) {
                if (res.success) { LoadParameters(); Swal.fire('Deleted!', res.message, 'success'); }
                else Swal.fire('Error', res.message, 'error');
            }, error: function () { Swal.fire('Error', 'Request failed.', 'error'); }
        });
    });
});

// Save
$('#btnSaveParam').on('click', function (e) {
    e.preventDefault();
    var id  = parseInt($('#param_id').val()) || 0;
    var key = $('#param_key').val().trim();
    var val = $('#param_value').val().trim();

    if (!key) { $('#param-error').text('Key is required.').show(); return; }
    if (!val) { $('#param-error').text('Value is required.').show(); return; }

    $.ajax({ url: '/Settings/SaveParameter', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ id: id, item_key: key, item_value: val, comments: $('#param_comments').val().trim() }),
        success: function (res) {
            if (res.success) {
                $('#param-modal').modal('hide');
                LoadParameters();
                Swal.fire('Saved!', res.message, 'success');
            } else { $('#param-error').text(res.message).show(); }
        }, error: function () { $('#param-error').text('Request failed.').show(); }
    });
});
