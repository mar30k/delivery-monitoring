(function (global) {
    let tableReloadInterval = null;
    let lastReloadTime = Date.now();
    let isReloading = false;

    const logWithTime = (...args) => {
        const now = moment().format('YYYY-MM-DD HH:mm:ss');
        console.log(`[${now}]`, ...args);
    };

    global.startTableAutoRefresh = function (tables, intervalMs = 60000) {
        const reloadTables = async () => {
            // Prevent overlapping reloads
            if (isReloading) {
                return;
            }
            isReloading = true;

            const today = moment();
            const reloadPromises = [];

            for (const { table, range } of tables) {
                const currentRange = typeof range === 'function' ? range() : range;
                if (!table || !currentRange) continue;

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

                if (inRange) {
                    reloadPromises.push(
                        new Promise((resolve) => {
                            try {
                                table.ajax.reload(() => resolve(), false);
                            } catch (err) {
                                console.error("Table reload crashed:", err);
                                resolve();
                            }
                        })
                    );
                }
            }

            try {
                await Promise.all(reloadPromises);
            } catch (err) {
                console.error("Error during table reload:", err);
            } finally {
                isReloading = false;
                lastReloadTime = Date.now();
            }
        };

        const startInterval = () => {
            if (tableReloadInterval) clearInterval(tableReloadInterval);
            tableReloadInterval = setInterval(() => {
                if (document.visibilityState === 'visible') reloadTables();
            }, intervalMs);
            logWithTime(`Auto-refresh interval started (every ${intervalMs / 1000}s).`);
        };

        startInterval();

        document.addEventListener('visibilitychange', () => {
            if (document.visibilityState === 'visible') {
                const now = Date.now();
                const elapsed = now - lastReloadTime;

                if (elapsed >= intervalMs) {
                    logWithTime("Tab became visible and interval passed, reloading tables...");
                    reloadTables();
                } else {
                    logWithTime(`Tab visible — only ${Math.round(elapsed / 1000)}s elapsed, skipping immediate reload.`);
                }

                startInterval();
            } else {
                if (tableReloadInterval) {
                    clearInterval(tableReloadInterval);
                    tableReloadInterval = null;
                    logWithTime("Auto-refresh paused (tab hidden).");
                }
            }
        });
    };

    global.stopTableAutoRefresh = function () {
        if (tableReloadInterval) {
            clearInterval(tableReloadInterval);
            tableReloadInterval = null;
            logWithTime("Auto-refresh stopped.");
        }
    };
})(window);
