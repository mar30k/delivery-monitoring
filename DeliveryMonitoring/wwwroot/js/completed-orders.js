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
                <div class="alert alert-secondary">
                    <h6 class="mb-1">${foundReview.fullName} <small class="text-muted float-end">${new Date(foundReview.date).toLocaleDateString()}</small></h6>
                    <p class="mb-1">${foundReview.review}</p>
                    <div class="text-warning">Rating: ${foundReview.rating.toFixed(1)} ⭐</div>
                    ${foundReview.reply ? `<div class="alert alert-light mt-2"><strong>Reply:</strong> ${foundReview.reply}</div>` : ''}
                </div>
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
                <div class="alert alert-info mt-3">
                    <strong>${foundReview.fullName}</strong> rated this ${foundReview.rating.toFixed(1)} ⭐<br/>
                    "${foundReview.review}"<br/>
                    ${foundReview.reply ? `<div class="text-muted mt-2">Reply: ${foundReview.reply}</div>` : ''}
                </div>
            `;
        } else {
            reviewSection.innerHTML = `<div class="alert alert-warning mt-3">No customer review found for this order.</div>`;
        }
    } catch (err) {
        reviewSection.innerHTML = `<div class="alert alert-danger mt-3">Failed to fetch customer review.</div>`;
    } finally {
        document.getElementById('reviewLoadingSpinner').style.display = 'none';
    }
}



async function fetchDriverReview(phoneNumber, voucherCode) {
    let page = 1;
    let foundReview = null;

    while (true) {
        const response = await fetch(`/Driver/fetchReview?phoneNumber=${encodeURIComponent(phoneNumber)}&page=${page}`);
        if (!response.ok) break;

        const result = await response.json();
        if (!result || !result.reviews || result.reviews.length === 0) break;
        result.reviews.forEach(review => {
            console.log(review.voucherCode)
        });
        foundReview = result.reviews.find(r => r.voucherCode === voucherCode);
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

var activityButtons = document.getElementsByClassName('activityBtn');
Array.from(activityButtons).forEach(button => {
    button.addEventListener('click', async function (e) {
        e.preventDefault();

        const voucherCode = e.target.getAttribute('data-voucher');
        const companyCode = e.target.getAttribute('data-company-code');

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

                // Show Modal (Bootstrap 5)
                const modal = new bootstrap.Modal(document.getElementById('activityModal'));
                modal.show();
            } else {
                showAlert("Failed to fetch activity.", "warning");
            }
        } catch (err) {
            console.error(err);
            showAlert("An error occurred while loading activity.", "danger");
        }
    });
});





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

