//#region Renderers for DataTables    
const Renderers = {
    number: (d, type, decimals = 2, plainZero = true) => {
        const safeDecimals = Number.isInteger(decimals) && decimals >= 0 ? decimals : 2;
        const value = parseFloat(d) || 0;
        if (type === 'sort' || type === 'type') return value;
        if (plainZero && value === 0) return "0";
        return value.toLocaleString('en-US', {
            minimumFractionDigits: safeDecimals,
            maximumFractionDigits: safeDecimals
        });
    },
    numericRender: (isFloat = true) => (data, type, row) => {
        if (type === 'sort' || type === 'type')
            return isFloat ? parseFloat(data) || 0 : parseInt(data) || 0;

        if (isFloat) {
            const value = parseFloat(data) || 0;
            return value.toLocaleString('en-US', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
        }
        return data;
    },
    // String renderers
    stringRender: (data, type_, row) => {
        if (type_ === 'sort' || type_ === 'type') {
            if (!data) return '';
            const text = $('<div>').html(data).text(); // strip HTML
            return text.trim().toLowerCase();
        }
        return data;
    },
    timeDeviationRenderer: function (d, type) {
        if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
        var value = parseFloat(d) || 0;
        var color = value > 0 ? "green" : value < 0 ? "red" : "gray";
        const formatted = value === 0 ? "0" : value.toFixed(2);
        return `<span style="color:${color}; font-weight:600;">${formatted} min</span>`;
    },
    dateRenderer: (dataKey, rawKey) => ({
        render: (data, type, row) => {
            // For sorting / type detection, use raw value
            if (type === 'sort' || type === 'type') return row[rawKey] || '';

            // Display formatted string if exists, else fallback to raw
            return row[dataKey] || row[rawKey] || 'N/A';
        },

        createdCell: (td, cellData, rowData) => {
            // Set data-order and data-iso attributes
            const orderValue = rowData[rawKey] || '';
            const displayValue = rowData[dataKey] || rowData[rawKey] || 'N/A';

            td.setAttribute('data-order', orderValue);
            td.setAttribute('data-iso', displayValue);

            td.innerText = displayValue;
            td.classList.add('text-center');
        }
    }),
    amount: (d, type) => Renderers.number(d, type, 2, false),
    rating: (d, type) => {
        if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
        const value = parseFloat(d) || 0;
        if (value === 0) return value;
        const hue = ((value - 1) / 4) * 120;
        return `<span style="color:hsl(${hue},70%,40%); font-weight:600;">${value.toFixed(2)}</span>`;
    },
    distance: (d, type) => Renderers.number(d, type, 2) + ' km',
    duration: (d, type) => Renderers.number(d, type, 2) + ' min',
    orDefault: function (data, type) {
        // Keep sort and type operations intact
        if (type === 'sort' || type === 'type') return data || '';
        // Display data if non-empty string, otherwise default to 'N/A'
        return (typeof data === 'string' && data.trim() !== '') ? data : 'N/A';
    },
    expandableText: function (data, type, row) {
        if (type === 'sort' || type === 'type') return data || '';
        return data ? renderExpandableText(data, type, row) : 'N/A';
    },
    phone: (data) => !data ? "N/A" : `
        <div class="d-inline-flex align-items-center gap-1">
            <a href="tel:${data}">${data}</a>
            <a onclick="copyToClipboard('${data}')" title="Copy to clipboard" class="text-primary text-decoration-none">
                <i class="bi bi-clipboard"></i>
            </a>
        </div>`,
    voucherCode: (data) => !data ? "N/A" : `
            ${data}
            <a onclick="copyToClipboard('${data}')" title="Copy to clipboard" class="text-secondary text-decoration-none">
                <i class="bi bi-clipboard"></i>
            </a>`,
    purposeSummary: function (data, type, row) {
        if (!data || !Array.isArray(data) || data.length === 0) return "N/A";

        // Sorting or type operations: sum of counts
        if (type === 'sort' || type === 'type') {
            return data.reduce((sum, item) => sum + (item.count || 0), 0);
        }

        // Display HTML
        return data.map(item =>
            `<span style="color:${item.color}; font-weight:600;">${item.purpose}: ${item.count}</span>`
        ).join('<br>');
    },

    reviewOrShow: (row, isDelivery) => {
        if (!isRedCloud) return `<p class="text-muted mb-0">N/A</p>`;
        const purposeKey = Object.keys(purposeOptions).find(k => purposeOptions[k] === row.purpose) || '';

        if (row.note || row.purpose) {
            return `
            <button class="btn btn-outline-success btn-sm"
                data-note="${row.note || ''}"
                data-purpose="${row.purpose || ''}"
                data-purpose-key="${purposeKey}"
                data-voucher-code="${row.voucherCode}"
                data-customer-phone="${row.phoneNumber || ''}"
                data-supervisor-phone="${row.supervisorPhoneNumber || ''}"
                data-customer-review="${row.review || ''}"
                data-customer-rating="${row.rating || 0}"
                data-phone-number="${row.driverPhoneNumber || ''}"
                data-is-delivery="${isDelivery}"
                onclick="showDetailsModal(this)">
                Show
            </button>`;
        } else {
            return `
            <button class="btn btn-outline-danger btn-sm"
                data-voucher-code="${row.voucherCode}"
                data-phone-number="${row.driverPhoneNumber || ''}"
                data-supervisor-phone="${row.supervisorPhoneNumber || ''}"
                data-customer-phone="${row.phoneNumber || ''}"
                data-customer-review="${row.review || ''}"
                data-customer-rating="${row.rating || 0}"
                data-is-delivery="${isDelivery}"
                onclick="showReviewModal(this)">
                Review
            </button>`;
        }
    },

    activityBtn: (row) => `
        <button class="btn btn-outline-primary activityBtn btn-sm"
            data-voucher="${row.voucherCode}"
            data-company-code="${row.companyCode}"
            onclick="showActivity(this)">
            Show
        </button>`,

    detailsLink: (row) => {
        if (!isRedCloud) return `<p class="text-muted mb-0">N/A</p>`;
        const hasValidTableId = row.tableId !== null && row.tableId !== undefined && row.tableId !== "";

        let href = `orderdetail?voucher=${row.voucherCode}`;

        if (hasValidTableId) {
            href += `&type=${row.tableId}`;
        }

        return `<a class="btn btn-outline-info activityBtn btn-sm text-decoration-none" target="_blank" href="${href}">Details</a>`;
    },
    completePendingOrder: (row) =>
        `<button class="btn btn-outline-success activityBtn btn-sm"
            data-voucher="${row.voucherCode}"
            data-duration="${row.duration}"
            data-distance="${row.distance}"
            data-eta="${row.eta}"
            data-driverphone="${row.driverPhoneNumber}"
            data-supervisorphone="${row.supervisorPhoneNumber}"
            onclick="openCompletePendingOrderModal(this)">
            Complete
        </button>`,

    textOrNA: (data, type) => {
        if (type === 'sort' || type === 'type') return data || '';
        return data !== null && data !== undefined && String(data).trim() !== ''
            ? data
            : 'N/A';
    },

    address: (geo, specific, type) => {
        if (type === 'sort' || type === 'type') {
            return [geo, specific].filter(Boolean).join(' ');
        }
        if (!geo && !specific) return 'N/A';
        return [geo, specific].filter(Boolean).join(' - ');
    },

    booleanYesNo: (data, type) => {
        if (type === 'sort' || type === 'type') return data ? 1 : 0;
        return `
            <span class = ${data ? 'text-success' : 'text-danger'}>
                    ${data ? 'Yes' : 'No'}
            </span>
        `; 
    },
    branch: (branchName, type, row) => {
        // For sorting / type detection
        if (type === 'sort' || type === 'type') return branchName || '';

        // If no branch name, return fallback
        if (!branchName) return 'N/A';

        // Include the same data-* attributes as in your old <td>
        return `
            <span
                data-tin="${row.companyTin || ''}"
                data-company="${row.companyName || ''}"
                data-branch="${branchName}"
                data-voucher="${row.voucherCode || ''}">
                ${branchName}
                <a onclick="openChangeBranchModal(this)">
                    <i class="fa-solid fa-pen-to-square"></i>
                </a>
            </span>
        `;
    },
    status: (status, type, row) => {
        if (type === 'sort' || type === 'type') return status || '';
        if (!status) return 'N/A';

        const tooltip = status.toLowerCase() === 'sos'
            ? (row?.sosReason || '')
            : '';
        const backgroundColor = statusColors[status.toLowerCase()];
        return `
            <span class="p-1 text-white d-flex justify-content-center align-items-center p-1 rounded rounded-2 my-1"
                  data-bs-toggle="tooltip"
                  data-bs-placement="top"
                  title="${tooltip}" style="background-color: ${backgroundColor}">
                ${status}
            </span>`;
    },
    statusReport: (data, type) => {
        if (type === 'sort' || type === 'type') return data || '';
        return data ? clean(data) : '-';
    },
    assign: (row) => {
        if (!row.supervisedBy) {
            return `
                <a class="btn btn-outline-dark btn-sm"
                   onclick="openAssignSupervisorModal('${row.voucherCode}')">
                    Assign
                </a>`;
        }

        return `
            <a href="tel:${row.supervisedBy}">${row.supervisorName ?? row.supervisedBy}</a>
            <a onclick="openAssignSupervisorModal('${row.voucherCode}')">
                <i class="fa-solid fa-pen-to-square"></i>
            </a>`;
    },
    detailsActions: (row) => {
        if (!row?.voucherCode) return 'N/A';

        // Compute redispatch dynamically
        const status = row.status?.toLowerCase() || '';
        const orderJson = JSON.stringify(row).replace(/"/g, '&quot;');

        const redispatch = (
            status === "drivernotfound" ||
            status === "declined" ||
            status === "requested" ||
            status === "sos" ||
            status === "assigned"
        ) ? `<a href="#" data-order="${orderJson}" onclick="openRedispatchModal(this)">Redispatch</a>` : '';

        return `
            <div style="display:inline-block;">
                <a href="/order/${row.voucherCode}">Details</a>
                ${redispatch}
            </div>
        `;
    }

};

//#endregion

//#region expandable text render and toggle
function renderExpandableText(data, type, row, maxLength = 50, remainingThreshold = 10) {
    if (type === 'sort' || type === 'type') return data || '';
    if (!data) return 'N/A';

    let shortText = data, showToggle = false;
    if (data.length > maxLength && (data.length - maxLength > remainingThreshold)) {
        shortText = data.substring(0, maxLength) + '...';
        showToggle = true;
    }

    return showToggle
        ? `<span class="short-text">${shortText}</span><span class="full-text" style="display:none;">
                ${data}</span><a href="#" class="toggle-text"
                onclick="toggleExpandableText(this);return false;">Read more</a>`
        : shortText;
}

function toggleExpandableText(link) {
    const c = link.parentElement, s = c.querySelector('.short-text'), f = c.querySelector('.full-text');
    if (s.style.display === 'none') { s.style.display = ''; f.style.display = 'none'; link.innerText = 'Read more'; }
    else { s.style.display = 'none'; f.style.display = ''; link.innerText = 'Show less'; }
}

//#endregion
const clean = (s) => new DOMParser().parseFromString(s, 'text/html').body.textContent || '-';

const statusColors = {
    requested: "deepskyblue",
    arrivedatbranch: "coral",
    assigned: "lawngreen",
    declined: "red",
    accepted: "seagreen",
    arrived: "#F7BEA2",
    ontheway: "darkorange",
    drivernotfound: "red",
    sos: "darkred",
    default: "yellow"
};