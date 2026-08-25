
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();

    GetLeaves();

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
                        "targets": 10,
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
                        "targets": 11,
                        "render": function (data, type, row, meta) {


                            if (row.status !== 0) {
                                return "<a href='#' class='btn btn-success btn-xs comment'><i class='fas fa-comment'></i> Comment</a>";
                            }


                        }
                    },
                    {
                        "targets": 12,
                        "render": function (data, type, row, meta) {


                            if (row.status === 0) {
                                return "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> View</a>";
                            }


                        }
                    },
                    {
                        "targets":13,
                        "render": function (data, type, row, meta) {


                            if (row.status === 0) {
                                return "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash'></i> Cancel</a>";
                            }

                        }
                    }
                ],
                "aoColumns": [
                    { "data": "employee_name", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "doc_number", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "mobile_number", "autoWidth": true, "sDefaultContent": "n.a" },
                    { "data": "email", "autoWidth": true, "sDefaultContent": "n.a" },
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

        }
    };
}();

function GetLeaves() {
    $.get('GetRecords', { module: 'staffleaves' }, function (data) {
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
    $('#leavetype').val("").trigger("change");
    $('#datefrom').val("");
    $('#dateto').val("");
    $('#reason').val("");
});