/**
 * riziki-utils.js — Global helpers for RIZIKI Portal
 * Provides: btnLoad, btnStop, pageBlock, pageUnblock
 */

/**
 * Show a spinner on a button and disable it while an AJAX call is running.
 * @param {HTMLElement|jQuery} btn  The button element (pass `this` from onclick)
 * @param {string} [label]          Loading label — defaults to "Processing..."
 */
function btnLoad(btn, label) {
    var $b = $(btn);
    $b.data('rz-orig', $b.html()).prop('disabled', true)
      .html('<i class="fa fa-spinner fa-spin m-r-5"></i>' + (label || 'Processing...'));
}

/**
 * Restore a button to its original state after an AJAX call.
 * @param {HTMLElement|jQuery} btn
 */
function btnStop(btn) {
    var $b = $(btn);
    $b.prop('disabled', false).html($b.data('rz-orig') || $b.html());
}

/**
 * Show a full-page overlay (blockUI). Use for data loads where there is no
 * single button to put the spinner on (e.g. loading a report table).
 * @param {string} [msg]
 */
function pageBlock(msg) {
    $.blockUI({
        message: '<h5 style="margin:0;"><i class="fa fa-spinner fa-spin m-r-5"></i>' + (msg || 'Please wait...') + '</h5>',
        css: {
            border: 'none', padding: '12px 24px',
            backgroundColor: '#2d353c', borderRadius: '6px',
            color: '#fff', width: 'auto', left: '50%',
            transform: 'translateX(-50%)', top: '45%'
        },
        overlayCSS: { backgroundColor: '#000', opacity: 0.3 }
    });
}

/** Hide the full-page blockUI overlay. */
function pageUnblock() { $.unblockUI(); }
