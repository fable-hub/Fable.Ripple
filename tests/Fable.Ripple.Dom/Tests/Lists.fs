module Fable.Ripple.Dom.Test.Tests.Lists

open Scriptorium.Nib.Browser

open type Scriptorium.Nib.Browser.UserEvents
open type Fable.Ripple.Dom.Test.RippleDomTest
open type Scriptorium.Quill.Test

let tests =
    testList (
        "Html.each",
        [
            testComponent (
                "renders one element per item, in order",
                "KeyedList",
                fun root ->
                    promise {
                        do! assertLocator (root.locator "#list > li") (haveCount 3)

                        do!
                            assertLocator
                                (root.locator "#list > li:nth-child(1)")
                                (haveAttribute "data-id" "1")

                        do!
                            assertLocator
                                (root.locator "#list > li:nth-child(3)")
                                (haveAttribute "data-id" "3")
                    }
            )

            testComponent (
                "reordering moves the existing elements",
                "KeyedList",
                fun root ->
                    promise {
                        do! fill (root.locator "#list > li[data-id='2'] input", "kept")
                        do! click (root.locator "#reverse")

                        do!
                            assertLocator
                                (root.locator "#list > li:nth-child(1)")
                                (haveAttribute "data-id" "3")

                        do!
                            assertLocator
                                (root.locator "#list > li[data-id='2'] input")
                                (haveValue "kept")

                        do! assertLocator (root.locator "#builds") (haveText "3")
                    }
            )

            testComponent (
                "appending builds only the new item",
                "KeyedList",
                fun root ->
                    promise {
                        do! click (root.locator "#append")
                        do! assertLocator (root.locator "#list > li") (haveCount 4)
                        do! assertLocator (root.locator "#builds") (haveText "4")
                    }
            )

            testComponent (
                "removing an item runs its cleanup once",
                "KeyedList",
                fun root ->
                    promise {
                        do! click (root.locator "#remove-first")
                        do! assertLocator (root.locator "#list > li") (haveCount 2)
                        do! assertLocator (root.locator "#list > li[data-id='1']") (haveCount 0)
                        do! assertLocator (root.locator "#cleanups") (haveText "1")
                        do! assertLocator (root.locator "#builds") (haveText "3")
                    }
            )
            testComponent (
                "each that starts empty inserts later rows at its own position",
                "EachPosition",
                fun root ->
                    promise {
                        do! assertLocator (root.locator "#host") (haveText "BEFOREAFTERfill")
                        do! click (root.locator "#fill")
                        do! assertLocator (root.locator "#host") (haveText "BEFOREabAFTERfill")
                    }
            )

        ]
    )
