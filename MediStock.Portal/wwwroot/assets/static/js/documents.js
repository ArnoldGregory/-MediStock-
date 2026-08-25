
var files_array = [];


$(document).ready(function () {
    App.init();

    InitiateEditableDataTable.init();
    InitiateDocumentDataTable.init();

    GetEmployeeDoc();
   
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
                    { "data": "names", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "email", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "document_desc", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "original_file_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-primary btn-xs download'> Download </a>"
                    },

                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa-solid fa-trash-can'></i> Delete</a>"
                    }
                ]
            });


            var isView = null;

            //View
            $('#editabledatatable').on("click", 'a.download', function (e) {
                e.preventDefault();

                nRow = $(this).parents('tr')[0];

                //console.log($(this).parents('tr').attr("recid"));

                console.log(nRow);

                if (isView !== null && isView != nRow) {
                    //restoreRow(oTable, isEditing);
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

                console.log(json);

                window.open("/assets/client_documents/" + json["new_file_name"], '_blank');


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



/** Documents Start */
$("body").on("click", ".add_new_frm_field_btn", function () {
    var index = $(".form_field_outer").find(".form_field_outer_row").length + 1;

    $(".form_field_outer").append(`
          <div class="row form_field_outer_row">
              <div class="form-group col-md-6">
				<select name="doc_type[]" id="doc_type_${index}" class="form-control selectpicker" style="width: 100%" >
                </select>
              </div>
              <div class="form-group col-md-4">
                <div class="input-group">
					<div id="drag-and-drop-zone-${index}" class="dm-uploader">
						<div class="btn btn-primary btn-sm">
							<span id="span-${index}">Open the file browser</span>
							<input type="file" title="Click to add file" />
						</div>
					</div>
				</div>
              </div>
              <div class="form-group col-md-2 add_del_btn_outer">
				<!--<a href="#" class="btn_round add_node_btn_frm_field" title="Copy or clone this row">
                  <i class="fas fa-copy"></i>
                </a>-->
                <a href="#" class="btn_round remove_node_btn_frm_field" id="btn-${index}" disabled>
                  <i class="fas fa-trash-alt"></i>
                </a>
              </div>
            </div>
        `);

    $('.selectpicker').select2({
        style: 'btn-white',
        size: 5
    });

    GetDocumentTypes("#doc_type_" + index);

    $(".form_field_outer").find(".remove_node_btn_frm_field:not(:first)").prop("disabled", false);
    $(".form_field_outer").find(".remove_node_btn_frm_field").first().prop("disabled", true);

    $('#drag-and-drop-zone-' + index).dmUploader({ //
        url: '/ClientSetup/Upload',
        extFilter: ["doc", "docx", "pdf", "jpg", "jpeg", "png", "gif"],
        fieldName: 'postedFiles',
        maxFileSize: 3000000, // 3 Megs 
        onDragEnter: function () {
            // Happens when dragging something over the DnD area
            this.addClass('active');
        },
        onDragLeave: function () {
            // Happens when dragging something OUT of the DnD area
            this.removeClass('active');
        },
        onInit: function () {
            // Plugin is ready to use
            ui_add_log('Penguin initialized :)', 'info');
        },
        onComplete: function () {
            // All files in the queue are processed (success or error)
            ui_add_log('All pending tranfers finished');
        },
        onNewFile: function (id, file) {
            // When a new file is added using the file selector or the DnD area
            ui_add_log('New file added #' + id);
            ui_multi_add_file(id, file, 'uploaderFile', 'files');
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            var dropdown = document.getElementById("doc_type_" + index);
            var document_type = dropdown.value;
            var document_type_description = dropdown.options[dropdown.selectedIndex].text;

            var json = $.parseJSON(JSON.stringify(data));
            for (var i = 0; i < json.length; i++) {
                var item = json[i];

                document.getElementById("span-" + index).innerHTML = item.original_file_name;

                var file_obj = {
                    'id': 0,
                    'original_file_name': item.original_file_name,
                    'new_file_name': item.new_file_name,
                    'document_type': document_type,
                    'document_type_description': document_type_description,
                    'action_type': 'CREATE',
                    /*'link': 'http://192.168.0.1:10024/Uploads/' + item.new_file_name*/
                    'link': '/assets/client_documents/'  + item.new_file_name
                };

                console.log(file_obj);

                files_array.push(file_obj);

                GetTableData($('#documentdatatable').dataTable(), files_array);

                ui_add_log('Server Response for file #' + id + ': ' + item.new_file_name);
                ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
                ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile');
                ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile');
            }
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size / 1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
        }
    });
});

$("body").on("click", ".add_node_btn_frm_field", function (e) {
    var index = $(e.target).closest(".form_field_outer").find(".form_field_outer_row").length + 1;

    var cloned_el = $(e.target).closest(".form_field_outer_row").clone(true);

    $(e.target).closest(".form_field_outer").last().append(cloned_el).find(".remove_node_btn_frm_field:not(:first)").prop("disabled", false);

    $(e.target).closest(".form_field_outer").find(".remove_node_btn_frm_field").first().prop("disabled", true);

    //change id
    $(e.target).closest(".form_field_outer").find(".form_field_outer_row").last().find("input[type='text']").attr("id", "mobile_no_" + index).attr("name", "mobile_no_" + index);

    $(e.target).closest(".form_field_outer").find(".form_field_outer_row").last().find("select").attr("id", "doc_type_" + index).attr("name", "doc_type_" + index);

    console.log("#doc_type_" + index);

    GetDocumentTypes("#doc_type_" + index);
    //count++;
});

$("body").on("click", ".remove_node_btn_frm_field", function (e) {
    var file_to_remove = document.getElementById("span-" + this.id.split('-')[1]).innerHTML;
    //console.log('Files before removing ' + JSON.stringify(files_array));
    //console.log('Removing: ' + file_to_remove);
    files_array = files_array.filter(item => item.original_file_name !== file_to_remove);
    //console.log('Files after removing ' + JSON.stringify(files_array));

    GetTableData($('#documentdatatable').dataTable(), files_array);
    $(this).closest(".form_field_outer_row").remove();
});



var InitiateDocumentDataTable = function () {
    return {
        init: function () {
            //Datatable Initiating
            var oTable = $('#documentdatatable').dataTable({
                "responsive": true,
                "createdRow": function (row, data, dataIndex) {
                    $(row).attr("recid", data.id);
                },
                "columnDefs": [
                    {
                        "targets": 3,
                        "render": function (data, type, row, meta) {
                            return "<a href='" + row.link + "' target='_blank' class='btn btn-info btn-xs download'><i class='fa-solid fa-eye'></i> View</a>";
                        }
                    }
                ],
                "aoColumns": [
                    { "data": "document_type_description", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "original_file_name", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "data": "action_type", "autoWidth": true, "sDefaultContent": "n/a" },
                    { "bSortable": false, "sDefaultContent": "n/a" },
                    {
                        "bSortable": false,
                        "sDefaultContent": "<a href='#' class='btn btn-danger btn-xs delete'><i class='fa-solid fa-trash-can'></i> Delete</a>"
                    }
                ]
            });

            $('#documentdatatable').on("click", 'a.delete', function (e) {
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
                        var current_files_array = oTable.fnGetData();

                        var deleted_file_object = current_files_array.find(item => item.id.toString() === rec.toString());

                        var new_array = files_array.filter(item => item.id.toString() !== rec.toString());

                        var file_obj = {
                            'id': deleted_file_object.id,
                            'original_file_name': deleted_file_object.original_file_name,
                            'new_file_name': deleted_file_object.new_file_name,
                            'document_type': deleted_file_object.document_type,
                            'document_type_description': deleted_file_object.document_type_description,
                            'action_type': 'DELETE',
                            'link': '/assets/client_documents/' + deleted_file_object.new_file_name
                        };

                        new_array.push(file_obj);

                        files_array = new_array;

                        GetTableData($('#documentdatatable').dataTable(), files_array);
                    } else {
                        e.preventDefault();
                    }
                });
            });
        }
    };
}();





