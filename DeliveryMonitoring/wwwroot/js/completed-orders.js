// Usage
const DeliveryOrderTypes = {
    PickUpAtBranch: 2076,
    DeliveryToLocation: 2077,
    ScheduledPickUp: 2078,
    ScheduledDeliveryToLocation: 2079,
    InHouseDining: 3203
};
var js = jQuery.noConflict(true);
var dineInTable, takeAwayTable, tablelist;
var tableDateRanges = {
    dineInTable: { start: moment().startOf('day'), end: moment().endOf('day') },
    takeAwayTable: { start: moment().startOf('day'), end: moment().endOf('day') },
    tablelist: { start: moment().startOf('day'), end: moment().endOf('day') }
};
var isClear = false;
var takeAwayOrderType = DeliveryOrderTypes.PickUpAtBranch; // 2076
var dineInOrderType = DeliveryOrderTypes.InHouseDining; // 2076

var dateFilterMappings = [
    { id: "dineInDateRange", tableName: "dineInTable" },
    { id: "dateRange", tableName: "tablelist" },
    { id: "takeAwayDateRange", tableName: "takeAwayTable" }
];

js(() => {
    // Shared helper functions

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

    const renderAmount = (d, type) => {
        if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
        if (!d) return "0.00";
        return parseFloat(d).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    };

    const renderCurrency = (d, type) => {
        if (type === 'sort' || type === 'type') return parseFloat(d) || 0;
        if (!d) return "0.00";
        return parseFloat(d).toLocaleString('en-US', { style: 'currency', currency: 'USD' });
    };

    const renderRequestDate = (td, cellData, rowData) => {
        if (rowData.requestCreatedAt) {
            const parsed = moment(rowData.requestCreatedAt, "YYYY-MM-DD hh:mm:ss");
            td.setAttribute("data-order", parsed.format("YYYY-MM-DDTHH:mm:ss"));
            td.innerText = rowData.requestCreatedAtString;
        } else {
            td.innerText = "N/A";
        }
    };

    const renderReviewOrShow = (row, isDelivery) => {
        if (!isRedCloud) return `<p class="text-muted mb-0">N/A</p>`;
        const purposeKey = Object.keys(purposeOptions).find(k => purposeOptions[k] === row.purpose) || '';

        if (row.note || row.purpose) {
            return `
        <button class="btn btn-outline-secondary btn-sm"
            data-note="${row.note || ''}"
            data-purpose="${row.purpose || ''}"
            data-purpose-key="${purposeKey}"
            data-voucher-code="${row.voucherCode}"
            data-customer-phone="${row.phoneNumber || ''}"
            data-customer-review="${row.review || ''}"
            data-customer-rating="${row.rating || 0}"
            data-phone-number="${row.driverPhoneNumber || ''}"
            data-is-delivery="${isDelivery}"
            onclick="showDetailsModal(this)">
            Show
        </button>`;
        } else {
            return `
        <button class="btn btn-outline-secondary btn-sm"
            data-voucher-code="${row.voucherCode}"
            data-phone-number="${row.driverPhoneNumber || ''}"
            data-customer-phone="${row.phoneNumber || ''}"
            data-customer-review="${row.review || ''}"
            data-customer-rating="${row.rating || 0}"
            data-is-delivery="${isDelivery}"
            onclick="showReviewModal(this)">
            Review
        </button>`;
        }
    };

    const renderActivityBtn = (row) => `
    <button class="btn btn-outline-secondary activityBtn btn-sm"
        data-voucher="${row.voucherCode}"
        data-company-code="${row.companyCode}"
        onclick="showActivity(this)">
        Show
    </button>`;

    const renderDetailsLink = (row, isDelivery) => {
        if (!isRedCloud) return `<p class="text-muted mb-0">N/A</p>`;
        const href = isDelivery
            ? `orderdetail?voucher=${row.voucherCode}`
            : `orderdetail?voucher=${row.voucherCode}&type=${row.tableId}`;
        return `<a class="btn btn-outline-secondary activityBtn btn-sm" target="_blank" href="${href}">Details</a>`;
    };

    // Shared base columns (used by all tables)
    const baseColumns = [
        { data: "voucherCode", className: "text-center" },
        { data: "companyName", className: "text-center" },
        { data: "branchName", className: "text-center" },
        { data: "firstName", className: "text-center" },
        { data: "phoneNumber", className: "text-center", render: renderPhone },
        {
            data: "requestCreatedAt",
            className: "text-center",
            createdCell: renderRequestDate
        }
    ];

    // Dine-In & Takeaway columns
    const dineInAndTakeawayColumns = [
        ...baseColumns,
        { data: "supervisorName", className: "text-center" },
        { data: "totalAmount", className: "text-center", render: renderAmount },
        {
            data: null, className: "text-center", orderable: false,
            render: (data, type, row) => renderReviewOrShow(row, false)
        },
        {
            data: null, className: "text-center", orderable: false,
            render: (data, type, row) => renderActivityBtn(row)
        },
        {
            data: null, className: "text-center", orderable: false,
            render: (data, type, row) => renderDetailsLink(row, false)
        }
    ];

    // Delivery columns (adds distance/duration/tip)
    const deliveryColumns = [
        ...baseColumns,
        { data: "distance", className: "text-center", render: d => d + " K.M" },
        { data: "duration", className: "text-center", render: d => d + " Min" },
        { data: "eta", className: "text-center", render: d => d + " Min" },
        { data: "driverPhoneNumber", className: "text-center", render: renderPhone },
        { data: "supervisorName", className: "text-center" },
        { data: "totalAmount", className: "text-center", render: renderAmount },
        { data: "tip", className: "text-center", render: renderAmount },
        {
            data: null, className: "text-center", orderable: false,
            render: (data, type, row) => renderReviewOrShow(row, true)
        },
        {
            data: null, className: "text-center", orderable: false,
            render: (data, type, row) => renderActivityBtn(row)
        },
        {
            data: null, className: "text-center", orderable: false,
            render: (data, type, row) => renderDetailsLink(row, true)
        }
    ];
    // Initialize tables first
    dineInTable = initOrderTable(
        "#dineInTable",
        "#dineInDateRange",
        dineInAndTakeawayColumns,
        `/getordersbytype?type=${dineInOrderType}`,
        "No dine-in orders available.",
        7,
        [0, 1, 2, 4, 7, 8, 9],
        [
            { index: 1, name: 'Branch' },
            { index: 2, name: 'Supervisor' }
        ]
    );
    
    takeAwayTable = initOrderTable(
        "#takeAwayTable",
        "#takeAwayDateRange",
        dineInAndTakeawayColumns,
        `/getordersbytype?type=${takeAwayOrderType}`,
        "No takeaway orders available.",
        7,
        [0, 1, 2, 4, 7, 8, 9],
        [
            { index: 1, name: 'Company' },
            { index: 2, name: 'Branch' }
        ]
    );

    tablelist = initOrderTable(
        "#tablelist",
        "#dateRange",
        deliveryColumns,
        "/getcompletedorders",
        "No orders history to display at the moment.",
        11,
        [0, 1, 2, 4, 9, 10, 13, 14, 15],
        [
            { index: 2, name: 'Branch' },
            { index: 1, name: 'Company' },
            { index: 10, name: 'Supervisor' }
        ]
    );
    // Initialize date range pickers AFTER tables are created
    initDateRangePickers();
});

