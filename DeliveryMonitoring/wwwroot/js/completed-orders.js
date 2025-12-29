const js = jQuery.noConflict(true);
const tableDateRanges = {};
const tables = {};

var TableConfigs = window.AppData?.tableConfigs || [];

// Build initial date ranges and table mappings
(TableConfigs || []).forEach(cfg => {
    tableDateRanges[cfg.TableId] = {
        start: moment().startOf('day'),
        end: moment().endOf('day'),
        isClear: false
    };
});

var dateFilterMappings = (TableConfigs || []).map(cfg => {
    return {
        id: `${cfg.TableId}DateRange`,
        tableName: cfg.TableId
    };
});

// Column sets
const baseColumns = [
    { data: "voucherCode", className: "text-center", render: Renderers.voucherCode },
    { data: "companyName", className: "text-center" },
    { data: "branchName", className: "text-center" },
    { data: "firstName", className: "text-center" },
    { data: "phoneNumber", className: "text-center", render: Renderers.phone },
    {
        data: "requestCreatedAt",
        className: "text-center",
        render: Renderers.requestDate.render,
        createdCell: Renderers.requestDate.createdCell
    }
];

const dineInAndTakeawayColumns = [
    ...baseColumns,
    { data: "supervisorName", className: "text-center" },
    { data: "totalAmount", className: "text-center", render: Renderers.amount },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.reviewOrShow(r, false) },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.activityBtn(r) },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.detailsLink(r) }
];

const deliveryColumns = [
    ...baseColumns,
    { data: "distance", className: "text-center", render: Renderers.distance },
    { data: "duration", className: "text-center", render: Renderers.duration },
    { data: "eta", className: "text-center", render: Renderers.duration },
    { data: "etaDifference", className: "text-center", render: Renderers.timeDeviationRenderer },
    { data: "driverPhoneNumber", className: "text-center", render: Renderers.phone },
    { data: "supervisorName", className: "text-center", render: Renderers.orDefault },
    { data: "totalAmount", className: "text-center", render: Renderers.amount },
    { data: "tip", className: "text-center", render: Renderers.amount },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.reviewOrShow(r, true) },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.activityBtn(r) },
    { data: null, className: "text-center", orderable: false, render: (d, t, r) => Renderers.detailsLink(r) }
];


const TableTypeConfigs = {
    "_DeliveryOrders": {
        columns: deliveryColumns,
        totalColumnIndex: [12, 13],
        nonOrderableTargets: [0, 4, 10, 11, 14, 15, 16],
        headerFilterColumns:  [
            { index: 1, name: 'Company' },
            { index: 2, name: 'Branch' },
            { index: 3, name: 'Customer' },
            { index: 11, name: 'Supervisor' }
        ]
    },
    "_NonDeliveryOrders": {
        columns: dineInAndTakeawayColumns,
        totalColumnIndex: [7],
        nonOrderableTargets: [0, 4, 8, 9],
        headerFilterColumns: [
            { index: 1, name: 'Company' },
            { index: 2, name: 'Branch' },
            { index: 3, name: 'Customer' },
            { index: 6, name: 'Supervisor' }
        ]
    }
};

