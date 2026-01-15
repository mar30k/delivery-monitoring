const { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } = await import(`./table-utils.js?v=${Date.now()}`);
export const PendingOrdersTable = (function () {

    const COLUMNS = [
        'VOUCHER',
        'COMPANY',
        'BRANCH',
        'STATUS',
        'FIRST_NAME',
        'PHONE',
        'REQUEST_DATE',
        'PAYMENT_METHOD',
        'DISTANCE',
        'DURATION',
        'ETA',
        'DRIVER_PHONE',
        'SUPERVISOR',
        'TOTAL_AMOUNT',
        'ACTIONS',
    ];
    const COL_INDEX = Object.fromEntries(
        COLUMNS.map((name, index) => [name, index])
    );
    function getPendingOrdersTable() {
        const baseConfig = {
            orderingColumn: [COL_INDEX.REQUEST_DATE, "desc"],
            headerFilterColumns: [],
            nonOrderableTargets: [COL_INDEX.VOUCHER],
            columns: [],
            floatCols: [COL_INDEX.DISTANCE, COL_INDEX.TOTAL_AMOUNT],
            intCols: [],
            avgCols: []
        };

        const columns = [
            center({ data: "voucherCode", render: Renderers.voucherCode }),
            center({ data: "companyName" }),
            center({ data: "branchName" }),
            center({ data: "status" }),
            center({ data: "firstName" }),
            center({ data: "phoneNumber", render: Renderers.phone }),
            center({
                data: "requestCreatedAtString",
                ...Renderers.dateRenderer("requestCreatedAtString", "requestCreatedAt")
            }),
            center({
                data: "paymentMethod",
                render: Renderers.orDefault
            }),
            center({
                data: "distance",
                render: (d, type) => d != null ? `${Renderers.number(d, type, 2)} km` : "-"
            }),
            center({
                data: "duration",
                render: (d, type) => d != null ? `${Renderers.number(d, type, 2)} min` : "-"
            }),
            center({
                data: "eta",
                render: (d, type) => d != null ? `${Renderers.number(d, type, 2)} min` : "-"
            }),
            center({ data: "driverPhoneNumber", render: Renderers.phone }),
            center({ data: "supervisorName", render: Renderers.orDefault }),
            center({ data: "totalAmount", render: Renderers.amount }),
            center({ data: null, render: (d, t, r) => Renderers.completePendingOrder(r) })
        ];

        const headerFilterColumns = [
            { index: COL_INDEX.COMPANY, name: 'Company' },
            { index: COL_INDEX.BRANCH, name: 'Branch' },
            { index: COL_INDEX.SUPERVISOR, name: 'Supervisor' },
            { index: COL_INDEX.PAYMENT_METHOD, name: 'Payment Method' }
        ];

        const nonOrderableTargets = [
            COL_INDEX.VOUCHER, COL_INDEX.PHONE,
            COL_INDEX.ACTIONS, COL_INDEX.DRIVER_PHONE
        ];

        return Object.assign({}, baseConfig, {
            columns,
            headerFilterColumns,
            nonOrderableTargets
        });
    }

    async function init({ tableId, ajaxUrl, userType, sheetName }) {
        const tableSelector = `#${tableId}`;
        const config = getPendingOrdersTable(tableId);

        const dateRange = DateRange.create("#dateRange");
        await dateRange.init();
        const table = initTable({
            ...config,
            tableSelector,
            ajaxUrl,
            dateRange,
            reloadOn: "#dateRange",
            emptyTableMessage: "No Pending Orders Available."
        });

        const tableEntry = {
            table: table,
            range: () => dateRange.getRange()
        }
        startTableAutoRefresh([tableEntry], 60000);
        bindExportButton(
            tableSelector,
            userType,
            sheetName,
            dateRange
        );

        return {
            table,
            dateRange
        };
    }

    return { init };

})();