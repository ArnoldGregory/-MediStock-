
$(document).ready(function () {
    App.init();
    GetManualDocuments();
    InitiateDataTableDocuments.init();
});




var InitiateDataTableDocuments = function () {
    return {
        init: function () {
            var oTable = $('#datatable_manual_documents').dataTable({
                "bPaginate": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "columnDefs": [
                    {
                        "targets": 1,
                        "render": function (data, type, row, meta) {
                            return "<a href='" + row.link + "' target='_blank' class='btn btn-info btn-xs download'><i class='fa fa-download'></i> Download</a>";
                        }
                    }
                ],
                "columns": [
                    { "data": "file_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "bSortable": false, "sDefaultContent": "n/a" }
                ]
            });

            function handleAjaxError(xhr, textStatus, error) {
                if (textStatus === 'timeout') {
                    Swal.fire({
                        title: "Error",
                        text: "The server took too long to send the data",
                        icon: "error",
                        confirmButtonText: "Ok"
                    });
                }
                else {
                    Swal.fire({
                        title: "Error",
                        text: "An error occurred on the server. Please try again in a minute, if the error persists contact the admin",
                        icon: "error",
                        confirmButtonText: "Ok"
                    });
                }
                oTable.fnProcessingIndicator(false);
            }
            //oTable.find('thead th').css('width', 'auto');
        }
    };
}();


function GetManualDocuments() {
    $.get('GetRecords', { module: 'manual' }, function (data) {
        getData(data);
    });
}


function getData(jsonstring) {
    table = $('#datatable_manual_documents').dataTable();
    oSettings = table.fnSettings();
    table.fnClearTable(this);

    var json = $.parseJSON(JSON.stringify(jsonstring));
    for (var i = 0; i < json.length; i++) {
        var item = json[i];
        table.oApi._fnAddData(oSettings, item);
    }
    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
    table.fnDraw();
}