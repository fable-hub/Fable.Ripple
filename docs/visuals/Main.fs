module Docs.Visuals.Main

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Widgets
open Demo.Examples.Schematic

let private controls (buttons: DomItem list) =
    Html.div
        [
            attr.style "display: flex; gap: 0.5rem; flex-wrap: wrap; margin: 0 0 0.75rem 0"
            yield! buttons
        ]

let private introGraph () =
    let repaint = Repaint()
    let runs = Probe("runs")

    let a = Var.create 1
    let b = Var.create 10

    let total =
        Signal.computed (fun () ->
            runs.Hit()
            a.Value + b.Value
        )

    let bump (v: Var<int>) =
        v.Value <- v.Value + 1
        repaint.Now()

    let aNode = Node.source "Var a" a
    let bNode = Node.source "Var b" b
    let totalNode = Node.derived "Signal.computed" total |> Node.counting runs
    let readNode = Node.binding "binding" total

    Html.div
        [
            controls
                [
                    Html.button
                        [
                            on.click (fun _ -> bump a)
                            Html.text "Bump a"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> bump b)
                            Html.text "Bump b"
                        ]
                ]

            graph
                repaint
                [
                    aNode ==> totalNode
                    bNode ==> totalNode
                    totalNode ==> readNode
                ]

            legend
        ]

let private bindRewiring () =
    let repaint = Repaint()
    let runs = Probe("runs")

    let celsius = Var.create 20
    let fahrenheit = Var.create 68
    let useCelsius = Var.create true

    let shown =
        useCelsius
        |> Signal.bind (fun c ->
            runs.Hit()

            if c then
                celsius.Signal
            else
                fahrenheit.Signal
        )

    let bump (v: Var<int>) =
        v.Value <- v.Value + 1
        repaint.Now()

    let toggle () =
        useCelsius.Value <- not useCelsius.Value
        repaint.Now()

    let selectorNode = Node.flag "useCelsius" useCelsius
    let celsiusNode = Node.source "Var celsius" celsius
    let fahrenheitNode = Node.source "Var fahrenheit" fahrenheit
    let bindNode = Node.derived "Signal.bind" shown |> Node.counting runs
    let readNode = Node.binding "binding" shown

    Html.div
        [
            controls
                [
                    Html.button
                        [
                            on.click (fun _ -> toggle ())
                            Html.text "Flip the selector"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> bump celsius)
                            Html.text "Bump celsius"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> bump fahrenheit)
                            Html.text "Bump fahrenheit"
                        ]
                ]

            schematic
                repaint
                [
                    selectorNode
                    celsiusNode
                    fahrenheitNode
                    bindNode
                    readNode
                ]
                (fun () ->
                    [
                        selectorNode ==> bindNode

                        if useCelsius.Value then
                            celsiusNode ==> bindNode
                        else
                            fahrenheitNode ==> bindNode

                        bindNode ==> readNode
                    ]
                )

            legend
        ]

let private cutoffChain () =
    let repaint = Repaint()
    let parityRuns = Probe("runs")
    let upperRuns = Probe("runs")

    let n = Var.create 4

    let parity =
        n
        |> Signal.map (fun v ->
            parityRuns.Hit()

            if v % 2 = 0 then
                "even"
            else
                "odd"
        )

    let upper =
        parity
        |> Signal.map (fun p ->
            upperRuns.Hit()
            p.ToUpper() + "!"
        )

    let step (by: int) =
        n.Value <- n.Value + by
        repaint.Now()

    let nNode = Node.source "n" n
    let parityNode = Node.derived "parity" parity |> Node.counting parityRuns
    let upperNode = Node.derived "upper" upper |> Node.counting upperRuns
    let readNode = Node.binding "binding" upper

    Html.div
        [
            controls
                [
                    Html.button
                        [
                            on.click (fun _ -> step 2)
                            Html.text "+2 (parity kept)"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> step 1)
                            Html.text "+1 (parity flips)"
                        ]
                ]

            graph
                repaint
                [
                    nNode ==> parityNode
                    parityNode ==> upperNode
                    upperNode ==> readNode
                ]

            legend
        ]

