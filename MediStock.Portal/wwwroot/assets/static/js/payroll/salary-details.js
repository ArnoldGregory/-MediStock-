
$(document).ready(function () {
    App.init();

    $('#standard_hrs, #standard_pay').on('input', function () {
       
        var standardHrs = parseFloat($('#standard_hrs').val()) || 0;
        var standardPay = parseFloat($('#standard_pay').val()) || 0;

        
        var basicPay = standardHrs * standardPay;

     
        $('#basic_pay').val(basicPay.toFixed(2));
    });

    $('#overtime_hrs, #overtime_pay_rate').on('input', function () {

        var overtimeHrs = parseFloat($('#overtime_hrs').val()) || 0;
        var overtimeratePay = parseFloat($('#overtime_pay_rate').val()) || 0;

        
        var overtimepay = overtimeHrs * overtimeratePay;

        
        $('#overtime_pay').val(overtimepay.toFixed(2));
    });

    $('#holiday_hrs, #holiday_pay_rate').on('input', function () {
      
        var holidayHrs = parseFloat($('#holiday_hrs').val()) || 0;
        var holidaypayrate = parseFloat($('#holiday_pay_rate').val()) || 0;

      
        var holidaypay = holidayHrs * holidaypayrate;

        $('#holiday_pay').val(holidaypay.toFixed(2));
    });


    $('.selectpicker').select2({
        style: 'btn-white',
        size: 5
    });

    $('#paydate').datetimepicker({
        format: 'MM-DD-YYYY',
        minDate: moment() // Set minimum date to today's date and time
    });



    InitiateEditableDataTable.init();

    GetCurrency();
    GetEmployeePayDetails();


   
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
        url: "/Employee/SearchNextofKin",
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
               
                "aoColumns": [
                    { "data": "CUSTOMER_FULL_NAME", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "EMAIL_ADDRESS", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "DEPARTMENT", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "CURRENCY_CODE", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "working_hours", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "rate_per_hr", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "salary", "autoWidth": true, "sDefaultContent": "n/a" },

                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>"
                    },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa-solid fa-trash-can'></i> Delete</a>"
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
                $('.modal-body #email_address').val(json["EMAIL_ADDRESS"]);
                $('.modal-body #full_name').val(json["CUSTOMER_FULL_NAME"]);
                $('.modal-body #currency').val(json["payment_currency"]).trigger("change");
                $('.modal-body #standard_hrs').val(json["working_hours"]);
                $('.modal-body #standard_pay').val(json["rate_per_hr"]);
                $('.modal-body #basic_pay').val(json["salary"]);
               

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



function GetEmployeePayDetails() {
    $.get('GetRecords', { module: 'basic_salary_records' }, function (data) {
        console.log(data);
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

$('#save').click(function () {
    var a = $(this).closest(".panel");

    var id = document.getElementById('recordid').value;
    var email = document.getElementById('email_address').value;
    var currency = document.getElementById('currency').value;
    var standard_hrs = document.getElementById('standard_hrs').value;
    var standard_pay = document.getElementById('standard_pay').value;
    var basic_pay = document.getElementById('basic_pay').value;



    if (email === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly Enter Email",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (currency === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select Currency",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (standard_hrs === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly Enter Standard Hours",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

   


    var parameters = {
        id: id,
        email: email,
        currency: currency,
        standard_hrs: standard_hrs,
        standard_pay: standard_pay,
        basic_pay: basic_pay
       

    };



    $.ajax({
        url: "/Payroll/MaintainSalaryDeatails",
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
           
            if (data[0].error_code === '00') {

                GetEmployeePayDetails();

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
                    text: data.error_desc,
                    icon: "error",
                    confirmButtonText: "Ok"
                });
            }

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
    $('#full_name').val("")
    $('#currency').val("").trigger("change");
    $('#standard_hrs').val("");
    $('#standard_pay').val("");
    $('#basic_pay').val("");
    $('#overtime_hrs').val("");
    $('#overtime_pay_rate').val("");
    $('#overtime_pay').val("");
    $('#holiday_hrs').val("");
    $('#holiday_pay_rate').val("");
    $('#holiday_pay').val("");
    $('#other_earning_basic_pay').val("");
    $('#commision_bonus').val("");
    $('#sick_pay').val("");
    $('#expenses').val("");
    $('#pay_tax').val("");
    $('#nhif').val("");
    $('#helb').val("");
    $('#pension').val("");
    $('#union_fees').val("");
    $('#non_taxable_reimbursement').val("");
    $('#trf_bank').val("");
    $('#other_hourly_pay').val("");
    $('#vacation_pay').val("");
    $('#paydate').val("");
    $('#period').val("");

});