module Demo.Examples.Signals.AutoTracking

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line
open Demo.Examples.Schematic // demo-hide-line

let render () =
    // demo-hide
    let repaint = Repaint()
    let hits = Probe("total ran")

    // demo-show
    let a = Var.create 1
    let b = Var.create 10
    let includeB = Var.create false

    let total =
        Signal.computed (fun () ->
            hits.Hit() // demo-hide-line
            // The `if` decides what gets tracked. On the false branch `b.Value`
            // is never evaluated, so the edge to `b` is not created.
            if includeB.Value then
                a.Value + b.Value
            else
                a.Value
        )

    let bump (v: Var<int>) =
        v.Value <- v.Value + 1
        repaint.Now() // demo-hide-line

    // demo-hide

    // The checkbox writes through `attr.bindChecked`, so there is no handler in
    // which to bump the repaint token. Watch the outputs instead - `includeB`
    // as well as `total`, since flipping the switch has to refresh the observer
    // counts even on the runs where the total lands on the same number.
    repaintAfter
        repaint
        (fun () ->
            includeB.Value |> ignore
            total.Value |> ignore
        )

    let switchNode = Node.flag "include B" includeB

    let aNode = Node.source "Var a" a
    let bNode = Node.source "Var b" b

    let totalNode = Node.derived "Signal.computed" total |> Node.counting hits

    let readNode = Node.binding "binding" total

    // demo-show
    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press `Bump b` with `read b` unticked: nothing runs and `observers on b` reads 0. Tick `read b` and press again: `total` includes `b`, which now has 1 observer. Nothing was declared - the computation found `b` by reading it."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ -> bump a)
                            Html.text "Bump a"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> bump b)
                            Html.text "Bump b"
                        ]
                ]

            Html.label
                [
                    Html.input
                        [
                            attr.type' "checkbox"
                            attr.bindChecked includeB
                        ]

                    Html.text "read b"
                ]

            Html.div
                [
                    attr.role "group"

                    // The binding that keeps `total` observed at all - without
                    // a reader the computed would never run.
                    Html.label
                        [
                            Html.text "total"
                            Html.output total
                        ]

                    // `Signal.observerCount` reads the live edges rather than
                    // describing them, so these are the graph printed.
                    Html.label
                        [
                            Html.text "observers on a"

                            Html.output
                                [
                                    Html.text (fun () ->
                                        repaint.Track() // demo-hide-line
                                        string (Signal.observerCount a.Signal)
                                    )
                                ]
                        ]

                    Html.label
                        [
                            Html.text "observers on b"

                            Html.output
                                [
                                    Html.text (fun () ->
                                        repaint.Track() // demo-hide-line
                                        string (Signal.observerCount b.Signal)
                                    )
                                ]
                        ]
                ]

            // demo-hide
            schematic
                repaint
                [
                    switchNode
                    aNode
                    bNode
                    totalNode
                    readNode
                ]
                (fun () ->
                    [
                        switchNode ==> totalNode
                        aNode ==> totalNode

                        // Discovered, not declared: the edge exists only on the
                        // runs where the branch actually read `b`.
                        if includeB.Value then
                            bNode ==> totalNode

                        totalNode ==> readNode
                    ]
                )

            legend
        // demo-show
        ]
