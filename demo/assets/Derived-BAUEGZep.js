var e=`module Demo.Examples.Signals.Derived

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line
open Demo.Examples.Schematic // demo-hide-line

let render () =
    // demo-hide
    let repaint = Repaint()

    let derivedHits = Probe("Signal.map ran")
    let plainHits = Probe("plain function ran")

    // demo-show
    let source = Var.create 3

    let doubled =
        source
        |> Signal.map (fun v ->
            derivedHits.Hit() // demo-hide-line
            v * 2
        )

    let plainDoubled () =
        plainHits.Hit() // demo-hide-line
        source.Value * 2

    // Three separate bindings over the same thunk - three readers, one page.
    let threeReaders (read: unit -> int) =
        Html.div
            [
                attr.role "group"

                for label in
                    [
                        "reader a"
                        "reader b"
                        "reader c"
                    ] do
                    Html.label
                        [
                            Html.text label
                            Html.output [ Html.text (fun () -> string (read ())) ]
                        ]
            ]

    // demo-hide
    let labels =
        [
            "reader a"
            "reader b"
            "reader c"
        ]

    let srcNode = Node.source "Source" source

    let mapNode = Node.derived "Signal.map" doubled |> Node.counting derivedHits

    let readerNodes = labels |> List.map (fun label -> Node.binding label doubled)

    let plainSrcNode = Node.source "Source" source

    // ONE box, fanning out to the three readers - deliberately the same shape as
    // the left column, because the shape is not the difference. The difference
    // is the badge: this box is dotted (no node, nothing cached) and its count
    // is 3 per change, against 1 for the cached node opposite.
    //
    // Drawn as three boxes it could only ever badge 1 each, since each call site
    // runs once - and the "three times" the prose promises would appear nowhere.
    let callNode =
        Node.ghost "v * 2" (fun () -> string (source.Value * 2))
        |> Node.counting plainHits

    let plainReaderNodes =
        labels
        |> List.map (fun label -> Node.bindingWith label (fun () -> string (source.Value * 2)))

    // demo-show
    Html.fragment
        [
            // demo-hide
            Try.observe
                "Bump the source. All six readers show the same answer, but \`Signal.map\` ran once while the plain function ran three times, once per reader."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ ->
                                source.Value <- source.Value + 1
                                repaint.Now() // demo-hide-line
                            )
                            Html.text "Bump the source"
                        ]
                ]

            Html.div
                [
                    Html.article
                        [
                            Html.header "Signal.map"

                            Html.p
                                [
                                    Html.small
                                        "One cached node feeding three readers. One run per change."
                                ]

                            threeReaders (fun () -> doubled.Value)
                            // demo-hide

                            graph
                                repaint
                                [
                                    srcNode ==> mapNode

                                    for reader in readerNodes do
                                        mapNode ==> reader
                                ]
                        // demo-show
                        ]

                    Html.article
                        [
                            Html.header "plain function"

                            Html.p
                                [
                                    Html.small
                                        "Nowhere to cache, so every reader recomputes - three runs, then six, then nine."
                                ]

                            threeReaders plainDoubled
                            // demo-hide

                            graph
                                repaint
                                [
                                    plainSrcNode ==> callNode

                                    for reader in plainReaderNodes do
                                        callNode ==> reader
                                ]
                        // demo-show
                        ]
                ]

            legend // demo-hide-line
        ]
`;export{e as default};