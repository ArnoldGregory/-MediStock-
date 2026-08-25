

function GetRecipients(audience) {

    var a = document.getElementById('main_panel');

    var parameters = {
        module: 'phone_numbers',
        param: audience
    };

    $.ajax({
        url: "/Broadcast/GetRecords",
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
            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

            var phone_numbers = '';
            var json = $.parseJSON(JSON.stringify(data));
            //var json = JSON.parse(jsonstring);
            for (var i = 0; i < json.length; i++) {
                var item = json[i];
                if (i === (json.length - 1))
                    phone_numbers = phone_numbers + item['phone_number']
                else
                    phone_numbers = phone_numbers + item['phone_number'] + '; '
            }

            document.getElementById('recipients').value = phone_numbers;
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

$('#send').click(function () {

    var a = $(this).closest(".panel");

    var recipients = document.getElementById('recipients').value;
    var message = document.getElementById('message').value;

    var parameters = {
        recipients: recipients,
        message: message
    };

    $.ajax({
        url: "/Broadcast/ScheduleMessage",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(parameters),
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

            if (data.includes('success')) {
                Swal.fire({
                    title: "Success",
                    text: data,
                    icon: "success",
                    confirmButtonText: "Ok"
                });

                document.getElementById('recipients').value = '';
                document.getElementById('message').value = '';

            } else {
                Swal.fire({
                    title: "Failed",
                    text: data,
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
                text: "Operation could not be completed " + errorThrown,
                icon: "error",
                confirmButtonText: "Ok"
            });
        }
    });
});

