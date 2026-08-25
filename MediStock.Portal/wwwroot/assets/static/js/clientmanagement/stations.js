
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();

    GetStations();



    
});




var InitiateEditableDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#editabledatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                
                "aoColumns": [
                    { "data": "station_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "physical_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "postal_address", "autoWidth": true, "sDefaultContent": "n/a" },

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

                $('.modal-body #station_name').val(json["station_name"]);
                $('.modal-body #physical_address').val(json["physical_address"]);
                $('.modal-body #postal_address').val(json["postal_address"]);

                $("#capture-record").appendTo("body").modal("show");


            }


        }
    };
}();


function GetStations() {
    $.get('GetRecords', { module: 'client_stations' }, function (data) {
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

    var station_name = document.getElementById('station_name').value;
    var physical_address = document.getElementById('physical_address').value;
    var postal_address = document.getElementById('postal_address').value;



    var parameters = {

        id: id,
        station_name: station_name,
        physical_address: physical_address,
        postal_address: postal_address

    };

    $.ajax({
        url: "/ClientManagement/CreateStations",
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
                GetStations();
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
    $('#station_name').val("");
    $('#physical_address').val("");
    $('#postal_address').val("");
});