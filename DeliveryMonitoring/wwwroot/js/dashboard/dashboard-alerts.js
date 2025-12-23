window.DashboardAlerts = (function () {

    const seen = new Set();
    let firstFetch = true;

    function processOrders(orders) {
        orders.forEach(o => {
            if (!seen.has(o.voucherCode)) {
                if (!firstFetch) showToast(o);
                seen.add(o.voucherCode);
            }
        });
        firstFetch = false;
    }

    function showToast(order) {
        const toastHTML = `
            <div class="toast align-items-center text-white bg-primary border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                            New Order: ${order.voucherCode} <br> Customer: ${order.customerFirstName || 'Unknown'} <br> Company: ${order.companyName || 'Unknown'} <br>
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>`;
        const toastContainer = document.getElementById('toastContainer');
        toastContainer.insertAdjacentHTML('beforeend', toastHTML);

        const toastEl = toastContainer.lastElementChild;
        const toast = new bootstrap.Toast(toastEl, { delay: 45000 });
        toast.show();

        // Attach click event to close button to hide toast
        const closeButton = toastEl.querySelector('.btn-close');
        console.log(closeButton);
        closeButton.addEventListener('click', () => toast.hide());    }

    return { processOrders };
})();
