
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();

    GetLeaves();

    GetLeaveCategories();

    $('.selectpicker').select2({
        style: 'btn-white',
        size: 5
    });

    $('#datefrom').datetimepicker({
        format: 'MM-DD-YYYY', 
        //minDate: moment() // Set minimum date to today's date and time
    });

    $('#dateto').datetimepicker({
        format: 'MM-DD-YYYY',
       // minDate: moment().add(1, 'days') // Set minimum date to one day ahead of today
    });

    
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
                                /*return '<span class="label label-warning">Pending</span>';*/
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.status === 1) {
                                return '<span class="label label-primary">Approved</span>';
                                /*return '<span class="label label-success">Approved</span>';*/
                            } else if (row.status === 2) {
                                return '<span class="label label-danger">Rejected</span>';
                            } else if (row.status === 3) {
                                return '<span class="label label-default">Cancelled</span>';
                            }

                           
                        }
                    },
                    {
                        "targets": 7,
                        "render": function (data, type, row, meta) {


                            if (row.status !== 0) {
                                return "<a href='#' class='btn btn-success btn-xs comment'><i class='fas fa-comment'></i> Comment</a>";
                            }


                        }
                    },
                    {
                        "targets": 8,
                        "render": function (data, type, row, meta) {


                            if (row.status === 0) {
                                return "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>";
                            }


                        }
                    },
                    {
                        "targets":9,
                        "render": function (data, type, row, meta) {


                            if (row.status === 0) {
                                return "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash'></i> Cancel</a>";
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
                    { "data": "leave_status", "autoWidth": true, "sDefaultContent": "n.a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": ""
                    },
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
                $('.modal-body #datefrom').val(json["start_date"]);
                $('.modal-body #dateto').val(json["end_date"]);
                $('.modal-body #leavetype').val(json["leave_type_id"]).trigger("change");

                $("#capture-record").appendTo("body").modal("show");
            }


            var isComment = null;

            //Comment
            $('#editabledatatable').on("click", 'a.comment', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                if (isComment !== null && isComment != nRow) {
                    commentRow(oTable, nRow);
                    isComment = nRow;
                } else {
                    commentRow(oTable, nRow);
                    isComment = nRow;
                }
            });

            function commentRow(oTable, nRow) {
                var aData = oTable.fnGetData(nRow);
                var jqTds = $('>td', nRow);

                var json = JSON.parse(JSON.stringify(aData));
                $('.modal-body #comments').val(json["comments"]);

                $("#capture-comment-record").appendTo("body").modal("show");
            }



            //Delete an Existing Row
            $('#editabledatatable').on("click", 'a.delete', function (e) {
                e.preventDefault();
                var a = $(this).closest(".panel");

                var nRow = $(this).parents('tr')[0];

                var parameters = {
                    id: $(this).parents('tr').attr("recid"),
                    module: 'leave_request'
                };

                console.log(parameters);


                Swal.fire({
                    title: "Are you sure?",
                    text: "You want to cancel this request",
                    icon: "question",
                    showCancelButton: true,
                    confirmButtonText: "Proceed!",
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        //$.blockUI();

                        $.ajax({
                            url: "/Leave/Cancel",
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

                                Swal.fire({
                                    title: "Cancelled",
                                    text: "Request has been cancelled",
                                    icon: "success",
                                    confirmButtonText: "Ok"
                                });
                                console.log('hehehehe');
                                GetLeaves();
                            },
                            error: function (xhr, textStatus, errorThrown) {
                                //$.unblockUI();
                                $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

                                Swal.fire({
                                    title: "Failed",
                                    text: "Request could not be cancelled " + errorThrown,
                                    icon: "error",
                                    confirmButtonText: "Ok",
                                });
                            }
                        });
                    } else {
                        return;
                        e.preventDefault();
                    }
                });
            });


        }
    };
}();

function GetLeaves() {
    $.get('GetRecords', { module: 'leave_schedule_request' }, function (data) {
        getData(data);
    });
}

function GetLeaveCategories() {
    $.get('GetRecords', { module: 'leave_categories' }, function (data) {
        $("#leavetype").get(0).options.length = 0;
        $("#leavetype").get(0).options[0] = new Option("Please Select Leave Category", "-1");

        $.each(data, function (index, item) {
            $("#leavetype").get(0).options[$("#leavetype").get(0).options.length] = new Option(item.leave_type, item.id);
        });

        $("#leavetype").bind("change", function () {
            var str = $("#leavetype option:selected").text();
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
    var leavetype = document.getElementById('leavetype').value;
    var datefrom = document.getElementById('datefrom').value;
    var dateto = document.getElementById('dateto').value;
    var reason = document.getElementById('reason').value;

    console.log(datefrom);
    console.log(dateto);

    var parameters = {
        id: id, leavetype: leavetype, datefrom: datefrom,
        dateto: dateto, reason: reason
    };

    $.ajax({
        url: "/Leave/CreateLeave",
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

            GetLeaves();
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
    $('#leavetype').val("").trigger("change");
    $('#datefrom').val("");
    $('#dateto').val("");
    $('#reason').val("");
});