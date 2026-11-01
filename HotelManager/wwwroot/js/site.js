document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-toggle-password]").forEach(button => {
        const input = document.getElementById(
            button.getAttribute("aria-controls")
        );

        if (!(input instanceof HTMLInputElement)) return;

        button.hidden = false;

        button.addEventListener("click", () => {
            const visible = input.type === "password";

            input.type = visible ? "text" : "password";

            button.setAttribute("aria-pressed", String(visible));

            button.setAttribute(
                "aria-label",
                visible ? "Ascunde parola" : "Arată parola"
            );

            const label = button.querySelector(
                "[data-password-label]"
            );

            if (label) {
                label.textContent = visible ? "Ascunde" : "Arată";
            }
        });
    });

    document.querySelectorAll("form[data-submit-form]").forEach(form => {
        form.addEventListener("submit", event => {
            if (event.defaultPrevented || !form.checkValidity()) return;

            const jq = window.jQuery;

            if (jq?.validator && !jq(form).valid()) return;

            const button = form.querySelector(
                "[data-submit-button]"
            );

            if (!(button instanceof HTMLButtonElement)) return;

            button.classList.add("is-submitting");
            button.disabled = true;
        });
    });

    const resetButtons = () => {
        document.querySelectorAll(
            "[data-submit-button].is-submitting"
        ).forEach(button => {
            button.classList.remove("is-submitting");
            button.disabled = false;
        });
    };

    window.addEventListener("pageshow", resetButtons);

    document.querySelectorAll("[data-dismiss-notice]").forEach(button => {
        button.addEventListener("click", () => {
            button.closest("[data-notice]")?.remove();
        });
    });

    document.querySelectorAll("img[data-room-image]").forEach(img => {
        const hideBrokenImage = () => {
            img.hidden = true;
        };

        img.addEventListener("error", hideBrokenImage);

        if (img.complete && img.naturalWidth === 0) {
            hideBrokenImage();
        }
    });
});