function initDateRangePickers() {
    dateFilterMappings.forEach(({ id, tableName }) => {
        const selector = `#${id}`;
        const tableRef = (tableName === "dineInTable") ? dineInTable :
            (tableName === "takeAwayTable") ? takeAwayTable :
                (tableName === "tablelist") ? tablelist : null;

        if (!tableRef) return;

        const tableRange = tableDateRanges[tableName];

        js(selector).daterangepicker({
            startDate: tableRange.start,
            endDate: tableRange.end,
            maxDate: moment(),
            autoUpdateInput: true,
            locale: {
                cancelLabel: 'Clear',
                format: 'YYYY-MM-DD'
            }
        });

        js(selector).val(
            tableRange.start.format('YYYY-MM-DD') + ' to ' + tableRange.end.format('YYYY-MM-DD')
        );

        js(selector).on('apply.daterangepicker', function (ev, picker) {
            tableDateRanges[tableName] = {
                start: picker.startDate.startOf('day'),
                end: picker.endDate.endOf('day')
            };
            js(this).val(picker.startDate.format('YYYY-MM-DD') + ' to ' + picker.endDate.format('YYYY-MM-DD'));
            tableRef.ajax.reload();
        });

        js(selector).on('cancel.daterangepicker', function () {
            tableDateRanges[tableName] = { start: null, end: null };
            js(this).val('');
            isClear = true;
            tableRef.ajax.reload(function () {
                isClear = false;
            });
        });
    });
}
function initOrderTable(selector, daterangepicker, columns, ajaxUrl, emptyMessage, totalColumnIndex, nonOrderableTargets = [0], headerFilterColumns = []) {
    const table = js(selector).DataTable({
        responsive: true,
        processing: true,
        serverSide: false,
        ajax: function (data, callback, settings) {
            const url = new URL(ajaxUrl, window.location.origin);
            const tableName = selector.replace('#', '');
            const { start, end } = tableDateRanges[tableName] || {};

            if (start && end && !isClear) {
                url.searchParams.append("startDate", start.format("YYYY-MM-DD"));
                url.searchParams.append("endDate", end.format("YYYY-MM-DD"));
            }

            url.searchParams.append("isClear", isClear);

            js.getJSON(url.toString(), function (json) {
                callback(json);
            });
        },
        order: [[5, "desc"]],
        pageLength: 50,
        columns: columns,
        lengthMenu: [[10, 15, 25, 50, 100, -1], [10, 15, 25, 50, 100, "All"]],
        columnDefs: [
            {
                orderable: false,
                targets: nonOrderableTargets
            },
            {
                targets: headerFilterColumns.map(col => col.index),
                orderable: true,
                render: function (data, type, row) {
                    if (type === 'sort') {
                        return data;
                    }
                    return data;
                }
            },
            {
                orderSequence: ['asc', 'desc'],
                targets: '_all'
            }
        ],
        language: { emptyTable: emptyMessage },
        footerCallback: function (row, data, start, end, display) {
            var api = this.api();

            var parseValue = function (i) {
                if (typeof i === "string") {
                    return parseFloat(i.replace(/[^0-9.-]+/g, "")) || 0;
                } else if (typeof i === "number") {
                    return i;
                }
                return 0;
            };

            var columnData = api.column(totalColumnIndex, { page: "current" }).data();
            var pageTotal = columnData.reduce(function (a, b) {
                return parseValue(a) + parseValue(b);
            }, 0);

            js(api.column(totalColumnIndex).footer()).html(
                pageTotal.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
            );
        },
        initComplete: function () {
            var dt = this;
            headerFilterColumns.forEach(function (col) {
                initHeaderFilterDropdown(dt, col.index, col.name);
            });
        }
    });

    return table;
}
async function showDetailsModal(button) {
    const note = button.getAttribute('data-note').replace(/\n/g, '<br>');;
    const purpose = button.getAttribute('data-purpose');
    const voucherCode = button.getAttribute('data-voucher-code');
    const phoneNumber = button.getAttribute('data-phone-number');
    const customerPhone = button.getAttribute('data-customer-phone');
    const customerReview = button.getAttribute('data-customer-review');
    const customerRating = button.getAttribute('data-customer-rating');
    const purposeKey = button.getAttribute('data-purpose-key');
    const isDelivery = button.getAttribute('data-is-delivery');


    document.getElementById('modalNote').innerHTML = note || '—';
    document.getElementById('modalPurpose').textContent = purpose || '—';
    document.getElementById('voucherCodeDetail').textContent = voucherCode ? `- ${voucherCode}` : '';

    const editBtn = document.getElementById('editNote');
    editBtn.setAttribute('data-voucher-code', voucherCode);
    editBtn.setAttribute('data-phone-number', phoneNumber);
    editBtn.setAttribute('data-customer-phone', customerPhone);
    editBtn.setAttribute('data-purpose-key', purposeKey);
    editBtn.setAttribute('data-note', note);
    editBtn.setAttribute('data-is-delivery', isDelivery);
    // Show the modal immediately
    const modal = new bootstrap.Modal(document.getElementById('reviewDetailsModal'));
    modal.show();
    showCustomerReview({
        phoneNumber,
        voucherCode,
        customerPhone,
        customerReview,
        customerRating,
        reviewSectionId: 'customerReview',
        spinnerId: 'reviewDetailsLoadingSpinner'
    });
}

