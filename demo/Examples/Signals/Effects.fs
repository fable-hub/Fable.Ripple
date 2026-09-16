module Demo.Examples.Signals.Effects

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line
open Demo.Examples.Schematic // demo-hide-line

let render () =
    // demo-hide-next-line
    let repaint = Repaint()
    let log = Log()

    let a = Var.create 0
    let b = Var.create 0

    // Two things, split by whether anything renders them. `live` is UI state -
    // the buttons and the graph read it, so it is a `Var`. The handle is only
    // ever disposed, so it is a plain `ref` and cannot be read reactively by
    // accident.
    let live = Var.create false
    let handle = ref None

    // demo-hide
    let runs = Probe("effect ran")

    // demo-show
    let start () =
        // Reads both, so it re-runs on either - the dependency set is whatever
        // the body touched.
        let effect =
            Signal.effect (fun () ->
                // demo-hide
                runs.Hit()
                // demo-show
                log.Add(sprintf "effect saw a=%d b=%d" a.Value b.Value)
            )

        handle.Value <- Some effect
        live.Value <- true
        // demo-hide
        repaint.Now()
    // demo-show

    let stop () =
        handle.Value |> Option.iter _.Dispose()
        handle.Value <- None
        live.Value <- false
        // demo-hide
        repaint.Now()
    // demo-show

    // An effect runs once on creation, so the first log line lands when this
    // page opens - at module load it would have logged once, at app start.
    start ()

    // Runs when this example is removed from the page, so an effect the reader
    // left running is disposed along with it.
    Signal.onCleanup stop

    let bump (v: Var<int>) =
        v.Value <- v.Value + 1
        // demo-hide
        repaint.Now()

    // The effect is drawn as a leaf: it consumes values and produces none. Its
    // edges are in the thunk, so disposing it does not grey the picture out -
    // the arrows physically leave, which is what disposal actually does.
    let aNode = Node.source "Var a" a
    let bNode = Node.source "Var b" b

    let effectNode =
        Node.bindingWith
            "Signal.effect"
            (fun () ->
                if live.Value then
                    "running"
                else
                    "disposed"
            )
        |> Node.counting runs
    // demo-show

    Html.fragment
        [
            // demo-hide
            Try.observe
                "The log already has a line: an effect runs as soon as it is created. Dispose it and `Write a` reaches nothing. Start it again and it runs straight away with the current values."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ -> bump a)
                            Html.text "Write a"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> bump b)
                            Html.text "Write b"
                        ]

                    Html.button
                        [
                            attr.disabled (fun () -> not live.Value)
                            on.click (fun _ -> stop ())
                            Html.text "Dispose the effect"
                        ]

                    Html.button
                        [
                            attr.disabled live
                            on.click (fun _ -> start ())
                            Html.text "Start it again"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> log.Clear())
                            Html.text "Clear console"
                        ]
                ]

            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "a"
                            Html.output a
                        ]

                    Html.label
                        [
                            Html.text "b"
                            Html.output b
                        ]

                    Html.label
                        [
                            Html.text "effect"

                            Html.output
                                [
                                    Html.text (fun () ->
                                        if live.Value then
                                            "running"
                                        else
                                            "disposed"
                                    )
                                ]
                        ]
                ]

            // demo-hide
            schematic
                repaint
                [
                    aNode
                    bNode
                    effectNode
                ]
                (fun () ->
                    [
                        if live.Value then
                            aNode ==> effectNode
                            bNode ==> effectNode
                    ]
                )

            legend // demo-hide-line
            // demo-show

            logPane log

        ]
