const ctx = document.getElementById('driversChart');
const orderChart = document.getElementById('ordersChart');
var driverDataSet = window.initialChartData.driverDataSet;
var orderDataSet = window.initialChartData.orderDataSet;
function createDoughnutChart(ctx, label, labels, dataSet, colors, titleText) {
    return new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: dataSet,
                backgroundColor: colors,
                hoverOffset: 4
            }]
        },
        options: {
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        font: {
                            size: 14
                        }
                    }
                },
                title: {
                    display: true,
                    text: titleText,
                    font: {
                        size: 18
                    }
                }
            }
        }
    });
}
const driverChart = createDoughnutChart(
    ctx,
    'Drivers',
    ['Ready', 'Offline', 'Accepted', 'Delivering', 'Completed', 'ArrivedAtBranch', 'Arrived'],
    driverDataSet,
    [
        '#28a745',     // Ready
        '#dc3545',     // Offline
        'seagreen',    // Accepted
        'darkorange',  // Delivering
        '#20c997',     // Completed
        'coral',       // ArrivedAtBranch
        '#F7BEA2'      // Arrived
    ],
    'Drivers Status'
);

// Initialize empty charts first
const countChart = createDoughnutChart(
    document.getElementById('completdChart'),
    'Orders',
    ['Takeaway (0)', 'Delivery (0)', 'Dine-in (0)'],
    [0, 0, 0],
    ['#17a2b8', '#007bff', '#ffc107'],
    `Completed Orders ${new Date().toISOString().slice(0, 10)}`
);

const totalChart = createDoughnutChart(
    document.getElementById('completedTotalChart'),
    'Orders',
    ['Takeaway (0)', 'Delivery (0)', 'Dine-in (0)'],
    [0, 0, 0],
    ['#17a2b8', '#007bff', '#ffc107'],
    `Completed Orders Total ${new Date().toISOString().slice(0, 10)}`
);

async function fetchAndUpdateChart(maxRetries = 3, updateMode = "all-at-once") {
    const orderSlices = [
        { url: '/GetChartData?type=delivery', index: 1, label: 'Delivery' },
        { url: '/GetChartData?type=takeaway', index: 0, label: 'Takeaway' },
        { url: '/GetChartData?type=dinein', index: 2, label: 'Dine-in' }
    ];

    // fetch and update a single slice
    async function fetchSlice(slice) {
        try {
            const response = await fetch(slice.url);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const data = await response.json();

            const count = data.count || 0;
            const total = data.total || 0;

            // update charts immediately if in gradual mode
            if (updateMode === "gradual") {
                updateCharts(slice, count, total);
            }

            return { slice, count, total, success: true };
        } catch (err) {
            console.warn(`Failed to fetch ${slice.label}:`, err);
            return { slice, success: false };
        }
    }

    function updateCharts(slice, count, total) {
        // Update count chart
        countChart.data.datasets[0].data[slice.index] = count;
        countChart.data.labels[slice.index] = `${slice.label} (${count})`;

        // Update total chart
        totalChart.data.datasets[0].data[slice.index] = total;
        totalChart.data.labels[slice.index] = `${slice.label} (${total.toFixed(2)})`;

        // Gradual mode updates immediately
        if (updateMode === "gradual") {
            countChart.update();
            totalChart.update();
        }
    }

    // retry logic for failed slices
    async function retryFailed(failed, attempt) {
        if (!failed.length) return [];
        console.log(`Retrying ${failed.length} slice(s), attempt ${attempt}...`);
        await new Promise(r => setTimeout(r, 1000 * attempt));
        const results = await Promise.all(failed.map(fetchSlice));
        return results.filter(r => !r.success).map(r => r.slice);
    }

    // --- Main run ---
    let results = await Promise.all(orderSlices.map(fetchSlice));

    // retry rounds if any failed
    let failedSlices = results.filter(r => !r.success).map(r => r.slice);
    for (let attempt = 2; attempt <= maxRetries && failedSlices.length; attempt++) {
        failedSlices = await retryFailed(failedSlices, attempt);
    }

    // All done: final update (for all-at-once mode)
    if (updateMode === "all-at-once") {
        results = results.filter(r => r.success);
        for (const r of results) updateCharts(r.slice, r.count, r.total);
        countChart.update();
        totalChart.update();
    }
}
// Orders Status Chart
const ordersStatusChart = createDoughnutChart(
    orderChart,
    'Orders',
    [
        'Completed',
        'Requested',
        'Assigned',
        'Accepted',
        'On The Way',
        'Declined',
        'Driver Not Found',
        'ArrivedAtBranch',
        'Arrived',
        'sos'
    ],
    orderDataSet,
    [
        'green',       // Completed
        'deepskyblue', // Requested
        'lawngreen',   // Assigned
        'seagreen',    // Accepted
        'darkorange',  // On The Way
        'red',         // Declined
        'firebrick',   // Driver Not Found
        'coral',       // ArrivedAtBranch
        '#F7BEA2',     // Arrived
        'darkred'      // SOS
    ],
    'Orders Status'
);

