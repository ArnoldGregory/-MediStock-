$(document).ready(function () {
    App.init();

    OnboardingFormWizard.init();

    InitiateApplicantDataTable.init();

    InitiateConfirmApplicantDataTable.init();

    GetCountries();

    GetOrganizationTypes();

    GetBanks();

    GetSexe();

    GetMaritalStatus();

    PrepopulateData();

    $('.selectpicker').select2({
        style: 'btn-white',
        size: 5
    });

    $('#cor_reg_date').datepicker({
        todayHighlight: true,
        /*startDate: '-6m',*/
        endDate: '0',
        format: 'yyyy-mm-dd',
        changeMonth: true,
        changeYear: true,
        autoclose: true,
        todayBtn: 'linked'
    });

});

var applicants = [];
var selected_branch;
var accounts = [];

var riskq1 = 0;
var riskq2 = 0;
var riskq3 = 0;
var riskq4 = 0;
var riskq5 = 0;
var riskq6 = 0;
var riskq7 = 0;
var riskq8 = 0;
var riskq9 = 0;
var riskavg = 0.00;
var staging_id = "";

var InitiateApplicantDataTable = function () {
    return {
        init: function () {
            var oTable = $('#applicantdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "aoColumns": [
                    { "data": "first_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "middle_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "last_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "id_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "email", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "mobile_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "hr_kra_pin", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>"
                    },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash-o'></i> Delete</a>"
                    }
                ]
            });

            var isEditing = null;

            $('#applicantdatatable').on("click", 'a.edit', function (e) {
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

                console.log(aData);

                var json = JSON.parse(JSON.stringify(aData));
                console.log(json);

                $('.modal-body #applicantrecordid').val($(nRow).attr("recid"));
                $('.modal-body #first_name').val(json["first_name"]);
                $('.modal-body #middle_name').val(json["middle_name"]);
                $('.modal-body #last_name').val(json["last_name"]);
                $('.modal-body #id_number').val(json["id_number"]);
                $('.modal-body #mobile_number').val(json["mobile_number"]);
                $('.modal-body #email').val(json["email"]);
                $('.modal-body #hr_kra_pin').val(json["hr_kra_pin"]);
                $("#sexetype").val(json["sexetype"]).trigger("change");
                $("#maritalstatus").val(json["maritalstatus"]).trigger("change");

                var siginatory_files_array_json = $.parseJSON(JSON.stringify(json["siginatories_documents"]));

                for (var i = 0; i < siginatory_files_array_json.length; i++) {
                    var item = siginatory_files_array_json[i];

                    var file_obj = {
                        file_name: item.file_name,
                        file_number: item.file_number,
                        filetype: item.filetype,
                        id_number: item.id_number
                    };

                    if (file_obj.filetype === 'National_ID') {
                        document.getElementById("label_national_id").innerHTML = file_obj.file_name;
                        document.getElementById("label_text_national_id").value = file_obj.file_number;
                    } else if (file_obj.filetype === 'Passport_Photo') {
                        document.getElementById("label_passportphoto").innerHTML = file_obj.file_name;
                        document.getElementById("label_text_passportphoto").value = file_obj.file_number;
                    } else if (file_obj.filetype === 'Signature') {
                        document.getElementById("label_signature").innerHTML = file_obj.file_name;
                        document.getElementById("label_text_signature").value = file_obj.file_number;
                    }
                }


                $("#capture-applicant").appendTo("body").modal("show");

            }

            $('#applicantdatatable').on("click", 'a.delete', function (e) {
                e.preventDefault();
                var a = $(this).closest(".panel");

                var nRow = $(this).parents('tr')[0];

                var rec = $(this).parents('tr').attr("recid");

                Swal.fire({
                    title: "Are you sure?",
                    text: "You want to delete this record",
                    icon: "question",
                    showCancelButton: true,
                    confirmButtonText: "Proceed!",
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        oTable.fnDeleteRow(nRow);
                        applicants = oTable.fnGetData();
                        GetApplicantData(applicants);
                    } else {
                        e.preventDefault();
                    }
                });
            });
        }
    };
}();

