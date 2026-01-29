const { UI } = await import(`./ui.js?v=${Date.now()}`);
const { DashboardApi } = await import(`./dashboard-api.js?v=${Date.now()}`);

export const SupervisorAssignment = {
    voucherCode: null,
    currentSupervisorPhone: null,

    open(voucherCode, currentSupervisorPhone) {
        this.voucherCode = voucherCode;
        this.currentSupervisorPhone = currentSupervisorPhone;
        $("#assignVoucherCodeLabel").text(`- ${voucherCode}`);
        $("#modalSupervisorSelect").prop("selectedIndex", 0);

        new bootstrap.Modal("#assignSupervisor").show();
        this.loadSupervisors();
    },

    async loadSupervisors() {
        const $select = $("#modalSupervisorSelect");

        try {
            const supervisors = await DashboardApi.getSupervisors();
            console.log(this.currentSupervisorPhone, supervisors) 

            const loggedIn = supervisors.filter(
                s => s.loggedInStatus && s.userName !== this.currentSupervisorPhone
            );


            if (!loggedIn.length) {
                $select.html(`<option disabled>No other supervisors online</option>`);
                return;
            }

            $select.html(`<option disabled>Select supervisor</option>`);
            loggedIn.forEach(s =>
                $select.append(
                    `<option value="${s.userName}">
                        ${s.firstName} ${s.secondName}
                    </option>`
                )
            );
        } catch {
            $select.html(`<option disabled>Error loading supervisors</option>`);
        }
    },

    async assign(isToAll) {
        let phoneNumber;
        if (!this.voucherCode) return;
        if (!isToAll) {
            phoneNumber = document.getElementById("modalSupervisorSelect")?.value;
            if (!phoneNumber) {
                UI.toast("Please select a supervisor.", "red")
                return;
            }
        }
        else {
            phoneNumber = "all"
        }
        UI.showLoading("assignLoading");

        try {
            const res = await DashboardApi.assignSupervisor({
                voucherCode: this.voucherCode,
                phoneNumber
            });

            $("#assignSupervisor").modal("hide");
            UI.toast(res?.message || "Supervisor assigned successfully!");
            setTimeout(() => location.reload(), 1000);
        } catch (err){
            UI.toast(
                err?.message || err.errors?.[0] || "Error assigning supervisor",
                "red"
            );
        } finally {
            UI.hideLoading("assignLoading");
        }
    }
};
window.SupervisorAssignment = SupervisorAssignment;