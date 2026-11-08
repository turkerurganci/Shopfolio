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
