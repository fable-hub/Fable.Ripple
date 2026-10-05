---
title: Templates
---

`Html.template`{fsharp} renders a list, like [`Html.each`](control-flow.md#keyed-lists). It takes the same three arguments: the items, as a signal or as a function returning an array, a function giving an item's key, and a render function that builds the element for one item. An item that keeps its key keeps its element.

The difference is how the elements are built. `Html.each`{fsharp} calls the render function for every item. `Html.template`{fsharp} calls it once, keeps the element it returns as a model, and gives every item a copy of that model. Copying a DOM element is much cheaper than building one, so a table with thousands of rows appears faster.

Because the render function runs once, it does not receive an item. It receives a `Signal`{fsharp} whose value is the item of whichever row is being built or updated at the time, and the rules below follow from that.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

type Item =
    {
        Id: int
        Label: Var<string>
    }

let app () =
    let items =
        Var.create
            [|
                for i in 1..5 ->
                    {
                        Id = i
                        Label = Var.create ("item " + string i)
                    }
            |]

    let selected = Var.create 0

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
                            attr.toggleClass ("active", fun () -> selected.Value = item.Value.Id)
                            on.click (fun _ -> selected.Value <- item.Value.Id)
                            Html.text (fun () -> string item.Value.Id + ": ")
                            Html.span [ Html.text (fun () -> item.Value.Label.Value) ]
                        ]
            )
        ]

Html.mount "app" app |> ignore
```

## Reading the item

`item.Value`{fsharp} returns the item of the current row. It works inside a binding: a function you pass to the DSL and that the library runs for each row, such as a reactive attribute, `Html.text`{fsharp} with a function, an event handler, `attr.ref`{fsharp}, `Html.show`{fsharp} or a nested `Html.each`{fsharp}. In the example above, every read of `item.Value`{fsharp} is inside one of those.

:::caution Not at the top level of the render function
The render function runs once, before any row exists. A read of `item.Value`{fsharp} there throws an error, and the error says what to change.

```fsharp
Html.template (
    items,
    _.Id,
    fun item ->
        Html.div
            [
                // throws
                Html.text (string item.Value.Id)
                // reads the item of each row
                Html.text (fun () -> string item.Value.Id)
            ]
)
```

The same applies to a signal taken from the item. `Html.text item.Value.Label`{fsharp} throws, because `item.Value`{fsharp} is read before the `Label`{fsharp} signal is passed on. Write `Html.text (fun () -> item.Value.Label.Value)`{fsharp}.

Passing `item`{fsharp} itself is fine. `Html.text item`{fsharp} shows the item of each row.
:::

A value that never changes is written the same way, with a function. A function that reads no signal runs once per row and is then dropped, so it costs nothing afterwards.

## Rows that differ

Every row is a copy of the same model, so a row cannot have a different structure of its own. To change what a row contains depending on the item, use `Html.show`{fsharp} or `Html.dynamic`{fsharp}. Each row then builds its own branch, and rebuilds it when the item's signals change:

```fsharp
Html.show (
    (fun () -> item.Value.IsHeader),
    (fun () -> Html.th [ Html.text (fun () -> item.Value.Label.Value) ]),
    fun () -> Html.td [ Html.text (fun () -> item.Value.Label.Value) ]
)
```

If most of a row depends on the item this way, use `Html.each`{fsharp} instead. There is little left to copy.

## State per row

A `Var`{fsharp} created at the top level of the render function is created once, when the model is built, and shared by every row. With `Html.each`{fsharp}, each row gets its own. Keep the state of a row on the item instead, as `Label`{fsharp} is in the example above.

## Code that runs later

:::caution The row is known only while a binding runs
Code that a binding schedules for later runs without it: a timer, a promise continuation, an observer callback. A read of `item.Value`{fsharp} from there throws the same error as a read at the top level.

Read the item into a local before scheduling:

```fsharp
attr.ref (fun el ->
    let current = item.Value
    window.setTimeout ((fun () -> current.Label.Value <- "seen"), 500) |> ignore
)
```
:::