var InitiateConfirmApplicantDataTable = function () {
    return {
        init: function () {
            var oTable = $('#confirm_applicantdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "aoColumns": [
                    { "data": "first_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "middle_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "last_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "id_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "mobile_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-eye'></i> View</a>"
                    }
                ]
            });

            var isEditing = null;

            $('#confirm_applicantdatatable').on("click", 'a.edit', function (e) {
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

                $('.modal-body #confirm_applicantrecordid').val($(nRow).attr("recid"));
                $('.modal-body #confirm_first_name').val(json["first_name"]);
                $('.modal-body #confirm_middle_name').val(json["middle_name"]);
                $('.modal-body #confirm_last_name').val(json["last_name"]);
                $('.modal-body #confirm_id_number').val(json["id_number"]);
                $('.modal-body #confirm_mobile_number').val(json["mobile_number"]);
                $('.modal-body #confirm_email').val(json["email"]);
                $("#capture-applicant-confirm").appendTo("body").modal("show");
            }
        }
    };
}();

$("#title").bind("change", function () {
    var str = $("#title option:selected").text();
    var x = document.getElementById("other_title_div");

    if (str === 'Other') {
        x.style.display = "block";
        $('#other_title').val("");
        //other_title = "";
    } else {
        x.style.display = "none";
        $('#other_title').val(str);
    }
});

function GetSigningMandateTypes() {
    $.get('GetRecords', { module: 'signing_mandate_types' }, function (data) {
        $("#mandate").get(0).options.length = 0;
        $("#mandate").get(0).options[0] = new Option("Please Select Signing Mandate", "-1");

        $.each(data, function (index, item) {
            $("#mandate").get(0).options[$("#mandate").get(0).options.length] = new Option(item.NAME, item.VALUE);
        });

        $("#mandate").bind("change", function () {

            var str = $("#mandate option:selected").text();
            var x = document.getElementById("mandate_desc_div");

            if (str === 'Other') {
                x.style.display = "block";
                //$('#mandate_desc').val("");
            } else {
                x.style.display = "none";
                $('#mandate_desc').val(str);
            }
        });
    });
}

function GetBanks() {
    $.get('GetRecords', { module: 'banks' }, function (data) {
        $("#bank").get(0).options.length = 0;
        $("#bank").get(0).options[0] = new Option("Please Select Bank", "-1");

        $.each(data, function (index, item) {
            $("#bank").get(0).options[$("#bank").get(0).options.length] = new Option(item.bank_name, item.id);
        });

        $("#bank").bind("change", function () {
            GetBankBranches($(this).val());
        });
    });
}

