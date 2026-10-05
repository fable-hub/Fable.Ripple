module Fable.Ripple.Dom.Test.Components

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Dom.Test.Components
open type Fable.Ripple.Dom.Test.RippleRegistry

(*
    Register components for Playwright testing. Each is a `unit -> DomItem` factory
*)

let private helloWorld () : DomItem = Html.div [ Html.text "Hello World" ]

let private counter () : DomItem =
    let count = Var.create 0

    Html.div
        [
            Html.div [ Html.text count ]
            Html.button
                [
                    on.click (fun _ -> count.Value <- count.Value + 1)
                    Html.text "Count"
                ]
        ]

let private checkbox () : DomItem =
    let isChecked = Var.create false

    Html.div
        [
            Html.input
                [
                    attr.type' "checkbox"
                    attr.bindChecked isChecked
                ]
            Html.div
                [
                    Html.text (fun () ->
                        if isChecked.Value then
                            "checked"
                        else
                            "unchecked"
                    )
                ]
        ]

type private RowsItem =
    {
        Id: int
        Label: Var<string>
        Tags: Var<string[]>
    }

let private rowsList () : DomItem =
    let items =
        Var.create
            [|
                for i in 1..3 ->
                    {
                        Id = i
                        Label = Var.create ("item " + string i)
                        Tags =
                            Var.create
                                [|
                                    "a"
                                    "b"
                                |]
                    }
            |]

    let selected = Var.create 0

    Html.div
        [
            Html.ul
                [
                    Html.template (
                        items,
                        (fun item -> item.Id),
                        (fun item ->
                            Html.li
                                [
                                    attr.toggleClass (
                                        "selected",
                                        fun () -> selected.Value = item.Value.Id
                                    )
                                    attr.custom ("data-id", fun () -> string item.Value.Id)
                                    Html.text (fun () -> string item.Value.Id + ": ")
                                    Html.span [ Html.text (fun () -> item.Value.Label.Value) ]
                                    Html.button
                                        [
                                            attr.className "select"
                                            on.click (fun _ -> selected.Value <- item.Value.Id)
                                            Html.text "select"
                                        ]
                                    Html.button
                                        [
                                            attr.className "rename"
                                            on.click (fun _ ->
                                                item.Value.Label.Value <-
                                                    item.Value.Label.Value + "!"
                                            )
                                            Html.text "rename"
                                        ]
                                    Html.button
                                        [
                                            attr.className "tag"
                                            on.click (fun _ ->
                                                item.Value.Tags.Value <-
                                                    Array.append item.Value.Tags.Value [| "c" |]
                                            )
                                            Html.text "tag"
                                        ]
                                    Html.show (
                                        (fun () -> item.Value.Id = 2),
                                        (fun () -> Html.em [ Html.text "second" ])
                                    )
                                    Html.each (
                                        (fun () -> item.Value.Tags.Value),
                                        id,
                                        fun tag ->
                                            Html.i
                                                [
                                                    on.click (fun _ ->
                                                        item.Value.Label.Value <- tag
                                                    )
                                                    Html.text tag
                                                ]
                                    )
                                    Html.fragment
                                        [
                                            Html.u
                                                [
                                                    Html.text (fun () ->
                                                        "frag" + string item.Value.Id
                                                    )
                                                ]
                                            attr.custom (
                                                "data-frag",
                                                fun () -> string item.Value.Id
                                            )
                                        ]
                                    Html.b [ Html.text "end" ]
                                ]
                        )
                    )
                ]
            Html.button
                [
                    attr.id "drop-first"
                    on.click (fun _ -> items.Value <- items.Value.[1..])
                    Html.text "drop first"
                ]
        ]

register ("HelloWorld", helloWorld)
register ("RowsList", rowsList)
register ("Counter", counter)
register ("Checkbox", checkbox)

for name, view in Rendering.all @ ControlFlow.all @ Lists.all @ Bindings.all do
    register (name, view)
