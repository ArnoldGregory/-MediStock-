
$(document).ready(function () {
    App.init();

    InitiateEditableDataTable.init();

    GetWellFareRecords();

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
                    { "data": "CUSTOMER_FULL_NAME", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "MOBILENUMBER", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "EMAIL_ADDRESS", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs view'><i class='fas fa-eye'></i> View</a>"
                    },
                ]
            });

            var isView = null;

            //View
            $('#editabledatatable').on("click", 'a.view', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                //console.log($(this).parents('tr').attr("recid"));

                //console.log(nRow);

                if (isView !== null && isView != nRow) {
                    //restoreRow(oTable, isEditing);
                    viewRow(oTable, nRow);
                    isView = nRow;
                } else {
                    viewRow(oTable, nRow);
                    isView = nRow;
                }
            });

            function viewRow(oTable, nRow) {
                var aData = oTable.fnGetData(nRow);
                var jqTds = $('>td', nRow);

                var json = JSON.parse(JSON.stringify(aData));
                console.log(json);
                $('.modal-body #recordid').val($(nRow).attr("recid"));

                $('.modal-body #customerName').val(json["CUSTOMER_FULL_NAME"]);
                $('.modal-body #mobileNumber').val(json["MOBILENUMBER"]);
                $('.modal-body #emailAddress').val(json["EMAIL_ADDRESS"]);
                $('.modal-body #address').val(json["PHYSICAL_ADDRESS"]);
                $('.modal-body #sexe').val(json["sex"]);
                $('.modal-body #contact_type').val(json["contract"]);
                $('.modal-body #badge_id').val(json["BADGE_ID"]);
                $('.modal-body #department').val(json["department_type"]);
                $('.modal-body #nhif_no').val(json["NHIF_NUMBER"]);
                $('.modal-body #nssf_no').val(json["NSSF_NUMBER"]);
                //$('.modal-body #contract_type').val(json["CONTRACT_TYPE"]);
                //$('.modal-body #pay_type').val(json["PAY_TYPE"]);
                //$('.modal-body #reason').val(json["reason"]);

                var cif = json["cif"];
                //var id = json["id"];

                //GetClaims(cif);
                //GetClaimsDocuments(id);


                $("#view-record").appendTo("body").modal("show");
            }

        }
    };
}();





function GetWellFareRecords() {
    $.get('/Welfare/GetRecords', { module: 'welfare_members' }, function (data) {
        console.log(data);
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


$("#capture-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");
    $('#currency').val("");
    $('#overtimedate').val("");
    $('#start_time_input').val("");
    $('#end_time_input').val("");
    $('#reason').val("");
});