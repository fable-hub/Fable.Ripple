---
title: Control flow
---

## Show

`Html.show`{fsharp} renders its branch while a condition holds, otherwise nothing - or a fallback, with the three-argument form. The branch is built fresh when it appears and disposed when it leaves:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let visible = Var.create false

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> visible.Value <- not visible.Value)
                    Html.text "Toggle details"
                ]

            Html.show (visible, (fun () -> Html.p "Built when it appears, disposed when it leaves."))
        ]

Html.mount "app" app |> ignore
```

The condition can be a `Var`{fsharp}/`Signal`{fsharp} of `bool`{fsharp}, as above, or any `unit -> bool`{fsharp} - it is auto-tracked either way.

## Switching views

`Html.switch`{fsharp} takes the signal that decides the shape and hands its value to a function. Only that signal rebuilds the subtree:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

type Tab =
    | Info
    | Settings

let app () =
    let tab = Var.create Info
    let ticks = Var.create 0

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> tab.Value <- if tab.Value = Info then Settings else Info)
                    Html.text "Switch tab"
                ]
            Html.button
                [
                    on.click (fun _ -> ticks.Value <- ticks.Value + 1)
                    Html.text "Tick"
                ]

            Html.switch
                tab
                (function
                | Info ->
                    let typed = Var.create ""

                    Html.div
                        [
                            Html.p $"ticks when this branch was built = %d{ticks.Value}"
                            Html.p [ Html.text (fun () -> $"ticks now = %d{ticks.Value}") ]
                            Html.input [ attr.bindValue typed ]
                        ]
                | Settings -> Html.p "The settings tab.")
        ]

Html.mount "app" app |> ignore
```

Press Tick. The second line moves, the first does not, and the box keeps what you typed - `ticks`{fsharp} is not what `switch`{fsharp} is watching.

That is the split to design around. **Shape** comes from the signal you name. **Content** comes from bindings nested inside the branch: `Html.text`{fsharp} with a function, a reactive attribute, `attr.bindValue`{fsharp}. A signal read directly in a branch is read once, when the branch is built.

Now press Switch tab twice. The branch really is rebuilt, so the first line catches up and the box is empty again - `typed`{fsharp} was created inside the branch, so it dies with it. Move it out to `app`{fsharp} to keep it across switches, or leave it in when a fresh field per visit is what you want.

Each branch builds in its own scope, disposed on the next rebuild, so effects and bindings inside one cannot leak. A branch may return `Html.none`{fsharp} to render nothing.

## When the shape needs more than one signal

`Html.dynamic`{fsharp} takes a plain function and re-runs it whenever **anything** it reads changes. That makes it the fallback rather than the first choice: every read in every branch is a rebuild trigger, including reads you only meant as content.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let ticks = Var.create 0

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> ticks.Value <- ticks.Value + 1)
                    Html.text "Tick"
                ]

            Html.dynamic (fun () ->
                let typed = Var.create ""

                Html.div
                    [
                        Html.p $"ticks = %d{ticks.Value}"
                        Html.input [ attr.bindValue typed ]
                    ]
            )
        ]

Html.mount "app" app |> ignore
```

Type in the box, then press Tick. The box clears, because the interpolated read of `ticks`{fsharp} made it a rebuild trigger.

Prefer to derive a signal and use `Html.switch`{fsharp}:

```fsharp
let hasItems = Signal.map (List.isEmpty >> not) items

Html.switch hasItems (function
    | true -> itemsView ()
    | false -> Html.p "Nothing yet.")
```

`Html.switchWith`{fsharp} does the same without naming the signal, when the condition is worth computing inline:

```fsharp
Html.switchWith ((fun () -> a.Value && b.Value), fun both -> ...)
```

If you do reach for `Html.dynamic`{fsharp}, `Signal.peek`{fsharp} reads a value in the body without making it a trigger.

For mutually exclusive branches that each hold form state, a few `Html.show`{fsharp}s can beat one switch: each `show`{fsharp} re-evaluates only its own condition, so a signal read in one branch cannot wipe another branch's fields.

## Keyed lists

`Html.each`{fsharp} renders one element per item and reconciles by key: an item whose key survives a change keeps its DOM node - and everything in it - rather than being rebuilt:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

type Item = { Id: int; Label: string }

let app () =
    let items =
        Var.create
            [|
                { Id = 1; Label = "first" }
                { Id = 2; Label = "second" }
            |]

    let nextId = Var.create 3

    Html.div
        [
            Html.button
                [
                    on.click (fun _ ->
                        let id = nextId.Value
                        nextId.Value <- id + 1

                        items.Value <-
                            Array.append
                                items.Value
                                [|
                                    {
                                        Id = id
                                        Label = $"item %d{id}"
                                    }
                                |]
                    )
                    Html.text "Add"
                ]
            Html.button
                [
                    on.click (fun _ -> items.Value <- Array.rev items.Value)
                    Html.text "Reverse"
                ]

            Html.ul
                [
                    Html.each
                        (fun () -> items.Value)
                        _.Id
                        (fun item -> Html.li [ Html.text item.Label ])
                ]
        ]

Html.mount "app" app |> ignore
```

Reversing moves the existing `<li>`{html} nodes; nothing is rebuilt - reconciliation computes the minimal set of moves. Each row runs in its own scope, disposed when its key disappears, so a `Var`{fsharp} created in the row renderer belongs to that row.

There is no index-keyed variant and no fallback slot: an empty array renders nothing, and an empty state is an `Html.show`{fsharp} on the emptiness, in plain F#.

## Fragments and nothing

Three items that are not elements:

- `Html.fragment`{fsharp} groups children without a wrapper. Use it when a helper returns several siblings and a `<div>`{html} around them would break the parent's layout.
- `Html.none`{fsharp} renders nothing. It is the empty branch of a conditional.
- `Html.node`{fsharp} splices in a `Node`{fsharp} built elsewhere - SVG, a server-rendered fragment, or the output of a non-Fable library.

`Html.node`{fsharp} follows the usual DOM rule: a node that is already mounted is moved, not copied.
