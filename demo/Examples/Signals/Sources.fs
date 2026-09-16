module Demo.Examples.Signals.Sources

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line
open Demo.Examples.Schematic // demo-hide-line

let render () =
    let a = Var.create 0
    let b = Var.create 0

    // demo-hide
    let repaint = Repaint()
    let aHits = Probe("A binding ran")
    let bHits = Probe("B binding ran")

    let aVar = Node.source "Var A" a
    let bVar = Node.source "Var B" b

    let aBinding = Node.binding "binding" a |> Node.counting aHits

    let bBinding = Node.binding "binding" b |> Node.counting bHits
    // demo-show

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Bump either source. Only the binding that reads it runs again; the other one does not run at all."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ ->
                                a.Value <- a.Value + 1
                                repaint.Now() // demo-hide-line
                            )
                            Html.text "Bump A"
                        ]

                    Html.button
                        [
                            on.click (fun _ ->
                                b.Value <- b.Value + 1
                                repaint.Now() // demo-hide-line
                            )
                            Html.text "Bump B"
                        ]
                ]

            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "a"
                            Html.output a
                        ]

                    Html.label
                        [
                            Html.text "b"
                            Html.output b
                        ]
                ]

            // demo-hide
            // The counted bindings. They have to be separate from the readouts
            // above: `Html.text a` builds its thunk INSIDE the library, so there
            // is nowhere to put a `Hit()` - which is exactly why the thunk
            // overload exists. These read the same sources and render nothing.
            reader aHits a
            reader bHits b

            graph
                repaint
                [
                    aVar ==> aBinding
                    bVar ==> bBinding
                ]

            legend
        // demo-show
        ]
