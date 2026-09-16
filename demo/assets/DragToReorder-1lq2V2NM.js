var e=`module Demo.Examples.Recipes.DragToReorder

open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// The reactive part is one line: the drop handler writes a reordered array to
// the \`Var\`. Everything after that is \`Html.each\` doing its job - there is no
// animation state, no placeholder element, and no list-of-refs to keep in step.

type private Row =
    {
        Id: int
        Label: string
    }

let render () =
    let rows =
        Var.create
            [|
                {
                    Id = 1
                    Label = "Collect requirements"
                }
                {
                    Id = 2
                    Label = "Draw the diagram"
                }
                {
                    Id = 3
                    Label = "Write the code"
                }
                {
                    Id = 4
                    Label = "Break the code"
                }
                {
                    Id = 5
                    Label = "Write the tests"
                }
                {
                    Id = 6
                    Label = "Ship it"
                }
            |]

    let dragging = Var.create (None: int option)
    let over = Var.create (None: int option)
    let drops = Var.create 0

    let move (fromId: int) (toId: int) =
        if fromId <> toId then
            let current = rows.Value
            let item = current |> Array.find (fun r -> r.Id = fromId)
            let without = current |> Array.filter (fun r -> r.Id <> fromId)
            let index = without |> Array.findIndex (fun r -> r.Id = toId)

            rows.Value <-
                Array.concat
                    [
                        without.[.. index - 1]
                        [| item |]
                        without.[index..]
                    ]

            drops.Value <- drops.Value + 1

    let row (r: Row) =
        // Per-row state, to prove a drop moves the element rather than rebuilding.
        let touched = Var.create 0

        Html.li
            [
                attr.className "drag-row"
                attr.classList (fun () -> [ "is-dragging", dragging.Value = Some r.Id ])
                attr.classList (fun () -> [ "is-over", over.Value = Some r.Id ])
                attr.custom ("draggable", "true")

                on.dragStart (fun _ -> dragging.Value <- Some r.Id)
                on.dragEnd (fun _ ->
                    dragging.Value <- None
                    over.Value <- None
                )

                on.dragOver (fun e ->
                    // Without this the drop is refused by the browser.
                    e.preventDefault ()
                    over.Value <- Some r.Id
                )

                on.drop (fun e ->
                    e.preventDefault ()
                    dragging.Value |> Option.iter (fun from -> move from r.Id)
                    dragging.Value <- None
                    over.Value <- None
                )

                Html.span
                    [
                        attr.className "drag-handle"
                        Html.text "::"
                    ]
                Html.span r.Label

                Button.action ("touch", fun _ -> touched.Value <- touched.Value + 1)

                readout "touched" (fun () -> string touched.Value)
            ]

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Press \`touch\` on a row a few times, then drag it by its handle. The count moves with it, because the row is moved, not rebuilt."

            // demo-show
            Row.row
                [
                    // demo-hide
                    readout "drops" (fun () -> string drops.Value)

                    readout
                        "order"
                        (fun () ->
                            rows.Value |> Array.map (fun r -> string r.Id) |> String.concat " "
                        )

                    // demo-show

                    Button.action (
                        "Sort back by id",
                        fun _ -> rows.Value <- rows.Value |> Array.sortBy (fun r -> r.Id)
                    )
                ]

            Html.ul
                [
                    attr.className "drag-list"
                    Html.each (fun () -> rows.Value) (fun r -> r.Id) row
                ]

        ]
`;export{e as default};