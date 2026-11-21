// The identity form's "fields by company type" (T604, D-1330; 04 9.16; 02 3.1.3). Without this file the form changes which fields are required only after a save (the server draws the form again
// by the type it holds); with it a change of "Firma tipi" marks the type's fields required (or optional) and opens (or locks) them at once. Nothing here decides which fields are required: the
// server writes the sets once from CompanyIdentityFieldSets into the data-type-fields hook ("Type=Input.A,Input.B;Type=..."), and the save checks the same sets, so the script only mirrors the
// server's own drawing.

for (const host of document.querySelectorAll('[data-js="company-type-fields"]')) {
    const choice = host.querySelector('select[name="Input.CompanyType"]');
    const sets = new Map();
    for (const part of (host.dataset.typeFields ?? "").split(";")) {
        const [type, names] = part.split("=");
        if (type) {
            sets.set(type, names ? names.split(",") : []);
        }
    }

    if (!choice || sets.size === 0) {
        continue;
    }

    const requiredText = host.dataset.requiredText ?? "";
    const optionalText = host.dataset.optionalText ?? "";
    const typeFieldNames = [...new Set([...sets.values()].flat())];

    const apply = () => {
        const required = sets.get(choice.value) ?? [];
        const hasType = choice.value !== "";
        for (const name of typeFieldNames) {
            const control = host.querySelector(`[name="${name}"]`);
            if (!control) {
                continue;
            }

            const isRequired = required.includes(name);
            if (isRequired) {
                control.setAttribute("aria-required", "true");
            } else {
                control.removeAttribute("aria-required");
            }

            control.readOnly = !hasType;
            const mark = host.querySelector(`label[for="${control.id}"] .field__requirement`);
            if (mark) {
                mark.textContent = isRequired ? requiredText : optionalText;
            }
        }
    };

    choice.addEventListener("change", apply);
}
