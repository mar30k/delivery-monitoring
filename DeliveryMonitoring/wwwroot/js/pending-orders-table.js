import { center, safeNumberRenderer, bindExportButton, getBaseTableConfig } from './table-utils.js';
export const PendingOrdersTable = (function () {

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
        ACTIONS: 12,
    };

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
            center({ data: null, render: (d, t, r) => Renderers.completePendingOrder(r) })
        ];

        const headerFilterColumns = [
            { index: COL_INDEX.COMPANY, name: 'Company' },
            { index: COL_INDEX.BRANCH, name: 'Branch' },
            { index: COL_INDEX.SUPERVISOR, name: 'Supervisor' }
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

    function init({ tableId, ajaxUrl, userType, sheetName }) {
        const tableSelector = `#${tableId}`;
        const config = getPendingOrdersTable(tableId);

        DateRange.init("#dateRange");

        initTable({
            ...config,
            tableSelector,
            ajaxUrl,
            ajaxDataHook: DateRange.applyToAjax,
            reloadOn: "#dateRange"
        });

        bindExportButton(
            tableSelector,
            userType,
            sheetName
        );
    }

    return { init };

})();