
$(document).ready(function () {
    App.init();
   
    InitiateEditableDataTable.init();

    GetOvertimeClaims();

    GetCurrency();


    $('.selectpicker').select2({
        style: 'btn-white',
        size: 5
    });

   

    $('#datetimepicker').datetimepicker({
        format: 'HH:mm', // 24-hour format
        icons: {
            time: 'fa fa-clock' // Icon for the time picker button
        }
    });

    $('#starttime').datetimepicker({
        format: 'HH:mm', // 24-hour format
        icons: {
            time: 'fa fa-clock' // Icon for the time picker button
        }
    });

    // Add click event listener to the span
    $('#timePickerTrigger').click(function () {
        $('#datetimepicker').datetimepicker('toggle');
    });

    $('#startPickerTrigger').click(function () {
        $('#starttime').datetimepicker('toggle');
    });

    //$('#start-time-input, #end-time-input').on('change.datetimepicker', function () {
    //    var startTime = $('#start-time-input').val();
    //    var endTime = $('#end-time-input').val();

    //    if (startTime && endTime) {
    //        var startMoment = moment(startTime, 'HH:mm');
    //        var endMoment = moment(endTime, 'HH:mm');
    //        var timeDifferenceHours = endMoment.diff(startMoment, 'hours', true); // Difference in hours with decimal precision
    //        $('#time-difference-hours').val(timeDifferenceHours.toFixed(2)); // Display with two decimal places
    //    }
    //});
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
                            if (row.status === '0') {
                                return '<span class="label label-warning">Pending</span>';
                            } else if (row.status === '1') {
                                return '<span class="label label-primary">Approved</span>';
                            } else if (row.status === '2') {
                                return '<span class="label label-danger">Rejected</span>';
                            } 
                        }
                    },
                    {
                        "targets": 8,
                        "render": function (data, type, row, meta) {


                            if (row.status === '0') {
                                return "<a href='#' class='btn btn-info btn-xs edit'><i class='fa fa-edit'></i> Edit</a>";
                            }


                        }
                    },

                    {
                        "targets": 9,
                        "render": function (data, type, row, meta) {


                            if (row.status === '0') {
                                return "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa fa-trash'></i> Cancel</a>";
                            }

                        }
                    }

                ],
                "aoColumns": [
                    { "data": "overtime_date", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "CURRENCY_CODE", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "starttime", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "endtime", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "hours_no", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "pay_rate", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "pay", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "status", "autoWidth": true, "sDefaultContent": "n/a" },
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
                $('.modal-body #currency').val(json["payment_currency"]).trigger("change");
                $('.modal-body #overtimedate').val(json["overtime_date"]);
                $('.modal-body #startTime').val(json["starttime"]);
                $('.modal-body #endTime').val(json["endtime"]);
                $('.modal-body #reason').val(json["description"]);
                

                $("#capture-record").appendTo("body").modal("show");
            }


        }
    };
}();


function GetOvertimeClaims() {
    $.get('GetRecords', { module: 'overtime_claim_entriesbyid' }, function (data) {
        getData(data);
    });
}

function GetClaimCategories() {
    $.get('GetRecords', { module: 'claim_types' }, function (data) {
        $("#claimtype").get(0).options.length = 0;
        $("#claimtype").get(0).options[0] = new Option("Please Select Claim Category", "-1");

        $.each(data, function (index, item) {
            $("#claimtype").get(0).options[$("#claimtype").get(0).options.length] = new Option(item.type, item.id);
        });

        $("#claimtype").bind("change", function () {
            var str = $("#claimtype option:selected").text();
        });
    });
}


function GetPaymentMode() {
    $.get('GetRecords', { module: 'payment_modes' }, function (data) {
        $("#disbursemode").get(0).options.length = 0;
        $("#disbursemode").get(0).options[0] = new Option("Please Select Mode", "-1");

        $.each(data, function (index, item) {
            $("#disbursemode").get(0).options[$("#disbursemode").get(0).options.length] = new Option(item.PAYMENT_MODE, item.id);
        });

        $("#disbursemode").bind("change", function () {
            var str = $("#disbursemode option:selected").text();
        });
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

    var startTime = document.getElementById('start_time_input').value;
    var endTime = document.getElementById('end_time_input').value;
    var timeDifferenceHrs, timeDifferenceMinutes, timeDifferenceHours;


    if (startTime && endTime) {
        var startMoment = moment(startTime, 'HH:mm');
        var endMoment = moment(endTime, 'HH:mm');

        // Calculate the difference in hours and minutes separately
        var duration = moment.duration(endMoment.diff(startMoment));
        timeDifferenceHrs = Math.floor(duration.asHours()); // Extract the whole hours
        timeDifferenceMinutes = Math.round(duration.asMinutes() % 60); // Extract the remaining minutes

        if (timeDifferenceHrs < 0) {
            timeDifferenceHrs += 24; // Add 24 hours to make it positive
        }

        timeDifferenceHours = timeDifferenceHrs + ':' + timeDifferenceMinutes;


        console.log('Start Time:', startTime);
        console.log('End Time:', endTime);
        console.log('Time Difference:', timeDifferenceHours );
    }

    var id = document.getElementById('recordid').value;
    var currency = document.getElementById('currency').value;
    var overtimedate = document.getElementById('overtimedate').value;
    var reason = document.getElementById('reason').value;

    if (currency === '-1') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select currency",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (startTime === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select start time",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

    if (endTime === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly select end time",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }




    var parameters = {
        id: id,
        startTime: startTime,
        endTime: endTime,
        timeDifferenceHours: timeDifferenceHours,
        currency: currency,
        overtimedate: overtimedate,
        reason: reason
    };
    console.log(parameters);

    $.ajax({
        url: "/Salary/CreateOvertimeClaim",
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

            GetOvertimeClaims();
        },
        error: function (xhr, textStatus, errorThrown) {
            //$.unblockUI();
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            Swal.fire({
                title: "Failed",
                text: "Mapping could not be completed " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
});

$("#capture-record").on("hidden.bs.modal", function (e) {
    $('#recordid').val("");
    $('#currency').val("");
    $('#overtimedate').val("");
    $('#start_time_input').val("");
    $('#end_time_input').val("");
    $('#reason').val("");
});