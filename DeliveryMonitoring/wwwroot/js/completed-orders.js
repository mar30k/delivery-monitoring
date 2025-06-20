var js = jQuery.noConflict(true);
var tablelist;
js( ()=> {
    tablelist = js('#tablelist').DataTable({
        responsive: true,
        "order": [[5, "desc"]],
        "pageLength": 15,
        columnDefs: [
            { orderable: false, targets: [0, 4, 9, 13, 12] } 
            //{
            //    targets: 5, // the column with the date
            //    type: 'html'
            //}
        ],
        language: {
            "emptyTable": "No orders history to display at the moment."
        }
    });
});

async function showDetailsModal(button) {
    const note = button.getAttribute('data-note');
    const purpose = button.getAttribute('data-purpose');
    const voucherCode = button.getAttribute('data-voucher-code');
    const phoneNumber = button.getAttribute('data-phone-number');

    // Reset modal fields
    document.getElementById('modalNote').textContent = note || '—';
    document.getElementById('modalPurpose').textContent = purpose || '—';
    document.getElementById('voucherCodeDetail').textContent = voucherCode ? `- ${voucherCode}` : '';
    document.getElementById('reviewDetailsLoadingSpinner').style.display = 'block';

    // Show the modal immediately
    const modal = new bootstrap.Modal(document.getElementById('reviewDetailsModal'));
    modal.show();

    const reviewSection = document.getElementById('customerReview'); // new div for this modal
    reviewSection.innerHTML = '';
    try {
        const foundReview = await fetchDriverReview(phoneNumber, voucherCode);
        if (foundReview) {
            reviewSection.innerHTML = ` 
                ${renderReview(foundReview)}
            `;

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
            <div class="alert alert-danger" role="alert">Failed to load review.</div>
        `;
        reviewSection.classList.remove('d-none');
    } finally {
        document.getElementById('reviewDetailsLoadingSpinner').style.display = 'none';
    }
}

async function showReviewModal(button) {
    const note = button.getAttribute('data-note');
    const voucherCode = button.getAttribute('data-voucher-code');
    const phoneNumber = button.getAttribute('data-phone-number');

    document.getElementById('reviewOrderId').value = voucherCode;
    document.getElementById('reviewPurpose').value = '';
    document.getElementById('reviewNote').value = note || '';
    document.getElementById('voucherCodeReview').textContent = voucherCode ? `- ${voucherCode}` : '';

    const modal = new bootstrap.Modal(document.getElementById('reviewModal'));
    modal.show();
    document.getElementById('reviewLoadingSpinner').style.display = 'block';

    const reviewSection = document.getElementById('customerReviewSection'); // new div for this modal
    reviewSection.innerHTML = '';
    try {
        const foundReview = await fetchDriverReview(phoneNumber, voucherCode);
        if (foundReview) {
            reviewSection.innerHTML = ` 
                ${renderReview(foundReview)}
            `;
        } else {
            reviewSection.innerHTML = `
                <div class="alert alert-warning mb-0" role="alert">
                    No customer review found for this order.
                </div>
            `;
        }
    } catch (err) {
        reviewSection.innerHTML = `<div class="alert alert-danger mt-3">Failed to fetch customer review.</div>`;
    } finally {
        document.getElementById('reviewLoadingSpinner').style.display = 'none';
    }
}

function renderReview(foundReview) {
    return `<div class="card shadow-sm border-0 mb-3">
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
}
function renderStars(rating) {
    rating = Math.floor(rating);
    return '<i class="bi bi-star-fill text-warning"></i>'.repeat(rating) +
        '<i class="bi bi-star text-warning"></i>'.repeat(5 - rating);
}


async function fetchDriverReview(phoneNumber, voucherCode) {
    let page = 1;
    let foundReview = null;

    while (true) {
        const response = await fetch(`/Driver/fetchReview?phoneNumber=${encodeURIComponent(phoneNumber)}&page=${page}`);
        if (!response.ok) break;

        const result = await response.json();
        if (!result || !result.reviews || result.reviews.length === 0) break;
        foundReview = result.reviews.find(r => r.referenceVoucher === voucherCode);
        if (foundReview) break;

        page++;
    }

    return foundReview;
}
document.getElementById('reviewForm').addEventListener('submit', async function (e) {
    e.preventDefault(); 

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
    }
});


async function showActivity(button) {
    const voucherCode = button.getAttribute('data-voucher');
    const companyCode = button.getAttribute('data-company-code');
    const url = `/CompletedOrders/getDeliveryActivity?voucherCode=${encodeURIComponent(voucherCode)}&companyCode=${encodeURIComponent(companyCode)}`;

    try {
        const response = await fetch(url);
        const result = await response.json();

        if (result.isSuccessful) {
            const data = result.data;

            // Fill Summary Section
            document.getElementById('activitySummary').innerHTML = `
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

            // Fill Activity Table
            const timeline = document.getElementById('activityTimeline');
            timeline.innerHTML = ''; // clear previous rows

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

            const existingModalEl = document.getElementById('activityModal');
            const existingModal = bootstrap.Modal.getInstance(existingModalEl);
            if (existingModal) {
                existingModal.hide(); // prevent aria-hidden issues
            }

            const modal = new bootstrap.Modal(existingModalEl);
            modal.show();
        } else {
            showAlert("Failed to fetch activity.", "warning");
        }
    } catch (err) {
        console.error(err);
        showAlert("An error occurred while loading activity.", "danger");
    }
}







function showAlert(message, type = 'danger') {
    const container = document.getElementById('alertContainer');
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    container.innerHTML = alertHtml;
}



setInterval(fetchCompletedOrders, 30000); // every 60 seconds

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

            const driverPhone = order.driverPhoneNumber
                ? `<a href="tel:${order.driverPhoneNumber}">${order.driverPhoneNumber}</a>`
                : "N/A";

            const reviewButton = order.purpose
                ? `<button class="btn btn-outline-secondary btn-sm"
                            data-note="${order.note ?? ''}"
                            data-purpose="${order.purpose ?? ''}"
                            data-voucher-code="${order.voucherCode ?? ''}"
                            data-phone-number="${order.driverPhoneNumber ?? ''}"
                            onclick="showDetailsModal(this)">Show</button>`
                : `<button class="btn btn-outline-secondary btn-sm"
                            data-voucher-code="${order.voucherCode ?? ''}"
                            data-phone-number="${order.driverPhoneNumber ?? ''}"
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
                <td>${order.voucherCode || 'N/A'}</td>
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
                <td class="text-center">${order.totalAmount ?? 'N/A'}</td>
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
