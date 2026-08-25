class Tooltip {

    static initTooltips = (container) => {
        [...container.querySelectorAll("[data-bs-toggle='tooltip']")].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }
}
export default Tooltip;