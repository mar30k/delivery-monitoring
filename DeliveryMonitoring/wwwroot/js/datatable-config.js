var startDate = moment().startOf('day');
var endDate = moment().endOf('day');

//#region User Types Constants

const USER_TYPE = {
    MERCHANT: "merchant",
    CONSIGNEE: "consignee",
    DRIVER: "driver",
    SUPERVISOR: "supervisor"
};
//#endregion

// #region excel export helper
function bindExportButton(tableSelector, typePrefix, sheetName, columnWidths= []) {
    $("#exportToExcelBtn").on("click", () => {
        exportTableToExcel({ tableSelector, typePrefix, sheetName, startDate, endDate, columnWidths });
    });
}
//#endregion

// #region Summary Table Config
function getSummaryTableConfig(userType) {
    const baseConfig = { orderingColumn: [1, "asc"], headerFilterColumns: [], nonOrderableTargets: [0], columns: [], floatCols: [], intCols: [], avgCols: [] };
    const configs = {
        [USER_TYPE.MERCHANT]: {
            orderingColumn: [2, "asc"],
            columns: [
                { data: "tin", className: "text-center" },
                { data: "companyName", className: "text-center" },
                { data: "branchName", className: "text-center" },
                { data: "totalDineInOrders", className: "text-center" },
                { data: "dineInAmount", className: "text-center", render: Renderers.amount },
                { data: "totalTakeAwayOrders", className: "text-center" },
                { data: "takeawayAmount", className: "text-center", render: Renderers.amount },
                { data: "totalDeliveryOrders", className: "text-center" },
                { data: "deliveryAmount", className: "text-center", render: Renderers.amount },
                { data: "grandTotal", className: "text-center", render: Renderers.amount },
                { data: "totalConsigneeCount", className: "text-center" }
            ],
            intCols: [3, 5, 7, 10],
            floatCols: [4, 6, 8, 9],
            headerFilterColumns: [{ index: 1, name: 'Company' }, { index: 2, name: 'Branch' }]
        },
        [USER_TYPE.CONSIGNEE]: {
            columns: [
                { data: "phoneNumber", className: "text-center", render: Renderers.phone },
                { data: "name", className: "text-center" },
                { data: "totalDineInOrders", className: "text-center" },
                { data: "dineInAmount", className: "text-center", render: Renderers.amount },
                { data: "totalTakeAwayOrders", className: "text-center" },
                { data: "takeawayAmount", className: "text-center", render: Renderers.amount },
                { data: "totalDeliveryOrders", className: "text-center" },
                { data: "deliveryAmount", className: "text-center", render: Renderers.amount },
                { data: "grandTotal", className: "text-center", render: Renderers.amount },
                { data: "totalMerchantCount", className: "text-center" }
            ],
            intCols: [2, 4, 6, 9],
            floatCols: [3, 5, 7, 8],
            headerFilterColumns: [{ index: 1, name: 'Name' }]
        },
        [USER_TYPE.DRIVER]: {
            columns: [
                { data: "driverPhoneNumber", className: "text-center", render: Renderers.phone },
                { data: "name", className: "text-center" },
                { data: "totalDeliveryOrders", className: "text-center" },
                { data: "deliveryAmount", className: "text-center", render: Renderers.amount },
                { data: "totalDistance", className: "text-center", render: Renderers.distance },
                { data: "totalEtaDifference", className: "text-center", render: Renderers.timeDeviationRenderer },
                { data: "averageRating", className: "text-center", render: Renderers.rating },
                { data: "tip", className: "text-center", render: (d, type) => Renderers.number(d, type, 2, false)},
                { data: "totalConsigneeCount", className: "text-center" },
                { data: "totalMerchantCount", className: "text-center" }
            ],
            intCols: [2],
            floatCols: [3, 4, 5, 7],
            avgCols: [{ index: 6, includeZeros: false }],
            headerFilterColumns: [{ index: 1, name: 'Name' }]
        },
        [USER_TYPE.SUPERVISOR]: {
            columns: [
                { data: "supervisorPhoneNumber", className: "text-center", render: Renderers.phone },
                { data: "supervisorName", className: "text-center" },
                { data: "totalDeliveryOrders", className: "text-center" },
                { data: "deliveryAmount", className: "text-center", render: Renderers.amount },
                {
                    data: "purposeSummary",
                    className: "text-center",
                    render: Renderers.purposeSummary
                },
                { data: "totalConsigneeCount", className: "text-center" },
                { data: "totalMerchantCount", className: "text-center" }
            ],
            intCols: [2],
            floatCols: [3],
            nonOrderableTargets: [0, 4],
            headerFilterColumns: [{ index: 1, name: 'Name' }]
        }
    };
    return { ...baseConfig, ...configs[userType] };
}
// #endregion

