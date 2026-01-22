export const DashboardCharts = (function () {

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

    function updateDataset(chart, data, labels) {
        chart.data.datasets[0].data = data;
        if (labels) chart.data.labels = labels;
        chart.update();
    }

    return {
        createDoughnut,
        updateDataset
    };
})();
