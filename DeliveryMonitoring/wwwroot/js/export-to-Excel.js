/**
 * Export DataTable to Excel, keeping full table data and display renderers
 * @param {Object} options
 * @param {string} options.tableSelector - jQuery selector for the table
 * @param {string} [options.typePrefix="report"] - Prefix for filename
 * @param {string} [options.sheetName="Sheet1"] - Excel sheet name
 * @param {moment} [options.startDate=null] - Optional start date
 * @param {moment} [options.endDate=null] - Optional end date
 * @param {Array} [options.columnWidths=[]] - Optional Excel column widths
 */
function exportTableToExcel({
    tableSelector,
    typePrefix = "",
    sheetName = "Sheet1",
    startDate = null,
    endDate = null,
    columnWidths = []
}) {
    var table = js(tableSelector);
    if (table.length === 0) {
        showToast(`Table not found: ${tableSelector}`, "warning");
        return;
    }

    // Get DataTable instance
    var dt = table.DataTable();
    if (!dt) {
        showToast(`DataTable not initialized: ${tableSelector}`, "warning");
        return;
    }

    // Extract headers in visible order
    var headers = [];
    table.find('thead th').each(function () {
        var title = js(this).find('.dt-column-title').first().text().trim();
        headers.push(title || "");
    });

    // Get all rows (not just current page)
    var allRows = dt.rows({ search: 'applied', page: 'all' }).data().toArray();

    if (!allRows.length) {
        showToast("No data found for export.", "info");
        return;
    }

    // Get column definitions (correct visual order)
    var columns = dt.settings()[0].aoColumns;

    // Build worksheet data
    var ws_data = [headers];

    allRows.forEach((rowData) => {
        var rowArray = [];

        columns.forEach((col) => {
            // Skip hidden columns if you only want visible ones
            if (col.bVisible === false) return;

            var cellValue;

            // Use DataTables renderer if defined
            if (typeof col.render === "function") {
                try {
                    cellValue = col.render(rowData[col.data], "display", rowData);
                } catch (e) {
                    showToast(`Render error in column: ${col.data}`, "error");
                    cellValue = rowData[col.data];
                }
            } else {
                cellValue = rowData[col.data];
            }

            // Handle nulls
            if (cellValue == null) cellValue = "";

            // Handle object-type cells (like {display: ...})
            if (typeof cellValue === "object") {
                if ("display" in cellValue) {
                    cellValue = cellValue.display;
                } else {
                    cellValue = JSON.stringify(cellValue);
                }
            }

            // Convert to string
            cellValue = cellValue.toString().trim();

            // Handle expandable full-text cells
            if (cellValue.includes("full-text")) {
                cellValue = getCellFullText(cellValue);
            } else if (/<[a-z][\s\S]*>/i.test(cellValue)) {
                // If HTML exists, extract text content safely
                var tmp = document.createElement("div");
                tmp.innerHTML = cellValue;
                cellValue = tmp.textContent.trim();
            }

            rowArray.push(cellValue);
        });

        ws_data.push(rowArray);
    });

    // Build worksheet
    var worksheet = XLSX.utils.aoa_to_sheet(ws_data);

    // Set column widths if provided
    if (columnWidths.length > 0) {
        worksheet['!cols'] = columnWidths;
    }

    try {
        // Create workbook
        var workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, sheetName);

        // Generate filename
        var startStr = startDate ? startDate.format("YYYY-MM-DD") + "_to_" : "all_dates";
        var endStr = endDate ? endDate.format("YYYY-MM-DD") : "";
        var filename = `${typePrefix}_${startStr}${endStr}.xlsx`;

        if (startDate && endDate && startDate.isSame(endDate, 'day')) {
            filename = `${typePrefix}_${startDate.format("YYYY-MM-DD")}.xlsx`;
        }

        // Save Excel file
        XLSX.writeFile(workbook, filename);

        showToast(`Excel file generated: ${filename}`, "success");
    } catch (e){
        showToast(`Failed to export Excel: ${e.message}`, "error");
    }
}

/**
 * Extracts full text from expandable cells containing .full-text span
 * @param {string} cellHtml
 * @returns {string}
 */
function getCellFullText(cellHtml) {
    if (!cellHtml) return "";
    var tmp = document.createElement("div");
    tmp.innerHTML = cellHtml;
    var fullSpan = tmp.querySelector(".full-text");
    if (fullSpan) {
        return fullSpan.textContent.trim();
    }
    return tmp.textContent.trim();
}