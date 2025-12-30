/**
 * Initialize a summary DataTable with optional AJAX source, date range filtering, footer totals, and header filters
 * @param {Object} options - Configuration object
 * @param {string} options.tableSelector - jQuery selector for the table
 * @param {Object} [options.orderingColumn={col: 1, direction: "asc"}] - Initial ordering { col: columnIndex, direction: "asc"|"desc" }
 * @param {string} options.ajaxUrl - URL to fetch table data via AJAX
 * @param {Array<Object>} [options.avgCols=[]] - Columns for calculating footer averages [{ index: colIndex, includeZeros: true|false }]
 * @param {Array<number>} [options.floatCols=[]] - Column indexes with float values for footer totals
 * @param {Array<number>} [options.intCols=[]] - Column indexes with integer values for footer totals
 * @param {Array<Object>} options.columns - DataTables column definitions [{ data: "field", className: "text-center", render: ... }]
 * @param {Array<Object>} [options.headerFilterColumns=[]] - Columns to initialize header filter dropdowns [{ index: colIndex, name: "ColumnName" }]
 * @param {Array<number>} [options.nonOrderableTargets=[]] - Column indexes that should be non-orderable
 * @returns {DataTable} - Initialized DataTable instance
 */
var js = jQuery.noConflict(true);
function initTable({
    tableSelector,
    orderingColumn = [1, "asc"],
    ajaxUrl,
    floatCols = [],
    intCols = [],
    avgCols = [],
    columns = [],
    headerFilterColumns = [],
    nonOrderableTargets = [],
    dateRange = null,
    reloadOn = null,
    emptyTableMessage = "No Summary Available."
}) {
    columns.forEach((col, idx) => {
        if (!col.render) { // Preserve custom renderers
            if (floatCols.includes(idx)) {
                col.render = Renderers.numericRender(true);   // ✅ isFloat = true
            } else if (intCols.includes(idx)) {
                col.render = Renderers.numericRender(false);  // ✅ isFloat = false
            } else {
                col.render = Renderers.stringRender;
            }
        }
    });

    js.fn.dataTable.ext.errMode = 'none';

    var table = js(tableSelector).DataTable({
        responsive: true,
        processing: true,
        serverSide: false,
        ajax: {
            url: ajaxUrl,
            type: 'GET',
            data: function (d) {
                if (typeof dateRange.applyToAjax === 'function') {
                    dateRange.applyToAjax(d);
                }
            }            
        },
        columnDefs: [
            { orderable: false, targets: nonOrderableTargets },
            {
                targets: headerFilterColumns.map(col => col.index),
                orderable: true,
            },
            {
                orderSequence: ['asc', 'desc'],
                targets: '_all'
            }
        ],
        order: [orderingColumn],
        lengthMenu: [[10, 15, 25, 50, 100, -1], [10, 15, 25, 50, 100, "All"]],
        pageLength: 50,
        columns: columns,
        language: { emptyTable: emptyTableMessage },

        footerCallback: function (row, data, start, end, display) {
            var api = this.api();
            const parseVal = i =>
                typeof i === 'string' ? i.replace(/[\$,]/g, '') * 1 :
                typeof i === 'number' ? i : 0;

            // Floats
            floatCols.forEach(col => {
                let total = api.column(col, { page: 'current' }).data()
                    .reduce((a, b) => parseVal(a) + parseVal(b), 0);
                js(api.column(col).footer()).html(
                    total.toLocaleString('en-US', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }));
            });

            // Ints
            intCols.forEach(col => {
                let total = api.column(col, { page: 'current' }).data()
                    .reduce((a, b) => parseVal(a) + parseVal(b), 0);
                js(api.column(col).footer()).html(total);
            });

            //Averages
            avgCols.forEach(cfg => {
                let values = api.column(cfg.index, { page: 'current' })
                    .data()
                    .map(parseVal);

                if (!cfg.includeZeros) {
                    values = values.filter(v => v !== 0);
                }

                let avg = values.length > 0
                    ? values.reduce((a, b) => a + b, 0) / values.length
                    : 0;

                js(api.column(cfg.index).footer()).html(
                    avg.toLocaleString('en-US', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    })
                );
            });
        },
        initComplete: function () {
            const dt = this;
            headerFilterColumns.forEach(col => {
                initHeaderFilterDropdown(dt, col.index, col.name);
            });
        }
    });

    if (reloadOn) {
        js(reloadOn).on('daterange:changed', function () {
            table.ajax.reload();
        });
    }



    // GLOBAL AJAX ERROR HANDLER
    js(tableSelector).on("xhr.dt", function (e, settings, json, xhr) {

        if (xhr.status !== 200) {

            console.log("❌ AJAX load failed:", xhr.status);

            const $table = js(tableSelector);
            const dt = $table.DataTable();
            const colCount = $table.find("thead th").length;

            // ❗ Only insert error row if table is currently empty
            const hasData = dt.data().any();  // true if table already has rows

            if (!hasData) {
                $table.find("tbody").html(`
                <tr class="text-center">
                    <td colspan="${colCount}" class="text-danger">
                        ⚠️ Failed to load data. Please check your connection or try again.
                    </td>
                </tr>
            `);
            }

            table.processing(false);
        }
    });


    return table;
}