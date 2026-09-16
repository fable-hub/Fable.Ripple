---
title: SVG
---

`Svg.*`{fsharp} creates elements in the SVG namespace. The namespace is not optional: an `<svg>`{html} subtree built with `document.createElement`{js} renders as nothing at all, silently.

`svgAttr.*`{fsharp} covers the SVG attribute vocabulary: `viewBox`, `cx`, `strokeWidth`{fsharp} for `stroke-width`, and the rest. Everything else is the same one-list model as HTML - and the HTML members mix in freely: `attr.className`{fsharp} and `on.click`{fsharp} attach to an SVG element with no special casing.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let size = Var.create 34.0

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> size.Value <- min 60.0 (size.Value + 8.0))
                    Html.text "Grow"
                ]
            Html.button
                [
                    on.click (fun _ -> size.Value <- max 10.0 (size.Value - 8.0))
                    Html.text "Shrink"
                ]

            Svg.svg
                [
                    svgAttr.viewBox "0 0 200 120"
                    svgAttr.width 200
                    svgAttr.height 120

                    Svg.circle
                        [
                            svgAttr.cx 100
                            svgAttr.cy 60
                            svgAttr.custom ("r", fun () -> string size.Value)
                            svgAttr.fill "seagreen"
                        ]
                ]
        ]

Html.mount "app" (app ())
```

## Reactive geometry

Most `svgAttr`{fsharp} members take a literal, because SVG geometry is usually static. Reactive geometry goes through `svgAttr.custom`{fsharp}, which has the same three shapes as `attr.custom`{fsharp} - a string, a function, or a signal:

```fsharp
svgAttr.custom ("r", fun () -> string size.Value)
```

The graph drawings on the [Fable.Ripple pages](../ripple/introduction.md#visualization) are built entirely this way - no diagramming library, just `Svg.*`{fsharp} and `attr.ref`{fsharp}.