function updateOrdersChart() {
    const statusCounts = {
        completed: 0,
        requested: 0,
        assigned: 0,
        accepted: 0,
        ontheway: 0,
        declined: 0,
        driverNotFound: 0,
        arrivedAtBranch: 0,
        arrived: 0,
        sos: 0
    };

    // Count orders by status
    data.forEach(order => {
        if (statusCounts.hasOwnProperty(order.status)) {
            statusCounts[order.status]++;
        }
    });

    // Update chart data
    ordersStatusChart.data.datasets[0].data = [
        statusCounts.completed,
        statusCounts.requested,
        statusCounts.assigned,
        statusCounts.accepted,
        statusCounts.ontheway,
        statusCounts.declined,
        statusCounts.driverNotFound,
        statusCounts.arrivedAtBranch,
        statusCounts.arrived,
        statusCounts.sos
    ];

    ordersStatusChart.update();
}
function updateDriversChart() {
    const statusCounts = {
        ready: 0,
        offline: 0,
        accepted: 0,
        delivering: 0,
        completed: 0,
        arrivedAtBranch: 0,
        arrived: 0,
    };
    fetch(`/Driver/LiveLocation`)
        .then(response => response.json())
        .then(data => {
            if (!Array.isArray(data) || data == null) return;
            const activeCount = data.filter(driver => !driver.isDisabled).length;
            const readyCount = data.filter(driver => driver.status === 'ready').length;
            $("#totalDrivers").text(activeCount);
            $("#readyDrivers").text(readyCount);
            // Count orders by status
            data.forEach(driver => {
                if (statusCounts.hasOwnProperty(driver.status)) {
                    statusCounts[driver.status]++;
                }
            });

            // Update chart data
            driverChart.data.datasets[0].data = [
                statusCounts.ready,
                statusCounts.offline,
                statusCounts.accepted,
                statusCounts.delivering,
                statusCounts.completed,
                statusCounts.arrivedAtBranch,
                statusCounts.arrived,
            ];

            driverChart.update();
        });
}
async function fetchKotStatus() {
    try {
        const response = await fetch('/getDeviceControl');
        const kotStatus = await response.json();
        if (!Array.isArray(kotStatus)) return;
        $("#kotStatus").text(kotStatus.length);
    }
    catch (error) {
        console.error("Error fetching alerts:", error);
    }
}
async function fetchSupervisors() {
    try {
        const response = await fetch('order/getAvailableSupervisors');
        let supervisors = await response.json();
        if (!Array.isArray(supervisors)) return;
        $("#totalSupervisors").text(supervisors.length);
        $("#loggedinSupervisors").text(supervisors.filter(s => s.loggedInStatus).length);
    }
    catch (error) {
        console.error("Error fetching alerts:", error);
    }
}
$(function () {
    // Run on page load
    setInterval(() => fetchAndUpdateChart(3, "all-at-once"), 60000);
    fetchAndUpdateChart();
    fetchKotStatus();
    updateDriversChart();
    setInterval(updateOrdersChart, 10000);
    setInterval(updateDriversChart, 30000);
    setInterval(fetchKotStatus, 60000);
    setInterval(fetchSupervisors, 60000);
});