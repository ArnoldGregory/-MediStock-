
$(document).ready(function () {
    App.init();

    InitiateEditableDataTable.init();

    InitiatePreviousAdvanceDataTable.init()

    GetAdvance();


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
                        "targets": 6,
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
                    }

                ],
                "aoColumns": [
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "advance_type", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "formatted_amount", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "payment_mode", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "reason", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n.a" },
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

                $('.modal-body #name').val(json["name"]);
                $('.modal-body #phone_number').val(json["MOBILENUMBER"]);
                $('.modal-body #email').val(json["EMAIL_ADDRESS"]);
                $('.modal-body #advance_amount').val(json["amount"]);
                $('.modal-body #advance_type').val(json["advance_type"]);
                $('.modal-body #payment_mode').val(json["payment_mode"]);
                $('.modal-body #id_no').val(json["DOCUMENT_NUMBER"]);
                $('.modal-body #kra_pin').val(json["TAX_ID_NUMBER"]);
                $('.modal-body #department').val(json["DEPARTMENT"]);
                $('.modal-body #contract_type').val(json["CONTRACT_TYPE"]);
                $('.modal-body #reason').val(json["reason"]);

                var cif = json["cif"];
                console.log(cif);
                GetPreviousAdvance(cif)

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

var InitiatePreviousAdvanceDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#previousAdvancedatatable').dataTable({
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
                    }

                ],
                "aoColumns": [
                    { "data": "advance_type", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "formatted_amount", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "payment_mode", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "reason", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n.a" },
                ]
            });


        }
    };
}();

function GetAdvance() {
    $.get('/Salary/GetRecords', { module: 'staff_advance_details' }, function (data) {
        getData(data);
    });
}

function GetPreviousAdvance(created_by) {
    $.get('GetRecords', {
        module: 'previous_advance_history', param: created_by
    }, function (data) {
        getpreAdvanceData(data);
    });
}

function getpreAdvanceData(jsonstring) {
    table = $('#previousAdvancedatatable').dataTable();
    oSettings = table.fnSettings();
    table.fnClearTable(this);

    var json = $.parseJSON(JSON.stringify(jsonstring));
    console.log(json);
    //var json = JSON.parse(jsonstring);
    for (var i = 0; i < json.length; i++) {
        var item = json[i];
        table.oApi._fnAddData(oSettings, item);
    }
    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
    table.fnDraw();
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


$('#SaveApproval').on("click", function (e) {
    e.preventDefault();
    var a = $(this).closest(".panel");

    var nRow = $(this).parents('tr')[0];
    var record_id = document.getElementById("recordid").value;
    var action_flag = document.getElementById("status").value;
    var comment = document.getElementById("comment").value;
    var parameters = {
        id: record_id,
        module: "advance_approval",
        action_flag: action_flag,
        comment: comment
    };
    console.log(parameters);

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
                        GetLeaves();

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


$("#capture-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");
    $('#advancetype').val("").trigger("change");
    $('#netpay').val("");
    $('#disbursemode').val("").trigger("change");
    $('#reason').val("");

});