$(document).ready(function ()
{

    App.init();

    InitiateEditableDataTable.init();

    GetStaffClaims();

    GetCurrency();


    $('.selectpicker').select2(
    {
        style: 'btn-white',
        size: 5
    });


    $('#datefrom').datetimepicker({
        format: 'DD-MM-YYYY'
    });

    $('#dateto').datetimepicker({
        format: 'DD-MM-YYYY'
    });
});




var InitiateEditableDataTable = function ()
{

    return
    {
        init: function ()
        {
            //Datatable Initiating
            var oTable = $('#editabledatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "columnDefs": [
                    {
                        "targets": 8,
                        "render": function (data, type, row, meta) {
                            if (row.status === 0) {
                                return '<span class="label label-warning"></span>';
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
                        "targets": 9,
                        "render": function (data, type, row, meta) {


                            if (row.status === 0) {
                                return "<a href='#' class='btn btn-info btn-xs edit'> <i class='fa fa-edit'></i> Edit</a>";
                            }


                        }
                    },
                    {
                        "targets": 10,
                        "render": function (data, type, row, meta) {


                            if (row.status === 0) {
                                return "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash'></i> Cancel</a>";
                            }

                        }
                    }

                ],
                "aoColumns": [
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "from_date", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "to_date", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "claim_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "currency", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "amount", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "payment_mode", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": ""
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
                $('.modal-body #reason').val(json["reason"]);
                $('.modal-body #name').val(json["name"]);
                $('.modal-body #datefrom').val(json["from_date"]);
                $('.modal-body #dateto').val(json["to_date"]);
                $('.modal-body #currency').val(json["currency"]).trigger("change");
                $('.modal-body #amount').val(json["amount"]);
                $('.modal-body #claimtype').val(json["type_of_claim"]).trigger("change");

                $('.modal-body #disbursemode').val(json["disbursement_mode"]).trigger("change");

                $("#capture-record").appendTo("body").modal("show");
            }


        }
    };
}();


function GetStaffClaims() {
    $.get('GetRecords', { module: 'claim_details' }, function (data) {
        getData(data);
    });
}

function GetClaimCategories() {
    $.get('GetRecords', { module: 'claim_types' }, function (data) {
        $("#claimtype").get(0).options.length = 0;
        $("#claimtype").get(0).options[0] = new Option("Please Select Claim Category", "-1");

        $.each(data, function (index, item) {
            $("#claimtype").get(0).options[$("#claimtype").get(0).options.length] = new Option(item.type, item.id);
        });

        $("#claimtype").bind("change", function () {
            var str = $("#claimtype option:selected").text();
        });
    });
}


function GetPaymentMode() {
    $.get('GetRecords', { module: 'payment_modes' }, function (data) {
        $("#disbursemode").get(0).options.length = 0;
        $("#disbursemode").get(0).options[0] = new Option("Please Select Mode", "-1");

        $.each(data, function (index, item) {
            $("#disbursemode").get(0).options[$("#disbursemode").get(0).options.length] = new Option(item.PAYMENT_MODE, item.id);
        });

        $("#disbursemode").bind("change", function () {
            var str = $("#disbursemode option:selected").text();
        });
    });
}

function GetCurrency() {
    $.get('GetRecords', { module: 'currencies' }, function (data) {
        $("#currency").get(0).options.length = 0;
        $("#currency").get(0).options[0] = new Option("Please Select Currency", "-1");

        $.each(data, function (index, item) {
            $("#currency").get(0).options[$("#currency").get(0).options.length] = new Option(item.CURRENCY_CODE, item.id);
        });

        $("#currency").bind("change", function () {
            var str = $("#currency option:selected").text();
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
    var claimtype = document.getElementById('claimtype').value;
    var amount = document.getElementById('amount').value;
    var disbursemode = document.getElementById('disbursemode').value;
    var reason = document.getElementById('reason').value;
    var datefrom = document.getElementById('datefrom').value;
    var dateto = document.getElementById('dateto').value;
    var currency = document.getElementById('currency').value;
    var claim_files = document.getElementById("uploadedFiles").innerHTML.trim();

    var parameters = {
        id: id, claimtype: claimtype, amount: amount,
        disbursemode: disbursemode, reason: reason,
        date_from: datefrom, date_to: dateto, currency: currency, claim_files: claim_files
    };

    $.ajax({
        url: "/Salary/CreateClaim",
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

            GetClaims();
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
    $('#claimtype').val("").trigger("change");
    $('#currency').val("").trigger("change");
    $('#disbursemode').val("").trigger("change");
    $('#amount').val("");
    $('#datefrom').val("");
    $('#dateto').val("");
    $('#reason').val("");
    $('#uploadedFiles').val("");
    $('#filelist').val("");
    document.getElementById("uploadedFiles").innerHTML = "";
    document.getElementById("debug-template").remove();
});// JavaScript source code
