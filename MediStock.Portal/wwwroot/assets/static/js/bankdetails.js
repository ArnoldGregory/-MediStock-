
$(document).ready(function () {
    App.init();

    InitiateEditableDataTable.init();

    GetEmployeeBanks();

    GetCurrency();

    GetBanks();
});

var selected_branch;

$('#search').click(function () {

    $('.modal-body #full_name').val(null);
    $('.modal-body #cif').val(null);

    var a = $(this).closest(".panel");

    var office_email = document.getElementById('email_address').value;

    var parameters = {
        office_email: office_email
    };

    $.ajax({
        url: "/Employee/SearchClient",
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

           // var json = $.parseJSON(JSON.stringify(data));

            

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
                        "targets": 8,
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
                    },
                    {
                        "targets": 9,
                        "render": function (data, type, row, meta) {


                            return "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>";


                        }
                    },
                    {
                        "targets": 10,
                        "render": function (data, type, row, meta) {


                            return "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash'></i> Delete</a>";

                        }
                    }

                ],
                "aoColumns": [
                    { "data": "name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "email_address", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "CURRENCY_CODE", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "account", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "account_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "bank_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "branch_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "created_on", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "auth_status", "autoWidth": true, "sDefaultContent": "n/a" },
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

                console.log(json);

                $('.modal-body #recordid').val($(nRow).attr("recid"));
                $('.modal-body #email_address').val(json["email_address"]);
                $('.modal-body #cif').val(json["cif"]);
                $('.modal-body #full_name').val(json["account_name"]);
                $('.modal-body #bank').val(json["bank_id"]).trigger("change");
                $('.modal-body #branch').val(json["branch_id"]).trigger("change");
                $('.modal-body #currency').val(json["currency"]).trigger("change");
                $('.modal-body #account').val(json["account"]);
                selected_branch = json["branch_id"];

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

function GetEmployeeBanks() {
    $.get('GetRecords', { module: 'employee_banks' }, function (data) {
        getData(data);
    });
}

function GetCurrency() {
    $.get('GetRecords', { module: 'currencies' }, function (data) {
        $("#currency").get(0).options.length = 0;
        $("#currency").get(0).options[0] = new Option("Please Select Currency", "-1");

        $.each(data, function (index, item) {
            $("#currency").get(0).options[$("#currency").get(0).options.length] = new Option(item.CURRENCY_CODE, item.id);
        });

        $("#currency").bind("change", function () {
            var str = $("#currency option:selected").text();
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
    var currency = document.getElementById('currency').value;
    var account = document.getElementById('account').value;
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

    if (currency === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select currency",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    var parameters = {
        id: id, cif: cif, currency: currency,
        account: account, bank: bank, branch: branch
    };

    $.ajax({
        url: "/Employee/CreateEmployeeBank",
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

            GetEmployeeBanks();
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
    $('#bank').val("").trigger("change");
    $('#branch').val("").trigger("change");
    $('#currency').val("").trigger("change");
    $('#account').val("");

});