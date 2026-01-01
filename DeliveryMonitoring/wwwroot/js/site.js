$(() => {
    $('#logoutButton').on('click', () => {
        window.location.href = '/Login/Logout';
    });

    closeAlert();
    if (!window.fetchAlertsFlag) {
        fetchAlerts();
    }
});

var data = {};

window.latestAlertData = null; // store latest JSON globally
window.alertFetchTimer = null;

async function fetchAlerts(jsonData = null) {
    try {
        let data;
        if (jsonData) {
            // Use JSON from table or elsewhere
            data = jsonData;
            window.latestAlertData = data; // store globally
        } else if (window.latestAlertData) {
            // If we already have data from a table, reuse it
            data = window.latestAlertData;
        } else {
            // Otherwise fetch from server
            const response = await fetch('/getorders');
            if (!response.ok) {
                console.error("Server returned error:", response.status, response.statusText);
                return;
            }
            data = await response.json();
            window.latestAlertData = data;
        }

        // Process alerts
        const alertBox = document.getElementById("floating-alert");
        const alertList = document.getElementById("alert-list");
        const storedAlerts = JSON.parse(sessionStorage.getItem("displayedAlerts")) || {};

        const filteredOrders = data.filter(order => order.alert && order.alert.trim() !== "");
        const newAlerts = {};
        let hasNewAlerts = false;

        filteredOrders.forEach(order => {
            const { voucherCode, alert } = order;
            if (!storedAlerts[voucherCode] || storedAlerts[voucherCode] !== alert) {
                hasNewAlerts = true;
                newAlerts[voucherCode] = alert;
            }
        });

        if (hasNewAlerts) {
            alertList.innerHTML = ""; // clear old alerts
            for (const voucherCode in newAlerts) {
                const listItem = document.createElement("li");
                listItem.innerHTML = `<a class="alerts" style="text-decoration: none;" target="_blank" href="/order/${voucherCode}">Order ${voucherCode}: ${newAlerts[voucherCode]}</a>`;
                alertList.appendChild(listItem);
            }
            alertBox.style.display = "block";
            sessionStorage.setItem("displayedAlerts", JSON.stringify({ ...storedAlerts, ...newAlerts }));
        }

    } catch (error) {
        console.error("Error processing alerts:", error);
    } finally {
        // Only schedule the next fetch if we're fetching from the server
        if (!jsonData && !window.alertFetchTimer) {
            window.alertFetchTimer = setInterval(() => fetchAlerts(), 10000); // 10s refresh
        }
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
// Example: clipboard copy using reusable toast
function copyToClipboard(text) {
    if (!text || text === "null") return;

    navigator.clipboard.writeText(text)
        .then(() => {
            Toastify({
                text: `${text} copied to clipboard!`,
                duration: 2000,
                close: true,
                gravity: "top",
                position: "center",
                style: {
                    background: "linear-gradient(to right, #28a745, #218838)",
                    fontSize: "13px",     // smaller font
                    padding: "6px 10px",  // smaller padding
                    minHeight: "30px"     // smaller overall height
                }
            }).showToast();
        })
        .catch(err => {
            Toastify({
                text: "Failed to copy: " + err,
                duration: 3000,
                close: true,
                gravity: "top",
                position: "center",
                style: {
                    background: "linear-gradient(to right, #dc3545, #c82333)",
                    fontSize: "13px",
                    padding: "6px 10px",
                    minHeight: "30px"
                }
            }).showToast();
        });
}

function showToast(message, type = "info") {
    let bgColor = "";

    switch (type) {
        case "success":
            bgColor = "linear-gradient(to right, #00b09b, #96c93d)";
            break;
        case "error":
            bgColor = "linear-gradient(to right, #ff5f6d, #ffc371)";
            break;
        case "warning":
            bgColor = "linear-gradient(to right, #f2994a, #f2c94c)";
            break;
        default:
            bgColor = "linear-gradient(to right, #616161, #9bc5c3)";
    }

    Toastify({
        text: message,
        duration: 4000,
        close: true,
        gravity: "top",
        position: "right",
        style: { background: bgColor },
        stopOnFocus: true,
    }).showToast();
}