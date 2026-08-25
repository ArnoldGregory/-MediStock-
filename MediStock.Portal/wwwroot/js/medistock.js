// MediStock Portal - Utility Functions

var MediStock = {
    // Show loading overlay
    showLoading: function () {
        if ($('#loading-overlay').length === 0) {
            $('body').append('<div id="loading-overlay" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(255,255,255,0.7);z-index:9999;display:flex;align-items:center;justify-content:center;"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
        }
        $('#loading-overlay').show();
    },

    // Hide loading overlay
    hideLoading: function () {
        $('#loading-overlay').hide();
    },

    // AJAX GET helper
    get: function (url, callback, errorCallback) {
        $.ajax({
            url: url,
            type: 'GET',
            success: function (response) {
                if (callback) callback(response);
            },
            error: function (xhr) {
                if (errorCallback) {
                    errorCallback(xhr);
                } else {
                    MediStock.handleError(xhr);
                }
            }
        });
    },

    // AJAX POST helper
    post: function (url, data, callback, errorCallback) {
        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (callback) callback(response);
            },
            error: function (xhr) {
                if (errorCallback) {
                    errorCallback(xhr);
                } else {
                    MediStock.handleError(xhr);
                }
            }
        });
    },

    // AJAX PUT helper
    put: function (url, data, callback, errorCallback) {
        $.ajax({
            url: url,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (callback) callback(response);
            },
            error: function (xhr) {
                if (errorCallback) {
                    errorCallback(xhr);
                } else {
                    MediStock.handleError(xhr);
                }
            }
        });
    },

    // AJAX DELETE helper
    delete: function (url, callback, errorCallback) {
        $.ajax({
            url: url,
            type: 'DELETE',
            success: function (response) {
                if (callback) callback(response);
            },
            error: function (xhr) {
                if (errorCallback) {
                    errorCallback(xhr);
                } else {
                    MediStock.handleError(xhr);
                }
            }
        });
    },

    // Handle errors
    handleError: function (xhr) {
        if (xhr.status === 401) {
            window.location.href = '/Account/Login';
        } else {
            Swal.fire('Error', 'An error occurred. Please try again.', 'error');
        }
    },

    // Show success alert
    success: function (title, text) {
        Swal.fire(title || 'Success', text || 'Operation completed successfully', 'success');
    },

    // Show error alert
    error: function (title, text) {
        Swal.fire(title || 'Error', text || 'An error occurred', 'error');
    },

    // Show confirmation dialog
    confirm: function (title, text, callback) {
        Swal.fire({
            title: title || 'Are you sure?',
            text: text || 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc2626',
            cancelButtonColor: '#64748b',
            confirmButtonText: 'Yes, proceed!'
        }).then(function (result) {
            if (result.isConfirmed && callback) {
                callback();
            }
        });
    },

    // Format currency (KES)
    formatCurrency: function (amount) {
        return 'KES ' + parseFloat(amount || 0).toLocaleString('en-KE', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    },

    // Format date
    formatDate: function (dateStr) {
        if (!dateStr) return '-';
        var d = new Date(dateStr);
        return d.toLocaleDateString('en-KE', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    },

    // Format datetime
    formatDateTime: function (dateStr) {
        if (!dateStr) return '-';
        var d = new Date(dateStr);
        return d.toLocaleDateString('en-KE', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Initialize DataTable with standard options
    initDataTable: function (tableId, options) {
        var defaults = {
            responsive: true,
            pageLength: 25,
            order: [[0, 'desc']],
            language: {
                search: '_INPUT_',
                searchPlaceholder: 'Search...',
                lengthMenu: 'Show _MENU_ entries',
                info: 'Showing _START_ to _END_ of _TOTAL_ entries',
                emptyTable: 'No data available'
            }
        };
        var settings = $.extend({}, defaults, options);
        return $(tableId).DataTable(settings);
    },

    // Button loading state
    btnLoad: function (btn) {
        var $btn = $(btn);
        $btn.data('original-text', $btn.html());
        $btn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Processing...');
    },

    // Button reset state
    btnReset: function (btn) {
        var $btn = $(btn);
        $btn.prop('disabled', false).html($btn.data('original-text'));
    },

    // Generate sale number
    generateSaleNumber: function () {
        var now = new Date();
        var prefix = 'SAL';
        var date = now.getFullYear().toString() +
            (now.getMonth() + 1).toString().padStart(2, '0') +
            now.getDate().toString().padStart(2, '0');
        var random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
        return prefix + date + random;
    },

    // Generate PO number
    generatePONumber: function () {
        var now = new Date();
        var prefix = 'PO';
        var date = now.getFullYear().toString() +
            (now.getMonth() + 1).toString().padStart(2, '0') +
            now.getDate().toString().padStart(2, '0');
        var random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
        return prefix + date + random;
    },

    // Generate prescription number
    generateRxNumber: function () {
        var now = new Date();
        var prefix = 'RX';
        var date = now.getFullYear().toString() +
            (now.getMonth() + 1).toString().padStart(2, '0') +
            now.getDate().toString().padStart(2, '0');
        var random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
        return prefix + date + random;
    }
};
