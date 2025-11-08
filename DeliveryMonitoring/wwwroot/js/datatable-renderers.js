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
    requestDate: {
        render: function (data, type, row) {
            if (type === 'sort' || type === 'type') return row.requestCreatedAt || '';
            return row.requestCreatedAtString || row.requestCreatedAt || 'N/A';
        },
        createdCell: function (td, cellData, rowData, row, col) {
            if (rowData.requestCreatedAt) {
                const parsed = moment(rowData.requestCreatedAt, "YYYY-MM-DD hh:mm:ss");
                td.setAttribute("data-order", parsed.format("YYYY-MM-DDTHH:mm:ss"));
                td.innerText = rowData.requestCreatedAtString || rowData.requestCreatedAt;
            } else {
                td.innerText = "N/A";
            }
        }
    },
    amount: (d, type) => Renderers.number(d, type, 2),
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
        </div>`
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