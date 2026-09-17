---
title: Performance
---

:::note Not upstream yet
The Fable.Ripple implementation is proposed in [pull request 2099](https://github.com/krausest/js-framework-benchmark/pull/2099) and is not part of the official benchmark yet. The numbers below come from running that implementation locally, so they are not on the published results table.
:::

Results from [js-framework-benchmark](https://github.com/krausest/js-framework-benchmark), the benchmark most JavaScript frameworks publish against. It builds a table of rows and measures the time, the memory and the bundle size of a set of operations on it.

Fable.Ripple 1.0.0-beta.1 (`Fable.Ripple` and `Fable.Ripple.Dom`) is measured against the hand-written DOM implementation, two fine-grained compilers and a virtual DOM. Every entry is the keyed variant.

Run in Chrome on Linux. Each operation is the median of 15 measured runs after warmup, 25 for `select a row`. Memory and size are measured once. Lower is better everywhere.

The benchmark throttles the CPU on the operations that are otherwise too short to time, marked in the table below. Every framework is throttled the same way, so the ratios hold; the milliseconds in those rows are not comparable with the unthrottled ones.

Absolute numbers depend on the machine. The ratio against `vanillajs`, the hand-written implementation, is the figure to compare.

## Summary

Geometric mean of each group's ratios to `vanillajs`. 1.00 is the hand-written implementation.

| Group | vanillajs | Solid 1.9.3 | Svelte 5.42.1 | Fable.Ripple | React 19.2.0 |
| --- | --- | --- | --- | --- | --- |
| Operations | 1.00 | 1.10 | 1.14 | 1.17 | 1.67 |
| Memory | 1.00 | 1.22 | 1.40 | 1.43 | 2.47 |
| Size and startup | 1.00 | 1.24 | 2.22 | 3.07 | 10.55 |

## Operations

Time in milliseconds.

| Benchmark | vanillajs | Solid 1.9.3 | Svelte 5.42.1 | Fable.Ripple | React 19.2.0 |
| --- | --- | --- | --- | --- | --- |
| create 1,000 rows | 42.7 | 44.1 | 44.7 | 51.3 | 50.0 |
| replace all 1,000 rows | 45.5 | 48.9 | 49.0 | 56.4 | 57.7 |
| update every 10th row, 16 cycles (4x CPU) | 18.0 | 19.5 | 19.8 | 18.5 | 23.7 |
| select a row (4x CPU) | 3.7 | 4.7 | 6.8 | 4.9 | 7.4 |
| swap two rows (4x CPU) | 22.0 | 23.9 | 24.5 | 23.8 | 179.6 |
| remove a row (2x CPU) | 19.7 | 19.9 | 20.0 | 19.5 | 21.2 |
| create 10,000 rows | 434.5 | 453.4 | 458.1 | 516.5 | 617.5 |
| append 1,000 rows to 1,000, 2 cycles | 46.1 | 48.4 | 48.9 | 55.0 | 54.9 |
| clear 1,000 rows, 8 cycles (4x CPU) | 15.9 | 20.1 | 18.1 | 20.5 | 28.2 |

Updating every 10th row, swapping and removing are within 10% of the hand-written implementation: each writes the nodes that changed and nothing else. Creating rows is 20% behind, at 1,000 rows and at 10,000.

## Memory

Heap size in MB.

| Benchmark | vanillajs | Solid 1.9.3 | Svelte 5.42.1 | Fable.Ripple | React 19.2.0 |
| --- | --- | --- | --- | --- | --- |
| after load | 0.57 | 0.60 | 0.67 | 0.69 | 1.19 |
| after creating 1,000 rows | 1.87 | 2.69 | 2.87 | 2.97 | 4.42 |
| after creating and clearing 1,000 rows, 5 cycles | 0.65 | 0.77 | 0.98 | 0.98 | 1.97 |

In this implementation a row holds one `Var`{fsharp} for its label and two reactive bindings, which is what the hand-written version does not allocate. The last row is measured after the table has been emptied, so it shows what a framework holds on to.

## Size and startup

Size of the implementation files, excluding CSS and HTTP headers.

| Benchmark | vanillajs | Solid 1.9.3 | Svelte 5.42.1 | Fable.Ripple | React 19.2.0 |
| --- | --- | --- | --- | --- | --- |
| Uncompressed (kB) | 11.3 | 11.5 | 26.6 | 42.8 | 190.3 |
| Brotli (kB) | 2.5 | 4.5 | 9.7 | 12.9 | 51.4 |
| First paint (ms) | 74.7 | 77.7 | 89.5 | 110.3 | 253.6 |

12.9 kB brotli is the application plus `Fable.Ripple`, `Fable.Ripple.Dom` and the part of the Fable JavaScript library it uses. Solid and Svelte compile their templates away; Fable.Ripple ships its DSL as runtime code.
