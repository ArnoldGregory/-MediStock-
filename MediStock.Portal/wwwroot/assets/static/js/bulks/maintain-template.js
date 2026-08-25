function GetTransactionCodesDropDown() {
    $.get('GetRecords', { module: 'transaction_codes' }, function (data) {
        $("#sendvia").get(0).options.length = 0;
        $("#sendvia").get(0).options[0] = new Option("Please Select Transaction Type", "-1");

        $.each(data, function (index, item) {
            $("#sendvia").get(0).options[$("#sendvia").get(0).options.length] = new Option(item.displaytext, item.code);
        });

        $("#sendvia").bind("change", function () {
            var str = $("#sendvia option:selected").text();
            //console.log($(this).val());
        });
    });
}

var InitiateTemplateEditableDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#editabletemplatedatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "aoColumns": [
                    { "data": "template_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "total_value", "autoWidth": true, "sDefaultContent": "n/a", render: $.fn.dataTable.render.number(',', '.', 2, '') },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs details'><i class='fa fa-list'></i> Template Details</a>"
                    },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-warning btn-xs edit'><i class='fa fa-edit'></i> Edit</a>"
                    },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash-o'></i> Delete</a>"
                    }
                ]
            });

            //Template Details
            $('#editabletemplatedatatable').on("click", 'a.details', function (e) {
                e.preventDefault();
                var a = $(this).closest(".panel");

                var nRow = $(this).parents('tr')[0];
                var aData = oTable.fnGetData(nRow);
                var jqTds = $('>td', nRow);

                var json = JSON.parse(JSON.stringify(aData));

                $('#templateid').val($(nRow).attr("recid"));

                $('#selectedtemplate').text('Showing details for template ' + json["template_name"]);

                var rec = $(this).parents('tr').attr("recid");

                var parameters = { module: 'bulk_payment_template_details', param: rec };

                $.ajax({
                    url: "/BulkPayments/GetRecords",
                    type: "GET",
                    data: parameters,
                    beforeSend: function () {
                        if (!$(a).hasClass("panel-loading")) {
                            var t = $(a).find(".panel-body"),
                                i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                            $(a).addClass("panel-loading"), $(t).prepend(i);
                        }
                    },
                    success: function (data) {
                        table = $('#editabletemplatedetailsdatatable').dataTable();
                        oSettings = table.fnSettings();
                        table.fnClearTable(this);

                        var json = $.parseJSON(JSON.stringify(data));
                        //var json = JSON.parse(jsonstring);
                        for (var i = 0; i < json.length; i++) {
                            var item = json[i];
                            table.oApi._fnAddData(oSettings, item);
                        }
                        oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
                        table.fnDraw();

                        $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
                        Swal.fire({
                            title: "Sorry",
                            text: "Operation could not be completed " + errorThrown,
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                    }
                });
            });

            var isEditing = null;

            //Edit
            $('#editabletemplatedatatable').on("click", 'a.edit', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                if (isEditing !== null && isEditing !== nRow) {
                    //restoreRow(oTable, isEditing);
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
                $('.modal-body #templatename').val(json["template_name"]);
                $('.modal-body #remarks').val(json["remarks"]);

                $("#capture-template").appendTo("body").modal("show");
            }

            //Delete an Existing Row
            $('#editabletemplatedatatable').on("click", 'a.delete', function (e) {
                e.preventDefault();
                var a = $(this).closest(".panel");

                var nRow = $(this).parents('tr')[0];

                var rec = $(this).parents('tr').attr("recid");

                //console.log($(this).parents('tr').attr("recid"));
                Swal.fire({
                    title: "Are you sure?",
                    text: "You want to delete this record?",
                    icon: "question",
                    showCancelButton: true,
                    confirmButtonText: "Proceed",
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        oTable.fnDeleteRow(nRow);

                        $.ajax({
                            url: "/BulkPayments/GetRecords",
                            type: "GET",
                            data: parameters,
                            beforeSend: function () {
                                if (!$(a).hasClass("panel-loading")) {
                                    var t = $(a).find(".panel-body"),
                                        i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                                    $(a).addClass("panel-loading"), $(t).prepend(i);
                                }
                            },
                            success: function (data) {
                                Swal.fire({
                                    title: "Success",
                                    text: "Record has been deleted",
                                    icon: "success",
                                    confirmButtonText: "Ok"
                                });

                                $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
                            },
                            error: function (xhr, textStatus, errorThrown) {
                                $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
                                Swal.fire({
                                    title: "Sorry",
                                    text: "Operation could not be completed " + errorThrown,
                                    icon: "warning",
                                    confirmButtonText: "Ok"
                                });
                            }
                        });
                    }
                });
            });
        }
    };
}();

$('#save').click(function () {
    var a = $(this).closest(".panel");

    var id = document.getElementById('recordid').value;
    var template_name = document.getElementById('templatename').value;
    var remarks = document.getElementById('remarks').value;

    var parameters = {
        id: id,
        template_name: template_name,
        template_module: 'BULK PAYMENT',
        remarks: remarks
    };

    $.ajax({
        url: "/BulkPayments/CreateBulkTemplate",
        type: "GET",
        data: parameters,
        beforeSend: function () {
            if (!$(a).hasClass("panel-loading")) {
                var t = $(a).find(".panel-body"),
                    i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                $(a).addClass("panel-loading"), $(t).prepend(i);
            }
        },
        success: function (data) {

            $("#capture-template").modal("hide").data("bs.modal", null);

            if (data.includes('success')) {
                GetTemplates();
            } else {
                Swal.fire({
                    title: "Sorry",
                    text: data,
                    icon: "error",
                    confirmButtonText: "Ok"
                });
            }

            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

        }
    });
});

