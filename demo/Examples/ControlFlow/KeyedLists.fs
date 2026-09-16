module Demo.Examples.ControlFlow.KeyedLists

open Browser.Types // demo-hide-line
open Fable.Core // demo-hide-line
open Fable.Core.JsInterop // demo-hide-line
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// demo-hide
let private watchMoves (moved: Var<int>) (list: HTMLElement) =
    let observer: obj =
        emitJsExpr
            (fun (records: obj[]) ->
                let mutable inserted = 0

                for r in records do
                    inserted <- inserted + r?addedNodes?length

                moved.Value <- moved.Peek() + inserted
            )
            "new MutationObserver($0)"

    emitJsStatement (observer, list) "$0.observe($1, { childList: true })"
    Signal.onCleanup (fun () -> emitJsStatement observer "$0.disconnect()")

// demo-show
type private Fruit =
    {
        Id: int
        Name: string
    }

let render () =
    let fruits =
        Var.create
            [|
                {
                    Id = 1
                    Name = "apple"
                }
                {
                    Id = 2
                    Name = "banana"
                }
                {
                    Id = 3
                    Name = "cherry"
                }
                {
                    Id = 4
                    Name = "grape"
                }
                {
                    Id = 5
                    Name = "lemon"
                }
                {
                    Id = 6
                    Name = "mango"
                }
            |]

    let mutable nextId = 7
    let built = Var.create 0 // demo-hide-line
    let moved = Var.create 0 // demo-hide-line

    let row (fruit: Fruit) =
        built.Value <- built.Peek() + 1 // demo-hide-line
        let clicks = Var.create 0

        Html.li
            [
                attr.className "todo-item"
                Html.span [ Html.text fruit.Name ]

                Html.button
                    [
                        on.click (fun _ -> clicks.Value <- clicks.Value + 1)
                        Html.text (fun () -> sprintf "clicked %d" clicks.Value)
                    ]
            ]

    let change (f: Fruit[] -> Fruit[]) =
        // demo-hide
        built.Value <- 0
        moved.Value <- 0
        // demo-show
        fruits.Value <- f fruits.Value

    let swapEnds (items: Fruit[]) =
        let copy = Array.copy items
        let last = copy.Length - 1
        copy.[0] <- items.[last]
        copy.[last] <- items.[0]
        copy

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Click a few of the counters, then reorder. `Reverse` builds 0 rows and every counter keeps its number: the rows are moved, not rebuilt. `Swap first and last` moves only 2. `Add a fruit` builds 1."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ -> change Array.rev)
                            Html.text "Reverse"
                        ]

                    Html.button
                        [
                            attr.disabled (fun () -> fruits.Value.Length < 2)
                            on.click (fun _ -> change swapEnds)
                            Html.text "Swap first and last"
                        ]

                    Html.button
                        [
                            on.click (fun _ ->
                                change (fun items ->
                                    Array.append
                                        items
                                        [|
                                            {
                                                Id = nextId
                                                Name = sprintf "fruit %d" nextId
                                            }
                                        |]
                                )

                                nextId <- nextId + 1
                            )
                            Html.text "Add a fruit"
                        ]

                    Html.button
                        [
                            attr.disabled (fun () -> Array.isEmpty fruits.Value)
                            on.click (fun _ -> change Array.tail)
                            Html.text "Remove the first"
                        ]
                ]

            // demo-hide
            Html.div
                [
                    attr.role "group"
                    readout "rows built" (fun () -> string built.Value)
                    readout "rows inserted or moved" (fun () -> string moved.Value)
                ]

            // demo-show
            Html.ul
                [
                    attr.ref (watchMoves moved) // demo-hide-line
                    Html.each (fun () -> fruits.Value) (fun fruit -> fruit.Id) row
                ]
        ]