function GetBankBranches(bank) {
    var a = $(this).closest(".panel");

    var parameters = { module: 'bank_branches', param: bank };

    $.ajax({
        url: "/ClientSetup/GetRecords",
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
            $("#branch").get(0).options.length = 0;
            $("#branch").get(0).options[0] = new Option("Please Select Branch", "-1");

            $.each(data, function (index, item) {
                $("#branch").get(0).options[$("#branch").get(0).options.length] = new Option(item.branch_name, item.id);
            });

            $("#branch").bind("change", function () {
            });

            if (selected_branch != -1)
                $("#branch").val(selected_branch).trigger("change");

            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
        },
        error: function (xhr, textStatus, errorThrown) {
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            Swal.fire({
                title: "Failed",
                text: "Operation could not be completed " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
}
function GetApplicantData(jsonstring) {
    var table = $('#applicantdatatable').dataTable();
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
function GetMaritalStatus() {
    $.get('GetRecords', { module: 'MARITALSTATUS' }, function (data) {
        $("#maritalstatus").get(0).options.length = 0;
        $("#maritalstatus").get(0).options[0] = new Option("Please Marital Status", "-1");

        $.each(data, function (index, item) {
            $("#maritalstatus").get(0).options[$("#maritalstatus").get(0).options.length] = new Option(item.status_type, item.id);
        });

        $("#maritalstatus").bind("change", function () {
            var str = $("#maritalstatus option:selected").text();
        });
    });
}
function GetOrganizationTypes() {
    $.get('GetRecords', { module: 'organization_type' }, function (data) {
        $("#cor_org_type").get(0).options.length = 0;
        $("#cor_org_type").get(0).options[0] = new Option("Please Select Organization Type", "-1");

        $.each(data, function (index, item) {
            $("#cor_org_type").get(0).options[$("#cor_org_type").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#cor_org_type").bind("change", function () {
           
        });
       

    });
}
function GetSexe() {
    $.get('GetRecords', { module: 'SEXE' }, function (data) {
        $("#sexetype").get(0).options.length = 0;
        $("#sexetype").get(0).options[0] = new Option("Please Select Sexe Type", "-1");

        $.each(data, function (index, item) {
            $("#sexetype").get(0).options[$("#sexetype").get(0).options.length] = new Option(item.type, item.id);
        });

        $("#sexetype").bind("change", function () {
            var str = $("#sexetype option:selected").text();
        });
    });
}
function GetCountries() {
    $.get('GetRecords', { module: 'countries' }, function (data) {
        $("#cor_country").get(0).options.length = 0;
        $("#cor_country").get(0).options[0] = new Option("Please Select Country", "-1");

        $.each(data, function (index, item) {
            $("#cor_country").get(0).options[$("#cor_country").get(0).options.length] = new Option(item.COUNTRY, item.RECORD_ID);
        });

        $("#cor_country").bind("change", function () {
            //var str = $("#source_account option:selected").text();
            //var acc = str.substring(str.lastIndexOf("[") + 1, str.lastIndexOf("]"));
            //console.log(acc.trim());
            //console.log($(this).val());
        });
    });
}
$('#save_applicant').click(function () {

    var a = $(this).closest(".panel");

    var first_name = document.getElementById('first_name').value;
    var middle_name = document.getElementById('middle_name').value;
    var last_name = document.getElementById('last_name').value;
    var id_number = document.getElementById('id_number').value;
    var mobile_number = hr_mobile.getNumber();
    var email = document.getElementById('email').value;    
    var hr_kra_pin = document.getElementById('hr_kra_pin').value;    
    var sexetype = document.getElementById('sexetype').value;    
    var maritalstatus = document.getElementById('maritalstatus').value;    
    var id_card = document.getElementById('label_national_id').innerHTML;
    var passport_photo = document.getElementById('label_passportphoto').innerHTML;
    var signature = document.getElementById('label_signature').innerHTML;

    var client_type = 'COR';

    if (first_name === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly enter first name",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (last_name === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly enter last name",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if ((id_number === '') && (pp_number === '')) {
        Swal.fire({
            title: "Missing information",
            text: "Kindly provide either ID number or passport number",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    } else if (id_number != '') {
        if (id_number.length < 5) {
            Swal.fire({
                title: "Missing information",
                text: "ID number value is too short, it should have 5 characters or more",
                icon: "warning",
                confirmButtonText: "Ok"
            });
            return false;
        }
    }


    if (id_card === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly upload id card",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (passport_photo === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly upload passport photo",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (signature === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly upload signature",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }
    if (hr_kra_pin === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly enter kra pin",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    


    var cnt = applicants.length;
    console.log(cnt);

    var applicant =
    {
        id: cnt + 1,
        first_name: first_name,
        middle_name: middle_name,
        last_name: last_name,
        id_number: id_number,
        mobile_number: mobile_number,
        email: email,
        signature: signature,
        client_type: client_type,
        hr_kra_pin: hr_kra_pin,
        sexetype: sexetype,
        maritalstatus: maritalstatus

    };
    console.log(applicant);
    var recid = document.getElementById('applicantrecordid').value;

    console.log(recid);
    console.log(applicants);

    if (document.getElementById('applicantrecordid').value > 0) {
        const index = applicants.findIndex(item => item.id === document.getElementById('applicantrecordid').value);
        applicants.splice(index, 1);
    }

    applicants.push(applicant);

    GetApplicantData(applicants);

    $("#capture-applicant").modal("hide").data("bs.modal", null);
});

$("#capture-applicant").on("hidden.bs.modal", function (e) {
    $('#applicantrecordid').val("");
    $('#first_name').val("");
    $('#middle_name').val("");
    $('#last_name').val("");
    $('#id_number').val("");
    $('#mobile_number').val("");
    $('#email').val("");
    $('#hr_kra_pin').val("");
    $('#sexetype').val("").trigger("change");
    $('#maritalstatus').val("").trigger("change");
    document.getElementById("label_national_id").innerHTML = '';
    $('#label_text_national_id').val("");
    document.getElementById("label_passportphoto").innerHTML = '';
    $('#label_text_passportphoto').val("");
    document.getElementById("label_signature").innerHTML = '';
    $('#label_text_signature').val("");
});

$('#clear_national_id').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_national_id").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_national_id").innerHTML = '';
    document.getElementById("label_text_national_id").value = '';
});

$('#clear_passportphoto').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_passportphoto").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_passportphoto").innerHTML = '';
    document.getElementById("label_text_passportphoto").value = '';
});

$('#clear_signature').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_signature").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_signature").innerHTML = '';
    document.getElementById("label_text_signature").value = '';
});

$('#clear_sigkrapin').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_sigkrapin").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_sigkrapin").innerHTML = '';
    document.getElementById("label_text_sigkrapin").value = '';
});

$('#clear_krapin').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_krapin").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_krapin").innerHTML = '';
    document.getElementById("label_text_krapin").value = '';
});

