// The product form's row buttons (T539; 07 8.2.2, 9.5; 09 16.2 item 4; UC-114). Without this file the GET gives one blank option, one blank value under each option and one blank variant row, and a row
// left blank is not read. With it, "Seçenek ekle", "Değer ekle" and "Varyant ekle" (hidden in the markup, shown here) copy a row from the list's <template data-js="row-template">. The server
// writes the template: the field names are the page's own, the row's number is the placeholder __index__, and this file only replaces that symbol with the number of rows now on the form, so the rows
// stay numbered one after the other (the framework's indexed binding stops at the first gap). Nothing is deleted and nothing is renumbered: a blank row is not read.

const PLACEHOLDER = "__index__";

function replaceIn(root, number) {
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT);
    for (let node = walker.nextNode(); node !== null; node = walker.nextNode()) {
        if (node.nodeType === Node.TEXT_NODE) {
            node.nodeValue = node.nodeValue.replaceAll(PLACEHOLDER, number);
            continue;
        }

        for (const attribute of Array.from(node.attributes)) {
            if (attribute.value.includes(PLACEHOLDER)) {
                node.setAttribute(attribute.name, attribute.value.replaceAll(PLACEHOLDER, number));
            }
        }
    }
}

function bindAdd(button) {
    const template = document.getElementById(button.dataset.template);
    const list = document.getElementById(button.dataset.list);
    if (!(template instanceof HTMLTemplateElement) || !list) {
        return;
    }

    const max = Number.parseInt(button.dataset.max ?? "", 10);
    const rows = () => list.querySelectorAll(":scope > [data-js='row']").length;
    button.hidden = Number.isInteger(max) && rows() >= max;
    button.addEventListener("click", () => {
        if (Number.isInteger(max) && rows() >= max) {
            return;
        }

        const copy = template.content.cloneNode(true);
        replaceIn(copy, String(rows()));
        if (list.contains(button)) {
            button.before(copy);
        } else {
            list.append(copy);
        }

        if (Number.isInteger(max) && rows() >= max) {
            button.hidden = true;
        }
    });
}

for (const button of document.querySelectorAll('[data-js="row-add"]')) {
    bindAdd(button);
}
