import { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } from './table-utils.js';
export const SummaryTable = (function () {

    const USER_TYPE = {
        MERCHANT: "merchant",
        CONSIGNEE: "consignee",
        DRIVER: "driver",
        SUPERVISOR: "supervisor"
    };

    // Column indexes per user type
    const COL_INDEX = {
        MERCHANT: {
            TIN: 0,
            COMPANY: 1,
            BRANCH: 2,
            TOTAL_DINEIN_ORDERS: 3,
            DINEIN_AMOUNT: 4,
            TOTAL_TAKEAWAY_ORDERS: 5,
            TAKEAWAY_AMOUNT: 6,
            TOTAL_DELIVERY_ORDERS: 7,
            DELIVERY_AMOUNT: 8,
            GRAND_TOTAL: 9,
            TOTAL_CONSIGNEE_COUNT: 10
        },
        CONSIGNEE: {
            PHONE: 0,
            NAME: 1,
            TOTAL_DINEIN_ORDERS: 2,
            DINEIN_AMOUNT: 3,
            TOTAL_TAKEAWAY_ORDERS: 4,
            TAKEAWAY_AMOUNT: 5,
            TOTAL_DELIVERY_ORDERS: 6,
            DELIVERY_AMOUNT: 7,
            GRAND_TOTAL: 8,
            TOTAL_MERCHANT_COUNT: 9
        },
        DRIVER: {
            PHONE: 0,
            NAME: 1,
            TOTAL_DELIVERY_ORDERS: 2,
            DELIVERY_AMOUNT: 3,
            TOTAL_DISTANCE: 4,
            TOTAL_ETA_DIFFERENCE: 5,
            TIMELY_DELIVERIES: 6,
            LATE_DELIVERIES: 7,
            AVERAGE_RATING: 8,
            TIP: 9,
            TOTAL_CONSIGNEE_COUNT: 10,
            TOTAL_MERCHANT_COUNT: 11
        },
        SUPERVISOR: {
            PHONE: 0,
            NAME: 1,
            TOTAL_DELIVERY_ORDERS: 2,
            DELIVERY_AMOUNT: 3,
            PURPOSE_SUMMARY: 4,
            TOTAL_CONSIGNEE_COUNT: 5,
            TOTAL_MERCHANT_COUNT: 6
        }
    };

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
                    center({ data: "totalDeliveryOrders" }),
                    center({ data: "deliveryAmount", render: Renderers.amount }),
                    center({ data: "grandTotal", render: Renderers.amount }),
                    center({ data: "totalConsigneeCount" })
                ],
                intCols: [
                    COL_INDEX.MERCHANT.TOTAL_DINEIN_ORDERS,
                    COL_INDEX.MERCHANT.TOTAL_TAKEAWAY_ORDERS,
                    COL_INDEX.MERCHANT.TOTAL_DELIVERY_ORDERS,
                    COL_INDEX.MERCHANT.TOTAL_CONSIGNEE_COUNT
                ],
                floatCols: [
                    COL_INDEX.MERCHANT.DINEIN_AMOUNT,
                    COL_INDEX.MERCHANT.TAKEAWAY_AMOUNT,
                    COL_INDEX.MERCHANT.DELIVERY_AMOUNT,
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
                    center({ data: "totalDeliveryOrders" }),
                    center({ data: "deliveryAmount", render: Renderers.amount }),
                    center({ data: "grandTotal", render: Renderers.amount }),
                    center({ data: "totalMerchantCount" })
                ],
                intCols: [
                    COL_INDEX.CONSIGNEE.TOTAL_DINEIN_ORDERS,
                    COL_INDEX.CONSIGNEE.TOTAL_TAKEAWAY_ORDERS,
                    COL_INDEX.CONSIGNEE.TOTAL_DELIVERY_ORDERS,
                    COL_INDEX.CONSIGNEE.TOTAL_MERCHANT_COUNT
                ],
                floatCols: [
                    COL_INDEX.CONSIGNEE.DINEIN_AMOUNT,
                    COL_INDEX.CONSIGNEE.TAKEAWAY_AMOUNT,
                    COL_INDEX.CONSIGNEE.DELIVERY_AMOUNT,
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

    function init({ tableId, ajaxUrl, userType, sheetName }) {
        const tableSelector = `#${tableId}`;
        const config = getSummaryTableConfig(userType);

        initTable(Object.assign({}, config, {
            tableSelector,
            ajaxUrl
        }));

        bindExportButton(
            tableSelector,
            `${userType}_Summary`,
            sheetName,
        );
    }

    return { init };

})();