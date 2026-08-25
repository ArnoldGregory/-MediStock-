$(document).ready(function () {
    App.init();

    InitiateEditableDataTable.init();

    InitiatesignatoriesdetailsViewtDataTable.init();
  
    InitiatesigntorylDocumentDataTable.init();
   
    InitiateCorporateDocumentDataTable.init();

    GetOnBoardingRequest();
    
});

var principals = [];
var dependants = [];

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
                        "targets": 3,
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
                    { "data": "fullname", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "email_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "mobile_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n/a" },
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
                $('.modal-body #full_name').val(json["fullname"]);
                $('.modal-body #id_number').val(json["id_number"]);
                $('.modal-body #email_address').val(json["email_address"]);
                $('.modal-body #mobile_number').val(json["mobile_number"]);
                $('.modal-body #request_date').val(json["created_on"]);

                var rec = json["id"];


                GetCorporateDetails(rec);
                GetSignatoriesDetails(rec);
                GetBankDetails(rec);
                GetSignatoryMemberDocuments(rec);
                GetCorporateDocuments(rec);

                $("#view-record").appendTo("body").modal("show");
            }

            



            var isApproval = null;


            //Approve

            $('#editabledatatable').on("click", 'a.approve', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];


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
               

                $('.modal-body #recordid').val(rec);
                $('.modal-body #status').val(json["approve"]).trigger("change");
                $('.modal-body #comment').val(json["comment"]);


                $("#capture-approval-record").appendTo("body").modal("show");
            }


        }
    };
}();

var InitiatesignatoriesdetailsViewtDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#signatoriesdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },

                "aoColumns": [
                    { "data": "first_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "middle_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "last_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "id_number", "bSortable": false, "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "mobile_number", "bSortable": false, "autoWidth": true, "sDefaultContent": "n/a" }

                ]
            });


        }
    };
}();

var InitiatesigntorylDocumentDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#signatorydocumentsdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },

                "aoColumns": [
                    { "data": "user_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "filetype", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "file_number", "autoWidth": true, "sDefaultContent": "n/a" },

                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-primary btn-xs download'> Download </a>"
                    }


                ]
            });

            var isView = null;

            //View
            $('#signatorydocumentsdatatable').on("click", 'a.download', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];


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

                window.open("/assets/client_documents/" + json["file_number"], '_blank');


            }


        }
    };
}();


var InitiateCorporateDocumentDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#coporatedocumentsdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },

                "aoColumns": [
                    { "data": "filetype", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "file_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "file_number", "autoWidth": true, "sDefaultContent": "n/a" },

                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-primary btn-xs download'> Download </a>"
                    }


                ]
            });

            var isView = null;

            //View
            $('#coporatedocumentsdatatable').on("click", 'a.download', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];


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


                window.open("/assets/client_documents/" + json["file_number"], '_blank');

               
            }


        }
    };
}();


