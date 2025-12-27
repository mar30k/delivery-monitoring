async function openCompletePendingOrderModal(el) {

    if (!el) return;

    const voucherCode = el.dataset.voucher;
    const duration = el.dataset.duration;
    const distance = el.dataset.distance;
    const eta = el.dataset.eta;
    const driverPhone = el.dataset.driverPhone;

    const modalEl = document.getElementById('completePendingOrderModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

    // Set voucher info
    document.getElementById('completeOrderVoucher').textContent =
        voucherCode ? `- ${voucherCode}` : '';
    document.getElementById('completeVoucherCode').value = voucherCode || '';

    // Pre-fill numeric inputs
    document.getElementById('duration').value = duration ?? '';
    document.getElementById('distance').value = distance ?? '';
    document.getElementById('eta').value = eta ?? '';

    // Prepare driver select UI (loading state)
    prepareDriverSelectLoading();

    // Disable submit until drivers are loaded
    toggleSubmit(false);

    // Show modal immediately ✅
    modal.show();

    // Load drivers asynchronously (non-blocking UX)
    loadDrivers(driverPhone)
        .then(() => toggleSubmit(true))
        .catch(() => toggleSubmit(false));
}

function prepareDriverSelectLoading() {
    const select = document.getElementById('driverPhone');
    if (!select) return;

    select.disabled = true;
    select.innerHTML = `
        <option value="">
            Loading drivers...
        </option>
    `;
}

async function loadDrivers(selectedPhone) {

    const select = document.getElementById('driverPhone');
    if (!select) return;

    try {
        const response = await fetch('/driver/getdrivers');
        if (!response.ok) {
            throw new Error(`Driver fetch failed: ${response.status}`);
        }

        const drivers = await response.json();

        select.innerHTML = '';

        if (!Array.isArray(drivers) || drivers.length === 0) {
            select.innerHTML = `<option value="">No drivers available</option>`;
            return;
        }

        select.appendChild(new Option('Select driver', ''));

        drivers.forEach(driver => {
            const option = new Option(
                `${driver.firstName} (${driver.phoneNumber})`,
                driver.phoneNumber,
                driver.phoneNumber === selectedPhone,
                driver.phoneNumber === selectedPhone
            );
            select.add(option);
        });

        select.disabled = false;

    } catch (error) {
        console.error('Error loading drivers:', error);
        select.innerHTML = `<option value="">Failed to load drivers</option>`;
    }
}
function toggleSubmit(enabled) {
    const btn = document.getElementById('completeOrderSubmitBtn');
    if (!btn) return;

    btn.disabled = !enabled;
    btn.textContent = enabled ? 'Complete Order' : 'Loading...';
}


document
    .getElementById('completePendingOrderForm')
    ?.addEventListener('submit', async function (e) {

        e.preventDefault();

        const modalEl = document.getElementById('completePendingOrderModal');
        const submitBtn = document.getElementById('completeOrderSubmitBtn');

        // Disable button and show feedback
        submitBtn.disabled = true;
        submitBtn.textContent = 'Submitting...';

        // Build payload
        const payload = {
            voucherCode: document.getElementById('completeVoucherCode').value,
            driverPhoneNumber: document.getElementById('driverPhone').value || null,
            distance: parseFloat(document.getElementById('distance').value) || null,
            duration: parseFloat(document.getElementById('duration').value) || null,
            eta: parseFloat(document.getElementById('eta').value) || null
        };

        try {
            const response = await fetch('/completePendingorder', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            const responseText = await response.text();

            if (!response.ok) {
                showToast(`Failed to complete order: ${responseText}`, "error");
                return;
            }

            if (responseText === "true") {
                bootstrap.Modal.getInstance(modalEl)?.hide();
                showToast('Order completed successfully.', "success");
                location.reload();
            } else {
                showToast(`Unexpected response: ${responseText}`, "error");
            }

        } catch (error) {
            showToast(`Failed to complete order: ${error.message}`, "error");
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Complete Order';
        }
    });


