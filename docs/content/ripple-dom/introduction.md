---
title: Introduction
---

Fable.Ripple.Dom is an HTML DSL where [signals](../ripple/introduction.md) bind straight to the DOM. There is no virtual tree: a signal knows which nodes read it, so a write updates those nodes and nothing else.

The demos on these pages are live. Press **Run** and the snippet is compiled by Fable in your browser; the **Result** tab is a real page - click its buttons, type in its fields, then edit the code and run it again.

## Installation

```bash frame="terminal"
dotnet add package Fable.Ripple.Dom --prerelease
```

## A first component

A component is a function returning a `DomItem`{fsharp}. State lives in a `Var`{fsharp}; the view reads it, and the DOM follows every write:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let counter () =
    let count = Var.create 0

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> count.Value <- count.Value + 1)
                    Html.text "Count"
                ]
            Html.output count
        ]

Html.mount "app" (counter ())
```

Clicking the button writes `count.Value`{fsharp}. The `<output>`{html} element reads `count`{fsharp}, so it is the only node that updates - no re-render of the component, no diff.

## Mounting

`Html.mount`{fsharp} renders an item into the element with the given id:

```fsharp
Html.mount "app" (counter ())
```

`Html.render`{fsharp} returns the `HTMLElement`{fsharp} instead of mounting it, for you to place yourself:

```fsharp
open Browser

let element = Html.render (counter ())
document.body.appendChild element |> ignore
```