/** Documents End */



function GetEmployeeDoc() {
    $.get('GetRecords', { module: 'employe_documents' }, function (data) {
        getData(data);
    });
}

function GetTableData(table, data) {
    oSettings = table.fnSettings();
    table.fnClearTable(this);

    var json = $.parseJSON(JSON.stringify(data));

    for (var i = 0; i < json.length; i++) {
        var item = json[i];
        table.oApi._fnAddData(oSettings, item);
    }
    oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
    table.fnDraw();
}

function GetDocumentTypes(control_name) {
    var a = $(this).closest(".panel");

    var parameters = { module: 'document_per_client' };

    $.ajax({
        url: "/ClientSetup/GetRecords",
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
            $(control_name).get(0).options.length = 0;
            $(control_name).get(0).options[0] = new Option("Please Select Document Type", "-1");

            $.each(data, function (index, item) {
                $(control_name).get(0).options[$(control_name).get(0).options.length] = new Option(item.name, item.Id);
            });

            $(control_name).bind("change", function () {
                //console.log($(this).val());
            });

            //$("#doc_type").get(0).options.length = 0;
            //$("#doc_type").get(0).options[0] = new Option("Please Select Document Type", "-1");

            //$.each(data, function (index, item) {
            //	$("#doc_type").get(0).options[$("#doc_type").get(0).options.length] = new Option(item.Type, item.Id);
            //});

            //$("#doc_type").bind("change", function () {
            //	//console.log($(this).val());
            //});

            $(a).removeClass("panel-loading"), $(a).find(".panel-loader").remove();
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
    var cif = document.getElementById('cif').value;

    if (email === '') {
        Swal.fire({
            title: "Missing information",
            text: "Kindly Enter Email",
            icon: "warning",
            confirmButtonText: "Ok"
        });
        return false;
    }

   


    var parameters = {
        id: id,
        cif: cif,
        email: email,
        employee_documents: files_array,
    };

    

    $.ajax({
        url: "/Employee/CreateEmployeeDocument",
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

            if (data.error_code == '00') {
                GetEmployeeDoc();
                $("#capture-record").modal("hide").data("bs.modal", null);
               
                swal.fire({
                    title: "Success",
                    text: data.error_desc,
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
    $('#cif').val("");
    $('#full_name').val("")
    $('#type').val("").trigger("change");
    $('#documenttype').val("").trigger("change");
    $('#document_number').val("");
    $('#name').val("");
    files_array.length = 0;
    GetTableData($('#documentdatatable').dataTable(), files_array);

    $('.selectpicker').val('');
    $('input[type=file]').val('');
    // Reset other form fields as needed

    // Remove dynamically created nodes
    $(".form_field_outer_row").remove();

});