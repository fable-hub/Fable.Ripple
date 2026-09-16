module Demo.SevenGUIs.Counter

open Fable.Ripple
open Fable.Ripple.Dom

// 7GUIs #1 - Counter: a value and a button that increments it.
let render () =
    let count = Var.create 0

    Html.div
        [
            Html.div
                [
                    attr.className "row"
                    Html.output count
                    Html.button
                        [
                            on.click (fun _ -> count.Value <- count.Value + 1)
                            Html.text "Count"
                        ]
                ]
        ]
