window.DashboardCharts = (function () {

    function createDoughnut({ ctx, labels, data, colors, title }) {
        return new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{ data, backgroundColor: colors }]
            },
            options: {
                plugins: {
                    legend: { position: 'bottom' },
                    title: { display: true, text: title }
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
