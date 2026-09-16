module Demo.Examples.UnderTheHood.Cutoff

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line
open Demo.Examples.Schematic // demo-hide-line

// The cutoff is a property of every derived value, not something you opt into
// per call site - there is no memo to remember.

let render () =
    // demo-hide
    let repaint = Repaint()
    let trace = Trace()

    let parityHits = Probe("parity ran")
    let expensiveHits = Probe("expensive ran")

    // demo-show
    let n = Var.create 4

    let parity =
        n
        |> Signal.map (fun v ->
            // demo-hide
            parityHits.Hit()
            trace.Note "parity"

            // demo-show
            if v % 2 = 0 then
                "even"
            else
                "odd"
        )

    let expensive =
        parity
        |> Signal.map (fun p ->
            // demo-hide
            expensiveHits.Hit()
            trace.Note "expensive"
            // demo-show
            p.ToUpper() + "!"
        )

    let step (by: int) =
        // demo-hide
        trace.Begin()
        // demo-show
        n.Value <- n.Value + by
        // demo-hide
        repaint.Now()

    // Three nodes in a line. The dot travelling between them is the propagation,
    // so a press that stops at `parity` is a dot that never reaches `expensive`.
    let nNode = Node.source "n" n

    let parityNode = Node.derived "parity" parity |> Node.counting parityHits

    let expensiveNode =
        Node.derived "expensive" expensive |> Node.counting expensiveHits

    // demo-show
    Stack.stack
        [
            // demo-hide
            Try.observe
                "Press `+2`: `parity` runs, gets `even` again, and stops there - `expensive` does not run. Press `+1`: the parity flips and `expensive` runs."

            // demo-show
            Row.row
                [
                    Button.action ("+2 (parity unchanged)", fun _ -> step 2)

                    Button.action ("+1 (parity flips)", fun _ -> step 1)
                    // demo-hide

                    Button.action (
                        "Reset counters",
                        fun _ ->
                            parityHits.Reset()
                            expensiveHits.Reset()
                            repaint.Now()
                    )
                // demo-show
                ]

            // demo-hide
            graph
                repaint
                [
                    nNode ==> parityNode
                    parityNode ==> expensiveNode
                ]

            legend

            probes
                repaint
                [
                    parityHits
                    expensiveHits
                ]

            // The propagation, for the last press. After `+2`, `parity` runs and
            // `expensive` is simply absent - the cutoff is a hole in the
            // sequence, not a counter that failed to move.
            traceStrip
                repaint
                trace
                [
                    "parity"
                    "expensive"
                ]
        // demo-show

        ]
