export const DashboardCharts = (function () {
    const chartInstances = {};
    const COLORS = {
        primary: '#4e73df', success: '#1cc88a', info: '#36b9cc', warning: '#f6c23e', danger: '#e74a3b', purple: '#6f42c1'
    };
    function createDoughnut({ ctx, labels, data, colors, title }) {
        return new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{ data, backgroundColor: colors }]
            },
            options: {
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            usePointStyle: true,
                            pointStyle: 'circle',
                            padding: 20,
                            font: {
                                size: 12
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: title,
                        font: {
                            size: 18
                        }
                    }
                }
            }
        });
    }

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
    function updateDataset(chart, data, labels) {
        chart.data.datasets[0].data = data;
        if (labels) chart.data.labels = labels;
        chart.update();
    }

    return {
        createDoughnut,
        renderChart,
        COLORS,
        updateDataset
    };
})();
