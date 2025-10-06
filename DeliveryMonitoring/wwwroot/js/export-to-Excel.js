/**
 * Export table to Excel keeping initial logic
 * @param {string} tableSelector - jQuery selector for the table
 * @param {string} typePrefix - Prefix used in filename (equivalent to '@type')
 * @param {string} sheetName - Excel sheet name
 * @param {moment} startDate - Optional start date
 * @param {moment} endDate - Optional end date
 * @param {Array} columnWidths - Optional column width array
 */
function exportTableToExcel({
    tableSelector,
    typePrefix = "report",
    sheetName = "Sheet1",
    startDate = null,
    endDate = null,
    columnWidths = []
}) {
    var table = js(tableSelector);
    if (table.length === 0) {
        console.warn("Table not found:", tableSelector);
        return;
    }

    // Get DataTable instance
    var dt = table.DataTable();
    if (!dt) {
        console.warn("DataTable not initialized:", tableSelector);
        return;
    }

    // Extract clean headers
    var headers = [];
    table.find('thead th').each(function () {
        var title = js(this).find('.dt-column-title').first().text().trim();
        headers.push(title || "");
    });

    // Extract object keys from first row
    var data = dt.rows({ search: 'applied' , page: 'all'}).data().toArray();
    if (!data.length) {
        console.warn("No data found for export.");
        return;
    }

    var keys = Object.keys(data[0]); // ['phoneNumber', 'name', 'dineInAmount', ...]

    // Build worksheet data
    var ws_data = [headers]; // first row = headers
    data.forEach(row => {
        ws_data.push(
            keys.map(k => {
                var val = row[k];

                // If val is null or undefined, make it empty string
                if (val == null) return "";

                // If it's an object (like {display: ..., @data-order: ...})
                if (typeof val === "object") {
                    // Use display if it exists
                    if ("display" in val) return val.display;
                    // Otherwise stringify the object
                    return JSON.stringify(val);
                }
                // Convert to string
                val = val.toString().trim();

                // If it contains a span (expandable cell), extract full text
                if (val.includes("<span")) {
                    return getCellFullText(val);
                }

                // Otherwise, return trimmed text
                return val;
            })
        );
    });

    // Build worksheet
    var worksheet = XLSX.utils.aoa_to_sheet(ws_data);

    // Set column widths if provided
    if (columnWidths.length > 0) {
        worksheet['!cols'] = columnWidths;
    }

    // Build workbook
    var workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, sheetName);

    // Prepare filename
    var startStr = startDate ? startDate.format("YYYY-MM-DD") + "_to_" : "all_dates";
    var endStr = endDate ? endDate.format("YYYY-MM-DD") : "";
    var filename = typePrefix + "_report_" + startStr + endStr + ".xlsx";
    if (startDate && endDate && startDate.isSame(endDate, 'day')) {
        filename = typePrefix + "_report_" + startDate.format("YYYY-MM-DD") + ".xlsx";
    }

    // Save workbook
    XLSX.writeFile(workbook, filename);
}
function getCellFullText(cellHtml) {
    if (!cellHtml) return "";

    // Create a temporary DOM element to parse HTML
    var tmp = document.createElement("div");
    tmp.innerHTML = cellHtml;

    // Look for .full-text span first
    var fullSpan = tmp.querySelector(".full-text");
    if (fullSpan) {
        return fullSpan.textContent.trim();
    }

    // Otherwise, fallback to plain text
    return tmp.textContent.trim();
}
