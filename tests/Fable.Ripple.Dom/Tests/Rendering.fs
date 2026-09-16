module Fable.Ripple.Dom.Test.Tests.Rendering

open Scriptorium.Nib.Browser

open type Scriptorium.Nib.Browser.UserEvents
open type Fable.Ripple.Dom.Test.RippleDomTest
open type Scriptorium.Quill.Test

let tests =
    testList (
        "Rendering",
        [
            testComponent (
                "text follows a thunk and a signal",
                "ReactiveText",
                fun root ->
                    promise {
                        do! assertLocator (root.locator "#thunk") (haveText "Hello Ada")
                        do! assertLocator (root.locator "#signal") (haveText "Ada")
                        do! click (root.locator "#rename")
                        do! assertLocator (root.locator "#thunk") (haveText "Hello Grace")
                        do! assertLocator (root.locator "#signal") (haveText "Grace")
                    }
            )

            testComponent (
                "attributes follow thunks and signals, literals stay",
                "ReactiveAttributes",
                fun root ->
                    promise {
                        let target = root.locator "#target"

                        do!
                            assertLocator
                                target
                                (haveAttribute "data-state" "off"
                                 >>. haveAttribute "aria-label" "disabled"
                                 >>. haveCSS "color" "rgb(0, 0, 255)"
                                 >>. beEnabled)

                        do! click (root.locator "#toggle")

                        do!
                            assertLocator
                                target
                                (haveAttribute "data-state" "on"
                                 >>. haveAttribute "aria-label" "enabled"
                                 >>. haveCSS "color" "rgb(255, 0, 0)"
                                 >>. beDisabled
                                 >>. haveAttribute "title" "literal")
                    }
            )

            testComponent (
                "classList toggles one token and keeps the base class",
                "ClassList",
                fun root ->
                    promise {
                        let chip = root.locator "#chip"
                        do! assertLocator chip (haveClass "chip")
                        do! click (root.locator "#active")
                        do! assertLocator chip (haveClass "chip is-active")
                        do! click (root.locator "#danger")
                        do! assertLocator chip (containClass "chip is-active is-danger")
                        do! click (root.locator "#active")
                        do! assertLocator chip (haveClass "chip is-danger")
                    }
            )

            testComponent (
                "an event handler flushes its writes once",
                "EventBatching",
                fun root ->
                    promise {
                        let runs = root.locator "#runs"
                        do! assertLocator runs (haveText "1")
                        do! click (root.locator "#handler")
                        do! assertLocator runs (haveText "2")
                    }
            )

            testComponent (
                "writes outside a handler flush one by one",
                "EventBatching",
                fun root ->
                    promise {
                        do! click (root.locator "#timeout")
                        do! assertLocator (root.locator "#runs") (haveText "3")
                    }
            )

            testComponent (
                "a fragment splices its children and Html.none renders nothing",
                "Fragment",
                fun root ->
                    promise {
                        do! assertLocator (root.locator "#parent > *") (haveCount 3)
                        do! assertLocator (root.locator "#parent") (haveText "abc")
                    }
            )

            testComponent (
                "Html.node adopts a node built outside the library",
                "ForeignNode",
                fun root ->
                    promise { do! assertLocator (root.locator "#host > em") (haveText "adopted") }
            )

            testComponent (
                "attr.ref receives the element",
                "Ref",
                fun root ->
                    promise {
                        do! assertLocator (root.locator "#field") (haveAttribute "data-ref" "input")
                    }
            )

            testComponent (
                "Svg elements are created in the SVG namespace",
                "Svg",
                fun root ->
                    promise {
                        do!
                            assertLocator
                                (root.locator "#ns")
                                (haveText "http://www.w3.org/2000/svg")
                    }
            )
        ]
    )
