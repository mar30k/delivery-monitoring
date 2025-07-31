var js = jQuery.noConflict(true);
var tablelist;

js(() => {
    let startDate = moment().startOf('day');
    let endDate = moment().endOf('day');

    // Add DataTables custom filter logic BEFORE initializing the table
    js.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
        if (!startDate || !endDate) return true;

        var orderDateStr = data[5]; // column with Order DateTime
        var orderDate = moment(orderDateStr, "YYYY-MM-DD HH:mm:ss");

        return orderDate.isBetween(startDate, endDate, undefined, '[]');
    });

    // Temporarily unhide all rows so DataTables can register them
    js('#tablelist tbody tr.initial-hide').removeClass('initial-hide');
    // Initialize DataTable
    tablelist = js('#tablelist').DataTable({
        responsive: true,
        order: [[5, "desc"]],
        pageLength: 50,
        lengthMenu: [[10, 15, 25, 50, 100, -1], [10, 15, 25, 50, 100, "All"]],
        columnDefs: [
            { orderable: false, targets: [0, 4, 9, 13, 12] }
        ],
        language: {
            emptyTable: "No orders history to display at the moment."
        },
        footerCallback: function (row, data, start, end, display) {
            var api = this.api();
            var columnIndex = 11; // <-- Update this to match the TotalAmount column

            var parseValue = function (i) {
                const parsed = typeof i === 'string'
                    ? parseFloat(i.replace(/[^0-9.-]+/g, '')) || 0
                    : typeof i === 'number'
                        ? i
                        : 0;
                return parsed;
            };

            var columnData = api.column(columnIndex, { page: 'current' }).data();
            var pageTotal = columnData.reduce(function (a, b) {
                return parseValue(a) + parseValue(b);
            }, 0);
            js(api.column(columnIndex).footer()).html(
                pageTotal.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
            );
        }
    });

    // Initialize Date Range Picker with today's date preselected
    js('#dateRange').daterangepicker({
        startDate: startDate,
        endDate: endDate,
        maxDate: moment(),
        autoUpdateInput: true,
        locale: {
            cancelLabel: 'Clear',
            format: 'YYYY-MM-DD'
        }
    });

    // Set the input manually (because autoUpdateInput may not trigger redraw)
    js('#dateRange').val(startDate.format('YYYY-MM-DD') + ' to ' + endDate.format('YYYY-MM-DD'));

    // Initial draw to apply default today's filter
    tablelist.draw();

    // On apply
    js('#dateRange').on('apply.daterangepicker', function (ev, picker) {
        startDate = picker.startDate.startOf('day');
        endDate = picker.endDate.endOf('day');
        js(this).val(picker.startDate.format('YYYY-MM-DD') + ' to ' + picker.endDate.format('YYYY-MM-DD'));
        tablelist.draw();
    });

    // On clear
    js('#dateRange').on('cancel.daterangepicker', function (ev, picker) {
        startDate = null;
        endDate = null;
        js(this).val('');
        tablelist.draw();
    });
});
async function showDetailsModal(button) {
    const note = button.getAttribute('data-note').replace(/\n/g, '<br>');;
    const purpose = button.getAttribute('data-purpose');
    const voucherCode = button.getAttribute('data-voucher-code');
    const phoneNumber = button.getAttribute('data-phone-number');
    const customerPhone = button.getAttribute('data-customer-phone');
    const purposeKey = button.getAttribute('data-purpose-key');


    document.getElementById('modalNote').innerHTML = note || '—';
    document.getElementById('modalPurpose').textContent = purpose || '—';
    document.getElementById('voucherCodeDetail').textContent = voucherCode ? `- ${voucherCode}` : '';

    const editBtn = document.getElementById('editNote');
    editBtn.setAttribute('data-voucher-code', voucherCode);
    editBtn.setAttribute('data-phone-number', phoneNumber);
    editBtn.setAttribute('data-customer-phone', customerPhone);
    editBtn.setAttribute('data-purpose-key', purposeKey);
    editBtn.setAttribute('data-note', note);
    // Show the modal immediately
    const modal = new bootstrap.Modal(document.getElementById('reviewDetailsModal'));
    modal.show();
    showCustomerReview({
        phoneNumber,
        voucherCode,
        customerPhone,
        reviewSectionId: 'customerReview',
        spinnerId: 'reviewDetailsLoadingSpinner'
    });
}

