// TableUtils.js
export function center(col) {
    return { className: "text-center", ...col };
}

export function safeNumberRenderer(d, type, decimals = 2, allowZero = true) {
    if (d == null) return "-";
    return Renderers.number(d, type, decimals, allowZero);
}


export function bindExportButton(tableSelector, typePrefix, sheetName, dateRange, columnWidths = [], excludeColumns) {
    $("#exportToExcelBtn").on("click", () => {
        const range = dateRange?.getRange() || {};
        const start = range.start ?? null;
        const end = range.end ?? null;
        exportTableToExcel({
            tableSelector,
            typePrefix,
            sheetName,
            startDate: start,
            endDate: end,
            columnWidths,
            excludeColumns
        });
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
