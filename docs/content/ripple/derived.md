---
title: Derived signals
---

## Mapping

`Signal.map`{fsharp} derives from one signal, `map2`{fsharp} and `map3`{fsharp} combine several:

```fsharp live
open Fable.Ripple

let count = Var.create 2

let double = Signal.map (fun n -> n * 2) count
let label = Signal.map2 (fun n d -> $"%d{n} doubled is %d{d}") count double

printfn "%s" label.Value // 2 doubled is 4
count.Value <- 21
printfn "%s" label.Value // 21 doubled is 42
```

## Auto-tracking

`Signal.computed`{fsharp} takes a plain function. Whatever it reads while running is a dependency - there is no list to declare:

```fsharp live
open Fable.Ripple

let price = Var.create 10.0
let quantity = Var.create 2

let total = Signal.computed (fun () -> price.Value * float quantity.Value)

printfn "%.2f" total.Value // 20.00
quantity.Value <- 5
printfn "%.2f" total.Value // 50.00
```

An `if`{fsharp} decides what gets tracked: `b`{fsharp} is a dependency only while `read b` is on. Bump it with the switch off and nothing runs - there is no edge to carry the change:

<div data-visual="signals-autotrack"></div>
<link rel="stylesheet" href="/Fable.Ripple/visuals/visuals.css">
<script type="module" src="/Fable.Ripple/visuals/Main.js"></script>

## Cached and lazy

A derived signal computes when read, and remembers the result until a dependency changes:

```fsharp live
open Fable.Ripple

let count = Var.create 1

let double =
    Signal.computed (fun () ->
        printfn "computing..."
        count.Value * 2
    )

printfn "%d" double.Value // computing... then 2
printfn "%d" double.Value // 2 - served from the cache
count.Value <- 5 // marks the signal stale, nothing runs
printfn "%d" double.Value // computing... then 10
```

`computing...` prints twice, not four times: the second read is served from cache, and the write only marks the signal stale - the recompute happens at the next read.

## One node, many readers

The cache is shared. A derived signal with several consumers runs once per change, however many read it - where a plain function would run once per caller:

```fsharp live
open Fable.Ripple

let a = Var.create 1

let b =
    Signal.map
        (fun n ->
            printfn "b ran"
            n * 2
        )
        a

let left = Signal.map (fun n -> n + 1) b
let right = Signal.map (fun n -> n - 1) b

// Runs immediately: b ran, left = 3, right = 1
Signal.effect (fun () -> printfn "left = %d, right = %d" left.Value right.Value)
|> ignore

a.Value <- 2 // b ran (once), left = 5, right = 3
```

`b ran` prints once per write, not once per consumer.

## Dynamic dependencies

`Signal.bind`{fsharp} chooses the dependency from a value. It is not a second mechanism: `Signal.bind f s`{fsharp} is `Signal.computed (fun () -> (f s.Value).Value)`{fsharp} - what `bind`{fsharp} adds is the flattening:

```fsharp live
open Fable.Ripple

let useCelsius = Var.create true
let celsius = Var.create 21.0
let fahrenheit = Var.create 70.0

let shown =
    useCelsius
    |> Signal.bind (fun c ->
        if c then
            celsius.Signal
        else
            fahrenheit.Signal
    )

// Runs immediately: shown = 21.0
Signal.effect (fun () -> printfn "shown = %.1f" shown.Value) |> ignore

fahrenheit.Value <- 80.0 // not a dependency - nothing runs
useCelsius.Value <- false // shown = 80.0
fahrenheit.Value <- 90.0 // now tracked: shown = 90.0
```

While `useCelsius`{fsharp} is true, writing `fahrenheit`{fsharp} reruns nothing - it is not a dependency until the switch flips.

## The rewiring, drawn

The same switch, as a picture: the arrow into `Signal.bind`{fsharp} follows whichever source it currently reads.

<div data-visual="signals-bind"></div>
<link rel="stylesheet" href="/Fable.Ripple/visuals/visuals.css">
<script type="module" src="/Fable.Ripple/visuals/Main.js"></script>

Bump the unwired source and nothing moves - no flash, no dot, no run. Flip the selector and the arrow jumps.

## The `signal`{fsharp} builder

A computation expression over the same combinators. `let! ... and!`{fsharp} is applicative, a lone `let!`{fsharp} is monadic:

```fsharp live
open Fable.Ripple

let first = Var.create "Ada"
let last = Var.create "Lovelace"

let full =
    signal {
        let! f = first
        and! l = last
        return $"%s{f} %s{l}"
    }

printfn "%s" full.Value // Ada Lovelace
last.Value <- "Hopper"
printfn "%s" full.Value // Ada Hopper
```

Each keyword compiles to one call:

```fsharp
let! a = x  and! b = y     //  Signal.map2 (fun a b -> ...) x y
let! a = x                 //  Signal.bind (fun a -> ...) x
return v                   //  Signal.constant v
return! s                  //  s, handed back untouched
```
