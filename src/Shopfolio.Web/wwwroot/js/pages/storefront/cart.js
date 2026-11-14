// The cart's progressive enhancement (T539; 04 5.12.8; 07 8.2.2). Without this file the quantity form's own "Güncelle" button sends UC-20 and the page opens again with the amount the server
// counted. With it, a change of the quantity sends that same form with its own button (09 16.2 item 1: requestSubmit with the submitter - the same handler, no other endpoint, no part of the
// page is replaced); the page that answers is whole, so nothing is kept here.

for (const form of document.querySelectorAll('form[data-js="submit-once"] input[name="Quantity"]')) {
    const owner = form.form;
    if (!owner) {
        continue;
    }

    form.addEventListener("change", () => {
        if (owner.getAttribute("aria-busy") !== "true" && form.checkValidity()) {
            owner.requestSubmit(owner.querySelector('button[type="submit"]'));
        }
    });
}
