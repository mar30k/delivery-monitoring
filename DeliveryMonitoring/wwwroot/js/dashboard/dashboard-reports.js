const { DashboardCharts } = await import(`/js/dashboard/dashboard-charts.js?v=${Date.now()}`);
const { DashboardUtils } = await import(`/js/dashboard/dashboard-utils.js?v=${Date.now()}`);
const { DashboardScroll } = await import(`/js/dashboard/dashboard-scroll.js?v=${Date.now()}`);

const COLORS = DashboardCharts.COLORS;
const renderChart = DashboardCharts.renderChart;

// Global cache to allow targeted chart updates without re-fetching
let cachedOrdersData = [];

bootstrap();

async function bootstrap() {
    try {
        // Initial load
        await loadDashboard();
    } catch (error) {
        console.error("Dashboard failed to load", error);
    } finally {
        // Hide loader with a slight delay for a smoother transition
        setTimeout(() => {
            const loader = document.getElementById('dashboardLoader');
            if (loader) loader.classList.add('loader-hidden');
        }, 600);
    }
    if (window.isAnalyticsPage)
        DashboardScroll.init();
    // Background interval (don't show loader for these)
    setInterval(loadDashboard, 5 * 60 * 1000);
}
const Processors = {
    // 1. Traffic Pattern: Filtered by the Moment object from the date picker
    todayHourly: (data, referenceMoment) => {
        const hours = data.reduce((acc, o) => {
            const orderDate = moment(o.requestCreatedAt);
            if (orderDate.isSame(referenceMoment, 'day')) {
                const hr = orderDate.hour();
                acc[hr] = (acc[hr] || 0) + 1;
            }
            return acc;
        }, {});
        return {
            labels: Array.from({ length: 24 }, (_, i) => `${i}:00`),
            values: Array.from({ length: 24 }, (_, i) => hours[i] || 0)
        };
    },

    // 2. Daily Volume (30 Days)
    daily30: (data) => {
        const thirtyDaysAgo = moment().subtract(30, 'days');
        const grouped = data
            .filter(o => moment(o.requestCreatedAt).isAfter(thirtyDaysAgo))
            .reduce((acc, o) => {
                const date = moment(o.requestCreatedAt).format('YYYY-MM-DD');
                acc[date] = (acc[date] || 0) + 1;
                return acc;
            }, {});
        const sorted = Object.keys(grouped).sort();
        return { labels: sorted, values: sorted.map(k => grouped[k]) };
    },

    // 3. Payment Split
    payments: (data) => {
        const counts = data.reduce((acc, o) => {
            const m = o.paymentMethod || 'Unknown';
            acc[m] = (acc[m] || 0) + 1;
            return acc;
        }, {});
        return { labels: Object.keys(counts), values: Object.values(counts) };
    },

    // 4. Monthly History
    monthly: (data) => {
        const grouped = data.reduce((acc, o) => {
            const m = moment(o.requestCreatedAt);
            const key = m.format('YYYY-MM');
            const label = m.format('MMM YYYY');
            if (!acc[key]) acc[key] = { label, val: 0 };
            acc[key].val += 1;
            return acc;
        }, {});
        const sorted = Object.keys(grouped).sort();
        return { labels: sorted.map(k => grouped[k].label), values: sorted.map(k => grouped[k].val) };
    },

    // 5. Company Revenue
    partners: (data) => {
        const rev = data.reduce((acc, o) => {
            acc[o.companyName] = (acc[o.companyName] || 0) + (parseFloat(o.totalAmount) || 0);
            return acc;
        }, {});
        const sorted = Object.entries(rev).sort((a, b) => b[1] - a[1]).slice(0, 20);
        return { labels: sorted.map(x => x[0]), values: sorted.map(x => x[1]) };
    },

    // 6. Global Peak Hours
    globalPeaks: (data) => {
        const hours = data.reduce((acc, o) => {
            const hr = moment(o.requestCreatedAt).hour();
            acc[hr] = (acc[hr] || 0) + 1;
            return acc;
        }, {});
        return { labels: Array.from({ length: 24 }, (_, i) => `${i}:00`), values: Array.from({ length: 24 }, (_, i) => hours[i] || 0) };
    },

    // 7. Daily Volume (7 Days)
    weekly: (data, referenceMoment) => {
        const last7 = [];
        for (let i = 6; i >= 0; i--) {
            const d = moment(referenceMoment).subtract(i, 'days');
            last7.push({
                dateKey: d.format('YYYY-MM-DD'),
                dayName: d.format('dddd'),
                count: 0
            });
        }
        data.forEach(o => {
            const dStr = moment(o.requestCreatedAt).format('YYYY-MM-DD');
            const match = last7.find(x => x.dateKey === dStr);
            if (match) match.count++;
        });
        return { labels: last7.map(x => x.dayName), values: last7.map(x => x.count) };
    }
};

