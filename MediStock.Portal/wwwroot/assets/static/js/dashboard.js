var blue = "#348fe2",
    blueLight = "#5da5e8",
    blueDark = "#1993E4",
    aqua = "#49b6d6",
    aquaLight = "#6dc5de",
    aquaDark = "#3a92ab",
    green = "#1B84C7",
    greenLight = "#33bdbd",
    greenDark = "#57afe8",
    orange = "#f59c1a",
    orangeLight = "#f7b048",
    orangeDark = "#c47d15",
    dark = "#2d353c",
    grey = "#b6c2c9",
    purple = "#727cb6",
    purpleLight = "#8e96c5",
    purpleDark = "#5b6392",
    red = "#ff5b57";

var getMonthName = function (a) {
    var b = [];
    return b[0] = "January",
        b[1] = "February",
        b[2] = "March",
        b[3] = "April",
        b[4] = "May",
        b[5] = "Jun",
        b[6] = "July",
        b[7] = "August",
        b[8] = "September",
        b[9] = "October",
        b[10] = "November",
        b[11] = "December",
        b[a];
}, getDate = function (a) {
    var b = new Date(a),
        c = b.getDate(),
        d = b.getMonth() + 1,
        e = b.getFullYear();
    return c < 10 && (c = "0" + c), d < 10 && (d = "0" + d), b = e + "-" + d + "-" + c;
}, handleStatisticsLineChart = function (data) {

    var a = "#B6C2C9", //Default
        b = "#FF5B57", //Danger
        c = "#F59C1A", //Warning
        d = "#1B84C7", //Success
        e = "#348FE2"; //Primary
    f = "#2D353C"; //Inverse
    g = "rgba(255,255,255,0.4)";

    var line_chart_data = [];

    var json_items = JSON.parse(data.line_chart_data);

    for (var i = 0; i < json_items.length; i++) {

        var json_item = json_items[i];

        var obj =
        {
            u: json_item["month_period"],
            v: json_item["clients"],
            w: json_item["employees"],
            //x: json_item["claims"]
        };

        line_chart_data.push(obj);
    }

    Morris.Line({
        element: "statistics-line-chart",
        data: line_chart_data,
        xkey: "u",
        ykeys: ["v", "w"],
        labels: ["Clients", "Employes"],
        lineColors: [a, b],
        pointFillColors: [a, b],
        lineWidth: "2px",
        pointStrokeColors: [f, f],
        resize: !0,
        gridTextFamily: "Open Sans",
        gridTextColor: a,
        gridTextWeight: "normal",
        gridTextSize: "11px",
        gridLineColor: "rgba(0,0,0,0.5)",
        hideHover: "auto"
    });
}, handleStatisticsDonutChart = function (data) {

    var a = "#B6C2C9", //Default
        b = "#FF5B57", //Danger
        c = "#F59C1A", //Warning
        d = "#1B84C7", //Success
        e = "#348FE2"; //Primary

    var pie_chart_data = [];

    var json_items = JSON.parse(data.doughnut_data);

    var total_transfers = 0;

    for (var i = 0; i < json_items.length; i++) {

        var json_item = json_items[i];

        total_transfers = total_transfers + Number(json_item["total"]);

        var li = '<li>' +
            '	<i class="fa fa-circle-o fa-fw text-' + json_item["color"] + ' m-r-5"></i> ' + json_item["percentile"] + ' % <span>' + json_item["category"] + '</span>' +
            '</li>';

        $("#percentiles ul").append(li);

        var obj =
        {
            label: json_item["category"],
            value: json_item["total"]
        };

        pie_chart_data.push(obj);
    }

    document.getElementById('total_transfers').innerHTML = total_transfers.toLocaleString();

    Morris.Donut({
        element: "statistics-donut-chart",
        data: pie_chart_data,
        colors: [a, b, c, d, e],
        labelFamily: "Open Sans",
        labelColor: "rgba(255,255,255,0.4)",
        labelTextSize: "12px",
        backgroundColor: "#242a30"
    });
}, handleDashboadData = function () {

    var a = $(this).closest(".panel");

    $.ajax({
        url: "GetIndexDashboardData/Dashboard",
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

            handleStatisticsDonutChart(data);

            handleStatisticsLineChart(data);
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
}