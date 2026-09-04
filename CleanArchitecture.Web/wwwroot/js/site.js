import { Theme } from './theme.js';
import Tooltip from './Helpers/tooltip.js';
import Toast from './Helpers/toast.js';

Theme.init();

Tooltip.initTooltips(document);

// Show the server-rendered toast (from TempData), if _Toast.cshtml emitted one.
document.addEventListener("DOMContentLoaded", function () {
    Toast.showExisting(document.getElementById("infoToast"));
});