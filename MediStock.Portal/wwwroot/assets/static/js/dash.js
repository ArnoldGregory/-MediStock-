
$(document).ready(function () {
    App.init();

    InitiateEditableDataTable.init();
    InitiateBankDetailsDataTable.init();
    InitiateNextOfKinDetailsDataTable.init();
    InitiateBeneficiariesDetailsDataTable.init();
    InitiateEmployeeDocumentEditableDataTable.init();
    GetCustomers();

    DashboardV2.init();


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
        console.log(data);
        getData(data);
    });
}

function getData(jsonstring) {
    table = $('#editabledatatable').dataTable();
    oSettings = table.fnSettings();
    table.fnClearTable(this);

    var json = $.parseJSON(JSON.stringify(jsonstring));

    for (var i = 0; i < json.length; i++) {
        var item = json[i];
        table.oApi._fnAddData(oSettings, item);
    }
    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
    table.fnDraw();
}

function GetBanks(employee_id) {
    
    $.get('GetRecords', { module: 'employee_banks_byid', param: employee_id }, function (data) {
        table = $('#bankdetailsdatatable').dataTable();
        getdatatable(table, data);
    });
}
function GetNOK(employee_id) {

    $.get('GetRecords', { module: 'next_of_kinbyid', param: employee_id }, function (data) {
        table = $('#nextofkindatatable').dataTable();
        getdatatable(table, data);
    });
}

function GetBeneficiary(employee_id) {

    $.get('GetRecords', { module: 'beneficiary_byid', param: employee_id }, function (data) {
        table = $('#beneficiariesdatatable').dataTable();
        getdatatable(table, data);
    });
}

function GetDocuments(employee_id) {

    $.get('GetRecords', { module: 'documents_byid', param: employee_id }, function (data) {
        table = $('#employeedocumentsdatatable').dataTable();
        getdatatable(table, data);
    });
}
function getdatatable(table, jsonstring) {
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

handleDashboadData = function () {

    var a = $(this).closest(".panel");

    $.ajax({
        url: "GetDashboardData/Dashboard",
        type: "GET",
        beforeSend: function () {
            if (!$(a).hasClass("panel-loading")) {
                var t = $(a).find(".panel-body"),
                    i = '<div class="panel-loader"><span class="spinner-small"></span></div>';

                $(a).addClass("panel-loading"), $(t).prepend(i);
            }
        },
        success: function (data) {
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            var jsonapplications = JSON.parse(data.widget_data);

            document.getElementById('statistic_one').innerHTML = jsonapplications[0]["statistic_one"];
            document.getElementById('statistic_two').innerHTML = jsonapplications[0]["statistic_two"];
            document.getElementById('statistic_three').innerHTML = jsonapplications[0]["statistic_three"];
            document.getElementById('statistic_four').innerHTML = jsonapplications[0]["statistic_four"];
        },
        error: function (xhr, textStatus, errorThrown) {
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            Swal.fire({
                title: "Failed",
                text: "Could not complete operation on account of " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
}, DashboardV2 = function () {
    "use strict";
    return {
        init: function () {
            handleDashboadData();
        }
    };
}();