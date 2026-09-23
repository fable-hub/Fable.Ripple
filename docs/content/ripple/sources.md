---
title: Sources
---

A `Var<'T>`{fsharp} is a value you read and write through `.Value`{fsharp}:

```fsharp live
open Fable.Ripple

let count = Var.create 0

count.Value <- count.Value + 1
printfn "%d" count.Value // 1
```

Reading `.Value`{fsharp} inside a computed or an effect registers a dependency. Writing it notifies everything that depends on it.

## Equal writes are no-ops

A write is compared to the current value with structural equality. Writing an equal value notifies nobody:

```fsharp live
open Fable.Ripple

let name = Var.create "ada"

// Runs immediately: name = ada
Signal.effect (fun () -> printfn "name = %s" name.Value) |> ignore

name.Value <- "ada" // equal - nothing runs
name.Value <- "grace" // name = grace
```

Only `grace` reprints. The first write changed nothing, so nothing ran.

Watch it on the graph - writing the same value again moves nothing, writing a new one sends a dot and bumps the effect's run counter:

<div data-visual="signals-equal-write"></div>
<link rel="stylesheet" href="/Fable.Ripple/visuals/visuals.css">
<script type="module" src="/Fable.Ripple/visuals/Main.js"></script>

## Custom equality

`Var.createWith`{fsharp} takes the comparison as a parameter:

```fsharp live
open Fable.Ripple

let name =
    Var.createWith
        (fun a b -> System.String.Equals(a, b, System.StringComparison.OrdinalIgnoreCase))
        "ada"

// Runs immediately: name = ada
Signal.effect (fun () -> printfn "name = %s" name.Value) |> ignore

name.Value <- "ADA" // equal under this comparison - nothing runs
name.Value <- "grace" // name = grace
```

`ADA` is equal under this comparison, so it does not reprint - and does not overwrite.

## The read-only view

`Signal<'T>`{fsharp} is the read-only interface of a `Var<'T>`{fsharp}: a getter and `Peek`{fsharp}, no setter. Every `Var<'T>`{fsharp} is a `Signal<'T>`{fsharp}, so a source is accepted wherever a `Signal<'T>`{fsharp} is expected. Take a `Signal<'T>`{fsharp} in anything that should read but never write:

```fsharp
let theme = Var.create "light"

let watch (s: Signal<string>) =
    // s.Value <- "dark"    would not compile
    Signal.subscribe (printfn "theme = %s") s

watch theme
```

The view costs nothing: a `Signal<'T>`{fsharp} is the node itself, seen through its interface. `myVar.Signal`{fsharp} gives the same value with the `Signal<'T>`{fsharp} type, for a list or a record field that holds sources and derived signals together.

A computed is only ever handed out as a `Signal<'T>`{fsharp}. Writing one through a downcast to `Var<'T>`{fsharp} throws `InvalidOperationException`{fsharp}.

## Sharing state

State lives where the `let`{fsharp} lives. A `Var`{fsharp} bound at module level belongs to the module, and every file that opens it reads the same value:

```fsharp
module Theme =
    let private state = Var.create "light"

    /// Read-only for consumers; the module owns the writes.
    let current: Signal<string> = state

    let set (name: string) = state.Value <- name
```

Module-level state never resets. That suits a theme, and not a counter that should start at zero each time its component appears. Keep per-visit state inside the component function.

## Reading without subscribing

`myVar.Peek()`{fsharp} reads the current value without registering a dependency. See [Batching and untracked](batching.md#untracked-reads).
