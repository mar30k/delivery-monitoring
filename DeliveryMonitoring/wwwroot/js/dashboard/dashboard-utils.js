export const DashboardUtils = {
    async fetchJson(url) {
        const res = await fetch(url);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return res.json();
    },

    async postJson(url, payload) {
        const res = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!res.ok) throw await res.json();
        return res.json();
    },

    today() {
        return new Date().toISOString().slice(0, 10);
    }
};