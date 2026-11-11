// The product page's progressive enhancement (T503; 04 5.4.9, 5.4.17; 07 4.4.1, 8.2.2). The page works without this file: the selection is a GET form with the two parameters and the
// add-to-cart form carries the variant the server chose. With the file, a change of an option is answered from the page's own variant data (data-variants: the places of the two values, the price
// lines, the image and whether the variant can be bought - never a stock count), the form's hidden variant field is filled from it, and the add-to-cart button is locked after the first send so a
// double click is one add. A combination the data does not hold is answered by the server: the form is sent as it is.

const root = document.querySelector("[data-variants]");
const variants = parseVariants(root);
const selectionForm = document.querySelector("[data-js='variant-form']");
const cartForm = document.querySelector("[data-js='add-to-cart']");

function parseVariants(element) {
    if (!element || !element.dataset.variants) {
        return [];
    }

    try {
        const parsed = JSON.parse(element.dataset.variants);
        return Array.isArray(parsed) ? parsed : [];
    } catch {
        return [];
    }
}

function setText(name, text) {
    const element = document.querySelector(`[data-js='${name}']`);
    if (!element) {
        return;
    }

    element.textContent = text ?? "";
    element.hidden = !text;
}

function selectedPlaces() {
    const options = selectionForm ? selectionForm.querySelectorAll("[data-js='option']") : [];
    const places = Array.from(options, (select) => Number.parseInt(select.value, 10));
    return { first: places.length > 0 ? places[0] : null, second: places.length > 1 ? places[1] : null };
}

function findVariant() {
    const { first, second } = selectedPlaces();
    return variants.find((v) => (v.first ?? null) === first && (v.second ?? null) === second);
}

function show(variant) {
    setText("price", variant.price);
    setText("reference", variant.reference);
    setText("mark", variant.mark);
    setText("dates", variant.dates);
    setText("unit-price", variant.unitPrice);
    const image = document.querySelector("[data-js='lead-image']");
    if (image && variant.imagePath) {
        image.src = variant.imagePath;
        image.alt = variant.imageAlt ?? image.alt;
    }

    const field = cartForm ? cartForm.querySelector("[data-js='variant-id']") : null;
    if (field) {
        field.value = variant.id;
    }

    const button = cartForm ? cartForm.querySelector("[data-js='add-button']") : null;
    if (button) {
        button.disabled = !variant.purchasable;
    }
}

if (selectionForm && variants.length > 0) {
    const apply = selectionForm.querySelector("[data-js='apply-selection']");
    if (apply) {
        apply.hidden = true;
    }

    selectionForm.addEventListener("change", () => {
        const variant = findVariant();
        if (variant && variant.purchasable) {
            show(variant);
        } else {
            selectionForm.requestSubmit();
        }
    });
}

if (cartForm) {
    cartForm.addEventListener("submit", (event) => {
        const button = cartForm.querySelector("[data-js='add-button']");
        if (!button) {
            return;
        }

        if (button.disabled) {
            event.preventDefault();
            return;
        }

        // Locked after the first send; the redirect after the add draws the page again with a fresh button.
        window.setTimeout(() => {
            button.disabled = true;
        }, 0);
    });
}
