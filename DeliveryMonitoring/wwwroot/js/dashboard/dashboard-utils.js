window.DashboardUtils = {
    fetchJson: async function (url) {
        const res = await fetch(url);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return res.json();
    },

    today: function () {
        return new Date().toISOString().slice(0, 10);
    }
};