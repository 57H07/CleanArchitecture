// Creates/reuses a single bottom-right toast-container.
// Views/Shared/_Toast.cshtml builds the same markup server-side; keep the two in step.

const VARIANTS = {
    success: { color: "success", icon: "bi-check-circle-fill", header: "Success" },
    error: { color: "danger", icon: "bi-exclamation-triangle-fill", header: "Error" },
    warning: { color: "warning", icon: "bi-exclamation-triangle-fill", header: "Warning" },
    info: { color: "info", icon: "bi-info-circle-fill", header: "Info" }
};

function getContainer() {
    let container = document.querySelector(".toast-container");
    if (!container) {
        container = document.createElement("div");
        container.className = "toast-container position-fixed bottom-0 end-0 p-3";
        container.style.zIndex = "1100";
        document.body.appendChild(container);
    }
    return container;
}

/**
 * Builds, shows and auto-removes a Bootstrap toast.
 * @param {string} message
 * @param {"success"|"error"|"warning"|"info"} [type="info"]
 * @param {object} [options]
 * @param {string} [options.header] - overrides the variant's default header text.
 * @param {number} [options.delay=4000]
 * @param {boolean} [options.autohide=true]
 * @returns {bootstrap.Toast}
 */
function show(message, type = "info", options = {}) {
    const { header, delay = 4000, autohide = true } = options;
    const variant = VARIANTS[type] || VARIANTS.info;

    const toastEl = document.createElement("div");
    toastEl.className = `toast toast--${variant.color}`;
    toastEl.setAttribute("role", "alert");
    toastEl.setAttribute("aria-live", "assertive");
    toastEl.setAttribute("aria-atomic", "true");
    toastEl.innerHTML = `
        <div class="toast-header">
            <i class="bi ${variant.icon} me-2"></i>
            <strong class="me-auto"></strong>
            <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body"></div>`;
    // Set as text, not interpolated above: server messages can quote user input.
    toastEl.querySelector(".toast-header strong").textContent = header || variant.header;
    toastEl.querySelector(".toast-body").textContent = message;

    getContainer().appendChild(toastEl);
    const toast = new bootstrap.Toast(toastEl, { autohide, delay });
    toast.show();
    toastEl.addEventListener("hidden.bs.toast", () => toastEl.remove());
    return toast;
}

/** Wires autohide+show onto a toast element already rendered server-side (e.g. from TempData in _Toast.cshtml). */
function showExisting(toastEl, options = {}) {
    if (!toastEl) return null;
    const { delay = 4000, autohide = true } = options;
    const toast = new bootstrap.Toast(toastEl, { autohide, delay });
    toast.show();
    return toast;
}

const Toast = {
    show,
    success: (message, options) => show(message, "success", options),
    error: (message, options) => show(message, "error", options),
    warning: (message, options) => show(message, "warning", options),
    info: (message, options) => show(message, "info", options),
    showExisting
};

export default Toast;
