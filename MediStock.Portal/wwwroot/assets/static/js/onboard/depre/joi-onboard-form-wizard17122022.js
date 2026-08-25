var applicants = [];

var accounts = [];

var noks = [];

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
                    { "data": "mobile_number", "autoWidth": true, "sDefaultContent": "n/a" },
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

                var json = JSON.parse(JSON.stringify(aData));

                $('.modal-body #applicantrecordid').val($(nRow).attr("recid"));
                $('.modal-body #first_name').val(json["first_name"]);
                $('.modal-body #middle_name').val(json["middle_name"]);
                $('.modal-body #last_name').val(json["last_name"]);
                $('.modal-body #id_number').val(json["id_number"]);
                $('.modal-body #pp_number').val(json["pp_number"]);
                $('.modal-body #mobile_number').val(json["mobile_number"]);
                $('.modal-body #email').val(json["email"]);
                $('.modal-body #source_of_funds').val(json["source_of_funds"]).trigger("change");
                $('.modal-body #occupation').val(json["occupation"]).trigger("change");
                $('.modal-body #nature_of_business').val(json["nature_of_business"]);
                $('.modal-body #name_of_employer').val(json["name_of_employer"]);
                $('.modal-body #designation').val(json["designation"]);
                $('.modal-body #expected_account_activity').val(json["expected_account_activity"]).trigger("change");
                $('.modal-body #income_distribution').val(json["income_distribution"]).trigger("change");
                $('.modal-body #pin_number').val(json["pin_number"]);
                $('.modal-body #tax_exempted').val(json["tax_exempted"]).trigger("change");
                $('.modal-body #postal_address').val(json["postal_address"]);
                $('.modal-body #physical_address').val(json["physical_address"]);
                document.getElementById('label_national_id').innerHTML = json["id_card"];
                $('.modal-body #label_text_national_id').val(json["id_card"]);
                document.getElementById('label_passportphoto').innerHTML = json["passport_photo"];
                $('.modal-body #label_text_passportphoto').val(json["passport_photo"]);
                document.getElementById('label_signature').innerHTML = json["signature"];
                $('.modal-body #label_text_signature').val(json["signature"]);

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

