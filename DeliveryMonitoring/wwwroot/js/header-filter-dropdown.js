function initHeaderFilterDropdown(tableObj, columnIndex, columnName) {
    var column = tableObj.api().column(columnIndex);
    var header = js(column.header());

    // Store original header content
    var originalContent = header.html();

    // Create filter dropdown HTML
    var dropdownId = columnName.toLowerCase() + 'FilterDropdown';
    var filterHtml = `
        <div class="header-filter-wrapper" 
             style="position: relative; display: flex; align-items: center; justify-content: center">
            <span class="header-text">${originalContent}</span>
            <button class="btn btn-sm btn-link p-0 ms-1 filter-toggle" 
                    type="button" 
                    style="border: none; background: none;">
                <i class="bi bi-funnel"></i>
            </button>
            <div class="filter-dropdown" 
                 style="display: none; position: absolute; top: 100%; right: 0; 
                        background: white; border: 1px solid #ddd; border-radius: 4px; 
                        box-shadow: 0 2px 8px rgba(0,0,0,0.1); z-index: 1000; 
                        min-width: 200px; padding: 10px; margin-top: 5px;">
                <div class="filter-header mb-2">
                    <strong>Filter ${columnName}</strong>
                    <button type="button" class="btn-close float-end" style="font-size: 0.7rem;"></button>
                </div>
                <div class="filter-options" style="max-height: 200px; overflow-y: auto;">
                    <input type="text" class="form-control form-control-sm filter-search mb-1"
                           placeholder="Search..." style="font-size: 12px;">
                    <div class="form-check mb-1">
                        <input class="form-check-input select-all" type="checkbox" id="selectAll_${dropdownId}" checked>
                        <label class="form-check-label" for="selectAll_${dropdownId}" style="font-size: 12px;">
                            Select All
                        </label>
                    </div>
                    <div class="filter-items"></div>
                </div>
                <div class="filter-actions mt-2">
                    <button class="btn btn-primary btn-sm apply-filter" style="font-size: 11px;">Apply</button>
                    <button class="btn btn-secondary btn-sm clear-filter ms-1" style="font-size: 11px;">Clear</button>
                </div>
            </div>
        </div>
    `;
    header.html(filterHtml);
    header.find('.filter-toggle, .filter-dropdown, .filter-dropdown *').on('click', function (e) {
        e.stopPropagation();
    });
    var filterToggle = header.find('.filter-toggle');
    var filterDropdown = header.find('.filter-dropdown');
    var filterToggleIcon = header.find('.filter-toggle i');
    var closeButton = header.find('.btn-close');
    var selectAll = header.find('.select-all');
    var filterItems = header.find('.filter-items');
    var applyButton = header.find('.apply-filter');
    var clearButton = header.find('.clear-filter');

    // Populate filter options
    function populateFilterOptions() {
        var columnData = column.data().toArray();
        var valueCounts = {};

        columnData.forEach(function (value) {
            value = (value || '').toString().trim() || 'N/A';
            valueCounts[value] = (valueCounts[value] || 0) + 1;
        });

        filterItems.empty();

        Object.keys(valueCounts).sort().forEach(function (value) {
            var count = valueCounts[value];
            var itemId = dropdownId + '_' + value.replace(/[^a-zA-Z0-9]/g, '_');

            var itemHtml = `
            <div class="form-check mb-1">
                <input class="form-check-input filter-item" type="checkbox"
                           value="${value}" id="${itemId}" checked>
                <label class="form-check-label" for="${itemId}" style="font-size: 12px;">
                    ${value} <span class="text-muted">(${count})</span>
                </label>
            </div>
        `;
            filterItems.append(itemHtml);
        });
    }

    // Toggle dropdown
    filterToggle.on('click', function (e) {
        e.stopPropagation();
        var isVisible = filterDropdown.is(':visible');

        // Close all other dropdowns
        $('.filter-dropdown').hide();

        if (!isVisible) {
            populateFilterOptions();
            filterDropdown.show();
        }
    });

    // Close dropdown
    closeButton.on('click', function (e) {
        e.stopPropagation();
        filterDropdown.hide();
    });

    // Select All functionality
    selectAll.on('change', function () {
        var isChecked = $(this).prop('checked');
        filterItems.find('.filter-item').prop('checked', isChecked);
    });

    // Apply filter
    applyButton.on('click', function (e) {
        e.stopPropagation();
        applyColumnFilter(column);
        filterDropdown.hide();
    });

    // Clear filter
    clearButton.on('click', function (e) {
        e.stopPropagation();
        filterItems.find('.filter-item').prop('checked', true);
        selectAll.prop('checked', true);
        column.search('').draw();
        filterDropdown.hide();
        filterToggleIcon.removeClass('bi-funnel-fill').addClass('bi-funnel');
        selectAll.prop('checked', true)
    });

    // Individual item change
    filterItems.on('change', '.filter-item', function () {
        var totalItems = filterItems.find('.filter-item').length;
        var checkedItems = filterItems.find('.filter-item:checked').length;

        if (checkedItems === totalItems) {
            selectAll.prop('checked', true);
            selectAll.prop('indeterminate', false);
        } else if (checkedItems === 0) {
            selectAll.prop('checked', false);
            selectAll.prop('indeterminate', false);
        } else {
            selectAll.prop('checked', false);
            selectAll.prop('indeterminate', true);
        }
    });

    // Close dropdown when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.header-filter-wrapper').length) {
            filterDropdown.hide();
        }
    });
}

// Apply column filter function
function applyColumnFilter(column) {
    var header = $(column.header());
    var filterToggle = header.find('.filter-toggle i');
    var selectedValues = [];

    header.find('.filter-item:checked').each(function () {
        selectedValues.push($(this).val());
    });

    if (selectedValues.length === 0) {
        column.search('').draw();
        filterToggle.removeClass('bi-funnel-fill').addClass('bi-funnel');
    } else {
        var regexPattern = selectedValues.map(function (val) {
            return '^' + val.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + '$';
        }).join('|');

        column.search(regexPattern, true, false).draw();
        filterToggle.removeClass('bi-funnel').addClass('bi-funnel-fill');
    }
}
// When typing in the search box
$(document).on('input', '.filter-search', function () {
    var searchVal = $(this).val().toLowerCase();
    var $items = $(this).siblings('.filter-items').find('.form-check');

    $items.each(function () {
        var text = $(this).text().toLowerCase();
        $(this).toggle(text.includes(searchVal));
    });
});
