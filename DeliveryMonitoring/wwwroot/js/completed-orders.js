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