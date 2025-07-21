var js = jQuery.noConflict(true);
var tablelist;
var existingVouchers = []; // Track existing voucher codes in new data
const statusColors = {
    requested: "deepskyblue",
    arrivedatbranch: "orange",
    assigned: "lawngreen",
    declined: "red",
    accepted: "seagreen",
    arrived: "coral",
    ontheway: "darkorange",
    drivernotfound: "red",
    sos: "darkred",
    default: "yellow"
};

//datatable initailization
js( ()=> {
    tablelist = js('#tablelist').DataTable({
        responsive: true,
        "order": [[4, "desc"]],
        "pageLength": 15,
        "lengthMenu": [[10, 15, 25, 50, 100], [10, 15, 25, 50, 100]],
        columnDefs: [
            { orderable: false, targets: [0, 2, 5, 6, 7,9] }
        ],
        language: {
            "emptyTable": "No orders to display at the moment."
        }
    });
    setInterval(fetchAndRender, 10000);
});

//get available supervisors request
let supervisors = null;
function getAvailableSupervisors() {
    return $.ajax({
        url: "order/getAvailableSupervisors",
        method: "GET"
    });
}

//fetch supervisors and update orders
async function fetchAndRender() {
    try {
        supervisors = await getAvailableSupervisors();
        updateOrders();
    } catch (err) {
        console.error("Failed to load supervisors:", err);
    }
}

//supervisor select filter
js('#supervisorSelect').on('change', function () {
    var selectedSupervisor = js(this).val();

    tablelist.rows().every(function () {
        var row = this.node();
        var supervisor = js(row).data('supervisor');

        // Show all rows if 'All Supervisors' is selected
        if (!selectedSupervisor || selectedSupervisor === "") {
            js(row).show();
        } else {
            js(row).toggle(supervisor === selectedSupervisor);
        }
    });

    tablelist.draw(false); // Redraw without changing pagination
});