async function showReviewModal(button) {
    const note = button.getAttribute('data-note');
    const voucherCode = button.getAttribute('data-voucher-code');
    const phoneNumber = button.getAttribute('data-phone-number');
    const customerPhone = button.getAttribute('data-customer-phone');
    const customerReview = button.getAttribute('data-customer-review');
    const customerRating = button.getAttribute('data-customer-rating');
    const purposeKey = button.getAttribute('data-purpose-key');
    const isDelivery = button.getAttribute('data-is-delivery');

    // Close any open modal before opening this one
    document.querySelectorAll('.modal.show').forEach(modalEl => {
        bootstrap.Modal.getInstance(modalEl)?.hide();
    });

    // Populate modal inputs
    document.getElementById('reviewOrderId').value = voucherCode;
    document.getElementById('isDelivery').value = isDelivery;
    document.getElementById('reviewPurpose').value = purposeKey || '';
    document.getElementById('reviewNote').value = note || '';
    document.getElementById('voucherCodeReview').textContent = voucherCode ? `- ${voucherCode}` : '';

    const modal = new bootstrap.Modal(document.getElementById('reviewModal'));
    modal.show();

    showCustomerReview({
        phoneNumber,
        voucherCode,
        customerPhone,
        customerReview,
        customerRating,
        reviewSectionId: 'customerReviewSection',
        spinnerId: 'reviewLoadingSpinner'
    });
}
async function showCustomerReview({
    phoneNumber,
    voucherCode,
    customerPhone,
    customerReview,
    customerRating,
    reviewSectionId,
    spinnerId,
}) {
    const reviewSection = document.getElementById(reviewSectionId);
    reviewSection.innerHTML = '';

    const spinner = document.getElementById(spinnerId);
    spinner.style.display = 'block';
    const retryButton = `<button class="btn btn-sm btn-outline-secondary p-1"
                        onclick="showCustomerReview({
                            phoneNumber: &quot;${phoneNumber}&quot;,
                            voucherCode: &quot;${voucherCode}&quot;,
                            customerPhone: &quot;${customerPhone}&quot;,
                            reviewSectionId: &quot;${reviewSectionId}&quot;,
                            spinnerId: &quot;${spinnerId}&quot;
                        })" style="font-size: 12px;">
                        <i class="fas fa-rotate-right me-1"></i>Retry
                    </button>`;
    try {
        const noReviewFound = `
                <div class="alert alert-warning mb-0" role="alert">
                    No customer review found for this order.
                </div>
            `;
        if (!phoneNumber || !customerRating) {
            reviewSection.innerHTML = noReviewFound;
            return;
        }
        const foundReview = await fetchDriverReview(phoneNumber, voucherCode, customerPhone);
        if (foundReview) {
            const rawImageUrl = foundReview.image || "";
            const imageUrl = rawImageUrl.startsWith("http://") ? "/images/default-avatar.png" : rawImageUrl || "/images/default-avatar.png";
            reviewSection.innerHTML =
                `<div class="card shadow-sm border-0 mb-3">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <div class="d-flex">
                                <img src="${imageUrl}" alt="${foundReview.fullName}" class="rounded-circle me-2" style="width: 40px; height: 40px; object-fit: cover;">
                                <div>
                                    <h6 class="mb-0 fw-bold">${foundReview.fullName}</h6>
                                    <small class="text-muted">${new Date(foundReview.date).toLocaleDateString()}</small>
                                </div>
                            </div>
                            <div class="text-end">
                                <span class="fw-semibold text-warning">
                                    ${renderStars(customerRating)} 
                                    <span class="ms-1 text-dark">${Number(customerRating).toFixed(1)}</span>
                                </span>
                            </div>
                        </div>

                        ${customerReview ? `<p class="my-1"><strong class="text-muted">Review:</strong> ${customerReview}</p>` : ''}
                        ${foundReview.reply ? `<p class="my-1"><strong class="text-muted">Reply:</strong> ${foundReview.reply}</p>` : ''}
                    </div>
                </div>`;
        } else {
            reviewSection.innerHTML = noReviewFound;
        }

        reviewSection.classList.remove('d-none');
    } catch (err) {
        console.error('Error fetching review:', err);
        reviewSection.innerHTML = `
            <div class="alert alert-danger mt-3" role="alert">
                Failed to fetch customer review. 
                ${retryButton}
            </div>
        `;
        reviewSection.classList.remove('d-none');
    } finally {
        if (spinner) spinner.style.display = 'none';
    }
}
function renderStars(rating) {
    rating = Math.floor(rating);
    return '<i class="bi bi-star-fill text-warning"></i>'.repeat(rating) +
        '<i class="bi bi-star text-warning"></i>'.repeat(5 - rating);
}

