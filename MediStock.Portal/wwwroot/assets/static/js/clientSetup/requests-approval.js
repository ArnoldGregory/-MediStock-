
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();
    InitiateBankDetailsDataTable.init();
    InitiateNextOfKinDetailsDataTable.init();
    InitiateBeneficiariesDetailsDataTable.init();
    InitiateEmployeeDocumentEditableDataTable.init();

    GetCustomers();
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
                        "targets": 8,
                        "render": function (data, type, row, meta) {
                            if (row.approve === 0) {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.approve === 1) {
                                return '<span class="label label-primary">Approved</span>';
                            } else if (row.approve === 2) {
                                return '<span class="label label-danger">Rejected</span>';
                            } else if (row.approve === 3) {
                                return '<span class="label label-default">Cancelled</span>';
                            }
                        }
                    }

                ],
                "aoColumns": [
                    { "data": "CUSTOMER_FULL_NAME", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "DOCUMENT_NUMBER", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "TAX_ID_NUMBER", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "MOBILENUMBER", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "EMAIL_ADDRESS", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "sextype", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "contract_type1", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "approve", "autoWidth": true, "sDefaultContent": "n/a" },
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
                $('.modal-body #physical_address').val(json["PHYSICAL_ADDRESS"]);
                $('.modal-body #postal_address').val(json["POSTAL_ADDRESS"]);
                $('.modal-body #firstname').val(json["CUSTOMER_FIRST_NAME"]);
                $('.modal-body #secondname').val(json["CUSTOMER_MIDDLE_NAME"]);
                $('.modal-body #lastname').val(json["CUSTOMER_LAST_NAME"]);
                $('.modal-body #taxid').val(json["TAX_ID_NUMBER"]);
                $('.modal-body #title').val(json["TITLE"]);
                $('.modal-body #documentnumber').val(json["DOCUMENT_NUMBER"]);
                $('.modal-body #badgeid').val(json["BADGE_ID"]);
                $('.modal-body #mobile').val(json["MOBILENUMBER"]);
                $('.modal-body #alternatemobile').val(json["alternatemobile"]);
                $('.modal-body #office_email').val(json["EMAIL_ADDRESS"]);
                $('.modal-body #otheremail').val(json["ALTERNATE_EMAILADDRESS"]);
                $('.modal-body #onboardingdate').val(json["onboardingdate"]);
                $('.modal-body #dob').val(json["DOB"]);
                $('.modal-body #contracttype').val(json["contract_type1"]);
                $('.modal-body #department').val(json["department_name"]);
                $('.modal-body #language').val(json["language_name"]);
                $('.modal-body #country').val(json["country_label"]);
                $('.modal-body #documenttype').val(json["DOC_TYPE_NAME"]);
                $('.modal-body #sexetype').val(json["sextype"]);
                $('.modal-body #maritalstatus').val(json["marital_type"]);
                $('.modal-body #nhif_number').val(json["NHIF_NUMBER"]);
                $('.modal-body #nssf_number').val(json["NSSF_NUMBER"]);

                var rec = json["CIF"];
                console.log(rec);

                var is_locked = json["locked"];
                //console.log(is_locked);

                if (typeof is_locked === 'string') {
                    is_locked = JSON.parse(json["locked"].toLowerCase());
                }

                if (is_locked) {
                    $('.modal-body #locked').prop({ checked: true });
                } else {
                    $('.modal-body #locked').prop({ checked: false });
                }

                var is_deceased = json["deceased"];
                //console.log(is_deceased);

                if (typeof is_deceased === 'string') {
                    is_deceased = JSON.parse(json["deceased"].toLowerCase());
                }

                if (is_deceased) {
                    $('.modal-body #deceased').prop({ checked: true });
                } else {
                    $('.modal-body #deceased').prop({ checked: false });
                }


                var is_whereaboutunknown = json["whereaboutunknown"];
                //console.log(is_whereaboutunknown);

                if (typeof is_whereaboutunknown === 'string') {
                    is_whereaboutunknown = JSON.parse(json["whereaboutunknown"].toLowerCase());
                }

                if (is_whereaboutunknown) {
                    $('.modal-body #whereaboutunknown').prop({ checked: true });
                } else {
                    $('.modal-body #whereaboutunknown').prop({ checked: false });
                }


                var is_indefiniteleave = json["indefiniteleave"];

                //console.log(is_indefiniteleave);

                if (typeof is_indefiniteleave === 'string') {
                    is_indefiniteleave = JSON.parse(json["indefiniteleave"].toLowerCase());
                }

                if (is_indefiniteleave) {
                    $('.modal-body #indefiniteleave').prop({ checked: true });
                } else {
                    $('.modal-body #indefiniteleave').prop({ checked: false });
                }

                var profilepic = json["photo"];
                //console.log(profilepic);

                GetBanks(rec);
                GetNOK(rec);
                GetBeneficiary(rec);
                GetDocuments(rec);
                

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
                var rec = json["CIF"];
                console.log(json);

                $('.modal-body #recordid').val(rec);
                $('.modal-body #status').val(json["approve"]).trigger("change");
                $('.modal-body #comment').val(json["comment"]);


                $("#capture-approval-record").appendTo("body").modal("show");
            }


        }
    };
}();

var InitiateBankDetailsDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#bankdetailsdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "columnDefs": [
                    {
                        "targets": 5,
                        "render": function (data, type, row, meta) {
                            if (row.auth_status === 'U') {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.status === 'A') {
                                return '<span class="label label-success">Approved</span>';
                            } else if (row.status === 'R') {
                                return '<span class="label label-danger">Rejected</span>';
                            } else if (row.status === 'C') {
                                return '<span class="label label-default">Cancelled</span>';
                            }
                        }
                    }

                ],

                "aoColumns": [
                    { "data": "currency_code", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "account", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "account_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "bank_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "branch_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "auth_status", "autoWidth": true, "sDefaultContent": "n/a" },
                ]
            });

            
        }
    };
}();

var InitiateNextOfKinDetailsDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#nextofkindatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "columnDefs": [
                    {
                        "targets": 6,
                        "render": function (data, type, row, meta) {
                            if (row.approved === 0) {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.approved === 1) {
                                return '<span class="label label-success">Approved</span>';
                            }
                        }
                    }

                ],
                "aoColumns": [
                    { "data": "cif", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "type_id", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "document_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "document_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "approved", "autoWidth": true, "sDefaultContent": "n/a" },
                ]
            });

           
        }
    };
}();

var InitiateBeneficiariesDetailsDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#beneficiariesdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "columnDefs": [
                    {
                        "targets": 6,
                        "render": function (data, type, row, meta) {
                            if (row.approved === 0) {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.approved === 1) {
                                return '<span class="label label-success">Approved</span>';
                            }
                        }
                    }

                ],
                "aoColumns": [
                    { "data": "cif", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "type_id", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "document_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "document_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "approved", "autoWidth": true, "sDefaultContent": "n/a" },
                    
                ]
            });

        }
    };
}();

var InitiateEmployeeDocumentEditableDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#employeedocumentsdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "columnDefs": [
                    {
                        "targets": 2,
                        "render": function (data, type, row, meta) {
                            return "<a href='" + row.link + "' target='_blank' class='btn btn-info btn-xs download'><i class='fas fa-eye'></i> View</a>";
                        }
                    }
                ],

                "aoColumns": [
                   
                    { "data": "document_desc", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "original_file_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa-solid fa-trash-can'></i> Delete</a>"
                    },

                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa-solid fa-trash-can'></i> Delete</a>"
                    }
                ]
            });


            $('#employeedocumentsdatatable').on("click", 'a.delete', function (e) {
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

function GetCustomers() {
    $.get('GetRecords', { module: 'customer_approval' }, function (data) {
        getData(data);
    });
}

function GetBanks(employee_id) {

    $.get('GetRecords', { module: 'employee_banks_byid', param:employee_id }, function (data) {
        geBanktData(data);
    });
}
function GetNOK(employee_id) {

    $.get('GetRecords', { module: 'next_of_kinbyid', param:employee_id }, function (data) {
        getNOK(data);
    });
}

function GetBeneficiary(employee_id) {

    $.get('GetRecords', { module: 'beneficiary_byid', param:employee_id }, function (data) {
        getBeneficiariesById(data);
    });
}

function GetDocuments(employee_id) {

    $.get('GetRecords', { module: 'documents_byid', param:employee_id }, function (data) {
        getDocumentsById(data);
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

function GetDocumentType() {
    $.get('GetRecords', { module: 'DOCUMENT_TYPE' }, function (data) {
        $("#documenttype").get(0).options.length = 0;
        $("#documenttype").get(0).options[0] = new Option("Please Select Document Type", "-1");

        $.each(data, function (index, item) {
            $("#documenttype").get(0).options[$("#documenttype").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#documenttype").bind("change", function () {
            var str = $("#documenttype option:selected").text();
        });
    });
}

function GetLanguage() {
    $.get('GetRecords', { module: 'LANGUAGE' }, function (data) {
        $("#language").get(0).options.length = 0;
        $("#language").get(0).options[0] = new Option("Please Select Language", "-1");

        $.each(data, function (index, item) {
            $("#language").get(0).options[$("#language").get(0).options.length] = new Option(item.name, item.id);
        });

        $("#language").bind("change", function () {
            var str = $("#language option:selected").text();
        });
    });
}


function GetCountry() {
    $.get('GetRecords', { module: 'countries' }, function (data) {
        $("#country").get(0).options.length = 0;
        $("#country").get(0).options[0] = new Option("Please Select Country", "-1");

        $.each(data, function (index, item) {
            $("#country").get(0).options[$("#country").get(0).options.length] = new Option(item.COUNTRY, item.COUNTRY_CODE);
        });

        $("#country").bind("change", function () {
            var str = $("#country option:selected").text();
        });
    });
}

function GetContract() {
    $.get('GetRecords', { module: 'CONTRACT' }, function (data) {
        $("#contracttype").get(0).options.length = 0;
        $("#contracttype").get(0).options[0] = new Option("Please Select Contract Type", "-1");

        $.each(data, function (index, item) {
            $("#contracttype").get(0).options[$("#contracttype").get(0).options.length] = new Option(item.contract_type_description, item.contract_type);
        });

        $("#contracttype").bind("change", function () {
            var str = $("#contracttype option:selected").text();
        });
    });
}

function GetDepartment() {
    $.get('GetRecords', { module: 'DEPARTMENT' }, function (data) {
        $("#department").get(0).options.length = 0;
        $("#department").get(0).options[0] = new Option("Please Department", "-1");

        $.each(data, function (index, item) {
            $("#department").get(0).options[$("#department").get(0).options.length] = new Option(item.department, item.id);
        });

        $("#department").bind("change", function () {
            var str = $("#department option:selected").text();
        });
    });
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

function geBanktData(jsonstring) {
    table = $('#bankdetailsdatatable').dataTable();
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

function getNOK(jsonstring) {
    table = $('#nextofkindatatable').dataTable();
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
function getBeneficiariesById(jsonstring) {
    table = $('#beneficiariesdatatable').dataTable();
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
function getDocumentsById(jsonstring) {
    table = $('#employeedocumentsdatatable').dataTable();
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
    var status = document.getElementById("status").value;
    var comment = document.getElementById("comment").value;
    var parameters = {
        id: record_id,
        module: "employee_approval",
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

                        GetCustomers();

                        Swal.fire({
                            title: "Approved",
                            text: data.error_desc,
                            icon: "success",
                            confirmButtonText: "Ok"
                        });
                    } else {
                        Swal.fire({
                            title: "Error",
                            text: data.error_desc,
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

$("#view-record").on("hidden.bs.modal", function (e) {

    $("#collapseZero").collapse('hide');
    $("#bankdetails").collapse('hide');
    $("#nextofkindetails").collapse('hide');
    $("#beneficiariesdetails").collapse('hide');
    $("#documentsdetails").collapse('hide');

});

$("#capture-approval-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val(""); 
    $('#comment').val("");
    $('#status').val("").trigger("change");
});


$("#capture-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");  
    $('#firstname').val("");
    $('#secondname').val("");
    $('#lastname').val("");
    $('#taxid').val("");
    $('#documentnumber').val("");
    $('#mobilenumber').val("");
    $('#alternatemobile').val("");
    $('#office_email').val("");
    $('#otheremail').val("");
    $('#onboardingdate').val("");
    $('#title').val("");
    $('#dob').val("");
    $('#badgeid').val("");
    $('#physical_address').val("");
    $('#postal_address').val("");
    $('#documenttype').val("").trigger("change");
    $('#country').val("").trigger("change");
    $('#language').val("").trigger("change");
    $('#contracttype').val("").trigger("change");
    $('#department').val("").trigger("change");
    $('#sexetype').val("").trigger("change");
    $('#maritalstatus').val("").trigger("change");

    document.getElementById("profilepic").src = "/assets/static/img/profile-pics/user.jpg";

    document.getElementById("locked").checked = false;
    document.getElementById("deceased").checked = false;
    document.getElementById("whereaboutunknown").checked = false;
    document.getElementById("indefiniteleave").checked = false;
});