const { DASHBOARD_CONFIG } = await import(`./dashboard-config.js?v=${Date.now()}`);
const { DashboardUtils } = await import(`./dashboard-utils.js?v=${Date.now()}`);
const { DashboardCharts } = await import(`./dashboard-charts.js?v=${Date.now()}`);
const { DashboardMap } = await import(`./dashboard-map.js?v=${Date.now()}`);
const { DashboardScroll } = await import(`./dashboard-scroll.js?v=${Date.now()}`);
const { DashboardAlerts } = await import(`./dashboard-alerts.js?v=${Date.now()}`);

/**
 * Main Dashboard Controller
 */
const Dashboard = {
    charts: {},
    orderTypes: [
        { tableId: 'takeAway', index: 0, label: 'Takeaway' },
        { tableId: 'delivery', index: 1, label: 'Delivery' },
        { tableId: 'dineIn', index: 2, label: 'Dine-in' },
        { tableId: 'scheduledDelivery', index: 3, label: 'Scheduled Delivery' },
        { tableId: 'scheduledPickUp', index: 4, label: 'Scheduled Takeaway' }
    ],

    async init() {
        this.initCharts();
        this.setupMap();

        // Initial load
        await this.refreshAll();

        // Start polling
        this.startIntervals();

        if (window.isAnalyticsPage) DashboardScroll.init();
    },

    initCharts() {
        const { driverDataSet, orderDataSet } = window.initialChartData ?? {};

        this.charts.drivers = DashboardCharts.createDoughnut({
            ctx: document.getElementById('driversChart'),
            labels: ['Ready', 'Offline', 'Accepted', 'Delivering', 'Completed', 'ArrivedAtBranch', 'Arrived'],
            data: driverDataSet,
            colors: DASHBOARD_CONFIG.colors.drivers,
            title: 'Drivers Status'
        });

        this.charts.ordersStatus = DashboardCharts.createDoughnut({
            ctx: document.getElementById('ordersChart'),
            labels: ['Completed', 'Requested', 'Assigned', 'Accepted', 'On The Way', 'Declined', 'Driver Not Found', 'ArrivedAtBranch', 'Arrived', 'SOS'],
            data: orderDataSet,
            colors: DASHBOARD_CONFIG.colors.orderStatus,
            title: 'Orders Status'
        });

        // Use helper for repeated chart creation logic
        this.charts.completedCount = this._createCompletedChart('completedChart', `Completed Orders ${DashboardUtils.today()}`);
        this.charts.completedTotal = this._createCompletedChart('completedTotalChart', `Completed Orders Total ${DashboardUtils.today()}`);
    },

    async refreshDrivers() {
        try {
            const drivers = await DashboardUtils.fetchJson(DASHBOARD_CONFIG.api.driverLive);
            if (!Array.isArray(drivers)) return;

            const counts = this._tally(drivers, ['ready', 'offline', 'accepted', 'delivering', 'completed', 'arrivedAtBranch', 'arrived']);

            DashboardCharts.updateDataset(this.charts.drivers, Object.values(counts));
            $("#totalDrivers").text(drivers.filter(d => !d.isDisabled).length);
            $("#readyDrivers").text(counts.ready);

            if (window.isAnalyticsPage) DashboardMap.refresh(drivers);
        } catch (err) { console.error("Drivers update failed", err); }
    },

    async refreshOrders() {
        try {
            const orders = window.isAnalyticsPage
                ? await DashboardUtils.fetchJson(DASHBOARD_CONFIG.api.orders)
                : window.data;

            if (!Array.isArray(orders)) return;

            const statusKeys = ['completed', 'requested', 'assigned', 'accepted', 'ontheway', 'declined', 'driverNotFound', 'arrivedAtBranch', 'arrived', 'sos'];
            const counts = this._tally(orders, statusKeys);

            DashboardCharts.updateDataset(this.charts.ordersStatus, statusKeys.map(k => counts[k]));
            $("#orderCount").text(orders.length);

            if (window.isAnalyticsPage) DashboardAlerts.processOrders(orders);
        } catch (err) { console.error("Orders update failed", err); }
    },

    async refreshAnalytics() {
        try {
            const data = await DashboardUtils.fetchJson(DASHBOARD_CONFIG.api.orderChart);
            this.orderTypes.forEach(({ tableId, index, label }) => {
                const entry = data.find(o => o.tableId === tableId) || { count: 0, total: 0 };

                this._updateChartSlice(this.charts.completedCount, index, label, entry.count);
                this._updateChartSlice(this.charts.completedTotal, index, label, entry.total.toFixed(2));
            });
            this.charts.completedCount.update();
            this.charts.completedTotal.update();
        } catch (err) { console.error("Analytics update failed", err); }
    },

    async refreshStats() {
        try {
            const [supervisors, kot] = await Promise.all([
                DashboardUtils.fetchJson(DASHBOARD_CONFIG.api.supervisors),
                DashboardUtils.fetchJson(DASHBOARD_CONFIG.api.kotStatus)
            ]);

            if (Array.isArray(supervisors)) {
                $("#totalSupervisors").text(supervisors.length);
                $("#loggedinSupervisors").text(supervisors.filter(s => s.loggedInStatus).length);
            }
            $("#kotStatus").text(Array.isArray(kot) ? kot.length : 0);
        } catch (err) { console.error("Stats update failed", err); }
    },

    async refreshAll() {
        return Promise.allSettled([
            this.refreshDrivers(),
            this.refreshOrders(),
            this.refreshAnalytics(),
            this.refreshStats()
        ]);
    },

    startIntervals() {
        setInterval(() => this.refreshDrivers(), DASHBOARD_CONFIG.refresh.drivers);
        setInterval(() => this.refreshOrders(), DASHBOARD_CONFIG.refresh.orders);
        setInterval(() => {
            this.refreshAnalytics();
            this.refreshStats();
        }, DASHBOARD_CONFIG.refresh.charts);
    },

    /** Helpers **/
    _tally(list, keys) {
        const counts = {};
        keys.forEach(k => counts[k] = 0);
        list.forEach(item => { if (counts[item.status] !== undefined) counts[item.status]++; });
        return counts;
    },

    _createCompletedChart(id, title) {
        return DashboardCharts.createDoughnut({
            ctx: document.getElementById(id),
            labels: this.orderTypes.map(o => `${o.label} (0)`),
            data: Array(this.orderTypes.length).fill(0),
            colors: DASHBOARD_CONFIG.colors.orderTypes,
            title: title
        });
    },

    _updateChartSlice(chart, index, label, value) {
        chart.data.datasets[0].data[index] = value;
        chart.data.labels[index] = `${label} (${value})`;
    },

    async setupMap() {
        if (!window.isAnalyticsPage) return;
        try {
            await this._loadGoogleMaps(DASHBOARD_CONFIG.googleMapsKey);
            DashboardMap.initMap();
        } catch (e) { console.error("Maps failed", e); }
    },

    _loadGoogleMaps(key) {
        return new Promise((res, rej) => {
            if (window.google?.maps) return res();
            const s = document.createElement('script');
            s.src = `https://maps.googleapis.com/maps/api/js?key=${key}`;
            s.onload = res; s.onerror = rej;
            document.head.appendChild(s);
        });
    }
};

// Start the Dashboard
$(() => Dashboard.init());
