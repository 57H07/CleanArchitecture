import Ajax from "../helpers/ajax.js";
import Toast from "../helpers/toast.js";
import Tooltip from "../helpers/tooltip.js";
import Modal from "../helpers/modal.js";
import { bindActions } from "../helpers/actions.js";

const tableContainer = document.getElementById("customerTableWrapper");
const filterForm = document.getElementById("customerFilterForm");
const formHost = document.getElementById("customerFormHost");
const customerModalEl = document.getElementById("customerModal");
const deleteModalEl = document.getElementById("customerDeleteModal");

if (tableContainer && filterForm && formHost && customerModalEl && deleteModalEl) {
    const customerModal = new bootstrap.Modal(customerModalEl);
    const deleteModal = new bootstrap.Modal(deleteModalEl);
    const deleteConfirmBtn = document.getElementById("customerDeleteConfirmBtn");
    const deleteSpinner = document.getElementById("customerDeleteSpinner");
    const countLabel = document.getElementById("customerCount");
    const clearBtn = document.getElementById("customerClearBtn");
    const sortByInput = filterForm.querySelector('input[name="sortBy"]');
    const sortOrderInput = filterForm.querySelector('input[name="sortOrder"]');

    let pendingDeleteUrl = null;

    // ---------------------------------------------------------------- wiring
    // Every way this page can be driven. One click listener for the whole page:
    // the table is replaced on every reload, and the view names each behaviour
    // through data-action.

    bindActions(document, {
        navigate: ({ route }) => loadCustomersOrReport(JSON.parse(route)),
        create: ({ url }) => openForm(url),
        edit: ({ url }) => openForm(url),
        delete: ({ url, name }) => openDeleteModal(url, name),
        "clear-filters": clearFilters,
        "confirm-delete": confirmDelete
    });

    filterForm.addEventListener("submit", applyFilters);
    filterForm.querySelector("#pageSize")?.addEventListener("change", () => filterForm.requestSubmit());

    // Delegated: the form element itself is replaced every time the modal is opened.
    formHost.addEventListener("submit", saveCustomer);

    // ------------------------------------------------------------------ list

    function applyFilters(event) {
        event.preventDefault();
        loadCustomersOrReport({ ...currentFilterRoute(), page: "1" });
    }

    function clearFilters() {
        filterForm.reset();
        loadCustomersOrReport({});
    }

    function loadCustomersOrReport(route) {
        return loadCustomers(route).catch((error) => Toast.error(error.message));
    }

    function refreshWithCurrentFilters(extra) {
        return loadCustomers({ ...currentFilterRoute(), ...extra });
    }

    async function loadCustomers(route) {
        const query = new URLSearchParams(route).toString();
        const url = query ? `${filterForm.action}?${query}` : filterForm.action;

        // Fade the list while it is being replaced
        tableContainer.classList.add("is-loading");
        try {
            const { body } = await Ajax.get(url);
            Tooltip.disposeTooltips(tableContainer);
            tableContainer.innerHTML = body;
            Tooltip.initTooltips(tableContainer);
            syncOutsideTable();
            syncSortInputs(route);
            window.history.replaceState(null, "", url);
        } finally {
            tableContainer.classList.remove("is-loading");
        }
    }

    function currentFilterRoute() {
        const route = {};
        new FormData(filterForm).forEach((value, key) => {
            if (value !== "") route[key] = value;
        });
        return route;
    }

    function syncSortInputs(route) {
        if (sortByInput && route.sortBy !== undefined) sortByInput.value = route.sortBy;
        if (sortOrderInput && route.sortOrder !== undefined) sortOrderInput.value = route.sortOrder;
    }

    // The page head and the toolbar live outside the swapped region; the partial carries their state.
    function syncOutsideTable() {
        const summary = tableContainer.querySelector("#customerTableContainer");
        if (!summary) return;
        if (countLabel) countLabel.textContent = summary.dataset.countLabel;
        if (clearBtn) clearBtn.hidden = summary.dataset.hasFilters !== "true";
    }

    // --------------------------------------------------------- create / edit

    async function openForm(url) {
        try {
            await Modal.loadForm(formHost, url);
            customerModal.show();
        } catch (error) {
            Toast.error(error.message);
        }
    }

    async function saveCustomer(event) {
        event.preventDefault();

        const form = event.target;
        if (!Modal.isValid(form)) return;

        const submitBtn = form.querySelector("#customerSubmitBtn");
        const submitSpinner = form.querySelector("#customerSubmitSpinner");

        setBusy(submitBtn, submitSpinner, true);
        let saved = false;
        try {
            const { body } = await Ajax.submitForm(form);
            customerModal.hide();
            Toast.success(body.message);
            saved = true;
        } catch (error) {
            if (error.errors) {
                Ajax.applyValidationErrors(form, error.errors);
            } else {
                Toast.error(error.message);
            }
        } finally {
            setBusy(submitBtn, submitSpinner, false);
        }

        // Outside the try: a failing refresh must not be reported as a failed save.
        if (saved) await refreshWithCurrentFilters().catch((error) => Toast.error(error.message));
    }

    // ---------------------------------------------------------------- delete

    function openDeleteModal(url, name) {
        pendingDeleteUrl = url;
        document.getElementById("customerDeleteName").textContent = name;
        deleteModal.show();
    }

    async function confirmDelete() {
        if (!pendingDeleteUrl) return;

        setBusy(deleteConfirmBtn, deleteSpinner, true);
        let deleted = false;
        try {
            const { body } = await Ajax.post(pendingDeleteUrl);
            deleteModal.hide();
            Toast.success(body.message);
            deleted = true;
        } catch (error) {
            deleteModal.hide();
            Toast.error(error.message);
        } finally {
            setBusy(deleteConfirmBtn, deleteSpinner, false);
            pendingDeleteUrl = null;
        }

        if (deleted) await refreshWithCurrentFilters().catch((error) => Toast.error(error.message));
    }

    // ---------------------------------------------------------------- shared

    function setBusy(button, spinner, busy) {
        button.disabled = busy;
        spinner.classList.toggle("d-none", !busy);
    }
}
