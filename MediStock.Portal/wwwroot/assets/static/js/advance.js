
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();

    GetAdvance();

    GetAdvanceCategories();

    GetPaymentMode();
    
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
                "columnDefs": [
                    {
                        "targets": 5,
                        "render": function (data, type, row, meta) {
                            if (row.status === 0) {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.status === 1) {
                                return '<span class="label label-primary">Approved</span>';
                            } else if (row.status === 2) {
                                return '<span class="label label-danger">Rejected</span>';
                            } else if (row.status === 3) {
                                return '<span class="label label-default">Cancelled</span>';
                            }

                        }
                    },
                    {
                        "targets": 6,
                        "render": function (data, type, row, meta) {


                            if (row.status === 0) {
                                return "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>";
                            }


                        }
                    },
                    {
                        "targets": 7,
                        "render": function (data, type, row, meta) {

                            if (row.status === 0) {
                                return "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash'></i> Cancel</a>";
                            }

                        }
                    }

                ],
                "aoColumns": [
                    { "data": "advance_type", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "amount", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "payment_mode", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "reason", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n.a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>"
                    },
                    {
                        "bSortable": false,
                        "sDefaultContent": ""
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
                $('.modal-body #amount').val(json["amount"]);
                $('.modal-body #advancetype').val(json["type_of_advance"]).trigger("change");
                $('.modal-body #disbursemode').val(json["disbursement_mode"]).trigger("change");
                $('.modal-body #reason').val(json["reason"]);
                $("#capture-record").appendTo("body").modal("show");
            }


        }
    };
}();

function GetAdvance() {
    $.get('GetRecords', { module: 'advance_details' }, function (data) {
        getData(data);
    });
}

function GetAdvanceCategories() {
    $.get('GetRecords', { module: 'advance_types' }, function (data) {
        $("#advancetype").get(0).options.length = 0;
        $("#advancetype").get(0).options[0] = new Option("Please Select Avance Category", "-1");

        $.each(data, function (index, item) {
            $("#advancetype").get(0).options[$("#advancetype").get(0).options.length] = new Option(item.type, item.id);
        });

        $("#advancetype").bind("change", function () {
            var str = $("#advancetype option:selected").text();
        });
    });
}

function GetPaymentMode() {
    $.get('GetRecords', { module: 'payment_modes' }, function (data) {
        $("#disbursemode").get(0).options.length = 0;
        $("#disbursemode").get(0).options[0] = new Option("Please Select Disbursement Mode", "-1");

        $.each(data, function (index, item) {
            $("#disbursemode").get(0).options[$("#disbursemode").get(0).options.length] = new Option(item.PAYMENT_MODE, item.id);
        });

        $("#disbursemode").bind("change", function () {
            var str = $("#disbursemode option:selected").text();
        });
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
    var advancetype = document.getElementById('advancetype').value;
    var amount = document.getElementById('amount').value;
    var disbursemode = document.getElementById('disbursemode').value;
    var reason = document.getElementById('reason').value;

    var parameters = {
        id: id,
        advancetype: advancetype,
        amount: amount,
        disbursemode: disbursemode,
        reason: reason
    };

    $.ajax({
        url: "/Salary/CreateAdvance",
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

                swal.fire({
                    title: "Success",
                    text: data[0].error_desc,
                    icon: "success",
                    confirmButtonText: "Ok"
                });
            } else {
                Swal.fire({
                    title: "Failed",
                    text: data,
                    icon: "error",
                    confirmButtonText: "Ok"
                });
            }

            GetAdvance();
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
    $('#advancetype').val("").trigger("change");
    $('#netpay').val("");
    $('#amount').val("");
    $('#disbursemode').val("").trigger("change");
    $('#reason').val("");

});