var e=`module Demo.Examples.ControlFlow.Fragments

open Browser
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// \`Html.fragment\` is what you want when a helper returns several siblings and a
// stray \`<div>\` would break the parent's layout - a table row's cells, a pair of
// \`<dt>\`/\`<dd>\`, grid children.
//
// \`Html.node\` is the other splice: it takes a \`Node\` you built yourself and
// adopts it as a child. Standard DOM rules apply, so a node that is already
// mounted is MOVED, not copied.

let private cells () =
    [
        Html.span "one"
        Html.span "two"
        Html.span "three"
    ]

/// Counting has to wait a macrotask: \`attr.ref\` fires while the element is
/// being built, and its children have not been appended yet.
let private count (into: Var<int>) (el: Browser.Types.HTMLElement) =
    window.setTimeout ((fun () -> into.Value <- el.children.length), 0) |> ignore

let render () =
    let wrappedCount = Var.create 0
    let splicedCount = Var.create 0

    // A node built with the raw DOM API, spliced in with Html.node.
    let handMade =
        let el = document.createElement "code"
        el.textContent <- "built with document.createElement"
        el :> Types.Node

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Both panels hold the same three labels. On the left they sit inside a \`div\`, so the grid gets one child and squeezes them into one cell. On the right, \`Html.fragment\` hands the grid all three. The line below the panels is a node built with \`document.createElement\` and added with \`Html.node\`."

            // demo-show
            Html.div
                [
                    Html.article
                        [
                            Html.header "wrapped in a div"

                            Html.p
                                [
                                    Html.small
                                        "One child in the parent, and a box the parent's layout treats as a single cell."
                                ]

                            Html.div
                                [
                                    attr.className "cells"
                                    attr.ref (count wrappedCount)
                                    Html.div (cells ())
                                ]
                        ]

                    Html.article
                        [
                            Html.header "Html.fragment"

                            Html.p
                                [
                                    Html.small
                                        "Three children in the parent. They join the parent's layout directly."
                                ]

                            Html.div
                                [
                                    attr.className "cells"
                                    attr.ref (count splicedCount)
                                    Html.fragment (cells ())
                                ]
                        ]
                ]

            // demo-hide
            Html.div
                [
                    attr.role "group"

                    readout "children, wrapped" (fun () -> string wrappedCount.Value)
                    readout "children, spliced" (fun () -> string splicedCount.Value)
                ]

            // demo-show
            Html.p [ Html.node handMade ]

        ]
`;export{e as default};