//update orders
function updateOrders() {
    let existingVouchers = new Set();  // To track existing voucher codes in the table
    let previousDates = new Map();     // Store previous requestCreatedAtIso values

    // Step 1: Retrieve previous requestCreatedAtIso values from DataTables
    tablelist.rows().every(function () {
        const rowNode = this.node(); // Get the actual row node
        const voucherCode = js(rowNode).find('td:nth-child(1)').text().trim(); // Adjust index if needed
        const requestCreatedAtIso = js(rowNode).find('td[data-iso]').attr('data-iso');

        if (voucherCode && requestCreatedAtIso) {
            previousDates.set(voucherCode, requestCreatedAtIso);
        }
    });

    let currentPage = tablelist.page.info().page;

    // First, clear all rows from the table (across all pages)
    const tbody = document.querySelector('#tablelist tbody');
    tbody.innerHTML = ''; // This clears the entire table body


    const superVisorSelect = document.getElementById("supervisorSelect");
    while (superVisorSelect.options.length > 1) { superVisorSelect.remove(1) }
    let disTinctSupervisors = Array.from(
        new Map(
            data
                .filter(item => item.supervisedBy != null)
                .map(item => [item.supervisedBy, item])
        ).values()
    );

    if (Array.isArray(disTinctSupervisors) && Array.isArray(supervisors)) {
            disTinctSupervisors.forEach(supervisor => {
            if (!supervisor?.supervisedBy) return; // skip if key missing
            const matched = supervisors.find(s => s.userName === supervisor.supervisedBy);
                const option = document.createElement("option");
            option.value = supervisor.supervisedBy;
                const supervisorName = matched?.firstName ? matched.firstName + " " : "";
                option.textContent = `${supervisorName}${supervisor.supervisedBy}`;

                superVisorSelect.appendChild(option);
            });
    } else {
        console.warn("disTinctSupervisors or supervisors is not a valid array");
    }
    // Add the new rows or update existing ones
    data.forEach(order => {
        const status = order.status.toLowerCase() || "default";
        const color = statusColors[status] || statusColors["default"];
        const textColorClass = (color === "lawngreen" || color === "yellow") ? "text-black" : "text-white";
        let requestCreatedAt = previousDates.get(order.voucherCode) || formatDateTime(order.createdAt);
        let newRow = document.createElement('tr');
        newRow.setAttribute('data-voucher', order.voucherCode);
        newRow.setAttribute('data-supervisor', order.supervisedBy);
        newRow.style.fontSize = "13px";

        if (Array.isArray(supervisors)) {
            supervisor = supervisors.find(x => x.userName === order.supervisedBy);
        }

        let assignedDriverPhoneNumber = order.assignedDriverPhoneNumber
            ? `<a href="tel:${order.assignedDriverPhoneNumber}">${order.assignedDriverPhoneNumber}</a>`
            : "N/A";
        let orderJson = JSON.stringify(order).replace(/"/g, '&quot;'); // Escape double quotes for HTML
        let redispatch = (order.status.toLowerCase() === "drivernotfound" || order.status.toLowerCase() === "declined" || order.status.toLowerCase() === "requested"
            || order.status.toLowerCase() === "sos" || order.status.toLowerCase() === "assigned")
            ? `<a class="btn btn-outline-dark btn-sm" data-order="${orderJson}" onclick="openRedispatchModal(this)">Redispatch</a>`
            : '';
        let assign = (order.supervisedBy === null || order.supervisedBy === undefined)
            ? `<a class="btn btn-outline-dark btn-sm" onclick="openAssignSupervisorModal('${order.voucherCode}')">Assign</a>`
            : `<a href="tel:${order.supervisedBy}">${supervisor ? supervisor.firstName + ' ' + supervisor.secondName : order.supervisedBy}</a> <a onclick="openAssignSupervisorModal('${order.voucherCode}')"> <i class="fa-solid fa-pen-to-square"></i></a>`;
        newRow.innerHTML = `                    
                    <td>${order.voucherCode}</td>
                    <td class="text-center">${order.customerFirstName || 'N/A'}</td>
                    <td class="text-center"><a href="tel:${order.customerPhoneNumber}">${order.customerPhoneNumber || 'N/A'}</a></td>
                    <td class="text-center">${order.customerGeocodeAddress} - ${order.customerSpecificAddress}</td>

                    <td data-order="${order.createdAt}" data-iso="${order.createdAtString}" class="text-center">
                        ${requestCreatedAt}
                    </td>
                    <td class="status-cell text-center  ${textColorClass}" style="background: ${color}">${order.status}</td>
                    <td class="driver-cell text-center" >${assignedDriverPhoneNumber}</td>
                    <td class="text-center">${assign}</td>
                    <td class="text-center">${order.grandTotal?.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) ?? 'N/A'}</td>
                    <td style="text-align: center; vertical-align: middle;">
                        <div style="display: inline-block;">
                            <a class="btn btn-outline-dark btn-sm" href="/order/${order.voucherCode}">Details</a>
                            ${redispatch}
                        </div>
                    </td>
                `;

        tbody.appendChild(newRow);
        existingVouchers.add(order.voucherCode);
    });


    // Second, remove rows that no longer exist in the new data
    const rows = document.querySelectorAll('#tablelist tbody tr');
    rows.forEach(row => {
        const voucherCode = row.getAttribute('data-voucher');
        if (!existingVouchers.has(voucherCode)) {
            row.remove();  // Remove the row if its voucherCode is not in the updated data
        }
    });

    // Reinitialize DataTable without destroying it
    tablelist.clear();
    tablelist.rows.add(js('#tablelist tbody tr'));  // Add the newly updated rows
    // Redraw the table and retain the current page
    tablelist.draw();
    tablelist.page(currentPage).draw(false);

}

function formatDateTime(input) {
    try {
        let date = new Date(input);

        if (isNaN(date.getTime())) {
            throw new Error("Invalid date format");
        }

        let year = date.getFullYear();
        let month = String(date.getMonth() + 1).padStart(2, '0');
        let day = String(date.getDate()).padStart(2, '0');

        let hours = String(date.getHours() % 12 || 12).padStart(2, '0'); // 12-hour format with leading zero
        let minutes = String(date.getMinutes()).padStart(2, '0');
        let seconds = String(date.getSeconds()).padStart(2, '0');

        return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    } catch (error) {
        return input;
    }
}

let selectedOrder = null;

function openRedispatchModal(el) {
    try {
        const dataAttr = el.getAttribute('data-order');
        selectedOrder = JSON.parse(dataAttr.replace(/&quot;/g, '"'));
        const driverSelect = document.getElementById("driverSelect");
        if (driverSelect) {
            driverSelect.selectedIndex = 0;
        }

        hideLoading("dispatchLoading");

        const modalElement = document.getElementById('reDispatchModal');
        $('#voucherCodeLabel').text(`- ${selectedOrder.voucherCode}`);
        if (!modalElement) throw new Error("Modal element missing");
        const modal = new bootstrap.Modal(modalElement);

        $('#reDispatchModal').one('shown.bs.modal', function () {
            loadAvailableDrivers();
        });
        modal.show();
    } catch (e) {
        console.error("Failed to open modal:", e);
        alert("Error preparing dispatch. Please try again.");
    }
}

function confirmRedispatchToAll() {
    if (!selectedOrder) return;
    selectedOrder.assignedDriverPhoneNumber = "";
    dispatchOrder(selectedOrder);
}

function confirmRedispatchToDriver() {
    const driverPhone = document.getElementById("driverSelect")?.value;
    if (!driverPhone) {
        alert("Please select a driver.");
        return;
    }
    selectedOrder.assignedDriverPhoneNumber = driverPhone;
    dispatchOrder(selectedOrder);
}

async function dispatchOrder(order) {
    showLoading("dispatchLoading");
    const isDispatchable = await checkOrderStatus(order.voucherCode);
    if (isDispatchable) {
        $.ajax({
            url: "/Order/dispatch",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(order),
            success: function (response) {
                hideLoading("dispatchLoading");
                showAlert("Dispatch successful!", "success", "reDispatchModal");

                //const modalElement = document.getElementById('reDispatchModal');
                //const modal = bootstrap.Modal.getInstance(modalElement);
                //if (modal) modal.hide();
            },
            error: function (xhr) {
                hideLoading("dispatchLoading");
                console.error("Dispatch failed:", xhr.responseText);
                showAlert("Dispatch failed: " + xhr.responseText, "danger", "reDispatchModal");
            }
        });
    } else {
        hideLoading("dispatchLoading");
        showAlert("Order Can't be Redispatched", "danger", "reDispatchModal");
    }
     
}

async function checkOrderStatus(voucherCode) {
    try {
        await $.ajax({
            url: "/Order/orderdetails", // Replace with your actual endpoint
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ voucherCode: voucherCode })
        });

        return true; // Order is dispatchable
    } catch (error) {
        return false; // Order is not dispatchable or call failed
    }
}