document.getElementById('editNote').addEventListener('click', function (e) {
    e.preventDefault();

    // Close the reviewDetailsModal
    const reviewDetailsModalEl = document.getElementById('reviewDetailsModal');
    const reviewDetailsModal = bootstrap.Modal.getInstance(reviewDetailsModalEl) || new bootstrap.Modal(reviewDetailsModalEl);
    reviewDetailsModal.hide();

    showReviewModal(this);
});

async function fetchDriverReview(phoneNumber, voucherCode, customerPhone) {
    const page = 1;
    try {
        const response = await fetch(`/Driver/fetchReview?phoneNumber=${encodeURIComponent(phoneNumber)}&page=${page}`);
        if (!response.ok) throw new Error(`Failed to fetch review: ${response.status}`);
        const result = await response.json();
        if (!result || !result.data.reviews || result.data.reviews.length === 0) return null;
        return result.data.reviews.find(r=> r.referenceVoucher === voucherCode && r.reviewerPhoneNumber == customerPhone) || null;

    } catch (error) {
        console.error("Error fetching driver review:", error);
        throw error;
    }
}
document.getElementById('reviewForm').addEventListener('submit', async function (e) {
    e.preventDefault(); 

    const submitBtn = document.getElementById('submitReviewBtn');
    submitBtn.disabled = true;
    const originalText = submitBtn.textContent;
    submitBtn.textContent = 'Submitting...';

    const voucherCode = document.getElementById('reviewOrderId').value;
    const purpose = document.getElementById('reviewPurpose').value;
    const note = document.getElementById('reviewNote').value || '';
    const isDelivery = document.getElementById('isDelivery').value == "true";
    try {
        const response = await fetch('/CompletedOrders/savenote', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ voucherCode, purpose, note, isDelivery })
        });

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('reviewModal')).hide();
            alert("Submitted successfully.");
            location.reload();
        } else {
            const error = await response.text();
            alert("Submission failed: " + error);
        }
    } catch (err) {
        console.error(err);
        alert("An error occurred while submitting.");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
    }
});
async function showActivity(button) {
    const voucherCode = button.getAttribute('data-voucher');
    const companyCode = button.getAttribute('data-company-code');
    const url = `/CompletedOrders/getDeliveryActivity?voucherCode=${encodeURIComponent(voucherCode)}&companyCode=${encodeURIComponent(companyCode)}`;

    // Get modal elements
    const modalEl = document.getElementById('activityModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

    const loader = document.getElementById('activityLoader');
    const alertContainer = document.getElementById('activityAlertContainer');
    const summary = document.getElementById('activitySummary');
    const timeline = document.getElementById('activityTimeline');
    const table = document.getElementById('activityTable');
    const title = document.getElementById('activityModalLabel');

    // Show modal and reset content
    title.innerText = `Delivery Activity - ${voucherCode}`;
    loader.classList.remove('d-none');
    alertContainer.innerHTML = '';
    summary.innerHTML = '';
    summary.classList.add('d-none');
    timeline.innerHTML = '';
    table.classList.add('d-none');
    modal.show();

    try {
        const response = await fetch(url);
        const result = await response.json();

        if (result.isSuccessful) {
            const data = result.data;

            summary.innerHTML = `
                <div class="row">
                   <div class="col-md-6">
                        ${data.startTime ? `<p><strong>Start Time:</strong> ${new Date(data.startTime).toLocaleString()}</p>` : ''}
                        ${data.eta ? `<p><strong>ETA:</strong> ${new Date(data.eta).toLocaleString()}</p>` : ''}
                        ${data.etaDifference ? `<p><strong>ETA Difference:</strong> ${data.etaDifference}</p>` : ''}
                    </div>
                    <div class="col-md-6">
                        ${data.actualArrival ? `<p><strong>Actual Arrival:</strong> ${new Date(data.actualArrival).toLocaleString()}</p>` : ''}
                        ${data.currentTime ? `<p><strong>Current Time:</strong> ${new Date(data.currentTime).toLocaleString()}</p>` : ''}
                    </div>
                </div>
            `;
            summary.classList.remove('d-none');

            data.activityResponse.forEach((activity, index) => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${index + 1}</td>
                    <td>${activity.name}</td>
                    <td>${new Date(activity.time).toLocaleTimeString()}</td>
                    <td>${activity.timeElapsed}</td>
                `;
                timeline.appendChild(row);
            });

            table.classList.remove('d-none');
        } else {
            showActivityAlert("Failed to fetch delivery activity.", "warning");
        }
    } catch (err) {
        console.error(err);
        showActivityAlert("An error occurred while loading delivery activity.", "danger");
    } finally {
        loader.classList.add('d-none');
    }
}
function showActivityAlert(message, type = 'danger') {
    const alertContainer = document.getElementById('activityAlertContainer');
    if (!alertContainer) return;

    alertContainer.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
}

setInterval(() => {
    if (dineInTable) dineInTable.ajax.reload(null, false); // false = don't reset paging
    if (takeAwayTable) takeAwayTable.ajax.reload(null, false);
    if (tablelist) tablelist.ajax.reload(null, false);
}, 60000)