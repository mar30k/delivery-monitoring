let startDate = moment().startOf('day');
let endDate = moment().endOf('day');

function initPageSummaryTable(options) {
    const {
        tableId,
        ajaxUrl,
        userType,
        sheetName
    } = options;

    const tableSelector = `#${tableId}`;
    const isMerchant = userType === "merchant";
    const isConsignee = userType === "consignee";
    const isDriver = userType === "driver";
    const isSupervisor = userType === "supervisor";

    const config = {
        tableSelector,
        ajaxUrl,
        orderingColumn: isMerchant ? [2, "asc"] : [1, "asc"],
        headerFilterColumns: [],
        nonOrderableTargets: [0],
        columns: [],
        floatCols: [],
        intCols: [],
        avgCols: [],
    };

    if (isMerchant) {
        config.columns = [
            { data: "tin", className: "text-center" },
            { data: "companyName", className: "text-center" },
            { data: "branchName", className: "text-center" },
            { data: "totalDineInOrders", className: "text-center" },
            {
                data: "dineInAmount",
                className: "text-center",
                render: numberRenderer
            },
            { data: "totalTakeAwayOrders", className: "text-center" },
            {
                data: "takeawayAmount",
                className: "text-center",
                render: numberRenderer
            },
            { data: "totalDeliveryOrders", className: "text-center" },
            {
                data: "deliveryAmount",
                className: "text-center",
                render: numberRenderer
            },
            {
                data: "grandTotal",
                className: "text-center",
                render: numberRenderer
            },
            { data: "totalConsigneeCount", className: "text-center" }
        ];

        config.intCols = [3, 5, 7, 10];
        config.floatCols = [4, 6, 8, 9];
        config.headerFilterColumns = [
            { index: 1, name: 'Company Name' },
            { index: 2, name: 'Branch Name' }
        ];
    } else if (isConsignee) {
        config.columns = [
            { data: "phoneNumber", className: "text-center" },
            { data: "name", className: "text-center" },
            { data: "totalDineInOrders", className: "text-center" },
            { data: "dineInAmount", className: "text-center", render: numberRenderer },
            { data: "totalTakeAwayOrders", className: "text-center" },
            { data: "takeawayAmount", className: "text-center", render: numberRenderer },
            { data: "totalDeliveryOrders", className: "text-center" },
            { data: "deliveryAmount", className: "text-center", render: numberRenderer },
            { data: "grandTotal", className: "text-center", render: numberRenderer },
            { data: "totalMerchantCount", className: "text-center" }
        ];

        config.intCols = [2, 4, 6, 9];
        config.floatCols = [3, 5, 7, 8];
        config.headerFilterColumns = [{ index: 1, name: 'Name' }];
    }
    else if (isDriver) {
        config.columns = [
            { data: "driverPhoneNumber", className: "text-center" },
            { data: "name", className: "text-center" },
            { data: "totalDeliveryOrders", className: "text-center" },
            { data: "deliveryAmount", className: "text-center", render: numberRenderer },
            {
                data: "totalDistance",
                className: "text-center",
                render: (d, type) => type === 'sort' || type === 'type'
                    ? parseFloat(d) || 0
                    : (parseFloat(d) || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " km"
            },
            {
                data: "totalTimeDeviation",
                className: "text-center",
                render: deviationRenderer
            },
            {
                data: "averageRating",
                className: "text-center",
                render: ratingRenderer
            },
            { data: "tip", className: "text-center", render: numberRenderer },
            { data: "totalConsigneeCount", className: "text-center" },
            { data: "totalMerchantCount", className: "text-center" }
        ];

        config.intCols = [2];
        config.floatCols = [3, 4, 5, 7];
        config.avgCols = [{ index: 6, includeZeros: false }];
        config.headerFilterColumns = [{ index: 1, name: 'Name' }];
    }
    else if (isSupervisor) {
        config.columns = [
            { data: "supervisorPhoneNumber", className: "text-center" },
            { data: "supervisorName", className: "text-center" },
            { data: "totalDeliveryOrders", className: "text-center" },
            { data: "deliveryAmount", className: "text-center", render: numberRenderer },
            { data: "purposeSummary", className: "text-center" },
            { data: "totalConsigneeCount", className: "text-center" },
            { data: "totalMerchantCount", className: "text-center" }
        ];

        config.intCols = [2];
        config.floatCols = [3];
        config.nonOrderableTargets = [4];
        config.headerFilterColumns = [{ index: 1, name: 'Name' }];
    }

    initSummaryTable(config);

    $("#exportToExcelBtn").on("click", function () {
        exportTableToExcel({
            tableSelector,
            typePrefix: `${userType}_Summary`,
            sheetName,
            startDate,
            endDate
        });
    });
}

/* ---------- Shared renderers ---------- */
function numberRenderer(d, type) {
    if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
    return (!d ? 0 : parseFloat(d)).toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function deviationRenderer(d, type) {
    if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
    var value = parseFloat(d) || 0;
    var color = value > 0 ? "green" : value < 0 ? "red" : "gray";
    var formatted = value.toFixed(2);
    return `<span style="color:${color}; font-weight:600;">${formatted}</span>`;
}

function ratingRenderer(d, type) {
    if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
    var value = parseFloat(d) || 0;
    if (value === 0) return value;
    var hue = ((value - 1) / 4) * 120;
    var color = `hsl(${hue}, 70%, 40%)`;
    return `<span style="color:${color}; font-weight:600;">${value.toFixed(2)}</span>`;
}