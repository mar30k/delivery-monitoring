export const UI = {
    showLoading(id) {
        document.getElementById(id)?.classList.remove("d-none");
    },

    hideLoading(id) {
        document.getElementById(id)?.classList.add("d-none");
    },

    showAlert({ message, type = "info", modalId }) {
        const modalHeader = document.querySelector(`#${modalId} .modal-header`);
        if (!modalHeader) return;

        document.querySelector(`#${modalId} .alert`)?.remove();

        modalHeader.insertAdjacentHTML(
            "afterend",
            `
            <div class="alert alert-${type} alert-dismissible fade show">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
            `
        );

        setTimeout(() => {
            document.querySelector(`#${modalId} .alert`)?.remove();
        }, 5000);
    },

    toast(message, color = "green") {
        Toastify({
            text: message,
            style: { background: color },
            duration: 3000,
            gravity: "top",
            position: "right"
        }).showToast();
    }
};
