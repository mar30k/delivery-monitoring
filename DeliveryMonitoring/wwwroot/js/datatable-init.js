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
    emptyTableMessage = "No Summary Available.",
    onDataLoaded = null
}) {
    columns.forEach((col, idx) => {
        if (!col.render) {
            if (floatCols.includes(idx)) {
                col.render = Renderers.numericRender(true);
            } else if (intCols.includes(idx)) {
                col.render = Renderers.numericRender(false);
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
                if (dateRange && typeof dateRange.applyToAjax === 'function') {
                    dateRange.applyToAjax(d);
                }
            },
            dataSrc: function (json) {
                if (typeof onDataLoaded === 'function') {
                    onDataLoaded(json); // call the callback with the full JSON
                }
                return json.data ?? json;
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
            const api = this.api();
            const parseVal = i =>
                typeof i === 'string' ? i.replace(/[\$,]/g, '') * 1 :
                    typeof i === 'number' ? i : 0;

            floatCols.forEach(col => {
                const total = api.column(col, { page: 'current' }).data()
                    .reduce((a, b) => parseVal(a) + parseVal(b), 0);
                js(api.column(col).footer()).html(
                    total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
                );
            });

            intCols.forEach(col => {
                const total = api.column(col, { page: 'current' }).data()
                    .reduce((a, b) => parseVal(a) + parseVal(b), 0);
                js(api.column(col).footer()).html(total);
            });

            avgCols.forEach(cfg => {
                let values = api.column(cfg.index, { page: 'current' })
                    .data()
                    .map(parseVal);

                if (!cfg.includeZeros) {
                    values = values.filter(v => v !== 0);
                }

                const avg = values.length ? values.reduce((a, b) => a + b, 0) / values.length : 0;

                js(api.column(cfg.index).footer()).html(
                    avg.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
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

    js(tableSelector).on("xhr.dt", function (e, settings, json, xhr) {
        if (xhr.status !== 200) {
            console.log("❌ AJAX load failed:", xhr.status);
            const $table = js(tableSelector);
            const dt = $table.DataTable();
            const colCount = $table.find("thead th").length;

            if (!dt.data().any()) {
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