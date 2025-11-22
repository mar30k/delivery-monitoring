var js = jQuery.noConflict(true);
var tablelist;
var existingVouchers = []; // Track existing voucher codes in new data
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

//datatable initailization
js(() => {
    tablelist = js('#tablelist').DataTable({
        responsive: true,
        order: [[6, "desc"]],
        pageLength: 50,
        lengthMenu: [[10, 15, 25, 50, 100], [10, 15, 25, 50, 100]],
        columnDefs: [
            { orderable: false, targets: [0, 4, 7, 8, 9, 10, 11, 13] }
        ],
        language: {
            emptyTable: "No orders to display at the moment."
        },
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

            var columnData = api.column(12, { page: "current" }).data();
            var pageTotal = columnData.reduce(function (a, b) {
                return parseValue(a) + parseValue(b);
            }, 0);

            js(api.column(12).footer()).html(
                pageTotal.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
            );
        },
    });

    // Safe to register custom filter here
    js.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
        const selectedSupervisor = js('#supervisorSelect').val();
        const rowNode = tablelist.row(dataIndex).node(); // This is now safe
        const supervisorAttr = js(rowNode).attr('data-supervisor');

        if (!selectedSupervisor || selectedSupervisor === "") {
            return true;
        }

        return supervisorAttr === selectedSupervisor;
    });

    // Event binding stays here
    js('#supervisorSelect').on('change', function () {
        tablelist.draw(); // Triggers filter
    });
    setInterval(fetchAndRender, 10000);
});

