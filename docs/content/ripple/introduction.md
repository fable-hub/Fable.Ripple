---
title: Introduction
---

Fable.Ripple gives you reactive values. Write a source and everything derived from it updates - synchronously, and nothing else recomputes.

## Installation

```bash frame="terminal"
dotnet add package Fable.Ripple --prerelease
```

This library knows nothing about rendering. DOM rendering comes as a separate package, [Fable.Ripple.Dom](../ripple-dom/introduction.md).

## The three ideas

A **source** is a `Var<'T>`{fsharp} you read and write. A **derived signal** is a `Signal<'T>`{fsharp} computed from other signals, cached, and read-only. An **effect** runs now and re-runs when a signal it reads changes:

```fsharp live
open Fable.Ripple

let price = Var.create 10.0
let quantity = Var.create 2

let total = Signal.computed (fun () -> price.Value * float quantity.Value)

// Runs immediately: total = 20.00
Signal.effect (fun () -> printfn "total = %.2f" total.Value) |> ignore

// Each write reruns the effect
quantity.Value <- 3 // total = 30.00
price.Value <- 8.0 // total = 24.00
```

## Visualization

The diagrams on these pages are live dependency graphs; the legend beneath each one names the shapes. This one holds two sources feeding one computed, read by one binding.

Press a button. The node that recomputes flashes, and a dot travels along each edge that carried the change. The run counter moves only when the computed actually ran.

<div data-visual="signals-intro"></div>
<link rel="stylesheet" href="/Fable.Ripple/visuals/visuals.css">
<script type="module" src="/Fable.Ripple/visuals/Main.js"></script>

## Where to go

- [Sources](sources.md) - creating and writing a `Var`{fsharp}, and the read-only view.
- [Derived signals](derived.md) - `map`{fsharp}, `computed`{fsharp}, dynamic dependencies, the `signal`{fsharp} builder.
- [Effects and scopes](effects.md) - reacting to changes, and tearing down cleanly.
- [Batching and untracked](batching.md) - one flush for many writes, reads that do not subscribe.
- [Equality and cutoff](cutoff.md) - why equal values stop propagation.
