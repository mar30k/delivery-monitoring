const COLORS = {
    primary: '#4e73df', success: '#1cc88a', info: '#36b9cc', warning: '#f6c23e', danger: '#e74a3b', purple: '#6f42c1'
};

const chartInstances = {};

const Processors = {
    // Chart 1: Today's Traffic Only
    todayHourly: (data, referenceDate) => {
        const hours = data.reduce((acc, o) => {
            const orderDate = new Date(o.requestCreatedAt);
            if (orderDate.toDateString() === referenceDate) {
                const hr = orderDate.getHours();
                acc[hr] = (acc[hr] || 0) + 1;
            }
            return acc;
        }, {});
        return { labels: Array.from({ length: 24 }, (_, i) => `${i}:00`), values: Array.from({ length: 24 }, (_, i) => hours[i] || 0) };
    },

    // Chart 2: Daily Volume (30 Days)
    daily30: (data) => {
        const thirtyDaysAgo = new Date();
        thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
        const filtered = data.filter(o => new Date(o.requestCreatedAt) >= thirtyDaysAgo);
        const grouped = filtered.reduce((acc, o) => {
            const date = new Date(o.requestCreatedAt).toISOString().split('T')[0];
            acc[date] = (acc[date] || 0) + 1;
            return acc;
        }, {});
        const sorted = Object.keys(grouped).sort();
        return { labels: sorted, values: sorted.map(k => grouped[k]) };
    },

    // Chart 3: Payment Split
    payments: (data) => {
        const counts = data.reduce((acc, o) => {
            const m = o.paymentMethod || 'Unknown';
            acc[m] = (acc[m] || 0) + 1;
            return acc;
        }, {});
        return { labels: Object.keys(counts), values: Object.values(counts) };
    },

    // Chart 4: Monthly History
    monthly: (data) => {
        const grouped = data.reduce((acc, o) => {
            const d = new Date(o.requestCreatedAt);
            const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
            const label = d.toLocaleString('default', { month: 'short', year: 'numeric' });
            if (!acc[key]) acc[key] = { label, val: 0 };
            acc[key].val += 1;
            return acc;
        }, {});
        const sorted = Object.keys(grouped).sort();
        return { labels: sorted.map(k => grouped[k].label), values: sorted.map(k => grouped[k].val) };
    },

    // Chart 5: Company Revenue
    partners: (data) => {
        const rev = data.reduce((acc, o) => {
            acc[o.companyName] = (acc[o.companyName] || 0) + (o.totalAmount || 0);
            return acc;
        }, {});
        const sorted = Object.entries(rev).sort((a, b) => b[1] - a[1]).slice(0, 20);
        return { labels: sorted.map(x => x[0]), values: sorted.map(x => x[1]) };
    },

    // Chart 6: Global Peak Hours (All Time)
    globalPeaks: (data) => {
        const hours = data.reduce((acc, o) => {
            const hr = new Date(o.requestCreatedAt).getHours();
            acc[hr] = (acc[hr] || 0) + 1;
            return acc;
        }, {});
        return { labels: Array.from({ length: 24 }, (_, i) => `${i}:00`), values: Array.from({ length: 24 }, (_, i) => hours[i] || 0) };
    },

    // Chart 7: Daily Volume (7 Days)
    weekly: (data, referenceDate) => {
        const days = Array.from({ length: 7 }, (_, i) =>
            new Intl.DateTimeFormat(undefined, { weekday: 'long' })
                .format(new Date(1970, 0, 4 + i))
        );
        const last7 = [];
        for (let i = 6; i >= 0; i--) {
            const d = new Date(referenceDate);
            d.setDate(d.getDate() - i);
            last7.push({ dateStr: d.toDateString(), dayName: days[d.getDay()], count: 0 });
        }
        data.forEach(o => {
            const dStr = new Date(o.requestCreatedAt).toDateString();

            const match = last7.find(x => x.dateStr === dStr);
            if (match) match.count++;
        });
        return { labels: last7.map(x => x.dayName), values: last7.map(x => x.count) };
    },
};

function renderChart(id, type, labels, data, label, color, isDoughnut = false) {
    if (chartInstances[id]) chartInstances[id].destroy();
    const ctx = document.getElementById(id).getContext('2d');
    chartInstances[id] = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: data,
                backgroundColor: isDoughnut ? [COLORS.primary, COLORS.success, COLORS.info, COLORS.warning, COLORS.danger] : `${color}33`,
                borderColor: color,
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: isDoughnut,
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        pointStyle: 'circle',
                        padding: 20,
                        font: {
                            size: 11
                        }
                    }
                }
            },
            scales: isDoughnut ? {} : { y: { beginAtZero: true } }
        }
    });
}

