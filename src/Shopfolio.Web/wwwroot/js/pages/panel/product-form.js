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
        const nested = Array.from(copy.querySelectorAll('[data-js="row-add"]'));
        if (list.contains(button)) {
            button.before(copy);
        } else {
            list.append(copy);
        }

        // A row the script adds may carry its own add button (a new dimension's "Değer ekle"); it is bound once the row is in the page.
        for (const added of nested) {
            bindAdd(added);
        }

        if (Number.isInteger(max) && rows() >= max) {
            button.hidden = true;
        }
    });
}

for (const button of document.querySelectorAll('[data-js="row-add"]')) {
    bindAdd(button);
}

// ---- The chunked upload of the digital file (T548; 09 16.2 item 6; 07 4.31.2, 5.4.2; 04 2.12.8; UC-83) ----
//
// A file input that carries data-js="digital-file-upload" sends its file to the upload endpoints in three kinds of request: start (POST /panel/yukleme, the name and the size), part (POST
// /panel/yukleme/{id}, the bytes with Content-Range) and status (GET /panel/yukleme/{id}). The part size comes from the start answer, and the size limits are the server's: this file
// holds no number of either. When the last part is received the upload's id is written to the hidden field named in data-target-field and the page shows the file's name and size; the form is
// saved afterwards by its own button. The listener sits on the document, so an input of a row this script added later is bound as the inputs of the first page are. Leaving the page or
// choosing another file calls no endpoint: an upload left unfinished is deleted by the nightly cleanup (07 4.31.2).

const UPLOAD_PATH = "/panel/yukleme";
const FORGERY_HEADER = "RequestVerificationToken";
const FORGERY_FIELD = "__RequestVerificationToken";
const RETRY_DELAYS_MS = [1000, 2000, 4000];
const SIZE_UNITS = ["B", "KB", "MB", "GB"];
const BYTES_PER_UNIT = 1024;
const MAX_STALLED_ANSWERS = 2;

const GENERIC_ERROR_TEXT = "Dosya yüklenemedi. Sayfayı yenileyip yeniden deneyin.";
const SESSION_ENDED_TEXT = "Oturumunuz sona erdi. Sayfayı yenileyip yeniden giriş yapın.";
const DISK_TEXT = "Sunucudaki depolama alanı dolmak üzere; dosya şimdilik yüklenemiyor.";

const uploadGenerations = new WeakMap();

