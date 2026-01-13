const { DashboardUtils } = await import(`../dashboard/dashboard-utils.js?v=${Date.now()}`);

export const DashboardApi = {
    getDrivers() {
        return DashboardUtils.fetchJson("/driver/getDrivers");
    },

    getSupervisors() {
        return DashboardUtils.fetchJson("/getAvailableSupervisors");
    },

    checkRedispatch(voucherCode) {
        return DashboardUtils.postJson(
            "/checkRedispatchEligibility",
            { voucherCode }
        );
    },

    dispatchOrder(order) {
        return DashboardUtils.postJson(
            "/dispatch",
            order
        );
    },

    assignSupervisor(data) {
        return DashboardUtils.postJson(
            "/assignSupervisor",
            data
        );
    },

    getBranches(tin) {
        return DashboardUtils.fetchJson(`/getCompanyBranches?tin=${tin}`);
    },

    changeBranch(data) {
        return DashboardUtils.postJson(
            "/changeBranch",
            data
        );
    }
};
