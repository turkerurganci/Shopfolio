// The delivery provinces' "Tümünü seç" and "Seçimi kaldır" (T539, D-1443; 07 8.2.2, 9.18.3; 04 9.18.4). Without this file the provinces are ticked one by one and the buttons do not exist; with it the
// two buttons are put above the boxes and tick or clear them in place. The send is the same UC-169 form, so nothing here decides which provinces are valid: the server reads the ticked list.

function addButton(host, label, checked) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "button button--secondary";
    button.textContent = label;
    button.addEventListener("click", () => {
        for (const box of host.querySelectorAll('input[type="checkbox"]')) {
            box.checked = checked;
        }
    });
    return button;
}

for (const host of document.querySelectorAll('[data-js="province-boxes"]')) {
    const bar = document.createElement("p");
    bar.className = "province-list__actions";
    bar.append(addButton(host, "Tümünü seç", true), addButton(host, "Seçimi kaldır", false));
    host.before(bar);
}
