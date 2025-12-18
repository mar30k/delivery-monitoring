import { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } from './table-utils.js';
export const ReportTable = (function () {

    // Column indexes for clarity
    const COL_INDEX = {
        VOUCHER: 0,
        COMPANY: 1,
        BRANCH: 2,
        FIRST_NAME: 3,
        PHONE: 4,
        REQUEST_DATE: 5,
        DISTANCE: 6,
        DURATION: 7,
        ETA: 8,
        DRIVER_PHONE: 9,
        SUPERVISOR: 10,
        TOTAL_AMOUNT: 11,
        TIP: 12,
        PURPOSE: 13,
        NOTE: 14,
        REVIEW: 15,
        RATING: 16,
        TYPE: 17 // only for allOrders
    };

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
            center({ data: "review", render: Renderers.orDefault }),
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

        initTable(Object.assign({}, config, {
            tableSelector,
            ajaxUrl
        }));

        const columnWidths = [...BASE_COLUMN_WIDTHS];
        if (tableId === "allOrders") {
            columnWidths.push({ wch: 15 }); // for tableId column
        }

        bindExportButton(
            tableSelector,
            userType,
            sheetName,
            columnWidths
        );
    }

    return { init };

})();