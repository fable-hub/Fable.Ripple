---
title: Equality and cutoff
---

Every signal compares its new value to its old one, and an equal value goes no further. The cutoff is a property of every source and every derived value, not something you opt into per call site - there is no memo to remember.

## Propagation stops mid-graph

A dependency can change while the derived value does not. Everything downstream of the unchanged value stays untouched:

```fsharp live
open Fable.Ripple

let count = Var.create 0
let isEven = Signal.map (fun n -> n % 2 = 0) count

// Runs immediately: isEven = true
Signal.effect (fun () -> printfn "isEven = %b" isEven.Value) |> ignore

count.Value <- 2 // recomputes to true - equal, nothing runs
count.Value <- 4 // same again
count.Value <- 5 // isEven = false
```

The writes to `2` and `4` recompute `isEven`{fsharp}, find it still `true`{fsharp}, and stop there - the effect does not run. Only `5` reaches it.

## The cutoff, drawn

Three nodes in a line. `parity`{fsharp} maps a number to a word, and `upper`{fsharp} maps that word again:

```fsharp
let n = Var.create 4

let parity = n |> Signal.map (fun v -> if v % 2 = 0 then "even" else "odd")
let upper = parity |> Signal.map (fun p -> p.ToUpper() + "!")
```

Press `+2`: `parity`{fsharp} recomputes to the same string and the dot dies there - `upper`{fsharp} never runs. Press `+1` and the change travels the whole chain.

<div data-visual="signals-cutoff"></div>
<link rel="stylesheet" href="/Fable.Ripple/visuals/visuals.css">
<script type="module" src="/Fable.Ripple/visuals/Main.js"></script>

## Custom equality

The comparison is a parameter, on sources and derived values alike:

```fsharp
Var.createWith equals initial
Signal.computedWith equals f
Signal.mapWith equals f s
Signal.map2With equals f a b
```

The default is structural. `Signal.referenceEquals`{fsharp} opts out of it, for large values where a structural compare costs more than a recompute:

```fsharp live
open Fable.Ripple

let items = Var.create [ 3; 1; 2 ]

let sorted = Signal.mapWith Signal.referenceEquals List.sort items

// Runs immediately: sorted = [1; 2; 3]
Signal.effect (fun () -> printfn "sorted = %A" sorted.Value) |> ignore

items.Value <- [ 2; 1; 3 ] // rebuilt, same content: sorted = [1; 2; 3] again
```

The write recomputes `sorted`{fsharp} to `[1; 2; 3]`{fsharp} - structurally the same as before, but a freshly built list. Under the default comparison the effect would not rerun; under reference equality it does. That is the trade: no structural compare on every change, at the price of reacting to rebuilds that changed nothing.
