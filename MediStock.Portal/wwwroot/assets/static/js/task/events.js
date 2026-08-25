$(document).ready(function () {
    App.init();

});

const datePicker = new DayPilot.Navigator("nav", {
    showMonths: 3,
    skipMonths: 3,
    selectMode: "Month",
    onTimeRangeSelected: args => {
        calendar.startDate = args.day;
        calendar.update();
        calendar.events.load("/api/schedule");
    }
});
datePicker.init();

const calendar = new DayPilot.Month("dp", {
    eventHeight: 30
    ,
    //onTimeRangeSelected: async args => {
    //    const modal = await DayPilot.Modal.prompt("Create a new event:", "Event");

    //    calendar.clearSelection();
    //    if (modal.canceled) {
    //        return;
    //    }
    //    const params = {
    //        start: args.start,
    //        end: args.end,
    //        text: modal.result,
    //        resource: args.resource
    //    };
    //    const { data: result } = await DayPilot.Http.post("/api/schedule", params);
    //    calendar.events.add(result);
    //}
    //,
    //onEventMove: async args => {
    //    const params = {
    //        id: args.e.id(),
    //        start: args.newStart,
    //        end: args.newEnd
    //    };
    //    const id = args.e.id();
    //    await DayPilot.Http.put(`/api/schedule/${id}/move`, params);
    //},
    //onEventResize: async args => {
    //    const params = {
    //        id: args.e.id(),
    //        start: args.newStart,
    //        end: args.newEnd
    //    };
    //    const id = args.e.id();
    //    await DayPilot.Http.put(`/api/schedule/${id}/move`, params);
    //},
    onBeforeEventRender: args => {

        const color = args.data.color;

        if (color) {
            args.data.backColor = DayPilot.ColorUtil.lighter(color);
            args.data.borderColor = "darker";
            args.data.barColor = color;
        }

        args.data.areas = [
            {
                top: 5,
                right: 8,
                width: 18,
                height: 18,
                symbol: "/assets/icons/daypilot.svg#minichevron-down-4",
                fontColor: "#666",
                visibility: "Hover",
                action: "ContextMenu",
                style: "background-color: #f9f9f9; border: 1px solid #666; cursor:pointer; border-radius: 15px;"
            }
        ];
    }
    
    //,
    //contextMenu: new DayPilot.Menu({
    //    items: [
    //        {
    //            text: "Delete",
    //            onClick: async args => {
    //                const e = args.source;
    //                const id = e.id();
    //                await DayPilot.Http.delete(`/api/schedule/${id}`);
    //                calendar.events.remove(e);
    //            }
    //        },
    //        {
    //            text: "-"
    //        },
    //        {
    //            text: "Blue",
    //            icon: "icon icon-blue",
    //            color: "#3c78d8",
    //            onClick: args => { app.updateColor(args.source, args.item.color); }
    //        },
    //        {
    //            text: "Green",
    //            icon: "icon icon-green",
    //            color: "#6aa84f",
    //            onClick: args => { app.updateColor(args.source, args.item.color); }
    //        },
    //        {
    //            text: "Yellow",
    //            icon: "icon icon-yellow",
    //            color: "#f1c232",
    //            onClick: args => { app.updateColor(args.source, args.item.color); }
    //        },
    //        {
    //            text: "Red",
    //            icon: "icon icon-red",
    //            color: "#cc4125",
    //            onClick: args => { app.updateColor(args.source, args.item.color); }
    //        },
    //        {
    //            text: "Auto",
    //            color: "",
    //            onClick: args => { app.updateColor(args.source, args.item.color); }
    //        },
    //    ]
    //})
});
calendar.init();



const app = {
    elements: {
        previous: document.querySelector("#previous"),
        today: document.querySelector("#today"),
        next: document.querySelector("#next"),
    },
    async updateColor(e, color) {
        const params = {
            color: color
        };
        const id = e.id();
        await DayPilot.Http.put(`/api/schedule/${id}/color`, params);
        e.data.color = color;
        calendar.events.update(e);
    },
    init() {
        app.elements.previous.addEventListener("click", () => {
            datePicker.select(datePicker.selectionDay.addMonths(-1));
        });

        app.elements.today.addEventListener("click", () => {
            datePicker.select(DayPilot.Date.today());
        });

        app.elements.next.addEventListener("click", () => {
            datePicker.select(datePicker.selectionDay.addMonths(1));
        });

        calendar.events.load("/api/schedule");

    }
};

app.init();