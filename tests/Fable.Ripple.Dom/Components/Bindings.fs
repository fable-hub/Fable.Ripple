module Fable.Ripple.Dom.Test.Components.Bindings

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Dom.Routing

let private bindValue () : DomItem =
    let text = Var.create "start"

    Html.div
        [
            Html.input
                [
                    attr.id "field"
                    attr.bindValue text
                ]
            Html.output
                [
                    attr.id "echo"
                    Html.text text
                ]
            Html.button
                [
                    attr.id "reset"
                    on.click (fun _ -> text.Value <- "reset")
                    Html.text "Reset"
                ]
        ]

let private bindCheckedFromCode () : DomItem =
    let isChecked = Var.create false

    Html.div
        [
            Html.input
                [
                    attr.id "box"
                    attr.type' "checkbox"
                    attr.bindChecked isChecked
                ]
            Html.button
                [
                    attr.id "check"
                    on.click (fun _ -> isChecked.Value <- true)
                    Html.text "Check"
                ]
        ]

type private Route =
    | Home
    | About

let private hashRouter () : DomItem =
    let parse =
        function
        | ""
        | "#/" -> Some Home
        | "#/about" -> Some About
        | _ -> None

    let toUrl =
        function
        | Home -> "#/"
        | About -> "#/about"

    let router = new HashRouter<Route>(parse, toUrl)

    Html.div
        [
            Html.a
                [
                    attr.id "link"
                    attr.href (router.Href About)
                    Html.text "About"
                ]
            Html.button
                [
                    attr.id "home"
                    on.click (fun _ -> router.NewUrl Home)
                    Html.text "Home"
                ]
            Html.output
                [
                    attr.id "route"
                    Html.text (fun () ->
                        match router.CurrentRoute.Value with
                        | Some Home -> "home"
                        | Some About -> "about"
                        | None -> "none"
                    )
                ]
        ]

let all: (string * (unit -> DomItem)) list =
    [
        "BindValue", bindValue
        "BindCheckedFromCode", bindCheckedFromCode
        "HashRouter", hashRouter
    ]
