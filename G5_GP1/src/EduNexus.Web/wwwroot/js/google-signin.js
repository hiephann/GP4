window.eduNexusGoogleSignIn = (() => {
    let loadingGoogleScript;

    function postCredential(loginUri, credential) {
        const form = document.createElement("form");
        form.method = "post";
        form.action = loginUri;
        const token = document.createElement("input");
        token.type = "hidden";
        token.name = "credential";
        token.value = credential;
        form.appendChild(token);
        document.body.appendChild(form);
        form.submit();
    }

    function loadGoogleScript() {
        if (window.google?.accounts?.id) return Promise.resolve();
        if (loadingGoogleScript) return loadingGoogleScript;

        loadingGoogleScript = new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.src = "https://accounts.google.com/gsi/client";
            script.async = true;
            script.defer = true;
            script.onload = resolve;
            script.onerror = () => reject(new Error("Google Identity Services could not be loaded."));
            document.head.appendChild(script);
        });
        return loadingGoogleScript;
    }

    function setStatus(container, message, isError) {
        const status = container.querySelector("[data-google-status]");
        if (!status) return;
        status.textContent = message;
        status.classList.toggle("text-danger", Boolean(isError));
        status.classList.toggle("text-muted", !isError);
    }

    async function renderOfficialButton(container) {
        if (!container) return;
        const clientId = container.dataset.clientId;
        const loginUri = container.dataset.loginUri;
        if (!clientId || !loginUri) {
            setStatus(container, "Google Client ID chưa được cấu hình.", true);
            return;
        }

        try {
            setStatus(container, "Đang tải Google Sign-In…", false);
            await loadGoogleScript();
            window.google.accounts.id.initialize({
                client_id: clientId,
                callback: response => postCredential(loginUri, response.credential),
                auto_select: false,
                cancel_on_tap_outside: true
            });
            const officialButton = container.querySelector("[data-google-official-button]");
            if (!officialButton) return;
            officialButton.replaceChildren();
            window.google.accounts.id.renderButton(officialButton, {
                type: "standard",
                theme: "outline",
                size: "large",
                text: "continue_with",
                shape: "rectangular",
                logo_alignment: "left",
                width: 420
            });
            container.querySelector("[data-google-fallback]")?.classList.add("d-none");
        } catch {
            setStatus(container, "Không tải được Google Sign-In. Kiểm tra mạng hoặc cấu hình OAuth rồi thử lại.", true);
        }
    }

    function start(container) {
        renderOfficialButton(container);
    }

    function boot() {
        document.querySelectorAll("[data-google-signin]").forEach(renderOfficialButton);
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", boot, { once: true });
    else boot();

    return { start };
})();
