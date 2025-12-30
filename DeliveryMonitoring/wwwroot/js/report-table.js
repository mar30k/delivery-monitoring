const { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } = await import(`./table-utils.js?v=${Date.now()}`);
export const ReportTable = (function () {

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
        'DRIVER_PHONE',
        'SUPERVISOR',
        'TOTAL_AMOUNT',
        'TIP',
        'PURPOSE',
        'NOTE',
        'REVIEW',
        'RATING',
        'TYPE',
    ];

    const COL_INDEX = Object.fromEntries(
        columns.map((name, index) => [name, index])
    );

    // Default report column widths
    const BASE_COLUMN_WIDTHS = [
        { wch: 20 }, { wch: 25 }, { wch: 25 }, { wch: 20 }, { wch: 15 }, { wch: 22 },
        { wch: 10 }, { wch: 10 }, { wch: 10 }, { wch: 20 }, { wch: 20 }, { wch: 12 },
        { wch: 10 }, { wch: 20 }, { wch: 30 }, { wch: 30 }, { wch: 10 }
    ];

    function getReportTableConfig(tableId) {
        const isAllOrders = tableId === "allOrders";

        const baseConfig = {
            orderingColumn: [COL_INDEX.REQUEST_DATE, "desc"],
            headerFilterColumns: [],
            nonOrderableTargets: [COL_INDEX.VOUCHER],
            columns: [],
            floatCols: [COL_INDEX.DISTANCE, COL_INDEX.TOTAL_AMOUNT, COL_INDEX.TIP],
            intCols: [],
            avgCols: [{ index: COL_INDEX.RATING, includeZeros: false }]
        };

        const columns = [
            center({ data: "voucherCode", render: Renderers.voucherCode }),
            center({ data: "companyName" }),
            center({ data: "branchName" }),
            center({ data: "firstName" }),
            center({ data: "phoneNumber", render: Renderers.phone }),
            center({
                data: "requestCreatedAt",
                render: Renderers.requestDate.render,
                createdCell: Renderers.requestDate.createdCell
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
            center({
                data: "tip",
                render: (d, type) => d != null ? Renderers.number(d, type, 2, false) : "-"
            }),
            center({ data: "purpose", render: Renderers.orDefault }),
            center({ data: "note", render: Renderers.expandableText }),
            center({ data: "review", render: Renderers.expandableText }),
            center({ data: "rating", render: Renderers.rating })
        ];

        if (isAllOrders) {
            columns.push(center({ data: "tableId" }));
        }

        const headerFilterColumns = [
            { index: COL_INDEX.COMPANY, name: 'Company' },
            { index: COL_INDEX.BRANCH, name: 'Branch' },
            { index: COL_INDEX.SUPERVISOR, name: 'Supervisor' }
        ];

        if (isAllOrders) {
            headerFilterColumns.push({ index: COL_INDEX.TYPE, name: 'Type' });
        }

        const nonOrderableTargets = [
            COL_INDEX.VOUCHER, COL_INDEX.PHONE, COL_INDEX.DRIVER_PHONE,
            COL_INDEX.PURPOSE, COL_INDEX.NOTE, COL_INDEX.REVIEW
        ];

        return Object.assign({}, baseConfig, {
            columns,
            headerFilterColumns,
            nonOrderableTargets
        });
    }

    function init({ tableId, ajaxUrl, userType, sheetName }) {
        const tableSelector = `#${tableId}`;
        const config = getReportTableConfig(tableId);

        const dateRange = DateRange.create("#dateRange");
        dateRange.init();

        const table = initTable({
            ...config,
            tableSelector,
            ajaxUrl,
            dateRange: dateRange,
            reloadOn: "#dateRange",
            emptyTableMessage: "No Report Available."
        });

        const tableEntry = {
            table: table,
            range: () => dateRange.getRange()
        };

        startTableAutoRefresh([tableEntry], 60000);

        const columnWidths = [...BASE_COLUMN_WIDTHS];
        if (tableId === "allOrders") {
            columnWidths.push({ wch: 15 }); // for tableId column
        }

        bindExportButton(
            tableSelector,
            userType,
            sheetName,
            dateRange,
            columnWidths
        );
    }

    return { init };

})();