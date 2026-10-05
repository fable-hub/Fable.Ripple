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
                    "Rows",
                    [
                        testComponent (
                            "renders one clone per item with the row's values",
                            "RowsList",
                            fun root ->
                                promise {
                                    do! assertLocator (root.locator "li") (haveCount 3)

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(2) span")
                                            (haveText "item 2")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(2)")
                                            (haveAttribute "data-id" "2")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(3)")
                                            (haveAttribute "data-id" "3")
                                }
                        )

                        testComponent (
                            "nested show and each are built per row at their position",
                            "RowsList",
                            fun root ->
                                promise {
                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(2) em")
                                            (haveText "second")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) em")
                                            (haveCount 0)

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) i")
                                            (haveCount 2)

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) i:nth-of-type(2) + u + b")
                                            (haveText "end")

                                    do! click (root.locator "li:nth-child(1) button.tag")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) i")
                                            (haveCount 3)

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(2) i")
                                            (haveCount 2)
                                }
                        )

                        testComponent (
                            "a fragment in a row is recorded once, with its bindings",
                            "RowsList",
                            fun root ->
                                promise {
                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) u")
                                            (haveCount 1)

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) u")
                                            (haveText "frag1")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(3)")
                                            (haveAttribute "data-frag" "3")
                                }
                        )

                        testComponent (
                            "a handler inside a nested list still sees the outer row",
                            "RowsList",
                            fun root ->
                                promise {
                                    do! click (root.locator "li:nth-child(2) i:nth-of-type(1)")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(2) span")
                                            (haveText "a")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) span")
                                            (haveText "item 1")
                                }
                        )

                        testComponent (
                            "a reactive text follows the row's own signal",
                            "RowsList",
                            fun root ->
                                promise {
                                    do! click (root.locator "li:nth-child(1) button.rename")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) span")
                                            (haveText "item 1!")

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(2) span")
                                            (haveText "item 2")
                                }
                        )

                        testComponent (
                            "a class toggle follows a shared signal",
                            "RowsList",
                            fun root ->
                                promise {
                                    do! click (root.locator "li:nth-child(3) button.select")
                                    do! assertLocator (root.locator "li.selected") (haveCount 1)

                                    do!
                                        assertLocator
                                            (root.locator "li.selected span")
                                            (haveText "item 3")

                                    do! click (root.locator "li:nth-child(1) button.select")

                                    do!
                                        assertLocator
                                            (root.locator "li.selected span")
                                            (haveText "item 1")
                                }
                        )

                        testComponent (
                            "rows are keyed: a removed item's element leaves, the others stay",
                            "RowsList",
                            fun root ->
                                promise {
                                    do! click (root.locator "#drop-first")
                                    do! assertLocator (root.locator "li") (haveCount 2)

                                    do!
                                        assertLocator
                                            (root.locator "li:nth-child(1) span")
                                            (haveText "item 2")
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
