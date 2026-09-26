---
title: Performance
---

:::note Not upstream yet
The Fable.Ripple implementation is proposed in [pull request 2099](https://github.com/krausest/js-framework-benchmark/pull/2099) and is not part of the official benchmark yet. The numbers below come from running that implementation locally, so they are not on the published results table.
:::

Results from [js-framework-benchmark](https://github.com/krausest/js-framework-benchmark), the benchmark most JavaScript frameworks publish against. It builds a table of rows and measures the time, the memory and the bundle size of a set of operations on it.

Fable.Ripple 1.0.0-beta.5 (`Fable.Ripple` and `Fable.Ripple.Dom`) is measured against the hand-written DOM implementation, two fine-grained compilers and a virtual DOM. Every entry is the keyed variant.

Run in Chrome on Linux. Each operation is the median of 15 measured runs after warmup, 25 for `select a row`. Memory and size are measured once. Lower is better everywhere.

The benchmark throttles the CPU on the operations that are otherwise too short to time, marked in the table below. Every framework is throttled the same way, so the ratios hold; the milliseconds in those rows are not comparable with the unthrottled ones.

Absolute numbers depend on the machine. The ratio against `vanillajs`, the hand-written implementation, is the figure to compare.

## Summary

Geometric mean of each group's ratios to `vanillajs`. 1.00 is the hand-written implementation.

| Group | vanillajs | Solid 1.9.3 | Svelte 5.42.1 | Fable.Ripple | React 19.2.0 |
| --- | --- | --- | --- | --- | --- |
| Operations | 1.00 | 1.08 | 1.14 | 1.14 | 1.67 |
| Memory | 1.00 | 1.21 | 1.40 | 1.43 | 2.45 |
| Size and startup | 1.00 | 1.20 | 2.28 | 3.02 | 10.32 |

## Operations

Time in milliseconds.

| Benchmark | vanillajs | Solid 1.9.3 | Svelte 5.42.1 | Fable.Ripple | React 19.2.0 |
| --- | --- | --- | --- | --- | --- |
| create 1,000 rows | 42.2 | 44.0 | 44.6 | 50.8 | 50.1 |
| replace all 1,000 rows | 45.3 | 48.5 | 48.9 | 54.0 | 57.2 |
| update every 10th row, 16 cycles (4x CPU) | 18.3 | 19.4 | 20.5 | 19.2 | 23.9 |
| select a row (4x CPU) | 3.9 | 4.6 | 7.0 | 5.1 | 7.5 |
| swap two rows (4x CPU) | 22.7 | 24.1 | 24.9 | 24.0 | 179.7 |
| remove a row (2x CPU) | 19.4 | 19.7 | 20.3 | 20.1 | 21.2 |
| create 10,000 rows | 431.1 | 451.7 | 458.0 | 512.1 | 616.2 |
| append 1,000 rows to 1,000, 2 cycles | 46.4 | 48.4 | 48.4 | 55.0 | 56.0 |
| clear 1,000 rows, 8 cycles (4x CPU) | 15.2 | 18.8 | 17.2 | 15.8 | 27.0 |

Updating every 10th row, swapping, removing and clearing are within 10% of the hand-written implementation: each writes the nodes that changed and nothing else. Creating rows is 20% behind, at 1,000 rows and at 10,000.

## Memory

Heap size in MB.

| Benchmark | vanillajs | Solid 1.9.3 | Svelte 5.42.1 | Fable.Ripple | React 19.2.0 |
| --- | --- | --- | --- | --- | --- |
| after load | 0.57 | 0.60 | 0.67 | 0.70 | 1.18 |
| after creating 1,000 rows | 1.88 | 2.68 | 2.88 | 2.92 | 4.43 |
| after creating and clearing 1,000 rows, 5 cycles | 0.65 | 0.78 | 0.98 | 1.00 | 1.97 |

In this implementation a row holds one `Var`{fsharp} for its label and two reactive bindings, which is what the hand-written version does not allocate. The last row is measured after the table has been emptied, so it shows what a framework holds on to.

## Size and startup

Size of the implementation files, excluding CSS and HTTP headers.

| Benchmark | vanillajs | Solid 1.9.3 | Svelte 5.42.1 | Fable.Ripple | React 19.2.0 |
| --- | --- | --- | --- | --- | --- |
| Uncompressed (kB) | 11.3 | 11.5 | 26.6 | 44.5 | 190.3 |
| Brotli (kB) | 2.5 | 4.5 | 9.7 | 13.3 | 51.4 |
| First paint (ms) | 74.0 | 70.5 | 95.4 | 97.3 | 235.0 |

13.3 kB brotli is the application plus `Fable.Ripple`, `Fable.Ripple.Dom` and the part of the Fable JavaScript library it uses. Solid and Svelte compile their templates away; Fable.Ripple ships its DSL as runtime code.
