module Fable.Ripple.Form.Tests.Main

open Glutinum.Playwright
open Scriptorium.Nib.Browser

open type Scriptorium.Nib.Browser.UserEvents
open type Fable.Ripple.Dom.Test.RippleDomTest
open type Scriptorium.Quill.Runner
open type Scriptorium.Quill.Test

do setup "Components.fs"

let private help (root: Locator) = root.locator ".rf-help"
let private result (root: Locator) = root.locator "#result"

[<EntryPoint>]
let main _ =
    runTests (
        testList (
            "Fable.Ripple.Form",
            [
                testList (
                    "Field",
                    [
                        testComponent (
                            "parses as the user types",
                            "SubmitForm",
                            fun root ->
                                promise {
                                    do! assertLocator (result root) (haveText "error:1")
                                    do! fill (root.locator "input", "Ada")
                                    do! assertLocator (result root) (haveText "ok:Ada")
                                }
                        )

                        testComponent (
                            "hides errors until submit, then shows them",
                            "SubmitForm",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "A")
                                    do! blur (root.locator "input")
                                    do! assertLocator (help root) (haveText "")
                                    do! click (root.locator "button[type=submit]")
                                    do! assertLocator (help root) (haveText "Too short")

                                    do!
                                        assertLocator
                                            (root.locator "input")
                                            (containClass "rf-input--invalid")

                                    do! assertLocator (root.locator "#submitted") (haveText "")
                                }
                        )

                        testComponent (
                            "submits the parsed output",
                            "SubmitForm",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "Ada")
                                    do! click (root.locator "button[type=submit]")
                                    do! assertLocator (root.locator "#submitted") (haveText "Ada")
                                }
                        )

                        testComponent (
                            "follows a Var written from code",
                            "SubmitForm",
                            fun root ->
                                promise {
                                    do! click (root.locator "#set")
                                    do! assertLocator (root.locator "input") (haveValue "Ada")
                                    do! assertLocator (result root) (haveText "ok:Ada")
                                }
                        )

                        testComponent (
                            "resetting the error visibility hides the errors again",
                            "SubmitForm",
                            fun root ->
                                promise {
                                    do! click (root.locator "button[type=submit]")

                                    do!
                                        assertLocator
                                            (help root)
                                            (haveText "This field is required")

                                    do! click (root.locator "#reset-errors")
                                    do! assertLocator (help root) (haveText "")
                                }
                        )

                        testComponent (
                            "Loading disables the field and the submit button spins",
                            "SubmitForm",
                            fun root ->
                                promise {
                                    do! click (root.locator "#load")
                                    do! assertLocator (root.locator "input") beDisabled

                                    do!
                                        assertLocator
                                            (root.locator "button[type=submit]")
                                            (containClass "rf-button--loading")
                                }
                        )

                        testComponent (
                            "ValidateOnBlur shows the error after the field loses focus",
                            "BlurForm",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "A")
                                    do! assertLocator (help root) (haveText "")
                                    do! blur (root.locator "input")
                                    do! assertLocator (help root) (haveText "Too short")
                                    do! fill (root.locator "input", "Ada")
                                    do! assertLocator (help root) (haveText "")
                                }
                        )
                    ]
                )

                testList (
                    "Field kinds",
                    [
                        testComponent (
                            "email, number, textarea, password and checkbox round-trip through their Vars",
                            "Kinds",
                            fun root ->
                                promise {
                                    do! assertLocator (result root) (haveText "error:1")
                                    do! fill (root.locator "#summary", "notes")

                                    do!
                                        assertLocator
                                            (result root)
                                            (haveText "ok:a@b.c 1 notes pw false")

                                    do!
                                        assertLocator
                                            (root.locator "#secret")
                                            (haveAttribute "type" "password")

                                    do! fill (root.locator "#email", "x@y.z")
                                    do! fill (root.locator "#number", "42")
                                    do! fill (root.locator "#secret", "hidden")
                                    do! check (root.locator "#agreed")

                                    do!
                                        assertLocator
                                            (result root)
                                            (haveText "ok:x@y.z 42 notes hidden true")

                                    do! fill (root.locator "#number", "")
                                    do! assertLocator (result root) (haveText "error:1")

                                    do!
                                        assertLocator
                                            (root.locator "#number")
                                            (haveAttribute "aria-invalid" "false")
                                }
                        )
                    ]
                )

                testList (
                    "Accessibility",
                    [
                        testComponent (
                            "links the label, the control and the help text, and focuses the first invalid field on submit",
                            "SubmitForm",
                            fun root ->
                                promise {
                                    do!
                                        assertLocator
                                            (root.locator "label")
                                            (haveAttribute "for" "name")

                                    do!
                                        assertLocator
                                            (root.locator "input")
                                            (haveAttribute "id" "name")

                                    do!
                                        assertLocator
                                            (root.locator "input")
                                            (haveAttribute "aria-describedby" "name-help")

                                    do! assertLocator (help root) (haveAttribute "id" "name-help")

                                    do!
                                        assertLocator
                                            (root.locator "input")
                                            (haveAttribute "aria-invalid" "false")

                                    do! click (root.locator "button[type=submit]")

                                    do!
                                        assertLocator
                                            (root.locator "input")
                                            (haveAttribute "aria-invalid" "true")

                                    do! assertLocator (root.locator "input") beFocused
                                }
                        )
                    ]
                )

                testList (
                    "State",
                    [
                        testComponent (
                            "isDirty, reset and commit",
                            "Dirty",
                            fun root ->
                                promise {
                                    do! assertLocator (root.locator "#dirty") (haveText "clean")
                                    do! fill (root.locator "input", "Grace")
                                    do! assertLocator (root.locator "#dirty") (haveText "dirty")
                                    do! click (root.locator "#reset")
                                    do! assertLocator (root.locator "input") (haveValue "Ada")
                                    do! assertLocator (root.locator "#dirty") (haveText "clean")
                                    do! fill (root.locator "input", "Grace")
                                    do! click (root.locator "#commit")
                                    do! assertLocator (root.locator "#dirty") (haveText "clean")
                                    do! fill (root.locator "input", "Ada")
                                    do! assertLocator (root.locator "#dirty") (haveText "dirty")
                                    do! click (root.locator "#reset")
                                    do! assertLocator (root.locator "input") (haveValue "Grace")
                                }
                        )

                        testComponent (
                            "load writes the record, commits, and snapshot follows the fields",
                            "Dirty",
                            fun root ->
                                promise {
                                    do! assertLocator (root.locator "#snapshot") (haveText "Ada")
                                    do! click (root.locator "#load-draft")
                                    do! assertLocator (root.locator "input") (haveValue "Grace")
                                    do! assertLocator (root.locator "#snapshot") (haveText "Grace")
                                    do! assertLocator (root.locator "#dirty") (haveText "clean")
                                    do! fill (root.locator "input", "Ada")
                                    do! assertLocator (root.locator "#snapshot") (haveText "Ada")
                                    do! assertLocator (root.locator "#dirty") (haveText "dirty")
                                }
                        )

                        testComponent (
                            "validateOn ValidateOnChange shows the error while typing",
                            "ValidateOnChange",
                            fun root ->
                                promise {
                                    do! assertLocator (help root) (haveText "")
                                    do! fill (root.locator "input", "A")
                                    do! assertLocator (help root) (haveText "Too short")
                                    do! fill (root.locator "input", "Ada")
                                    do! assertLocator (help root) (haveText "")
                                }
                        )

                        testComponent (
                            "Field.create withExternalError and",
                            "ExternalCombinator",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "ada")
                                    do! assertLocator (result root) (haveText "ok:ada")
                                    do! click (root.locator "#fail")
                                    do! assertLocator (help root) (haveText "Taken")
                                    do! assertLocator (result root) (haveText "error:1")
                                    do! fill (root.locator "input", "ada2")
                                    do! assertLocator (result root) (haveText "ok:ada2")
                                }
                        )
                    ]
                )

                testList (
                    "Cross-field",
                    [
                        testComponent (
                            "a parser reading another Var re-runs when it changes",
                            "RepeatPassword",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input >> nth=0", "secret")
                                    do! fill (root.locator "input >> nth=1", "secret")
                                    do! assertLocator (result root) (haveText "ok:secret")
                                    do! fill (root.locator "input >> nth=0", "changed")
                                    do! assertLocator (result root) (haveText "error:1")
                                    do! click (root.locator "button[type=submit]")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-help >> nth=1")
                                            (haveText "The passwords do not match")
                                }
                        )

                        testComponent (
                            "disableIf follows the signal it reads",
                            "Disable",
                            fun root ->
                                promise {
                                    do! assertLocator (root.locator "input[type=text]") beEnabled
                                    do! check (root.locator "input[type=checkbox]")
                                    do! assertLocator (root.locator "input[type=text]") beDisabled
                                    do! uncheck (root.locator "input[type=checkbox]")
                                    do! assertLocator (root.locator "input[type=text]") beEnabled
                                }
                        )

                        testComponent (
                            "an external error shows without submit and clears when the value changes",
                            "External",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "ada")
                                    do! click (root.locator "#fail")
                                    do! assertLocator (help root) (haveText "Taken")
                                    do! assertLocator (result root) (haveText "error:1")
                                    do! fill (root.locator "input", "ada2")
                                    do! assertLocator (help root) (haveText "")
                                    do! assertLocator (result root) (haveText "ok:ada2")
                                }
                        )

                        testComponent (
                            "optional succeeds with None when empty and fails when partly filled",
                            "Optional",
                            fun root ->
                                promise {
                                    do! assertLocator (result root) (haveText "ok:nothing")
                                    do! fill (root.locator "input", "A")
                                    do! blur (root.locator "input")
                                    do! assertLocator (result root) (haveText "error:1")
                                    do! assertLocator (help root) (haveText "Too short")
                                    do! fill (root.locator "input", "")
                                    do! assertLocator (result root) (haveText "ok:nothing")
                                    do! assertLocator (help root) (haveText "")
                                    do! fill (root.locator "input", "Ada")
                                    do! assertLocator (result root) (haveText "ok:Ada")
                                }
                        )
                    ]
                )

                testList (
                    "validateAsync",
                    [
                        testComponent (
                            "shows a spinner while checking, then the rejection as an external error",
                            "AsyncCheck",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "taken")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-field >> nth=0")
                                            (containClass "rf-field--validating")

                                    do! assertLocator (help root) (haveText "Taken")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-field >> nth=0")
                                            (haveClass "rf-field")

                                    do! assertLocator (result root) (haveText "error:1")
                                }
                        )

                        testComponent (
                            "drops a result that arrives after the value changed again",
                            "AsyncCheck",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "taken")
                                    do! fill (root.locator "input", "free")
                                    do! assertLocator (result root) (haveText "ok:free")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-field >> nth=0")
                                            (haveClass "rf-field")

                                    do! assertLocator (help root) (haveText "")
                                }
                        )

                        testComponent (
                            "submit waits for a pending check and then submits",
                            "AsyncCheck",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "free")
                                    do! click (root.locator "button[type=submit]")
                                    do! assertLocator (root.locator "#submitted") (haveText "free")
                                }
                        )

                        testComponent (
                            "submit waits for a pending check and does not submit a rejected value",
                            "AsyncCheck",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "taken")
                                    do! click (root.locator "button[type=submit]")
                                    do! assertLocator (help root) (haveText "Taken")
                                    do! assertLocator (root.locator "#submitted") (haveText "")
                                }
                        )
                    ]
                )

                testList (
                    "andThen",
                    [
                        testComponent (
                            "renders the branch for the selected value and keeps shared values across switches",
                            "Branch",
                            fun root ->
                                promise {
                                    do! assertLocator (root.locator "fieldset") (haveCount 0)
                                    do! selectOption (root.locator "select", "student")
                                    do! assertLocator (root.locator "legend") (haveText "Student")
                                    do! fill (root.locator "input[type=text] >> nth=0", "Ada")
                                    do! assertLocator (result root) (haveText "ok:student Ada")
                                    do! selectOption (root.locator "select", "teacher")
                                    do! assertLocator (root.locator "legend") (haveText "Teacher")

                                    do!
                                        assertLocator
                                            (root.locator "input[type=text] >> nth=0")
                                            (haveValue "Ada")

                                    do!
                                        assertLocator
                                            (root.locator "input[type=text]")
                                            (haveCount 2)

                                    do! fill (root.locator "input[type=text] >> nth=1", "Maths")

                                    do!
                                        assertLocator
                                            (result root)
                                            (haveText "ok:teacher Ada Maths")
                                }
                        )

                        testComponent (
                            "disposes the previous branch: observer count is stable after 50 switches",
                            "Branch",
                            fun root ->
                                promise {
                                    do! selectOption (root.locator "select", "student")
                                    do! click (root.locator "#count")
                                    do! assertLocator (root.locator "#observers") (haveText "2")
                                    do! click (root.locator "#switch-many")
                                    do! click (root.locator "#count")
                                    do! assertLocator (root.locator "#observers") (haveText "2")
                                }
                        )
                    ]
                )

                testList (
                    "conditional",
                    [
                        testComponent (
                            "showIf hides the field, drops its errors and yields None",
                            "Conditional",
                            fun root ->
                                promise {
                                    do! assertLocator (root.locator "#pet-name") (haveCount 0)
                                    do! assertLocator (result root) (haveText "ok:no pet")
                                    do! check (root.locator "#has-pet")
                                    do! assertLocator (root.locator "#pet-name") (haveCount 1)
                                    do! assertLocator (result root) (haveText "error:1")
                                    do! fill (root.locator "#pet-name", "Rex")
                                    do! assertLocator (result root) (haveText "ok:Rex")
                                    do! uncheck (root.locator "#has-pet")
                                    do! assertLocator (root.locator "#pet-name") (haveCount 0)
                                    do! assertLocator (result root) (haveText "ok:no pet")
                                    do! check (root.locator "#has-pet")
                                    do! assertLocator (root.locator "#pet-name") (haveValue "Rex")
                                }
                        )

                        testComponent (
                            "stepIf adds a step only while the condition holds",
                            "ConditionalWizard",
                            fun root ->
                                promise {
                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment")
                                            (haveCount 1)

                                    do!
                                        assertLocator
                                            (root.locator "button[type=submit]")
                                            (haveText "Finish")

                                    do! check (root.locator "#wants")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment")
                                            (haveCount 2)

                                    do!
                                        assertLocator
                                            (root.locator "button[type=submit]")
                                            (haveText "Next")

                                    do! click (root.locator "button[type=submit]")
                                    do! fill (root.locator "#details", "more")
                                    do! click (root.locator "button[type=submit]")
                                    do! assertLocator (root.locator "#submitted") (haveText "more")
                                }
                        )
                    ]
                )

                testList (
                    "wizard",
                    [
                        testComponent (
                            "Next on an invalid step shows its errors and stays",
                            "Wizard",
                            fun root ->
                                promise {
                                    do! assertLocator (root.locator "h2") (haveText "Account")
                                    do! click (root.locator "button[type=submit]")
                                    do! assertLocator (root.locator "h2") (haveText "Account")

                                    do!
                                        assertLocator
                                            (help root)
                                            (haveText "This field is required")

                                    do! assertLocator (root.locator "input") beFocused

                                    do!
                                        assertLocator
                                            (root.locator "button[type=submit]")
                                            (haveText "Next")
                                }
                        )

                        testComponent (
                            "Next moves on when the step is valid, Back keeps the values",
                            "Wizard",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "a@b.c")
                                    do! press (root.locator "input", "Enter")
                                    do! assertLocator (root.locator "h2") (haveText "Type")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment >> nth=1")
                                            (containClass "rf-steps__segment--active")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment >> nth=0")
                                            (containClass "rf-steps__segment--done")

                                    do! click (root.locator "button:has-text('Back')")
                                    do! assertLocator (root.locator "h2") (haveText "Account")
                                    do! assertLocator (root.locator "input") (haveValue "a@b.c")
                                    do! assertLocator (help root) (haveText "")
                                }
                        )

                        testComponent (
                            "steps added by andThen follow the chosen value and the last step submits",
                            "Wizard",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "a@b.c")
                                    do! click (root.locator "button[type=submit]")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment")
                                            (haveCount 2)

                                    do! selectOption (root.locator "select", "teacher")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment")
                                            (haveCount 3)

                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment >> nth=2")
                                            (haveText "Teacher")

                                    do! click (root.locator "button[type=submit]")
                                    do! assertLocator (root.locator "h2") (haveText "Teacher")

                                    do!
                                        assertLocator
                                            (root.locator "button[type=submit]")
                                            (haveText "Register")

                                    do! fill (root.locator "input >> nth=0", "Ada")
                                    do! fill (root.locator "input >> nth=1", "Maths")
                                    do! click (root.locator "button[type=submit]")

                                    do!
                                        assertLocator
                                            (root.locator "#submitted")
                                            (haveText "a@b.c teacher Ada Maths")
                                }
                        )

                        testComponent (
                            "changing the branch value replaces the dependent steps",
                            "Wizard",
                            fun root ->
                                promise {
                                    do! fill (root.locator "input", "a@b.c")
                                    do! click (root.locator "button[type=submit]")
                                    do! selectOption (root.locator "select", "teacher")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment >> nth=2")
                                            (haveText "Teacher")

                                    do! selectOption (root.locator "select", "student")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-steps__segment >> nth=2")
                                            (haveText "Student")

                                    do! click (root.locator "button[type=submit]")
                                    do! assertLocator (root.locator "input") (haveCount 1)
                                    do! fill (root.locator "input", "Ada")
                                    do! click (root.locator "button[type=submit]")

                                    do!
                                        assertLocator
                                            (root.locator "#submitted")
                                            (haveText "a@b.c student Ada")
                                }
                        )
                    ]
                )

                testList (
                    "list",
                    [
                        testComponent (
                            "adds and removes items and renumbers the labels",
                            "List",
                            fun root ->
                                promise {
                                    do!
                                        assertLocator
                                            (root.locator ".rf-label >> nth=1")
                                            (haveText "Item #1")

                                    do! click (root.locator ".rf-list button:has-text('Add')")
                                    do! assertLocator (root.locator "input") (haveCount 2)

                                    do!
                                        assertLocator
                                            (root.locator ".rf-label >> nth=2")
                                            (haveText "Item #2")

                                    do! fill (root.locator "input >> nth=1", "second")

                                    do!
                                        assertLocator
                                            (result root)
                                            (haveText "ok:[\"first\"; \"second\"]")

                                    do!
                                        click (
                                            root.locator
                                                ".rf-list button:has-text('Remove') >> nth=0"
                                        )

                                    do! assertLocator (root.locator "input") (haveCount 1)
                                    do! assertLocator (root.locator "input") (haveValue "second")

                                    do!
                                        assertLocator
                                            (root.locator ".rf-label >> nth=1")
                                            (haveText "Item #1")

                                    do! assertLocator (result root) (haveText "ok:[\"second\"]")
                                }
                        )

                        testComponent (
                            "disposes removed items: observer count is stable after 50 add/remove cycles",
                            "List",
                            fun root ->
                                promise {
                                    do! click (root.locator "#count")
                                    do! assertLocator (root.locator "#observers") (haveText "1")
                                    do! click (root.locator "#cycle")
                                    do! assertLocator (root.locator "input") (haveCount 1)
                                    do! click (root.locator "#count")
                                    do! assertLocator (root.locator "#observers") (haveText "1")
                                }
                        )
                    ]
                )
            ]
        )
    )
