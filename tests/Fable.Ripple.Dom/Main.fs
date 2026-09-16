module Fable.Ripple.Dom.Test.Main

open Scriptorium.Nib.Browser
open Fable.Ripple.Dom.Test.Tests

open type Scriptorium.Nib.Browser.UserEvents
open type Fable.Ripple.Dom.Test.RippleDomTest
open type Scriptorium.Quill.Runner
open type Scriptorium.Quill.Test

do setup "Components.fs"

(*
    Tests - each mounts a registered component in a fresh Playwright page and
    asserts against the real, rendered DOM via Scriptorium.Nib.Browser locators.
*)

[<EntryPoint>]
let main _ =

    let tests =
        testList (
            "Fable.Ripple.Dom",
            [

                testList (
                    "HelloWorld",
                    [
                        testComponent (
                            "renders Hello World",
                            "HelloWorld",
                            fun root -> promise { do! assertLocator root (haveText "Hello World") }
                        )
                    ]
                )

                testList (
                    "Counter",
                    [
                        testComponent (
                            "starts at 0",
                            "Counter",
                            fun root ->
                                promise {
                                    do! assertLocator (root.locator "div > div") (haveText "0")
                                }
                        )

                        testComponent (
                            "increments when Count is clicked",
                            "Counter",
                            fun root ->
                                promise {
                                    do! click (root.locator "button")
                                    do! assertLocator (root.locator "div > div") (haveText "1")
                                }
                        )

                        testComponent (
                            "can increment multiple times",
                            "Counter",
                            fun root ->
                                promise {
                                    let button = root.locator "button"
                                    do! click button
                                    do! click button
                                    do! click button
                                    do! assertLocator (root.locator "div > div") (haveText "3")
                                }
                        )
                    ]
                )

                testList (
                    "Checkbox",
                    [
                        testComponent (
                            "check sets the label to checked",
                            "Checkbox",
                            fun root ->
                                promise {
                                    do! check (root.locator "input[type=checkbox]")

                                    do!
                                        assertLocator
                                            (root.locator "div > div")
                                            (haveText "checked")
                                }
                        )

                        testComponent (
                            "toggles back to unchecked",
                            "Checkbox",
                            fun root ->
                                promise {
                                    let checkbox = root.locator "input[type=checkbox]"
                                    do! check checkbox
                                    do! uncheck checkbox

                                    do!
                                        assertLocator
                                            (root.locator "div > div")
                                            (haveText "unchecked")
                                }
                        )
                    ]
                )

                Rendering.tests
                ControlFlow.tests
                Lists.tests
                Bindings.tests
            ]
        )

    runTests tests