var InitiateNOKDataTable = function () {
    return {
        init: function () {
            var oTable = $('#nokdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "aoColumns": [
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "relationship_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "relationship_desc", "autoWidth": true, "sDefaultContent": "n/a" },
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

            $('#nokdatatable').on("click", 'a.edit', function (e) {
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

                $('.modal-body #nokrecordid').val($(nRow).attr("recid"));
                $('.modal-body #beneficiary_name').val(json["name"]);
                $('.modal-body #beneficiary_id_number').val(json["id_number"]);
                $('.modal-body #beneficiary_pp_number').val(json["pp_number"]);
                $('.modal-body #beneficiary_email').val(json["email"]);
                $('.modal-body #beneficiary_mobile_number').val(json["mobile_number"]);
                //$('.modal-body #beneficiary_allocation').val(json["allocation"]);
                $('.modal-body #beneficiary_relationship').val(json["relationship"]).trigger("change");
                $('.modal-body #beneficiary_other_relationship').val(json["relationship_desc"]);

                $("#capture-nok").appendTo("body").modal("show");
            }

            $('#nokdatatable').on("click", 'a.delete', function (e) {
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
                        noks = oTable.fnGetData();
                        GetNOKData(noks);
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
                $('.modal-body #confirm_pp_number').val(json["pp_number"]);
                $('.modal-body #confirm_mobile_number').val(json["mobile_number"]);
                $('.modal-body #confirm_email').val(json["email"]);
                $('.modal-body #confirm_source_of_funds').val(json["source_of_funds_text"]);
                $('.modal-body #confirm_occupation').val(json["occupation_text"]);
                $('.modal-body #confirm_nature_of_business').val(json["nature_of_business"]);
                $('.modal-body #confirm_name_of_employer').val(json["name_of_employer"]);
                $('.modal-body #confirm_designation').val(json["designation"]);
                $('.modal-body #confirm_expected_account_activity').val(json["expected_account_activity_text"]);
                $('.modal-body #confirm_income_distribution').val(json["income_distribution_text"]);
                $('.modal-body #confirm_pin_number').val(json["pin_number"]);
                $('.modal-body #confirm_tax_exempted').val(json["tax_exempted"]);
                $('.modal-body #confirm_postal_address').val(json["postal_address"]);
                $('.modal-body #confirm_physical_address').val(json["physical_address"]);
                //document.getElementById('label_national_id').innerHTML = json["id_card"];
                //$('.modal-body #label_text_national_id').val(json["id_card"]);
                //document.getElementById('label_passportphoto').innerHTML = json["passport_photo"];
                //$('.modal-body #label_text_passportphoto').val(json["passport_photo"]);
                //document.getElementById('label_signature').innerHTML = json["signature"];
                //$('.modal-body #label_text_signature').val(json["signature"]);

                $("#capture-applicant-confirm").appendTo("body").modal("show");
            }
        }
    };
}();

var InitiateConfirmNOKDataTable = function () {
    return {
        init: function () {
            var oTable = $('#confirm_nokdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "aoColumns": [
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "relationship_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "relationship_desc", "autoWidth": true, "sDefaultContent": "n/a" }
                ]
            });
        }
    };
}();

function GetOccupations() {
    $.get('GetRecords', { module: 'occupation' }, function (data) {
        $("#occupation").get(0).options.length = 0;
        $("#occupation").get(0).options[0] = new Option("Please Select Occupation", "-1");

        $.each(data, function (index, item) {
            $("#occupation").get(0).options[$("#occupation").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#occupation").bind("change", function () {
            //var str = $("#source_account option:selected").text();
            //var acc = str.substring(str.lastIndexOf("[") + 1, str.lastIndexOf("]"));
            //console.log(acc.trim());
            //console.log($(this).val());
        });
    });
}

function GetSourceOfFunds() {
    $.get('GetRecords', { module: 'source_of_funds' }, function (data) {
        $("#source_of_funds").get(0).options.length = 0;
        $("#source_of_funds").get(0).options[0] = new Option("Please Select Source Of Funds", "-1");

        $.each(data, function (index, item) {
            $("#source_of_funds").get(0).options[$("#source_of_funds").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#source_of_funds").bind("change", function () {
            //console.log($(this).val());
            var str = $("#source_of_funds option:selected").text();
            if (str === 'Other') {
                $("#capture-other-source-of-funds").appendTo("body").modal("show");
            }
        });
    });
}

function GetExpectedAccountActivity() {
    $.get('GetRecords', { module: 'expected_account_activity' }, function (data) {
        $("#expected_account_activity").get(0).options.length = 0;
        $("#expected_account_activity").get(0).options[0] = new Option("Please Select Expected Account Activity", "-1");

        $.each(data, function (index, item) {
            $("#expected_account_activity").get(0).options[$("#expected_account_activity").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#expected_account_activity").bind("change", function () {
            //console.log($(this).val());
        });
    });
}

function GetIncomeDistributions() {
    $.get('GetRecords', { module: 'income_distribution' }, function (data) {
        $("#income_distribution").get(0).options.length = 0;
        $("#income_distribution").get(0).options[0] = new Option("Please Select Income Distribution", "-1");

        $.each(data, function (index, item) {
            $("#income_distribution").get(0).options[$("#income_distribution").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#income_distribution").bind("change", function () {
            //console.log($(this).val());
        });
    });
}

function GetRiskProfiles() {
    $.get('GetRecords', { module: 'risk_profile' }, function (data) {
        $("#risk_profile").get(0).options.length = 0;
        $("#risk_profile").get(0).options[0] = new Option("Please Select Risk Profile", "-1");

        $.each(data, function (index, item) {
            $("#risk_profile").get(0).options[$("#risk_profile").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#risk_profile").bind("change", function () {
            //console.log($(this).val());
            var str = $("#risk_profile option:selected").text();
            var x = document.getElementById("political_relation_div");

            if (str === 'Political involvement (By relation)') {
                x.style.display = "block";
                //$('#political_relation').val("");
            } else {
                x.style.display = "none";
                $('#political_relation').val(str);
            }
        });
    });
}

function GetRelationships() {
    $.get('GetRecords', { module: 'relationship' }, function (data) {
        $("#beneficiary_relationship").get(0).options.length = 0;
        $("#beneficiary_relationship").get(0).options[0] = new Option("Please Select Relationship", "-1");

        $.each(data, function (index, item) {
            $("#beneficiary_relationship").get(0).options[$("#beneficiary_relationship").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#beneficiary_relationship").bind("change", function () {
            //console.log($(this).val());
            var str = $("#beneficiary_relationship option:selected").text();
            //console.log(str);
            var x = document.getElementById("other_relationship_div");

            if (str === 'Other') {
                x.style.display = "block";
                //$('#beneficiary_other_relationship').val("");
                //beneficiary_other_relationship = "";
            } else {
                x.style.display = "none";
                $('#beneficiary_other_relationship').val(str);
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
            //console.log($(this).val());
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
                //console.log($(this).val());
            });

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

function GetNOKData(jsonstring) {
    table = $('#nokdatatable').dataTable();
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

$('#save_sof').click(function () {
    document.getElementById("other_source_of_funds").value = document.getElementById('other_source_of_funds_desc').value;
    $("#capture-other-source-of-funds").modal("hide").data("bs.modal", null);
});

$("#capture-other-source-of-funds").on("hidden.bs.modal", function (e) {
    $('#other_source_of_funds_desc').val("");
});

$('#save_applicant').click(function () {

    var a = $(this).closest(".panel");

    var first_name = document.getElementById('first_name').value;
    var middle_name = document.getElementById('middle_name').value;
    var last_name = document.getElementById('last_name').value;
    var id_number = document.getElementById('id_number').value;
    var pp_number = document.getElementById('pp_number').value;
    var mobile_number = document.getElementById('mobile_number').value;
    var email = document.getElementById('email').value;
    var source_of_funds = document.getElementById('source_of_funds').value;
    var source_of_funds_text = $("#source_of_funds option:selected").text();
    var occupation = document.getElementById('occupation').value;
    var occupation_text = $("#occupation option:selected").text();
    var nature_of_business = document.getElementById('nature_of_business').value;
    var name_of_employer = document.getElementById('name_of_employer').value;
    var designation = document.getElementById('designation').value;
    var other_source_of_funds_desc = document.getElementById('other_source_of_funds').value;
    var expected_account_activity = document.getElementById('expected_account_activity').value;
    var expected_account_activity_text = $("#expected_account_activity option:selected").text();
    var income_distribution = document.getElementById('income_distribution').value;
    var income_distribution_text = $("#income_distribution option:selected").text();
    var pin_number = document.getElementById('pin_number').value;
    var tax_exempted = document.getElementById('tax_exempted').value;
    var postal_address = document.getElementById('postal_address').value;
    var physical_address = document.getElementById('physical_address').value;
    var id_card = document.getElementById('label_national_id').innerHTML;
    var passport_photo = document.getElementById('label_passportphoto').innerHTML;
    var signature = document.getElementById('label_signature').innerHTML;
    var risk_profile = document.getElementById('risk_profile').value;
    var risk_profile_text = document.getElementById('political_relation').value;
    var client_type = 'JOI';

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
    }

    if (occupation === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select occupation",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (nature_of_business === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select nature of business",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (designation === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select designation",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (source_of_funds === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select source of funds",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (source_of_funds_text === 'Other') {
        if (other_source_of_funds_desc === '') {
            Swal.fire({
                title: "Missing information",
                text: "Kindly provide details on other source of funds",
                icon: "warning",
                confirmButtonText: "Ok"
            });
            return false;
        } else {
            source_of_funds_text = other_source_of_funds_desc;
        }
    }

    if (expected_account_activity === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select expected account activity",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (income_distribution === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select income distribution",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (tax_exempted === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select taxation option",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (postal_address === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly enter postal address",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (physical_address === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly enter physical address",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
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

    if (risk_profile === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select risk profile",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (client_type === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select client type",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    var cnt = applicants.length;

    var applicant =
    {
        id: cnt + 1,
        first_name: first_name,
        middle_name: middle_name,
        last_name: last_name,
        id_number: id_number,
        pp_number: pp_number,
        mobile_number: mobile_number,
        email: email,
        source_of_funds: source_of_funds,
        source_of_funds_text: source_of_funds_text,
        occupation: occupation,
        occupation_text: occupation_text,
        nature_of_business: nature_of_business,
        name_of_employer: name_of_employer,
        designation: designation,
        expected_account_activity: expected_account_activity,
        expected_account_activity_text: expected_account_activity_text,
        income_distribution: income_distribution,
        income_distribution_text: income_distribution_text,
        pin_number: pin_number,
        tax_exempted: tax_exempted,
        postal_address: postal_address,
        physical_address: physical_address,
        id_card: id_card,
        passport_photo: passport_photo,
        signature: signature,
        risk_profile: risk_profile,
        risk_profile_desc: risk_profile_text,
        client_type: client_type
    };

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
    $('#pp_number').val("");
    $('#mobile_number').val("");
    $('#email').val("");
    $('#source_of_funds').val("-1").trigger("change");
    $('#occupation').val("-1").trigger("change");
    $('#nature_of_business').val("");
    $('#name_of_employer').val("");
    $('#designation').val("");
    $('#expected_account_activity').val("-1").trigger("change");
    $('#income_distribution').val("-1").trigger("change");
    $('#pin_number').val("");
    $('#tax_exempted').val("-1").trigger("change");
    $('#postal_address').val("");
    $('#physical_address').val("");
    document.getElementById("label_national_id").innerHTML = '';
    $('#label_text_national_id').val("");
    document.getElementById("label_passportphoto").innerHTML = '';
    $('#label_text_passportphoto').val("");
    document.getElementById("label_signature").innerHTML = '';
    $('#label_text_signature').val("");
    document.getElementById("label_sigkrapin").innerHTML = '';
    $('#label_text_sigkrapin').val("");
});

$('#save_nok').click(function () {

    var a = $(this).closest(".panel");

    var cnt = noks.length;

    var name = document.getElementById('beneficiary_name').value;
    var id_number = document.getElementById('beneficiary_id_number').value;
    var pp_number = document.getElementById('beneficiary_pp_number').value;
    var email = document.getElementById('beneficiary_email').value;
    var mobile_number = document.getElementById('beneficiary_mobile_number').value;
    //var allocation = document.getElementById('beneficiary_allocation').value;
    var relationship = document.getElementById('beneficiary_relationship').value;
    var relationship_desc = document.getElementById('beneficiary_other_relationship').value;
    var other_relationship = document.getElementById('beneficiary_other_relationship').value;

    if (name === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly enter name",
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
    }

    if (email === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly enter email",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (mobile_number === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly enter mobile",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (relationship === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select relationship",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    var relationship_text = $("#beneficiary_relationship option:selected").text();
    if (relationship_text === 'Other') {
        if (other_relationship === '') {
            Swal.fire({
                title: "Missing information",
                text: "Kindly provide details on other relationship",
                icon: "warning",
                confirmButtonText: "Ok"
            });
            return false;
        }
    }

    var nok =
    {
        id: cnt + 1,
        name: name,
        id_number: id_number,
        pp_number: pp_number,
        email: email,
        mobile_number: mobile_number,
        //allocation: allocation,
        relationship: relationship,
        relationship_name: $("#beneficiary_relationship option:selected").text(),
        relationship_desc: relationship_desc
    };

    if (document.getElementById('nokrecordid').value > 0) {
        const index = noks.findIndex(item => item.id === document.getElementById('nokrecordid').value);
        noks.splice(index, 1);
    }

    noks.push(nok);

    GetNOKData(noks);

    $("#capture-nok").modal("hide").data("bs.modal", null);
});

$("#capture-nok").on("hidden.bs.modal", function (e) {
    $('#nokrecordid').val("");
    $('#beneficiary_name').val("");
    $('#beneficiary_id_number').val("");
    $('#beneficiary_pp_number').val("");
    $('#beneficiary_email').val("");
    $('#beneficiary_mobile_number').val("");
    //$('#beneficiary_allocation').val("");
    $('#beneficiary_relationship').val("").trigger("change");
    $('#beneficiary_other_relationship').val("");
});

$('#clear_national_id').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_national_id").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_national_id").innerHTML = '';
});

$('#clear_passportphoto').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_passportphoto").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_passportphoto").innerHTML = '';
});

$('#clear_signature').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_signature").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_signature").innerHTML = '';
});

$('#clear_utilitybill').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_utilitybill").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_utilitybill").innerHTML = '';
});

$('#clear_proofofbanking').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_proofofbanking").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_proofofbanking").innerHTML = '';
});

$('#clear_krapin').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_krapin").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_krapin").innerHTML = '';
});

$('#clear_proofoffunds').click(function () {
    var container = document.getElementById('uploadedFiles');
    var customer_files = container.textContent.trim();
    var val_to_replace = document.getElementById("label_text_proofoffunds").value;
    customer_files = customer_files.replace(val_to_replace + '|', '');
    document.getElementById('uploadedFiles').innerHTML = customer_files;
    document.getElementById("label_proofoffunds").innerHTML = '';
});

$("#riskq1").bind("change", function () {
    riskq1 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

$("#riskq2").bind("change", function () {
    //console.log($(this).val());
    riskq2 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

$("#riskq3").bind("change", function () {
    //console.log($(this).val());
    riskq3 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

$("#riskq4").bind("change", function () {
    //console.log($(this).val());
    riskq4 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

$("#riskq5").bind("change", function () {
    //console.log($(this).val());
    riskq5 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

$("#riskq6").bind("change", function () {
    //console.log($(this).val());
    riskq6 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

$("#riskq7").bind("change", function () {
    //console.log($(this).val());
    riskq7 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

$("#riskq8").bind("change", function () {
    //console.log($(this).val());
    riskq8 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

$("#riskq9").bind("change", function () {
    //console.log($(this).val());
    riskq9 = $(this).val();
    var risk_total = Number(riskq1) + Number(riskq2) + Number(riskq3) + Number(riskq4) + Number(riskq5) + Number(riskq6) + Number(riskq7) + Number(riskq8) + Number(riskq9);
    riskavg = risk_total / 9;
    var floated = parseFloat(riskavg);
    document.getElementById("riskqsummary").innerHTML = floated.toFixed(2);
});

var handleOnboardingWizards = function () {
    "use strict";
    $("#wizard").bwizard({
        validating: function (e, ui) {
            if (ui.index === 0) {
                // step-1 client details
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-1")) {
                    return false;
                } else {
                    if (Array.isArray(applicants) && !applicants.length) {
                        Swal.fire({
                            title: "Missing information",
                            text: "Kindly provide at least one applicant",
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                        return false;
                    }
                }
            } else if ((ui.index === 1) && (ui.nextIndex > ui.index)) {
                // step-2 NOK details
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-2")) {
                    return false;
                } else {
                    if (Array.isArray(noks) && !noks.length) {
                        Swal.fire({
                            title: "Missing information",
                            text: "Kindly provide at least one next of kin",
                            icon: "warning",
                            confirmButtonText: "Ok"
                        });
                        return false;
                    }
                }
            } else if ((ui.index === 2) && (ui.nextIndex > ui.index)) {
                // step-3 bank details
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-3")) {
                    return false;
                } else {
                    var bank = document.getElementById('bank').value;
                    var branch = document.getElementById('branch').value;

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
                }
            } else if ((ui.index === 3) && (ui.nextIndex > ui.index)) {
                // step-4 uploads
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-4")) {
                    return false;
                }
            } else if ((ui.index === 4) && (ui.nextIndex > ui.index)) {
                // step-5 risk assessment
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-5")) {
                    return false;
                } else {
                    document.getElementById("indemnity_name").innerHTML = applicants[0]['first_name'] + ' ' + applicants[0]['middle_name'] + ' ' + applicants[0]['last_name'];
                    document.getElementById("indemnity_email").innerHTML = applicants[0]['email'];
                }
            } else if ((ui.index === 5) && (ui.nextIndex > ui.index)) {
                // step-6 risk assessment
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-6")) {
                    return false;
                } else {

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

                    /***** Bank details confirmation *****/
                    document.getElementById('confirm_bank').value = $("#bank option:selected").text();
                    document.getElementById('confirm_branch').value = $("#branch option:selected").text();
                    document.getElementById('confirm_account_name').value = document.getElementById('account_name').value;
                    document.getElementById('confirm_account_number').value = document.getElementById('account_number').value;
                    /***** Bank details confirmation *****/

                    /***** Applicants confirmation table *****/
                    table = $('#confirm_applicantdatatable').dataTable();
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

                    /***** NOK confirmation table *****/
                    table = $('#confirm_nokdatatable').dataTable();
                    oSettings = table.fnSettings();
                    table.fnClearTable(this);

                    json = noks;
                    for (var i = 0; i < json.length; i++) {
                        var item = json[i];
                        table.oApi._fnAddData(oSettings, item);
                    }
                    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
                    table.fnDraw();
                    /***** NOK confirmation table *****/
                }
            } else if ((ui.index === 6) && (ui.nextIndex > ui.index)) {
                // step-7 risk assessment
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-7")) {
                    return false;
                } else {
                    var a = $(this).closest(".panel");

                    Swal.fire({
                        title: "Are you sure?",
                        text: "you want to proceed with onboarding?",
                        icon: "question",
                        showCancelButton: true,
                        confirmButtonText: "Proceed",
                        reverseButtons: true
                    }).then((result) => {
                        if (result.isConfirmed) {
                            var bank = document.getElementById('bank').value;
                            var branch = document.getElementById('branch').value;
                            var account_name = document.getElementById('account_name').value;
                            var account_number = document.getElementById('account_number').value;

                            const container = document.getElementById('uploadedFiles');
                            const customer_files = container.textContent.trim();

                            //console.log(customer_files);

                            var bank_details_obj =
                            {
                                bank: bank, branch: branch,
                                account_name: account_name,
                                account_number: account_number
                            };

                            var risk_assessment_obj =
                            {
                                riskq1: riskq1, riskq2: riskq2,
                                riskq3: riskq3, riskq4: riskq4,
                                riskq5: riskq5, riskq6: riskq6,
                                riskq7: riskq7, riskq8: riskq8,
                                riskq9: riskq9, riskavg: riskavg
                            };

                            var parameters = {
                                applicant_details: applicants,
                                customer_files: customer_files, bank_details: bank_details_obj,
                                beneficiary_details: noks, risk_assessment: risk_assessment_obj
                            };

                            $.ajax({
                                url: "/ClientSetup/OnboardClient",
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
                                    document.getElementById("summary_system_reference").innerHTML = data.system_ref;

                                    var buttons = document.getElementsByClassName("previous");

                                    for (var i = 0; i < buttons.length; i++) {
                                        buttons[i].setAttribute("aria-disabled", "true");
                                        buttons[i].setAttribute("class", "previous disabled");
                                    }

                                    if (data.error_code === '00') {
                                        document.getElementById("summary_status").innerHTML = "Success";
                                        document.getElementById("summary_status").classList = "label label-success";
                                        Swal.fire({
                                            title: "Success",
                                            text: data.error_desc,
                                            icon: "success",
                                            confirmButtonText: "Ok"
                                        });
                                    } else {
                                        document.getElementById("summary_status").innerHTML = "Failed";
                                        document.getElementById("summary_status").classList = "label label-danger";
                                        Swal.fire({
                                            title: "Failed",
                                            text: data.error_desc,
                                            icon: "error",
                                            confirmButtonText: "Ok"
                                        });
                                    }

                                    $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
                                }
                            });
                        } else {
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