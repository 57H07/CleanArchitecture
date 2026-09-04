import Ajax from "./Helpers/ajax.js";
import Toast from "./Helpers/toast.js";
import Tooltip from "./Helpers/tooltip.js";

const INDEX_URL = "/Customers";

const tableContainer = document.getElementById("customerTableWrapper");
const filterForm = document.getElementById("customerFilterForm");
const customerForm = document.getElementById("customerForm");
const customerModalEl = document.getElementById("customerModal");
const deleteModalEl = document.getElementById("customerDeleteModal");

if (tableContainer && filterForm && customerForm && customerModalEl && deleteModalEl) {
    const customerModal = new bootstrap.Modal(customerModalEl);
    const deleteModal = new bootstrap.Modal(deleteModalEl);
    const submitBtn = document.getElementById("customerSubmitBtn");
    const submitSpinner = document.getElementById("customerSubmitSpinner");
    const deleteConfirmBtn = document.getElementById("customerDeleteConfirmBtn");
    const deleteSpinner = document.getElementById("customerDeleteSpinner");
    let pendingDeleteId = null;

    const sortByInput = filterForm.querySelector('input[name="sortBy"]');
    const sortOrderInput = filterForm.querySelector('input[name="sortOrder"]');

    function currentFilterRoute() {
        const route = {};
        new FormData(filterForm).forEach((value, key) => {
            if (value !== "") route[key] = value;
        });
        return route;
    }

    // Sorting goes through the table's data-route links, which never touch the filter
    // form; without this its hidden fields would keep replaying the original sort.
    function syncSortInputs(route) {
        if (sortByInput && route.sortBy !== undefined) sortByInput.value = route.sortBy;
        if (sortOrderInput && route.sortOrder !== undefined) sortOrderInput.value = route.sortOrder;
    }

    async function loadCustomers(route) {
        const query = new URLSearchParams(route).toString();
        const url = query ? `${INDEX_URL}?${query}` : INDEX_URL;

        // Fade the list while it is being replaced
        tableContainer.classList.add("is-loading");
        try {
            const { body } = await Ajax.get(url);
            Tooltip.disposeTooltips(tableContainer);
            tableContainer.innerHTML = body;
            Tooltip.initTooltips(tableContainer);
            syncSortInputs(route);
            window.history.replaceState(null, "", url);
        } finally {
            tableContainer.classList.remove("is-loading");
        }
    }

    function refreshWithCurrentFilters(extra) {
        return loadCustomers({ ...currentFilterRoute(), ...extra });
    }

    function loadCustomersOrReport(route) {
        return loadCustomers(route).catch((error) => Toast.error(error.message));
    }

    filterForm.addEventListener("submit", (event) => {
        event.preventDefault();
        loadCustomersOrReport({ ...currentFilterRoute(), page: "1" });
    });

    filterForm.querySelector("#pageSize")?.addEventListener("change", () => filterForm.requestSubmit());

    function clearFilters() {
        filterForm.reset();
        loadCustomersOrReport({});
    }

    filterForm.querySelector("#customerClearFiltersBtn")?.addEventListener("click", (event) => {
        event.preventDefault();
        clearFilters();
    });

    // Delegated handlers: the table container's content is replaced on every reload.
    tableContainer.addEventListener("click", (event) => {
        const sortLink = event.target.closest(".customer-sort-link");
        const pageLink = event.target.closest(".customer-page-link");
        const editBtn = event.target.closest(".customer-edit-btn");
        const deleteBtn = event.target.closest(".customer-delete-btn");
        const emptyCreateBtn = event.target.closest("#customerEmptyCreateBtn");
        const emptyClearBtn = event.target.closest("#customerEmptyClearBtn");

        if (sortLink || pageLink) {
            event.preventDefault();
            const link = sortLink || pageLink;
            if (link.closest(".disabled")) return;
            loadCustomersOrReport(JSON.parse(link.dataset.route));
        } else if (editBtn) {
            openEditModal(editBtn.dataset.id);
        } else if (deleteBtn) {
            openDeleteModal(deleteBtn.dataset.id, deleteBtn.dataset.name);
        } else if (emptyCreateBtn) {
            openCreateModal();
        } else if (emptyClearBtn) {
            event.preventDefault();
            clearFilters();
        }
    });

    document.getElementById("customerCreateBtn")?.addEventListener("click", openCreateModal);

    function resetForm() {
        customerForm.reset();
        document.getElementById("customerId").value = "";
        Ajax.clearValidationErrors(customerForm);
    }

    function openCreateModal() {
        resetForm();
        document.getElementById("customerModalLabel").textContent = "New customer";
        customerModal.show();
    }

    async function openEditModal(id) {
        resetForm();
        try {
            const { body: customer } = await Ajax.get(`${INDEX_URL}/GetDetails/${id}`);
            document.getElementById("customerModalLabel").textContent = "Edit customer";
            document.getElementById("customerId").value = customer.id;
            document.getElementById("customerName").value = customer.name ?? "";
            document.getElementById("customerEmail").value = customer.email ?? "";
            document.getElementById("customerPhone").value = customer.phone ?? "";
            document.getElementById("customerCompany").value = customer.company ?? "";
            document.getElementById("customerNotes").value = customer.notes ?? "";
            document.getElementById("customerIsActive").checked = !!customer.isActive;
            customerModal.show();
        } catch (error) {
            Toast.error(error.message);
        }
    }

    customerForm.addEventListener("submit", async (event) => {
        event.preventDefault();

        const id = document.getElementById("customerId").value;
        const url = id ? `${INDEX_URL}/Edit/${id}` : `${INDEX_URL}/Create`;

        setBusy(submitBtn, submitSpinner, true);
        let saved = false;
        try {
            const { body } = await Ajax.submitForm(customerForm, { url, method: "POST" });
            customerModal.hide();
            Toast.success(body.message);
            saved = true;
        } catch (error) {
            if (error.errors) {
                Ajax.applyValidationErrors(customerForm, error.errors);
            } else {
                Toast.error(error.message);
            }
        } finally {
            setBusy(submitBtn, submitSpinner, false);
        }

        // Outside the try: a failing refresh must not be reported as a failed save.
        if (saved) await refreshWithCurrentFilters().catch((error) => Toast.error(error.message));
    });

    function openDeleteModal(id, name) {
        pendingDeleteId = id;
        document.getElementById("customerDeleteName").textContent = name;
        deleteModal.show();
    }

    deleteConfirmBtn.addEventListener("click", async () => {
        if (!pendingDeleteId) return;

        setBusy(deleteConfirmBtn, deleteSpinner, true);
        let deleted = false;
        try {
            const { body } = await Ajax.post(`${INDEX_URL}/Delete/${pendingDeleteId}`);
            deleteModal.hide();
            Toast.success(body.message);
            deleted = true;
        } catch (error) {
            deleteModal.hide();
            Toast.error(error.message);
        } finally {
            setBusy(deleteConfirmBtn, deleteSpinner, false);
            pendingDeleteId = null;
        }

        if (deleted) await refreshWithCurrentFilters().catch((error) => Toast.error(error.message));
    });

    function setBusy(button, spinner, busy) {
        button.disabled = busy;
        spinner.classList.toggle("d-none", !busy);
    }
}