$("#capture-template").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");
    $('#templatename').val("");
    $('#remarks').val("");
});

function GetTemplates() {
    $.get('GetRecords', { module: 'bulk_payment_templates', param: 'BULK PAYMENT' }, function (data) {
        table = $('#editabletemplatedatatable').dataTable();
        oSettings = table.fnSettings();
        table.fnClearTable(this);

        var json = $.parseJSON(JSON.stringify(data));
        //var json = JSON.parse(jsonstring);
        for (var i = 0; i < json.length; i++) {
            var item = json[i];
            table.oApi._fnAddData(oSettings, item);
        }
        oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
        table.fnDraw();
    });
}

function GetTemplateDetails(template_id) {
    $.get('GetRecords', { module: 'bulk_payment_template_details', param: template_id }, function (data) {
        table = $('#editabletemplatedetailsdatatable').dataTable();
        oSettings = table.fnSettings();
        table.fnClearTable(this);

        var json = $.parseJSON(JSON.stringify(data));
        //var json = JSON.parse(data);
        for (var i = 0; i < json.length; i++) {
            var item = json[i];
            table.oApi._fnAddData(oSettings, item);
        }
        oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
        table.fnDraw();
    });
}

var InitiateTemplateDetailsEditableDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#editabletemplatedetailsdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "aoColumns": [
                    { "data": "roll_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "names", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "bank", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "branch", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "account_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "phone_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "amount", "autoWidth": true, "sDefaultContent": "n/a", render: $.fn.dataTable.render.number(',', '.', 2, '') },
                    { "data": "send_via", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-warning btn-xs edit'><i class='fa fa-edit'></i> Edit</a>"
                    },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash-o'></i> Delete</a>"
                    }
                ]
            });

            var isEditing = null;

            //Edit
            $('#editabletemplatedetailsdatatable').on("click", 'a.edit', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                if (isEditing !== null && isEditing !== nRow) {
                    //restoreRow(oTable, isEditing);
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

                $('.modal-body #detailrecordid').val($(nRow).attr("recid"));
                $('.modal-body #names').val(json["names"]);
                $('.modal-body #bank').val(json["bank"]);
                $('.modal-body #branch').val(json["branch"]);
                $('.modal-body #account').val(json["account_number"]);
                $('.modal-body #phonenumber').val(json["phone_number"]);
                $('.modal-body #amount').val(json["amount"]);
                $('.modal-body #sendvia').val(json["send_via"]).trigger("change");

                $("#capture-template-detail").appendTo("body").modal("show");
            }

            //Delete an Existing Row
            $('#editabletemplatedetailsdatatable').on("click", 'a.delete', function (e) {
                e.preventDefault();

                var a = $(this).closest(".panel");

                var nRow = $(this).parents('tr')[0];

                var rec = $(this).parents('tr').attr("recid");

                //console.log($(this).parents('tr').attr("recid"));

                Swal.fire({
                    title: "Are you sure?",
                    text: "You want to delete this record?",
                    icon: "question",
                    showCancelButton: true,
                    confirmButtonText: "Proceed",
                    reverseButtons: true
                }).then((result) => {
                    oTable.fnDeleteRow(nRow);

                    $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

                    Swal.fire({
                        title: "Success",
                        text: "Record has been deleted",
                        icon: "success",
                        confirmButtonText: "Ok"
                    });
                });
            });
        }
    };
}();

$('#save_detail').click(function () {
    var a = $(this).closest(".panel");

    var id = document.getElementById('detailrecordid').value;
    var templateid = document.getElementById('templateid').value;
    var rollnumber = document.getElementById('rollnumber').value;
    var names = document.getElementById('names').value;
    var bank = document.getElementById('bank').value;
    var branch = document.getElementById('branch').value;
    var account = document.getElementById('account').value;
    var phonenumber = document.getElementById('phonenumber').value;
    var amount = document.getElementById('amount').value;
    var sendvia = document.getElementById('sendvia').value;

    var parameters = {
        id: id,
        templateid: templateid,
        rollnumber: rollnumber,
        names: names,
        bank: bank,
        branch: branch,
        account: account,
        phonenumber: phonenumber,
        amount: amount,
        sendvia: sendvia
    };

    $.ajax({
        url: "/BulkPayments/CreateBulkTemplateDetail",
        type: "GET",
        data: parameters,
        beforeSend: function () {
            if (!$(a).hasClass("panel-loading")) {
                var t = $(a).find(".panel-body"),
                    i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                $(a).addClass("panel-loading"), $(t).prepend(i);
            }
        },
        success: function (data) {
            $("#capture-template-detail").modal("hide").data("bs.modal", null);

            if (data.includes('success')) {
                GetTemplateDetails(templateid);
            } else {
                Swal.fire({
                    title: "Sorry",
                    text: data,
                    icon: "warning",
                    confirmButtonText: "Ok"
                });
            }

            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
        }
    });
});

$("#capture-template-detail").on("hidden.bs.modal", function (e) {
    $('#rollnumber').val("");
    $('#detailrecordid').val("");
    $('#names').val("");
    $('#bank').val("");
    $('#branch').val("");
    $('#account').val("");
    $('#phonenumber').val("");
    $('#amount').val("");
    $('#sendvia').val("").trigger('change');
});