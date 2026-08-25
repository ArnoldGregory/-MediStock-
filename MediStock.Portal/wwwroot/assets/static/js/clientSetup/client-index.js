$(document).ready(function () {
    App.init();

    InitiateEditableDataTable.init();

    InitiatesignatoriesdetailsViewtDataTable.init();

    InitiatesigntorylDocumentDataTable.init();

    InitiateCorporateDocumentDataTable.init();

    InitiateEmployeeEditableDataTable.init();

    GetClients();


    
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
                        "targets": 5,
                        "render": function (data, type, row, meta) {
                            return "<div class='input-group-btn input-group-btn-xs'>" +
                                "   <button type='button' class='btn btn-primary btn-xs'>Action</button> " +
                                "   <button type='button' class='btn btn-primary btn-xs dropdown-toggle' data-toggle='dropdown'> " +
                                "       <span class='caret'></span> " +
                                "   </button> " +
                                "   <ul class='dropdown-menu'> " +
                                "       <li><a href='#' class='dropdown-item employee'><i class='fas fa-users'></i> Employees</a></li> " +
                                "       <li><a href='#' class='dropdown-item view'><i class='fas fa-eye'></i> View</a></li> " +
                                "       <li class='divider'></li> " +
                                "       <li><a href='#' class='dropdown-item edit'><i class='fa fa-edit'></i> Edit</a></li> " +
                                "       <li><a href='#' class='dropdown-item delete'><i class='fa fa-trash'></i> Delete</a></li> " +
                                "   </ul> " +
                                "</div>";
                        }
                    }
                ],
                "aoColumns": [
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "reg_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "telephone", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "contact_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "contact_mobile", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "bSortable": false, "sDefaultContent": "n/a" }
                ]
            });

            //Emloyees 
            $('#editabledatatable').on("click", 'a.employee', function (e) {
                e.preventDefault();
                var a = $(this).closest(".panel");

                var nRow = $(this).parents('tr')[0];
                var aData = oTable.fnGetData(nRow);
                var jqTds = $('>td', nRow);

                var json = JSON.parse(JSON.stringify(aData));

                $('#client_id').val($(nRow).attr("recid"));

                $('#selectedclient').text('Showing Employes for Client  ' + json["name"]);

                var rec = $(this).parents('tr').attr("recid");

                Getmployee(rec);


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



                var rec = json["onboarding_id"];

                //console.log(rec);

                GetCorporateDetails(rec);
                GetSignatoriesDetails(rec);
                GetBankDetails(rec);
                GetSignatoryMemberDocuments(rec);
                GetCorporateDocuments(rec);

                $("#view-record").appendTo("body").modal("show");
            }






            var isEditing = null;

            //Edit
            $('#editabledatatable').on("click", 'a.edit', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                //console.log($(this).parents('tr').attr("recid"));

                //console.log(nRow);

                if (isEditing !== null && isEditing != nRow) {
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
                $('.modal-body #full_name').val(json["full_name"]);
                $('.modal-body #id_number').val(json["id_number"]);
                $('.modal-body #serial_number').val(json["serial_number"]);
                $('.modal-body #pin_number').val(json["pin_number"]);
                $('.modal-body #office_phone').val(json["office_phone"]);
                $('.modal-body #home_phone').val(json["home_phone"]);
                $('.modal-body #mobile_phone').val(json["mobile_phone"]);
                $('.modal-body #office_email').val(json["office_email"]);
                $('.modal-body #other_email').val(json["other_email"]);
                $('.modal-body #physical_address').val(json["physical_address"]);
                $('.modal-body #postal_address').val(json["postal_address"]);

                var is_sys_admin = json["locked"];

                if (typeof is_sys_admin === 'string') {
                    is_sys_admin = JSON.parse(json["locked"].toLowerCase());
                }

                if (is_sys_admin) {
                    $('.modal-body #locked').prop({ checked: true });
                } else {
                    $('.modal-body #locked').prop({ checked: false });
                }

                $("#capture-record").appendTo("body").modal("show");
            }

            //Delete an Existing Row
            $('#editabledatatable').on("click", 'a.delete', function (e) {
                e.preventDefault();
                var a = $(this).closest(".panel");

                var nRow = $(this).parents('tr')[0];

                var rec = $(this).parents('tr').attr("recid");

                //console.log($(this).parents('tr').attr("recid"));
                Swal.fire({
                    title: "Are you sure?",
                    text: "You want to delete this record",
                    icon: "question",
                    showCancelButton: true,
                    confirmButtonText: "Proceed!",
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        //$.blockUI();

                        var parameters = { module: 'clients', id: rec };

                        $.ajax({
                            url: "/ClientSetup/Delete",
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

                                Swal.fire({
                                    title: "Deleted",
                                    text: "Record has been deleted",
                                    icon: "success",
                                    confirmButtonText: "Ok"
                                });
                                getData(data);
                            },
                            error: function (xhr, textStatus, errorThrown) {
                                //$.unblockUI();
                                $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

                                Swal.fire({
                                    title: "Failed",
                                    text: "Record could not be deleted " + errorThrown,
                                    icon: "error",
                                    confirmButtonText: "Ok"
                                });
                            }
                        });
                    } else {
                        e.preventDefault();
                    }
                });
            });
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

                //console.log($(this).parents('tr').attr("recid"));

                console.log(nRow);

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

                window.open("/assets/client_documents/" + json["file_number"], '_blank');


            }


        }
    };
}();

var InitiateEmployeeEditableDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#editabletemplatedetailsdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
               
                "aoColumns": [
                    { "data": "CUSTOMER_FULL_NAME", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "DOCUMENT_NUMBER", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "TAX_ID_NUMBER", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "MOBILENUMBER", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "EMAIL_ADDRESS", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs view'><i class='fas fa-eye'></i> View</a>"
                    }
                ]

            });

           


        }
    };
}();



function Getmployee(clientid) {

    var a = $(this).closest(".panel");

    var parameters = { module: 'client_employee', param: clientid };

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








function GetClients() {
    $.get('GetRecords', { module: 'coporate_clients' }, function (data) {

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

        console.log(result);

        document.getElementById("cor_reg_name").value = data[0].reg_name;
        document.getElementById("cor_reg_number").value = data[0].reg_number;
        document.getElementById("cor_org_type").value = data[0].org_type_desc;
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

    });
}

function GetBankDetails(applicant_id) {
    $.get('GetRecords', { module: 'bank_details', param: applicant_id }, function (data) {

        var result = data;

        console.log(data);

        document.getElementById("bank").value = data[0].bank_name;
        document.getElementById("branch").value = data[0].branch;
        document.getElementById("account_name").value = data[0].account_name;
        document.getElementById("account_number").value = data[0].account_number;

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

















