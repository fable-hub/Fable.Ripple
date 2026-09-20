---
title: Events
---

Handlers go in the element's list like everything else. `on.click`{fsharp}, `on.keyDown`{fsharp}, `on.focus`{fsharp} and the rest hand you the raw DOM event:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let mouseX = Var.create 0.
    let mouseY = Var.create 0.

    Html.div
        [
            attr.className "mouse-move-demo"
            on.mouseMove (fun ev ->
                mouseX.Value <- ev.clientX
                mouseY.Value <- ev.clientY
            )
            Html.text (fun () -> $"mouseX = %.0f{mouseX.Value} - mouseY = %.0f{mouseY.Value}")
        ]

Html.mount "app" app |> ignore
```

## Value-extracting shortcuts

Some handlers hand you the value instead of the event. `on.input`{fsharp} and `on.change`{fsharp} pass the field's text; `on.checkedChange`{fsharp} passes the checkbox state:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let text = Var.create ""
    let lastKey = Var.create "-"
    let agreed = Var.create false

    Html.div
        [
            Html.input
                [
                    on.input (fun value -> text.Value <- value)
                    on.keyDown (fun ev -> lastKey.Value <- ev.key)
                ]

            Html.label
                [
                    Html.input
                        [
                            attr.type' "checkbox"
                            on.checkedChange (fun b -> agreed.Value <- b)
                        ]
                    Html.text "agree"
                ]

            Html.p [ Html.text (fun () -> $"text: %s{text.Value}") ]
            Html.p [ Html.text (fun () -> $"last key: %s{lastKey.Value}") ]
            Html.p [ Html.text (fun () -> $"agreed: %b{agreed.Value}") ]
        ]

Html.mount "app" app |> ignore
```

`on.input`{fsharp} fires on every keystroke; `on.change`{fsharp} when the field is left. Both also exist as raw `Event`{fsharp} overloads when you need the event itself.

When the handler only writes a `Var`{fsharp} that the same input displays, use a [binding](bindings.md) instead - it is one line for both directions.