function showLoading(id) {
    document.getElementById(id).classList.remove("d-none");
}

function hideLoading(id) {
    document.getElementById(id).classList.add("d-none");
}

// Function to show a temporary Bootstrap alert
function showAlert(message, type = "info", modal) {
    // Create alert HTML
    const alertHTML = `
                <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                    ${message}
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>
            `;

    // Insert after modal header
    const modalHeader = document.querySelector(`#${modal} .modal-header`);
    if (!modalHeader) return;

    // Remove any existing alert first
    const existingAlert = document.querySelector(`#${modal} .alert`);
    if (existingAlert) existingAlert.remove();

    // Insert new alert
    modalHeader.insertAdjacentHTML('afterend', alertHTML);

    // Auto-close after 5 seconds
    setTimeout(() => {
        const newAlert = document.querySelector(`#${modal} .alert`);
        if (newAlert) {
            bootstrap.Alert.getOrCreateInstance(newAlert).close();
        }
    }, 5000);
}
function showAssignAlert(message, type = "info") {
    const alertHTML = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    $('#alertContainer').html(alertHTML);

    setTimeout(() => {
        $('.alert').alert('close');
        if (type === "success") {
            location.reload();
        }
    }, 3000);
}

async function assignSupervisor(voucherCode, id = "all") {
    showLoading("assignLoading");
    var data = {
        voucherCode,
        id
    };

    $.ajax({
        url: "/order/assignSupervisor",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (response) {
            hideLoading("assignLoading");
            showAlert("Supervisor Assignment successful!", "success", "assignSupervisor");
        },
        error: function (xhr) {
            hideLoading("assignLoading");
            showAlert("Supervisor Assignment failed " , "danger", "assignSupervisor");
        }
    });
}
let assignSupervisorVoucherCode = null;

function openAssignSupervisorModal(voucherCode) {
    try {
        const supervisor = document.getElementById("modalSupervisorSelect");
        if (supervisor) {
            supervisor.selectedIndex = 0;
        }
        assignSupervisorVoucherCode = voucherCode;
        hideLoading("assignLoading");

        const modalElement = document.getElementById('assignSupervisor');
        if (!modalElement) throw new Error("Modal element missing");

        $('#assignVoucherCodeLabel').text(`- ${voucherCode}`);

        const modal = new bootstrap.Modal(modalElement);

        $('#assignSupervisor').one('shown.bs.modal', function () {
            loadAvailableSupervisors();
        });

        modal.show();
    } catch (e) {
        console.error("Failed to open modal:", e);
        alert("Error preparing supervisor assignment. Please try again.");
    }
}


function getAvailableSupervisors() {
    return $.ajax({
        url: "order/getAvailableSupervisors",
        method: "GET"
    });
}

function loadAvailableSupervisors() {
    const $select = $('#modalSupervisorSelect');

    getAvailableSupervisors()
        .then(supervisors => {
            if (!supervisors || supervisors.length === 0) {
                $select.html(`<option disabled>No available supervisors found</option>`);
                return;
            }
            const loggedInSupervisors = supervisors.filter(s => s.loggedInStatus === true);

            if (loggedInSupervisors.length === 0) {
                $select.html(`<option disabled>No logged-in supervisors found</option>`);
                return;
            }

            $select.empty().append(`<option value="" selected disabled>Select a supervisor</option>`);
            loggedInSupervisors.forEach(supervisor => {
                const option = `<option value="${supervisor.id}">
                ${supervisor.firstName || ''} ${supervisor.secondName || ''}
            </option>`;
                $select.append(option);
            });
        })
        .catch(() => {
            $select.html(`<option disabled>Error loading supervisors</option>`);
        });
}




function confirmAssignToAll() {
    if (!assignSupervisorVoucherCode) return;
    assignSupervisor(assignSupervisorVoucherCode);
}

function confirmAssignToSupervisor() {
    const supervisorId = document.getElementById("modalSupervisorSelect")?.value;
    if (!supervisorId) {
        alert("Please select a supervisor.");
        return;
    }
    assignSupervisor(assignSupervisorVoucherCode, supervisorId);
}