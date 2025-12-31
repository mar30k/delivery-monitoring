const { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } = await import(`./table-utils.js?v=${Date.now()}`);

export const OrdersTable = (function () {

    const columns = [
        'VOUCHER',
        'COMPANY',
        'BRANCH',
        'CUSTOMER',
        'PHONE',
        'ADDRESS',
        'REQUEST_DATE',
        'ETA',
        'STATUS',
        'STATUS_REPORT',
        'PRINTED',
        'DRIVER_PHONE',
        'SUPERVISOR',
        'TOTAL_AMOUNT',
        'PAYMENT_METHOD',
        'ACTIONS'
    ];

    const COL_INDEX = Object.fromEntries(
        columns.map((name, index) => [name, index])
    );

    function getOrdersTableConfig() {

        const baseConfig = {
            orderingColumn: [COL_INDEX.REQUEST_DATE, "desc"],
            headerFilterColumns: [],
            nonOrderableTargets: [
                COL_INDEX.VOUCHER,
                COL_INDEX.PHONE,
                COL_INDEX.DRIVER_PHONE,
                COL_INDEX.ACTIONS
            ],
            floatCols: [COL_INDEX.TOTAL_AMOUNT],
            intCols: [],
            avgCols: []
        };

        const columns = [
            center({ data: "voucherCode", render: Renderers.voucherCode }),

            center({ data: "companyName", render: Renderers.company }),

            center({
                data: "branchName",
                render: Renderers.branch
            }),

            center({
                data: "customerFirstName",
                render: Renderers.customerName
            }),

            center({
                data: "customerPhoneNumber",
                render: Renderers.phone
            }),

            center({
                data: null,
                render: (_, __, row) =>
                    Renderers.address(
                        row.customerGeocodeAddress,
                        row.customerSpecificAddress
                    )
            }),
            center({
                data: "createdAtString",
                ...Renderers.dateRenderer("createdAtString", "createdAt")
            }),
            center({
                data: "eta",
                ...Renderers.dateRenderer("etaString", "eta")
            }),
            center({
                data: "status",
                render: Renderers.status
            }),

            center({
                data: "statusReport",
                render: Renderers.statusReport
            }),

            center({
                data: "orderPrinted",
                render: Renderers.booleanYesNo
            }),

            center({
                data: "assignedDriverPhoneNumber",
                render: Renderers.phone
            }),

            center({
                data: null,
                render: (_, __, row) =>
                    Renderers.assign(row, row.supervisor)
            }),

            center({
                data: "grandTotal",
                render: Renderers.amount
            }),

            center({
                data: "paymentMethod",
                render: Renderers.paymentMethod
            }),

            center({
                data: null,
                render: (_, __, row) =>
                    Renderers.detailsActions(row, row.redispatch)
            })
        ];

        const headerFilterColumns = [
            { index: COL_INDEX.COMPANY, name: "Company" },
            { index: COL_INDEX.BRANCH, name: "Branch" },
            { index: COL_INDEX.STATUS, name: "Status" },
            { index: COL_INDEX.PRINTED, name: "Order Printed" }
        ];

        return Object.assign({}, baseConfig, {
            columns,
            headerFilterColumns
        });
    }

    function init({ tableId, ajaxUrl, userType, sheetName }) {

        const tableSelector = `#${tableId}`;
        const config = getOrdersTableConfig();

        const dateRange = DateRange.create("#dateRange");
        dateRange.init();

        const table = initTable({
            ...config,
            tableSelector,
            ajaxUrl,
            dateRange,
            reloadOn: "#dateRange",
            emptyTableMessage: "No Orders Available."
        });

        startTableAutoRefresh(
            [{ table, range: () => dateRange.getRange() }],
            10000
        );
    }

    return { init };

})();
