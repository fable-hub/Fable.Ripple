module Demo.Examples.UnderTheHood.WhenItRuns

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

let render () =
    // demo-hide
    let repaint = Repaint()

    let hits = Probe("computed ran")
    // demo-show
    let revealed = Var.create false

    let source = Var.create 2

    let derived =
        Signal.computed (fun () ->
            // demo-hide
            hits.Hit()
            // demo-show
            source.Value * 100
        )

    // Filled in by the button, which writes and then reads on the next line.
    let observation = Var.create ""

    let writeThenRead () =
        let before = source.Value
        source.Value <- before + 1
        // No await, no tick, no flush call - the graph is already up to date.
        let after = derived.Value

        observation.Value <-
            sprintf
                "wrote %d -> read derived = %d (a deferred engine would say %d)"
                (before + 1)
                after
                (before * 100)

        // demo-hide
        repaint.Now()
    // demo-show

    Stack.stack
        [
            // demo-hide
            Try.observe
                "The counter starts at 0: creating a computed does not run it, reading it does. Below, a write updates everything before the next line runs."

            // demo-show
            Row.row
                [
                    Button.primary (
                        "Reveal the derived value",
                        (fun _ ->
                            revealed.Value <- true
                            // demo-hide-next-line
                            repaint.Now()
                        ),
                        [ attr.disabled (fun () -> revealed.Value) ]
                    )
                    // demo-hide

                    probes repaint [ hits ]
                // demo-show
                ]

            // Until this branch exists, nothing reads `derived`, so it has never
            // run - the counter beside the button says 0.
            Html.show (
                revealed,
                fun () -> Html.p [ Html.text (fun () -> sprintf "derived = %d" derived.Value) ]
            )

            Row.row
                [ Button.action ("Write, then read on the next line", fun _ -> writeThenRead ()) ]

            Html.p [ Html.small [ Html.text (fun () -> observation.Value) ] ]
        ]