let private equalWrite () =
    let repaint = Repaint()
    let runs = Probe("runs")

    let name = Var.create "ada"

    Signal.effect (fun () ->
        runs.Hit()
        name.Value |> ignore
    )
    |> ignore

    let write (value: string) =
        name.Value <- value
        repaint.Now()

    let nameNode = Node.source "Var name" name

    let effectNode =
        Node.bindingWith "effect" (fun () -> name.Peek()) |> Node.counting runs

    Html.div
        [
            controls
                [
                    Html.button
                        [
                            on.click (fun _ -> write (name.Peek()))
                            Html.text "Write the same value again"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> write (name.Peek() + "!"))
                            Html.text "Write a new value"
                        ]
                ]

            graph repaint [ nameNode ==> effectNode ]

            legend
        ]

let private autoTracking () =
    let repaint = Repaint()
    let runs = Probe("runs")

    let a = Var.create 1
    let b = Var.create 10
    let includeB = Var.create false

    let total =
        Signal.computed (fun () ->
            runs.Hit()

            if includeB.Value then
                a.Value + b.Value
            else
                a.Value
        )

    let bump (v: Var<int>) =
        v.Value <- v.Value + 1
        repaint.Now()

    let toggle () =
        includeB.Value <- not includeB.Value
        repaint.Now()

    let switchNode = Node.flag "read b" includeB
    let aNode = Node.source "Var a" a
    let bNode = Node.source "Var b" b
    let totalNode = Node.derived "Signal.computed" total |> Node.counting runs
    let readNode = Node.binding "binding" total

    Html.div
        [
            controls
                [
                    Html.button
                        [
                            on.click (fun _ -> toggle ())
                            Html.text "Toggle read b"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> bump a)
                            Html.text "Bump a"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> bump b)
                            Html.text "Bump b"
                        ]
                ]

            schematic
                repaint
                [
                    switchNode
                    aNode
                    bNode
                    totalNode
                    readNode
                ]
                (fun () ->
                    [
                        switchNode ==> totalNode
                        aNode ==> totalNode

                        if includeB.Value then
                            bNode ==> totalNode

                        totalNode ==> readNode
                    ]
                )

            legend
        ]

let private batching () =
    let repaint = Repaint()
    let fired = Probe("fired")
    let recomputed = Probe("Signal.map3")

    let a = Var.create 0
    let b = Var.create 0
    let c = Var.create 0

    let total =
        Signal.map3
            (fun x y z ->
                recomputed.Hit()
                x + y + z
            )
            a
            b
            c

    Signal.subscribe (fun _ -> fired.Hit()) total |> ignore

    // on.* handlers batch their writes; the unbatched case must escape to a timeout.
    let writeAll (batchIt: bool) =
        Browser.Dom.window.setTimeout (
            (fun () ->
                let writes () =
                    a.Value <- a.Value + 1
                    b.Value <- b.Value + 1
                    c.Value <- c.Value + 1

                if batchIt then
                    Signal.batch writes
                else
                    writes ()

                repaint.Now()
            ),
            0
        )
        |> ignore

    let aNode = Node.source "Var a" a
    let bNode = Node.source "Var b" b
    let cNode = Node.source "Var c" c

    let totalNode =
        Node.derived "Signal.map3" total |> Node.beating (fun () -> recomputed.Hits)

    let subNode =
        Node.bindingWith "subscriber" (fun () -> string (total.Peek()))
        |> Node.counting fired

    Html.div
        [
            controls
                [
                    Html.button
                        [
                            on.click (fun _ -> writeAll false)
                            Html.text "Three writes, no batch"
                        ]
                    Html.button
                        [
                            on.click (fun _ -> writeAll true)
                            Html.text "Three writes, batched"
                        ]
                ]

            graph
                repaint
                [
                    aNode ==> totalNode
                    bNode ==> totalNode
                    cNode ==> totalNode
                    totalNode ==> subNode
                ]

            legend
        ]

let private visuals =
    Map.ofList
        [
            "signals-intro", introGraph
            "signals-bind", bindRewiring
            "signals-cutoff", cutoffChain
            "signals-equal-write", equalWrite
            "signals-autotrack", autoTracking
            "signals-batch", batching
        ]

let private mountAll () =
    let hosts = Browser.Dom.document.querySelectorAll "[data-visual]"

    for i in 0 .. hosts.length - 1 do
        let host = hosts.item i :?> Browser.Types.HTMLElement
        let name = host.getAttribute "data-visual"

        match Map.tryFind name visuals with
        | Some make -> host.appendChild (Html.render (make ())) |> ignore
        | None -> Browser.Dom.console.warn $"No visual named '{name}'"

mountAll ()
