// Tooltips render into document.body
// Call disposeTooltips() on the old content before replacing markup.

const SELECTOR = "[data-bs-toggle='tooltip']";

class Tooltip {

    static initTooltips = (container) => {
        container.querySelectorAll(SELECTOR).forEach(el => bootstrap.Tooltip.getOrCreateInstance(el));
    }

    static disposeTooltips = (container) => {
        container.querySelectorAll(SELECTOR).forEach(el => bootstrap.Tooltip.getInstance(el)?.dispose());
    }
}
export default Tooltip;
