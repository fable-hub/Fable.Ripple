module Demo.Examples.Signals.Untracked

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line
open Demo.Examples.Schematic // demo-hide-line

// Three forms, all used below. `Peek()` does one read. `Signal.peek s` is the
// same behind an SRTP call, so it works on a `Var` or a `Signal` alike.
// `Signal.untracked f` does it for a whole block, for when the reads are buried
// in a helper you did not write.

let render () =
    let repaint = Repaint() // demo-hide-line
    // demo-hide
    let trackedHits = Probe("tracked ran")
    let peekedHits = Probe("peeked ran")

    // demo-show
    let data = Var.create 1
    let config = Var.create 10

    let trackedTotal =
        Signal.computed (fun () ->
            trackedHits.Hit() // demo-hide-line
            data.Value * config.Value
        )

    let peekedTotal =
        Signal.computed (fun () ->
            peekedHits.Hit() // demo-hide-line
            // Reads the value; does not create an edge.
            data.Value * Signal.peek config
        )

    // The block form, for when the reads are inside a helper.
    let snapshot () =
        Signal.untracked (fun () -> sprintf "data=%d config=%d" data.Value config.Value)

    let bump (v: Var<int>) =
        v.Value <- v.Value + 1
        repaint.Now() // demo-hide-line
    // demo-hide

    // The graph says it in one picture: `config` has an arrow into the left
    // computation and none at all into the right one.
    let dataNode = Node.source "Var data" data
    let configNode = Node.source "Var config" config

    let trackedNode =
        Node.derived "reads .Value" trackedTotal |> Node.counting trackedHits

    let peekedNode = Node.derived "peeks config" peekedTotal |> Node.counting peekedHits

    // demo-show
    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press `Bump data`: both columns recompute. Press `Bump config`: only the left one does, because the right reads it with `Signal.peek` and does not subscribe. `observers on config` shows the difference."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ -> bump config)
                            Html.text "Bump config"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> bump data)
                            Html.text "Bump data"
                        ]
                    Html.label
                        [
                            Html.text "observers on config"

                            Html.output
                                [
                                    Html.text (fun () ->
                                        repaint.Track() // demo-hide-line
                                        string (Signal.observerCount config.Signal)
                                    )
                                ]
                        ]
                ]

            Html.div
                [
                    Html.article
                        [
                            Html.header "reads both with .Value"

                            Html.p
                                [
                                    Html.small
                                        [
                                            Html.text
                                                "Depends on both, so changing the config recomputes it."
                                        ]
                                ]

                            Html.label
                                [
                                    Html.text "result"

                                    Html.output trackedTotal
                                ]
                        ]

                    Html.article
                        [
                            Html.header "peeks at config"

                            Html.p
                                [
                                    Html.small
                                        [
                                            Html.text
                                                "Uses the config without subscribing - only the data recomputes it."
                                        ]
                                ]

                            Html.label
                                [
                                    Html.text "result"

                                    Html.output peekedTotal
                                ]
                        ]
                ]

            Html.label
                [
                    Html.text "Signal.untracked snapshot"
                    Html.output [ Html.text (fun () -> snapshot ()) ]
                ]

            // demo-hide
            graph
                repaint
                [
                    dataNode ==> trackedNode
                    configNode ==> trackedNode

                    // ...and nothing from `configNode`. That missing arrow is
                    // the entire difference between the two columns above.
                    dataNode ==> peekedNode
                ]

            legend
        // demo-show
        ]
