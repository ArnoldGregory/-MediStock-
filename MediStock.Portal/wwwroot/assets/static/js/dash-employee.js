
$(document).ready(function () {
    App.init();

    DashboardV2.init();


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