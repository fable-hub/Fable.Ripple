module Table

open Fable.Ripple
open Fable.Ripple.Dom

type Row =
    {
        Id: int
        Label: string
    }

[<Component>]
let view (count: int) : DomItem =
    let rows =
        Var.create
            [|
                for i in 1..count ->
                    {
                        Id = i
                        Label = Shared.greeting + " " + string i
                    }
            |]

    let selected = Var.create 0

    Html.table
        [
            attr.id "table"
            Html.tbody
                [
                    Html.each
                        (fun () -> rows.Value)
                        (fun r -> r.Id)
                        (fun r ->
                            // Reactive per row, so each row contributes real nodes to
                            // the graph - a static row contributes none.
                            Html.tr
                                [
                                    attr.classList (fun () -> [ "sel", selected.Value = r.Id ])
                                    Html.td [ Html.text (fun () -> string r.Id) ]
                                    Html.td [ Html.text (fun () -> r.Label) ]
                                    Html.td
                                        [
                                            on.click (fun _ -> selected.Value <- r.Id)
                                            Html.text "x"
                                        ]
                                ]
                        )
                ]
        ]
