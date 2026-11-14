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

// The selection's choices drawn again for the shown variant, with the server's rule (VariantSelection.Fields; 04 5.4.6, 5.4.9): each value of one option is read as the variant it would give while the
// other option keeps its place; a value whose combination does not exist cannot be chosen, a sold-out one carries "Tükendi" and cannot be chosen, the value now shown is always the chosen one.
function refreshChoices() {
    const selects = selectionForm ? Array.from(selectionForm.querySelectorAll("[data-js='option']")) : [];
    const places = selects.map((select) => Number.parseInt(select.value, 10));
    selects.forEach((select, dimension) => {
        const placeOf = (variant, d) => (d === 0 ? variant.first : variant.second) ?? null;
        for (const option of select.options) {
            option.dataset.label ??= option.textContent.replace(/ \(Tükendi\)$/, "");
            const value = Number.parseInt(option.value, 10);
            const candidate = variants.find((v) => placeOf(v, dimension) === value && (selects.length < 2 || placeOf(v, 1 - dimension) === places[1 - dimension]));
            const soldOut = candidate !== undefined && candidate.soldOut === true;
            option.disabled = !(value === places[dimension] || (candidate !== undefined && !soldOut));
            option.textContent = soldOut ? `${option.dataset.label} (Tükendi)` : option.dataset.label;
        }
    });
}

function show(variant) {
    refreshChoices();
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
            selectionForm.requestSubmit(apply ?? undefined);
        }
    });
}

if (cartForm) {
    // Locked after the first send (09 16.2 item 3): the form carries aria-busy and a second submit is stopped; the button is not disabled, so its own value still enters the form. A page that comes back
    // from the back/forward cache is made ready again; the redirect after the add draws the page with a fresh form.
    cartForm.addEventListener("submit", (event) => {
        if (cartForm.getAttribute("aria-busy") === "true") {
            event.preventDefault();
            return;
        }

        cartForm.setAttribute("aria-busy", "true");
    });
    window.addEventListener("pageshow", (event) => {
        if (event.persisted) {
            cartForm.removeAttribute("aria-busy");
        }
    });
}
