// The chunked upload script of the product form, run without a browser (T548; 09 16.2 items 6 and 7; 11 T548 KK1-KK9). The module's source is evaluated in a context that holds a fake
// document, a fake fetch that answers from a script of answers, and a timer that records the waits and fires at once. Run by ChunkedUploadScriptTests: `node chunked-upload.test.mjs <module path>`.

import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";
import vm from "node:vm";

const modulePath = process.argv[2];
assert.ok(modulePath, "The path of the page module is the first argument.");
const source = readFileSync(modulePath, "utf8");

const START = (id = "u1", chunk = 4) => ({ status: 201, body: { id, receivedBytes: 0, chunkSizeBytes: chunk, isCompleted: false } });
const PART = (received, completed = false, id = "u1") => ({ status: 200, body: { id, receivedBytes: received, isCompleted: completed } });
const CONFLICT = (received, completed = false) => ({ status: 409, body: { id: "u1", receivedBytes: received, isCompleted: completed } });
const STATUS = (received, completed = false) => ({ status: 200, body: { id: "u1", receivedBytes: received, isCompleted: completed } });
const EMPTY = (status) => ({ status, body: null });
const NETWORK = "network";
const REDIRECT = "redirect";

function settle() {
    return new Promise((resolve) => setImmediate(resolve));
}

function makeButton() {
    const attributes = new Map();
    return {
        setAttribute: (name, value) => attributes.set(name, value),
        removeAttribute: (name) => attributes.delete(name),
        getAttribute: (name) => attributes.get(name) ?? null,
    };
}

// answers: an array consumed in order; an item is a response description, NETWORK or REDIRECT; a function item is called with the request and may return a promise (a held answer).
function makeEnvironment(answers) {
    const handlers = {};
    const calls = [];
    const delays = [];
    const state = { dataset: {}, textContent: "" };
    const target = { value: "old" };
    const saveButton = makeButton();
    const form = {
        dataset: {},
        elements: {
            namedItem: (name) => (name === "__RequestVerificationToken" ? { value: "F" } : name === "Input.DigitalFileId" ? target : null),
        },
        querySelectorAll: () => [saveButton],
    };

    const document = {
        addEventListener: (type, handler) => {
            handlers[type] = handler;
        },
        querySelectorAll: () => [],
        getElementById: () => null,
        createElement: () => ({ dataset: {}, textContent: "" }),
    };

    let next = 0;
    const fetch = async (url, init) => {
        assert.ok(url.startsWith("/panel/yukleme"), `Only the upload endpoints are called: ${url}`);
        assert.ok(init.method === "POST" || init.method === "GET", `Only POST and GET are sent: ${init.method}`);
        const call = { url, method: init.method, headers: Object.fromEntries(init.headers ?? []), redirect: init.redirect, body: init.body };
        calls.push(call);
        assert.ok(next < answers.length, `A request nobody answers: ${init.method} ${url}`);
        let answer = answers[next];
        next += 1;
        if (typeof answer === "function") {
            answer = await answer(call);
        }

        if (answer === NETWORK) {
            throw new TypeError("network down");
        }

        if (answer === REDIRECT) {
            return { type: "opaqueredirect", status: 0, json: async () => null };
        }

        return { type: "basic", status: answer.status, json: async () => answer.body };
    };

    const setTimeout = (callback, milliseconds) => {
        delays.push(milliseconds);
        Promise.resolve().then(callback);
        return 0;
    };

    const context = vm.createContext({ document, fetch, Headers, setTimeout });
    vm.runInContext(source, context, { filename: modulePath });

    function makeInput(file, hook = "digital-file-upload") {
        return { dataset: { js: hook, targetField: "Input.DigitalFileId" }, files: file ? [file] : [], form, parentElement: { querySelector: () => state }, after: () => {} };
    }

    function makeFile(size, name = "book.pdf") {
        const slices = [];
        return { name, size, slices, slice: (from, to) => { slices.push([from, to]); return { from, to }; } };
    }

    return { handlers, calls, delays, state, target, form, saveButton, makeInput, makeFile, remaining: () => answers.length - next };
}

