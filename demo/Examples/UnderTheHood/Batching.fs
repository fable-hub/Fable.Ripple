module Demo.Examples.UnderTheHood.Batching

open Browser
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line

let render () =
    let a = Var.create 0
    let b = Var.create 0
    let c = Var.create 0

    let total = Signal.map3 (fun x y z -> x + y + z) a b c

    let runs = Var.create 0

    Signal.subscribe (fun _ -> runs.Value <- runs.Peek() + 1) total |> ignore

    let addOneToEach () =
        a.Value <- a.Value + 1
        b.Value <- b.Value + 1
        c.Value <- c.Value + 1

    let later (f: unit -> unit) = window.setTimeout (f, 0) |> ignore

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Each button adds 1 to `a`, `b` and `c`, then counts how often the subscriber on their total runs. From a click: once. From a timeout: three times, once per write. From a timeout inside `Signal.batch`: once again."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ ->
                                runs.Value <- 0
                                addOneToEach ()
                            )
                            Html.text "In a click"
                        ]

                    Html.button
                        [
                            on.click (fun _ ->
                                runs.Value <- 0
                                later addOneToEach
                            )
                            Html.text "In a timeout"
                        ]

                    Html.button
                        [
                            on.click (fun _ ->
                                runs.Value <- 0
                                later (fun () -> Signal.batch addOneToEach)
                            )
                            Html.text "In a timeout, batched"
                        ]
                ]

            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "total"
                            Html.output total
                        ]

                    Html.label
                        [
                            Html.text "subscriber runs"
                            Html.output runs
                        ]
                ]
        ]
