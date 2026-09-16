module Demo.Examples.Rendering.Classes

open Browser.Types
open Browser
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// Four ways to put classes on an element, and the point is that they COMPOSE.
//
//   attr.className "card"                  sets the class attribute
//   attr.classes [ "card"; "wide" ]        the same, joined for you
//   attr.classList (fun () -> [ n, b ])    toggles tokens by a flag
//   attr.toggleClass ("on", someSignal)    one token, driven by a signal
//
// `className` and `classes` OWN the attribute - they write it whole. The other
// two go through the DOM `classList` API and diff individual tokens, so they add
// and remove only what changed and leave everything else alone.

let render () =
    let active = Var.create false
    let danger = Var.create false
    let live = Var.create ""

    // Read the real attribute back, so the readout cannot be politely wrong.
    let mutable el: Element option = None

    // Deferred by one macrotask, and it has to be. The `classList` bindings are
    // effects on the same signals as this one, and nothing orders this after
    // them - reading the attribute during the flush reports the PREVIOUS class,
    // which for a readout whose whole job is to quote the real attribute is the
    // worst kind of wrong: quietly plausible.
    let refresh () =
        window.setTimeout ((fun () -> el |> Option.iter (fun e -> live.Value <- e.className)), 0)
        |> ignore

    // Watches the flags rather than being called from a handler, so it covers a
    // checkbox, a keyboard toggle and a programmatic write alike.
    Signal.effect (fun () ->
        active.Value |> ignore
        danger.Value |> ignore
        refresh ()
    )
    |> ignore

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Tick either box. One class appears or disappears and the others stay. The line underneath is the element's real `class` attribute."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    // Checkboxes rather than buttons: the thing being toggled is
                    // a boolean, so the control should show its state instead of
                    // making you read it off the chip below.
                    Html.label
                        [
                            Html.input
                                [
                                    attr.type' "checkbox"
                                    attr.bindChecked active
                                ]

                            Html.text ".is-active"
                        ]

                    Html.label
                        [
                            Html.input
                                [
                                    attr.type' "checkbox"
                                    attr.bindChecked danger
                                ]

                            Html.text ".is-danger"
                        ]
                ]

            Html.div
                [
                    // A static base, then two independent reactive sources. All
                    // three survive each other because the last two diff tokens.
                    attr.classes
                        [
                            "chip"
                            "chip-demo"
                        ]

                    attr.classList (fun () -> [ "is-active", active.Value ])
                    attr.toggleClass ("is-danger", danger)

                    attr.ref (fun e -> el <- Some(e :> Element))

                    Html.text "class composition"
                ]

            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "live class attribute"
                            Html.output live
                        ]
                ]

        ]
