import Ajax from "./ajax.js";

const jq = () => window.jQuery;

/**
 * Replaces `host`'s content with the partial served at `url` and arms unobtrusive validation on it.
 * @returns {HTMLFormElement|null} the injected form
 */
export async function loadForm(host, url) {
    const { body } = await Ajax.get(url);
    host.innerHTML = body;
    jq()?.validator?.unobtrusive?.parse(host);

    return host.querySelector("form");
}

/** False only when unobtrusive validation is armed and the form fails it. */
export function isValid(form) {
    const $form = jq()?.(form);

    return !$form?.data("validator") || $form.valid();
}

export default { loadForm, isValid };
