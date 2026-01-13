const { UI } = await import(`./ui.js?v=${Date.now()}`);
const { DashboardApi } = await import(`./dashboard-api.js?v=${Date.now()}`);

export const BranchChange = {
    async open(element) {
        const data = $(element).closest("span").data();

        $("#voucherCodeInput").val(data.voucher);
        $("#companyNameLable").text(`- ${data.company}`);
        $("#branchSelectDropdown").html(`<option disabled>Loading...</option>`);

        $("#changeBranchModal").modal("show");

        try {
            const res = await DashboardApi.getBranches(data.tin);
            const branches = res.data.branches.filter(
                b => b.name.toLowerCase() !== data.branch.toLowerCase()
            );

            const options = branches.length
                ? branches.map(b => `<option value="${b.code}">${b.name}</option>`).join("")
                : `<option disabled>No other branches</option>`;

            $("#branchSelectDropdown").html(options);
        } catch {
            $("#branchSelectDropdown").html(`<option disabled>Error loading branches</option>`);
        }
    },

    async confirm() {
        const data = {
            branchCode: $("#branchSelectDropdown").val(),
            branchName: $("#branchSelectDropdown option:selected").text(),
            voucherCode: $("#voucherCodeInput").val(),
            remark: $("#remarkInput").val()
        };

        if (!data.branchCode) {
            UI.toast("Please select a branch", "red");
            return;
        }

        UI.showLoading("branchChangeLoading");

        try {
            const res = await DashboardApi.changeBranch(data);

            $("#changeBranchModal").modal("hide");
            UI.toast(res.message || "Branch changed successfully!");
            setTimeout(() => location.reload(), 2000);

        } catch (err) {
            const msg =
                err?.message ||
                err?.errors?.[0] ||
                "Error changing branch";

            UI.toast(msg, "red");
        } finally {
            UI.hideLoading("branchChangeLoading");
        }
    }
};
window.BranchChange = BranchChange;