import { chromium } from "playwright";
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

const DIR = fileURLToPath(new URL("./src/", import.meta.url));
const URL_ = `http://localhost:${process.env.PORT ?? 5401}/`;
const originals = {};
for (const f of ["Counter.fs", "Shared.fs", "Widgets.fs", "Pair.fs", "Rows.fs"])
    originals[f] = readFileSync(DIR + f, "utf8");
const restore = () => {
    for (const f in originals) writeFileSync(DIR + f, originals[f]);
};
const edit = (f, from, to) =>
    writeFileSync(DIR + f, readFileSync(DIR + f, "utf8").replace(from, to));

let failures = 0;
const check = (name, actual, expected) => {
    const ok = JSON.stringify(actual) === JSON.stringify(expected);
    if (!ok) failures++;
    console.log(
        `   ${ok ? "PASS" : "FAIL"}  ${name}${ok ? "" : `  got ${JSON.stringify(actual)} want ${JSON.stringify(expected)}`}`,
    );
};

const browser = await chromium.launch();
const page = await browser.newPage();
let loads = 0;
page.on("load", () => loads++);

const state = () =>
    page.evaluate(() => ({
        label: document.querySelector("#label")?.textContent,
        note: document.querySelector("#note")?.value,
        focus: document.activeElement?.id ?? "",
        caret: document.activeElement?.selectionStart ?? null,
        loadId: window.__loadId,
    }));