async function choose(environment, input) {
    environment.handlers.change({ target: input });
    await settle();
}

test("KK1: the change of an input with the hook starts the upload; another element does not", async () => {
    const environment = makeEnvironment([START("u1", 4), PART(3, true)]);
    await choose(environment, environment.makeInput(environment.makeFile(3), "row-add"));
    assert.equal(environment.calls.length, 0);
    await choose(environment, environment.makeInput(null));
    assert.equal(environment.calls.length, 0);
    await choose(environment, environment.makeInput(environment.makeFile(3)));
    assert.equal(environment.calls.length, 2);
    assert.equal(environment.target.value, "u1");
});

test("KK1: an input that was not on the page at the start is bound the same way (a row the script added)", async () => {
    const environment = makeEnvironment([START("u9", 4), PART(2, true, "u9")]);
    const added = environment.makeInput(environment.makeFile(2));
    await choose(environment, added);
    assert.equal(environment.target.value, "u9");
});

test("KK2: the size is not compared at the choice, and the form can not be saved while the upload runs", async () => {
    let release;
    const gate = new Promise((resolve) => { release = resolve; });
    const environment = makeEnvironment([async () => { await gate; return EMPTY(500); }]);
    const huge = environment.makeFile(5_000_000_000_000);
    environment.handlers.change({ target: environment.makeInput(huge) });
    await settle();
    assert.equal(environment.calls.length, 1, "The start request goes out whatever the size.");
    assert.equal(environment.form.dataset.uploads, "1");
    assert.equal(environment.saveButton.getAttribute("aria-disabled"), "true");
    let prevented = false;
    environment.handlers.submit({ target: environment.form, preventDefault: () => { prevented = true; } });
    assert.equal(prevented, true);
    release();
    await settle();
    assert.equal(environment.form.dataset.uploads, "0");
    assert.equal(environment.saveButton.getAttribute("aria-disabled"), null);
    prevented = false;
    environment.handlers.submit({ target: environment.form, preventDefault: () => { prevented = true; } });
    assert.equal(prevented, false);
});

test("KK3, KK4, KK5: start body and header, the part size from the answer, Content-Range of the first, middle and last part, the id into the field and the name and size shown", async () => {
    const environment = makeEnvironment([START("u1", 4), PART(4), PART(8), PART(10, true)]);
    const file = environment.makeFile(10);
    await choose(environment, environment.makeInput(file));
    const [start, first, middle, last] = environment.calls;
    assert.equal(start.method, "POST");
    assert.equal(start.url, "/panel/yukleme");
    assert.equal(start.headers["content-type"], "application/json");
    assert.equal(start.headers.requestverificationtoken, "F");
    assert.deepEqual(JSON.parse(start.body), { fileName: "book.pdf", sizeBytes: 10 });
    assert.equal(start.redirect, "manual");
    for (const part of [first, middle, last]) {
        assert.equal(part.method, "POST");
        assert.equal(part.url, "/panel/yukleme/u1");
        assert.equal(part.headers["content-type"], "application/octet-stream");
        assert.equal(part.headers.requestverificationtoken, "F");
        assert.equal(part.redirect, "manual");
    }

    assert.equal(first.headers["content-range"], "bytes 0-3/10");
    assert.equal(middle.headers["content-range"], "bytes 4-7/10");
    assert.equal(last.headers["content-range"], "bytes 8-9/10");
    assert.deepEqual(file.slices, [[0, 4], [4, 8], [8, 10]]);
    assert.equal(environment.target.value, "u1");
    assert.equal(environment.state.dataset.state, "done");
    assert.ok(environment.state.textContent.includes("book.pdf") && environment.state.textContent.includes("10 B"), environment.state.textContent);
});

