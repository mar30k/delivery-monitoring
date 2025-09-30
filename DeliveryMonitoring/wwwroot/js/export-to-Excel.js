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
    var table = js(tableSelector)[0];
    if (!table) {
        console.warn("Table not found:", tableSelector);
        return;
    }

    // Convert table to workbook
    var workbook = XLSX.utils.table_to_book(table, { sheet: sheetName });
    var worksheet = workbook.Sheets[sheetName];

    // Set column widths if provided
    if (columnWidths.length > 0) {
        worksheet['!cols'] = columnWidths;
    }

    // Generate timestamp
    var timestamp = moment().format("YYYY-MM-DD_HH-mm-ss");

    // Prepare filename parts
    var startStr = startDate ? startDate.format("YYYY-MM-DD") + "_to_" : "all_dates";
    var endStr = endDate ? endDate.format("YYYY-MM-DD") : " ";

    // Default filename logic
    var filename = typePrefix + "_report_" + startStr + endStr + ".xlsx";

    // Same-day adjustment
    if (startDate && endDate && startDate.isSame(endDate, 'day')) {
        filename = typePrefix + "_report_" + startDate.format("YYYY-MM-DD") + ".xlsx";
    }

    // Save workbook
    XLSX.writeFile(workbook, filename);
}


    //all pages are not loading so using datatable is must
    //columns names for columns with head filters is not correct 
    //footers should be removed 
    //only full texts should be displayed in cells rather than html