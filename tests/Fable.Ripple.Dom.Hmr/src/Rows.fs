module Rows

open Fable.Ripple
open Fable.Ripple.Dom

type Item =
    {
        Id: int
        Label: string
    }

/// A component used as a keyed list row - `each` needs one element per key.
[<Component>]
let row (item: Item) : DomItem =
    let hits = Var.create 0

    Html.li
        [
            attr.id ("row-" + string item.Id)
            on.click (fun _ -> hits.Value <- hits.Value + 1)
            Html.text (fun () -> "R" + item.Label + ":" + string hits.Value)
        ]

[<Component>]
let list () : DomItem =
    let items =
        Var.create
            [|
                {
                    Id = 1
                    Label = "one"
                }
                {
                    Id = 2
                    Label = "two"
                }
            |]

    Html.ul
        [
            attr.id "rows"
            Html.each (fun () -> items.Value) (fun i -> i.Id) row
        ]

/// A component inside a dynamic region - `dynamic` caches the node it inserted.
[<Component>]
let branch (name: string) : DomItem =
    let seen = Var.create 0

    Html.div
        [
            attr.id "branch"
            on.click (fun _ -> seen.Value <- seen.Value + 1)
            Html.text (fun () -> "B" + name + ":" + string seen.Value)
        ]

let toggle = Var.create true

[<Component>]
let switcher () : DomItem =
    Html.div
        [
            attr.id "switcher"
            Html.button
                [
                    attr.id "flip"
                    on.click (fun _ -> toggle.Value <- not toggle.Value)
                    Html.text "flip"
                ]
            Html.dynamic (fun () ->
                branch (
                    if toggle.Value then
                        "left"
                    else
                        "right"
                )
            )
        ]
