# Framework benchmark - isolated production apps

Each implementation lives in its own folder under `apps/`, built exactly the way
it would ship (production toolchain, minified), so the measured **bundle size**
and **render performance** are the real shippable numbers - not artifacts of a
shared project.

## Apps

| app | language | category |
|---|---|---|
| `manual` | vanilla JS | hand-written imperative DOM - the floor |
| `fable-ripple` | F# / Fable | this library (`Var`/`Signal` + `Fable.Ripple.Dom`) |
| `solid` | TSX | compiled fine-grained signals (no VDOM) |
| `svelte` | Svelte 5 | compiler, signals-based |
| `vanjs` | JS | tiny direct-DOM runtime |
| `sutil` | F# / Fable | F# fine-grained direct-DOM |
| `websharper` | F# / WS | F# reactive `Var`/`View` |

## The contract every app implements

Each app builds to `dist/app.js` (ES module, minified, production) and, on load,
mounts a grid into `#main` and assigns `window.__bench`:

```js
window.__bench = {
    create(n),          // replace all rows with n fresh rows
    append(n),          // append n rows to the current list
    updateEvery10th(),  // append " !!!" to every 10th row's label
    swap(),             // swap row index 1 and index length-2
    select(i),          // mark row i selected (class "danger")
    remove(i),          // remove row i
    clear(),            // remove all rows
}
```

Row shape follows js-framework-benchmark (4 cells: id, label link, remove link,
spacer). Labels are deterministic (`"row " + id`) so every app renders identical
work and runs are reproducible. Ops mutate synchronously where the framework
allows; the harness always waits for a rendered frame before stopping the clock,
so batch-on-rAF frameworks (WebSharper) are measured end-to-end and fairly.

## Running

```
node harness/run.mjs                 # build every app, measure size + perf
node harness/run.mjs solid manual    # a subset
RUNS=15 node harness/run.mjs         # more sweeps (default 10)
BROWSER=firefox node harness/run.mjs # chromium (default) | firefox | webkit
```

Each app is buildable on its own too: `cd apps/<x> && node build.mjs` (or its
documented build), producing `dist/{index.html,app.js}`.

## What's measured

- **Bundle**: gzip(`dist/app.js`) - the real transfer size of the app + its
  framework runtime, tree-shaken and minified.
- **Perf**: median ms/op over RUNS sweeps, timed in-page around each op plus a
  double-rAF settle. Absolute ms are machine-dependent; read ratios against
  `manual`.
