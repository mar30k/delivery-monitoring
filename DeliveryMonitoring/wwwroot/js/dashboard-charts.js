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


async function fetchAndUpdateChart() {
    const orderSlices = [
        { url: '/getChartData?type=delivery', index: 1, label: 'Delivery' },
        { url: '/getChartData?type=takeaway', index: 0, label: 'Takeaway' },
        { url: '/getChartData?type=dinein', index: 2, label: 'Dine-in' }
    ];

    // fetch a single slice
    async function fetchSlice(slice) {
        try {
            const response = await fetch(slice.url);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const data = await response.json();
            return { slice, count: data.count || 0, total: data.total || 0, success: true };
        } catch (err) {
            console.warn(`Failed to fetch ${slice.label}:`, err);
            return { slice, count: 0, total: 0, success: false };
        }
    }

    // fetch all slices
    const results = await Promise.all(orderSlices.map(fetchSlice));

    // update charts once at the end only for successful slices
    results
        .filter(r => r.success)
        .forEach(r => {
            countChart.data.datasets[0].data[r.slice.index] = r.count;
            countChart.data.labels[r.slice.index] = `${r.slice.label} (${r.count})`;

            totalChart.data.datasets[0].data[r.slice.index] = r.total;
            totalChart.data.labels[r.slice.index] = `${r.slice.label} (${r.total.toFixed(2)})`;
        });

    countChart.update();
    totalChart.update();
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
        const response = await fetch('/getAvailableSupervisors');
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
    fetchAndUpdateChart();
    setInterval(fetchAndUpdateChart, 60000);
    fetchKotStatus();
    updateDriversChart();
    setInterval(updateOrdersChart, 10000);
    setInterval(updateDriversChart, 30000);
    setInterval(fetchKotStatus, 60000);
    setInterval(fetchSupervisors, 60000);
});