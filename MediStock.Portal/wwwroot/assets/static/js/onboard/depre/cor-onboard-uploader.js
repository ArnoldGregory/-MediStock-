$(function () {
    /*
     * For the sake keeping the code clean, this file
     * contains only the plugin configuration & callbacks.
     * 
     * UI functions ui_* can be located in: debbuger.js
     */
    $('#drag-and-drop-zone-cert-of-inc').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-cert-of-inc', 'files-cert-of-inc');
            $('#uploadedFiles').append(file.name + '|<br />')
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-cert-of-inc');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-cert-of-inc');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-cert-of-inc');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-cert-of-inc');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-cert-of-inc');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-cert-of-inc');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-cert-of-inc');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-cert-of-inc');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-cert-of-inc');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
        }
    });

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
            ui_multi_add_file(id, file, 'uploaderFile-ids', 'files-ids');
            $('#uploadedFiles').append(file.name + '|<br />')
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
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
        }
    });

    $('#drag-and-drop-zone-kra-pin').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-kra-pin', 'files-kra-pin');
            $('#uploadedFiles').append(file.name + '|<br />')
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-kra-pin');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-kra-pin');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-kra-pin');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-kra-pin');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-kra-pin');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-kra-pin');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-kra-pin');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-kra-pin');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-kra-pin');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
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
            ui_multi_add_file(id, file, 'uploaderFile-sig-kra-pin', 'files-sig-kra-pin');
            $('#uploadedFiles').append(file.name + '|<br />')
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
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
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
            ui_multi_add_file(id, file, 'uploaderFile-passport-photo', 'files-passport-photo');
            $('#uploadedFiles').append(file.name + '|<br />')
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
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
        }
    });

    $('#drag-and-drop-zone-board-resolution').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-board-resolution', 'files-board-resolution');
            $('#uploadedFiles').append(file.name + '|<br />')
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-board-resolution');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-board-resolution');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-board-resolution');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-board-resolution');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-board-resolution');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-board-resolution');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-board-resolution');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-board-resolution');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-board-resolution');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
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
            ui_multi_add_file(id, file, 'uploaderFile-utility-bill', 'files-utility-bill');
            $('#uploadedFiles').append(file.name + '|<br />')
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
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
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
            ui_multi_add_file(id, file, 'uploaderFile-banking-details', 'files-banking-details');
            $('#uploadedFiles').append(file.name + '|<br />')
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
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
        }
    });

    $('#drag-and-drop-zone-cr-moa').dmUploader({ //
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
            ui_multi_add_file(id, file, 'uploaderFile-cr-moa', 'files-cr-moa');
            $('#uploadedFiles').append(file.name + '|<br />')
        },
        onBeforeUpload: function (id) {
            // about to start uploading a file
            ui_add_log('Starting the upload of #' + id);
            ui_multi_update_file_status(id, 'uploading', 'Uploading...', 'uploaderFile-cr-moa');
            ui_multi_update_file_progress(id, 0, '', true, 'uploaderFile-cr-moa');
        },
        onUploadCanceled: function (id) {
            // Happens when a file is directly canceled by the user.
            ui_multi_update_file_status(id, 'warning', 'Canceled by User', 'uploaderFile-cr-moa');
            ui_multi_update_file_progress(id, 0, 'warning', false, 'uploaderFile-cr-moa');
        },
        onUploadProgress: function (id, percent) {
            // Updating file progress
            ui_multi_update_file_progress(id, percent, '', true, 'uploaderFile-cr-moa');
        },
        onUploadSuccess: function (id, data) {
            // A file was successfully uploaded
            ui_add_log('Server Response for file #' + id + ': ' + data);
            ui_add_log('Upload of file #' + id + ' COMPLETED', 'success');
            ui_multi_update_file_status(id, 'success', 'Upload Complete', 'uploaderFile-cr-moa');
            ui_multi_update_file_progress(id, 100, 'success', false, 'uploaderFile-cr-moa');
        },
        onUploadError: function (id, xhr, status, message) {
            ui_multi_update_file_status(id, 'danger', message, 'uploaderFile-cr-moa');
            ui_multi_update_file_progress(id, 0, 'danger', false, 'uploaderFile-cr-moa');
        },
        onFallbackMode: function () {
            // When the browser doesn't support this plugin :(
            ui_add_log('Plugin cant be used here, running Fallback callback', 'danger');
        },
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
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
            ui_multi_add_file(id, file, 'uploaderFile-proof-of-funds', 'files-proof-of-funds');
            $('#uploadedFiles').append(file.name + '|<br />')
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
        onFileSizeError: function (file) {
            ui_add_log('File \'' + file.name + '\' cannot be added: size excess limit', 'danger');
        }
    });
});