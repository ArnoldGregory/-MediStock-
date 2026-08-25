
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
                    { "data": "basic_pay", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-primary btn-xs view'> Details </a>"
                    },

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


            var isView = null;

            //Edit
            $('#editabledatatable').on("click", 'a.view', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                if (isView !== null && isView != nRow) {
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
                $('.modal-body #view_recordid').val($(nRow).attr("recid"));
                $('.modal-body #view_email_address').val(json["EMAIL_ADDRESS"]);
                $('.modal-body #view_full_name').val(json["CUSTOMER_FULL_NAME"]);
                $('.modal-body #view_currency').val(json["CURRENCY_CODE"]);
                $('.modal-body #view_standard_hrs').val(json["standard_hours"]);
                $('.modal-body #view_standard_pay').val(json["standard_pay_rate"]);
                $('.modal-body #view_basic_pay').val(json["basic_pay"]);
                $('.modal-body #view_overtime_hrs').val(json["overtime_hours"]);
                $('.modal-body #view_overtime_pay_rate').val(json["overtime_pay_rate"]);
                $('.modal-body #view_overtime_pay').val(json["orvertime_pay"]);
                $('.modal-body #view_holiday_hrs').val(json["holiday_hours"]);
                $('.modal-body #view_holiday_pay_rate').val(json["holiday_pay_rate"]);
                $('.modal-body #view_holiday_pay').val(json["holiday_pay"]);
                $('.modal-body #view_other_earning_basic_pay').val(json["other_earning_basic_pay"]);
                $('.modal-body #view_commision_bonus').val(json["commission_and_bonus"]);
                $('.modal-body #view_sick_pay').val(json["sick_pay"]);
                $('.modal-body #view_expenses').val(json["expenses"]);
                $('.modal-body #view_pay_tax').val(json["paye_tax"]);
                $('.modal-body #view_nhif').val(json["nhif"]);
                $('.modal-body #view_helb').val(json["helb"]);
                $('.modal-body #view_pension').val(json["pension"]);
                $('.modal-body #view_union_fees').val(json["union_fees"]);
                $('.modal-body #view_non_taxable_reimbursement').val(json["non_taxable_reimbursements"]);
                $('.modal-body #view_trf_bank').val(json["trf_bank"]);
                $('.modal-body #view_other_hourly_pay').val(json["end_date"]);
                $('.modal-body #view_vacation_pay').val(json["end_date"]);
                $('.modal-body #view_paydate').val(json["paydate"]);
                $('.modal-body #view_period').val(json["period"]);

                $("#capture-details-record").appendTo("body").modal("show");
            }

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
                $('.modal-body #currency').val(json["currency"]).trigger("change");
                $('.modal-body #standard_hrs').val(json["standard_hours"]);
                $('.modal-body #standard_pay').val(json["standard_pay_rate"]);
                $('.modal-body #basic_pay').val(json["basic_pay"]);
                $('.modal-body #overtime_hrs').val(json["overtime_hours"]);
                $('.modal-body #overtime_pay_rate').val(json["overtime_pay_rate"]);
                $('.modal-body #overtime_pay').val(json["orvertime_pay"]);
                $('.modal-body #holiday_hrs').val(json["holiday_hours"]);
                $('.modal-body #holiday_pay_rate').val(json["holiday_pay_rate"]);
                $('.modal-body #holiday_pay').val(json["holiday_pay"]);
                $('.modal-body #other_earning_basic_pay').val(json["other_earning_basic_pay"]);
                $('.modal-body #commision_bonus').val(json["commission_and_bonus"]);
                $('.modal-body #sick_pay').val(json["sick_pay"]);
                $('.modal-body #expenses').val(json["expenses"]);
                $('.modal-body #pay_tax').val(json["paye_tax"]);
                $('.modal-body #nhif').val(json["nhif"]);
                $('.modal-body #helb').val(json["helb"]);
                $('.modal-body #pension').val(json["pension"]);
                $('.modal-body #union_fees').val(json["union_fees"]);
                $('.modal-body #non_taxable_reimbursement').val(json["non_taxable_reimbursements"]);
                $('.modal-body #trf_bank').val(json["trf_bank"]);
                $('.modal-body #other_hourly_pay').val(json["end_date"]);
                $('.modal-body #vacation_pay').val(json["end_date"]);
                $('.modal-body #paydate').val(json["paydate"]);
                $('.modal-body #period').val(json["period"]);

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
    $.get('GetRecords', { module: 'employee_pay_details' }, function (data) {
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

    var overtime_hrs = document.getElementById('overtime_hrs').value;
    var overtime_pay_rate = document.getElementById('overtime_pay_rate').value;
    var overtime_pay = document.getElementById('overtime_pay').value;

    var holiday_hrs = document.getElementById('holiday_hrs').value;
    var holiday_pay_rate = document.getElementById('holiday_pay_rate').value;
    var holiday_pay = document.getElementById('holiday_pay').value;

    var other_earning_basic_pay = document.getElementById('other_earning_basic_pay').value;
    var commision_bonus = document.getElementById('commision_bonus').value;
    var sick_pay = document.getElementById('sick_pay').value;

    var expenses = document.getElementById('expenses').value;
    var pay_tax = document.getElementById('pay_tax').value;
    var nhif = document.getElementById('nhif').value;

    var helb = document.getElementById('helb').value;
    var pension = document.getElementById('pension').value;
    var union_fees = document.getElementById('union_fees').value;

    var non_taxable_reimbursement = document.getElementById('non_taxable_reimbursement').value;
    var trf_bank = document.getElementById('trf_bank').value;
    var other_hourly_pay = document.getElementById('other_hourly_pay').value;

    var vacation_pay = document.getElementById('other_hourly_pay').value;
    var paydate = document.getElementById('paydate').value;
    var period = document.getElementById('period').value;



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
        basic_pay: basic_pay,
        overtime_hrs: overtime_hrs,
        overtime_pay_rate: overtime_pay_rate,
        overtime_pay: overtime_pay,
        holiday_hrs: holiday_hrs,
        holiday_pay_rate: holiday_pay_rate,
        holiday_pay: holiday_pay,
        other_earning_basic_pay: other_earning_basic_pay,
        commision_bonus: commision_bonus,
        sick_pay: sick_pay,
        expenses: expenses,
        pay_tax: pay_tax,
        nhif: nhif,
        helb: helb,
        pension: pension,
        union_fees: union_fees,
        non_taxable_reimbursement: non_taxable_reimbursement,
        trf_bank: trf_bank,
        other_hourly_pay: other_hourly_pay,
        vacation_pay: vacation_pay,
        paydate: paydate,
        period: period

    };



    $.ajax({
        url: "/Payroll/MaintainPayroll",
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