try {
    await page.goto(URL_, { waitUntil: "networkidle" });
    const first = await state();

    // establish state the user would hate to lose
    for (let i = 0; i < 3; i++) await page.click("#inc");
    await page.fill("#note", "half typed");
    await page.focus("#note");
    await page.evaluate(() =>
        document.querySelector("#note").setSelectionRange(4, 4),
    );

    console.log("1. edit the component");
    edit("Counter.fs", "VERSION-ONE", "VERSION-TWO");
    await page.waitForFunction(
        () =>
            document
                .querySelector("#label")
                ?.textContent?.includes("VERSION-TWO"),
        { timeout: 60000 },
    );
    let s = await state();
    check("new code applied", s.label.startsWith("VERSION-TWO"), true);
    check("signal state kept", s.label.includes("count=3"), true);
    check("input value kept", s.note, "half typed");
    check("focus kept", s.focus, "note");
    check("caret kept", s.caret, 4);
    check("no page reload", s.loadId === first.loadId && loads === 1, true);
    await page.click("#inc");
    check("still interactive", (await state()).label.includes("count=4"), true);

    // the click above moved focus; put it back where a user would have it
    await page.focus("#note");
    await page.evaluate(() =>
        document.querySelector("#note").setSelectionRange(4, 4),
    );

    console.log("2. change a value in a dependency");
    edit("Shared.fs", '"hello"', '"HOLA"');
    await page.waitForFunction(
        () => document.querySelector("#label")?.textContent?.includes("HOLA"),
        { timeout: 60000 },
    );
    s = await state();
    check("dependency change is not stale", s.label.includes("HOLA"), true);
    check("signal state kept", s.label.includes("count=4"), true);
    check("input value kept", s.note, "half typed");
    check("focus kept", s.focus, "note");
    check("caret kept", s.caret, 4);
    check("no page reload", s.loadId === first.loadId && loads === 1, true);

    console.log("3. a component taking arguments");
    check(
        "args reach the component",
        await page.textContent("#wrap .btn"),
        "v1:press",
    );
    await page.click("#wrap .btn");
    check(
        "handler arg works",
        (await state()).label.includes("count=14"),
        true,
    );
    edit("Widgets.fs", '"v1:"', '"v2:"');
    await page.waitForFunction(
        () =>
            document
                .querySelector("#wrap .btn")
                ?.textContent?.startsWith("v2:"),
        { timeout: 60000 },
    );
    check(
        "parameterised component hot-swaps",
        await page.textContent("#wrap .btn"),
        "v2:press",
    );
    await page.click("#wrap .btn");
    check(
        "handler still wired after swap",
        (await state()).label.includes("count=24"),
        true,
    );

    check(
        "partially applied component renders",
        await page.textContent("#partial .btn"),
        "v2:partial",
    );
    await page.click("#partial .btn");
    check(
        "partially applied handler works",
        (await state()).label.includes("count=124"),
        true,
    );
    edit("Widgets.fs", '"v2:"', '"v3:"');
    await page.waitForFunction(
        () =>
            document
                .querySelector("#partial .btn")
                ?.textContent?.startsWith("v3:"),
        { timeout: 60000 },
    );
    check(
        "partially applied component hot-swaps",
        await page.textContent("#partial .btn"),
        "v3:partial",
    );
    await page.click("#partial .btn");
    check(
        "handler still wired after swap",
        (await state()).label.includes("count=224"),
        true,
    );

    console.log("4. rename a binding inside the component");
    // Rename every occurrence, or the file no longer compiles and nothing arrives.
    writeFileSync(
        DIR + "Counter.fs",
        readFileSync(DIR + "Counter.fs", "utf8").replace(/\bcount\b/g, "tally"),
    );
    await page.waitForFunction(
        () => document.querySelector("#label")?.textContent?.includes("tally="),
        { timeout: 60000 },
    );
    s = await state();
    check(
        "renamed binding resets, not carries",
        s.label.includes("tally=0"),
        true,
    );
    check("unrelated state kept", s.note, "half typed");

    console.log("5. one component, two instances");
    const cells = () =>
        page.evaluate(() => ({
            a: document.querySelector("#out-a")?.textContent,
            b: document.querySelector("#out-b")?.textContent,
            solo: document.querySelector("#out-solo")?.textContent,
        }));
    check("both instances start clean", await cells(), {
        a: "A0",
        b: "A0",
        solo: "S0",
    });
    await page.click("#bump-a");
    await page.click("#bump-a");
    await page.click("#bump-solo");
    check("instances do not share state", await cells(), {
        a: "A2",
        b: "A0",
        solo: "S1",
    });

    edit("Pair.fs", '"A" + string n.Value', '"B" + string n.Value');
    await page.waitForFunction(
        () => document.querySelector("#out-a")?.textContent?.startsWith("B"),
        { timeout: 60000 },
    );
    check("each instance keeps its own value across a swap", await cells(), {
        a: "B2",
        b: "B0",
        solo: "S1",
    });
    await page.click("#bump-b");
    check("both instances still interactive", await cells(), {
        a: "B2",
        b: "B1",
        solo: "S1",
    });

    console.log("6. move a component within the file");
    {
        const src = readFileSync(DIR + "Pair.fs", "utf8");
        const start = src.indexOf("// MOVE-START");
        const end = src.indexOf("// MOVE-END") + "// MOVE-END".length;
        const block = src.slice(start, end);
        const moved =
            (src.slice(0, start) + src.slice(end)).trimEnd() +
            "\n\n" +
            block +
            "\n";
        writeFileSync(DIR + "Pair.fs", moved);
    }
    // The move changes no rendered text, so wait on the rebuild counter instead.
    const before = await page.evaluate(() => globalThis.__rebuilds ?? 0);
    await page.waitForFunction(
        (n) => (globalThis.__rebuilds ?? 0) > n,
        before,
        { timeout: 60000 },
    );
    s = await state();
    check("state survives the component moving", await cells(), {
        a: "B2",
        b: "B1",
        solo: "S1",
    });
    check("no page reload", s.loadId === first.loadId && loads === 1, true);
    await page.click("#bump-solo");
    check("still interactive after the move", (await cells()).solo, "S2");

    console.log("7. a component used as an each row");
    const rows = () =>
        page.evaluate(() => ({
            count: document.querySelectorAll("#rows li").length,
            one: document.querySelector("#row-1")?.textContent,
            two: document.querySelector("#row-2")?.textContent,
        }));
    check("component rows render", await rows(), {
        count: 2,
        one: "Rone:0",
        two: "Rtwo:0",
    });
    await page.click("#row-1");
    await page.click("#row-1");
    await page.click("#row-1");
    check("each row has its own state", await rows(), {
        count: 2,
        one: "Rone:3",
        two: "Rtwo:0",
    });

    edit("Rows.fs", '"R" + item.Label', '"Q" + item.Label');
    await page.waitForFunction(
        () => document.querySelector("#row-1")?.textContent?.startsWith("Q"),
        { timeout: 60000 },
    );
    check("rows hot-swap, keeping per-row state", await rows(), {
        count: 2,
        one: "Qone:3",
        two: "Qtwo:0",
    });
    await page.click("#row-2");
    check(
        "rows still interactive after the swap",
        (await rows()).two,
        "Qtwo:1",
    );

    console.log("8. a component inside a dynamic region");
    const branch = () => page.textContent("#branch");
    check("branch renders", await branch(), "Bleft:0");
    await page.click("#branch");
    await page.click("#branch");
    check("branch keeps its own state", await branch(), "Bleft:2");

    edit("Rows.fs", '"B" + name', '"C" + name');
    await page.waitForFunction(
        () => document.querySelector("#branch")?.textContent?.startsWith("C"),
        { timeout: 60000 },
    );
    check("branch hot-swaps, keeping state", await branch(), "Cleft:2");

    // The swap replaced the node `dynamic` cached; switching branches must not throw.
    await page.click("#flip");
    await page.waitForTimeout(300);
    check("switching after a swap works", await branch(), "Cright:0");
    await page.click("#branch");
    check("the new branch is interactive", await branch(), "Cright:1");
} finally {
    restore();
    await browser.close();
}
console.log(failures === 0 ? "\nALL PASS" : `\n${failures} FAILURE(S)`);
process.exit(failures === 0 ? 0 : 1);
