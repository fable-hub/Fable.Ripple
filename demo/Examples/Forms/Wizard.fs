module Demo.Examples.Forms.Wizard

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain
open Demo.Examples.Components // demo-hide-line

// A wizard composes steps the way a form composes fields, and its output stays
// typed. `stepIf` adds a step only while a condition holds. Next validates the
// current step only; the last step submits the whole result.

type Registration =
    {
        Email: string
        Company: string option
        Newsletter: bool
    }

let render () =
    let email = Var.create ""
    let isCompany = Var.create false
    let company = Var.create ""
    let newsletter = Var.create false

    let account =
        Form.succeed (fun email _ -> email)
        |> Form.append (
            TextField.create "email"
            |> TextField.withLabel "Email"
            |> Field.create
                email
                (fun value ->
                    if value.Contains "@" then
                        Ok value
                    else
                        Error "An email needs an @"
                )
            |> Form.textField
        )
        |> Form.append (
            CheckboxField.create "is-company"
            |> CheckboxField.withText "This is a company account"
            |> Field.create isCompany Ok
            |> Form.checkboxField
        )

    let companyStep =
        TextField.create "company"
        |> TextField.withLabel "Company name"
        |> Field.create company Ok
        |> Form.textField

    let preferences =
        CheckboxField.create "newsletter"
        |> CheckboxField.withText "Send me the newsletter"
        |> Field.create newsletter Ok
        |> Form.checkboxField

    let wizard =
        Wizard.succeed (fun email company newsletter ->
            {
                Email = email
                Company = company
                Newsletter = newsletter
            }
        )
        |> Wizard.step "Account" account
        |> Wizard.stepIf (fun () -> isCompany.Value) "Company" companyStep
        |> Wizard.step "Preferences" preferences

    let state = Var.create View.Idle

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Tick the company box: a step appears between the two others. Next refuses to leave a step with errors; Enter in a field is Next."

            // demo-show
            WizardView.asHtml
                {
                    OnSubmit = fun registration -> state.Value <- View.Success $"%A{registration}"
                    State = state
                    Validation = ValidateOnSubmit
                    Back = "Back"
                    Next = "Next"
                    Submit = "Register"
                }
                wizard
        ]