async function showReviewModal(button) {
    const note = button.getAttribute('data-note');
    const voucherCode = button.getAttribute('data-voucher-code');
    const phoneNumber = button.getAttribute('data-phone-number');
    const customerPhone = button.getAttribute('data-customer-phone');
    const purposeKey = button.getAttribute('data-purpose-key');

    // Close any open modal before opening this one
    document.querySelectorAll('.modal.show').forEach(modalEl => {
        bootstrap.Modal.getInstance(modalEl)?.hide();
    });

    // Populate modal inputs
    document.getElementById('reviewOrderId').value = voucherCode;
    document.getElementById('reviewPurpose').value = purposeKey || '';
    document.getElementById('reviewNote').value = note || '';
    document.getElementById('voucherCodeReview').textContent = voucherCode ? `- ${voucherCode}` : '';

    const modal = new bootstrap.Modal(document.getElementById('reviewModal'));
    modal.show();

    showCustomerReview({
        phoneNumber,
        voucherCode,
        customerPhone,
        reviewSectionId: 'customerReviewSection',
        spinnerId: 'reviewLoadingSpinner'
    });
}
async function showCustomerReview({
    phoneNumber,
    voucherCode,
    customerPhone,
    reviewSectionId,
    spinnerId,
}) {
    const reviewSection = document.getElementById(reviewSectionId);
    reviewSection.innerHTML = '';

    const spinner = document.getElementById(spinnerId);
    spinner.style.display = 'block';
    const retryButton = `<button class="btn btn-sm btn-outline-secondary p-1"
                        onclick="showCustomerReview({
                            phoneNumber: &quot;${phoneNumber}&quot;,
                            voucherCode: &quot;${voucherCode}&quot;,
                            customerPhone: &quot;${customerPhone}&quot;,
                            reviewSectionId: &quot;${reviewSectionId}&quot;,
                            spinnerId: &quot;${spinnerId}&quot;
                        })" style="font-size: 12px;">
                        <i class="fas fa-rotate-right me-1"></i>Retry
                    </button>`;
    try {
        const foundReview = await fetchDriverReview(phoneNumber, voucherCode, customerPhone);
        if (foundReview) {
            reviewSection.innerHTML =
                `<div class="card shadow-sm border-0 mb-3">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <div class="d-flex">
                                <img src="${foundReview.image}" alt="${foundReview.fullName}" class="rounded-circle me-2" style="width: 40px; height: 40px; object-fit: cover;">
                                <div>
                                    <h6 class="mb-0 fw-bold">${foundReview.fullName}</h6>
                                    <small class="text-muted">${new Date(foundReview.date).toLocaleDateString()}</small>
                                </div>
                            </div>
                            <div class="text-end">
                                <span class="fw-semibold text-warning">
                                    ${renderStars(foundReview.rating)} 
                                    <span class="ms-1 text-dark">${foundReview.rating.toFixed(1)}</span>
                                </span>
                            </div>
                        </div>

                        ${foundReview.review ? `<p class="my-1"><strong class="text-muted">Review:</strong> ${foundReview.review}</p>` : ''}
                        ${foundReview.reply ? `<p class="my-1"><strong class="text-muted">Reply:</strong> ${foundReview.reply}</p>` : ''}
                    </div>
                </div>`;
        } else {
            reviewSection.innerHTML = `
                <div class="alert alert-warning mb-0" role="alert">
                    No customer review found for this order.
                </div>
            `;
        }

        reviewSection.classList.remove('d-none');
    } catch (err) {
        console.error('Error fetching review:', err);
        reviewSection.innerHTML = `
            <div class="alert alert-danger mt-3" role="alert">
                Failed to fetch customer review. 
                ${retryButton}
            </div>
        `;
        reviewSection.classList.remove('d-none');
    } finally {
        if (spinner) spinner.style.display = 'none';
    }
}

function renderStars(rating) {
    rating = Math.floor(rating);
    return '<i class="bi bi-star-fill text-warning"></i>'.repeat(rating) +
        '<i class="bi bi-star text-warning"></i>'.repeat(5 - rating);
}

