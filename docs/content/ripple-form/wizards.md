---
title: Wizards
---

A wizard is a form split into steps. The steps are composed the same way fields are, and the result is still one typed value.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

type Registration =
    {
        Email: string
        Company: string option
        Newsletter: bool
    }

let email = Var.create ""
let isCompany = Var.create false
let company = Var.create ""
let newsletter = Var.create false

let account =
    Form.succeed (fun email _ -> email)
    |> Form.append (
        EmailField.create "email"
        |> EmailField.withLabel "Email"
        |> Field.create
            email
            (fun value ->
                if value.Contains "@" then
                    Ok value
                else
                    Error "An email needs an @"
            )
        |> Form.emailField
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

let view () =
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

Html.mount "app" view |> ignore
```

Tick the company box: a step appears between the two others. Press Next with a bad email: it refuses to move on. Enter in a field is Next.

## Building one

`Wizard.succeed`{fsharp} takes the function that builds the result; `Wizard.step`{fsharp} adds a titled step and feeds its value to that function, like `Form.append`{fsharp} does for fields. A step is any form, so a step with several fields is a `Form.succeed`{fsharp} of its own.

`Wizard.stepIf`{fsharp} adds a step only while a condition holds; its value is `None`{fsharp} when the step is absent. `Wizard.andThen`{fsharp} builds the following steps from a value, when the shape of the rest depends on a choice.

## How it behaves

- Next validates the current step only. The last step's Next is the submit button, and it validates every step; if an earlier step became invalid, the wizard goes back to it.
- Every step keeps its values while the user moves around, since they live in your `Var`{fsharp}s.
- `WizardView.asHtml`{fsharp} takes the same `State`{fsharp} and `Validation`{fsharp} as a form view, plus the three button labels.
