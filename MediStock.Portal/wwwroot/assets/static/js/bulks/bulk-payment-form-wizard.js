function GetClientsDropDown() {
    $.get('GetRecords', { module: 'clients' }, function (data) {
        $("#client").get(0).options.length = 0;
        $("#client").get(0).options[0] = new Option("Please Select a Client", "-1");

        $.each(data, function (index, item) {
            $("#client").get(0).options[$("#client").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#client").bind("change", function () {
            var str = $("#client option:selected").text();
            //var acc = str.substring(str.lastIndexOf("[") + 1, str.lastIndexOf("]"));
            //console.log(acc.trim());
            //console.log($(this).val());
            //GetSourceAccountBalance();
        });
    });
}



var formdata = new FormData();

$("#fileInput").on("change", function () {
    var fileInput = document.getElementById('fileInput');

    for (i = 0; i < fileInput.files.length; i++) {

        var sfilename = fileInput.files[i].name;
        let srandomid = Math.random().toString(36).substring(7);

        formdata.append(sfilename, fileInput.files[i]);

        var markup = '<tr id=' + srandomid + '>' +
            '				<td>' + sfilename + '</td>' +
            '				<td>' +
            '					<button class="btn btn-danger btn-xs" onclick=DeleteFile("' + srandomid + '","' + sfilename + '");>' +
            '						<i class="fa fa-times"></i>' +
            '					</button>' +
            '				</td>' +
            '		  </tr>';
        $("#FilesList tbody").append(markup);

        markup = '<tr id=' + srandomid + '>' +
            '           <td>' + sfilename + '</td>' +
            '           <td></td>' +
            '	  </tr>';
        $("#FilesList2 tbody").append(markup);
    }
    chkatchtbl();
    $('#fileInput').val('');
});

$("#btnupload").click(function () {
    $.ajax({
        url: '/BulkPayments/UploadFiles',
        type: "POST",
        contentType: false,
        processData: false,
        data: formdata,
        async: false,
        success: function (data) {
            table = $('#editabletemplatedetailsdatatable').dataTable();
            oSettings = table.fnSettings();

            var data_table = table.fnGetData();
            var json = $.parseJSON(JSON.stringify(data_table));

            var total_amount = 0;
            for (var i = 0; i < json.length; i++) {
                var item = json[i];
                total_amount = total_amount + Number(item.amount);
            }

            json = $.parseJSON(JSON.stringify(data));

            for (var j = 0; j < json.length; j++) {
                item = json[j];
                table.oApi._fnAddData(oSettings, item);
                total_amount = total_amount + Number(item.amount);
            }

            oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
            table.fnDraw();

            document.getElementById("amount").value = total_amount.toFixed(2).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        },
        error: function (err) {
            Swal.fire({
                title: "Failed",
                text: err.statusText,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
});

function DeleteFile(Fileid, FileName) {
    formdata.delete(FileName);
    $("#" + Fileid).remove();
    chkatchtbl();
}

function chkatchtbl() {
    if ($('#FilesList tr').length > 1) {
        $("#FilesList").css("visibility", "visible");
        $("#FilesList2").css("visibility", "visible");
    } else {
        $("#FilesList").css("visibility", "hidden");
        $("#FilesList2").css("visibility", "hidden");
    }
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
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash-o'></i> Delete</a>"
                    }
                ]
            });

            //Delete an Existing Row
            $('#editabletemplatedetailsdatatable').on("click", 'a.delete', function (e) {
                e.preventDefault();

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

                        var table = $('#editabletemplatedetailsdatatable').dataTable();
                        var data = table.fnGetData();
                        var json = $.parseJSON(JSON.stringify(data));

                        var total_amount = 0;
                        for (var i = 0; i < json.length; i++) {
                            var item = json[i];
                            total_amount = total_amount + item.amount;
                        }

                        document.getElementById("amount").value = total_amount.toFixed(2).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");

                        Swal.fire({
                            title: "Success",
                            text: "Record has been deleted",
                            icon: "success",
                            confirmButtonText: "Ok"
                        });
                    }
                });
            });
        }
    };
}();

var InitiateTemplateDetailsConfirmEditableDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#editabletemplatedetailsdatatableconfirm').dataTable({
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
                    { "data": "send_via", "autoWidth": true, "sDefaultContent": "n/a" }
                ]
            });
        }
    };
}();

