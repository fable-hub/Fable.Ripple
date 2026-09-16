// Framework benchmark driver: builds each app, measures its production bundle
// size (gzip) and its render performance (median ms/op over RUNS sweeps), then
// prints a table. Each app is isolated under apps/<name>/ and builds to
// apps/<name>/dist/{index.html,app.js}. See ../README.md for the contract.

import { readFileSync, existsSync } from 'fs';
import { gzipSync } from 'zlib';
import { execSync } from 'child_process';
import { fileURLToPath } from 'url';
import { dirname, resolve } from 'path';
import { chromium, firefox, webkit } from 'playwright';

const here = dirname(fileURLToPath(import.meta.url));
const appsDir = resolve(here, '..', 'apps');

const RUNS = parseInt(process.env.RUNS || '10', 10);
const WARMUP = 2;
const BROWSER = process.env.BENCH_BROWSER || 'chromium';
const ALL = ['manual', 'vanjs', 'solid', 'svelte', 'fable-ripple', 'sutil', 'websharper'];
const wanted = process.argv.slice(2).length ? process.argv.slice(2) : ALL;

// Each scenario: an untimed `setup` and a timed `op`, both expressions over `b`
// (window.__bench), evaluated in-page. The harness waits for a rendered frame
// after the op, so batch-on-rAF frameworks are measured end-to-end.
const scenarios = [
    { name: 'create1k',  setup: 'b.create(0)',    op: 'b.create(1000)' },
    { name: 'create10k', setup: 'b.create(0)',    op: 'b.create(10000)' },
    { name: 'append1k',  setup: 'b.create(1000)', op: 'b.append(1000)' },
    { name: 'update',    setup: 'b.create(1000)', op: 'b.updateEvery10th()' },
    { name: 'swap',      setup: 'b.create(1000)', op: 'b.swap()' },
    { name: 'select',    setup: 'b.create(1000)', op: 'b.select(1)' },
    { name: 'remove',    setup: 'b.create(1000)', op: 'b.remove(1)' },
    { name: 'clear',     setup: 'b.create(1000)', op: 'b.clear()' },
];

function buildApp(name) {
    const dir = resolve(appsDir, name);
    if (!existsSync(dir)) return null;
    try {
        execSync('node build.mjs', { cwd: dir, stdio: 'pipe' });
    } catch (e) {
        console.error(`\n  build failed for ${name}:\n${(e.stdout || '') + (e.stderr || e.message)}`);
        return null;
    }
    const appJs = resolve(dir, 'dist', 'app.js');
    if (!existsSync(appJs)) { console.error(`\n  ${name}: no dist/app.js`); return null; }
    const bytes = readFileSync(appJs);
    return { dir, min: bytes.length, gz: gzipSync(bytes, { level: 9 }).length };
}

const median = a => { const s = [...a].sort((x, y) => x - y); return s[Math.floor(s.length / 2)]; };

async function measure(browserType, dir) {
    const browser = await browserType.launch({
        headless: true,
        args: ['--no-sandbox', '--disable-gpu', '--disable-dev-shm-usage'],
    });
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', e => errors.push(e.message));
    await page.goto('file://' + resolve(dir, 'dist', 'index.html'));
    await page.waitForFunction('window.__bench !== undefined', null, { timeout: 10000 });
    await page.evaluate(() => {
        window.__settle = () => new Promise(r => requestAnimationFrame(() => requestAnimationFrame(r)));
    });

    const result = {};
    for (const sc of scenarios) {
        process.stderr.write(`    ${sc.name}…`);
        const samples = [];
        for (let i = 0; i < WARMUP + RUNS; i++) {
            const ms = await page.evaluate(async ({ setup, op }) => {
                const b = window.__bench;
                new Function('b', setup)(b);
                await window.__settle();
                const s = performance.now();
                new Function('b', op)(b);
                await window.__settle();
                return performance.now() - s;
            }, sc);
            if (i >= WARMUP) samples.push(ms);
        }
        result[sc.name] = median(samples);
    }
    await browser.close();
    if (errors.length) throw new Error(errors[0]);
    return result;
}

const engine = { chromium, firefox, webkit }[BROWSER];
console.log(`\nbench: ${wanted.join(', ')}  (browser=${BROWSER}, RUNS=${RUNS})\n`);

const rows = [];
for (const name of wanted) {
    process.stdout.write(`${name}: building… `);
    const built = buildApp(name);
    if (!built) { console.log('skipped'); continue; }
    process.stdout.write(`${(built.gz / 1024).toFixed(1)} KB gz, measuring… `);
    let perf = {};
    try { perf = await measure(engine, built.dir); console.log('ok'); }
    catch (e) { console.log('perf failed: ' + e.message); }
    rows.push({ name, gz: built.gz, min: built.min, perf });
}

const ops = scenarios.map(s => s.name);
const pad = (s, n) => String(s).padStart(n);
console.log('\n' + pad('app', 15) + pad('gz KB', 8) + pad('min KB', 9) + ops.map(o => pad(o, 10)).join(''));
for (const r of rows) {
    console.log(
        pad(r.name, 15) + pad((r.gz / 1024).toFixed(1), 8) + pad((r.min / 1024).toFixed(1), 9) +
        ops.map(o => pad(r.perf[o] != null ? r.perf[o].toFixed(2) : '-', 10)).join('')
    );
}
console.log();
