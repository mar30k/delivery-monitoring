let startDate = moment().startOf('day');
let endDate = moment().endOf('day');

function initPageReportTable(options) {
    const {
        tableId,
        ajaxUrl,
        userType,
        sheetName
    } = options;
    const tableSelector = `#${tableId}`;
    const isAllOrders = tableId == "allOrders";

    const config = {
        tableSelector,
        ajaxUrl,
        orderingColumn: [5, "des"],
        headerFilterColumns: [],
        nonOrderableTargets: [0],
        columns: [],
        floatCols: [],
        intCols: [],
        avgCols: [],
    };

    config.columns = [
        { data: "voucherCode", className: "text-center" },
        { data: "companyName", className: "text-center" },
        { data: "branchName", className: "text-center" },
        { data: "firstName", className: "text-center" },
        { data: "phoneNumber", className: "text-center", render: renderPhone },
        {
            data: "requestCreatedAt",
            className: "text-center",
            createdCell: renderRequestDate
        },
        {
            data: "distance",
            className: "text-center",
            render: (d, type) => genericNumberRenderer(d, type, { decimals: 2, unit: "km" })
        },
        {
            data: "duration",
            className: "text-center",
            render: (d, type) => genericNumberRenderer(d, type, { decimals: 2, unit: "min" })
        },
        { data: "eta", className: "text-center" },
        { data: "driverPhoneNumber", className: "text-center", render: renderPhone },
        { data: "supervisorName", className: "text-center" },
        { data: "totalAmount", className: "text-center", render: renderAmount },
        { data: "tip", className: "text-center", render: renderAmount },
        { data: "purpose", className: "text-center", render: renderOrDefault },
        {
            data: "note",
            className: "text-center",
            render: function (data, type, row) {
                return renderExpandableText(data, type, row);
            },
            createdCell: function (td, cellData, rowData, row, col) {
                $(td).html(renderExpandableText(cellData, 'display', rowData));
            }
        },
        { data: "review", className: "text-center", render: renderOrDefault },
        { data: "rating", className: "text-center" }
    ];
    // Conditionally add column for all orders
    if (isAllOrders) {
        config.columns.push({ data: "tableId", className: "text-center" });
    }
    config.headerFilterColumns = [{ index: 1, name: 'Company' }, { index: 2, name: 'Branch' }];
    config.floatCols = [6, 11, 12];
    config.avgCols = [{ index: 16, includeZeros: false }];
    config.nonOrderableTargets = [0, 4, 9, 13, 14, 15]
    initSummaryTable(config);


    $("#exportToExcelBtn").on("click", function () {
        exportTableToExcel({
            tableSelector,
            typePrefix: `${userType}`,
            sheetName,
            startDate,
            endDate,
            columnWidths: [
                { wch: 20 }, { wch: 25 }, { wch: 25 }, { wch: 20 }, { wch: 15 }, { wch: 22 },
                { wch: 10 }, { wch: 10 }, { wch: 10 }, { wch: 20 }, { wch: 20 }, { wch: 12 },
                { wch: 10 }, { wch: 20 }, { wch: 30 }, { wch: 30 }, { wch: 10 }
            ]
        });
    });
}
function renderOrDefault(data, type, row) {
    if (type === 'sort' || type === 'type') {
        return data || '';
    }
    return data ? data : 'N/A'; // You can choose any placeholder like "-" or empty string
}

const renderPhone = (data) => {
    if (!data) return "N/A";
    return `
        <div class="d-inline-flex align-items-center gap-1">
            <a href="tel:${data}">${data}</a>
            <a onclick="copyToClipboard('${data}')" title="Copy to clipboard" class="text-primary text-decoration-none">
                <i class="bi bi-clipboard"></i>
            </a>
        </div>`;
};
function renderExpandableText(data, type, row, maxLength = 50, remainingThreshold = 10) {
    if (type === 'sort' || type === 'type') {
        // For sorting/filtering, return raw value
        return data || '';
    }

    if (!data) {
        return 'N/A';
    }

    let shortText = data;
    let showToggle = false;

    if (data.length > maxLength && (data.length - maxLength > remainingThreshold)) {
        shortText = data.substring(0, maxLength) + '...';
        showToggle = true;
    }

    if (showToggle) {
        return `
            <span class="short-text">${shortText}</span>
            <span class="full-text" style="display:none;">${data}</span>
            <a href="#" class="toggle-text" onclick="toggleExpandableText(this);return false;">Read more</a>
        `;
    } else {
        return shortText;
    }
}
const renderRequestDate = (td, cellData, rowData) => {
    if (rowData.requestCreatedAt) {
        const parsed = moment(rowData.requestCreatedAt, "YYYY-MM-DD hh:mm:ss");
        td.setAttribute("data-order", parsed.format("YYYY-MM-DDTHH:mm:ss"));
        td.innerText = rowData.requestCreatedAtString;
    } else {
        td.innerText = "N/A";
    }
};

const renderAmount = (d, type) => {
    if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
    if (!d) return "0.00";
    return parseFloat(d).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
};
function numberRenderer(d, type) {
    if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
    return (!d ? 0 : parseFloat(d)).toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function ratingRenderer(d, type) {
    if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
    var value = parseFloat(d) || 0;
    if (value === 0) return value;
    var hue = ((value - 1) / 4) * 120;
    var color = `hsl(${hue}, 70%, 40%)`;
    return `<span style="color:${color}; font-weight:600;">${value.toFixed(2)}</span>`;
}
function genericNumberRenderer(d, type, options = {}) {
    const { decimals = 2, unit = '' } = options;

    // Sorting and filtering use raw numeric value
    if (type === 'sort' || type === 'type') {
        return parseFloat(d) || 0;
    }

    // Display formatted value
    const value = parseFloat(d) || 0;
    const formatted = value.toLocaleString('en-US', {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    });

    return unit ? `${formatted} ${unit}` : formatted;
}

// Toggle long text in Note/Review cells
function toggleExpandableText(link) {
    const container = link.parentElement;
    const shortText = container.querySelector('.short-text');
    const fullText = container.querySelector('.full-text');

    if (shortText.style.display === 'none') {
        shortText.style.display = '';
        fullText.style.display = 'none';
        link.innerText = 'Read more';
    } else {
        shortText.style.display = 'none';
        fullText.style.display = '';
        link.innerText = 'Show less';
    }
};