
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();
    InitiatePreviousLeaveDataTable.init();

    GetLeaves();

   

    //}
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
                                return '<span class="label label-success">Approved</span>';
                            } else if (row.status === 2) {
                                return '<span class="label label-danger">Rejected</span>';
                            } else if (row.status === 3) {
                                return '<span class="label label-default">Cancelled</span>';
                            }
                        }
                    }

                ],
                "aoColumns": [
                    { "data": "leave_type", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "start_date", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "end_date", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "leave_days", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "reason", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n.a" },
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
                $('.modal-body #department').val(json["department_name"]);
                $('.modal-body #contract_type').val(json["contract_type"]);
                $('.modal-body #kra_pin').val(json["TAX_ID_NUMBER"]);
                $('.modal-body #leave_type').val(json["leave_type"]);
                $('.modal-body #days').val(json["leave_days"]);

                var cif = json["created_by"];
                console.log(cif);
                GetPreviousLeave(cif)

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


var InitiatePreviousLeaveDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#previousleavedatatable').dataTable({
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
                    { "data": "leave_type", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "start_date", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "end_date", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "leave_days", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "reason", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n.a" },
                ]
            });


        }
    };
}();



function GetLeaves() {
    $.get('GetRecords', { module: 'leave_approval_request' }, function (data) {
        getData(data);
    });
}

function GetPreviousLeave(created_by) {
    $.get('GetRecords', { module: 'previous_leave_request', param: created_by
        }, function (data) {
        getpreLeaveData(data);
    });
}

function getpreLeaveData(jsonstring) {
    table = $('#previousleavedatatable').dataTable();
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
    console.log(json);
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
    var status = document.getElementById("status").value;
    var comment = document.getElementById("comment").value;
    var parameters = {
        id: record_id,
        module: "leave_approval",
        action_flag: status,
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


$("#capture-approval-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val(""); 
    $('#comment').val("");
    $('#status').val("").trigger("change");
});