// EXPOSED FUNCTION: Targets only the traffic chart update
window.updateTrafficChart = function () {
    const datePicker = document.getElementById('trafficDateSelector');
    if (!datePicker || !cachedOrdersData.length) return;

    const selectedMoment = moment(datePicker.value);
    const trafficData = Processors.todayHourly(cachedOrdersData, selectedMoment);

    renderChart(
        "hourlyTraficPtterToday",
        "bar",
        trafficData.labels,
        trafficData.values,
        `Orders for ${selectedMoment.format('MMM DD, YYYY')}`,
        COLORS.info
    );
};

function updateStats(data, referenceMoment) {
    const todayStr = referenceMoment.format('YYYY-MM-DD');
    const threeMonthsAgo = moment(referenceMoment).subtract(3, 'months');

    const todayData = data.filter(o => moment(o.requestCreatedAt).isSame(referenceMoment, 'day'));
    const threeMonthData = data.filter(o => moment(o.requestCreatedAt).isAfter(threeMonthsAgo));

    const getStatus = (arr) => {
        let onTime = 0, delayed = 0;
        arr.forEach(o => {
            const eta = parseFloat(o.eta) || 0;
            const duration = parseFloat(o.duration) || 0;
            if (eta > 0 && duration > 0) {
                (duration <= eta) ? onTime++ : delayed++;
            }
        });
        return { onTime, delayed };
    };

    const calcAvg = (arr, key) => {
        const valid = arr.filter(i => parseFloat(i[key]) > 0.1);
        return valid.length ? (valid.reduce((s, o) => s + parseFloat(o[key]), 0) / valid.length) : 0;
    };

    const globalPerf = getStatus(data);
    const todayPerf = getStatus(todayData);
    const totalRev = data.reduce((s, o) => s + (parseFloat(o.totalAmount) || 0), 0);
    const todayRev = todayData.reduce((s, o) => s + (parseFloat(o.totalAmount) || 0), 0);

    const updateText = (id, val) => {
        const el = document.getElementById(id);
        if (el) el.innerText = val;
    };

    updateText('stat-revenue', `ETB ${totalRev.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
    updateText('stat-revenue-today', `ETB ${todayRev.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
    updateText('stat-count', data.length.toLocaleString());
    updateText('stat-count-today', todayData.length.toLocaleString());
    updateText('stat-ontime', globalPerf.onTime.toLocaleString());
    updateText('stat-ontime-today', todayPerf.onTime.toLocaleString());
    updateText('stat-delayed', globalPerf.delayed.toLocaleString());
    updateText('stat-delayed-today', todayPerf.delayed.toLocaleString());
    updateText('stat-duration', `${calcAvg(threeMonthData, 'duration').toFixed(0)}m`);
    updateText('stat-duration-today', `${calcAvg(todayData, 'duration').toFixed(0)}m`);
    updateText('stat-distance', `${calcAvg(threeMonthData, 'distance').toFixed(1)}km`);
    updateText('stat-distance-today', `${calcAvg(todayData, 'distance').toFixed(1)}km`);
}

async function loadDashboard() {
    const [timeResult, orderResult] = await Promise.all([
        DashboardUtils.fetchJson("/serverTime"),
        DashboardUtils.fetchJson("/getCompletedOrders")
    ]);

    if (!orderResult.isSuccessful) return;
    cachedOrdersData = orderResult.data;

    const refMoment = timeResult.serverLocalNow ? moment(timeResult.serverLocalNow) : moment();
    const todayFormatted = refMoment.format('YYYY-MM-DD');

    // Set default and max values for the date picker
    const datePicker = document.getElementById('trafficDateSelector');
    if (datePicker) {
        if (!datePicker.value) datePicker.value = todayFormatted;
        datePicker.max = todayFormatted; // Prevent selecting future dates
    }

    // 1. Update text-based KPIs
    updateStats(cachedOrdersData, refMoment);

    // 2. Prepare remaining datasets
    const ds = {
        weekly: Processors.weekly(cachedOrdersData, refMoment),
        daily30: Processors.daily30(cachedOrdersData),
        payment: Processors.payments(cachedOrdersData),
        monthly: Processors.monthly(cachedOrdersData),
        partner: Processors.partners(cachedOrdersData),
        peaks: Processors.globalPeaks(cachedOrdersData)
    };

    // 3. Render all charts
    renderChart("weeklyPerformanceChart", "line", ds.weekly.labels, ds.weekly.values, "Weekly Orders", COLORS.primary);
    renderChart("dailyOrdersChart", "line", ds.daily30.labels, ds.daily30.values, "Daily Total", COLORS.primary);
    renderChart("paymentMethodChart", "doughnut", ds.payment.labels, ds.payment.values, "Methods", null, true);
    renderChart("monthlyTrendChart", "line", ds.monthly.labels, ds.monthly.values, "Monthly Total", COLORS.warning);
    renderChart("companyRevenueChart", "bar", ds.partner.labels, ds.partner.values, "Revenue", COLORS.success);
    renderChart("peakHourChart", "bar", ds.peaks.labels, ds.peaks.values, "Global Trend", COLORS.purple);

    // 4. Specifically trigger the Traffic Chart logic
    window.updateTrafficChart();
}