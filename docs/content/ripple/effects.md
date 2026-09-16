---
title: Effects and scopes
---

## Effects

`Signal.effect`{fsharp} runs its function now, and again whenever a signal it read changes. It returns an `IDisposable`{fsharp}; dispose it to stop:

```fsharp live
open Fable.Ripple

let count = Var.create 0

// Runs immediately: count = 0
let stop = Signal.effect (fun () -> printfn "count = %d" count.Value)

count.Value <- 1 // count = 1
stop.Dispose()
count.Value <- 2 // disposed - nothing runs
```

The write to `2` prints nothing - the effect is gone.

Effects run synchronously: the write returns after every affected effect has run. To coalesce several writes into one run, see [Batching](batching.md).

## Subscribe

`Signal.subscribe`{fsharp} is the same, with the current value passed to the handler:

```fsharp live
open Fable.Ripple

let count = Var.create 0

// Runs immediately: count = 0
count |> Signal.subscribe (fun n -> printfn "count = %d" n) |> ignore

count.Value <- 5 // count = 5
```

## Scopes

`Signal.root`{fsharp} runs a function in a fresh scope. Every effect and computed created inside belongs to the scope; disposing the returned handle tears them all down at once:

```fsharp live
open Fable.Ripple

let count = Var.create 0

let _, dispose =
    Signal.root (fun () ->
        Signal.effect (fun () -> printfn "count = %d" count.Value) |> ignore
        Signal.onCleanup (fun () -> printfn "torn down")
    ) // the effect ran on creation: count = 0

count.Value <- 1 // count = 1
dispose.Dispose() // torn down
count.Value <- 2 // the scope is gone - nothing runs
```

After `Dispose`{fsharp}, the source lives on but the effect is detached - the write to `2` prints nothing, and no reference is left behind.

`Signal.onCleanup`{fsharp} registers a callback with the current scope, for teardown work of your own.

The DOM layer does this for you: every row of an `Html.each`{fsharp} and every branch of an `Html.switch`{fsharp} runs in its own root, disposed when the row or branch leaves.
