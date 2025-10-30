const reviewsPerPage = 12;
let currentPage = 1;
let selectedRating = 0; // 0 = all
let selectedDateRange = { start: null, end: null };

// -------------------------
//  DATE RANGE INITIALIZATION
// -------------------------
$(function () {
    $('#dateRange').daterangepicker({
        autoUpdateInput: true,
        ranges: false,
        opens: 'right',
        drops: 'down',
        parentEl: 'body',
        locale: { format: 'MMMM DD, YYYY', cancelLabel: 'Clear' }
    });

    // Apply range
    $('#dateRange').on('apply.daterangepicker', function (ev, picker) {
        selectedDateRange.start = picker.startDate.startOf('day');
        selectedDateRange.end = picker.endDate.endOf('day');
        $(this).val(
            picker.startDate.format('MMMM DD, YYYY') + ' - ' + picker.endDate.format('MMMM DD, YYYY')
        );
        currentPage = 1;
        updateReviewsDisplay();
    });

    $('#dateRange').on('cancel.daterangepicker', function (ev, picker) {
        selectedDateRange.start = null;
        selectedDateRange.end = null;
        $(this).val('');
        picker.setStartDate(moment().startOf('day'));
        picker.setEndDate(moment().endOf('day'));
        currentPage = 1;
        updateReviewsDisplay();
    });
});

// -------------------------
//  MAIN FILTER FUNCTION
// -------------------------
function updateReviewsDisplay() {
    const allReviews = document.querySelectorAll('.review-card');
    let filtered = [];

    allReviews.forEach(card => {
        const rating = parseInt(card.getAttribute('data-rating'), 10);
        const dateText = card.querySelector('small.text-muted')?.textContent?.split('•')[0]?.trim();
        const reviewDate = dateText ? new Date(dateText) : null;

        const matchesRating = selectedRating === 0 || rating === selectedRating;
        const matchesDate =
            !selectedDateRange.start ||
            !selectedDateRange.end ||
            (reviewDate && reviewDate >= selectedDateRange.start && reviewDate <= selectedDateRange.end);

        if (matchesRating && matchesDate) filtered.push(card);
        card.style.display = 'none';
    });

    // Apply pagination
    const start = (currentPage - 1) * reviewsPerPage;
    const end = start + reviewsPerPage;
    filtered.slice(start, end).forEach(card => card.style.display = '');

    // Handle empty state
    const msg = document.getElementById('noReviewsMessage');
    if (filtered.length === 0) {
        const messages = {
            0: "No reviews found for this driver.",
            1: "No 1-star reviews found.",
            2: "No 2-star reviews found.",
            3: "No 3-star reviews found.",
            4: "No 4-star reviews found.",
            5: "No 5-star reviews found.",
        };
        msg.textContent = messages[selectedRating] || "No reviews found.";
        msg.classList.remove('d-none');
    } else {
        msg.classList.add('d-none');
    }

    renderPagination(filtered.length);
}

// -------------------------
//  STAR BUTTON HANDLER
// -------------------------
function handleRatingClick(button) {
    const rating = parseInt(button.getAttribute('data-rating'), 10);

    // Toggle logic
    if (selectedRating === rating && rating !== 0) {
        selectedRating = 0; // clicking again resets to all
    } else {
        selectedRating = rating;
    }

    // Update active button
    document.querySelectorAll('.btn-group button[data-rating]').forEach(btn => btn.classList.remove('active'));
    const activeBtn = document.querySelector(`.btn-group button[data-rating="${selectedRating}"]`);
    if (activeBtn) activeBtn.classList.add('active');

    currentPage = 1;
    updateReviewsDisplay();
}

// -------------------------
//  PAGINATION RENDERING
// -------------------------
function renderPagination(totalReviews) {
    const totalPages = Math.ceil(totalReviews / reviewsPerPage);
    const container = document.getElementById('paginationContainer');
    container.innerHTML = '';
    if (totalPages <= 1) return;

    const createPage = (page, label = null, disabled = false, active = false) => {
        const li = document.createElement('li');
        li.className = 'page-item' + (disabled ? ' disabled' : '') + (active ? ' active' : '');
        li.innerHTML = `<a href="#" class="page-link text-decoration-none">${label || page}</a>`;
        if (!disabled && !active) {
            li.addEventListener('click', e => {
                e.preventDefault();
                currentPage = page;
                updateReviewsDisplay();
            });
        }
        return li;
    };

    // Previous
    container.appendChild(createPage(currentPage - 1, '«', currentPage === 1));

    // Pages
    for (let i = 1; i <= totalPages; i++) {
        container.appendChild(createPage(i, null, false, i === currentPage));
    }

    // Next
    container.appendChild(createPage(currentPage + 1, '»', currentPage === totalPages));
}

// -------------------------
//  INITIAL LOAD
// -------------------------
document.addEventListener('DOMContentLoaded', () => {
    updateReviewsDisplay();
});

function updateHeaderStats(filteredReviews) {
    // Calculate average rating
    let sum = 0;
    filteredReviews.forEach(card => {
        sum += parseInt(card.getAttribute('data-rating'), 10);
    });
    const avgRating = filteredReviews.length > 0 ? (sum / filteredReviews.length).toFixed(1) : "0.0";

    // Update DOM
    document.getElementById('averageRating').innerHTML = `${avgRating} <i class="bi bi-star-fill"></i>`;
    document.getElementById('reviewCount').textContent = `(${filteredReviews.length} reviews)`;
}
