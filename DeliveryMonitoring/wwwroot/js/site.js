$(() => {
    $('#logoutButton').on('click', () => {
        window.location.href = '/Login/Logout';
    });

    closeAlert();
    fetchAlerts();
});

var data = {};

async function fetchAlerts() {
    try {
        const response = await fetch('/getorders');
        if (!response.ok) {
            console.error("Server returned error:", response.status, response.statusText);
            return;
        }
        let newData;
        try {
            newData = await response.json();
        } catch (jsonError) {
            console.error("Failed to parse JSON:", jsonError);
            return;
        }

        // Only update 'data' if JSON parsing succeeded
        data = newData;

        const orderCount = document.getElementById("orderCount");
        if (orderCount) {
            orderCount.textContent = data.length;
        }
        const alertBox = document.getElementById("floating-alert");
        const alertList = document.getElementById("alert-list");

        // Retrieve previously displayed alerts from sessionStorage
        let storedAlerts = JSON.parse(sessionStorage.getItem("displayedAlerts")) || {};

        // Filter orders that have non-empty alerts
        const filteredOrders = data.filter(order => order.alert && order.alert.trim() !== "");

        // Track alerts that need to be displayed in this refresh
        let newAlerts = {};
        let hasNewAlerts = false;
        filteredOrders.forEach(order => {
            const { voucherCode, alert } = order;
            // Only add if it's a completely new alert or the alert message has changed
            if (!storedAlerts[voucherCode] || storedAlerts[voucherCode] !== alert) {
                hasNewAlerts = true;
                newAlerts[voucherCode] = alert;
            }
        });

        // If there are new alerts, update the UI
        if (hasNewAlerts) {
            alertList.innerHTML = ""; // Clear alert container

            for (const voucherCode in newAlerts) {
                const listItem = document.createElement("li");
                listItem.innerHTML = `<a class="alerts" style="text-decoration: none;" target="_blank" href="/order/${voucherCode}">Order ${voucherCode}: ${newAlerts[voucherCode]}</a>`;
                alertList.appendChild(listItem);
            }

            // Show the floating alert box only if there are new alerts
            alertBox.style.display = "block";

            // Update sessionStorage to track displayed alerts
            sessionStorage.setItem("displayedAlerts", JSON.stringify({ ...storedAlerts, ...newAlerts }));
        }

    } catch (error) {
        console.error("Error fetching alerts:", error);
    } finally {
        setTimeout(fetchAlerts, 10000); // schedule next only after current finishes
    }
}

let alertAudio = null;

function playAlertSound(times = 4) {
    if (alertAudio) {
        alertAudio.pause(); // Stop any currently playing audio
        alertAudio.currentTime = 0;
    }

    alertAudio = new Audio('/images/alarm2.mp3');
    let count = 0;

    alertAudio.addEventListener("ended", function () {
        count++;
        if (count < times) {
            alertAudio.currentTime = 0;
            alertAudio.play();
        }
    });

    alertAudio.play().catch(error => console.error("Audio play failed:", error));
}

function closeAlert() {
    const alertBox = document.getElementById("floating-alert");
    if (alertBox) {
        alertBox.style.display = "none";
    }

    // Stop the audio
    if (alertAudio) {
        alertAudio.pause();
        alertAudio.currentTime = 0;
    }
}
function showToast(message, type = 'info', iconClass = '', delay = 3000) {
    const container = document.querySelector('.toast-container');
    if (!container) return;

    // 🔹 Determine color based on type
    const bgColor = {
        success: 'bg-success text-white',
        error: 'bg-danger text-white',
        warning: 'bg-warning text-dark',
        info: 'bg-primary text-white'
    }[type] || 'bg-primary text-white';

    // 🔹 Create toast element
    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center border-0 shadow ${bgColor}`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');

    toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2">
                    ${iconClass ? `<i class="${iconClass} fs-5"></i>` : ''}
                    <span>${message}</span>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

    container.appendChild(toastEl);

    const toast = new bootstrap.Toast(toastEl, { delay });
    toast.show();

    // Remove from DOM after hidden
    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
}

// Example: clipboard copy using reusable toast
function copyToClipboard(text) {
    if (!text || text === "null") return;
    navigator.clipboard.writeText(text)
        .then(() => showToast("Phone number copied!", "success", "bi bi-clipboard-check-fill"))
        .catch(err => showToast("Failed to copy: " + err, "error", "bi bi-x-circle-fill"));
}

// Example: attach to copy buttons
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.copy-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const phone = this.getAttribute('data-phone');
            copyToClipboard(phone);
        });
    });
});