$('#clear_boardresolution').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_boardresolution").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_boardresolution").innerHTML = '';
    document.getElementById("label_text_boardresolution").value = '';
});

$('#clear_cr12').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_cr12").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_cr12").innerHTML = '';
    document.getElementById("label_text_cr12").value = '';
});

$('#clear_moa').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_moa").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_moa").innerHTML = '';
    document.getElementById("label_text_moa").value = '';
});

$('#clear_cert_of_inc').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_cert_of_inc").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_cert_of_inc").innerHTML = '';
    document.getElementById("label_text_cert_of_inc").value = '';
});

function StaggeredRegistrationServerCall(server_endpoint, parameters) {
    var a = $(this).closest(".panel");

    $.ajax({
        url: server_endpoint,
        type: "POST",
        headers: {
            "Oluf": "0aa0e60de3bd9586609214071476fe1fb51bcbc3d38486ce50b316151cf3e1d9",
        },
        data: parameters,
        beforeSend: function () {
            if (!$(a).hasClass("panel-loading")) {
                var t = $(a).find(".panel-body"),
                    i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                $(a).addClass("panel-loading"), $(t).prepend(i);
            }
        },
        success: function (data) {
            if (data.error_code === '00') {
                if (parameters["page"] === 'application_details')
                    staging_id = data.error_desc.id;

                handleGritterMessage(data.error_desc.message);
            } else {
                if (parameters["page"] === 'application_details')
                    staging_id = data.error_desc[0].id;

                handleGritterMessage(data.error_desc[0].exception);
            }
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
        }
    });
}


handleGritterMessage = function (data) {
    if (data != '') {
        setTimeout(function () {
            $.gritter.add({
                title: 'Success!',
                text: data
            });
        }, 1000);
    }
}

