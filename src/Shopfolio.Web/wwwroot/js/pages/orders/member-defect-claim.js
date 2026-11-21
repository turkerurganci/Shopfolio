// E-19's one defective-units field opens in the row of the chosen item (T604, D-1412; 04 2.12.1.8; 07 5 SubmitDefectClaimInput carries one Quantity). Without this file the field stays under the list
// while several rows carry one and none is chosen (the server draws it in the row once an item is sent); with it a click on an item's choice moves the same field into that row, and back under the list
// when the chosen row has no field. Nothing here decides the limit: the row's own limit line is the server's, and the send is the same form, so the server still checks the item and the units.

for (const root of document.querySelectorAll('[data-js="defect-claim-items"]')) {
    const holder = root.querySelector('[data-js="defect-claim-quantity"]');
    const slot = root.querySelector('[data-js="defect-claim-slot"]');
    if (!holder || !slot) {
        continue;
    }

    const place = (row) => {
        if (row && row.dataset.quantityField === "true") {
            row.querySelector(".operation-screen__choice")?.after(holder);
        } else {
            slot.append(holder);
        }
    };

    for (const choice of root.querySelectorAll('input[type="radio"][name="OrderItemId"]')) {
        choice.addEventListener("change", () => {
            if (choice.checked) {
                place(choice.closest(".operation-screen__item"));
            }
        });
    }
}
