var e=`module Demo.Examples.Rendering.SvgBasics

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// \`Svg.*\` creates elements in the SVG namespace, which is not optional: an
// \`<svg>\` subtree built with \`document.createElement\` renders as nothing at all,
// silently.
//
// \`svgAttr.*\` covers the SVG attribute vocabulary (\`cx\`, \`viewBox\`,
// \`stroke-width\` as \`strokeWidth\`, ...).
//
// One wrinkle: most \`svgAttr\` members take a literal, since SVG geometry is
// usually static. Reactive geometry goes through \`svgAttr.custom (name, thunk)\`,
// which has the same three shapes as \`attr.custom\`.

let render () =
    let size = Var.create 34.0
    let selected = Var.create -1

    let bars =
        [
            "a", 40.0
            "b", 72.0
            "c", 28.0
            "d", 95.0
            "e", 60.0
        ]

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press \`Grow\` and \`Shrink\`, then click a bar. SVG elements take the same attributes and events as HTML ones."

            // demo-show
            Html.div
                [
                    attr.role "group"

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

                    // demo-hide
                    readout "r" (fun () -> sprintf "%.0f" size.Value)

                    readout
                        "clicked bar"
                        (fun () ->
                            if selected.Value < 0 then
                                "none"
                            else
                                fst bars.[selected.Value]
                        )
                // demo-show
                ]

            Svg.svg
                [
                    svgAttr.viewBox "0 0 300 120"
                    svgAttr.width 300
                    svgAttr.height 120
                    // An HTML-side member on an SVG element - no special casing.
                    attr.className "figure"

                    // Reactive geometry: \`custom\` takes the thunk.
                    Svg.circle
                        [
                            svgAttr.cx 60
                            svgAttr.cy 60
                            svgAttr.custom ("r", fun () -> string size.Value)
                            attr.className "dot"
                        ]

                    Svg.line
                        [
                            svgAttr.x1 10
                            svgAttr.y1 110
                            svgAttr.x2 290
                            svgAttr.y2 110
                            svgAttr.stroke "currentColor"
                            svgAttr.strokeWidth 1
                        ]

                    // Static geometry from a plain F# list - the one-list model
                    // is the same here as in Html.
                    Svg.g
                        [
                            for i, (label, h) in List.indexed bars do
                                Svg.rect
                                    [
                                        // \`attr.className\` on an SVG element, as
                                        // above - here so the CSS can colour a
                                        // bar and give it a pointer cursor.
                                        attr.className "bar"
                                        svgAttr.x (140 + i * 30)
                                        svgAttr.custom ("y", string (110.0 - h))
                                        svgAttr.width 20
                                        svgAttr.custom ("height", string h)
                                        attr.classList (fun () ->
                                            [ "is-selected", selected.Value = i ]
                                        )
                                        // An HTML event handler on an SVG node.
                                        on.click (fun _ -> selected.Value <- i)
                                    ]

                                Svg.text
                                    [
                                        svgAttr.x (146 + i * 30)
                                        svgAttr.y 118
                                        svgAttr.custom ("font-size", "9")
                                        svgAttr.fill "currentColor"
                                        Html.text label
                                    ]
                        ]
                ]

        ]
`;export{e as default};