test("KK4: a single part and an exact multiple of the part size", async () => {
    const single = makeEnvironment([START("u1", 4), PART(3, true)]);
    await choose(single, single.makeInput(single.makeFile(3)));
    assert.equal(single.calls[1].headers["content-range"], "bytes 0-2/3");

    const multiple = makeEnvironment([START("u1", 4), PART(4), PART(8, true)]);
    await choose(multiple, multiple.makeInput(multiple.makeFile(8)));
    assert.deepEqual(multiple.calls.slice(1).map((call) => call.headers["content-range"]), ["bytes 0-3/8", "bytes 4-7/8"]);
});

test("KK5: the next part starts at receivedBytes", async () => {
    const environment = makeEnvironment([START("u1", 4), PART(6), PART(10, true)]);
    await choose(environment, environment.makeInput(environment.makeFile(10)));
    assert.equal(environment.calls[2].headers["content-range"], "bytes 6-9/10");
});

test("KK6: a 409 resumes from the receivedBytes of its body; a completed one ends the upload", async () => {
    const resumed = makeEnvironment([START("u1", 4), PART(4), CONFLICT(8), PART(10, true)]);
    await choose(resumed, resumed.makeInput(resumed.makeFile(10)));
    assert.equal(resumed.calls[2].headers["content-range"], "bytes 4-7/10");
    assert.equal(resumed.calls[3].headers["content-range"], "bytes 8-9/10");
    assert.equal(resumed.target.value, "u1");

    const completed = makeEnvironment([START("u1", 4), CONFLICT(10, true)]);
    await choose(completed, completed.makeInput(completed.makeFile(10)));
    assert.equal(completed.calls.length, 2);
    assert.equal(completed.target.value, "u1");
    assert.equal(completed.state.dataset.state, "done");
});

test("KK7: a 404 starts again with the same file and a new id; a second 404 stops", async () => {
    const again = makeEnvironment([START("u1", 4), PART(4), EMPTY(404), START("u2", 4), PART(4, false, "u2"), PART(10, true, "u2")]);
    const file = again.makeFile(10);
    await choose(again, again.makeInput(file));
    assert.equal(again.calls[3].method, "POST");
    assert.equal(again.calls[3].url, "/panel/yukleme");
    assert.deepEqual(JSON.parse(again.calls[3].body), { fileName: "book.pdf", sizeBytes: 10 });
    assert.equal(again.calls[4].url, "/panel/yukleme/u2");
    assert.equal(again.calls[4].headers["content-range"], "bytes 0-3/10");
    assert.equal(again.target.value, "u2");

    const twice = makeEnvironment([START("u1", 4), EMPTY(404), START("u2", 4), EMPTY(404)]);
    await choose(twice, twice.makeInput(twice.makeFile(10)));
    assert.equal(twice.calls.length, 4);
    assert.equal(twice.state.dataset.state, "error");
    assert.equal(twice.target.value, "old");
});

test("KK7: 400, 413 and 500 stop at once, show the general error and are not tried again", async () => {
    for (const status of [400, 413, 500]) {
        const environment = makeEnvironment([START("u1", 4), EMPTY(status)]);
        await choose(environment, environment.makeInput(environment.makeFile(10)));
        assert.equal(environment.calls.length, 2, `status ${status}`);
        assert.equal(environment.state.dataset.state, "error");
        assert.ok(environment.state.textContent.includes("yüklenemedi"));
        assert.equal(environment.target.value, "old");
        assert.equal(environment.delays.length, 0);
    }
});

