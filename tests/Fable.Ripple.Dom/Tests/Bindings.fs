module Fable.Ripple.Dom.Test.Tests.Bindings

open Scriptorium.Nib.Browser

open type Scriptorium.Nib.Browser.UserEvents
open type Fable.Ripple.Dom.Test.RippleDomTest
open type Scriptorium.Quill.Test

let tests =
    testList (
        "Bindings and routing",
        [
            testComponent (
                "bindValue writes the Var as the user types",
                "BindValue",
                fun root ->
                    promise {
                        do! assertLocator (root.locator "#field") (haveValue "start")
                        do! fill (root.locator "#field", "typed")
                        do! assertLocator (root.locator "#echo") (haveText "typed")
                    }
            )

            testComponent (
                "bindValue updates the field when the Var is written",
                "BindValue",
                fun root ->
                    promise {
                        do! fill (root.locator "#field", "typed")
                        do! click (root.locator "#reset")
                        do! assertLocator (root.locator "#field") (haveValue "reset")
                    }
            )

            testComponent (
                "bindChecked updates the checkbox when the Var is written",
                "BindCheckedFromCode",
                fun root ->
                    promise {
                        do! click (root.locator "#check")
                        do! assertLocator (root.locator "#box") beChecked
                    }
            )

            testComponent (
                "HashRouter follows a link to a hash",
                "HashRouter",
                fun root ->
                    promise {
                        do! assertLocator (root.locator "#route") (haveText "home")
                        do! click (root.locator "#link")
                        do! assertLocator (root.locator "#route") (haveText "about")
                    }
            )

            testComponent (
                "HashRouter.NewUrl navigates to a route",
                "HashRouter",
                fun root ->
                    promise {
                        do! click (root.locator "#link")
                        do! click (root.locator "#home")
                        do! assertLocator (root.locator "#route") (haveText "home")
                    }
            )
        ]
    )
