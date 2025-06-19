var js = jQuery.noConflict(true);
var tablelist;
js(document).ready(function () {
    tablelist = js('#tablelist').DataTable({
        responsive: true,
        "order": [[5, "desc"]],
        "pageLength": 15,
        columnDefs: [
            { orderable: false, targets: [0, 4, 9, 11] } 
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

function showDetailsModal(button) {
    const note = button.getAttribute('data-note');
    const purpose = button.getAttribute('data-purpose');
    document.getElementById('modalNote').textContent = note || '—';
    document.getElementById('modalPurpose').textContent = purpose || '—';

    const modal = new bootstrap.Modal(document.getElementById('detailsModal'));
    modal.show();
}

function showReviewModal(button) {
    const note = button.getAttribute('data-note');
    const orderId = button.getAttribute('data-order-id');

    document.getElementById('reviewOrderId').value = orderId;
    document.getElementById('reviewPurpose').value = ''; // blank by default
    document.getElementById('reviewNote').value = note || '';

    const modal = new bootstrap.Modal(document.getElementById('reviewModal'));
    modal.show();
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

