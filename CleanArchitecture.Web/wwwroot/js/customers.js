import Ajax from "./Helpers/ajax.js";
import Toast from "./Helpers/toast.js";

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

    function currentFilterRoute() {
        const route = {};
        new FormData(filterForm).forEach((value, key) => {
            if (value !== "") route[key] = value;
        });
        return route;
    }

    async function loadCustomers(route) {
        const query = new URLSearchParams(route).toString();
        const url = query ? `${INDEX_URL}?${query}` : INDEX_URL;

        const { body } = await Ajax.get(url);
        tableContainer.innerHTML = body;
        window.history.replaceState(null, "", url);
    }

    function refreshWithCurrentFilters(extra) {
        return loadCustomers({ ...currentFilterRoute(), ...extra });
    }

    filterForm.addEventListener("submit", (event) => {
        event.preventDefault();
        loadCustomers({ ...currentFilterRoute(), page: "1" });
    });

    filterForm.querySelector("#pageSize")?.addEventListener("change", () => filterForm.requestSubmit());

    // Delegated handlers: the table container's content is replaced on every reload.
    tableContainer.addEventListener("click", (event) => {
        const sortLink = event.target.closest(".customer-sort-link");
        const pageLink = event.target.closest(".customer-page-link");
        const editBtn = event.target.closest(".customer-edit-btn");
        const deleteBtn = event.target.closest(".customer-delete-btn");
        const emptyCreateBtn = event.target.closest("#customerEmptyCreateBtn");

        if (sortLink || pageLink) {
            event.preventDefault();
            const link = sortLink || pageLink;
            if (link.closest(".disabled")) return;
            loadCustomers(JSON.parse(link.dataset.route));
        } else if (editBtn) {
            openEditModal(editBtn.dataset.id);
        } else if (deleteBtn) {
            openDeleteModal(deleteBtn.dataset.id, deleteBtn.dataset.name);
        } else if (emptyCreateBtn) {
            openCreateModal();
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
        document.getElementById("customerModalLabel").textContent = "New Customer";
        customerModal.show();
    }

    async function openEditModal(id) {
        resetForm();
        try {
            const { body: customer } = await Ajax.get(`${INDEX_URL}/GetDetails/${id}`);
            document.getElementById("customerModalLabel").textContent = "Edit Customer";
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
        try {
            const { body } = await Ajax.submitForm(customerForm, { url, method: "POST" });
            customerModal.hide();
            Toast.success(body.message);
            await refreshWithCurrentFilters();
        } catch (error) {
            if (error.errors) {
                Ajax.applyValidationErrors(customerForm, error.errors);
            } else {
                Toast.error(error.message);
            }
        } finally {
            setBusy(submitBtn, submitSpinner, false);
        }
    });

    function openDeleteModal(id, name) {
        pendingDeleteId = id;
        document.getElementById("customerDeleteName").textContent = name;
        deleteModal.show();
    }

    deleteConfirmBtn.addEventListener("click", async () => {
        if (!pendingDeleteId) return;

        setBusy(deleteConfirmBtn, deleteSpinner, true);
        try {
            const { body } = await Ajax.post(`${INDEX_URL}/Delete/${pendingDeleteId}`);
            deleteModal.hide();
            Toast.success(body.message);
            await refreshWithCurrentFilters();
        } catch (error) {
            deleteModal.hide();
            Toast.error(error.message);
        } finally {
            setBusy(deleteConfirmBtn, deleteSpinner, false);
            pendingDeleteId = null;
        }
    });

    function setBusy(button, spinner, busy) {
        button.disabled = busy;
        spinner.classList.toggle("d-none", !busy);
    }
}
