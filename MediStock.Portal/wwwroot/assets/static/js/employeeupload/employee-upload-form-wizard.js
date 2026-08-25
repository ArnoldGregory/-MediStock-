
var employee_details_record1 = [];
var next_of_kin_record = [];
var beneficiary_details_record = [];
var bank_details_record = [];
var pay_details_record = [];



$(document).ready(function () {
    App.init();

    BulkPaymentFormWizard.init();

    
    $('#drag-and-drop-zone').dmUploader({ //

        url: '/Employee/UploadFiles',
        extFilter: ["doc", "docx", "pdf", "jpg", "jpeg", "xls", "png", "gif"],
        fieldName: 'postedFiles',
        maxFileSize: 3000000, // 3 Megs 
        onDragEnter: function () {
            // Happens when dragging something over the DnD area
            this.addClass('active');
        },
        onDragLeave: function () {
            // Happens when dragging something OUT of the DnD area
            this.removeClass('active');
        },
        onInit: function () {
            // Plugin is ready to use
            ui_add_log('Penguin initialized :)', 'info');
        },
        onComplete: function () {
            // All files in the queue are processed (success or error)
            ui_add_log('All pending tranfers finished');
        },
        onNewFile: function (id, file) {
            // When a new file is added using the file selector or the DnD area
            ui_add_log('New file added #' + id);
            ui_multi_add_file(id, file, 'uploaderFile', 'files');

            //$('#uploadedFiles').append(file.name + '|<br />');
            //document.getElementById("label_national_id").innerHTML = file.name;
            //document.getElementById("label_text_national_id").value = file.name;
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile');
        },
        onUploadSuccess: function (id, data) {

            var json_employee = data["employee_details"];
            var json_nok = data["nok_details"];
            var json_beneficiaries = data["beneficiary_details"];
            var json_bank = data["bank_details"];
           // var json_pay = data["pay_details"];




            for (var j = 0; j < json_nok.length; j++) {
                var item = json_nok[j];

                var obj = {
                    /*templateid: 0,*/
                    RN: item["RN"],
                    employee_email: item["employee_email"],
                    type: item["type"],
                    next_of_kin_name: item["next_of_kin_name"],
                    document_type: item["document_type"],
                    doc_number: item["doc_number"],
                    nOk_email_address: item["nOk_email_address"],
                    nOK_mobile_number: item["nOK_mobile_number"]

                };

                next_of_kin_record.push(obj);
            }

            for (var k = 0; k < json_beneficiaries.length; k++) {
                var item = json_beneficiaries[k];

                var obj = {
                    /*templateid: 0,*/
                    RN: item["RN"],
                    employee_email: item["employee_email"],
                    type: item["type"],
                    beneficiary_name: item["beneficiary_name"],
                    document_type: item["document_type"],
                    doc_number: item["doc_number"],
                    beneficiary_address: item["beneficiary_address"],
                    beneficiary_mobile_number: item["beneficiary_mobile_number"]

                };

                beneficiary_details_record.push(obj);
            }

            for (var m = 0; m < json_bank.length; m++) {
                var item = json_bank[m];

                var obj = {
                    /*templateid: 0,*/
                    RN: item["RN"],
                    employee_email: item["employee_email"],
                    account_name: item["account_name"],
                    currency: item["currency"],
                    account_number: item["account_number"],
                    bank: item["bank"],
                    branch: item["branch"]

                };

                bank_details_record.push(obj);
            }

            //for (var n = 0; n < json_pay.length; n++) {
            //    var item = json_pay[n];

            //    var obj = {
            //        /*templateid: 0,*/
            //        RN: item["RN"],
            //        employee_email: item["employee_email"],
            //        currency: item["currency"],
            //        standard_hours: item["standard_hours"],
            //        standard_payrate: item["standard_payrate"],
            //        basic_pay: item["basic_pay"],
            //        overtime_hours: item["overtime_hours"],
            //        overtime_pay_rate: item["overtime_pay_rate"],
            //        overtime_pay: item["overtime_pay"],
            //        holiday_hrs: item["holiday_hrs"],
            //        holiday_pay_rate: item["holiday_pay_rate"],
            //        holiday_pay: item["holiday_pay"],
            //        other_basic_pay: item["other_basic_pay"],
            //        commissions_bonus: item["commissions_bonus"],
            //        sick_pay: item["sick_pay"],
            //        expenses: item["expenses"],
            //        paye_tax: item["paye_tax"],
            //        nhif: item["nhif"],
            //        help: item["help"],
            //        pension: item["pension"],
            //        union_fees: item["union_fees"],
            //        non_taxable_reimbursements: item["non_taxable_reimbursements"],
            //        trf_bank: item["trf_bank"],
            //        other_hourly_pay: item["other_hourly_pay"],
            //        vacation_pay: item["vacation_pay"]

            //    };

            //    pay_details_record.push(obj);
            //}



            // console.log(json_employee);
            //var json_consolidated = $.parseJSON(JSON.stringify(data["consolidated"]));
            for (var i = 0; i < json_employee.length; i++) {
                var item = json_employee[i];

                var obj = {
                    /*templateid: 0,*/
                    RN: item["RN"],
                    alt_email: item["alt_email"],
                    employee_type: item["employee_type"],
                    nhif: item["nhif"],
                    nssf: item["nssf"],
                    postal_address: item["postal_address"],
                    alt_mobile_number: item["alt_mobile_number"],
                    badge_id: item["badge_id"],
                    contract_type: item["contract_type"],
                    country: item["country"],
                    department: item["department"],
                    dob: item["dob"],
                    doc_number: item["doc_number"],
                    doc_type: item["doc_type"],
                    email_address: item["email_address"],
                    first_name: item["first_name"],
                    language: item["language"],
                    marital_status: item["marital_status"],
                    mobile_number: item["mobile_number"],
                    other_name: item["other_name"],
                    pay_type: item["pay_type"],
                    physical_address: item["physical_address"],
                    sexe: item["sexe"],
                    surname: item["surname"],
                    tax_id: item["tax_id "],
                    title: item["title"]
                };

                employee_details_record1.push(obj);
            }

            //console.log(employee_details_record1);
            //console.log(next_of_kin_record);
            //console.log(beneficiary_details_record);
            //console.log(bank_details_record);
            //console.log(pay_details_record);


            table = $('#editabletemplatedetailsdatatable').dataTable();
            oSettings = table.fnSettings();

            var data_table = table.fnGetData();
            var json = $.parseJSON(JSON.stringify(data_table));

            var total_amount = 0;
            for (var i = 0; i < json.length; i++) {
                var item = json[i];
                total_amount = total_amount + Number(item.amount);
            }

            json = $.parseJSON(JSON.stringify(data["employee_details"]));

            for (var j = 0; j < json.length; j++) {
                item = json[j];
                table.oApi._fnAddData(oSettings, item);
                total_amount = total_amount + Number(item.amount);
            }

            oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
            table.fnDraw();

            ui_add_log('Server Response for file #' + id + ': ' + item.new_file_name);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile');


        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
            /*document.getElementById("label_national_id").innerHTML = '';
            document.getElementById("label_text_national_id").value = '';*/
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
            /*document.getElementById("label_national_id").innerHTML = '';
            document.getElementById("label_text_national_id").value = '';*/
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size / 1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
            /*document.getElementById("label_national_id").innerHTML = '';
            document.getElementById("label_text_national_id").value = '';*/
        }
    });




    InitiateTemplateDetailsEditableDataTable.init();

    InitiateTemplateDetailsConfirmEditableDataTable.init();

    FormMultipleUpload.init();

    $('.selectpicker').select2({
        style: 'btn-white',
        size: 5
    });
});


