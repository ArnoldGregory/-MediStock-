// ============================================================
//  RIZIKI — ss-mydocuments.js  (Employee: My Documents)
// ============================================================
var _docTable;

$(document).ready(function () {
    App.init();
    _docTable = $('#myDocsTable').dataTable({
        responsive: true,
        aoColumns: [
            { data: null, sDefaultContent: '—', mRender: function (d, t, row) {
                return row.document_name || row.file_name || '—';
            }},
            { data: 'document_type',  sDefaultContent: '—' },
            { data: 'created_on',     sDefaultContent: '—' },
            { bSortable: false,
              mRender: function (d, t, row) {
                  var docId = row.id || row.document_id;
                  if (!docId) return '—';
                  return '<a href="/SelfService/DownloadDocument?doc_id=' + docId + '" class="btn btn-default btn-xs">'
                       + '<i class="fa fa-download"></i> Download</a>';
              }}
        ]
    });
    LoadMyDocs();
});

function LoadMyDocs() {
    $.get('/SelfService/GetMyDocuments', function (data) {
        var t = _docTable, s = t.fnSettings();
        t.fnClearTable(true);
        if (data && data.length) for (var i = 0; i < data.length; i++) t.oApi._fnAddData(s, data[i]);
        s.aiDisplay = s.aiDisplayMaster.slice();
        t.fnDraw();
    });
}
