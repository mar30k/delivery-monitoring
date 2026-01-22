const COLORS = {
    primary: '#4e73df', success: '#1cc88a', info: '#36b9cc', warning: '#f6c23e', danger: '#e74a3b', purple: '#6f42c1'
};

const chartInstances = {};

const Processors = {
    // Chart 1: Today's Traffic Only
    todayHourly: (data) => {
        const todayStr = new Date().toDateString();
        const hours = data.reduce((acc, o) => {
            const orderDate = new Date(o.requestCreatedAt);
            if (orderDate.toDateString() === todayStr) {
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
    }
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

function updateStats(data) {
    const cutoff = new Date(); cutoff.setMonth(cutoff.getMonth() - 3);
    const window = data.filter(o => new Date(o.requestCreatedAt) >= cutoff);
    const totalRev = data.reduce((sum, o) => sum + (o.totalAmount || 0), 0);
    const validDur = window.filter(d => parseFloat(d.duration) > 0.1);
    const avgDur = validDur.length ? (validDur.reduce((s, o) => s + parseFloat(o.duration), 0) / validDur.length) : 0;
    const validDist = window.filter(d => parseFloat(d.distance) > 0);
    const avgDist = validDist.length ? (validDist.reduce((s, o) => s + parseFloat(o.distance), 0) / validDist.length) : 0;

    document.getElementById('stat-revenue').innerText = `ETB ${totalRev.toLocaleString()}`;
    document.getElementById('stat-count').innerText = data.length;
    document.getElementById('stat-duration').innerText = `${avgDur.toFixed(1)} min`;
    document.getElementById('stat-distance').innerText = `${avgDist.toFixed(2)} km`;
}

async function loadDashboard() {
    try {
        const res = await fetch("/getCompletedOrders");
        const result = await res.json();
        if (!result.isSuccessful) return;
        const data = result.data;

        updateStats(data);
        renderChart("hourlyTraficPtterToday", "bar", Processors.todayHourly(data).labels, Processors.todayHourly(data).values, "Today's Orders", COLORS.info);
        renderChart("dailyOrdersChart", "line", Processors.daily30(data).labels, Processors.daily30(data).values, "Daily Total", COLORS.primary);
        renderChart("paymentMethodChart", "doughnut", Processors.payments(data).labels, Processors.payments(data).values, "Methods", null, true);
        renderChart("monthlyTrendChart", "line", Processors.monthly(data).labels, Processors.monthly(data).values, "Monthly Total", COLORS.warning);
        renderChart("companyRevenueChart", "bar", Processors.partners(data).labels, Processors.partners(data).values, "Revenue", COLORS.success);
        renderChart("peakHourChart", "bar", Processors.globalPeaks(data).labels, Processors.globalPeaks(data).values, "Global Trend", COLORS.purple);

    } catch (e) { console.error(e); }
}

document.addEventListener("DOMContentLoaded", () => { loadDashboard(); setInterval(loadDashboard, 300000); });