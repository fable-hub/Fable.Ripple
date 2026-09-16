---
title: Batching and untracked
---

## Batching

Writes flush immediately, so two writes in a row expose the state between them. `Signal.batch`{fsharp} defers the flush to the end of the block - effects run once, seeing only the final state:

```fsharp live
open Fable.Ripple

let first = Var.create "Ada"
let last = Var.create "Lovelace"

// Runs immediately: Ada Lovelace
Signal.effect (fun () -> printfn "%s %s" first.Value last.Value) |> ignore

first.Value <- "Grace" // Grace Lovelace - a torn state
last.Value <- "Hopper" // Grace Hopper

// One flush at the end: Alan Turing
Signal.batch (fun () ->
    first.Value <- "Alan"
    last.Value <- "Turing"
)
```

Batches nest: the flush happens when the outermost one ends.

Every `on.*`{fsharp} handler in [Fable.Ripple.Dom](../ripple-dom/events.md) already batches its writes, so one user gesture is a single flush.

Three sources feed one subscriber. Unbatched, each write is its own flush: three dots, and the counter moves by three. Batched, the same writes reach the subscriber once:

<div data-visual="signals-batch"></div>
<link rel="stylesheet" href="/Fable.Ripple/visuals/visuals.css">
<script type="module" src="/Fable.Ripple/visuals/Main.js"></script>

## Untracked reads

Three forms, one behaviour - read the current value without becoming a dependent:

- `myVar.Peek()`{fsharp} - one read, on the source itself.
- `Signal.peek s`{fsharp} - the same, on a `Var`{fsharp} or a `Signal`{fsharp} alike.
- `Signal.untracked f`{fsharp} - a whole block, for when the reads are buried in a helper you did not write.

```fsharp live
open Fable.Ripple

let query = Var.create "signals"
let page = Var.create 1

Signal.effect (fun () ->
    printfn "searching '%s' (page %d at the time)" query.Value (Signal.peek page)
)
|> ignore

page.Value <- 2 // peeked, not tracked - nothing runs
query.Value <- "adaptive" // searching 'adaptive' (page 2 at the time)
```

The effect reads `page`{fsharp} without depending on it, so writing `page`{fsharp} reruns nothing. Writing `query`{fsharp} reruns the effect, and it sees the current page.
