
$(document).ready(function () {
    App.init();

    InitiateEditableDataTable.init();

    GetBeneficiary();

    GetBeneficiaryType();

    GetDocumentType();
});

$('#search').click(function () {


    $('.modal-body #full_name').val(null);
    $('.modal-body #cif').val(null);

    var a = $(this).closest(".panel");

    var office_email = document.getElementById('email_address').value;

    var parameters = {
        office_email: office_email
    };

    $.ajax({
        url: "/Employee/SearchBeneficiary",
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
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            var json = $.parseJSON(JSON.stringify(data));

            console.log(json);

            var emailInput = $('.modal-body #email_address');
            var emailErrorSpan = $('#emailError');

            // Check if the JSON array is empty
            if (json.length === 0) {
                emailErrorSpan.text('Email does not exist'); // Set the text content of the span
            } else {
                // Check if the email_address is null or empty
                if (!json[0]["EMAIL_ADDRESS"]) {
                    emailErrorSpan.text('Email does not exist'); // Set the text content of the span
                } else {
                    emailErrorSpan.text(''); // Clear the span text if email exists
                    $('.modal-body #full_name').val(json[0]["CUSTOMER_FULL_NAME"]);
                    $('.modal-body #cif').val(json[0]["CIF"]);
                }

                emailInput.val(json[0]["EMAIL_ADDRESS"]);
            }

        },
        error: function (xhr, textStatus, errorThrown) {
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            Swal.fire({
                title: "Failed",
                text: "Record could not be saved " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
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
                        "targets": 7,
                        "render": function (data, type, row, meta) {
                            if (row.approved === 0) {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.approved === 1) {
                                return '<span class="label label-success">Approved</span>';
                            }
                        }
                    },
                    {
                        "targets": 8,
                        "render": function (data, type, row, meta) {


                            return "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>";


                        }
                    },
                    {
                        "targets": 9,
                        "render": function (data, type, row, meta) {


                            return "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash'></i> Delete</a>";

                        }
                    }

                ],
                "aoColumns": [
                    { "data": "account_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "email_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "type_id", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "document_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "document_number", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "approved", "autoWidth": true, "sDefaultContent": "n/a" },
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
                $('.modal-body #email_address').val(json["email_address"]);
                $('.modal-body #cif').val(json["cif"]);
                $('.modal-body #full_name').val(json["account_name"]);
                $('.modal-body #type').val(json["nok_type_id"]).trigger("change");
                $('.modal-body #documenttype').val(json["document_id_type"]).trigger("change");
                $('.modal-body #document_number').val(json["document_number"]);
                $('.modal-body #name').val(json["name"]);

                $("#capture-record").appendTo("body").modal("show");
            }

            $('#editabledatatable').on("click", 'a.delete', function (e) {
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

function GetBeneficiary() {
    $.get('GetRecords', { module: 'beneficiary' }, function (data) {
        getData(data);
    });
}

function GetBeneficiaryType() {
    $.get('GetRecords', { module: 'beneficiary_type' }, function (data) {
        $("#type").get(0).options.length = 0;
        $("#type").get(0).options[0] = new Option("Please Select Beneficiary Type", "-1");

        $.each(data, function (index, item) {
            $("#type").get(0).options[$("#type").get(0).options.length] = new Option(item.type, item.id);
        });

        $("#type").bind("change", function () {
            var str = $("#type option:selected").text();
        });
    });
}


function GetDocumentType() {
    $.get('GetRecords', { module: 'client_document' }, function (data) {
        $("#documenttype").get(0).options.length = 0;
        $("#documenttype").get(0).options[0] = new Option("Please Select Document Type", "-1");

        $.each(data, function (index, item) {
            $("#documenttype").get(0).options[$("#documenttype").get(0).options.length] = new Option(item.NAME, item.RECORD_ID);
        });

        $("#documenttype").bind("change", function () {
            var str = $("#documenttype option:selected").text();
        });
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

$('#save').click(function () {
    var a = $(this).closest(".panel");

    var id = document.getElementById('recordid').value;
    var cif = document.getElementById('cif').value;
    var type = document.getElementById('type').value;
    var documenttype = document.getElementById('documenttype').value;
    var document_number = document.getElementById('document_number').value
    var name = document.getElementById('name').value;

    if (type === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select next of kin type",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (documenttype === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select document type",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }


    var parameters = {
        id: id, cif: cif, type: type, name: name,
        documenttype: documenttype, document_number: document_number
    };

    $.ajax({
        url: "/Employee/CreateBeneficiary",
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
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            if (data == 'Success') {

                $("#capture-record").modal("hide").data("bs.modal", null);

                swal.fire({
                    title: "Success",
                    text: data[0].error_desc,
                    icon: "success",
                    confirmButtonText: "Ok"
                });
            } else {
                Swal.fire({
                    title: "Failed",
                    text: data,
                    icon: "error",
                    confirmButtonText: "Ok"
                });
            }

            GetBeneficiary();
        },
        error: function (xhr, textStatus, errorThrown) {
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            Swal.fire({
                title: "Failed",
                text: "Create could not be completed " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
});

$("#capture-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");
    $('#email_address').val("");
    $('#cif').val("");
    $('#full_name').val("");
    $('#type').val("").trigger("change");
    $('#documenttype').val("").trigger("change");
    $('#document_number').val("");
    $('#name').val("");
});