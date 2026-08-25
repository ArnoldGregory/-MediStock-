$(function () {
    /*
     * For the sake keeping the code clean, this file
     * contains only the plugin configuration & callbacks.
     * 
     * UI functions ui_* can be located in: debbuger.js
     */
    $('#drag-and-drop-zone-ids').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-ids', 'files');
            $('#uploadedFiles').append(file.name + '|<br />');
            document.getElementById("label_national_id").innerHTML = file.name;
            document.getElementById("label_text_national_id").value = file.name;
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-ids');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-ids');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-ids');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-ids');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-ids');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-ids');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-ids');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-ids');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-ids');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
            document.getElementById("label_national_id").innerHTML = '';
            document.getElementById("label_text_national_id").value = '';
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
            document.getElementById("label_national_id").innerHTML = '';
            document.getElementById("label_text_national_id").value = '';
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size/1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
            document.getElementById("label_national_id").innerHTML = '';
            document.getElementById("label_text_national_id").value = '';
        }
    });

    $('#drag-and-drop-zone-passport-photo').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-passport-photo', 'files');
            $('#uploadedFiles').append(file.name + '|<br />');
            document.getElementById("label_passportphoto").innerHTML = file.name;
            document.getElementById("label_text_passportphoto").value = file.name;
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-passport-photo');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-passport-photo');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-passport-photo');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-passport-photo');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-passport-photo');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-passport-photo');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-passport-photo');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-passport-photo');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-passport-photo');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
            document.getElementById("label_passportphoto").innerHTML = '';
            document.getElementById("label_text_passportphoto").value = '';
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
            document.getElementById("label_passportphoto").innerHTML = '';
            document.getElementById("label_text_passportphoto").value = '';
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size/1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
            document.getElementById("label_passportphoto").innerHTML = '';
            document.getElementById("label_text_passportphoto").value = '';
        }
    });

    $('#drag-and-drop-zone-signature').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-signature', 'files');
            $('#uploadedFiles').append(file.name + '|<br />');
            document.getElementById("label_signature").innerHTML = file.name;
            document.getElementById("label_text_signature").value = file.name;
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-signature');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-signature');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-signature');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-signature');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-signature');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-signature');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-signature');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-signature');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-signature');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
            document.getElementById("label_signature").innerHTML = '';
            document.getElementById("label_text_signature").value = '';
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
            document.getElementById("label_signature").innerHTML = '';
            document.getElementById("label_text_signature").value = '';
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size/1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
            document.getElementById("label_signature").innerHTML = '';
            document.getElementById("label_text_signature").value = '';
        }
    });

    $('#drag-and-drop-zone-sig-kra-pin').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-sig-kra-pin', 'files');
            $('#uploadedFiles').append(file.name + '|<br />');
            document.getElementById("label_sigkrapin").innerHTML = file.name;
            document.getElementById("label_text_sigkrapin").value = file.name;
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-sig-kra-pin');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-sig-kra-pin');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-sig-kra-pin');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-sig-kra-pin');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-sig-kra-pin');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-sig-kra-pin');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-sig-kra-pin');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-sig-kra-pin');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-sig-kra-pin');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
            document.getElementById("label_sigkrapin").innerHTML = '';
            document.getElementById("label_text_sigkrapin").value = '';
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
            document.getElementById("label_sigkrapin").innerHTML = '';
            document.getElementById("label_text_sigkrapin").value = '';
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size/1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
            document.getElementById("label_sigkrapin").innerHTML = '';
            document.getElementById("label_text_sigkrapin").value = '';
        }
    });

    $('#drag-and-drop-zone-utility-bill').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-utility-bill', 'files');
            $('#uploadedFiles').append(file.name + '|<br />');
            document.getElementById("label_utilitybill").innerHTML = file.name;
            document.getElementById("label_text_utilitybill").value = file.name;
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-utility-bill');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-utility-bill');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-utility-bill');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-utility-bill');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-utility-bill');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-utility-bill');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-utility-bill');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-utility-bill');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-utility-bill');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
            document.getElementById("label_utilitybill").innerHTML = '';
            document.getElementById("label_text_utilitybill").value = '';
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
            document.getElementById("label_utilitybill").innerHTML = '';
            document.getElementById("label_text_utilitybill").value = '';
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size/1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
            document.getElementById("label_utilitybill").innerHTML = '';
            document.getElementById("label_text_utilitybill").value = '';
        }
    });

    $('#drag-and-drop-zone-banking-details').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-banking-details', 'files');
            $('#uploadedFiles').append(file.name + '|<br />');
            document.getElementById("label_proofofbanking").innerHTML = file.name;
            document.getElementById("label_text_proofofbanking").value = file.name;
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-banking-details');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-banking-details');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-banking-details');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-banking-details');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-banking-details');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-banking-details');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-banking-details');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-banking-details');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-banking-details');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
            document.getElementById("label_proofofbanking").innerHTML = '';
            document.getElementById("label_text_proofofbanking").value = '';
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
            document.getElementById("label_proofofbanking").innerHTML = '';
            document.getElementById("label_text_proofofbanking").value = '';
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size/1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
            document.getElementById("label_proofofbanking").innerHTML = '';
            document.getElementById("label_text_proofofbanking").value = '';
        }
    });

    $('#drag-and-drop-zone-proof-of-funds').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-proof-of-funds', 'files');
            $('#uploadedFiles').append(file.name + '|<br />');
            document.getElementById("label_proofoffunds").innerHTML = file.name;
            document.getElementById("label_text_proofoffunds").value = file.name;
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-proof-of-funds');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-proof-of-funds');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-proof-of-funds');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-proof-of-funds');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-proof-of-funds');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-proof-of-funds');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-proof-of-funds');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-proof-of-funds');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-proof-of-funds');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileTypeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file type', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added: invalid file type");
            document.getElementById("label_proofoffunds").innerHTML = '';
            document.getElementById("label_text_proofoffunds").value = '';
        },
        onFileExtError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: invalid file extension', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; invalid file extension, allowed extensions are doc, docx, pdf, jpg, jpeg, png and gif");
            document.getElementById("label_proofoffunds").innerHTML = '';
            document.getElementById("label_text_proofoffunds").value = '';
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');

            RemoveFromUploadedFiles(file.name, "File " + file.name + " cannot be added; size limit exceeded, file size is " + (file.size/1000000).toFixed(2) + " MB while maximum allowed is 3 MB");
            document.getElementById("label_proofoffunds").innerHTML = '';
            document.getElementById("label_text_proofoffunds").value = '';
        }
    });

    function RemoveFromUploadedFiles(file_to_replace, message) {
        var container = document.getElementById('uploadedFiles');
        var customer_files = container.textContent.trim();
        customer_files = customer_files.replace(file_to_replace + '|', '');
        document.getElementById('uploadedFiles').innerHTML = customer_files;

        Swal.fire({
            title: "Information",
            text: message,
            icon: "warning",
            confirmButtonText: "Ok"
        });
    }
});