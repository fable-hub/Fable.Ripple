---
title: Elements and attributes
---

An element takes one list holding everything: children, attributes and event handlers, in any order.

```fsharp
Html.article
    [
        attr.className "card"
        Html.h2 "Title"
        Html.p "Body text."
    ]
```

Everything on this page follows one rule: a value is static, a function is reactive, and a signal passed directly is reactive too.

## Text

Text-bearing elements are overloaded. A `string`{fsharp} is static. A `Var`{fsharp} or `Signal`{fsharp} passed directly updates with it. `Html.text`{fsharp} takes a function for text computed from several signals:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let count = Var.create 0

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> count.Value <- count.Value + 1)
                    Html.text "Count"
                ]

            Html.p "A static line."
            Html.p [ Html.text (fun () -> $"A computed line: count is %d{count.Value}.") ]
            Html.p count
        ]

Html.mount "app" (app ())
```

## Attributes

The same three forms apply to attributes:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let enabled = Var.create false

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> enabled.Value <- not enabled.Value)
                    Html.text "Toggle"
                ]
            Html.p
                [
                    attr.style (fun () ->
                        if enabled.Value then
                            "color: seagreen"
                        else
                            "color: gray"
                    )
                    Html.text (fun () -> if enabled.Value then "enabled" else "disabled")
                ]
        ]

Html.mount "app" (app ())
```

## Classes

Beyond `attr.className`{fsharp}, three helpers cover the common shapes:

- `attr.classes [ "card"; "wide" ]`{fsharp} - a list joined with spaces.
- `attr.classList [ "card", true; "active", false ]`{fsharp} - names paired with conditions. A function form re-evaluates them.
- `attr.toggleClass ("active", fun () -> isActive.Value)`{fsharp} - one name, one reactive condition.

`className`{fsharp} and `classes`{fsharp} own the attribute and write it whole. `classList`{fsharp} and `toggleClass`{fsharp} add and remove only their own names, so a static base class and a reactive toggle sit on the same element:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let selected = Var.create false

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> selected.Value <- not selected.Value)
                    Html.text "Select"
                ]

            Html.p
                [
                    attr.className "card"
                    attr.toggleClass ("active", selected)
                    Html.text "card stays; active comes and goes"
                ]
        ]

Html.mount "app" (app ())
```

## Form properties

`attr.value`{fsharp}, `attr.checked'`{fsharp}, `attr.disabled`{fsharp}, `attr.hidden`{fsharp}, `attr.selected`{fsharp}, `attr.readOnly`{fsharp}, `attr.required`{fsharp}, `attr.multiple`{fsharp} and `attr.isOpen`{fsharp} set DOM properties rather than attributes - what a running page reads:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let saving = Var.create false

    Html.div
        [
            Html.label
                [
                    Html.input
                        [
                            attr.type' "checkbox"
                            attr.bindChecked saving
                        ]
                    Html.text "saving"
                ]

            Html.button
                [
                    attr.disabled saving
                    Html.text "Save"
                ]
        ]

Html.mount "app" (app ())
```

## Custom attributes

`attr.custom`{fsharp} sets an attribute the DSL has no name for:

```fsharp
attr.custom ("aria-current", fun () -> if active.Value then "true" else "false")
```

It calls `setAttribute`{js}, so it writes the markup rather than the live property. For a name that is a form property, use the named member above.

## Refs

`attr.ref`{fsharp} hands you the raw `HTMLElement`{fsharp} - to attach an observer, mount a third-party widget, or read a measurement:

```fsharp
Html.div [ attr.ref (fun el -> el.textContent <- "reached directly") ]
```

It runs while the element is being built, inside the enclosing scope, so a `Signal.onCleanup`{fsharp} registered in it tears down with the element.

The element is not in the document yet. `focus()`{fsharp} does nothing, `getBoundingClientRect()`{fsharp} returns zeros, and `dialog.showModal()`{fsharp} throws. Defer those to an event or a timeout.

## SVG

The `Svg`{fsharp} type mirrors `Html`{fsharp} for elements in the SVG namespace, with `svgAttr`{fsharp} for their attributes. See [SVG](svg.md).
