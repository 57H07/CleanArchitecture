// Generic Bootstrap toast helper. Creates/reuses a single bottom-right
// toast-container and knows how to render each of Bootstrap's semantic
// variants, so callers just say what happened, not how to build the markup.

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
    toastEl.className = `toast border-${variant.color}`;
    toastEl.setAttribute("role", "alert");
    toastEl.setAttribute("aria-live", "assertive");
    toastEl.setAttribute("aria-atomic", "true");
    toastEl.innerHTML = `
        <div class="toast-header text-bg-${variant.color}">
            <i class="bi ${variant.icon} me-2"></i>
            <strong class="me-auto">${header || variant.header}</strong>
            <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body">${message}</div>`;

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
