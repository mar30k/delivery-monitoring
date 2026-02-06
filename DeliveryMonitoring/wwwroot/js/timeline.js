(function ($) {
    let lastDataHash = "";
    let timelineDateRange = null;

    // --- 1. Filter & UI Logic ---
    function applyActivityFilter(mode) {
        // Find all items within the container and toggle visibility
        $('.activity-timeline-item').each(function () {
            const lat = parseFloat($(this).data('lat'));
            const lng = parseFloat($(this).data('lng'));

            if (mode === 'all') {
                $(this).show();
            } else {
                // Driver Only: Show if lat/lng are valid and not zero
                const isDriver = !isNaN(lat) && !isNaN(lng) && lat !== 0 && lng !== 0;
                isDriver ? $(this).show() : $(this).hide();
            }
        });
    }

    // --- 2. Pagination Core ---
    window.setPage = function (page) {
        const $orders = $('.paged-order');
        const pageSize = 15;
        const totalItems = $orders.length;
        const totalPages = Math.ceil(totalItems / pageSize);

        if (page < 1 || page > totalPages) return;

        const startIdx = (page - 1) * pageSize;
        const endIdx = startIdx + pageSize;

        // Toggle Order Strips
        $orders.each((idx, el) => {
            $(el).toggle(idx >= startIdx && idx < endIdx);
        });

        // Update Info Text
        $('#paginationInfo').html(`Showing <b>${startIdx + 1}</b> - <b>${Math.min(endIdx, totalItems)}</b> of <b>${totalItems}</b> orders`);

        // Build Buttons
        let html = `<button class="pg-btn" ${page === 1 ? 'disabled' : ''} onclick="window.setPage(${page - 1})">Prev</button>`;
        for (let i = 1; i <= totalPages; i++) {
            if (i === 1 || i === totalPages || (i >= page - 1 && i <= page + 1)) {
                html += `<button class="pg-btn ${i === page ? 'active' : ''}" onclick="window.setPage(${i})">${i}</button>`;
            } else if (i === page - 2 || i === page + 2) {
                html += `<span class="pg-dots">...</span>`;
            }
        }
        html += `<button class="pg-btn" ${page === totalPages ? 'disabled' : ''} onclick="window.setPage(${page + 1})">Next</button>`;
        $('#paginationList').html(html);

        // Re-apply current filter to the new page
        applyActivityFilter($('.filter-btn.active').data('filter') || 'all');
    };

    // --- 3. AJAX Refresh Logic ---
    function refreshTimeline() {
        if (!timelineDateRange) return;

        const range = timelineDateRange.getRange();
        if (range.isClear) return;

        const startStr = range.start.format('YYYY-MM-DD');
        const endStr = range.end.format('YYYY-MM-DD');
        const currentFilter = $('.filter-btn.active').data('filter') || 'all';

        $('#loader').removeClass('d-none');

        $.ajax({
            url: '/gettimeLineOrder',
            method: 'GET',
            data: { startDate: startStr, endDate: endStr, filter: currentFilter },
            headers: { "X-Requested-With": "XMLHttpRequest" },
            success: function (result, status, xhr) {
                $('#loader').addClass('d-none');
                const newHash = xhr.getResponseHeader("X-Data-Hash");

                if (newHash && newHash === lastDataHash) {
                    return; // No changes, keep current DOM
                }

                lastDataHash = newHash;

                // Update Container
                const $container = $('#timeline-container');
                $container.html(result); // result already contains buttons with correct active class

                // Re-init local logic for the new HTML
                const newFilter = $('.precision-monitor').data('active-filter') || 'all';
                applyActivityFilter(newFilter);
                window.setPage(1);
            },
            error: function () {
                $('#loader').addClass('d-none');
                console.error("Timeline update failed");
            }
        });
    }

    // --- 4. Initialization & Event Delegation ---
    $(document).ready(async function () {

        // Use Delegation: Attach to #timeline-container so clicks work after AJAX refresh
        $('#timeline-container').on('click', '.filter-btn', function () {
            $('.filter-btn').removeClass('active');
            $(this).addClass('active');
            applyActivityFilter($(this).data('filter'));
        });

        // Date Range Setup
        timelineDateRange = DateRange.create('#dateRange');
        await timelineDateRange.init();

        // Initial Load
        refreshTimeline();

        // Listen for changes
        js('#dateRange').on('daterange:changed', function () {
            refreshTimeline();
        });

        // Auto-refresh (3 minutes based on your setInterval logic)
        setInterval(refreshTimeline, 3 * 60 * 1000);
    });

})(jQuery);