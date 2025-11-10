let phoneNumber = '';
$(document).ready(function () {
    var phoneText = $('#driverPhoneNumber').text().trim();
    phoneNumber = (phoneText === "N/A" ? "" : phoneText);
});
setInterval(updateOrderStatuses, 10000);
var voucherCode = window.voucherCode;
function updateOrderStatuses() {
    const matchedOrder = data.find(order => order.voucherCode === voucherCode);
    if (matchedOrder) {
        $("#orderStatus").text(matchedOrder.status);
        $("#sosReason").text(matchedOrder.status === 'sos' && matchedOrder.sosReason ? matchedOrder.sosReason : '');
        $("#driverPhoneNumber").text(matchedOrder.assignedDriverPhoneNumber?.trim() || "N/A");
        phoneNumber = matchedOrder.assignedDriverPhoneNumber;
        const isDriverPhoneInvalid = !phoneNumber || phoneNumber.length < 10;
        $("#driverPhoneWarning").toggleClass("d-none", !isDriverPhoneInvalid);
        $("#trackDriverLink")
            .toggleClass("d-none", isDriverPhoneInvalid)
            .attr("href", isDriverPhoneInvalid ? "#" : `/driverdetail/${phoneNumber}`);

    }
    if (matchedOrder && matchedOrder.activities?.actualArrival) {
        const formatted = moment(matchedOrder.activities?.actualArrival)
            .format("ddd, MMMM DD - hh:mm:ss A"); // Matches C# format

        $("#actualArrival").text(formatted);
        $("#actualArrival").closest('div').removeClass('d-none');
    }
    if (matchedOrder && matchedOrder.status === 'sos') {
        $("#changeOrderStatus").removeClass('d-none');
    }
    if (matchedOrder && matchedOrder.status !== 'sos') {
        $("#changeOrderStatus").addClass('d-none');
    }
    // Update photo attachment link
    const $attachmentLink = $("#sosAttachment");
    if (matchedOrder.photoAttachment) {
            $attachmentLink
            .attr("href", "#")
            .attr("data-img", matchedOrder.photoAttachment)
            .removeClass("d-none");
    } else {
        $attachmentLink.addClass("d-none");
    }
}
function showModalAlert(message, containerId, type = "warning", timeout = 4000) {
    const alertContainer = document.getElementById(containerId);
    if (!alertContainer) return;

    // Clear any existing alerts
    alertContainer.innerHTML = "";

    const alertDiv = document.createElement("div");
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.role = "alert";
    alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
          `;

    alertContainer.appendChild(alertDiv);

    setTimeout(() => {
        // Trigger Bootstrap fade out and remove alert
        alertDiv.classList.remove("show");
        alertDiv.classList.add("hide");
        setTimeout(() => alertDiv.remove(), 150);
    }, timeout);
}

document.querySelectorAll('input[name="recipient"]').forEach((radio) => {
    radio.addEventListener('click', async () => {
        const form = document.getElementById('alertForm');
        const sendButton = document.getElementById('sendAlertBtn');
        const deviceIdInput = document.getElementById('deviceId');

        // Show form and enable send
        form.style.display = 'block';
        sendButton.disabled = false;

        if (radio.value === 'customer') {
            // Use Razor to fill in customer's device ID
            deviceIdInput.value = window.customerDeviceId;
        } else if (radio.value === 'driver') {

            if (!phoneNumber || phoneNumber.trim() === '') {
                showModalAlert("No driver has been assigned to this order yet.", "sendAlertContainer");
                form.style.display = 'none';
                sendButton.disabled = true;
                return;
            }

            try {
                // Call API to get driver device ID
                const response = await fetch(`/getDeviceID/${encodeURIComponent(phoneNumber)}`);

                if (!response.ok) {
                    showModalAlert(`Failed to fetch driver info`, "sendAlertContainer");
                    form.style.display = 'none';
                    sendButton.disabled = true;
                    return;
                }

                const data = await response.json();
                deviceIdInput.value = data.deviceId || '';

                if (!data.deviceId) {
                    showModalAlert("Driver device ID not found.", "sendAlertContainer");
                    form.style.display = 'none';
                    sendButton.disabled = true;
                }
            } catch (error) {
                showModalAlert('Error fetching driver info: ' + error.message, "sendAlertContainer");
                form.style.display = 'none';
                sendButton.disabled = true;
            }
        }
    });
});


document.getElementById('sendAlertBtn').addEventListener('click', async () => {
    const recipient = document.querySelector('input[name="recipient"]:checked')?.value;
    const deviceId = document.getElementById('deviceId').value;
    const title = document.getElementById('title').value.trim();
    const body = document.getElementById('body').value.trim();

    if (!recipient || !title || !body) {
        showModalAlert("Please fill in all fields.", "sendAlertContainer");
        return;
    }

    const payload = {
        id: deviceId,
        title: title,
        body: body
    };

    try {
        const response = await fetch('/sendAlertMessage', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errorText = await response.text();
            showModalAlert('Failed to send alert: ', "sendAlertContainer");
        } else {
            const result = await response.text();
            console.log('Alert sent successfully:', result);
            showModalAlert('Alert sent successfully!', "sendAlertContainer");

            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('sendAlertModal'));
            modal.hide();
        }
    } catch (error) {
        showModalAlert('Error sending alert: ' + error.message, "sendAlertContainer");
    }
});


$('#supervisorAccept').on('click', function () {
    const $btn = $(this);
    $('#confirmSupervisorBtn').data({
        tin: $btn.data('tin'),
        voucherCode: $btn.data('voucher-code'),
        clientPhone: $btn.data('client-phone'),
        driverPhone: $btn.data('driver-phone')
    });


    const modal = new bootstrap.Modal(document.getElementById('supervisorConfirmModal'));
    modal.show();
});

$('#confirmSupervisorBtn').on('click', async function () {
    const $confirmBtn = $(this);
    $confirmBtn.prop('disabled', true).text('Processing...');

    const payload = {
        voucherCode: $confirmBtn.data('voucherCode'),
        companyTin: String($confirmBtn.data('tin')),
        assignedDriverPhoneNumber: $confirmBtn.data('driverPhone'),
        customerPhoneNumber: $confirmBtn.data('clientPhone')
    };
    try {
        const response = await fetch('/supervisoraccept', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            showModalAlert('Error: Could not accept order supervision.', "accetOrderAlertContainer");
            $confirmBtn.prop('disabled', false).text('Confirm');
        } else {
            showModalAlert('Order supervision accepted successfully!', "accetOrderAlertContainer", "success");
            setTimeout(() => window.location.reload(), 5000);
        }
    } catch (error) {
        showModalAlert('Error:' + error.message, "accetOrderAlertContainer");
        $confirmBtn.prop('disabled', false).text('Confirm');
    }
});
function loadAvailableDrivers() {
    const $select = $("#driverSelect");
    $select.html(`<option value="" selected disabled>Loading drivers...</option>`);

    $.ajax({
        url: "/driver/getDrivers",
        method: "GET",
        success: function (drivers) {
            if (!drivers || drivers.length === 0) {
                $select.html(`<option disabled>No available drivers found</option>`);
                return;
            }
            drivers.sort((a, b) => a.firstName.localeCompare(b.firstName));
            $select.empty().append(`<option value="" selected disabled>Select a driver</option>`);
            drivers.forEach(driver => {
                const option = `<option value="${driver.phoneNumber}">${driver.firstName} (${driver.phoneNumber}) (${driver.status})</option>`;
                $select.append(option);
            });
        },
        error: function () {
            $select.html(`<option disabled>Error loading drivers</option>`);
        }
    });
}
$('#changeOrderStatus').on('click', async function () {
    console.log("changeOrderStatus")
    const voucherCode = $(this).data('voucher-code');
    $('#voucherCode').text(voucherCode);
    $('#voucherCodeInput').val(voucherCode);
    $('#statusSelect').val('');
    $('#orderStatusButton').prop('disabled', true);
    const modal = new bootstrap.Modal($('#changeStatusModal'));
    modal.show();
    loadAvailableDrivers();
});

$('#driverSelect, #statusSelect').on('change', function () {
    const hasDriver = $('#driverSelect').val();
    const hasStatus = $('#statusSelect').val();
    $('#orderStatusButton').prop('disabled', !(hasDriver && hasStatus));
});

$('#changeOrderStatusForm').on('submit', function (e) {
    e.preventDefault(); // stop normal form submission
    $('#orderStatusButton').text('Confirming...');
    $('#orderStatusButton').prop('disabled', true);
    const voucherCode = $('#voucherCodeInput').val();
    const status = $('#statusSelect').val();
    const assignedDriverPhoneNumber = $('#driverSelect').val();

    // ✅ Validation
    if (!orderStatus) {
        showModalAlert("Please select an order status.", "changeOrderStatusAlertContainer", "warning");
        return;
    }
    if (!assignedDriverPhoneNumber) {
        showModalAlert("Please select a driver.", "changeOrderStatusAlertContainer", "warning");
        return;
    }

    const data = {
        voucherCode,
        status,
        assignedDriverPhoneNumber
    };

    $.ajax({
        url: '/changeorderstatus',  // adjust to your backend endpoint
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            showModalAlert("Order status updated successfully!", "changeOrderStatusAlertContainer", "success");
            setTimeout(() => window.location.reload(), 5000);
        },
        error: function (xhr) {
            const errorMessage = xhr.responseText || "An error occurred while updating order status.";
            showModalAlert(errorMessage, "changeOrderStatusAlertContainer", "danger");
            $('#orderStatusButton').prop('disabled', false).text('Submit');
        }
    });
});
