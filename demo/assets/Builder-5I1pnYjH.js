var e=`module Demo.Examples.Signals.Builder

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line
open Demo.Examples.Schematic // demo-hide-line

// Each keyword compiles to one call:
//
//     let! a = x  and! b = y     ->  Signal.map2 (fun a b -> ...) x y
//     let! a = x  (alone)        ->  Signal.bind (fun a -> ...) x
//     return v                   ->  Signal.constant v
//     return! s                  ->  s, handed back untouched
//
// \`and!\` chains, so a third input is \`map3\`; past that it nests pairs. The
// lone \`let!\` form is \`Signal.bind\`, covered in the Derived signals docs.

let render () =
    // demo-hide
    let repaint = Repaint()

    let ceHits = Probe("signal { } ran")
    let mapHits = Probe("map2 ran")

    // demo-show
    let width = Var.create 3
    let height = Var.create 4

    let viaCe =
        signal {
            let! w = width
            and! h = height
            // demo-hide
            ceHits.Hit()
            // demo-show
            return w * h
        }

    let viaMap2 =
        Signal.map2
            (fun w h ->
                // demo-hide
                mapHits.Hit()
                // demo-show
                w * h
            )
            width
            height

    // \`return\` with no \`let!\` is a constant node: it has no inputs, so it never
    // recomputes and never invalidates anything.
    let fixedRate = signal { return 20 }

    // \`return!\` forwards an existing signal unchanged - useful as the tail of a
    // branch, where the other arm computes something.
    let forwarded = signal { return! viaCe }

    let bump (v: Var<int>) =
        v.Value <- v.Value + 1
        // demo-hide
        repaint.Now()

    // Drawn as ONE graph rather than two side by side. Two identical pictures
    // would only say "these look alike"; a single picture in which both nodes
    // hang off the same two sources says they ARE the same graph, and the two
    // badges then move in step in front of you.
    let widthNode = Node.source "Var width" width
    let heightNode = Node.source "Var height" height

    let ceSource =
        "let viaCe =\\n    signal {\\n        let! w = width\\n        and! h = height\\n        return w * h\\n    }"

    let map2Source =
        "let viaMap2 =\\n    Signal.map2\\n        (fun w h -> w * h)\\n        width\\n        height"

    let fixedRateSource = "let fixedRate = signal { return 20 }"

    let forwardedSource = "let forwarded = signal { return! viaCe }"

    let ceNode = Node.derived "signal { }" viaCe |> Node.counting ceHits

    let map2Node = Node.derived "Signal.map2" viaMap2 |> Node.counting mapHits
    // demo-show

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press \`Wider\` or \`Taller\`. Both panels compute the same area from the same two sources, and each runs once per press. The \`signal { }\` block compiles to the \`Signal.map2\` call beside it."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ -> bump width)
                            Html.text "Wider"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> bump height)
                            Html.text "Taller"
                        ]

                    Html.label
                        [
                            Html.text "w x h"

                            Html.output
                                [ Html.text (fun () -> sprintf "%d x %d" width.Value height.Value) ]
                        ]
                ]

            Html.div
                [
                    Html.article
                        [
                            Html.header "let! + and!"

                            // demo-hide
                            Code.block ceSource

                            // demo-show
                            Html.p
                                [
                                    Html.small
                                        [
                                            Html.code "and!"
                                            Html.text
                                                " keeps both reads at the same level, so the two inputs are parallel rather than nested."
                                        ]
                                ]

                            Html.label
                                [
                                    Html.text "viaCe"
                                    Html.output viaCe
                                ]
                        ]

                    Html.article
                        [
                            Html.header "Signal.map2"

                            // demo-hide
                            Code.block map2Source

                            // demo-show
                            Html.p
                                [
                                    Html.small
                                        "What the block above compiles to. Same value, same run count - which is the point."
                                ]

                            Html.label
                                [
                                    Html.text "viaMap2"
                                    Html.output viaMap2
                                ]
                        ]
                ]

            Html.div
                [
                    Html.article
                        [
                            Html.header "return"

                            // demo-hide
                            Code.block fixedRateSource

                            // demo-show
                            Html.p
                                [
                                    Html.small
                                        [
                                            Html.text
                                                "Press either button: this one never moves. A "
                                            Html.code "return"
                                            Html.text " with no "
                                            Html.code "let!"
                                            Html.text
                                                " reads nothing, so the node has no inputs and nothing can invalidate it."
                                        ]
                                ]

                            Html.label
                                [
                                    Html.text "fixedRate"
                                    Html.output fixedRate
                                ]
                        ]

                    Html.article
                        [
                            Html.header "return!"

                            // demo-hide
                            Code.block forwardedSource

                            // demo-show
                            Html.p
                                [
                                    Html.small
                                        [
                                            Html.text "Always equal to "
                                            Html.code "viaCe"
                                            Html.text " on the left, because "
                                            Html.code "return!"
                                            Html.text
                                                " hands back the signal it was given - this is that same node, not a copy kept in step with it."
                                        ]
                                ]

                            Html.label
                                [
                                    Html.text "forwarded"
                                    Html.output forwarded
                                ]
                        ]
                ]

            // demo-hide
            graph
                repaint
                [
                    widthNode ==> ceNode
                    heightNode ==> ceNode
                    widthNode ==> map2Node
                    heightNode ==> map2Node
                ]

            legend
        // demo-show
        ]
`;export{e as default};