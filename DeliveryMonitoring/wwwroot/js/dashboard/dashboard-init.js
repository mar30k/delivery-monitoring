const { DASHBOARD_CONFIG } = await import(`./dashboard-config.js?v=${Date.now()}`);
const { DashboardUtils } = await import(`./dashboard-utils.js?v=${Date.now()}`);
const { DashboardCharts } = await import(`./dashboard-charts.js?v=${Date.now()}`);
const { DashboardMap } = await import(`./dashboard-map.js?v=${Date.now()}`);
const { DashboardScroll } = await import(`./dashboard-scroll.js?v=${Date.now()}`);
const { DashboardAlerts } = await import(`./dashboard-alerts.js?v=${Date.now()}`);

$(function () {

    const { driverDataSet, orderDataSet } = window.initialChartData ?? {};
    const orderTypes = [
        { tableId: 'takeAway', index: 0, label: 'Takeaway' },
        { tableId: 'delivery', index: 1, label: 'Delivery' },
        { tableId: 'dineIn', index: 2, label: 'Dine-in' },
        { tableId: 'scheduledDelivery', index: 3, label: 'Scheduled Delivery' },
        { tableId: 'scheduledPickUp', index: 4, label: 'Scheduled Takeaway' }
    ];
    const charts = {
        drivers: DashboardCharts.createDoughnut({
            ctx: document.getElementById('driversChart'),
            labels: [
                'Ready', 'Offline', 'Accepted',
                'Delivering', 'Completed',
                'ArrivedAtBranch', 'Arrived'
            ],
            data: driverDataSet,
            colors: DASHBOARD_CONFIG.colors.drivers,
            title: 'Drivers Status'
        }),

        ordersStatus: DashboardCharts.createDoughnut({
            ctx: document.getElementById('ordersChart'),
            labels: [
                'Completed', 'Requested', 'Assigned', 'Accepted',
                'On The Way', 'Declined', 'Driver Not Found',
                'ArrivedAtBranch', 'Arrived', 'SOS'
            ],
            data: orderDataSet,
            colors: DASHBOARD_CONFIG.colors.orderStatus,
            title: 'Orders Status'
        }),

        completedCount: DashboardCharts.createDoughnut({
            ctx: document.getElementById('completedChart'),
            labels: orderTypes.map(o => `${o.label} (0)`),
            data: Array(orderTypes.length).fill(0),
            colors: DASHBOARD_CONFIG.colors.orderTypes,
            title: `Completed Orders ${DashboardUtils.today()}`
        }),

        completedTotal: DashboardCharts.createDoughnut({
            ctx: document.getElementById('completedTotalChart'),
            labels: orderTypes.map(o => `${o.label} (0)`),
            data: Array(orderTypes.length).fill(0),
            colors: DASHBOARD_CONFIG.colors.orderTypes,
            title: `Completed Orders Total ${DashboardUtils.today()}`
        }),
    };
    async function refreshDriversChart() {
        const drivers = await DashboardUtils.fetchJson(
            DASHBOARD_CONFIG.api.driverLive
        );
        if (!Array.isArray(drivers)) return;

        const counts = {
            ready: 0,
            offline: 0,
            accepted: 0,
            delivering: 0,
            completed: 0,
            arrivedAtBranch: 0,
            arrived: 0
        };

        drivers.forEach(d => {
            if (counts[d.status] !== undefined) {
                counts[d.status]++;
            }
        });

        DashboardCharts.updateDataset(
            charts.drivers,
            Object.values(counts)
        );

        $("#totalDrivers").text(drivers.filter(d => !d.isDisabled).length);
        $("#readyDrivers").text(counts.ready);

        if (window.isAnalyticsPage) {
            DashboardMap.refresh(drivers);
        }
    }
    async function refreshOrdersStatusChart() {
        let orders;
        if (window.isAnalyticsPage) {
            orders = await DashboardUtils.fetchJson(
                DASHBOARD_CONFIG.api.orders
            );
        }
        else {
            orders = window.data; // global orders already available
        }
        if (!Array.isArray(orders)) return;
        const counts = {
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

        orders.forEach(o => {
            if (counts[o.status] !== undefined) {
                counts[o.status]++;
            }
        });

        DashboardCharts.updateDataset(charts.ordersStatus, [
            counts.completed,
            counts.requested,
            counts.assigned,
            counts.accepted,
            counts.ontheway,
            counts.declined,
            counts.driverNotFound,
            counts.arrivedAtBranch,
            counts.arrived,
            counts.sos
        ]);

        $("#orderCount").text(orders.length);
        if (window.isAnalyticsPage) {
            DashboardAlerts.processOrders(orders);
        }
    }
    async function refreshCompletedOrdersCharts() {

        try {
            const data = await DashboardUtils.fetchJson(DASHBOARD_CONFIG.api.orderChart);
            orderTypes.forEach(({ tableId, index, label }) => {
                const orderData = data.find(o => o.tableId === tableId) || { count: 0, total: 0 };

                charts.completedCount.data.datasets[0].data[index] = orderData.count;
                charts.completedCount.data.labels[index] = `${label} (${orderData.count})`;

                charts.completedTotal.data.datasets[0].data[index] = orderData.total;
                charts.completedTotal.data.labels[index] = `${label} (${orderData.total.toFixed(2)})`;
            });

            charts.completedCount.update();
            charts.completedTotal.update();
        } catch (err) {
            console.error('Failed to refresh completed orders charts', err);
        }
    }

    async function refreshSupervisors() {
        try {
            const supervisors = await DashboardUtils.fetchJson(
                DASHBOARD_CONFIG.api.supervisors
            );

            if (!Array.isArray(supervisors)) return;

            const total = supervisors.length;
            const loggedIn = supervisors.filter(s => s.loggedInStatus).length;

            $("#totalSupervisors").text(total);
            $("#loggedinSupervisors").text(loggedIn);

        } catch (err) {
            console.error("Failed to refresh supervisors", err);
        }
    }

    async function refreshKotStatus() {
        try {
            const kot = await DashboardUtils.fetchJson(
                DASHBOARD_CONFIG.api.kotStatus
            );

            const count = Array.isArray(kot) ? kot.length : 0;
            $("#kotStatus").text(count);

        } catch (err) {
            console.error("Failed to refresh KOT status", err);
        }
    }

    function loadGoogleMapsAPI(apiKey) {
        return new Promise((resolve, reject) => {
            if (window.google?.maps) {
                resolve();
                return;
            }

            const script = document.createElement('script');
            script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}`;
            script.async = true;
            script.defer = true;

            script.onload = resolve;
            script.onerror = reject;

            document.head.appendChild(script);
        });
    }

    if (window.isAnalyticsPage) {
        loadGoogleMapsAPI(DASHBOARD_CONFIG.googleMapsKey)
            .then(() => DashboardMap.initMap())
            .catch(() => console.error("Failed to load Google Maps"));
    }
    refreshSupervisors();
    refreshDriversChart();
    refreshOrdersStatusChart();
    refreshCompletedOrdersCharts();
    refreshKotStatus();
    if (window.isAnalyticsPage) {
        DashboardScroll.init();
    }



    setInterval(refreshDriversChart, DASHBOARD_CONFIG.refresh.drivers);
    setInterval(refreshOrdersStatusChart, DASHBOARD_CONFIG.refresh.orders);
    setInterval(refreshCompletedOrdersCharts, DASHBOARD_CONFIG.refresh.charts);
    setInterval(refreshSupervisors, DASHBOARD_CONFIG.refresh.charts);
    setInterval(refreshKotStatus, DASHBOARD_CONFIG.refresh.charts);
});
