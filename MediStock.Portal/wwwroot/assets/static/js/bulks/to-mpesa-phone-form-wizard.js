function GetSourceAccounts() {
    $.get('GetDropDownData', { module: 'accounts' }, function (data) {
        $("#source_account").get(0).options.length = 0;
        $("#source_account").get(0).options[0] = new Option("Please Select Source Account", "-1");

        $.each(data, function (index, item) {
            $("#source_account").get(0).options[$("#source_account").get(0).options.length] = new Option(item.itemtext, item.itemid);
        });

        $("#source_account").bind("change", function () {
            var str = $("#source_account option:selected").text();
            var acc = str.substring(str.lastIndexOf("[") + 1, str.lastIndexOf("]"));
            //console.log(acc.trim());
            //console.log($(this).val());
        });
    });
}

$('#search').click(function () {
    document.getElementById("recipient_name").value = "JOHN DOE";
    //var searchvalue = document.getElementById('dest_phone').value;

    //var parameters = { searchvalue: searchvalue };
    //$.get('GetDropDownData', parameters, function (data) {
    //  document.getElementById("recipient_name").value = data[0].recipient_name;
    //});
});

var handleMPesaToPhoneWizards = function () {
    "use strict";
    $("#wizard").bwizard({
        validating: function (e, ui) {
            if (ui.index == 0) {
                // step-1 confirmation
                if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-1")) {
                    return false
                } else {
                    var source_str = $("#source_account option:selected").text();
                    var source_acc = source_str.substring(source_str.lastIndexOf("[") + 1, source_str.lastIndexOf("]"));
                    document.getElementById("confirm_source_account").value = source_acc.trim();

                    document.getElementById("confirm_dest_phone").value = $("#dest_phone").val().trim();

                    document.getElementById("confirm_recipient_name").value = $("#recipient_name").val().trim();

                    document.getElementById("confirm_amount").value = $("#amount").val();

                    document.getElementById("confirm_reference").value = $("#reference").val();

                    document.getElementById("confirm_comments").value = $("#comments").val();
                }
            } else if ((ui.index === 1) && (ui.nextIndex > ui.index)) {
                // step-2 summary
                swal({
                    title: "Are you sure?",
                    text: "you want to commit this transaction?",
                    type: "info",
                    showCancelButton: !0,
                    confirmButtonClass: "btn-info",
                    confirmButtonText: "Proceed",
                    closeOnConfirm: false,
                    closeOnCancel: false
                },
                    function (isConfirm) {
                        if (isConfirm) {
                            var source_account = document.getElementById('confirm_source_account').value;
                            var dest_phone = document.getElementById('confirm_dest_phone').value;
                            var recipient_name = document.getElementById('confirm_recipient_name').value;
                            var amount = document.getElementById('confirm_amount').value;
                            var reference = document.getElementById('confirm_reference').value;
                            var comments = document.getElementById('confirm_comments').value;

                            var parameters = {
                                source_account: source_account,
                                dest_wallet: dest_phone,
                                recipient_name: recipient_name,
                                amount: amount,
                                reference: reference,
                                comments: comments
                            };

                            $.get('MPesaToPhone', parameters, function (data) {
                                document.getElementById("summary_source_account").innerHTML = source_account.trim();
                                document.getElementById("summary_recipient_phone").innerHTML = dest_phone.trim();
                                document.getElementById("summary_recipient_name").innerHTML = recipient_name.trim();
                                document.getElementById("summary_amount").innerHTML = amount;
                                document.getElementById("summary_my_reference").innerHTML = reference;
                                document.getElementById("summary_comments").innerHTML = comments;
                                document.getElementById("summary_system_reference").innerHTML = data[0].system_ref;

                                if (data[0].code === '00') {
                                    document.getElementById("summary_status").innerHTML = "Success";
                                    document.getElementById("summary_status").classList = "label label-success";
                                    swal({
                                        title: "Success",
                                        text: data[0].desc,
                                        type: "success",
                                        confirmButtonClass: "btn-info",
                                        confirmButtonText: "Ok"
                                    });
                                } else {
                                    document.getElementById("summary_status").innerHTML = "Failed";
                                    document.getElementById("summary_status").classList = "label label-danger";
                                    swal({
                                        title: "Failed",
                                        text: data[0].desc,
                                        type: "error",
                                        confirmButtonClass: "btn-info",
                                        confirmButtonText: "Ok"
                                    });
                                }
                            });
                        } else {
                            swal({
                                title: "Cancelled",
                                text: "Transaction has been cancelled",
                                type: "success",
                                confirmButtonClass: "btn-info",
                                confirmButtonText: "Ok"
                            });
                        }
                    });
            }
        }
    });
};

var MPesaToPhoneFormWizard = function () {
    "use strict";
    return {
        init: function () {
            handleMPesaToPhoneWizards();
        }
    };
}();