// #region Report Table Config
function getReportTableConfig(tableId) {
    const baseConfig = { orderingColumn: [5, "desc"], headerFilterColumns: [], nonOrderableTargets: [0], columns: [], floatCols: [], intCols: [], avgCols: [] };
    const isAllOrders = tableId === "allOrders";

    const columns = [
        { data: "voucherCode", className: "text-center", render: Renderers.voucherCode},
        { data: "companyName", className: "text-center" },
        { data: "branchName", className: "text-center" },
        { data: "firstName", className: "text-center" },
        { data: "phoneNumber", className: "text-center", render: Renderers.phone },
        {
            data: "requestCreatedAt",
            className: "text-center",
            render: Renderers.requestDate.render,
            createdCell: Renderers.requestDate.createdCell
        },
        {
            data: "distance",
            className: "text-center",
            render: (d, type) => Renderers.number(d, type, 2) + ' km'
        },
        {
            data: "duration",
            className: "text-center",
            render: (d, type) => Renderers.number(d, type, 2) + ' min'
        },
        {
            data: "eta",
            className: "text-center",
            render: (d, type) => Renderers.number(d, type, 2) + ' min'
        },
        { data: "driverPhoneNumber", className: "text-center", render: Renderers.phone },
        { data: "supervisorName", className: "text-center", render: Renderers.orDefault},
        { data: "totalAmount", className: "text-center", render: Renderers.amount },
        { data: "tip", className: "text-center", render: (d, type) => Renderers.number(d, type, 2, false) },
        {
            data: "purpose", className: "text-center", render: Renderers.orDefault
        },
        {
            data: "note",
            className: "text-center",
            render: Renderers.expandableText
        },
        {
            data: "review", className: "text-center", render: Renderers.orDefault            
        },
        { data: "rating", className: "text-center" , render: Renderers.rating }
    ];

    if (isAllOrders) {
        columns.push({ data: "tableId", className: "text-center" });
    }

    const headerFilterColumns = [
        { index: 1, name: 'Company' },
        { index: 2, name: 'Branch' },
        { index: 10, name: 'Supervisor' }
    ];

    if (isAllOrders) {
        headerFilterColumns.push({ index: columns.length - 1, name: 'Type' });
    }

    return { ...baseConfig, columns, floatCols: [6, 11, 12], avgCols: [{ index: 16, includeZeros: false }], headerFilterColumns, nonOrderableTargets: [0, 4, 9, 13, 14, 15] };
}
// #endregion

//#region Table Initialization Functions
function initSummaryTableWrapper({ tableId, ajaxUrl, userType, sheetName }) {
    const tableSelector = `#${tableId}`;
    const config = getSummaryTableConfig(userType);
    config.tableSelector = tableSelector;
    config.ajaxUrl = ajaxUrl;

    initTable(config);
    bindExportButton(tableSelector, `${userType}_Summary`, sheetName);
}

function initReportTableWrapper({ tableId, ajaxUrl, userType, sheetName }) {
    const tableSelector = `#${tableId}`;
    const config = getReportTableConfig(tableId);
    config.tableSelector = tableSelector;
    config.ajaxUrl = ajaxUrl;

    initTable(config);

    const reportColumnWidths = [
        { wch: 20 }, { wch: 25 }, { wch: 25 }, { wch: 20 }, { wch: 15 }, { wch: 22 },
        { wch: 10 }, { wch: 10 }, { wch: 10 }, { wch: 20 }, { wch: 20 }, { wch: 12 },
        { wch: 10 }, { wch: 20 }, { wch: 30 }, { wch: 30 }, { wch: 10 }
    ];

    bindExportButton(tableSelector, userType, sheetName, reportColumnWidths);
}
//#endregion