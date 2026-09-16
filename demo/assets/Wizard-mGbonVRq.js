var e=`module Demo.Examples.Recipes.Wizard

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// The steps are three \`Html.show\` branches rather than one \`dynamic\` over a
// \`match\`. That looks redundant - the steps are mutually exclusive, so a switch
// would do - but a \`dynamic\` re-runs whenever ANYTHING it reads changes, so a
// validation signal read in the body would wipe the form as you type.

let render () =
    let step = Var.create 1

    // Owned by the wizard, not by any step - so navigating back and forth keeps
    // what was typed.
    let name = Var.create ""
    let email = Var.create ""
    let plan = Var.create "free"
    let agreed = Var.create false

    let submitted = Var.create (None: string option)

    let emailLooksReal =
        email |> Signal.map (fun e -> e.Contains "@" && e.Contains "." && e.Length > 5)

    // Derived per step. Nothing writes a "valid" flag.
    let canContinue =
        Signal.computed (fun () ->
            match step.Value with
            | 1 -> name.Value.Trim().Length >= 2 && emailLooksReal.Value
            | 2 -> plan.Value <> ""
            | _ -> agreed.Value
        )

    let goto n =
        if n >= 1 && n <= 3 then
            step.Value <- n

    let submit () =
        submitted.Value <-
            Some(sprintf "%s <%s> on the %s plan" (name.Value) (email.Value) (plan.Value))

    let stepDot (n: int) (label: string) =
        Html.div
            [
                attr.className "wizard-step"
                attr.classList (fun () -> [ "is-current", step.Value = n ])
                attr.classList (fun () -> [ "is-done", step.Value > n ])
                Html.span
                    [
                        attr.className "probe-label"
                        Html.text (sprintf "%d" n)
                    ]
                Html.span label
            ]

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Type a bad email and try to continue: the step will not let you. Fix it, go on, then come back. Your answers are still there, because they live in the wizard, not in the step."

            // demo-show
            Row.row
                [
                    stepDot 1 "Details"
                    stepDot 2 "Plan"
                    stepDot 3 "Confirm"
                ]

            Html.show (
                (fun () -> step.Value = 1),
                fun () ->
                    Stack.stack
                        [
                            Html.label
                                [
                                    Html.text "name "
                                    Html.input [ attr.bindValue name ]
                                ]

                            Html.label
                                [
                                    Html.text "email "
                                    Html.input [ attr.bindValue email ]
                                ]

                            Html.show (
                                (fun () -> email.Value <> "" && not emailLooksReal.Value),
                                fun () ->
                                    Html.p
                                        [
                                            attr.className "error"
                                            Html.text "that does not look like an email address"
                                        ]
                            )
                        ]
            )

            Html.show (
                (fun () -> step.Value = 2),
                fun () ->
                    Row.row
                        [
                            for value, label in
                                [
                                    "free", "Free"
                                    "pro", "Pro"
                                    "team", "Team"
                                ] do
                                Html.label
                                    [
                                        Html.input
                                            [
                                                attr.type' "radio"
                                                attr.custom ("name", "plan")
                                                attr.checked' (fun () -> plan.Value = value)
                                                on.change (fun (_: string) -> plan.Value <- value)
                                            ]

                                        Html.text label
                                    ]
                        ]
            )

            Html.show (
                (fun () -> step.Value = 3),
                fun () ->
                    Stack.stack
                        [
                            Html.p
                                [
                                    Html.text (fun () ->
                                        sprintf
                                            "%s <%s> on the %s plan"
                                            name.Value
                                            email.Value
                                            plan.Value
                                    )
                                ]

                            Html.label
                                [
                                    Html.input
                                        [
                                            attr.type' "checkbox"
                                            attr.bindChecked agreed
                                        ]

                                    Html.text "I agree to the terms"
                                ]
                        ]
            )

            Row.row
                [
                    Html.button
                        [
                            attr.disabled (fun () -> step.Value <= 1)
                            on.click (fun _ -> goto (step.Value - 1))
                            Html.text "Back"
                        ]

                    Button.primary (
                        "Continue",
                        (fun _ -> goto (step.Value + 1)),
                        [ attr.disabled (fun () -> not canContinue.Value || step.Value = 3) ]
                    )

                    Button.primary (
                        "Submit",
                        (fun _ -> submit ()),
                        [
                            attr.hidden (fun () -> step.Value <> 3)
                            attr.disabled (fun () -> not canContinue.Value)
                        ]
                    )

                    // demo-hide
                    readout "step" (fun () -> string step.Value)
                    readout "can continue" (fun () -> string canContinue.Value)
                // demo-show
                ]

            Html.show (
                (fun () -> submitted.Value.IsSome),
                fun () ->
                    Html.p
                        [
                            Html.text (fun () ->
                                "Submitted: " + Option.defaultValue "" submitted.Value
                            )
                        ]
            )

        ]
`;export{e as default};