// Initialize tables dynamically
js(() => {
    (TableConfigs || []).forEach(cfg => {
        const selector = `#${cfg.TableId}`;
        const datePickerId = `#${cfg.TableId}DateRange`;

        const tableConfig = TableTypeConfigs[cfg.SheetName] || TableTypeConfigs["_NonDeliveryOrders"];

        // Initialize DataTable
        tables[cfg.TableId] = initOrderTable(
            selector,
            datePickerId,
            tableConfig.columns,
            cfg.AjaxUrl,
            `No ${cfg.Title.toLowerCase()} orders available.`,
            tableConfig.totalColumnIndex,
            tableConfig.nonOrderableTargets,
            tableConfig.headerFilterColumns
        );
    });

    // Initialize date range pickers dynamically
    initDateRangePickers();
});
// Date range pickers dynamic
function initDateRangePickers() {
    (TableConfigs || []).forEach(cfg => {
        const tableName = cfg.TableId;
        const selector = `#${tableName}DateRange`;
        const tableRef = tables[tableName];
        if (!tableRef) return;

        const tableRange = tableDateRanges[tableName];

        js(selector).daterangepicker({
            startDate: tableRange.start,
            endDate: tableRange.end,
            maxDate: moment(),
            autoUpdateInput: true,
            locale: { cancelLabel: 'Clear', format: 'YYYY-MM-DD' }
        });

        js(selector).val(tableRange.start.format('YYYY-MM-DD') + ' to ' + tableRange.end.format('YYYY-MM-DD'));

        js(selector).on('apply.daterangepicker', function (ev, picker) {
            tableDateRanges[tableName] = { start: picker.startDate.startOf('day'), end: picker.endDate.endOf('day'), isClear: false };
            js(this).val(picker.startDate.format('YYYY-MM-DD') + ' to ' + picker.endDate.format('YYYY-MM-DD'));
            tableRef.settings()[0].oLanguage.sEmptyTable =
                picker.startDate.isSame(picker.endDate, 'day') ?
                    `No records found for ${picker.startDate.format('YYYY-MM-DD')}` :
                    `No records found between ${picker.startDate.format('YYYY-MM-DD')} and ${picker.endDate.format('YYYY-MM-DD')}`;
            tableRef.ajax.reload();
        });

        js(selector).on('cancel.daterangepicker', function () {
            tableDateRanges[tableName] = { start: null, end: null, isClear: true };
            js(this).val(' ');
            tableRef.ajax.reload();
        });
    });
}
function initOrderTable(selector, daterangepicker, columns, ajaxUrl, emptyMessage, totalColumnIndex, nonOrderableTargets = [0], headerFilterColumns = []) {
    js.fn.dataTable.ext.errMode = 'none';
    const table = js(selector).DataTable({
        responsive: true,
        processing: true,
        serverSide: false,
        ajax: function (data, callback, settings) {
            const url = new URL(ajaxUrl, window.location.origin);
            const tableName = selector.replace('#', '');
            const { start, end, isClear} = tableDateRanges[tableName] || {};

            if (start && end && !isClear) {
                url.searchParams.append("startDate", start.format("YYYY-MM-DD"));
                url.searchParams.append("endDate", end.format("YYYY-MM-DD"));
            }

            url.searchParams.append("isClear", isClear);

            js.ajax({
                url: url.toString(),
                type: "GET",
                dataType: "json",
                success: function (json) {
                    callback(json);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error("❌ AJAX request failed:", textStatus, errorThrown);

                    // Only display error row if table is empty
                    if (!table.data().any()) {
                        const $table = js(selector);
                        const colCount = $table.find("thead th").length;

                        $table.find("tbody").html(`
                            <tr class="text-center">
                                <td colspan="${colCount}" class="text-danger">
                                    ⚠️ Failed to load data. Please check your connection or try again.
                                </td>
                            </tr>
                        `);
                    }

                    table.processing(false);
                    // Do NOT clear existing table data
                    callback(); // just call callback without data
                }
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

            totalColumnIndex.forEach(function (colIndex) {
                var columnData = api.column(colIndex, { page: "current" }).data();

                var pageTotal = columnData.reduce(function (a, b) {
                    return parseValue(a) + parseValue(b);
                }, 0);

                js(api.column(colIndex).footer()).html(
                    pageTotal.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
                );
            });
            
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
    const supervisorPhone = button.getAttribute('data-supervisor-phone');
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
    editBtn.setAttribute('data-supervisor-phone', supervisorPhone);
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
    const supervisorPhone = button.getAttribute('data-supervisor-phone');

    // Close any open modal before opening this one
    document.querySelectorAll('.modal.show').forEach(modalEl => {
        bootstrap.Modal.getInstance(modalEl)?.hide();
    });

    // Populate modal inputs
    document.getElementById('reviewOrderId').value = voucherCode;
    document.getElementById('supervisorPhone').value = supervisorPhone;
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
                        ${foundReview.attachment ?
                                    `<div class="mt-2">
                                        <img src="${foundReview.attachment}" 
                                         alt="Attachment" 
                                         style=" cursor: ;">
                                    </div>`
                                    : ''
                        }
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
    const supervisorPhoneNumber = document.getElementById('supervisorPhone').value || '';
    try {
        const response = await fetch('/savenote', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ voucherCode, purpose, note, isDelivery, supervisorPhoneNumber })
        });
        const responseText = await response.text();
        if (!response.ok) {
            showToast("Submission failed: " + responseText, "error");
            return;
            
        } else {
            bootstrap.Modal.getInstance(document.getElementById('reviewModal'))?.hide();
            showToast("Note saved successfully!", "success");
            location.reload();
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
    const url = `/getDeliveryActivity?voucherCode=${encodeURIComponent(voucherCode)}&companyCode=${encodeURIComponent(companyCode)}`;

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

setTimeout(() => {
    startTableAutoRefresh(
        (TableConfigs || []).map(cfg => ({
            table: tables[cfg.TableId],
            range: () => tableDateRanges[cfg.TableId]
        })),
        60000
    );
}, 2000);
