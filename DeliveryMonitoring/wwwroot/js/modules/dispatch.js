const { UI } = await import(`./ui.js?v=${Date.now()}`);
const { DashboardApi } = await import(`./dashboard-api.js?v=${Date.now()}`);

export const Dispatch = {
    selectedOrder: null,

    openModal(element) {
        try {
            this.selectedOrder = JSON.parse(
                element.getAttribute("data-order").replace(/&quot;/g, '"')
            );

            $("#driverSelect").prop("selectedIndex", 0);
            $("#voucherCodeLabel").text(`- ${this.selectedOrder.voucherCode}`);

            new bootstrap.Modal("#reDispatchModal").show();
            this.loadDrivers();
        } catch (e) {
            console.error(e);
            alert("Failed to prepare redispatch");
        }
    },

    async loadDrivers() {
        const $select = $("#driverSelect").html(`<option value="" selected disabled>Loading...</option>`);
        
        try {
            const drivers = await DashboardApi.getDrivers();
            drivers.sort((a, b) => {
                const statusOrder = a.status === 'ready' ? 0 : 1;
                const statusOrderB = b.status === 'ready' ? 0 : 1;

                return (
                    statusOrder - statusOrderB ||
                    a.firstName.localeCompare(b.firstName)
                );
            });
            $select.empty().append(`<option disabled>Select a driver</option>`);
            drivers.filter(d => !d.isDisabled).forEach(d =>
                $select.append(
                    `<option value="${d.phoneNumber}">
                        ${d.firstName} (${d.phoneNumber}) (${d.status})
                    </option>`
                )
            );
        } catch {
            $select.html(`<option disabled>Error loading drivers</option>`);
        }
    },

    async confirm(isToAll) {
        if (!this.selectedOrder) return;

        if (!isToAll) {
            const phoneNumber = document.getElementById("driverSelect")?.value;
            if (!phoneNumber) {
                UI.showAlert({
                    message: "Please select a driver.",
                    type: "warning",
                    modalId: "reDispatchModal"
                });
                return;
            }
            this.selectedOrder.assignedDriverPhoneNumber = phoneNumber;
        } else {
            this.selectedOrder.assignedDriverPhoneNumber = "";
        }

        UI.showLoading("dispatchLoading");

        try {
            await DashboardApi.dispatchOrder(this.selectedOrder);

            UI.showAlert({
                message: "Re-Dispatch Successful!",
                type: "success",
                modalId: "reDispatchModal"
            });
        } catch (err) {
            UI.showAlert({
                message: err.message || "Order can't be redispatched",
                type: "danger",
                modalId: "reDispatchModal"
            });
        } finally {
            UI.hideLoading("dispatchLoading");
        }
    }

};
window.Dispatch = Dispatch;
