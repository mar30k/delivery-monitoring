// TableUtils.js
export function center(col) {
    return { className: "text-center", ...col };
}

export function safeNumberRenderer(d, type, decimals = 2, allowZero = true) {
    if (d == null) return "-";
    return Renderers.number(d, type, decimals, allowZero);
}


export function bindExportButton(tableSelector, typePrefix, sheetName, columnWidths = []) {
    $("#exportToExcelBtn").on("click", () => {
        const { start, end } = DateRange.getRange();
        exportTableToExcel({
            tableSelector,
            typePrefix,
            sheetName,
            startDate: start,
            endDate: end,
            columnWidths
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
