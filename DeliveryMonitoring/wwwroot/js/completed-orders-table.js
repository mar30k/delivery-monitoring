/**
 * @typedef TableConfig
 * @type {object}
 * @property {string} Type
 * @property {string} Title
 * @property {string} TableId
 * @property {string} AjaxUrl
 * @property {string} SheetName
 */
const tableRanges = {};
const tables = {};
const tableEntries = [];
const TableConfigs = window.AppData?.tableConfigs || [];

const BASE_COLS = [
    'VOUCHER',
    'COMPANY',
    'BRANCH',
    'FIRST_NAME',
    'PHONE',
    'REQUEST_DATE'
];
const NON_DELIVERY_COLS = [
    ...BASE_COLS,

    'SUPERVISOR',
    'TOTAL_AMOUNT',

    'REVIEW',
    'ACTIVITY',
    'DETAILS'
];

const DELIVERY_COLS = [
    ...BASE_COLS,

    'DISTANCE',
    'DURATION',
    'ETA',
    'ETA_DIFF',
    'DRIVER_PHONE',
    'SUPERVISOR',
    'TOTAL_AMOUNT',
    'TIP',

    'REVIEW',
    'ACTIVITY',
    'DETAILS'
];
const NON_DELIVERY_COL_INDEX = Object.freeze(
    Object.fromEntries(
        NON_DELIVERY_COLS.map((name, index) => [name, index])
    )
);
const DELIVERY_COL_INDEX = Object.freeze(
    Object.fromEntries(
        DELIVERY_COLS.map((name, index) => [name, index])
    )
);

// Column definitions
const baseColumns = [
    { data: "voucherCode", className: "text-center", render: Renderers.voucherCode },
    { data: "companyName", className: "text-center" },
    { data: "branchName", className: "text-center" },
    { data: "firstName", className: "text-center" },
    { data: "phoneNumber", className: "text-center", render: Renderers.phone },
    { data: "requestCreatedAt", className: "text-center", render: Renderers.requestDate.render, createdCell: Renderers.requestDate.createdCell }
];

const dineInAndTakeawayColumns = [
    ...baseColumns,
    { data: "supervisorName", className: "text-center" },
    { data: "totalAmount", className: "text-center", render: Renderers.amount },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.reviewOrShow(r, false) },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.activityBtn(r) },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.detailsLink(r) }
];

const deliveryColumns = [
    ...baseColumns,
    { data: "distance", className: "text-center", render: Renderers.distance },
    { data: "duration", className: "text-center", render: Renderers.duration },
    { data: "eta", className: "text-center", render: Renderers.duration },
    { data: "etaDifference", className: "text-center", render: Renderers.timeDeviationRenderer },
    { data: "driverPhoneNumber", className: "text-center", render: Renderers.phone },
    { data: "supervisorName", className: "text-center", render: Renderers.orDefault },
    { data: "totalAmount", className: "text-center", render: Renderers.amount },
    { data: "tip", className: "text-center", render: Renderers.amount },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.reviewOrShow(r, true) },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.activityBtn(r) },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.detailsLink(r) }
];

const TableTypeConfigs = {
    "_DeliveryOrders": {
        columns: deliveryColumns,

        floatCols: [
            DELIVERY_COL_INDEX.TOTAL_AMOUNT,
            DELIVERY_COL_INDEX.TIP
        ],

        nonOrderableTargets: [
            DELIVERY_COL_INDEX.VOUCHER,
            DELIVERY_COL_INDEX.PHONE,
            DELIVERY_COL_INDEX.DRIVER_PHONE,
            DELIVERY_COL_INDEX.SUPERVISOR,
            DELIVERY_COL_INDEX.REVIEW,
            DELIVERY_COL_INDEX.ACTIVITY,
            DELIVERY_COL_INDEX.DETAILS
        ],

        headerFilterColumns: [
            { index: DELIVERY_COL_INDEX.COMPANY, name: 'Company' },
            { index: DELIVERY_COL_INDEX.BRANCH, name: 'Branch' },
            { index: DELIVERY_COL_INDEX.FIRST_NAME, name: 'Customer' },
            { index: DELIVERY_COL_INDEX.SUPERVISOR, name: 'Supervisor' }
        ]
    },

    "_NonDeliveryOrders": {
        columns: dineInAndTakeawayColumns,

        floatCols: [
            NON_DELIVERY_COL_INDEX.TOTAL_AMOUNT
        ],

        nonOrderableTargets: [
            NON_DELIVERY_COL_INDEX.VOUCHER,
            NON_DELIVERY_COL_INDEX.PHONE,
            NON_DELIVERY_COL_INDEX.REVIEW,
            NON_DELIVERY_COL_INDEX.ACTIVITY,
            NON_DELIVERY_COL_INDEX.DETAILS
        ],

        headerFilterColumns: [
            { index: NON_DELIVERY_COL_INDEX.COMPANY, name: 'Company' },
            { index: NON_DELIVERY_COL_INDEX.BRANCH, name: 'Branch' },
            { index: NON_DELIVERY_COL_INDEX.FIRST_NAME, name: 'Customer' },
            { index: NON_DELIVERY_COL_INDEX.SUPERVISOR, name: 'Supervisor' }
        ]
    }
};

(TableConfigs || []).forEach(cfg => {
    const selector = `#${cfg.TableId}`;
    const datePickerSelector = `#${cfg.TableId}DateRange`;
    const tableConfig = TableTypeConfigs[cfg.SheetName] || TableTypeConfigs["_NonDeliveryOrders"];

    // Create DateRange instance
    const range = DateRange.create(datePickerSelector);
    range.init();
    tableRanges[cfg.TableId] = range;

    // Initialize table
    const table = initTable({
        tableSelector: selector,
        ajaxUrl: cfg.AjaxUrl,
        columns: tableConfig.columns,
        intCols: [],
        avgCols: [],
        headerFilterColumns: tableConfig.headerFilterColumns,
        nonOrderableTargets: tableConfig.nonOrderableTargets,
        dateRange: range,
        reloadOn: datePickerSelector,
        emptyTableMessage: `No ${cfg.Title.toLowerCase()} orders available.`,
        orderingColumn: [5, "desc"],
        floatCols: tableConfig.floatCols
    });

    tables[cfg.TableId] = table;

    // Add to global refresh array
    tableEntries.push({
        table: table,
        range: () => range.getRange()
    });
});

startTableAutoRefresh(tableEntries, 60000);