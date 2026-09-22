module Pair

open Fable.Ripple
open Fable.Ripple.Dom

// MOVE-START - test.mjs relocates the block between these markers.
[<Component>]
let solo () : DomItem =
    let n = Var.create 0

    Html.div
        [
            Html.button
                [
                    attr.id "bump-solo"
                    on.click (fun _ -> n.Value <- n.Value + 1)
                    Html.text "+"
                ]
            Html.span
                [
                    attr.id "out-solo"
                    Html.text (fun () -> "S" + string n.Value)
                ]
        ]
// MOVE-END

/// The shape the instance key is about: one component, two instances on the page.
[<Component>]
let cell (tag: string) : DomItem =
    let n = Var.create 0

    Html.div
        [
            Html.button
                [
                    attr.id ("bump-" + tag)
                    on.click (fun _ -> n.Value <- n.Value + 1)
                    Html.text "+"
                ]
            Html.span
                [
                    attr.id ("out-" + tag)
                    Html.text (fun () -> "A" + string n.Value)
                ]
        ]

/// A component holding components, so the key has to nest.
[<Component>]
let both () : DomItem =
    Html.div
        [
            attr.id "pair"
            cell "a"
            cell "b"
        ]