document.getElementById('editNote').addEventListener('click', function (e) {
    e.preventDefault();

    // Close the reviewDetailsModal
    const reviewDetailsModalEl = document.getElementById('reviewDetailsModal');
    const reviewDetailsModal = bootstrap.Modal.getInstance(reviewDetailsModalEl) || new bootstrap.Modal(reviewDetailsModalEl);
    reviewDetailsModal.hide();

    showReviewModal(this);
});

async function fetchDriverReview(phoneNumber, voucherCode, customerPhone) {
    const page = 1;
    try {
        const response = await fetch(`/Driver/fetchReview?phoneNumber=${encodeURIComponent(phoneNumber)}&page=${page}`);
        if (!response.ok) throw new Error(`Failed to fetch review: ${response.status}`);
        const result = await response.json();
        if (!result || !result.reviews || result.reviews.length === 0) return null;
        return result.reviews.find(r=> r.referenceVoucher === voucherCode && r.reviewerPhoneNumber == customerPhone) || null;

    } catch (error) {
        console.error("Error fetching driver review:", error);
        throw error;
    }
}
document.getElementById('reviewForm').addEventListener('submit', async function (e) {
    e.preventDefault(); 

    const submitBtn = document.getElementById('submitReviewBtn');
    submitBtn.disabled = true;
    const originalText = submitBtn.textContent;
    submitBtn.textContent = 'Submitting...';

    const voucherCode = document.getElementById('reviewOrderId').value;
    const purpose = document.getElementById('reviewPurpose').value;
    const note = document.getElementById('reviewNote').value || '';
    try {
        const response = await fetch('/CompletedOrders/savenote', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ voucherCode, purpose, note })
        });

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('reviewModal')).hide();
            alert("Submitted successfully.");
            location.reload();
        } else {
            const error = await response.text();
            alert("Submission failed: " + error);
        }
    } catch (err) {
        console.error(err);
        alert("An error occurred while submitting.");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
    }
});


