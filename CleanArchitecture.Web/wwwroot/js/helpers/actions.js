// Click delegation driven by data-action

/**
 * @param {EventTarget} container
 * @param {Record<string, (dataset: DOMStringMap, element: HTMLElement) => void>} map - action name -> handler
 */
export function bindActions(container, map) {
    container.addEventListener("click", (event) => {
        const element = event.target.closest("[data-action]");
        if (!element) return;

        const handler = map[element.dataset.action];
        if (!handler) return;

        event.preventDefault();
        if (element.closest(".disabled")) return;

        handler(element.dataset, element);
    });
}

export default { bindActions };
