// Shared progressive-enhancement behavior loaded on every page (09 section 16.1).
// Plain JavaScript module, no build step; pages must work with JavaScript disabled.

// The one "Share" button of a product and a content page (02 3.30.7; T363). The button is hidden in the markup and shown here, so a browser without
// JavaScript shows no button that does nothing. A phone opens the device's own share menu; a browser without one copies the page's link and says so next
// to the button. No platform's share code is loaded and no address of another site is written here: the link is this page's own path.
function pageLink() {
    return window.location.origin + window.location.pathname;
}

async function copyLink(link) {
    if (!navigator.clipboard || typeof navigator.clipboard.writeText !== "function") {
        return false;
    }

    try {
        await navigator.clipboard.writeText(link);
        return true;
    } catch {
        return false;
    }
}

async function share(button) {
    const message = button.parentElement.querySelector("[data-share-result]");
    const link = pageLink();
    if (typeof navigator.share === "function") {
        try {
            await navigator.share({ title: document.title, url: link });
        } catch {
            // The visitor closed the menu: nothing to say.
        }

        return;
    }

    const copied = await copyLink(link);
    if (message) {
        message.textContent = copied ? message.dataset.copied : message.dataset.failed;
    }
}

for (const button of document.querySelectorAll("button[data-share]")) {
    button.hidden = false;
    button.addEventListener("click", () => share(button));
}

// The fold-out menu of the narrow class, in both frames (04 2.2.4, 2.2.8; T540). The toggle button is hidden in the markup and shown here, and the panel it controls starts folded;
// without JavaScript the toggle never shows and the whole menu stays open, so nothing is lost. The medium and wide classes never fold the panel: the style sheet shows it whatever
// state it is in, and the toggle is not drawn there. State lives in aria-expanded and data-collapsed (09 16.1 item 4); Escape folds the menu and gives the focus back to the toggle.
function bindMenuToggle(toggle) {
    const panel = document.getElementById(toggle.getAttribute("aria-controls"));
    if (!panel) {
        return;
    }

    const setOpen = (open) => {
        toggle.setAttribute("aria-expanded", open ? "true" : "false");
        panel.dataset.collapsed = open ? "false" : "true";
    };

    toggle.hidden = false;
    setOpen(false);
    toggle.addEventListener("click", () => setOpen(toggle.getAttribute("aria-expanded") !== "true"));
    panel.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && toggle.getAttribute("aria-expanded") === "true") {
            setOpen(false);
            toggle.focus();
        }
    });
}

for (const toggle of document.querySelectorAll('[data-js="menu-toggle"]')) {
    bindMenuToggle(toggle);
}

// The storefront menu's category tree under "Ürünler" (04 2.1.4 item 3, 2.2.4; T540). The title is a plain text in the markup and a hidden button beside it; once this script has run the button
// replaces the title and the tree folds on every class: click, Enter or Space opens and closes it, Escape closes it and gives the focus back to the button. Without the script the tree stays open.
function bindMenuTreeGroup(group) {
    const button = group.querySelector('[data-js="menu-tree-toggle"]');
    const panel = button ? document.getElementById(button.getAttribute("aria-controls")) : null;
    if (!button || !panel) {
        return;
    }

    const setOpen = (open) => {
        button.setAttribute("aria-expanded", open ? "true" : "false");
        group.dataset.collapsed = open ? "false" : "true";
    };

    button.hidden = false;
    group.dataset.jsReady = "true";
    setOpen(false);
    button.addEventListener("click", () => setOpen(button.getAttribute("aria-expanded") !== "true"));
    group.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && button.getAttribute("aria-expanded") === "true") {
            setOpen(false);
            button.focus();
        }
    });
}

for (const group of document.querySelectorAll('[data-js="menu-tree-group"]')) {
    bindMenuTreeGroup(group);
}

// The panel menu's groups fold on the narrow class and the active group arrives open (04 2.2.8). Each group's title is also a button that is shown only when this script has run; the
// wide and medium classes keep every group open in the style sheet.
function bindMenuGroup(group) {
    const button = group.querySelector('[data-js="group-toggle"]');
    if (!button) {
        return;
    }

    const setOpen = (open) => {
        button.setAttribute("aria-expanded", open ? "true" : "false");
        group.dataset.collapsed = open ? "false" : "true";
    };

    group.dataset.jsReady = "true";
    setOpen(group.dataset.active === "true");
    button.addEventListener("click", () => setOpen(button.getAttribute("aria-expanded") !== "true"));
}

for (const group of document.querySelectorAll('[data-js="menu-group"]')) {
    bindMenuGroup(group);
}

// A resend form is sent once (04 2.5.4, 2.7.2; 09 16.2 item 3): the first send marks the form busy and the button says it is sending; a second send is stopped. The button is not
// disabled, so the value of the pressed button still enters the form. A page restored from the back/forward cache is made usable again.
function bindResendOnce(form) {
    const button = form.querySelector('button[type="submit"]');
    const original = button ? button.textContent : "";
    form.addEventListener("submit", (event) => {
        if (form.getAttribute("aria-busy") === "true") {
            event.preventDefault();
            return;
        }
        form.setAttribute("aria-busy", "true");
        if (button && button.dataset.sendingLabel) {
            button.textContent = button.dataset.sendingLabel;
        }
    });
    window.addEventListener("pageshow", (event) => {
        if (event.persisted) {
            form.removeAttribute("aria-busy");
            if (button) {
                button.textContent = original;
            }
        }
    });
}

for (const form of document.querySelectorAll('[data-js="resend-once"]')) {
    bindResendOnce(form);
}

