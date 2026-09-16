var e=`module Demo.Examples.ControlFlow.Show

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// Two arities: \`show (cond, whenTrue)\` renders nothing when false - that is
// \`Html.none\` - and \`show (cond, whenTrue, whenFalse)\` is if/else.

let private mounts = Var.create 0
let private unmounts = Var.create 0

let render () =
    let visible = Var.create false
    let mode = Var.create true

    let panel () =
        // \`Peek\` throughout: this builds inside \`Html.show\`'s scope, and the
        // cleanup below runs from inside the effect that tears that scope down.
        mounts.Value <- mounts.Peek() + 1
        Signal.onCleanup (fun () -> unmounts.Value <- unmounts.Peek() + 1)

        Html.article
            [
                Html.p "Type here, then hide me."

                Html.input [ attr.value "state lives here" ]
            ]

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Show the panel, type in the box, then hide and show it again. Your text is gone and both counters went up: the panel was destroyed and built again, not hidden."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ -> visible.Value <- not (visible.Value))

                            Html.text (fun () ->
                                if visible.Value then
                                    "Hide the panel"
                                else
                                    "Show the panel"
                            )
                        ]

                    Html.button
                        [
                            on.click (fun _ -> mode.Value <- not (mode.Value))
                            Html.text "Flip the if/else"
                        ]

                    // demo-hide
                    readout "built" (fun () -> string mounts.Value)
                    readout "disposed" (fun () -> string unmounts.Value)
                // demo-show
                ]

            // Two arities. The first renders nothing at all when false.
            Html.show (visible, panel)

            Html.show (
                mode,
                (fun () -> Html.p "the TRUE branch is mounted"),
                fun () -> Html.p "the FALSE branch is mounted"
            )

        ]
`;export{e as default};