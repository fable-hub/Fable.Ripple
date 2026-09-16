module Demo.Examples.Rendering.Bindings

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// `attr.bindValue` and `attr.bindChecked` take a `Var` specifically - a
// read-only `Signal` would have nothing to write back to, and that is a compile
// error rather than a surprise.
//
// The manual pair is `attr.value` plus `on.input`, for when the two directions
// are not symmetric: validation, normalising, or refusing a value outright.

let render () =
    let bound = Var.create "ada"
    let manual = Var.create "ADA"
    let rejected = Var.create 0

    let agreed = Var.create false

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Type in the first box and the text below follows. Press `Toggle it from code` and the checkbox follows. The right-hand box accepts only digits, so it wires the two directions by hand."

            // demo-show
            Html.div
                [
                    Html.article
                        [
                            Html.header "attr.bindValue"

                            Html.p
                                [
                                    Html.small
                                        "Both directions in one line: typing writes the signal, the signal writes the field."
                                ]

                            Html.input [ attr.bindValue bound ]

                            Html.label
                                [
                                    Html.text "stored"
                                    Html.output bound
                                ]
                        ]

                    Html.article
                        [
                            Html.header "attr.value + on.input"

                            Html.p
                                [
                                    Html.small
                                        "The same two directions spelled out - for when the write has to do something else first."
                                ]

                            Html.input
                                [
                                    // The element's value follows the Var...
                                    attr.value manual

                                    // ...and this decides what the Var accepts.
                                    on.input (fun (raw: string) ->
                                        let kept =
                                            raw |> String.filter (System.Char.IsDigit >> not)

                                        if kept.Length <> raw.Length then
                                            rejected.Value <- rejected.Value + 1

                                        manual.Value <- kept.ToUpper()
                                    )
                                ]

                            // demo-hide
                            Html.div
                                [
                                    attr.role "group"

                                    readout "stored" (fun () -> manual.Value)
                                    readout "digits dropped" (fun () -> string rejected.Value)
                                ]
                        // demo-show
                        ]
                ]

            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.input
                                [
                                    attr.type' "checkbox"
                                    attr.bindChecked agreed
                                ]

                            Html.text "bindChecked"
                        ]

                    Html.button
                        [
                            // Writing the Var moves the checkbox: the binding
                            // runs in both directions, not just element-to-state.
                            on.click (fun _ -> agreed.Value <- not (agreed.Value))
                            Html.text "Toggle it from code"
                        ]

                    // demo-hide
                    readout "agreed" (fun () -> string agreed.Value)
                // demo-show
                ]

        ]