function updateStats(data, refDate) {
    // 1. Ensure we have a proper string for comparison and a Date object for math
    const referenceDate = new Date(refDate);
    const todayStr = referenceDate.toDateString();

    // 2. Setup Cutoffs
    const threeMonthsAgo = new Date(referenceDate);
    threeMonthsAgo.setMonth(threeMonthsAgo.getMonth() - 3);

    // 3. Filter Data
    const todayData = data.filter(o => new Date(o.requestCreatedAt).toDateString() === todayStr);
    const threeMonthData = data.filter(o => new Date(o.requestCreatedAt) >= threeMonthsAgo);

    // 4. Helper Functions
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

    // 5. Run Calculations
    const globalPerf = getStatus(data);
    const todayPerf = getStatus(todayData);

    const totalRev = data.reduce((s, o) => s + (parseFloat(o.totalAmount) || 0), 0);
    const todayRev = todayData.reduce((s, o) => s + (parseFloat(o.totalAmount) || 0), 0);

    // Helper to update text safely
    const updateText = (id, val) => {
        const el = document.getElementById(id);
        if (el) el.innerText = val;
    };

    // 6. Update UI
    // Revenue & Counts
    updateText('stat-revenue', `ETB ${totalRev.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
    updateText('stat-revenue-today', `ETB ${todayRev.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);

    updateText('stat-count', data.length.toLocaleString());
    updateText('stat-count-today', todayData.length.toLocaleString());

    // Performance (Split Card)
    updateText('stat-ontime', globalPerf.onTime.toLocaleString());
    updateText('stat-ontime-today', todayPerf.onTime.toLocaleString());

    updateText('stat-delayed', globalPerf.delayed.toLocaleString());
    updateText('stat-delayed-today', todayPerf.delayed.toLocaleString());

    // Averages (3-Month vs Today)
    updateText('stat-duration', `${calcAvg(threeMonthData, 'duration').toFixed(0)}m`);
    updateText('stat-duration-today', `${calcAvg(todayData, 'duration').toFixed(0)}m`);

    updateText('stat-distance', `${calcAvg(threeMonthData, 'distance').toFixed(1)}km`);
    updateText('stat-distance-today', `${calcAvg(todayData, 'distance').toFixed(1)}km`);

    return todayStr;
}
async function loadDashboard() {
    // Parallel fetch for server time and orders to save time
    const [timeResult, orderResult] = await Promise.all([
        fetchData("/serverTime"),
        fetchData("/getCompletedOrders")
    ]);

    if (!orderResult.isSuccessful) return;

    const data = orderResult.data;

    // Process the reference date from the helper result
    const refDate = timeResult.serverLocalNow
        ? new Date(timeResult.serverLocalNow)
        : new Date();
    const refDateStr = refDate.toDateString();

    // 1. Update Text KPIs
    updateStats(data, refDate);

    // 2. Prepare Datasets (Consistent Mapping)
    const ds = {
        weekly: Processors.weekly(data, refDate),
        today: Processors.todayHourly(data, refDateStr),
        daily30: Processors.daily30(data),
        payment: Processors.payments(data),
        monthly: Processors.monthly(data),
        partner: Processors.partners(data),
        peaks: Processors.globalPeaks(data)
    };

    // 3. Render Charts
    renderChart("weeklyPerformanceChart", "line", ds.weekly.labels, ds.weekly.values, "Weekly Orders", COLORS.primary);
    renderChart("hourlyTraficPtterToday", "bar", ds.today.labels, ds.today.values, "Today's Orders", COLORS.info);
    renderChart("dailyOrdersChart", "line", ds.daily30.labels, ds.daily30.values, "Daily Total", COLORS.primary);
    renderChart("paymentMethodChart", "doughnut", ds.payment.labels, ds.payment.values, "Methods", null, true);
    renderChart("monthlyTrendChart", "line", ds.monthly.labels, ds.monthly.values, "Monthly Total", COLORS.warning);
    renderChart("companyRevenueChart", "bar", ds.partner.labels, ds.partner.values, "Revenue", COLORS.success);
    renderChart("peakHourChart", "bar", ds.peaks.labels, ds.peaks.values, "Global Trend", COLORS.purple);
}

async function fetchData(url) {
    try {
        const res = await fetch(url);
        if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
        return await res.json();
    } catch (e) {
        console.error(`Fetch failed for ${url}:`, e);
        return { isSuccessful: false, data: null };
    }
}

document.addEventListener("DOMContentLoaded", () => { loadDashboard(); setInterval(loadDashboard, 300000); });