function GetOnBoardingRequest() {
    $.get('GetRecords', { module: 'onboardrequest' }, function (data) {
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


function GetCorporateDetails(applicant_id) {
    $.get('GetRecords', { module: 'corporate_details', param: applicant_id }, function (data) {

        var result = data;
        if (Array.isArray(data) && data.length > 0) {
            if (data[0].reg_name != null) {
                document.getElementById("cor_reg_name").value = data[0].reg_name;
            }

            document.getElementById("cor_reg_number").value = data[0].reg_number;
            document.getElementById("cor_org_type").value = data[0].org_name;
            document.getElementById("cor_reg_date").value = data[0].created_on;
            document.getElementById("cor_pin_number").value = data[0].kra_pin;
            document.getElementById("cor_email").value = data[0].email_address;
            document.getElementById("cor_contact_person").value = data[0].contact_person;
            document.getElementById("cor_contact_person_number").value = data[0].contact_person_number;
            document.getElementById("cor_contact_narration").value = data[0].contact_person;;
            document.getElementById("cor_telephone_number").value = data[0].telephone;
            document.getElementById("cor_country").value = data[0].country;
            document.getElementById("cor_city").value = data[0].city;
            document.getElementById("cor_postal_address").value = data[0].postal_address;
            document.getElementById("cor_physical_address").value = data[0].physical_address;
        }
        

    });
}

function GetBankDetails(applicant_id) {
    $.get('GetRecords', { module: 'bank_details', param: applicant_id }, function (data) {

        var result = data;

        if (Array.isArray(data) && data.length > 0) {
            if (data[0].bank_name != null) {

                document.getElementById("bank").value = data[0].bank_name;
            }

            document.getElementById("branch").value = data[0].branch_name;
            document.getElementById("account_name").value = data[0].account_name;
            document.getElementById("account_number").value = data[0].account_number;
        }

    });
}



function GetSignatoriesDetails(applicant_id) {

    var a = $(this).closest(".panel");

    var parameters = { module: 'signatories_details', param: applicant_id };

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
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            table = $('#signatoriesdatatable').dataTable();
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
        },
        error: function (xhr, textStatus, errorThrown) {
            //$.unblockUI();
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

function GetSignatoryMemberDocuments(applicant_id) {

    var a = $(this).closest(".panel");

    var parameters = { module: 'signatories_document_details', param: applicant_id };

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
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            table = $('#signatorydocumentsdatatable').dataTable();
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
        },
        error: function (xhr, textStatus, errorThrown) {
            //$.unblockUI();
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

function GetCorporateDocuments(applicant_id) {

    var a = $(this).closest(".panel");

    var parameters = { module: 'corporate_document_details', param: applicant_id };

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
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            table = $('#coporatedocumentsdatatable').dataTable();
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
        },
        error: function (xhr, textStatus, errorThrown) {
            //$.unblockUI();
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


$('#SaveApproval').on("click", function (e) {
    e.preventDefault();
    var a = $(this).closest(".panel");

    var nRow = $(this).parents('tr')[0];
    var record_id = document.getElementById("recordid").value;
    var status = document.getElementById("status").value;
    var comment = document.getElementById("comment").value;
    var parameters = {
        id: record_id,
        action_flag: status,
        comment: comment
    };

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
                url: "/ClientSetup/OnboardApprove",
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

                        GetOnBoardingRequest();

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
                        text: "Record could not be approved something went wrong " + errorThrown,
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
    $("#corporatedetails").collapse('hide');
    $("#signatoriesdetails").collapse('hide');
    $("#signatorydocumentsdatatable").collapse('hide');
    $("#bankdetails").collapse('hide');
    $("#uploaddetails").collapse('hide');

});


/*
Template Name: Color Admin - Responsive Admin Dashboard Template build with Twitter Bootstrap 3.3.7 & Bootstrap 4.0.0-Alpha 6
Version: 3.0.0
Author: Sean Ngu
Website: http://www.seantheme.com/color-admin-v3.0/admin/html/
*/

function GetMyProfileDetails() {
    $.get('GetMyProfileData', function (data) {
        //document.getElementById("username").innerHTML = data[0].username;
        //document.getElementById("phonenumber").innerHTML = data[0].phonenumber;
        //document.getElementById("emailaddress").innerHTML = data[0].emailaddress;
        //document.getElementById("profile").innerHTML = data[0].profile;
       // document.getElementById("createdon").innerHTML = data[0].createdon;
        //document.getElementById("menutoggler").innerHTML = data[0].menulayout;
        document.getElementById("profilepic").src = "/assets/static/img/profile-pics/" + data[0].profilepic;
        document.getElementById("rightprofilepic").src = "/assets/static/img/profile-pics/" + data[0].profilepic;
    });
}

var StartRead1 = function () {
    var file = document.getElementById('img-file-input').files[0];
    var reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = addImg;
};

function addImg(imgsrc) {
    var img = document.getElementById('profilepic');
    var client_id = document.getElementById('recordid').value;



    //upload to server
    var parameters = {
        name: 'principal_profilepic',
        value: imgsrc.target.result,
        client_id: client_id
    };

    $.ajax({
        url: "/Data/UpdateClientProfilePic",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(parameters),
        dataType: "json",
        success: function (data) {
            if (data === 'Success') {
                GetProfilePic(client_id);
            } else {
                Swal.fire({
                    title: "Failed",
                    text: data,
                    type: "error",
                    confirmButtonText: "Ok"
                });
            }
        }
    });
}

$("#capture-approval-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");
    $('#comment').val("");
    $('#status').val("").trigger("change");
});
$("#view-record").on("hidden.bs.modal", function (e) {
    $('#full_name').val("");
    $('#email_address').val("");
    $('#mobile_number').val("");
    $('#request_date').val("");
    $('#cor_reg_name').val("");
    $('#cor_reg_number').val("");
    $('#cor_org_type').val("");
    $('#cor_reg_date').val("");
    $('#cor_pin_number').val("");
    $('#cor_email').val("");
    $('#cor_contact_person').val("");
    $('#cor_contact_person_number').val("");
    $('#cor_contact_narration').val("");
    $('#cor_telephone_number').val("");
    $('#cor_country').val("");
    $('#cor_city').val("");
    $('#cor_postal_address').val("");
    $('#cor_physical_address').val("");
    $('#bank').val("");
    $('#branch').val("");
    $('#account_name').val("");
    $('#account_number').val("");
});




