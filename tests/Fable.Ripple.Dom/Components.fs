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

register ("HelloWorld", helloWorld)
register ("Counter", counter)
register ("Checkbox", checkbox)

for name, view in Rendering.all @ ControlFlow.all @ Lists.all @ Bindings.all do
    register (name, view)