test("KK8: a 422 at the start says the size when the file is above maxSizeBytes and the disk threshold when it is not", async () => {
    const large = makeEnvironment([{ status: 422, body: { maxSizeBytes: 100 } }]);
    await choose(large, large.makeInput(large.makeFile(150)));
    assert.equal(large.state.dataset.state, "error");
    assert.ok(large.state.textContent.includes("çok büyük"), large.state.textContent);
    assert.equal(large.target.value, "old");

    const disk = makeEnvironment([{ status: 422, body: { maxSizeBytes: 100 } }]);
    await choose(disk, disk.makeInput(disk.makeFile(50)));
    assert.equal(disk.state.dataset.state, "error");
    assert.ok(disk.state.textContent.includes("depolama alanı"), disk.state.textContent);
    assert.ok(!disk.state.textContent.includes("çok büyük"));
});

test("KK9: a redirect stops the upload and asks for a new sign-in", async () => {
    const inPart = makeEnvironment([START("u1", 4), REDIRECT]);
    await choose(inPart, inPart.makeInput(inPart.makeFile(10)));
    assert.equal(inPart.calls.length, 2);
    assert.equal(inPart.state.dataset.state, "error");
    assert.ok(inPart.state.textContent.includes("Oturumunuz"));

    const atStart = makeEnvironment([REDIRECT]);
    await choose(atStart, atStart.makeInput(atStart.makeFile(10)));
    assert.equal(atStart.calls.length, 1);
    assert.ok(atStart.state.textContent.includes("Oturumunuz"));
});

test("KK9: a network error resumes from the status request, at most three times after 1, 2 and 4 seconds, and then stops", async () => {
    const down = makeEnvironment([START("u1", 4), NETWORK, NETWORK, NETWORK, NETWORK]);
    await choose(down, down.makeInput(down.makeFile(10)));
    assert.deepEqual(down.delays, [1000, 2000, 4000]);
    assert.deepEqual(down.calls.slice(2).map((call) => call.method), ["GET", "GET", "GET"]);
    assert.equal(down.calls.length, 5);
    assert.equal(down.state.dataset.state, "error");
    assert.equal(down.target.value, "old");

    const recovered = makeEnvironment([START("u1", 4), PART(4), NETWORK, STATUS(8), PART(10, true)]);
    await choose(recovered, recovered.makeInput(recovered.makeFile(10)));
    assert.deepEqual(recovered.calls.map((call) => call.method), ["POST", "POST", "POST", "GET", "POST"]);
    assert.equal(recovered.calls[3].url, "/panel/yukleme/u1");
    assert.equal(recovered.calls[4].headers["content-range"], "bytes 8-9/10");
    assert.equal(recovered.target.value, "u1");

    const flapping = makeEnvironment([START("u1", 4), NETWORK, STATUS(0), NETWORK, NETWORK, NETWORK]);
    await choose(flapping, flapping.makeInput(flapping.makeFile(10)));
    assert.deepEqual(flapping.delays, [1000, 2000, 4000]);
    assert.equal(flapping.state.dataset.state, "error");
});

test("KK9: when the answer to the last part is lost the same part is sent again", async () => {
    const environment = makeEnvironment([START("u1", 4), PART(4), NETWORK, STATUS(8), PART(8, true)]);
    await choose(environment, environment.makeInput(environment.makeFile(8)));
    assert.equal(environment.calls[4].headers["content-range"], "bytes 4-7/8");
    assert.equal(environment.target.value, "u1");
});

test("KK10: choosing another file or leaving calls no other endpoint and the first upload is dropped", async () => {
    let release;
    const gate = new Promise((resolve) => { release = resolve; });
    const environment = makeEnvironment([async () => { await gate; return START("u1", 4); }, START("u2", 4), PART(3, true, "u2")]);
    const input = environment.makeInput(environment.makeFile(3));
    environment.handlers.change({ target: input });
    await settle();
    input.files = [environment.makeFile(3, "other.pdf")];
    environment.handlers.change({ target: input });
    await settle();
    release();
    await settle();
    assert.equal(environment.target.value, "u2");
    assert.ok(environment.state.textContent.includes("other.pdf"));
    assert.equal(environment.form.dataset.uploads, "0");
    assert.equal(environment.remaining(), 0);
});
