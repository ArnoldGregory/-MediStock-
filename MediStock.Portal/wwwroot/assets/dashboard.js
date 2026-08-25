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
            v: json_item["prepaid"],
            w: json_item["postpaid"],
            x: json_item["airtime"]
        };

        line_chart_data.push(obj);
    }

    Morris.Line({
        element: "statistics-line-chart",
        data: line_chart_data,
        xkey: "u",
        ykeys: ["v", "w", "x"],
        labels: ["Prepaid", "Postpaid", "Airtime"],
        lineColors: [a, b, c],
        pointFillColors: [a, b, c],
        lineWidth: "2px",
        pointStrokeColors: [f, f, f],
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
}, handleVisitorsVectorMap = function (data) {

    var json_items = JSON.parse(data.map_data);

    var html_line = '';

    var obj = {};

    for (var i = 0; i < json_items.length; i++) {

        var json_item = json_items[i];

        var serial = i + 1;

        html_line = html_line + '<a href="#" class="list-group-item list-group-item-inverse text-ellipsis">' +
            '<span class="badge badge-success">' + json_item["percentile"] + ' %</span>' +
            serial.toLocaleString() + '. ' + json_item["country"] +
            '</a>';

        var country = json_item["country"].substring(
            json_item["country"].lastIndexOf("[") + 1,
            json_item["country"].lastIndexOf("]")
        );

        obj[country] = json_item["color"];
    }

    document.getElementById('visitors_percentages').innerHTML = html_line;

    0 !== $("#visitors-map").length && $("#visitors-map").vectorMap({
        map: "world_merc_en",
        scaleColors: ["#e74c3c", "#0071a4"],
        container: $("#visitors-map"),
        normalizeFunction: "linear",
        hoverOpacity: .5,
        hoverColor: !1,
        markerStyle: {
            initial: {
                fill: "#4cabc7",
                stroke: "transparent",
                r: 3
            }
        },
        regions: [{
            attribute: "fill"
        }],
        regionStyle: {
            initial: {
                fill: "rgb(97,109,125)",
                "fill-opacity": 1,
                stroke: "none",
                "stroke-width": .4,
                "stroke-opacity": 1
            },
            hover: {
                "fill-opacity": .8
            },
            selected: {
                fill: "yellow"
            },
            selectedHover: {}
        },
        series: {
            regions: [{
                values: obj
            }]
        },
        focusOn: {
            x: .5,
            y: .5,
            scale: 2
        },
        backgroundColor: "#2d353c"
    });
}, handleStackedChart = function (data) {
    "use strict";

    function p(a, b, c) {
        $('<div id="tooltip" class="flot-tooltip">' + c + "</div>").css({
            top: b,
            left: a + 35
        }).appendTo("body").fadeIn(200);
    }

    var m = [];

    var a = [];
    var c = [];
    var e = [];

    var json_items = JSON.parse(data.stacked_chart_data);

    for (var i = 0; i < json_items.length; i++) {
        var json_item = json_items[i];
        var day_arr = [
            i, json_item["day_period"]
        ];
        m.push(day_arr);

        a.push([i, parseInt(json_item["prepaid"])]);
        c.push([i, parseInt(json_item["postpaid"])]);
        e.push([i, parseInt(json_item["airtime"])]);
    }

    var n = {
        xaxis: {
            tickColor: "transparent",
            ticks: m
        },
        yaxis: {
            tickColor: "#ddd",
            ticksLength: 10
        },
        grid: {
            hoverable: !0,
            tickColor: "#ccc",
            borderWidth: 0,
            borderColor: "rgba(0,0,0,0.2)"
        },
        series: {
            stack: !0,
            lines: {
                show: !1,
                fill: !1,
                steps: !1
            },
            bars: {
                show: !0,
                barWidth: .5,
                align: "center",
                fillColor: null
            },
            highlightColor: "rgba(0,0,0,0.8)"
        },
        legend: {
            show: !0,
            labelBoxBorderColor: "#ccc",
            position: "ne",
            noColumns: 1
        }
    },
        o = [{
            data: a,
            color: grey,
            label: "Prepaid",
            bars: {
                fillColor: grey
            }
        }, {
            data: c,
            color: red,
            label: "Postpaid",
            bars: {
                fillColor: red
            }
        }, {
            data: e,
            color: orange,
            label: "Airtime",
            bars: {
                fillColor: orange
            }
        }];
    $.plot("#stacked-chart", o, n);
    var q = null,
        r = null;
    $("#stacked-chart").bind("plothover", function (a, b, c) {
        if (c) {
            var d = c.datapoint[1] - c.datapoint[2];
            q === c.series.label && d === r || (q = c.series.label, r = d, $("#tooltip").remove(), p(c.pageX, c.pageY, d + " " + c.series.label));
        } else $("#tooltip").remove(), q = null, r = null;
    });
}, handleUtilityBalances = function (data) {
    var json_items = JSON.parse(data.utility_balances);

    var html_line = '';

    for (var i = 0; i < json_items.length; i++) {

        var json_item = json_items[i];

        var serial = i + 1;

        html_line = html_line + '<a href="#" class="list-group-item list-group-item-inverse text-ellipsis">' +
            '<span class="badge badge-success">' + json_item["balance"] + '</span>' +
            serial.toLocaleString() + '. ' + json_item["utility"] +
            '</a>';
    }

    document.getElementById('utility_balances').innerHTML = html_line;

}, handleDashboadData = function () {

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

            //handleStatisticsDonutChart(data);

            //handleStatisticsLineChart(data);

            //handleVisitorsVectorMap(data);

            //handleStackedChart(data);

            //handleUtilityBalances(data);
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