// The sign-in merge message can be closed (07 4.34.3; 04 2.7): the close button is hidden in the markup and shown here.
for (const button of document.querySelectorAll('[data-js="merge-notice-close"]')) {
    button.hidden = false;
    button.addEventListener("click", () => {
        const notice = button.closest('[data-js="merge-notice"]');
        if (notice) {
            notice.hidden = true;
        }
    });
}

// A message told once is closable (04 2.3.1.3): the close button is hidden in the markup and shown here, so a browser without JavaScript shows no button that does nothing.
for (const button of document.querySelectorAll('[data-js="message-close"]')) {
    button.hidden = false;
    button.addEventListener("click", () => {
        const message = button.closest('[data-js="message"]');
        if (message) {
            message.hidden = true;
        }
    });
}

// The focus goes to the first wrong field of a submitted form (04 2.3.2, T541). The page marks that field with autofocus already; this covers a browser that ignores it on a page load,
// and falls back to the summary itself when the field is not on the page.
for (const summary of document.querySelectorAll('[data-js="error-summary"]')) {
    // The first wrong field is the first one on the page, whatever order the list came in.
    const fields = Array.from(summary.querySelectorAll('a[href^="#"]'), (link) => document.getElementById(link.getAttribute("href").slice(1))).filter(Boolean);
    fields.sort((a, b) => (a.compareDocumentPosition(b) & Node.DOCUMENT_POSITION_FOLLOWING ? -1 : 1));
    const firstFieldId = summary.dataset.firstField;
    const field = fields[0] ?? (firstFieldId ? document.getElementById(firstFieldId) : null);
    (field ?? summary).focus();
}

// A form that carries submit-once can not be sent twice and says that the operation continues (04 2.4.6, 2.7.2; 09 16.2 item 3). The button is not disabled: a disabled button would drop the handler
// value from the form. State lives in aria-busy; a page that comes back from the back/forward cache is made ready again.
function bindSubmitOnce(form) {
    const labelOf = (button) => button.dataset.originalLabel ?? button.textContent;
    form.addEventListener("submit", (event) => {
        if (form.getAttribute("aria-busy") === "true") {
            event.preventDefault();
            return;
        }

        form.setAttribute("aria-busy", "true");
        const submitter = event.submitter ?? form.querySelector('button[type="submit"]');
        if (submitter && submitter.dataset.busyLabel) {
            submitter.dataset.originalLabel = labelOf(submitter);
            submitter.textContent = submitter.dataset.busyLabel;
        }
    });
    window.addEventListener("pageshow", (event) => {
        if (!event.persisted) {
            return;
        }

        form.removeAttribute("aria-busy");
        for (const button of form.querySelectorAll("button[data-original-label]")) {
            button.textContent = button.dataset.originalLabel;
        }
    });
}

// A form whose answer is a file (the panel's exports, 04 9.24.7; T524): the screen does not change, so the form is sent in the background, the button says the file is being prepared and can not be
// pressed twice, and the file is handed to the browser's download when it comes; the button is ready again after it. Any other answer (a field message, an empty set, an error) is the page itself:
// the form is then sent the plain way, which draws the page the server answers - those answers write nothing, so the second sending changes nothing.
function bindDownloadOnce(form) {
    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        if (form.getAttribute("aria-busy") === "true") {
            return;
        }

        const button = event.submitter ?? form.querySelector('button[type="submit"]');
        const label = button ? button.textContent : "";
        form.setAttribute("aria-busy", "true");
        if (button && button.dataset.busyLabel) {
            button.textContent = button.dataset.busyLabel;
        }

        try {
            const response = await fetch(form.action, { method: "POST", body: new FormData(form), credentials: "same-origin" });
            const disposition = response.headers.get("Content-Disposition") ?? "";
            if (!response.ok || !disposition.startsWith("attachment")) {
                form.submit();
                return;
            }

            const name = /filename=("?)([^";]+)\1/.exec(disposition)?.[2] ?? "dosya.csv";
            const address = URL.createObjectURL(await response.blob());
            const link = document.createElement("a");
            link.href = address;
            link.download = name;
            document.body.append(link);
            link.click();
            link.remove();
            URL.revokeObjectURL(address);
        } catch {
            form.submit();
            return;
        } finally {
            form.removeAttribute("aria-busy");
            if (button) {
                button.textContent = label;
            }
        }
    });
}

// A reading form whose choice applies at once (the sales summary's period, 04 9.22.8): a change of the closed list sends the form, unless the choice is the custom range, whose two dates are sent by
// the form's button. Without a script the button sends the form.
for (const form of document.querySelectorAll('[data-js="submit-on-change"]')) {
    const choice = form.querySelector("select");
    choice?.addEventListener("change", () => {
        if (choice.value !== "Custom") {
            form.submit();
        }
    });
}

for (const form of document.querySelectorAll('[data-js="download-once"]')) {
    bindDownloadOnce(form);
}

for (const form of document.querySelectorAll('[data-js="submit-once"]')) {
    bindSubmitOnce(form);
}

// The administrator's confirmation window (04 2.4.4, T541): drawn open by the server, it is opened as a modal window here; the focus is on "Vazgeç"; Escape and a click outside the window are "Vazgeç".
function bindConfirmWindow(dialog) {
    const cancel = dialog.querySelector('[data-js="confirm-cancel"]');
    if (!cancel || typeof dialog.showModal !== "function") {
        return;
    }

    dialog.close();
    dialog.showModal();
    cancel.focus();
    const goBack = () => {
        window.location.assign(cancel.href);
    };
    dialog.addEventListener("cancel", (event) => {
        event.preventDefault();
        goBack();
    });
    dialog.addEventListener("click", (event) => {
        if (event.target === dialog) {
            goBack();
        }
    });
}

for (const dialog of document.querySelectorAll('[data-js="confirm-window"]')) {
    bindConfirmWindow(dialog);
}
