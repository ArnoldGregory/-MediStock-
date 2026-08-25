
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();
    InitiatePreviousClaimEditableDataTable.init();
    GetApproveClaims();
    


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
                        "targets": 7,
                        "render": function (data, type, row, meta) {
                            if (row.status === '0') {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.status === '1') {
                                return '<span class="label label-primary">Approved</span>';
                            } else if (row.status === '2') {
                                return '<span class="label label-danger">Rejected</span>';
                            }
                        }
                    },

                ],
                "aoColumns": [
                    { "data": "overtime_date", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "CURRENCY_CODE", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "starttime", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "endtime", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "hours_no", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "pay_rate", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "pay", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs view'><i class='fas fa-eye'></i> View</a>"
                    },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs approve'><i class='fa fa-check'></i> Approve</a>"
                    }
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

                $('.modal-body #name').val(json["CUSTOMER_FULL_NAME"]);
                $('.modal-body #phone_number').val(json["MOBILENUMBER"]);
                $('.modal-body #email').val(json["EMAIL_ADDRESS"]);
                $('.modal-body #id_no').val(json["DOCUMENT_NUMBER"]);
                $('.modal-body #kra_pin').val(json["TAX_ID_NUMBER"]);
                $('.modal-body #department').val(json["DEPARTMENT"]);
                $('.modal-body #contract_type').val(json["CONTRACT_TYPE"]);

                var employee_id = json["employee_id"];

                GetClaims(employee_id);

                $("#view-record").appendTo("body").modal("show");
            }

            var isApproval = null;


            //Approve

            $('#editabledatatable').on("click", 'a.approve', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                //console.log($(this).parents('tr').attr("recid"));

                console.log(nRow);

                if (isApproval !== null && isApproval != nRow) {
                    //restoreRow(oTable, isEditing);
                    approvalRow(oTable, nRow);
                    isApproval = nRow;
                } else {
                    approvalRow(oTable, nRow);
                    isApproval = nRow;
                }
            });

            function approvalRow(oTable, nRow) {
                var aData = oTable.fnGetData(nRow);
                var jqTds = $('>td', nRow);

                var json = JSON.parse(JSON.stringify(aData));
                var rec = json["id"];
                console.log(rec);

                $('.modal-body #recordid').val(rec);
                $('.modal-body #status').val(json["approve"]).trigger("change");
                $('.modal-body #comment').val(json["comment"]);


                $("#capture-approval-record").appendTo("body").modal("show");
            }


        }
    };
}();


var InitiatePreviousClaimEditableDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#previousovertimedatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "columnDefs": [
                    {
                        "targets": 7,
                        "render": function (data, type, row, meta) {
                            if (row.status === '0') {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.status === '1') {
                                return '<span class="label label-primary">Approved</span>';
                            } else if (row.status === '2') {
                                return '<span class="label label-danger">Rejected</span>';
                            } 
                        }
                    }

                ],
                "aoColumns": [
                    { "data": "overtime_date", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "CURRENCY_CODE", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "starttime", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "endtime", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "hours_no", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "pay_rate", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "pay", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n/a" },
                ]
            });

           


        }
    };
}();


function GetApproveClaims() {
    $.get('GetRecords', { module: 'holiday_claim_approve' }, function (data) {
        getApproveData(data);
    });
}


function GetClaims(employee_id) {
    $.get('GetRecords', { module: 'previous_holiday_claim', param: employee_id }, function (data) {
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

function getApproveData(jsonstring) {
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

function getData(jsonstring) {
    table = $('#previousovertimedatatable').dataTable();
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

$('#SaveApproval').on("click", function (e) {
    e.preventDefault();

    var a = $(this).closest(".panel");

    var nRow = $(this).parents('tr')[0];
    var record_id = document.getElementById("recordid").value;
    var action_flag = document.getElementById("status").value;
    var comment = document.getElementById("comment").value;
    var parameters = {
        id: record_id,
        module: "holiday_claim_approval",
        action_flag: action_flag,
        comment: comment
    };

    Swal.fire({
        title: "Are you sure?",
        text: "You want to approve this record",
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "Proceed!",
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: "/Employee/EmployeeApprove",
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
                    $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

                    if (data.error_code == '00') {

                        $("#capture-approval-record").modal("hide").data("bs.modal", null);
                        GetApproveClaims()

                        Swal.fire({
                            title: "Approved",
                            text: data.error_desc,
                            icon: "success",
                            confirmButtonText: "Ok"
                        });
                    } else {
                        Swal.fire({
                            title: "Error",
                            text: data,
                            icon: "error",
                            confirmButtonText: "Ok"
                        });
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

                    Swal.fire({
                        title: "Failed",
                        text: "Record could not be approved " + errorThrown,
                        icon: "error",
                        confirmButtonText: "Ok"
                    });
                }
            });
        } else {
            return;
            e.preventDefault();
        }
    });
});

$("#capture-transaction").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");
    $('#claimtype').val("").trigger("change");
    $('#netpay').val("");
    $('#disbursemode').val("").trigger("change");
    $('#datefrom').val("");
    $('#dateto').val("");
    $('#reason').val("");
    $('#currency').val("");
});