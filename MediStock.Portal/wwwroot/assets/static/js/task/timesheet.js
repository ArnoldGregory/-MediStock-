$(document).ready(function () {
    App.init();

});


function getMonday(d) {
    d = new Date(d);
    var day = d.getDay(),
        diff = d.getDate() - day + (day == 0 ? -6 : 1); // adjust when day is sunday
    return new Date(d.setDate(diff));
}

const timesheet = new DayPilot.Scheduler("dp",
    {

        viewType: "Days",
        startDate: getMonday(new Date()),
        days: 30,
        scale: "CellDuration",
        cellDuration: 15,
        timeHeaders: [
            { groupBy: "Hour", format: "h tt" },
            { groupBy: "Cell", format: "mm" }
        ],
        rowHeaderColumns: [
            { name: "Date" },
            { name: "Day" },
            { name: "Total", width: 40 },
        ],
        resources: [
            { name: "Histogram", id: "histogram", frozen: "top", cellsAutoUpdated: true, cellsDisabled: true }
        ],
        onBeforeRowHeaderRender: args => {

            if (args.row.data.frozen) {
                args.row.columns = null;
                return;
            }

            args.row.columns[1].text = args.row.start.toString("dddd");
            args.row.columns[2].text = args.row.events.totalDuration().toString("h:mm");


            const max = DayPilot.Duration.ofHours(8);
            const pct = args.row.events.totalDuration().totalSeconds() / max.totalSeconds();
            args.row.columns[2].areas = [
                {
                    bottom: 0,
                    left: 0,
                    width: 40,
                    height: 4,
                    backColor: "#ffe599",
                },
                {
                    bottom: 0,
                    left: 0,
                    width: 40 * pct,
                    height: 4,
                    backColor: "#f1c232",
                }
            ];
        },
        onTimeRangeSelected: async args => {
            const modal = await DayPilot.Modal.prompt("Create a new task:", "Task");

            const dp = args.control;
            dp.clearSelection();
            if (modal.canceled) {
                return;
            }
            const params = {
                start: args.start,
                end: args.end,
                text: modal.result,
                resource: args.resource
            };
            const { data: result } = await DayPilot.Http.post("/api/TimesheetEvents", params);
            dp.events.add(result);
            dp.message("Event created");
            timesheet.events.load("/api/TimesheetEvents");

        },
        onEventMoved: async (args) => {
            const event = {
                ...args.e.data,
                start: args.newStart,
                end: args.newEnd
            };
            const { data } = await DayPilot.Http.put(`/api/TimesheetEvents/${event.id}`, event);
        },
        onEventResized: async (args) => {
            const event = {
                ...args.e.data,
                start: args.newStart,
                end: args.newEnd
            };
            const { data } = await DayPilot.Http.put(`/api/TimesheetEvents/${event.id}`, event);
        },
        onBeforeEventRender: args => {
            args.data.barColor = "green";

            args.data.areas = [
                {
                    top: 0,
                    right: 3,
                    width: 40,
                    bottom: 0,
                    text: new DayPilot.Event(args.data).duration().toString("h:mm"),
                    fontColor: "#999",
                    style: "display: flex; align-items: center; justify-content: end; padding: 2px;"
                }
            ];
        },
        onEventClicked: async (args) => {

            const form = [
                { name: "Text", id: "text" }
            ];
            const modal = await DayPilot.Modal.form(form, args.e.data);
            if (modal.canceled) {
                return;
            }

            const params = {
                ...args.e.data,
                text: modal.result.text,
            };

            console.log("params", params);

            await DayPilot.Http.put(`/api/TimesheetEvents/${params.id}`, params);
            timesheet.events.update(modal.result);
        },
        onBeforeCellRender: args => {

            if (args.cell.resource === "histogram") {
                const max = timesheet.rows.all().filter(r => r.data.frozen !== "top" && r.children().length === 0).length;

                const start = args.cell.start.getTimePart();
                const end = args.cell.end.getTimePart();
                const inUse = timesheet.events.all().filter(e => {
                    return DayPilot.Util.overlaps(start, end, e.start().getTimePart(), e.end().getTimePart());
                }).length;

                //                    const inUse = dp.events.forRange(args.cell.start, args.cell.end).length;
                const percentage = inUse / max;

                args.cell.properties.backColor = "#ffffff";
                if (inUse > 0) {

                    const cellHeight = timesheet.eventHeight - 1;
                    const barHeight = Math.min(percentage, 1) * cellHeight;

                    args.cell.properties.areas = [
                        {
                            bottom: 1,
                            height: barHeight,
                            left: 3,
                            right: 3,
                            backColor: "#dd7e6b",
                            style: "box-sizing: border-box; border: 1px solid #cc4125;",
                        }
                    ];
                }
            }

        }
    });
timesheet.init();


const app = {
    changeMonth(date) {
        timesheet.update({
            startDate: date.firstDayOfMonth(),
            days: date.daysInMonth()
        });
    },
    init() {

        var today = new Date();
        var dd = String(today.getDate()).padStart(2, '0');
        var mm = String(today.getMonth() + 1).padStart(2, '0');
        var yyyy = today.getFullYear();

        today = yyyy + '-' + mm + '-' + dd + 'T09:00:00';
        timesheet.scrollTo(today);
        timesheet.events.load("/api/TimesheetEvents");
        /*timesheet.events.load(`/api/TimesheetEvents?id=${id}`);*/
    }
};

app.init();