var handleBulkPaymentWizard = function () {
    "use strict";
    $("#wizard").bwizard({
        validating: function (e, ui) {
            if (ui.index === 0) {
                // step-1 confirmation
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-1")) {
                    return false;
                } else {
                    var source_str = $("#source_account option:selected").text();
                    var source_acc = source_str.substring(source_str.lastIndexOf("[") + 1, source_str.lastIndexOf("]"));
                    document.getElementById("confirm_source_account").value = source_acc.trim();
                    document.getElementById("confirm_amount").value = $("#amount").val();
                    document.getElementById("confirm_reference").value = $("#reference").val();
                    document.getElementById("confirm_comments").value = $("#comments").val();
                    document.getElementById("confirm_bulk_template").value = $("#bulk_template option:selected").text();
                    document.getElementById("confirm_selectedtemplate").innerHTML = document.getElementById("selectedtemplate").innerHTML;

                    var table = $('#editabletemplatedetailsdatatable').dataTable();
                    var data = table.fnGetData();

                    table = $('#editabletemplatedetailsdatatableconfirm').dataTable();
                    var oSettings = table.fnSettings();
                    table.fnClearTable(this);

                    var json = $.parseJSON(JSON.stringify(data));

                    for (var i = 0; i < json.length; i++) {
                        var item = json[i];
                        table.oApi._fnAddData(oSettings, item);
                    }
                    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
                    table.fnDraw();
                }
            } else if ((ui.index === 1) && (ui.nextIndex > ui.index)) {
                // step-2 summary
                var a = $(this).closest(".panel");

                Swal.fire({
                    title: "Are you sure?",
                    text: "you want to commit this transaction?",
                    icon: "question",
                    showCancelButton: true,
                    confirmButtonText: "Proceed",
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        var source_account = document.getElementById('confirm_source_account').value;
                        var amount = document.getElementById('confirm_amount').value;
                        var reference = document.getElementById('confirm_reference').value;
                        var comments = document.getElementById('confirm_comments').value;

                        var table = $('#editabletemplatedetailsdatatableconfirm').dataTable();
                        var data = table.fnGetData();

                        var json = $.parseJSON(JSON.stringify(data));

                        var bulktemplatedetailrecords = [];
                        for (var i = 0; i < json.length; i++) {
                            var item = json[i];

                            var obj = {
                                templateid: 0,
                                id: 0,
                                rollnumber: item["roll_number"],
                                names: item["names"],
                                bank: item["bank"],
                                branch: item["branch"],
                                account: item["account_number"],
                                phonenumber: item["phone_number"],
                                amount: item["amount"],
                                sendvia: item["send_via"]
                            };

                            bulktemplatedetailrecords.push(obj);
                        }

                        var parameters = {
                            source_account: source_account,
                            amount: amount,
                            reference: reference,
                            comments: comments,
                            bulktemplatedetailrecords: bulktemplatedetailrecords
                        };

                        $.ajax({
                            url: "/BulkPayments/BulkPayment",
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
                                document.getElementById("summary_source_account").innerHTML = source_account.trim();
                                document.getElementById("summary_amount").innerHTML = amount;
                                document.getElementById("summary_my_reference").innerHTML = reference;
                                document.getElementById("summary_comments").innerHTML = comments;
                                document.getElementById("summary_system_reference").innerHTML = data[0].system_ref;

                                if (data[0].code === '00') {
                                    document.getElementById("summary_status").innerHTML = "Success";
                                    document.getElementById("summary_status").classList = "label label-success";
                                    Swal.fire({
                                        title: "Success",
                                        text: data[0].desc,
                                        icon: "success",
                                        confirmButtonText: "Ok"
                                    });
                                } else {
                                    Swal.fire({
                                        title: "Failed",
                                        text: data[0].desc,
                                        icon: "error",
                                        confirmButtonText: "Ok"
                                    });
                                }
                                $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
                            }
                        });
                    }
                    else {
                        document.getElementById("summary_source_account").innerHTML = source_account.trim();
                        document.getElementById("summary_amount").innerHTML = amount;
                        document.getElementById("summary_my_reference").innerHTML = reference;
                        document.getElementById("summary_comments").innerHTML = comments;
                        document.getElementById("summary_system_reference").innerHTML = "-";
                        document.getElementById("summary_status").innerHTML = "Cancelled";
                        document.getElementById("summary_status").classList = "label label-info";

                        Swal.fire({
                            title: "Cancelled",
                            text: "Transaction has been cancelled",
                            icon: "info",
                            confirmButtonText: "Ok"
                        });
                    }
                });
            }
        }
    });
};

var BulkPaymentFormWizard = function () {
    "use strict";
    return {
        init: function () {
            handleBulkPaymentWizard();
        }
    };
}();


$(document).ready(function () {
    App.init();

    BulkPaymentFormWizard.init();

    GetClientsDropDown;

    //GetTemplatesDropDown();

    InitiateTemplateDetailsEditableDataTable.init();

    InitiateTemplateDetailsConfirmEditableDataTable.init();

    FormMultipleUpload.init();

    $('.selectpicker').select2({
        style: 'btn-white',
        size: 5
    });
});
