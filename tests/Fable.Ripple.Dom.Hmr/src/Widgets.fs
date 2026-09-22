module Widgets

open Fable.Ripple
open Fable.Ripple.Dom

/// The shape the question is about: a component taking a label and a handler.
[<Component>]
let button (label: string) (onClick: unit -> unit) : DomItem =
    Html.button
        [
            attr.className "btn"
            on.click (fun _ -> onClick ())
            Html.text ("v1:" + label)
        ]
