import { Theme } from './theme.js';
import Tooltip from './Helpers/tooltip.js';

Theme.init();

Tooltip.initTooltips(document);

// Toast management
document.addEventListener("DOMContentLoaded", function () {
    const toastEl = document.getElementById("infoToast");
    if (toastEl) {
        const toast = new bootstrap.Toast(toastEl, {
            autohide: true,
            delay: 4000
        });
        toast.show();
    }
});