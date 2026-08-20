/**
* Pagination - JavaScript functionality for the pagination component
*/
window.Pagination = (function () {
    'use strict';

    // Configuration pour chaque instance de pagination
    const paginationConfigs = new Map();

    /**
    * Initialize a pagination component
    * @param {string} paginationId - The unique pagination ID
    * @param {object} config - Configuration object
    */
    function init(paginationId, config) {
        const defaultConfig = {
            ajaxUrl: null,
            containerId: null,
            method: 'GET',
            additionalParams: {},
            getAdditionalParams: null,
            onPageChange: null,
            onError: null,
            loadingText: 'Chargement...',
            errorText: 'An error occurred while loading the data.',
            csrfToken: null
        };

        paginationConfigs.set(paginationId, { ...defaultConfig, ...config });

        addLoadingStyles();
    }

    /**
    * Change page for a specific pagination
    * @param {string} paginationId - The unique pagination ID
    * @param {number} page - The page number to navigate to
    */
    function changePage(paginationId, page) {
        const config = paginationConfigs.get(paginationId);

        let pageSize = null;
        const pageSizeSelect = document.getElementById(`pageSize_${paginationId}`);
        if (pageSizeSelect) {
            pageSize = pageSizeSelect.value;
        }

        if (!config) {
            console.error(`Pagination configuration not found for ID: ${paginationId}`);
            const url = new URL(window.location);
            url.searchParams.set('page', page);
            if (pageSize) url.searchParams.set('pageSize', pageSize);
            window.location.href = url.toString();
            return;
        }

        if (pageSize) {
            updateParams(paginationId, { pageSize: pageSize });
        }

        if (config.onPageChange) {
            config.onPageChange(page);
        } else if (config.ajaxUrl && config.containerId) {
            loadPageViaAjax(paginationId, page, config);
        } else {
            const url = new URL(window.location);
            url.searchParams.set('page', page);
            if (pageSize) url.searchParams.set('pageSize', pageSize);
            window.location.href = url.toString();
        }
    }

    /**
     * Change page size
     * @param {string} paginationId - The unique pagination ID
     * @param {number} pageSize - The new page size
     */
    function changePageSize(paginationId, pageSize) {
        const config = paginationConfigs.get(paginationId);

        if (!config) {
            const url = new URL(window.location);
            url.searchParams.set('pageSize', pageSize);
            url.searchParams.set('page', 1);
            window.location.href = url.toString();
            return;
        }

        updateParams(paginationId, { pageSize: pageSize });

        if (config.ajaxUrl && config.containerId) {
            loadPageViaAjax(paginationId, 1, config);
        } else {
            const url = new URL(window.location);
            url.searchParams.set('pageSize', pageSize);
            url.searchParams.set('page', 1);
            window.location.href = url.toString();
        }
    }

    /**
     * Load page content via AJAX
     * @param {string} paginationId - The unique pagination ID
     * @param {number} page - The page number
     * @param {object} config - Pagination configuration
     */
    function loadPageViaAjax(paginationId, page, config) {
        const container = document.getElementById(config.containerId);

        if (!container) {
            console.error(`Container not found: ${config.containerId}`);
            return;
        }

        showLoading(container, config.loadingText);

        const url = config.ajaxUrl;
        const method = config.method.toUpperCase();

        let params = { page: page };

        if (config.additionalParams) {
            params = { ...params, ...config.additionalParams };
        }

        if (config.getAdditionalParams && typeof config.getAdditionalParams === 'function') {
            const dynamicParams = config.getAdditionalParams();
            if (dynamicParams) {
                params = { ...params, ...dynamicParams };
            }
        }

        let fetchOptions = {
            method: method,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8'
            }
        };

        let finalUrl = url;

        if (method === 'GET') {
            const urlObj = new URL(url, window.location.origin);
            Object.keys(params).forEach(key => {
                if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
                    urlObj.searchParams.set(key, params[key]);
                }
            });
            finalUrl = urlObj.toString();
        } else if (method === 'POST') {
            const formData = new FormData();
            Object.keys(params).forEach(key => {
                if (params[key] !== null && params[key] !== undefined) {
                    if (Array.isArray(params[key])) {
                        params[key].forEach(value => {
                            formData.append(key, value);
                        });
                    } else {
                        formData.append(key, params[key]);
                    }
                }
            });

            if (config.csrfToken) {
                formData.append('__RequestVerificationToken', config.csrfToken);
            }

            fetchOptions.body = formData;
        }

        fetch(finalUrl, fetchOptions)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.text();
            })
            .then(html => {
                hideLoading(container);
                container.innerHTML = html;

                const event = new CustomEvent('paginationPageLoaded', {
                    detail: { paginationId, page, container, params }
                });
                document.dispatchEvent(event);
            })
            .catch(error => {
                hideLoading(container);
                console.error('Error while loading the page:', error);

                if (config.onError) {
                    config.onError(error);
                } else {
                    showError(container, config.errorText);
                }
            });
    }

    /**
     * Update additional parameters for a pagination instance
     * @param {string} paginationId - The unique pagination ID
     * @param {object} newParams - New parameters to merge
     */
    function updateParams(paginationId, newParams) {
        const config = paginationConfigs.get(paginationId);
        if (!config) {
            console.error(`Configuration de pagination introuvable pour l'ID : ${paginationId}`);
            return;
        }

        config.additionalParams = { ...config.additionalParams, ...newParams };
    }

    /**
     * Show the loading overlay
     * @param {HTMLElement} container - Container element
     * @param {string} loadingText - Loading message
     */
    function showLoading(container, loadingText) {
        const loadingOverlay = document.createElement('div');
        loadingOverlay.className = 'pagination-loading';
        loadingOverlay.innerHTML = `
            <div class="d-flex justify-content-center align-items-center h-100">
                <div class="text-center">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">${loadingText}</span>
                    </div>
                    <div class="mt-2">${loadingText}</div>
                </div>
            </div>
        `;

        container.style.position = 'relative';
        container.appendChild(loadingOverlay);
    }

    /**
     * Hide the loading overlay
     * @param {HTMLElement} container - Container element
     */
    function hideLoading(container) {
        const loadingOverlay = container.querySelector('.pagination-loading');
        if (loadingOverlay) {
            loadingOverlay.remove();
        }
    }

    /**
     * Show an error message
     * @param {HTMLElement} container - Container element
     * @param {string} errorText - Error message
     */
    function showError(container, errorText) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger d-flex align-items-center';
        errorDiv.innerHTML = `
            <i class="bi bi-exclamation-triangle-fill me-2"></i>
            <div>${errorText}</div>
        `;

        const existingAlert = container.querySelector('.alert');
        if (existingAlert) {
            existingAlert.remove();
        }

        container.insertBefore(errorDiv, container.firstChild);

        setTimeout(() => {
            if (errorDiv.parentNode) {
                errorDiv.remove();
            }
        }, 5000);
    }

    /**
     * Add CSS styles for the loading overlay.
     */
    function addLoadingStyles() {
        if (document.getElementById('pagination-styles')) {
            return;
        }

        const style = document.createElement('style');
        style.id = 'pagination-styles';
        style.textContent = `
            .pagination-loading {
                position: absolute;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background-color: rgba(255, 255, 255, 0.9);
                z-index: 1000;
                display: flex;
                align-items: center;
                justify-content: center;
                min-height: 100px;
                border-radius: 0.375rem;
                backdrop-filter: blur(2px);
            }
            
            .pagination-loading .spinner-border {
                width: 1.5rem;
                height: 1.5rem;
                border-width: 0.2em;
            }
        `;

        document.head.appendChild(style);
    }

    /**
     * Get the current page number for a pagination
     * @param {string} paginationId - The unique pagination ID
     * @returns {number} Current page number
     */
    function getCurrentPage(paginationId) {
        const pagination = document.getElementById(paginationId);
        if (!pagination) {
            return 1;
        }

        const activePage = pagination.querySelector('.page-item.active .page-link');
        return activePage ? parseInt(activePage.textContent.trim()) : 1;
    }

    /**
     * Update the pagination display without reloading
     * @param {string} paginationId - The unique pagination ID
     * @param {number} currentPage - New current page
     * @param {number} totalPages - New total number of pages
     */
    function updatePagination(paginationId, currentPage, totalPages) {
        const pagination = document.getElementById(paginationId);
        if (!pagination) {
            return;
        }

        // This would require more complex logic to update the pagination
        // For simplicity, reloading the page or replacing content via AJAX is recommended
        console.log(`Updating pagination ${paginationId} to page ${currentPage} of ${totalPages}`);
    }

    // Public API
    return {
        init: init,
        changePage: changePage,
        changePageSize: changePageSize,
        getCurrentPage: getCurrentPage,
        updatePagination: updatePagination,
        updateParams: updateParams
    };
})();
