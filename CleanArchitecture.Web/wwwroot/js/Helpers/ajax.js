// Generic vanilla-JS AJAX helper for Razor forms.
// Handles the antiforgery token, JSON/FormData bodies, ModelState-shaped
// validation errors, and distinguishing JSON error payloads from the
// Development exception page (HTML) so callers never have to parse either by hand.

const TOKEN_FIELD_NAME = "__RequestVerificationToken";
const TOKEN_HEADER_NAME = "RequestVerificationToken";

function getAntiForgeryToken(scope) {
    const input = (scope || document).querySelector(`input[name="${TOKEN_FIELD_NAME}"]`);
    return input ? input.value : null;
}

function isMutatingMethod(method) {
    return !["GET", "HEAD"].includes(method.toUpperCase());
}

async function parseResponse(response) {
    const contentType = response.headers.get("content-type") || "";
    const isJson = contentType.includes("application/json");
    const body = isJson
        ? await response.json().catch(() => null)
        : await response.text().catch(() => "");

    if (!response.ok) {
        const message =
            (isJson && body && (body.message || body?.error?.message)) ||
            (!isJson && response.status === 404 ? "Not found." : null) ||
            "The request could not be completed.";

        const error = new Error(message);
        error.status = response.status;
        error.body = body;
        error.isJson = isJson;
        error.errors = isJson && body && typeof body.errors === "object" ? body.errors : null;
        throw error;
    }

    return { status: response.status, body, isJson };
}

/**
 * Sends an AJAX request.
 * @param {string} url
 * @param {object} [options]
 * @param {string} [options.method="GET"]
 * @param {HTMLFormElement} [options.form] - serialized as FormData; antiforgery token read from it automatically.
 * @param {object|FormData} [options.data] - plain object (sent as JSON) or FormData. Ignored when `form` is set (merged in instead).
 * @param {object} [options.headers]
 * @param {AbortSignal} [options.signal]
 */
async function request(url, options = {}) {
    const { method = "GET", form = null, data = null, headers = {}, signal } = options;
    const finalHeaders = { "X-Requested-With": "XMLHttpRequest", Accept: "application/json", ...headers };
    let body;

    if (form instanceof HTMLFormElement) {
        body = new FormData(form);
        if (data && typeof data === "object" && !(data instanceof FormData)) {
            Object.entries(data).forEach(([key, value]) => body.set(key, value));
        }
    } else if (data instanceof FormData) {
        body = data;
    } else if (data !== null && data !== undefined) {
        finalHeaders["Content-Type"] = "application/json";
        body = JSON.stringify(data);
    }

    if (isMutatingMethod(method)) {
        const token = getAntiForgeryToken(form);
        if (token) finalHeaders[TOKEN_HEADER_NAME] = token;
    }

    const response = await fetch(url, { method, headers: finalHeaders, body, credentials: "same-origin", signal });
    return parseResponse(response);
}

/** Clears any validation UI previously applied by applyValidationErrors(). */
function clearValidationErrors(form) {
    form.querySelectorAll(".is-invalid").forEach((el) => el.classList.remove("is-invalid"));
    form.querySelectorAll("[data-valmsg-for]").forEach((el) => {
        el.textContent = "";
    });
    const summary = form.querySelector("[data-valmsg-summary]");
    if (summary) {
        summary.innerHTML = "";
        summary.hidden = true;
    }
}

/**
 * Applies a ModelState-shaped error dictionary ({ FieldName: "message" | ["message", ...] })
 * onto a Razor form's asp-validation-for spans (matched via data-valmsg-for) and marks
 * the matching inputs invalid. Unmatched (e.g. "") entries go into a [data-valmsg-summary] element if present.
 */
function applyValidationErrors(form, errors) {
    clearValidationErrors(form);
    if (!errors) return;

    const summaryMessages = [];

    Object.entries(errors).forEach(([field, messages]) => {
        const message = Array.isArray(messages) ? messages[0] : messages;
        if (!field) {
            summaryMessages.push(message);
            return;
        }

        const input = form.querySelector(`[name="${field}"]`);
        const span = form.querySelector(`[data-valmsg-for="${field}"]`);

        if (input) input.classList.add("is-invalid");
        if (span) {
            span.textContent = message;
        } else {
            summaryMessages.push(message);
        }
    });

    if (summaryMessages.length) {
        const summary = form.querySelector("[data-valmsg-summary]");
        if (summary) {
            summary.innerHTML = summaryMessages.map((m) => `<div>${m}</div>`).join("");
            summary.hidden = false;
        }
    }
}

const Ajax = {
    get: (url, options) => request(url, { ...options, method: "GET" }),
    post: (url, data, options) => request(url, { ...options, method: "POST", data }),
    put: (url, data, options) => request(url, { ...options, method: "PUT", data }),
    delete: (url, options) => request(url, { ...options, method: "DELETE" }),
    /** Submits a Razor <form> (and its antiforgery token) as multipart/urlencoded FormData. */
    submitForm: (form, options) => request(options?.url || form.getAttribute("action") || window.location.href, {
        ...options,
        method: options?.method || form.getAttribute("method") || "POST",
        form
    }),
    applyValidationErrors,
    clearValidationErrors,
    getAntiForgeryToken
};

export default Ajax;
