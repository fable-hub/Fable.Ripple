var e=`module Demo.Examples.Rendering.Attributes

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// Every attribute member comes in the same three shapes:
//
//   attr.style "color: red"          a literal - set once, no binding
//   attr.style (fun () -> ...)       a thunk - re-run when its reads change
//   attr.style someVarOrSignal       SRTP - a Var or Signal, no conversion
//
// \`attr.style\` also takes \`(property, value)\` pairs. For anything with no named
// member, \`attr.custom\` is the escape hatch and has all three shapes too -
// \`attr.aria\` and \`attr.data\` are literal-only, so reactive aria state goes
// through \`custom\`.

let render () =
    let hue = Var.create 20
    let busy = Var.create false

    // A whole declaration, not just a colour: \`attr.style\` sets the style
    // ATTRIBUTE, so its content has to be valid CSS on its own.
    let swatch = Var.create "background: oklch(0.75 0.14 20)"

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press \`Rotate the hue\`. The \`thunk\` and \`signal\` swatches change colour. \`literal\` and \`pairs\` hold fixed values, so they are set once."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ ->
                                hue.Value <- (hue.Value + 40) % 360

                                swatch.Value <-
                                    sprintf "background: oklch(0.75 0.14 %d)" (hue.Value)
                            )
                            Html.text "Rotate the hue"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> busy.Value <- not (busy.Value))
                            Html.text (fun () ->
                                if busy.Value then
                                    "Finish"
                                else
                                    "Start working"
                            )
                        ]
                ]

            Html.div
                [
                    attr.role "group"

                    // 1 - literal, set once.
                    Html.div
                        [
                            attr.className "swatch"
                            attr.style "background: var(--sg-border)"
                            Html.text "literal"
                        ]

                    // 2 - a thunk over a signal.
                    Html.div
                        [
                            attr.className "swatch"
                            attr.style (fun () ->
                                sprintf "background: oklch(0.75 0.14 %d)" hue.Value
                            )
                            Html.text "thunk"
                        ]

                    // 3 - the Var itself, via SRTP.
                    Html.div
                        [
                            attr.className "swatch"
                            attr.style swatch
                            Html.text "signal"
                        ]

                    // 4 - pairs, when a string literal reads badly.
                    Html.div
                        [
                            attr.className "swatch"

                            attr.style
                                [
                                    "background", "var(--sg-bg-subtle)"
                                    "border", "1px solid var(--sg-ink)"
                                ]

                            Html.text "pairs"
                        ]
                ]

            // Boolean properties take the same three shapes. \`disabled\` is a
            // live DOM property, not an attribute - see "Attribute vs property".
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            attr.disabled busy
                            Html.text "disabled while busy"
                        ]

                    Html.label
                        [
                            // Reactive aria state has to go through \`custom\`;
                            // \`attr.aria\` only takes a literal.
                            attr.custom ("aria-busy", fun () -> string(busy.Value).ToLower())
                            attr.data ("role", "status")

                            Html.text "aria-busy"

                            Html.output [ Html.text (fun () -> string(busy.Value).ToLower()) ]
                        ]
                ]

        ]
`;export{e as default};