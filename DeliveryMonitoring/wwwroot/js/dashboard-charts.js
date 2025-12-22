/* =========================================================
   DASHBOARD CONFIGURATION
========================================================= */

const CONFIG = {
    refresh: {
        orders: 10000,
        drivers: 30000,
        charts: 60000
    },
    api: {
        driverLive: '/driver/liveLocation',
        orderChart: '/getChartData',
        kotStatus: '/getDeviceControl',
        supervisors: '/getAvailableSupervisors'
    },
    colors: {
        drivers: [
            '#28a745', '#dc3545', 'seagreen', 'darkorange',
            '#20c997', 'coral', '#F7BEA2'
        ],
        orderTypes: ['#17a2b8', '#007bff', '#ffc107'],
        orderStatus: [
            'green', 'deepskyblue', 'lawngreen', 'seagreen',
            'darkorange', 'red', 'firebrick',
            'coral', '#F7BEA2', 'darkred'
        ]
    }
};

/* =========================================================
   UTILITIES
========================================================= */

async function fetchJson(url) {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return response.json();
}

function today() {
    return new Date().toISOString().slice(0, 10);
}

/* =========================================================
   CHART FACTORY
========================================================= */

function createDoughnutChart({ ctx, label, labels, data, colors, title }) {
    return new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels,
            datasets: [{
                label,
                data,
                backgroundColor: colors,
                hoverOffset: 4
            }]
        },
        options: {
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { font: { size: 14 } }
                },
                title: {
                    display: true,
                    text: title,
                    font: { size: 18 }
                }
            }
        }
    });
}

/* =========================================================
   CHART INITIALIZATION
========================================================= */

const charts = {
    drivers: createDoughnutChart({
        ctx: document.getElementById('driversChart'),
        label: 'Drivers',
        labels: [
            'Ready', 'Offline', 'Accepted',
            'Delivering', 'Completed',
            'ArrivedAtBranch', 'Arrived'
        ],
        data: window.initialChartData.driverDataSet,
        colors: CONFIG.colors.drivers,
        title: 'Drivers Status'
    }),

    completedCount: createDoughnutChart({
        ctx: document.getElementById('completdChart'),
        label: 'Orders',
        labels: ['Takeaway (0)', 'Delivery (0)', 'Dine-in (0)'],
        data: [0, 0, 0],
        colors: CONFIG.colors.orderTypes,
        title: `Completed Orders ${today()}`
    }),

    completedTotal: createDoughnutChart({
        ctx: document.getElementById('completedTotalChart'),
        label: 'Orders',
        labels: ['Takeaway (0)', 'Delivery (0)', 'Dine-in (0)'],
        data: [0, 0, 0],
        colors: CONFIG.colors.orderTypes,
        title: `Completed Orders Total ${today()}`
    }),

    orderStatus: createDoughnutChart({
        ctx: document.getElementById('ordersChart'),
        label: 'Orders',
        labels: [
            'Completed', 'Requested', 'Assigned', 'Accepted',
            'On The Way', 'Declined', 'Driver Not Found',
            'ArrivedAtBranch', 'Arrived', 'SOS'
        ],
        data: window.initialChartData.orderDataSet,
        colors: CONFIG.colors.orderStatus,
        title: 'Orders Status'
    })
};

/* =========================================================
   DATA UPDATERS
========================================================= */

function updateOrdersChart() {
    if (!Array.isArray(data)) return;

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

    data.forEach(order => {
        if (statusCounts.hasOwnProperty(order.status)) {
            statusCounts[order.status]++;
        }
    });

    charts.orderStatus.data.datasets[0].data = [
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

    charts.orderStatus.update();
}

async function updateCompletedOrdersCharts() {
    const slices = [
        { type: 'takeaway', index: 0, label: 'Takeaway' },
        { type: 'delivery', index: 1, label: 'Delivery' },
        { type: 'dinein', index: 2, label: 'Dine-in' }
    ];

    const results = await Promise.all(
        slices.map(async s => {
            try {
                const data = await fetchJson(`${CONFIG.api.orderChart}?type=${s.type}`);
                return { ...s, ...data, success: true };
            } catch {
                return { ...s, count: 0, total: 0, success: false };
            }
        })
    );

    results.filter(r => r.success).forEach(r => {
        charts.completedCount.data.datasets[0].data[r.index] = r.count;
        charts.completedCount.data.labels[r.index] = `${r.label} (${r.count})`;

        charts.completedTotal.data.datasets[0].data[r.index] = r.total;
        charts.completedTotal.data.labels[r.index] = `${r.label} (${r.total.toFixed(2)})`;
    });

    charts.completedCount.update();
    charts.completedTotal.update();
}

async function updateDriversChart() {
    const drivers = await fetchJson(CONFIG.api.driverLive);
    if (!Array.isArray(drivers)) return;

    const statusCounts = {
        ready: 0, offline: 0, accepted: 0,
        delivering: 0, completed: 0,
        arrivedAtBranch: 0, arrived: 0
    };
    drivers.forEach(d => {
        if (statusCounts.hasOwnProperty(d.status)) {
            statusCounts[d.status]++;
        }
    });

    charts.drivers.data.datasets[0].data = Object.values(statusCounts);
    charts.drivers.update();

    $("#totalDrivers").text(drivers.filter(d => !d.isDisabled).length);
    $("#readyDrivers").text(statusCounts.ready);
}

async function fetchKotStatus() {
    const data = await fetchJson(CONFIG.api.kotStatus);
    $("#kotStatus").text(Array.isArray(data) ? data.length : 0);
}

async function fetchSupervisors() {
    const data = await fetchJson(CONFIG.api.supervisors);
    if (!Array.isArray(data)) return;

    $("#totalSupervisors").text(data.length);
    $("#loggedinSupervisors").text(data.filter(s => s.loggedInStatus).length);
}

/* =========================================================
   DASHBOARD BOOTSTRAP
========================================================= */

$(function () {
    updateCompletedOrdersCharts();
    updateDriversChart();
    fetchKotStatus();
    fetchSupervisors();


    setInterval(updateCompletedOrdersCharts, CONFIG.refresh.charts);
    setInterval(updateDriversChart, CONFIG.refresh.drivers);
    setInterval(updateOrdersChart, CONFIG.refresh.orders);
    setInterval(fetchKotStatus, CONFIG.refresh.charts);
    setInterval(fetchSupervisors, CONFIG.refresh.charts);
});