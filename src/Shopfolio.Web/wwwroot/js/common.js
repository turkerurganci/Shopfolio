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
