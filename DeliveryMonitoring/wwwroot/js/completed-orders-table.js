const { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } = await import(`./table-utils.js?v=${Date.now()}`);
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
    'BUSINESS_UNIT',
    'FIRST_NAME',
    'PHONE',
    'REQUEST_DATE',
    'PAYMENT_METHOD'
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

const baseColumns = [
    center({ data: "voucherCode", render: Renderers.voucherCode }),
    center({ data: "companyName" }),
    center({ data: "branchName" }),
    center({ data: "businessUnit" }),
    center({ data: "firstName" }),
    center({ data: "phoneNumber", render: Renderers.phone }),
    center({
        data: "requestCreatedAtString",
        ...Renderers.dateRenderer("requestCreatedAtString", "requestCreatedAt")
    }),
    center({
        data: "paymentMethod",
        render: Renderers.orDefault
    })
];

const dineInAndTakeawayColumns = [
    ...baseColumns,
    center({ data: "supervisorName" }),
    center({ data: "totalAmount", render: Renderers.amount }),

    center({
        data: null,
        orderable: false,
        render: (d, t, r) => Renderers.reviewOrShow(r, false)
    }),

    center({
        data: null,
        orderable: false,
        render: (d, t, r) => Renderers.activityBtn(r)
    }),

    center({
        data: null,
        orderable: false,
        render: (d, t, r) => Renderers.detailsLink(r)
    })
];

const deliveryColumns = [
    ...baseColumns,

    center({ data: "distance", render: Renderers.distance }),
    center({ data: "duration", render: Renderers.duration }),
    center({ data: "eta", render: Renderers.duration }),
    center({ data: "etaDifference", render: Renderers.timeDeviationRenderer }),

    center({
        data: function (row) {
            return row.assignedDriverName ?? row.driverPhoneNumber ?? 'N/A';
        },
        render: (data, type, row) => Renderers.driver(type, row.driverPhoneNumber, row.assignedDriverName)
    }),
    center({ data: "supervisorName", render: (d, t, r) => Renderers.supervisor( r, t) }),
    center({ data: "totalAmount", render: Renderers.amount }),
    center({ data: "tip", render: Renderers.amount }),

    center({
        data: null,
        orderable: false,
        render: (d, t, r) => Renderers.reviewOrShow(r, true)
    }),

    center({
        data: null,
        orderable: false,
        render: (d, t, r) => Renderers.activityBtn(r)
    }),

    center({
        data: null,
        orderable: false,
        render: (d, t, r) => Renderers.detailsLink(r)
    })
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
            { index: DELIVERY_COL_INDEX.SUPERVISOR, name: 'Supervisor' },
            { index: DELIVERY_COL_INDEX.PAYMENT_METHOD, name: 'Payment Method' },
            { index: DELIVERY_COL_INDEX.DRIVER_PHONE, name: 'Driver' }
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

for (const cfg of (TableConfigs || [])) {
    const selector = `#${cfg.TableId}`;
    const datePickerSelector = `#${cfg.TableId}DateRange`;
    const tableConfig = TableTypeConfigs[cfg.SheetName] || TableTypeConfigs["_NonDeliveryOrders"];

    // Create DateRange instance
    const range = DateRange.create(datePickerSelector);
    tableRanges[cfg.TableId] = range;
    await range.init();

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
}

startTableAutoRefresh(tableEntries, 60000);