function PrepopulateData() {
    var parameters = {
        module: 'prepopulated_data'
    };

    var a = $(this).closest(".panel");

    $.ajax({
        url: "/ClientSetup/GetRecords2",
        type: "POST",
        headers: {
            "Oluf": "0aa0e60de3bd9586609214071476fe1fb51bcbc3d38486ce50b316151cf3e1d9",
        },
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(parameters),

        beforeSend: function () {
            
            if (!$(a).hasClass("panel-loading")) {
                var t = $(a).find(".panel-body"),
                    i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                $(a).addClass("panel-loading"), $(t).prepend(i);
            }
        },
        success: function (data) {

            var json = data;

            if (json && Object.keys(json).length > 0 && json.constructor === Object) {
                /** Prepopulate application details */
                if (json.hasOwnProperty("staging_onboards") &&
                    json["staging_onboards"] &&
                    Object.keys(json["staging_onboards"]).length > 0
                    && json["staging_onboards"].constructor === Object) {
                    $("#title").val(json["staging_onboards"]["title"]).trigger("change");
                    document.getElementById("other_title").value = json["staging_onboards"]["title"];
                    document.getElementById("first_name").value = json["staging_onboards"]["first_name"];
                    document.getElementById("middle_name").value = json["staging_onboards"]["middle_name"];
                    document.getElementById("last_name").value = json["staging_onboards"]["last_name"];
                    document.getElementById("id_number").value = json["staging_onboards"]["id_number"];
                    document.getElementById("pp_number").value = json["staging_onboards"]["passport_number"];
                    document.getElementById("mobile_number").value = json["staging_onboards"]["mobile_number"];
                    document.getElementById("email").value = json["staging_onboards"]["email_address"];
                    $("#source_of_funds").val(json["staging_onboards"]["source_of_funds"]).trigger("change");
                    $("#occupation").val(json["staging_onboards"]["occupation_id"]).trigger("change");
                    document.getElementById("nature_of_business").value = json["staging_onboards"]["nature_of_occupation"];
                    document.getElementById("name_of_employer").value = json["staging_onboards"]["name_of_employer"];
                    document.getElementById("designation").value = json["staging_onboards"]["designation"];
                    $("#expected_account_activity").val(json["staging_onboards"]["expected_activity"]).trigger("change");
                    $("#income_distribution").val(json["staging_onboards"]["income_distribution_id"]).trigger("change");
                    document.getElementById("pin_number").value = json["staging_onboards"]["pin_number"];
                    $("#risk_profile").val(json["staging_onboards"]["risk_profile_id"]).trigger("change");
                    document.getElementById("political_relation").value = json["staging_onboards"]["risk_profile_desc"];
                    $("#tax_exempted").val(json["staging_onboards"]["tax_exempted"]).trigger("change");
                    $("#notification_via").val(json["staging_onboards"]["notification_via"]).trigger("change");
                    document.getElementById("investment_consultant").value = json["staging_onboards"]["investment_consultant"];
                    document.getElementById("postal_address").value = json["staging_onboards"]["postal_address"];
                    document.getElementById("physical_address").value = json["staging_onboards"]["physical_address"];
                    $("#fund_product").val(json["staging_onboards"]["fund_product"]).trigger("change");
                    selected_fund_product = json["staging_onboards"]["fund_product"];
                }
                /** End Prepopulate application details */

                /** Prepopulate corporate details */
                if (json.hasOwnProperty("staging_onboard_corporate_details") &&
                    json["staging_onboard_corporate_details"] &&
                    Object.keys(json["staging_onboard_corporate_details"]).length > 0
                    && json["staging_onboard_corporate_details"].constructor === Object) {

                    document.getElementById("cor_reg_name").value = json["staging_onboard_corporate_details"]["reg_name"];
                    document.getElementById("cor_reg_number").value = json["staging_onboard_corporate_details"]["reg_number"];
                    $("#cor_org_type").val(json["staging_onboard_corporate_details"]["org_type"]).trigger("change");
                    document.getElementById("cor_reg_date").value = json["staging_onboard_corporate_details"]["reg_date"];
                    document.getElementById("cor_pin_number").value = json["staging_onboard_corporate_details"]["pin_number"];
                    document.getElementById("cor_email").value = json["staging_onboard_corporate_details"]["email_address"];
                    document.getElementById("cor_contact_person").value = json["staging_onboard_corporate_details"]["contact_person"];
                    document.getElementById("cor_contact_person_number").value = json["staging_onboard_corporate_details"]["contact_person_number"];
                    document.getElementById("cor_contact_narration").value = json["staging_onboard_corporate_details"]["contact_narration"];
                    document.getElementById("cor_telephone_number").value = json["staging_onboard_corporate_details"]["telephone"];
                    $("#cor_country").val(json["staging_onboard_corporate_details"]["country"]).trigger("change");
                    document.getElementById("cor_city").value = json["staging_onboard_corporate_details"]["city"];
                    document.getElementById("cor_postal_address").value = json["staging_onboard_corporate_details"]["postal_address"];
                    document.getElementById("cor_physical_address").value = json["staging_onboard_corporate_details"]["physical_address"];
                }
                /** End Prepopulate corporate details */

                /** Prepopulate siginatories document details */
                if (json.hasOwnProperty("staging_onboard_signatories_details") &&
                    json["staging_onboard_signatories_details"]) {

                   

                    applicants = [];
                    var signatories_json = $.parseJSON(JSON.stringify(json["staging_onboard_signatories_details"]));

                   

                    for (var i = 0; i < signatories_json.length; i++) {

                        var temp_siginatories_documents = [];

                        var item = signatories_json[i];

                        for (var k = 0; k < item.signatory_documents.length; k++) {

                            var file_obj = {
                                id: item.signatory_documents[k].Id,
                                file_name: item.signatory_documents[k].file_name,
                                file_number: item.signatory_documents[k].file_number,
                                filetype: item.signatory_documents[k].filetype,
                                id_number: item.signatory_documents[k].id_number,
                                p_customer_id: item.signatory_documents[k].p_customer_id,
                                user_name: item.signatory_documents[k].user_name
                            };

                            temp_siginatories_documents.push(file_obj);
                        }

                        var applicant =
                        {
                            id: i + 1,
                            first_name: item.first_name,
                            middle_name: item.middle_name,
                            last_name: item.last_name,
                            id_number: item.id_number,
                            mobile_number: item.mobile_number,
                            email: item.email_address,
                            signature: item.signature,
                            client_type: item.client_type,
                            hr_kra_pin: item.pin_number,
                            sexetype: item.sex_type,
                            maritalstatus: item.marital_status,
                            siginatories_documents: temp_siginatories_documents
                        };

                        applicants.push(applicant);

                    }

                    GetApplicantData(applicants);
                   // GetTableData($('#bankdatatable').dataTable(), banks);

                    
                }
                /** End Prepopulate siginatories documents */

                /** Prepopulate bank details */
                if (json.hasOwnProperty("staging_onboard_bank_details") &&
                    json["staging_onboard_bank_details"] &&
                    Object.keys(json["staging_onboard_bank_details"]).length > 0
                    && json["staging_onboard_bank_details"].constructor === Object) {

                    $("#bank").val(json["staging_onboard_bank_details"]["bank"]).trigger("change");
                    selected_branch = json["staging_onboard_bank_details"]["branch"];
                    $("#branch").val(json["staging_onboard_bank_details"]["branch"]).trigger("change");
                    document.getElementById("account_name").value = json["staging_onboard_bank_details"]["account_name"];
                    document.getElementById("account_number").value = json["staging_onboard_bank_details"]["account_number"];
                }
                /** End Prepopulate bank details */

                /** Prepopulate files */
                if (json.hasOwnProperty("staging_onboard_files") &&
                    json["staging_onboard_files"]) {

                    files_array = [];
                    var files_array_json = $.parseJSON(JSON.stringify(json["staging_onboard_files"]));
                    for (var i = 0; i < files_array_json.length; i++) {
                        var item = files_array_json[i];

                        var file_obj = {
                            'filetype': item.filetype,
                            'file_number': item.file_number,
                            'file_name': item.file_name,
                            'staging_id': item.staging_id
                        };

                        files_array.push(file_obj);

                        if (item.filetype === 'KRA_PIN') {
                            document.getElementById("label_krapin").innerHTML = item.file_name;
                            document.getElementById("label_text_krapin").value = item.file_number;
                        } else if (item.filetype === 'Board_Resolution') {
                            document.getElementById("label_boardresolution").innerHTML = item.file_name;
                            document.getElementById("label_text_boardresolution").value = item.file_number;
                        } else if (item.filetype === 'CR12') {
                            document.getElementById("label_cr12").innerHTML = item.file_name;
                            document.getElementById("label_text_cr12").value = item.file_number;
                        } else if (item.filetype === 'Memorandum_and_articles_of_association') {
                            document.getElementById("label_moa").innerHTML = item.file_name;
                            document.getElementById("label_text_moa").value = item.file_number;
                        } else if (item.filetype === 'incorporation_cert') {
                            document.getElementById("label_cert_of_inc").innerHTML = item.file_name;
                            document.getElementById("label_text_cert_of_inc").value = item.file_number;
                        } 
                    }
                }
                /** End Prepopulate files */

                /** Prepopulate risk assessment */
                if (json.hasOwnProperty("staging_risk_assessment") &&
                    json["staging_risk_assessment"] &&
                    Object.keys(json["staging_risk_assessment"]).length > 0
                    && json["staging_risk_assessment"].constructor === Object) {
                    $("#riskq1").val(json["staging_risk_assessment"]["riskq1"]).trigger("change");
                    $("#riskq2").val(json["staging_risk_assessment"]["riskq2"]).trigger("change");
                    $("#riskq3").val(json["staging_risk_assessment"]["riskq3"]).trigger("change");
                    $("#riskq4").val(json["staging_risk_assessment"]["riskq4"]).trigger("change");
                    $("#riskq5").val(json["staging_risk_assessment"]["riskq5"]).trigger("change");
                    $("#riskq6").val(json["staging_risk_assessment"]["riskq6"]).trigger("change");
                    $("#riskq7").val(json["staging_risk_assessment"]["riskq7"]).trigger("change");
                    $("#riskq8").val(json["staging_risk_assessment"]["riskq8"]).trigger("change");
                    $("#riskq9").val(json["staging_risk_assessment"]["riskq9"]).trigger("change");
                    document.getElementById("riskqsummary").value = json["staging_risk_assessment"]["riskqsummary"];
                }
                /** End Prepopulate risk assessment */
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire({
                title: "Failed",
                text: "Operation could not be completed " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
}



var handleOnboardingWizards = function () {
    "use strict";
    $("#wizard").bwizard({
        validating: function (e, ui) {
            if (ui.index === 0) {
                // step-1 Client Details
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-1")) {
                    return false;
                } else {

                    var cap = document.getElementById("captchaInput").value;
                    var reg_name = document.getElementById('cor_reg_name').value;
                    var reg_number = document.getElementById('cor_reg_number').value;
                    var org_type = document.getElementById('cor_org_type').value;
                    var reg_date = document.getElementById('cor_reg_date').value;
                    var kra_pin = document.getElementById('cor_pin_number').value;
                    var email_address = document.getElementById('cor_email').value;
                    var contact_person = document.getElementById('cor_contact_person').value;
                    var contact_person_number = iti.getNumber();
                    var contact_phone_full = document.getElementById('contact_phone_full');
                    var contact_narration = document.getElementById('cor_contact_narration').value;
                    var telephone = document.getElementById('cor_telephone_number').value;
                    var country = document.getElementById('cor_country').value;
                    var city = document.getElementById('cor_city').value;
                    var postal_address = document.getElementById('cor_postal_address').value;
                    var physical_address = document.getElementById('cor_physical_address').value;
                    const container = document.getElementById('uploadedFiles');
                    const customer_files = container.textContent.trim();

                    var corporate_details = {
                        reg_name: reg_name,
                        reg_number: reg_number,
                        org_type: org_type,
                        reg_date: reg_date,
                        pin_number: kra_pin,
                        email_address: email_address,
                        contact_person: contact_person,
                        contact_person_number: contact_person_number,
                        contact_phone_full: contact_phone_full,
                        contact_narration: contact_narration,
                        telephone: telephone,
                        country: country,
                        city: city,
                        postal_address: postal_address,
                        physical_address: physical_address

                    } 

                    var parameters = {
                        staging_id: 0,
                        applicant_details: applicants,
                        corporate_details: corporate_details,
                        customer_files: customer_files,
                        page: 'application_details',
                        pac: cap
                    };
                    console.log("param: " + applicants);

                    var table = document.getElementById('applicantdatatable');
                    var rows = table.getElementsByTagName('tr');

                    console.log(rows);

                    // Check if there are no rows in the table
                    if (rows.length < 2) { // Assuming the first row is the header row
                        Swal.fire({
                            title: "Missing information",
                            text: "Kindly Add Signatories ,Click on HR Officials",
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                        return false;

                    }

                    StaggeredRegistrationServerCall("/ClientSetup/RegisterStaggeredClient", parameters);


                }
            }  else if ((ui.index === 1) && (ui.nextIndex > ui.index)) {
                // step-2 Bank Details
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-2")) {
                    return false;
                } else {

                    if (bank === '-1') {
                        Swal.fire({
                            title: "Missing information",
                            text: "Kindly select bank",
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                        return false;
                    }

                    if (branch === '-1') {
                        Swal.fire({
                            title: "Missing information",
                            text: "Kindly select branch",
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                        return false;
                    }


                    var bank = document.getElementById('bank').value;
                    var branch = document.getElementById('branch').value;
                    var account_name = document.getElementById('account_name').value;
                    var account_number = document.getElementById('account_number').value;

                    var bank_details = {
                        bank: bank,
                        branch: branch,
                        account_name: account_name,
                        account_number: account_number,
                    };
                        

                    var parameters = {
                        staging_id: staging_id,
                        bank_details: bank_details,
                        page: 'bank_details',
                        pac: cap
                    
                    };

                    console.log(parameters);

                    StaggeredRegistrationServerCall("/ClientSetup/RegisterStaggeredClient", parameters);
             


                }
            } else if ((ui.index === 2) && (ui.nextIndex > ui.index)) {
                // step-3 Uploads
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-3")) {
                    return false;
                } else {

                    const containerfiles = document.getElementById('uploadedcorpFiles');;
                    const customer_corp_files = containerfiles.textContent.trim();

                    var parameters = {
                        staging_id: staging_id,
                        customer_corp_files: customer_corp_files,
                        page: 'documents',
                        pac: cap

                    };

                    /***** Corporate details confirmation *****/
                    document.getElementById('confirm_cor_reg_name').value = document.getElementById('cor_reg_name').value;
                    document.getElementById('confirm_cor_reg_number').value = document.getElementById('cor_reg_number').value;
                    document.getElementById('confirm_cor_reg_date').value = document.getElementById('cor_reg_date').value;
                    document.getElementById('confirm_cor_pin_number').value = document.getElementById('cor_pin_number').value;
                    document.getElementById('confirm_cor_email').value = document.getElementById('cor_email').value;
                    document.getElementById('confirm_cor_telephone_number').value = document.getElementById('cor_telephone_number').value;
                    document.getElementById('confirm_cor_contact_person').value = document.getElementById('cor_contact_person').value;
                    document.getElementById('confirm_cor_contact_person_number').value = document.getElementById('cor_contact_person_number').value;
                    document.getElementById('confirm_cor_contact_narration').value = document.getElementById('cor_contact_narration').value;
                    document.getElementById('confirm_cor_postal_address').value = document.getElementById('cor_postal_address').value;
                    document.getElementById('confirm_cor_physical_address').value = document.getElementById('cor_physical_address').value;
                    document.getElementById('confirm_cor_org_type').value = $("#cor_org_type option:selected").text();
                    document.getElementById('confirm_country').value = $("#cor_country option:selected").text();
                    document.getElementById('confirm_cor_city').value = document.getElementById('cor_city').value;
                    document.getElementById('confirm_cor_contact_narration').value = document.getElementById('cor_contact_narration').value;
                    /***** Corporate details confirmation *****/

                    /***** Bank details confirmation *****/
                    document.getElementById('confirm_bank').value = $("#bank option:selected").text();
                    document.getElementById('confirm_branch').value = $("#branch option:selected").text();
                    document.getElementById('confirm_account_name').value = document.getElementById('account_name').value;
                    document.getElementById('confirm_account_number').value = document.getElementById('account_number').value;
                    /***** Bank details confirmation *****/

                    /***** Applicants confirmation table *****/
                    var table = $('#confirm_applicantdatatable').dataTable();
                    oSettings = table.fnSettings();
                    table.fnClearTable(this);

                    var json = applicants;
                    for (var i = 0; i < json.length; i++) {
                        var item = json[i];
                        table.oApi._fnAddData(oSettings, item);
                    }
                    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
                    table.fnDraw();
                    /***** Applicants confirmation table *****/

                    StaggeredRegistrationServerCall("/ClientSetup/RegisterStaggeredClient", parameters);

                }
            }
            else if ((ui.index === 3) && (ui.nextIndex > ui.index)) {
                // step-4 Confirm
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-5")) {
                    return false;
                } else {
                    var a = $(this).closest(".panel");


                    var indemnity = document.getElementById('indemnity').checked;
                    var terms = document.getElementById('terms').checked;
                    var consent = document.getElementById('consent').checked;

                    if (!indemnity) {
                        Swal.fire({
                            title: "Information",
                            text: "You must accept indemnity terms in order to proceed",
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                        return false;
                    }

                    if (!terms) {
                        Swal.fire({
                            title: "Information",
                            text: "You must accept terms and conditions in order to proceed",
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                        return false;
                    }

                    if (!consent) {
                        Swal.fire({
                            title: "Information",
                            text: "You must give consent allowing us to store your information in order to proceed",
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                        return false;
                    }

                    Swal.fire({
                        title: "Are you sure?",
                        text: "you want to proceed with onboarding?",
                        icon: "question",
                        showCancelButton: true,
                        confirmButtonText: "Proceed",
                        reverseButtons: true
                    }).then((result) => {
                        if (result.isConfirmed) {

                            var vendor_name = document.getElementById('cor_reg_name').value;
                            var org_number = document.getElementById('cor_reg_number').value;
                            var cap = document.getElementById("captchaInput").value;

                            var parameters = {
                                staging_id: staging_id,
                                page: 'confirm',
                                pac: cap
                            };


                            $.ajax({
                                url: "/ClientSetup/RegisterStaggeredClient",
                                type: "POST",
                                headers: {
                                    "Oluf": "0aa0e60de3bd9586609214071476fe1fb51bcbc3d38486ce50b316151cf3e1d9",
                                },
                                data: parameters,
                                beforeSend: function () {
                                    if (!$(a).hasClass("panel-loading")) {
                                        var t = $(a).find(".panel-body"),
                                            i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                                        $(a).addClass("panel-loading"), $(t).prepend(i);
                                    }
                                },
                                success: function (data) {

                                    var buttons = document.getElementsByClassName("previous");

                                    for (var i = 0; i < buttons.length; i++) {
                                        buttons[i].setAttribute("aria-disabled", "true");
                                        buttons[i].setAttribute("class", "previous disabled");
                                    }

                                    if (data.error_code === '00') {
                                        document.getElementById("summary_system_reference").innerHTML = data.error_desc.id;
                                        document.getElementById("summary_status").innerHTML = "Success";
                                        document.getElementById("summary_status").classList = "label label-success";
                                        Swal.fire({
                                            title: "Success",
                                            text: data.error_desc.message,
                                            icon: "success",
                                            confirmButtonText: "Ok"
                                        });
                                    } else {
                                        document.getElementById("summary_status").innerHTML = "Failed";
                                        document.getElementById("summary_status").classList = "label label-danger";
                                        Swal.fire({
                                            title: "Failed",
                                            text: data.error_desc[0]["exception"],
                                            icon: "error",
                                            confirmButtonText: "Ok"
                                        });
                                    }

                                    $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
                                }
                            });

                        }
                        else {
                            document.getElementById("summary_system_reference").innerHTML = "-";
                            document.getElementById("summary_status").innerHTML = "Cancelled";
                            document.getElementById("summary_status").classList = "label label-info";

                            Swal.fire({
                                title: "Cancelled",
                                text: "Registration has been cancelled",
                                icon: "info",
                                confirmButtonText: "Ok"
                            });
                        }
                    });
                }
            }
        }
    });
};

var OnboardingFormWizard = function () {
    "use strict";
    return {
        init: function () {
            handleOnboardingWizards();
        }
    };
}();