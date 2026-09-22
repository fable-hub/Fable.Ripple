// How much DOM work does "always swap" cost when a shared dependency is edited?
// Requires vite + fable watch running.  PORT=5404 node measure.mjs
import { chromium } from "playwright";
import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

const DIR = fileURLToPath(new URL("./src/", import.meta.url));
const PORT = process.env.PORT ?? 5404;
const original = readFileSync(DIR + "Shared.fs", "utf8");
const browser = await chromium.launch();

const results = [];
try {
    for (const n of [0, 100, 1000, 5000]) {
        const page = await browser.newPage();
        await page.goto(`http://localhost:${PORT}/?n=${n}`, {
            waitUntil: "networkidle",
        });
        const nodes = await page.evaluate(
            () => document.querySelectorAll("#root *").length,
        );
        await page.evaluate(() => {
            globalThis.__rebuildMs = [];
            globalThis.__rebuilds = 0;
        });

        const word = "W" + Math.random().toString(36).slice(2, 7);
        writeFileSync(
            DIR + "Shared.fs",
            original.replace('"hello"', `"${word}"`),
        );
        await page.waitForFunction(
            (w) => document.body.innerHTML.includes(w),
            word,
            { timeout: 90000 },
        );
        await page.waitForTimeout(400);

        const ms = await page.evaluate(() => globalThis.__rebuildMs ?? []);
        const rebuilds = await page.evaluate(() => globalThis.__rebuilds);
        const total = ms.reduce((a, b) => a + b, 0);
        results.push({
            n,
            nodes,
            rebuilds,
            totalMs: +total.toFixed(1),
            slowestMs: +Math.max(0, ...ms).toFixed(1),
        });
        writeFileSync(DIR + "Shared.fs", original);
        await page.waitForTimeout(3000);
        await page.close();
    }
} finally {
    writeFileSync(DIR + "Shared.fs", original);
    await browser.close();
}

console.log(
    "\nEdit one shared dependency; every downstream component rebuilds.\n",
);
console.log(
    "  rows  DOM nodes  boundaries rebuilt  total ms  slowest boundary",
);
for (const r of results)
    console.log(
        `  ${String(r.n).padStart(4)}  ${String(r.nodes).padStart(9)}  ${String(r.rebuilds).padStart(18)}  ${String(r.totalMs).padStart(8)}  ${String(r.slowestMs).padStart(16)}`,
    );
