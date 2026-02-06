const { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } = await import(`./table-utils.js?v=${Date.now()}`);
export const SummaryTable = (function () {

    const USER_TYPE = {
        MERCHANT: "merchant",
        CONSIGNEE: "consignee",
        DRIVER: "driver",
        SUPERVISOR: "supervisor"
    };

    // Column indexes per user type
    const COLUMNS = {
        MERCHANT: [
            'TIN',
            'COMPANY',
            'BRANCH',
            'TOTAL_DINEIN_ORDERS',
            'DINEIN_AMOUNT',
            'TOTAL_TAKEAWAY_ORDERS',
            'TAKEAWAY_AMOUNT',
            'TOTAL_SCHEDULED_TAKEAWAY_ORDERS',
            'SCHEDULED_TAKEAWAY_AMOUNT',
            'TOTAL_DELIVERY_ORDERS',
            'DELIVERY_AMOUNT',
            'TOTAL_SCHEDULED_DELIVERY_ORDERS',
            'SCHEDULED_DELIVERY_AMOUNT',
            'GRAND_TOTAL',
            'TOTAL_CONSIGNEE_COUNT',
        ],

        CONSIGNEE: [
            'PHONE',
            'NAME',
            'TOTAL_DINEIN_ORDERS',
            'DINEIN_AMOUNT',
            'TOTAL_TAKEAWAY_ORDERS',
            'TAKEAWAY_AMOUNT',
            'TOTAL_SCHEDULED_TAKEAWAY_ORDERS',
            'SCHEDULED_TAKEAWAY_AMOUNT',
            'TOTAL_DELIVERY_ORDERS',
            'DELIVERY_AMOUNT',
            'TOTAL_SCHEDULED_DELIVERY_ORDERS',
            'SCHEDULED_DELIVERY_AMOUNT',
            'GRAND_TOTAL',
            'TOTAL_MERCHANT_COUNT',
        ],

        DRIVER: [
            'PHONE',
            'NAME',
            'TOTAL_DELIVERY_ORDERS',
            'DELIVERY_AMOUNT',
            'TOTAL_DISTANCE',
            'TOTAL_ETA_DIFFERENCE',
            'TIMELY_DELIVERIES',
            'LATE_DELIVERIES',
            'AVERAGE_RATING',
            'TIP',
            'TOTAL_CONSIGNEE_COUNT',
            'TOTAL_MERCHANT_COUNT',
        ],

        SUPERVISOR: [
            'PHONE',
            'NAME',
            'TOTAL_DELIVERY_ORDERS',
            'DELIVERY_AMOUNT',
            'PURPOSE_SUMMARY',
            'TOTAL_CONSIGNEE_COUNT',
            'TOTAL_MERCHANT_COUNT',
        ],
    };

    const COL_INDEX = Object.fromEntries(
        Object.entries(COLUMNS).map(([group, cols]) => [
            group,
            Object.fromEntries(cols.map((name, index) => [name, index]))
        ])
    );
    function getSummaryTableConfig(userType) {
        const baseConfig = {
            orderingColumn: [1, "asc"],
            headerFilterColumns: [],
            nonOrderableTargets: [0],
            columns: [],
            floatCols: [],
            intCols: [],
            avgCols: []
        };

        const configs = {
            [USER_TYPE.MERCHANT]: {
                orderingColumn: [COL_INDEX.MERCHANT.BRANCH, "asc"],
                columns: [
                    center({ data: "tin" }),
                    center({ data: "companyName" }),
                    center({ data: "branchName" }),
                    center({ data: "totalDineInOrders" }),
                    center({ data: "dineInAmount", render: Renderers.amount }),
                    center({ data: "totalTakeAwayOrders" }),
                    center({ data: "takeawayAmount", render: Renderers.amount }),
                    center({ data: "totalScheduledTakeawayOrders" }),
                    center({ data: "scheduledTakeawayAmount", render: Renderers.amount }),
                    center({ data: "totalDeliveryOrders" }),
                    center({ data: "deliveryAmount", render: Renderers.amount }),
                    center({ data: "totalScheduledDeliveryOrders" }),
                    center({ data: "scheduledDeliveryAmount", render: Renderers.amount }),
                    center({ data: "grandTotal", render: Renderers.amount }),
                    center({ data: "totalConsigneeCount" })
                ],
                intCols: [
                    COL_INDEX.MERCHANT.TOTAL_DINEIN_ORDERS,
                    COL_INDEX.MERCHANT.TOTAL_TAKEAWAY_ORDERS,
                    COL_INDEX.MERCHANT.TOTAL_DELIVERY_ORDERS,
                    COL_INDEX.MERCHANT.TOTAL_SCHEDULED_DELIVERY_ORDERS,
                    COL_INDEX.MERCHANT.TOTAL_SCHEDULED_TAKEAWAY_ORDERS,
                    COL_INDEX.MERCHANT.TOTAL_CONSIGNEE_COUNT
                ],
                floatCols: [
                    COL_INDEX.MERCHANT.DINEIN_AMOUNT,
                    COL_INDEX.MERCHANT.TAKEAWAY_AMOUNT,
                    COL_INDEX.MERCHANT.DELIVERY_AMOUNT,
                    COL_INDEX.MERCHANT.SCHEDULED_DELIVERY_AMOUNT,
                    COL_INDEX.MERCHANT.SCHEDULED_TAKEAWAY_AMOUNT,
                    COL_INDEX.MERCHANT.GRAND_TOTAL
                ],
                headerFilterColumns: [
                    { index: COL_INDEX.MERCHANT.COMPANY, name: 'Company' },
                    { index: COL_INDEX.MERCHANT.BRANCH, name: 'Branch' }
                ]
            },
            [USER_TYPE.CONSIGNEE]: {
                columns: [
                    center({ data: "phoneNumber", render: Renderers.phone }),
                    center({ data: "name" }),
                    center({ data: "totalDineInOrders" }),
                    center({ data: "dineInAmount", render: Renderers.amount }),
                    center({ data: "totalTakeAwayOrders" }),
                    center({ data: "takeawayAmount", render: Renderers.amount }),
                    center({ data: "totalScheduledTakeawayOrders" }),
                    center({ data: "scheduledTakeawayAmount", render: Renderers.amount }),
                    center({ data: "totalDeliveryOrders" }),
                    center({ data: "deliveryAmount", render: Renderers.amount }),
                    center({ data: "totalScheduledDeliveryOrders" }),
                    center({ data: "scheduledDeliveryAmount", render: Renderers.amount }),
                    center({ data: "grandTotal", render: Renderers.amount }),
                    center({ data: "totalMerchantCount" })
                ],
                intCols: [
                    COL_INDEX.CONSIGNEE.TOTAL_DINEIN_ORDERS,
                    COL_INDEX.CONSIGNEE.TOTAL_TAKEAWAY_ORDERS,
                    COL_INDEX.CONSIGNEE.TOTAL_DELIVERY_ORDERS,
                    COL_INDEX.CONSIGNEE.TOTAL_TAKEAWAY_ORDERS,
                    COL_INDEX.CONSIGNEE.TOTAL_SCHEDULED_TAKEAWAY_ORDERS,
                    COL_INDEX.CONSIGNEE.TOTAL_MERCHANT_COUNT
                ],
                floatCols: [
                    COL_INDEX.CONSIGNEE.DINEIN_AMOUNT,
                    COL_INDEX.CONSIGNEE.TAKEAWAY_AMOUNT,
                    COL_INDEX.CONSIGNEE.DELIVERY_AMOUNT,
                    COL_INDEX.CONSIGNEE.SCHEDULED_DELIVERY_AMOUNT,
                    COL_INDEX.CONSIGNEE.SCHEDULED_TAKEAWAY_AMOUNT,
                    COL_INDEX.CONSIGNEE.GRAND_TOTAL
                ],
                headerFilterColumns: [
                    { index: COL_INDEX.CONSIGNEE.NAME, name: 'Name' }
                ]
            },
            [USER_TYPE.DRIVER]: {
                columns: [
                    center({ data: "driverPhoneNumber", render: Renderers.phone }),
                    center({ data: "name" }),
                    center({ data: "totalDeliveryOrders" }),
                    center({ data: "deliveryAmount", render: Renderers.amount }),
                    center({ data: "totalDistance", render: Renderers.distance }),
                    center({ data: "totalEtaDifference", render: Renderers.timeDeviationRenderer }),
                    center({ data: "timelyDeliveriesCount" }),
                    center({ data: "lateDeliveriesCount" }),
                    center({ data: "averageRating", render: Renderers.rating }),
                    center({
                        data: "tip",
                        render: (d, type) => d != null ? Renderers.number(d, type, 2, false) : "-"
                    }),
                    center({ data: "totalConsigneeCount" }),
                    center({ data: "totalMerchantCount" })
                ],
                intCols: [COL_INDEX.DRIVER.TOTAL_DELIVERY_ORDERS, COL_INDEX.DRIVER.TIMELY_DELIVERIES, COL_INDEX.DRIVER.LATE_DELIVERIES],
                floatCols: [
                    COL_INDEX.DRIVER.DELIVERY_AMOUNT,
                    COL_INDEX.DRIVER.TOTAL_DISTANCE,
                    COL_INDEX.DRIVER.TOTAL_ETA_DIFFERENCE,
                    COL_INDEX.DRIVER.TIP
                ],
                avgCols: [{ index: COL_INDEX.DRIVER.AVERAGE_RATING, includeZeros: false }],
                headerFilterColumns: [
                    { index: COL_INDEX.DRIVER.NAME, name: 'Name' }
                ]
            },
            [USER_TYPE.SUPERVISOR]: {
                columns: [
                    center({ data: "supervisorPhoneNumber", render: Renderers.phone }),
                    center({ data: "supervisorName" }),
                    center({ data: "totalDeliveryOrders" }),
                    center({ data: "deliveryAmount", render: Renderers.amount }),
                    center({ data: "purposeSummary", render: Renderers.purposeSummary }),
                    center({ data: "totalConsigneeCount" }),
                    center({ data: "totalMerchantCount" })
                ],
                intCols: [COL_INDEX.SUPERVISOR.TOTAL_DELIVERY_ORDERS],
                floatCols: [COL_INDEX.SUPERVISOR.DELIVERY_AMOUNT],
                nonOrderableTargets: [COL_INDEX.SUPERVISOR.PHONE, COL_INDEX.SUPERVISOR.PURPOSE_SUMMARY],
                headerFilterColumns: [
                    { index: COL_INDEX.SUPERVISOR.NAME, name: 'Name' }
                ]
            }
        };

        return Object.assign({}, baseConfig, configs[userType]);
    }

    async function init({ tableId, ajaxUrl, userType, sheetName }) {
        const tableSelector = `#${tableId}`;
        const config = getSummaryTableConfig(userType);

        const dateRange = DateRange.create("#dateRange");
        await dateRange.init();


        const table = initTable({
            ...config,
            tableSelector,
            ajaxUrl,
            dateRange,
            reloadOn: "#dateRange"
        });

        const tableEntry = {
            table: table,
            range: () => dateRange.getRange()
        };

        startTableAutoRefresh([tableEntry], 60000);
        bindExportButton(
            tableSelector,
            `${userType}_Summary`,
            sheetName,
            dateRange
        );
    }

    return { init };

})();