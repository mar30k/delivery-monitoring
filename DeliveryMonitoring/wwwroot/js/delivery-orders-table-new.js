const { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } = await import(`./table-utils.js?v=${Date.now()}`);

export const DeliveryOrdersTable = (function () {

    const columns = [
        'VOUCHER',
        'COMPANY',
        'BRANCH',
        'FIRST_NAME',
        'PHONE',
        'REQUEST_DATE',
        'DISTANCE',
        'DURATION',
        'ETA',
        'ETA_DIFFERENCE',
        'DRIVER_NAME',
        'DRIVER_PHONE',
        'IS_FREELANCE',
        'SUPERVISOR',
        'TOTAL_AMOUNT',
        'PAYMENT_METHOD',
        'ACTIVITY',
        'DETAILS',
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
                COL_INDEX.PAYMENT_METHOD,
                COL_INDEX.ACTIVITY,
                COL_INDEX.DETAILS,
            ],
            floatCols: [COL_INDEX.TOTAL_AMOUNT, COL_INDEX.DISTANCE],
            intCols: [],
            avgCols: []
        };

        const columns = [
            center({ data: "voucherCode", render: Renderers.voucherCode }),
            center({ data: "companyName" }),
            center({ data: "branchName" }),
            center({ data: "firstName" }),
            center({ data: "phoneNumber", render: Renderers.phone }),
            center({
                data: "requestCreatedAtString",
                ...Renderers.dateRenderer("requestCreatedAtString", "requestCreatedAt")
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
            center({ data: "etaDifference", render: Renderers.timeDeviationRenderer }),
            center({ data: "assignedDriverName", render: Renderers.orDefault }),
            center({ data: "assignedDriverPhoneNumber", render: Renderers.phone }),
            center({
                data: row => row.isDriverFreelnace ? 'Yes' : 'No',
                render: (data) => Renderers.booleanYesNo(data)
            }),
            center({ data: "supervisorName", render: Renderers.orDefault }),
            center({ data: "totalAmount", render: Renderers.amount }),
            center({ data: "paymentMethod", render: Renderers.orDefault }),
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

        const headerFilterColumns = [
            { index: COL_INDEX.COMPANY, name: "Company" },
            { index: COL_INDEX.BRANCH, name: "Branch" },
            { index: COL_INDEX.FIRST_NAME, name: "Customer" },
            { index: COL_INDEX.PAYMENT_METHOD, name: "Payment Method" },
            { index: COL_INDEX.DRIVER_NAME, name: "Driver" },
            { index: COL_INDEX.IS_FREELANCE, name: "Is Freelance" },
        ];

        return Object.assign({}, baseConfig, {
            columns,
            headerFilterColumns
        });
    }

    async function init({ tableId, ajaxUrl, userType, sheetName }) {
        console.log(tableId, ajaxUrl, userType, sheetName)
        const tableSelector = `#${tableId}`;
        const config = getOrdersTableConfig();

        const dateRange = DateRange.create("#dateRange");
        await dateRange.init();

        const table = initTable({
            ...config,
            tableSelector,
            ajaxUrl,
            dateRange,
            reloadOn: "#dateRange",
            emptyTableMessage: "No Orders Available.",
            onDataLoaded: (json) => {
                fetchAlerts(json);
            }
        });

        startTableAutoRefresh(
            [{ table, range: () => dateRange.getRange() }],
            10000
        );

        var excludeColumns = [COL_INDEX.DETAILS, COL_INDEX.ACTIVITY];
        bindExportButton(
            tableSelector,
            userType,
            sheetName,
            dateRange,
            [],
            excludeColumns
        );
        return {
            table,
            dateRange
        };
    }

    return { init };

})();