function GetTemplateDetails() {
    $.get('GetRecords', { module: 'bulk_payment_template_details' }, function (data) {
        table = $('#editabletemplatedetailsdatatable').dataTable();
        oSettings = table.fnSettings();
        table.fnClearTable(this);

        var json = $.parseJSON(JSON.stringify(data));

        var total_amount = 0;
        for (var i = 0; i < json.length; i++) {
            var item = json[i];
            table.oApi._fnAddData(oSettings, item);
            total_amount = total_amount + item.amount;
        }
        oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
        table.fnDraw();

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
                    { "data": "RN", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "employee_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "first_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "other_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "surname", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "doc_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "doc_number", "autoWidth": true, "sDefaultContent": "n/a", /*render: $.fn.dataTable.render.number(',', '.', 2, '')*/ },
                    { "data": "postal_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "physical_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "country", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "language", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "tax_id", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "mobile_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "email_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "pay_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "sexe", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "contract_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "alt_mobile_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "alt_email", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "department", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "title", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "marital_status", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "dob", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "nhif", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "nssf", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "badge_id", "autoWidth": true, "sDefaultContent": "n/a" },

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

                        /*document.getElementById("amount").value = total_amount.toFixed(2).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");*/

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
                    { "data": "RN", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "employee_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "first_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "other_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "surname", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "doc_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "doc_number", "autoWidth": true, "sDefaultContent": "n/a", /*render: $.fn.dataTable.render.number(',', '.', 2, '')*/ },
                    { "data": "postal_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "physical_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "country", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "language", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "tax_id", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "mobile_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "email_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "pay_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "sexe", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "contract_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "alt_mobile_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "alt_email", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "department", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "title", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "marital_status", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "dob", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "nhif", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "nssf", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "badge_id", "autoWidth": true, "sDefaultContent": "n/a" },
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
                    text: "you want to save principal members?",
                    icon: "question",
                    showCancelButton: true,
                    confirmButtonText: "Proceed",
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {

                        var table = $('#editabletemplatedetailsdatatable').dataTable();
                        var data = table.fnGetData();

                        var json = $.parseJSON(JSON.stringify(data));

                        var parameters = {

                            employee_details_record1: employee_details_record1,
                            next_of_kin_record: next_of_kin_record,
                            beneficiary_details_record: beneficiary_details_record,
                            bank_details_record: bank_details_record
                            //pay_details_record: pay_details_record

                        };
                        

                        $.ajax({
                            url: "/EmployeeUpload/BulkEmployeeOnboard",
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

                                document.getElementById("summary_system_reference").innerHTML = document.getElementById("confirm_client").value;
                                /*document.getElementById("account_number").innerHTML = data[0].account_number;*/

                                var buttons = document.getElementsByClassName("previous");

                                for (var i = 0; i < buttons.length; i++) {
                                    buttons[i].setAttribute("aria-disabled", "true");
                                    buttons[i].setAttribute("class", "previous disabled");
                                }

                                if (data[0].error_code === '00') {
                                    document.getElementById("summary_status").innerHTML = "Success";
                                    document.getElementById("summary_status").classList = "label label-success";
                                    Swal.fire({
                                        title: "Success",
                                        text: data[0].desc,
                                        icon: "success",
                                        confirmButtonText: "Ok"
                                    });
                                } else {
                                    document.getElementById("summary_status").innerHTML = "Failed";
                                    document.getElementById("summary_status").classList = "label label-danger";
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