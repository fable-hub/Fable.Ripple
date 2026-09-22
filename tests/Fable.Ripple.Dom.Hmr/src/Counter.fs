module Counter

open Fable.Ripple
open Fable.Ripple.Dom

/// A plain component. Nothing here mentions HMR.
[<Component>]
let render () : DomItem =
    let count = Var.create 0
    let draft = Var.create ""

    let summary = Signal.map (fun c -> "count=" + string c) count

    Html.div
        [
            attr.id "counter"
            Html.div
                [
                    attr.id "label"
                    Html.text (fun () -> "VERSION-ONE " + Shared.greeting + " " + summary.Value)
                ]
            Html.input
                [
                    attr.id "note"
                    attr.bindValue draft
                ]
            Html.div
                [
                    attr.id "wrap"
                    Widgets.button "press" (fun () -> count.Value <- count.Value + 10)
                ]
            Html.div
                [
                    attr.id "partial"
                    (let half = Widgets.button "partial"
                     half (fun () -> count.Value <- count.Value + 100))
                ]
            Html.button
                [
                    attr.id "inc"
                    on.click (fun _ -> count.Value <- count.Value + 1)
                    Html.text "+"
                ]
        ]