//get available supervisors request
let supervisors = null;
function getAvailableSupervisors() {
    return $.ajax({
        url: "/getAvailableSupervisors",
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

        let orderJson = JSON.stringify(order).replace(/"/g, '&quot;'); // Escape double quotes for HTML
        let redispatch = (order.status.toLowerCase() === "drivernotfound" || order.status.toLowerCase() === "declined" || order.status.toLowerCase() === "requested"
            || order.status.toLowerCase() === "sos" || order.status.toLowerCase() === "assigned")
            ? `<a href="#" data-order="${orderJson}" onclick="openRedispatchModal(this)">Redispatch</a>`
            : '';
        let assign = (order.supervisedBy === null || order.supervisedBy === undefined)
            ? `<a class="btn btn-outline-dark btn-sm" onclick="openAssignSupervisorModal('${order.voucherCode}')">Assign</a>`
            : `<a href="tel:${order.supervisedBy}">${supervisor ? supervisor.firstName + ' ' + supervisor.secondName : order.supervisedBy}</a> 
            <a onclick="openAssignSupervisorModal('${order.voucherCode}')"> <i class="fa-solid fa-pen-to-square"></i></a>`;
        newRow.innerHTML = `                    
                    <td>
                        ${order.voucherCode}
                        <a onclick="copyToClipboard('${order.voucherCode}')" title="Copy to clipboard" class="text-secondary text-decoration-none">
                            <i class="bi bi-clipboard"></i>
                        </a>
                    </td>
                    <td class="text-center">${order.companyName || 'N/A'}</td>
                    <td class="text-center">
                        ${order.branchName || 'N/A'}
                        <a onclick="openChangeBranchModal('${order.companyTin}','${order.companyName}','${order.branchName}','${order.voucherCode}')"> <i class="fa-solid fa-pen-to-square"></i></a>
                    </td>
                    <td class="text-center">${order.customerFirstName || 'N/A'}</td>
                    <td class="text-center">
                        <div class="d-inline-flex align-items-center gap-1">
                            <a href="tel:${order.customerPhoneNumber}">${order.customerPhoneNumber || 'N/A'}</a>
                                ${order.customerPhoneNumber ? `
                                    <a href="#" onclick="copyToClipboard('${order.customerPhoneNumber}')" title="Copy to clipboard" >
                                        <i class="bi bi-clipboard"></i>
                                    </a>` : ''
                                }
                        </div>
                    </td>
                    <td class="text-center">${order.customerGeocodeAddress} - ${order.customerSpecificAddress}</td>

                    <td data-order="${order.createdAt}" data-iso="${order.createdAtString}" class="text-center">
                        ${requestCreatedAt}
                    </td>
                    <td class="status-cell text-center  ${textColorClass}" style="background: ${color}">
                        <span class="p-1"
                            data-bs-toggle="tooltip"
                            data-bs-placement="top"
                            title="${order.status === 'sos' ? order.sosReason ?? '' : ''}"
                            >
                            ${order.status}
                        </span>
                    </td>
                    <td class="text-center">${clean(order.statusReport ?? '-')}</td>
                    <td class="text-center ${order.orderPrinted ? 'text-success' : 'text-danger'}">${order.orderPrinted ? 'Yes': 'No'}</td>
                    <td class="driver-cell text-center" >
                        <div class="d-inline-flex align-items-center gap-1">
                            <a href="tel:${order.assignedDriverPhoneNumber}">${order.assignedDriverPhoneNumber || 'N/A'}</a>
                                ${order.assignedDriverPhoneNumber ? `
                                    <a href="#" onclick="copyToClipboard('${order.assignedDriverPhoneNumber}')" title="Copy to clipboard" >
                                        <i class="bi bi-clipboard"></i>
                                    </a>` : ''
                                }
                        </div>
                    </td>
                    <td class="text-center">${assign}</td>
                    <td class="text-center">${order.grandTotal?.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) ?? 'N/A'}</td>
                    <td style="text-align: center; vertical-align: middle;">
                        <div style="display: inline-block;">
                            <a href="/order/${order.voucherCode}">Details</a>
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
const clean = (s) => new DOMParser().parseFromString(s, 'text/html').body.textContent || '-';
// time date format
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

// open redispatch modal
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

//load available drivers for redispatch modal initailization
function loadAvailableDrivers() {
    const $select = $("#driverSelect");
    $select.html(`<option value="" selected disabled>Loading drivers...</option>`);

    $.ajax({
        url: "driver/getDrivers",
        method: "GET",
        success: function (drivers) {
            if (!drivers || drivers.length === 0) {
                $select.html(`<option disabled>No available drivers found</option>`);
                return;
            }
            drivers.sort((a, b) => a.firstName.localeCompare(b.firstName));
            $select.empty().append(`<option value="" selected disabled>Select a driver</option>`);
            drivers.forEach(driver => {
                const option = `<option value="${driver.phoneNumber}">${driver.firstName} (${driver.phoneNumber}) (${driver.status})</option>`;
                $select.append(option);
            });
        },
        error: function () {
            $select.html(`<option disabled>Error loading drivers</option>`);
        }
    });
}

//confirm dispatch to all dirvers
function confirmRedispatchToAll() {
    if (!selectedOrder) return;
    selectedOrder.assignedDriverPhoneNumber = "";
    dispatchOrder(selectedOrder);
}

//confirm dispatch to a dirver
function confirmRedispatchToDriver() {
    const driverPhone = document.getElementById("driverSelect")?.value;
    if (!driverPhone) {
        alert("Please select a driver.");
        return;
    }
    selectedOrder.assignedDriverPhoneNumber = driverPhone;
    dispatchOrder(selectedOrder);
}

//dispatch order request
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

// check order status for redispatching
async function checkOrderStatus(voucherCode) {
    try {
        await $.ajax({
            url: "/Order/checkRedispatchEligibility", // Replace with your actual endpoint
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ voucherCode: voucherCode })
        });

        return true; // Order is dispatchable
    } catch (error) {
        return false; // Order is not dispatchable or call failed
    }
}

//show loading
function showLoading(id) {
    document.getElementById(id).classList.remove("d-none");
}

//hide loading
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
//show assign alert
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

//assign supervisor request
async function assignSupervisor(voucherCode, phoneNumber = "all") {
    showLoading("assignLoading");
    var data = {
        voucherCode,
        phoneNumber
    };

    $.ajax({
        url: "/order/assignSupervisor",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (response) {
            hideLoading("assignLoading");
            $("#assignSupervisor").modal("hide");
            Toastify({
                text: "Supervisor assigned successfully!",
                style: {
                    background: "green",
                },
                duration: 3000,
                gravity: "top",
                position: "right"
            }).showToast();

            // Reload the page after 1 second
            setTimeout(() => {
                location.reload();
            }, 1000);
        },
        error: function (xhr) {
            hideLoading("assignLoading");
            Toastify({
                text: "Error assigning supervisor.",
                style: { background: "red", },
                duration: 3000,
                gravity: "top",
                position: "right"
            }).showToast();        }
    });
}

//assign suupervisor modal opening
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

//load supervisors for supervisor modal
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

            $select.empty().append(`<option value="" selected disabled>Select a supervisor</option>`); //userName value="${supervisor.userName}"
            loggedInSupervisors.forEach(supervisor => {
                const option = `<option value="${supervisor.userName}">
                ${supervisor.firstName || ''} ${supervisor.secondName || ''}
            </option>`;
                $select.append(option);
            });
        })
        .catch(() => {
            $select.html(`<option disabled>Error loading supervisors</option>`);
        });
}



//assign randomly from all supervisor
function confirmAssignToAll() {
    if (!assignSupervisorVoucherCode) return;
    assignSupervisor(assignSupervisorVoucherCode);
}

//assign a specific supervisor
function confirmAssignToSupervisor() {
    const supervisorPhone = document.getElementById("modalSupervisorSelect")?.value;
    if (!supervisorPhone) {
        alert("Please select a supervisor.");
        return;
    }
    assignSupervisor(assignSupervisorVoucherCode, supervisorPhone);
}


function openChangeBranchModal(companyTin, companyName, currentBranchName, voucherCode) {
    $("#branchSelectDropdown").html(`<option disabled selected>Loading branches...</option>`);
    $('#companyNameLable').text(`- ${companyName}`);
    $("#changeBranchModal").modal("show");
    $("#voucherCodeInput").val(voucherCode || "");
    $.ajax({
        url: `/getCompanyBranches?tin=${companyTin}`,
        method: "GET",
        success: function (response) {
            if (!response.isSuccessful || !response.data || !response.data.branches) {
                $("#branchSelectDropdown").html(`<option disabled selected>No branches found</option>`);
                return;
            }
            $('#companyNameLable').text(`- ${response.data.brandName}`);

            const branches = response.data.branches;

            let options = "";

            branches.forEach(branch => {
                if (branch.name.toLowerCase() !== currentBranchName.toLowerCase()) {
                    options += `
                        <option value="${branch.code}">
                            ${branch.name}
                        </option>
                    `;
                }
            });

            options += `
                <option value="" disabled selected>
                    ${currentBranchName} (Current)
                </option>
            `;
            if (options === "") {
                options = `<option disabled>No other branches available</option>`;
            }

            $("#branchSelectDropdown").html(options);
        },
        error: function () {
            $("#branchSelectDropdown").html(`<option disabled>Error loading branches</option>`);
        }
    });
}

function confirmBranchChange() {
    const selectedBranchCode = $("#branchSelectDropdown").val();
    const voucherCode = $("#voucherCodeInput").val();
    const selectedBranchName = $("#branchSelectDropdown option:selected").text().trim(); // <-- get branch name
    const remark = $("#remarkInput").val();
    if (!selectedBranchCode) {
        Toastify({ text: "Please select a branch.", style: { background: "red" } }).showToast();
        return;
    }


    var data = {
        branchName: selectedBranchName,
        branchCode: selectedBranchCode,
        voucherCode: voucherCode,
        remark: remark
    };
    console.log(data);
    $("#branchChangeLoading").removeClass("d-none");

    $.ajax({
        url: "/changeBranch",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function () {
            $("#branchChangeLoading").addClass("d-none");
            $("#changeBranchModal").modal("hide");

            Toastify({
                text: "Branch changed successfully!",
                style: { background: "green" }
            }).showToast();

            // Reload the page after 1 second
            setTimeout(() => {
                location.reload();
            }, 2000);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $("#branchChangeLoading").addClass("d-none");

            // Try to get server message from response
            let msg = jqXHR.responseJSON?.message || errorThrown || textStatus || "Unknown error";

            Toastify({
                text: `Error changing branch. ${msg}`,
                style: { background: "red" }
            }).showToast();
        }
    });
}
