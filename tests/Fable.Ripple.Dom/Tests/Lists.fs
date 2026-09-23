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
                        do! assertLocator (root.locator "#host") (haveText "BEFOREAFTERfillclear")
                        do! click (root.locator "#fill")
                        do! assertLocator (root.locator "#host") (haveText "BEFOREabAFTERfillclear")
                    }
            )

            testComponent (
                "clearing a list that owns its parent runs every cleanup and keeps its place",
                "KeyedList",
                fun root ->
                    promise {
                        do! click (root.locator "#clear")
                        do! assertLocator (root.locator "#list > li") (haveCount 0)
                        do! assertLocator (root.locator "#cleanups") (haveText "3")

                        // Rows added after the clear still land inside the list.
                        do! click (root.locator "#append")
                        do! assertLocator (root.locator "#list > li") (haveCount 1)

                        do!
                            assertLocator
                                (root.locator "#list > li:nth-child(1)")
                                (haveAttribute "data-id" "4")

                        // Full replace (disjoint keys) takes the same path.
                        do! click (root.locator "#replace")
                        do! assertLocator (root.locator "#list > li") (haveCount 2)
                        do! assertLocator (root.locator "#cleanups") (haveText "4")
                        do! click (root.locator "#append")

                        do!
                            assertLocator
                                (root.locator "#list > li:nth-child(3)")
                                (haveAttribute "data-id" "7")
                    }
            )

            testComponent (
                "clearing a list between siblings leaves the siblings in place",
                "EachPosition",
                fun root ->
                    promise {
                        do! click (root.locator "#fill")
                        do! click (root.locator "#clear")
                        do! assertLocator (root.locator "#host") (haveText "BEFOREAFTERfillclear")
                        do! click (root.locator "#fill")
                        do! assertLocator (root.locator "#host") (haveText "BEFOREabAFTERfillclear")
                    }
            )

        ]
    )
