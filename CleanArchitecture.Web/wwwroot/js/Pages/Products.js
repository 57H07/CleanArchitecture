export default class Products {
    constructor(paginationId) {
        this.paginationId = paginationId;
    }

    init() {
        if (!window.Pagination) {
            return;
        }

        window.Pagination.init(this.paginationId, {});
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const page = new Products("1");
    page.init();
});
