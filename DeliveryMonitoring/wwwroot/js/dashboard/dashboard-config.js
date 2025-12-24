export const DASHBOARD_CONFIG = {
    refresh: {
        orders: 10_000,
        drivers: 30_000,
        charts: 60_000
    },
    api: {
        driverLive: '/driver/liveLocation',
        orderChart: '/getChartData',
        kotStatus: '/getDeviceControl',
        supervisors: '/getAvailableSupervisors',
        orders: '/getorders'
    },
    colors: {
        drivers: [
            '#28a745', '#dc3545', 'seagreen', 'darkorange',
            '#20c997', 'coral', '#F7BEA2'
        ],
        orderTypes: ['#17a2b8', '#007bff', '#ffc107', 'lawngreen', '#F7BEA2'],
        orderStatus: [
            'green', 'deepskyblue', 'lawngreen', 'seagreen',
            'darkorange', 'red', 'firebrick',
            'coral', '#F7BEA2', 'darkred'
        ]
    },
    googleMapsKey: 'AIzaSyAA8U3kqWJt2stT_CX_r8md8FKsj0-rmiQ'
};
