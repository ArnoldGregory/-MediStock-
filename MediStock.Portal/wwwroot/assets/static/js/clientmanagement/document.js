
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();

    GetDocument();



    
});




var InitiateEditableDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#editabledatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.RECORD_ID);
                },
                
                "aoColumns": [
                    { "data": "NAME", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "DESCRIPTION", "autoWidth": true, "sDefaultContent": "n/a" },

                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>"
                    },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa-solid fa-trash-can'></i> Delete</a>"
                    }
                ]
            });

            var isEditing = null;

            //Edit
            $('#editabledatatable').on("click", 'a.edit', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                if (isEditing !== null && isEditing != nRow) {
                    editRow(oTable, nRow);
                    isEditing = nRow;
                } else {
                    editRow(oTable, nRow);
                    isEditing = nRow;
                }
            });

            function editRow(oTable, nRow) {
                var aData = oTable.fnGetData(nRow);
                var jqTds = $('>td', nRow);

                var json = JSON.parse(JSON.stringify(aData));

                $('.modal-body #recordid').val($(nRow).attr("recid"));

                $('.modal-body #document_name').val(json["NAME"]);
                $('.modal-body #description').val(json["DESCRIPTION"]);

                $("#capture-record").appendTo("body").modal("show");


            }


        }
    };
}();


function GetDocument() {
    $.get('GetRecords', { module: 'client_document' }, function (data) {
        getData(data);
    });
}



function getData(jsonstring) {
    table = $('#editabledatatable').dataTable();
    oSettings = table.fnSettings();
    table.fnClearTable(this);

    var json = $.parseJSON(JSON.stringify(jsonstring));
    //var json = JSON.parse(jsonstring);
    for (var i = 0; i < json.length; i++) {
        var item = json[i];
        table.oApi._fnAddData(oSettings, item);
    }
    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
    table.fnDraw();
}

$('#save').click(function () {


    var a = $(this).closest(".panel");

    var id = document.getElementById('recordid').value;

    var document_name = document.getElementById('document_name').value;
    var description = document.getElementById('description').value;



    var parameters = {

        id: id,
        document_name: document_name,
        description: description

    };

    $.ajax({
        url: "/ClientManagement/CreateDocument",
        type: "POST",
        data: parameters,
        beforeSend: function () {
            if (!$(a).hasClass("panel-loading")) {
                var t = $(a).find(".panel-body"),
                    i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                $(a).addClass("panel-loading"), $(t).prepend(i);
            }
        },
        success: function (data) {
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            if (data == 'Success') {

                $("#capture-record").modal("hide").data("bs.modal", null);
                GetDocument();
                swal.fire({
                    title: "Success",
                    text: data[0].error_desc,
                    icon: "success",
                    confirmButtonText: "Ok"
                });
            } else {
                Swal.fire({
                    title: "Failed",
                    text: data[0].error_desc,
                    icon: "error",
                    confirmButtonText: "Ok"
                });
            }

        },
        error: function (xhr, textStatus, errorThrown) {
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            Swal.fire({
                title: "Failed",
                text: "Mapping could not be completed " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
});

$("#capture-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");  
    $('#document_name').val("");
    $('#description').val("");
});