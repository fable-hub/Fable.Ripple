module Fable.Ripple.Dom.Test.Tests.ControlFlow

open Scriptorium.Nib.Browser

open type Scriptorium.Nib.Browser.UserEvents
open type Fable.Ripple.Dom.Test.RippleDomTest
open type Scriptorium.Quill.Test

let tests =
    testList (
        "Control flow",
        [
            testList (
                "Html.show",
                [
                    testComponent (
                        "builds the branch when the condition holds",
                        "Show",
                        fun root ->
                            promise {
                                do! assertLocator (root.locator "#panel") (haveCount 0)
                                do! assertLocator (root.locator "#builds") (haveText "0")
                                do! click (root.locator "#toggle")
                                do! assertLocator (root.locator "#panel") toBeVisible
                                do! assertLocator (root.locator "#builds") (haveText "1")
                            }
                    )

                    testComponent (
                        "disposes the branch when it leaves and rebuilds it fresh",
                        "Show",
                        fun root ->
                            promise {
                                let toggle = root.locator "#toggle"
                                do! click toggle
                                do! fill (root.locator "#draft", "typed")
                                do! click toggle
                                do! assertLocator (root.locator "#panel") (haveCount 0)
                                do! assertLocator (root.locator "#cleanups") (haveText "1")
                                do! click toggle
                                do! assertLocator (root.locator "#draft") (haveValue "")
                                do! assertLocator (root.locator "#builds") (haveText "2")
                            }
                    )

                    testComponent (
                        "renders the fallback while the condition is false",
                        "ShowFallback",
                        fun root ->
                            promise {
                                do! assertLocator (root.locator "#off") (haveText "off")
                                do! click (root.locator "#toggle")
                                do! assertLocator (root.locator "#on") (haveText "on")
                                do! assertLocator (root.locator "#off") (haveCount 0)
                            }
                    )

                    testComponent (
                        "stops effects created in a branch when it leaves",
                        "EffectDisposal",
                        fun root ->
                            promise {
                                let runs = root.locator "#runs"
                                do! assertLocator runs (haveText "1")
                                do! click (root.locator "#bump")
                                do! assertLocator runs (haveText "2")
                                do! click (root.locator "#hide")
                                do! click (root.locator "#bump")
                                do! assertLocator runs (haveText "2")
                            }
                    )
                ]
            )

            testList (
                "Html.switch",
                [
                    testComponent (
                        "rebuilds when the value changes",
                        "Switch",
                        fun root ->
                            promise {
                                do! assertLocator (root.locator "#arm") (haveText "idle")
                                do! assertLocator (root.locator "#builds") (haveText "1")
                                do! click (root.locator "#loading-40")
                                do! assertLocator (root.locator "#arm") (haveText "loading 40")
                                do! click (root.locator "#loading-50")
                                do! assertLocator (root.locator "#arm") (haveText "loading 50")
                                do! assertLocator (root.locator "#builds") (haveText "3")
                            }
                    )

                    testComponent (
                        "writing an equal value rebuilds nothing",
                        "Switch",
                        fun root ->
                            promise {
                                let loading = root.locator "#loading-40"
                                do! click loading
                                do! click loading
                                do! assertLocator (root.locator "#builds") (haveText "2")
                            }
                    )

                    testComponent (
                        "a signal read in the branch does not rebuild it",
                        "Switch",
                        fun root ->
                            promise {
                                do! click (root.locator "#loading-40")
                                do! click (root.locator "#bump-other")
                                do! assertLocator (root.locator "#live") (haveText "1")
                                do! assertLocator (root.locator "#snapshot") (haveText "0")
                                do! assertLocator (root.locator "#builds") (haveText "2")
                            }
                    )

                    testComponent (
                        "switchWith rebuilds only when the computed value changes",
                        "SwitchWith",
                        fun root ->
                            promise {
                                let both = root.locator "#both"
                                let builds = root.locator "#builds"
                                do! assertLocator both (haveText "not both")
                                do! click (root.locator "#toggle-a")
                                do! assertLocator builds (haveText "1")
                                do! click (root.locator "#toggle-b")
                                do! assertLocator both (haveText "both")
                                do! assertLocator builds (haveText "2")
                            }
                    )
                ]
            )

            testComponent (
                "Html.dynamic rebuilds on every signal its body reads",
                "Dynamic",
                fun root ->
                    promise {
                        do! assertLocator (root.locator "#builds") (haveText "1")
                        do! click (root.locator "#bump-a")
                        do! click (root.locator "#bump-b")
                        do! assertLocator (root.locator "#sum") (haveText "2")
                        do! assertLocator (root.locator "#builds") (haveText "3")
                    }
            )
        ]
    )
