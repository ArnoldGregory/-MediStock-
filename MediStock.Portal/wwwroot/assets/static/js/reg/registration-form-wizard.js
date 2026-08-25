var tooltip_content = '<p style="color: #fff; max-width: 400px;"> ' +
	'To keep you informed about our products and any other product of Madison Group and its other subsidiaries, your contact details ' +
	'have been stored at the Madison Group database. In order to comply with the Data Protection Act, 2019, we require your consent, to ' +
	'enable us to serve you better as we collect your data and process the same in accordance with data protection laws.We value the ' +
	'confidentiality and integrity of your data. By procuring fund management services, insurance services and any other services, you will ' +
	'give us consent to handle and process your data. ' +
	'</p >';

//$('[data-toggle="tooltip"]').tooltip({
//	html: true,
//	title: tooltip_content
//});

tooltip_content = 'To keep you informed about our products and any other product of Madison Group and its other subsidiaries, your contact details ' +
	'have been stored at the Madison Group database. In order to comply with the Data Protection Act, 2019, we require your consent, to ' +
	'enable us to serve you better as we collect your data and process the same in accordance with data protection laws.We value the ' +
	'confidentiality and integrity of your data. By procuring fund management services, insurance services and any other services, you will ' +
	'give us consent to handle and process your data.';

//tippy('.tooltip-wide', {
//	content: tooltip_content,
//	theme: 'blue'
//});

var accounts = [];

var id_number;
var mobile_phone;
var full_name;
var serial_number;
var pin_number;
var office_phone;
var home_phone;
var mobile_phone;
var office_email;
var other_email;
var physical_address;
var postal_address;

