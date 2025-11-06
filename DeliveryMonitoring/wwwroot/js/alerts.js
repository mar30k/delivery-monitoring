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
            return;
        }
        const newData = await response.json();
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
    }
    setTimeout(fetchAlerts, 10000);
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
