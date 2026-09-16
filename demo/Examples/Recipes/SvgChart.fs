module Demo.Examples.Recipes.SvgChart

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

type private Point =
    {
        Month: string
        Value: Var<float>
    }

let private months =
    [
        "jan"
        "feb"
        "mar"
        "apr"
        "may"
        "jun"
        "jul"
        "aug"
    ]

(*
    Geometry, named rather than computed inline.

    `axisBand` is the point worth spelling out: the strip under the axis line
    that belongs to the month labels. Without it the labels are placed relative
    to the bottom edge and end up crowding the axis, which is what happens when
    a chart has a padding but no notion of where its parts go.
*)
let private chartWidth = 560.0
let private chartHeight = 250.0
let private padding = 30.0
let private axisBand = 44.0
let private axisY = chartHeight - axisBand
let private plotHeight = axisY - padding

let render () =
    // demo-hide
    let repaint = Repaint()
    let scaleHits = Probe("scale recomputed")

    // demo-show
    let rnd = System.Random()

    let data =
        months
        |> List.map (fun m ->
            {
                Month = m
                Value = Var.create (float (rnd.Next(20, 100)))
            }
        )

    let hovered = Var.create (None: string option)

    // One shared derived value. Every bar reads it; it runs once per change.
    let maxValue =
        Signal.computed (fun () ->
            // demo-hide-next-line
            scaleHits.Hit()
            data |> List.map (fun p -> p.Value.Value) |> List.max |> max 1.0
        )

    let barWidth = (chartWidth - padding * 2.0) / float (List.length months)

    let randomise () =
        Signal.batch (fun () ->
            for p in data do
                p.Value.Value <- float (rnd.Next(20, 100))
        )
        // demo-hide
        repaint.Now()
    // demo-show

    let bar (index: int) (p: Point) =
        let x = padding + float index * barWidth

        Svg.g
            [
                on.mouseEnter (fun _ -> hovered.Value <- Some p.Month)
                on.mouseLeave (fun _ -> hovered.Value <- None)

                Svg.rect
                    [
                        svgAttr.custom ("x", string (x + 3.0))
                        svgAttr.custom ("width", string (barWidth - 6.0))

                        // Height and y are both derived from the value and the
                        // shared scale - two bindings, one rectangle.
                        svgAttr.custom (
                            "y",
                            fun () -> string (axisY - (p.Value.Value / maxValue.Value) * plotHeight)
                        )

                        svgAttr.custom (
                            "height",
                            fun () -> string ((p.Value.Value / maxValue.Value) * plotHeight)
                        )

                        attr.className "bar"
                        attr.classList (fun () -> [ "is-selected", hovered.Value = Some p.Month ])
                    ]

                Svg.text
                    [
                        attr.className "axis-label"
                        svgAttr.custom ("x", string (x + barWidth / 2.0))
                        // Placed off the AXIS, not off the bottom edge, so the
                        // gap above it stays the same whatever the chart's size.
                        svgAttr.custom ("y", string (axisY + 26.0))
                        svgAttr.custom ("text-anchor", "middle")
                        svgAttr.fill "currentColor"
                        Html.text p.Month
                    ]
            ]

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Press `New data`: eight bars change and the shared scale recomputes once. `Set jan to 100` changes one bar. Hover a bar to read its value."

            // demo-show
            Row.row
                [
                    Button.primary ("New data", fun _ -> randomise ())

                    Button.action (
                        "Set jan to 100",
                        fun _ ->
                            (List.head data).Value.Value <- 100.0
                            // demo-hide-next-line
                            repaint.Now()
                    )

                    // demo-hide
                    Button.action (
                        "Reset counter",
                        fun _ ->
                            scaleHits.Reset()
                            repaint.Now()
                    )

                    readout "max" (fun () -> sprintf "%.0f" maxValue.Value)

                    readout
                        "hovered"
                        (fun () ->
                            match hovered.Value with
                            | Some m -> m
                            | None -> "none"
                        )
                // demo-show
                ]

            // demo-hide
            probes repaint [ scaleHits ]

            // demo-show
            Svg.svg
                [
                    svgAttr.viewBox (sprintf "0 0 %.0f %.0f" chartWidth chartHeight)
                    attr.className "figure"

                    Svg.line
                        [
                            svgAttr.custom ("x1", string padding)
                            svgAttr.custom ("y1", string axisY)
                            svgAttr.custom ("x2", string (chartWidth - padding))
                            svgAttr.custom ("y2", string axisY)
                            svgAttr.stroke "currentColor"
                            svgAttr.strokeWidth 1
                        ]

                    Html.fragment (data |> List.mapi bar)
                ]

            Html.show (
                (fun () -> hovered.Value.IsSome),
                fun () ->
                    Html.p
                        [
                            Html.small
                                [
                                    Html.text (fun () ->
                                        match hovered.Value with
                                        | Some m ->
                                            let p = data |> List.find (fun x -> x.Month = m)
                                            sprintf "%s = %.0f" m p.Value.Value
                                        | None -> ""
                                    )
                                ]
                        ]
            )

        ]
