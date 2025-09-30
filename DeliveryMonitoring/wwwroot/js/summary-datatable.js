/**
 * Initialize a summary DataTable with optional AJAX source, date range filtering, footer totals, and header filters
 * @param {Object} options - Configuration object
 * @param {string} options.tableSelector - jQuery selector for the table
 * @param {Object} [options.orderingColumn={col: 1, direction: "asc"}] - Initial ordering { col: columnIndex, direction: "asc"|"desc" }
 * @param {string} options.ajaxUrl - URL to fetch table data via AJAX
 * @param {Array<number>} [options.floatCols=[]] - Column indexes with float values for footer totals
 * @param {Array<number>} [options.intCols=[]] - Column indexes with integer values for footer totals
 * @param {Array<Object>} options.columns - DataTables column definitions [{ data: "field", className: "text-center", render: ... }]
 * @param {Array<Object>} [options.headerFilterColumns=[]] - Columns to initialize header filter dropdowns [{ index: colIndex, name: "ColumnName" }]
 * @param {Array<number>} [options.nonOrderableTargets=[]] - Column indexes that should be non-orderable
 * @returns {DataTable} - Initialized DataTable instance
 */

var js = jQuery.noConflict(true);
js(() => {
    js("#dateRange").daterangepicker({
        startDate: startDate,
        endDate: endDate,
        maxDate: moment(),
        autoUpdateInput: true,
        locale: { cancelLabel: 'Clear', format: 'YYYY-MM-DD' }
    });
    js("#dateRange").val(startDate.format('YYYY-MM-DD') + ' to ' + endDate.format('YYYY-MM-DD'));
});
var isClear = false;
function initSummaryTable({
    tableSelector,
    orderingColumn = [1, "asc"],
    ajaxUrl,
    floatCols = [],
    intCols = [],
    columns = [],
    headerFilterColumns = [],
    nonOrderableTargets = []
}) {
    // Helper render functions
    const numericRender = (type, isFloat) => (data, type_, row) => {
        if (type_ === 'sort' || type_ === 'type') {
            return isFloat ? parseFloat(data) || 0 : parseInt(data) || 0;
        }
        return isFloat ? parseFloat(data).toFixed(2) : data;
    };

    const stringRender = (data, type_, row) => {
        if (type_ === 'sort' || type_ === 'type') {
            if (!data) return '';
            const text = $('<div>').html(data).text(); // strip HTML
            return text.trim().toLowerCase();
        }
        return data;
    };

    // Apply numeric render to float/int columns
    columns.forEach((col, idx) => {
        if (floatCols.includes(idx)) {
            col.render = numericRender('sort', true);
        } else if (intCols.includes(idx)) {
            col.render = numericRender('sort', false);
        } else {
            col.render = stringRender;
        }
    });

    var table = js(tableSelector).DataTable({
        responsive: true,
        processing: true,
        serverSide: false,
        ajax: {
            url: ajaxUrl,
            type: 'POST',
            data: function (d) {
                const picker = js("#dateRange").data('daterangepicker');
                if (picker && !isClear) {
                    d.startDate = picker.startDate.format("YYYY-MM-DD");
                    d.endDate = picker.endDate.format("YYYY-MM-DD");
                    d.isClear = false;
                } else if(isClear){
                    d.startDate = null;
                    d.endDate = null;
                    d.isClear = true;
                }
            }
        },
        columnDefs: [
            { orderable: false, targets: nonOrderableTargets },
            {
                targets: headerFilterColumns.map(col => col.index),
                orderable: true,
                render: function (data, type, row) { return data; }
            }
        ],
        order: [orderingColumn],
        lengthMenu: [[10, 15, 25, 50, 100, -1], [10, 15, 25, 50, 100, "All"]],
        pageLength: 50,
        columns: columns,
        language: { emptyTable: "No Summary Available." },

        footerCallback: function (row, data, start, end, display) {
            var api = this.api();
            const parseVal = i => typeof i === 'string' ? i.replace(/[\$,]/g, '') * 1 : typeof i === 'number' ? i : 0;

            // Floats
            floatCols.forEach(col => {
                let total = api.column(col, { page: 'current' }).data().reduce((a, b) => parseVal(a) + parseVal(b), 0);
                js(api.column(col).footer()).html(total.toFixed(2));
            });

            // Ints
            intCols.forEach(col => {
                let total = api.column(col, { page: 'current' }).data().reduce((a, b) => parseVal(a) + parseVal(b), 0);
                js(api.column(col).footer()).html(total);
            });
        },
        initComplete: function () {
            const dt = this;
            headerFilterColumns.forEach(function (col) {
                initHeaderFilterDropdown(dt, col.index, col.name);
            });
        }
    });

    // Reload table on date range change
    js("#dateRange").on('apply.daterangepicker', function (ev, picker) {
        startDate = picker.startDate.startOf('day');
        endDate = picker.endDate.endOf('day');
        js(this).val(picker.startDate.format('YYYY-MM-DD') + ' to ' + picker.endDate.format('YYYY-MM-DD'));
        table.ajax.reload();
    });

    js("#dateRange").on('cancel.daterangepicker', function (ev, picker) {

        // Clear internal dates
        picker.setStartDate(moment().startOf('day'));
        picker.setEndDate(moment().startOf('day'));

        // Clear the input field
        js(this).val('');

        // Reset your variables
        startDate = null;
        endDate = null;
        isClear = true;

        // Reload table with isClear = true
        table.ajax.reload(function () {
            isClear = false;
        });
    });


    return table;
}