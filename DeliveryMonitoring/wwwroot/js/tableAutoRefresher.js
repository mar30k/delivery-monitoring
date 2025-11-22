(function (global) {
    let tableReloadInterval = null;
    let lastReloadTime = Date.now();
    let isReloading = false;

    // 🔹 Helper: timestamped logger
    const logWithTime = (...args) => {
        const now = moment().format('YYYY-MM-DD HH:mm:ss');
        console.log(`[${now}]`, ...args);
    };


    /**
     * Starts automatic reloading of DataTables at a fixed interval.
     * @param {Array} tables - Array of { table, range } pairs
     * @param {number} intervalMs - Refresh interval in milliseconds
     */
    global.startTableAutoRefresh = function (tables, intervalMs = 60000) {
        const reloadTables = async () => {
            if (isReloading) return;
            isReloading = true;

            const today = moment();
            const reloadPromises = [];

            for (const { table, range } of tables) {
                const currentRange = typeof range === 'function' ? range() : range;
                if (!table || !currentRange) continue;

                logWithTime("currentRange.isClear", currentRange.isClear);

                

                const inRange =
                    currentRange.isClear ||
                    (
                        currentRange.start &&
                        currentRange.end &&
                        today.isBetween(
                            moment(currentRange.start),
                            moment(currentRange.end),
                            "day",
                            "[]"
                        )
                    );

                logWithTime(
                    "Checking range:",
                    currentRange.start ? moment(currentRange.start).format("YYYY-MM-DD") : "null",
                    "to",
                    currentRange.end ? moment(currentRange.end).format("YYYY-MM-DD") : "null",
                    "today:",
                    today.format("YYYY-MM-DD"),
                    "-> inRange:",
                    inRange
                );

                if (inRange) {
                    logWithTime("Reloading table:", table.table().node());
                    reloadPromises.push(
                        new Promise(resolve => {
                            try {
                                table.ajax.reload(() => resolve(), false);
                            } catch (err) {
                                console.error("Table reload crashed:", err);
                                resolve();
                            }
                            setTimeout(() => resolve(), 5000); // Fallback safety
                        })
                    );
                }
            }

            try {
                await Promise.all(reloadPromises);
            } catch (err) {
                console.error("Error during table reload:", err);
            } finally {
                lastReloadTime = Date.now();
                isReloading = false;
            }
        };

        const startInterval = () => {
            if (tableReloadInterval) clearInterval(tableReloadInterval);
            tableReloadInterval = setInterval(() => {
                if (document.visibilityState === 'visible') reloadTables();
            }, intervalMs);
            logWithTime(`🔁 Auto-refresh interval started (every ${intervalMs / 1000}s).`);
        };

        // Start first interval
        startInterval();

        // 🔹 Handle tab visibility changes
        document.addEventListener('visibilitychange', () => {
            if (document.visibilityState === 'visible') {
                const now = Date.now();
                const elapsed = now - lastReloadTime;

                if (elapsed >= intervalMs) {
                    logWithTime("Tab became visible and interval passed, reloading tables...");
                    reloadTables();
                } else {
                    logWithTime(`🟡 Tab visible — only ${Math.round(elapsed / 1000)}s elapsed, skipping immediate reload.`);
                }

                // restart interval cleanly
                startInterval();
            } else {
                // pause interval when hidden
                if (tableReloadInterval) {
                    clearInterval(tableReloadInterval);
                    tableReloadInterval = null;
                    logWithTime("⏸ Auto-refresh paused (tab hidden).");
                }
            }
        });

        //// 🔹 Handle going online
        //window.addEventListener('online', () => {
        //    logWithTime("📶 Came online reloading tables...");
        //    reloadTables();
        //});
    };

    global.stopTableAutoRefresh = function () {
        if (tableReloadInterval) {
            clearInterval(tableReloadInterval);
            tableReloadInterval = null;
            logWithTime("🟥 Auto-refresh stopped.");
        }
    };
})(window);
