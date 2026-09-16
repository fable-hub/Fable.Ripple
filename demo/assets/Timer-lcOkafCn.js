var e=`module Demo.SevenGUIs.Timer

open Fable.Core
open Fable.Ripple
open Fable.Ripple.Dom

// 7GUIs #4 - Timer: elapsed gauge, adjustable duration, reset. A JS interval
// drives \`elapsed\`; \`Signal.onCleanup\` clears it when the GUI is torn down.
let render () =
    let elapsed = Var.create 0.0 // ms
    let duration = Var.create 10000.0 // ms

    let handle =
        JS.setInterval
            (fun () ->
                if elapsed.Value < duration.Value then
                    elapsed.Value <- min duration.Value (elapsed.Value + 100.0)
            )
            100

    Signal.onCleanup (fun () -> JS.clearInterval handle)

    let pct () =
        let d = duration.Value

        (if d <= 0.0 then
             100.0
         else
             100.0 * elapsed.Value / d)
        |> min 100.0

    Html.div
        [
            Html.div
                [
                    attr.className "stack is-narrow"

                    // \`max\` stays at 100 and \`pct\` does the scaling: binding
                    // value/max directly would change the duration = 0 case,
                    // since <progress max="0"> is spec-ignored.
                    Html.label
                        [
                            attr.className "label-row"
                            Html.text "Elapsed"

                            Html.progress
                                [
                                    attr.max "100"
                                    attr.prop ("value", fun () -> box (pct ()))
                                ]
                        ]

                    Html.output [ Html.text (fun () -> sprintf "%.1f s" (elapsed.Value / 1000.0)) ]

                    Html.label
                        [
                            attr.className "label-row"
                            Html.text "Duration"

                            Html.input
                                [
                                    attr.type' "range"
                                    attr.min "0"
                                    attr.max "30000"
                                    attr.step "500"
                                    attr.value 10000
                                    on.input (fun v -> duration.Value <- float v)
                                ]
                        ]

                    Html.button
                        [
                            on.click (fun _ -> elapsed.Value <- 0.0)
                            Html.text "Reset"
                        ]
                ]
        ]
`;export{e as default};