var handleRegistrationWizards = function () {
	"use strict";
	$("#wizard").bwizard({
		validating: function (e, ui) {
			if (ui.index === 0) {
				// step-1 confirmation
				if (full_name === '') {
					Swal.fire({
						title: "Success",
						text: "Enter ID number and mobile number then click on the search button",
						icon: "success",
						confirmButtonText: "Ok"
					});
				} else {
					if (false === $('form[name="form-wizard"]').parsley().validate("wizard-step-1")) {
						return false;
					} else {
						var a = $(this).closest(".panel");

						var consent = document.getElementById('consent').checked;

						if (!consent) {
							Swal.fire({
								title: "Information",
								text: "You must give consent allowing us to store your information in order to proceed",
								icon: "warning",
								confirmButtonText: "Ok"
							});
							return false;
						}

						var parameters = {
							id_number: document.getElementById('id_number').value,
							mobile_phone: document.getElementById('mobile_phone').value
						};

						$.ajax({
							url: "/ClientSetup/SearchClient",
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
								$(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

								var json = $.parseJSON(JSON.stringify(data));

								if (json["error_code"] == '00') {
									if (json["error_desc"]["success"] === true) {
										Swal.fire({
											title: "Success",
											text: "Client available for registration, click 'Next' to proceed",
											icon: "success",
											confirmButtonText: "Ok"
										});

										full_name = json["error_desc"]["mimClntEntity"]["ccFullName"]; //json["error_desc"]["mimClntList"][0]["fullName"];
										id_number = json["error_desc"]["mimClntEntity"]["ccIDNumber"]; //json["error_desc"]["mimClntList"][0]["idNumber"];
										serial_number = json["error_desc"]["mimClntEntity"]["ccIDSerialNumber"]; //json["error_desc"]["mimClntList"][0]["idSerialNumber"];
										pin_number = json["error_desc"]["mimClntEntity"]["ccPinNumber"]; //json["error_desc"]["mimClntList"][0]["pinNumber"];
										office_phone = json["error_desc"]["mimClntEntity"]["ccOfficePhone"]; //json["error_desc"]["mimClntList"][0]["officePhone"];
										home_phone = json["error_desc"]["mimClntEntity"]["ccHomePhone"]; //json["error_desc"]["mimClntList"][0]["homePhone"];
										mobile_phone = json["error_desc"]["mimClntEntity"]["ccMobilePhone"]; //json["error_desc"]["mimClntList"][0]["mobilePhone"];
										office_email = json["error_desc"]["mimClntEntity"]["ccOfficeEmail"]; //json["error_desc"]["mimClntList"][0]["officeEmail"];
										other_email = json["error_desc"]["mimClntEntity"]["ccOtherEmail"]; //json["error_desc"]["mimClntList"][0]["otherEmail"];
										physical_address = json["error_desc"]["mimAccntList"][0]["physicalAddress"]; //json["error_desc"]["mimClntList"][0]["physicalAddress"];
										postal_address = json["error_desc"]["mimAccntList"][0]["postalAddress"]; //json["error_desc"]["mimClntList"][0]["postalAddress"];

										var json_items = json["error_desc"]["mimAccntList"];

										for (var i = 0; i < json_items.length; i++) {
											var json_item = json_items[i];

											var obj =
											{
												client_code: json_item["clientCode"],
												customer_number: json_item["customerNumber"],
												account_name: json_item["accountName"], //json_item["fullName"],
												fund_share_class_code: json_item["fundShareClassCode"],
												fund_share_class_name: json_item["fundShareClassName"]
											};

											accounts.push(obj);
										}

										/********************/
										Swal.fire({
											title: "Are you sure?",
											text: "Do you wish to proceed with your application?",
											icon: "question",
											showCancelButton: true,
											confirmButtonText: "Proceed",
											reverseButtons: true
										}).then((result) => {
											if (result.isConfirmed) {
												var client_type = 'IND'; //document.getElementById('clienttype').value;
												var locked = false;

												var parameters = {
													full_name: full_name, id_number: id_number,
													serial_number: serial_number, pin_number: pin_number,
													office_phone: office_phone, home_phone: home_phone,
													mobile_phone: mobile_phone, office_email: office_email,
													other_email: other_email, physical_address: physical_address,
													postal_address: postal_address, client_type: client_type,
													locked: Number(locked), accounts: accounts
												};

												$.ajax({
													url: "/ClientSetup/RegisterClient",
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
														document.getElementById("summary_id_number").innerHTML = id_number;
														document.getElementById("summary_mobile_number").innerHTML = mobile_phone;
														document.getElementById("summary_system_reference").innerHTML = data.system_ref;

														var buttons = document.getElementsByClassName("previous");
														for (var i = 0; i < buttons.length; i++) {
															buttons[i].setAttribute("aria-disabled", "true");
															buttons[i].setAttribute("class", "previous disabled");
														}
														console.log(data);
														if (data.error_code === '00') {
															document.getElementById("summary_status").innerHTML = "Success";
															document.getElementById("summary_status").classList = "label label-success";
															Swal.fire({
																title: "Success",
																text: data.error_desc,
																icon: "success",
																confirmButtonText: "Ok"
															});
														} else {
															document.getElementById("summary_status").innerHTML = "Failed";
															document.getElementById("summary_status").classList = "label label-danger";
															Swal.fire({
																title: "Failed",
																text: data.error_desc,
																icon: "error",
																confirmButtonText: "Ok"
															});
														}
														$(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
													}
												});
											} else {
												document.getElementById("summary_id_number").innerHTML = "-";
												document.getElementById("summary_mobile_number").innerHTML = "-";
												document.getElementById("summary_system_reference").innerHTML = "-";
												document.getElementById("summary_status").innerHTML = "Cancelled";
												document.getElementById("summary_status").classList = "label label-info";

												Swal.fire({
													title: "Cancelled",
													text: "Registration has been cancelled",
													icon: "info",
													confirmButtonText: "Ok"
												});
											}
										});
										/********************/
									} else {
										Swal.fire({
											title: "Failed",
											text: json["error_desc"]["message"],
											icon: "error",
											confirmButtonText: "Ok"
										});
									}
								} else {
									Swal.fire({
										title: "Failed",
										text: json["error_desc"],
										icon: "error",
										confirmButtonText: "Ok"
									});
								}
							},
							error: function (xhr, textStatus, errorThrown) {
								$(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();

								Swal.fire({
									title: "Failed",
									text: "Record could not be saved " + errorThrown,
									icon: "error",
									confirmButtonText: "Ok"
								});
							}
						});
                    }
                }
			}
		}
	});
};

var RegistrationFormWizard = function () {
	"use strict";
	return {
		init: function () {
			handleRegistrationWizards();
		}
	};
}();