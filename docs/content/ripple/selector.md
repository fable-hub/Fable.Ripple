---
title: Selectors
---

`Signal.selector`{fsharp} turns a signal into a function that tells whether a key is the current value of the signal. When the signal changes, only the readers of the previous key and of the next key run again.

```fsharp
Signal.selector : Signal<'k> -> ('k -> bool)
```

## Usage

```fsharp live
open Fable.Ripple

let selected = Var.create 1
let isSelected = Signal.selector selected

// Runs immediately: row 1 = true, row 2 = false, row 3 = false
for id in 1..3 do
    Signal.effect (fun () -> printfn "row %d = %b" id (isSelected id)) |> ignore

selected.Value <- 3 // row 1 = false, row 3 = true
selected.Value <- 3 // equal value, nothing runs
```

The second write does not reach the readers. Row 2 does not run again, because its key is neither the previous value nor the next value.

## In a list

Use a selector when each row of a list compares the same signal to its own key:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

type Item = { Id: int; Label: string }

let app () =
    let items = Var.create [| for i in 1..5 -> { Id = i; Label = "item " + string i } |]
    let selected = Var.create 0
    let isSelected = Signal.selector selected

    Html.div
        [
            Html.template (
                items,
                _.Id,
                fun item ->
                    Html.div
                        [
                            attr.className "card"
                            attr.style "cursor: pointer"
                            attr.toggleClass ("active", fun () -> isSelected item.Value.Id)
                            on.click (fun _ -> selected.Value <- item.Value.Id)
                            Html.text (fun () -> item.Value.Label)
                        ]
            )
        ]

Html.mount "app" app |> ignore
```

A click runs the bindings of two rows: the row that loses the selection and the row that gets it. With `selected.Value = item.Value.Id`{fsharp}, a click runs the binding of every row.

## Keys

Keys use F# equality. Two records or tuples with the same fields are the same key:

```fsharp
let selected = Var.create (1, "a")
let isSelected = Signal.selector selected

isSelected (1, "a") // true
```

## Reads outside an effect

Outside an effect or a binding, for example in an event handler, `isSelected key`{fsharp} returns the current result.

## Lifetime

Create the selector in the scope that uses it, for example in the component function. The selector stops when that scope is disposed.