function wait(milliseconds) {
    return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

function formatSize(bytes) {
    let value = bytes;
    let unit = 0;
    while (value >= BYTES_PER_UNIT && unit < SIZE_UNITS.length - 1) {
        value /= BYTES_PER_UNIT;
        unit += 1;
    }

    const text = unit === 0 ? String(value) : value.toFixed(1).replace(".", ",");
    return `${text} ${SIZE_UNITS[unit]}`;
}

// One request. A redirect is caught, not followed (the session has ended); a network error is told apart from an answer.
async function send(url, init) {
    let response;
    try {
        response = await fetch(url, { ...init, redirect: "manual" });
    } catch {
        return { kind: "network" };
    }

    if (response.type === "opaqueredirect") {
        return { kind: "redirect" };
    }

    return { kind: "response", response };
}

async function readStatus(response) {
    try {
        const body = await response.json();
        if (Number.isFinite(body.receivedBytes) && body.receivedBytes >= 0) {
            return body;
        }
    } catch {
        return null;
    }

    return null;
}

const FAILED = { outcome: "failed" };

async function startSession(file, forgeryValue) {
    const headers = new Headers();
    headers.set("Content-Type", "application/json");
    headers.set(FORGERY_HEADER, forgeryValue);
    const result = await send(UPLOAD_PATH, { method: "POST", headers, body: JSON.stringify({ fileName: file.name, sizeBytes: file.size }) });
    if (result.kind === "redirect") {
        return { outcome: "redirect" };
    }

    if (result.kind !== "response") {
        return FAILED;
    }

    const { response } = result;
    if (response.status === 422) {
        try {
            const body = await response.json();
            return Number.isFinite(body.maxSizeBytes) ? { outcome: "rejected", maxSizeBytes: body.maxSizeBytes } : FAILED;
        } catch {
            return FAILED;
        }
    }

    if (response.status !== 201) {
        return FAILED;
    }

    try {
        const body = await response.json();
        return typeof body.id === "string" && Number.isFinite(body.chunkSizeBytes) && body.chunkSizeBytes > 0 ? { outcome: "ok", id: body.id, chunkSize: body.chunkSizeBytes } : FAILED;
    } catch {
        return FAILED;
    }
}

// The whole upload of one file: the answer is { outcome: "done", id } or one of "rejected", "redirect", "failed" and "cancelled". The status request is asked only after a network error; the
// resumes it allows are three (waits of 1, 2 and 4 seconds), counted again after a part is received.
async function runUpload(file, forgeryValue, isCurrent, progress) {
    const started = await startSession(file, forgeryValue);
    if (started.outcome !== "ok") {
        return started;
    }

    let { id, chunkSize } = started;
    let received = 0;
    let restarted = false;
    let needStatus = false;
    let failures = 0;
    let stalls = 0;

    // The upload is gone from the server (404): it starts again with the same file once; a second 404 is a failure.
    const startAgain = async () => {
        if (restarted) {
            return FAILED;
        }

        restarted = true;
        const again = await startSession(file, forgeryValue);
        if (again.outcome !== "ok") {
            return again;
        }

        ({ id, chunkSize } = again);
        received = 0;
        needStatus = false;
        failures = 0;
        stalls = 0;
        progress(received);
        return null;
    };

    const advance = (next) => {
        stalls = next === received ? stalls + 1 : 0;
        received = next;
        progress(received);
        return stalls >= MAX_STALLED_ANSWERS || received > file.size;
    };

    while (true) {
        if (!isCurrent()) {
            return { outcome: "cancelled" };
        }

        if (needStatus) {
            if (failures >= RETRY_DELAYS_MS.length) {
                return FAILED;
            }

            await wait(RETRY_DELAYS_MS[failures]);
            failures += 1;
            const status = await send(`${UPLOAD_PATH}/${id}`, { method: "GET" });
            if (status.kind === "redirect") {
                return { outcome: "redirect" };
            }

            if (status.kind === "response" && status.response.status === 404) {
                const stopped = await startAgain();
                if (stopped !== null) {
                    return stopped;
                }

                continue;
            }

            if (status.kind === "response" && status.response.status === 200) {
                const body = await readStatus(status.response);
                if (body === null) {
                    return FAILED;
                }

                if (body.isCompleted === true) {
                    return { outcome: "done", id };
                }

                received = body.receivedBytes;
                progress(received);
                needStatus = false;
            }

            continue;
        }

        // The part after what the server holds; when the server holds the whole file but has not marked it complete (the answer to the last part was lost), the last part is sent again and the
        // server completes the upload without adding a byte (K-1203).
        const lastStart = Math.floor((file.size - 1) / chunkSize) * chunkSize;
        const from = received >= file.size ? lastStart : received;
        const to = Math.min(from + chunkSize, file.size);
        const headers = new Headers();
        headers.set("Content-Type", "application/octet-stream");
        headers.set("Content-Range", `bytes ${from}-${to - 1}/${file.size}`);
        headers.set(FORGERY_HEADER, forgeryValue);
        const sent = await send(`${UPLOAD_PATH}/${id}`, { method: "POST", headers, body: file.slice(from, to) });
        if (sent.kind === "redirect") {
            return { outcome: "redirect" };
        }

        if (sent.kind === "network") {
            needStatus = true;
            continue;
        }

        const code = sent.response.status;
        if (code === 200 || code === 409) {
            const body = await readStatus(sent.response);
            if (body === null) {
                return FAILED;
            }

            if (body.isCompleted === true) {
                return { outcome: "done", id };
            }

            if (code === 200) {
                failures = 0;
            }

            if (advance(body.receivedBytes)) {
                return FAILED;
            }

            continue;
        }

        if (code === 404) {
            const stopped = await startAgain();
            if (stopped !== null) {
                return stopped;
            }

            continue;
        }

        // 400, 413, 500 and any answer not listed: a fault of the script or of the server, the upload stops and is not tried again.
        return FAILED;
    }
}

function stateElementOf(input) {
    const found = input.parentElement?.querySelector('[data-js="file-state"]');
    if (found) {
        return found;
    }

    const created = document.createElement("p");
    created.dataset.js = "file-state";
    input.after(created);
    return created;
}

function showState(element, state, text) {
    element.dataset.state = state;
    element.textContent = text;
}

// While an upload runs the form can not be saved (04 2.12.8.4): the buttons get aria-disabled (they are not disabled, 09 16.2 item 3) and the submit is stopped. The number of running
// uploads of the form is state in data-uploads.
function holdForm(form, delta) {
    const count = Math.max(0, Number.parseInt(form.dataset.uploads ?? "0", 10) + delta);
    form.dataset.uploads = String(count);
    for (const button of form.querySelectorAll('button[type="submit"]')) {
        if (count > 0) {
            button.setAttribute("aria-disabled", "true");
        } else {
            button.removeAttribute("aria-disabled");
        }
    }
}

async function onFileChosen(input) {
    const generation = (uploadGenerations.get(input) ?? 0) + 1;
    uploadGenerations.set(input, generation);
    const file = input.files?.[0];
    const form = input.form;
    if (!file || !form) {
        return;
    }

    const state = stateElementOf(input);
    const target = form.elements.namedItem(input.dataset.targetField ?? "");
    const forgery = form.elements.namedItem(FORGERY_FIELD);
    const isCurrent = () => uploadGenerations.get(input) === generation;
    if (!target || !forgery) {
        showState(state, "error", GENERIC_ERROR_TEXT);
        return;
    }

    holdForm(form, 1);
    showState(state, "busy", `${file.name} yükleniyor: %0`);
    const percent = (bytes) => Math.min(100, Math.floor((bytes / file.size) * 100));
    try {
        const result = await runUpload(file, forgery.value, isCurrent, (bytes) => {
            if (isCurrent()) {
                showState(state, "busy", `${file.name} yükleniyor: %${percent(bytes)}`);
            }
        });

        if (!isCurrent()) {
            return;
        }

        if (result.outcome === "done") {
            target.value = result.id;
            showState(state, "done", `Yüklendi: ${file.name} (${formatSize(file.size)})`);
        } else if (result.outcome === "rejected") {
            // The file's size above the limit the server names is the reason; otherwise it is the disk threshold (04 2.12.8.3).
            const text = file.size > result.maxSizeBytes ? `Dosya çok büyük; en çok ${formatSize(result.maxSizeBytes)} yüklenebilir.` : DISK_TEXT;
            showState(state, "error", text);
        } else if (result.outcome === "redirect") {
            showState(state, "error", SESSION_ENDED_TEXT);
        } else {
            showState(state, "error", GENERIC_ERROR_TEXT);
        }
    } finally {
        holdForm(form, -1);
    }
}

document.addEventListener("change", (event) => {
    const input = event.target;
    if (input?.dataset?.js === "digital-file-upload") {
        void onFileChosen(input);
    }
});

document.addEventListener("submit", (event) => {
    const form = event.target;
    if (Number.parseInt(form?.dataset?.uploads ?? "0", 10) > 0) {
        event.preventDefault();
    }
});