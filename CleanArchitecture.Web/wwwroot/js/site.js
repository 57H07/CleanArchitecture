import { Theme } from './theme.js';
import Tooltip from './helpers/tooltip.js';
import Toast from './helpers/toast.js';

Theme.init();

Tooltip.initTooltips(document);

// Show the server-rendered toast (from TempData), if _Toast.cshtml emitted one.
document.addEventListener("DOMContentLoaded", function () {
    Toast.showExisting(document.getElementById("infoToast"));
});