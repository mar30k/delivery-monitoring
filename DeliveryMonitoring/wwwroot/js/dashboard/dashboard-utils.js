export const DashboardUtils = {
    async fetchJson(url) {
        const res = await fetch(url);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return res.json();
    },

    async postJson(url, body) {
        const res = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        let payload = {};
        try {
            payload = await res.json();
        } catch {
            // non-json response
        }

        if (!res.ok) {
            const error = new Error(
                payload.message || "Request failed"
            );
            error.errors = payload.errors;
            error.status = res.status;
            throw error;
        }

        return payload;
    },

    today() {
        return new Date().toISOString().slice(0, 10);
    }
};