async function showActivity(button) {
    const voucherCode = button.getAttribute('data-voucher');
    const companyCode = button.getAttribute('data-company-code');
    const url = `/CompletedOrders/getDeliveryActivity?voucherCode=${encodeURIComponent(voucherCode)}&companyCode=${encodeURIComponent(companyCode)}`;

    // Get modal elements
    const modalEl = document.getElementById('activityModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

    const loader = document.getElementById('activityLoader');
    const alertContainer = document.getElementById('activityAlertContainer');
    const summary = document.getElementById('activitySummary');
    const timeline = document.getElementById('activityTimeline');
    const table = document.getElementById('activityTable');
    const title = document.getElementById('activityModalLabel');

    // Show modal and reset content
    title.innerText = `Delivery Activity - ${voucherCode}`;
    loader.classList.remove('d-none');
    alertContainer.innerHTML = '';
    summary.innerHTML = '';
    summary.classList.add('d-none');
    timeline.innerHTML = '';
    table.classList.add('d-none');
    modal.show();

    try {
        const response = await fetch(url);
        const result = await response.json();

        if (result.isSuccessful) {
            const data = result.data;

            summary.innerHTML = `
                <div class="row">
                    <div class="col-md-6">
                        <p><strong>Start Time:</strong> ${new Date(data.startTime).toLocaleString()}</p>
                        <p><strong>ETA:</strong> ${new Date(data.eta).toLocaleString()}</p>
                    </div>
                    <div class="col-md-6">
                        <p><strong>Actual Arrival:</strong> ${new Date(data.actualArrival).toLocaleString()}</p>
                        <p><strong>Current Time:</strong> ${new Date(data.currentTime).toLocaleString()}</p>
                    </div>
                </div>
            `;
            summary.classList.remove('d-none');

            data.activityResponse.forEach((activity, index) => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${index + 1}</td>
                    <td>${activity.name}</td>
                    <td>${new Date(activity.time).toLocaleTimeString()}</td>
                    <td>${activity.timeElapsed}</td>
                `;
                timeline.appendChild(row);
            });

            table.classList.remove('d-none');
        } else {
            showActivityAlert("Failed to fetch delivery activity.", "warning");
        }
    } catch (err) {
        console.error(err);
        showActivityAlert("An error occurred while loading delivery activity.", "danger");
    } finally {
        loader.classList.add('d-none');
    }
}

function showActivityAlert(message, type = 'danger') {
    const alertContainer = document.getElementById('activityAlertContainer');
    if (!alertContainer) return;

    alertContainer.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
}



setInterval(fetchCompletedOrders, 30000); // every 30 seconds

async function fetchCompletedOrders() {
    try {
        const response = await fetch('/completedOrders/getcompletedorders');
        if (!response.ok) {
            console.error("Failed to fetch completed orders");
            return;
        }

        const result = await response.json();
        const data = result.data || [];
        let currentPage = tablelist.page.info().page;
        const tbody = document.querySelector('#tablelist tbody');
        tbody.innerHTML = ''; // Clear old rows
        data.forEach(order => {
            const requestCreatedAt = order.requestCreatedAtString || "N/A";
            const purposeKey = Object.entries(purposeOptions).find(([key, val]) => val === order.purpose)?.[0];
            const driverPhone = order.driverPhoneNumber
                ? `<a href="tel:${order.driverPhoneNumber}">${order.driverPhoneNumber}</a>`
                : "N/A";

            const reviewButton = order.purpose
                ? `<button class="btn btn-outline-secondary btn-sm"
                            data-note="${order.note ?? ''}"
                            data-purpose="${order.purpose ?? ''}"
                            data-purpose-key="${purposeKey}"
                            data-voucher-code="${order.voucherCode ?? ''}"
                            data-customer-phone="${order.phoneNumber ?? ''}"
                            data-phone-number="${order.driverPhoneNumber ?? ''}"
                            onclick="showDetailsModal(this)">Show</button>`
                : `<button class="btn btn-outline-secondary btn-sm"
                            data-voucher-code="${order.voucherCode ?? ''}"
                            data-phone-number="${order.driverPhoneNumber ?? ''}"
                            data-customer-phone="${order.phoneNumber ?? ''}"
                            onclick="showReviewModal(this)">Review</button>`;

            const activityButton = `<button class="btn btn-outline-secondary activityBtn btn-sm"
                                      data-voucher="${order.voucherCode ?? ''}"
                                      data-company-code="${order.companyCode}"
                                      onclick="showActivity(this)"
                                      >
                                      Show
                                    </button>`;

            let row = document.createElement('tr');
            row.setAttribute('data-voucher', order.voucherCode);
            row.style.fontSize = "13px";

            row.innerHTML = `
                <td class="text-center">${order.voucherCode || 'N/A'}</td>
                <td class="text-center">${order.companyName || 'N/A'}</td>
                <td class="text-center">${order.branchName || 'N/A'}</td>
                <td class="text-center">${order.firstName || 'N/A'}</td>
                <td class="text-center"><a href="tel:${order.phoneNumber}">${order.phoneNumber || 'N/A'}</a></td>
                <td data-order="${order.requestCreatedAt}" data-iso="${requestCreatedAt}" class="text-center">${requestCreatedAt}</td>
                <td data-order="${order.distance}" class="text-center">${order.distance ?? 'N/A'} K.M</td>
                <td data-order="${order.duration}" class="text-center">${order.duration ?? 'N/A'} Min</td>
                <td data-order="${order.eta}" class="text-center">${order.eta ?? 'N/A'} Min</td>
                <td class="driver-cell text-center">${driverPhone}</td>
                <td class="text-center">${order.supervisorName || 'N/A'}</td>
                <td class="text-center">
                  ${order.totalAmount?.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) ?? 'N/A'}
                </td>
                <td class="text-center">${reviewButton}</td>
                <td class="text-center">${activityButton}</td>
            `;

            tbody.appendChild(row);
        });

        // Reinitialize DataTable without destroying it
        tablelist.clear();
        tablelist.rows.add(js('#tablelist tbody tr'));  // Add the newly updated rows
        // Redraw the table and retain the current page
        tablelist.draw();
        tablelist.page(currentPage).draw(false);
    } catch (err) {
        console.error("Error rendering completed orders:", err);
    }
}
