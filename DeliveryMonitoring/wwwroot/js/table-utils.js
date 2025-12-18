// TableUtils.js
export function center(col) {
    return { className: "text-center", ...col };
}

export function safeNumberRenderer(d, type, decimals = 2, allowZero = true) {
    if (d == null) return "-";
    return Renderers.number(d, type, decimals, allowZero);
}

function getDateRange() {
    return {
        startDate: startDate,
        endDate: endDate
    };
}
export function bindExportButton(tableSelector, typePrefix, sheetName, columnWidths = []) {
    $("#exportToExcelBtn").on("click", () => {
        const { startDate, endDate } = getDateRange();
        exportTableToExcel({ tableSelector, typePrefix, sheetName, startDate, endDate, columnWidths });
    });
}

export function getBaseTableConfig() {
    return {
        orderingColumn: [0, 'asc'],
        headerFilterColumns: [],
        nonOrderableTargets: [],
        columns: [],
        floatCols: [],
        intCols: [],
        avgCols: []
    };
}
