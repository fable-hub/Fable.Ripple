module Demo.Examples.UnderTheHood.Cleanup

open Fable.Core
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line

let private running = Var.create 0

let private stopwatch (id: int) =
    let seconds = Var.create 0
    let timer = JS.setInterval (fun () -> seconds.Value <- seconds.Value + 1) 1000
    running.Value <- running.Peek() + 1

    Signal.onCleanup (fun () ->
        JS.clearInterval timer
        running.Value <- running.Peek() - 1
    )

    Html.li
        [
            attr.className "todo-item"
            Html.span [ Html.text (sprintf "Stopwatch %d" id) ]
            Html.output [ Html.text (fun () -> sprintf "%d s" seconds.Value) ]
        ]

let render () =
    let stopwatches =
        Var.create
            [|
                1
                2
            |]

    let mutable nextId = 3

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Every stopwatch starts a timer and stops it in `Signal.onCleanup`. Add and remove a few: `timers running` always matches the stopwatches on screen. Leave this page and come back, and it still does."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ ->
                                stopwatches.Value <- Array.append stopwatches.Value [| nextId |]
                                nextId <- nextId + 1
                            )
                            Html.text "Add a stopwatch"
                        ]

                    Html.button
                        [
                            attr.disabled (fun () -> Array.isEmpty stopwatches.Value)
                            on.click (fun _ -> stopwatches.Value <- Array.tail stopwatches.Value)
                            Html.text "Remove the first"
                        ]

                    Html.label
                        [
                            Html.text "timers running"
                            Html.output running
                        ]
                ]

            Html.ul [ Html.each (fun () -> stopwatches.Value) id stopwatch ]
        ]
