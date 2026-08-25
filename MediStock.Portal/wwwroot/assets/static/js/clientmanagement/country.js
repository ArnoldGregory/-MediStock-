
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();

    GetCountry();



    
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
                    { "data": "COUNTRY", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "COUNTRY_CODE", "autoWidth": true, "sDefaultContent": "n/a" },

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

                $('.modal-body #country_name').val(json["COUNTRY"]);
                $('.modal-body #counrty_code').val(json["COUNTRY_CODE"]);

                $("#capture-record").appendTo("body").modal("show");


            }


        }
    };
}();


function GetCountry() {
    $.get('GetRecords', { module: 'client_country' }, function (data) {
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

    var country_name = document.getElementById('country_name').value;
    var counrty_code = document.getElementById('counrty_code').value;



    var parameters = {

        id: id,
        country_name: country_name,
        counrty_code: counrty_code

    };


    $.ajax({
        url: "/ClientManagement/CreateCountry",
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
                GetCountry();
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

            GetCustomers();
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
    $('#country_name').val("");
    $('#counrty_code').val("");
});