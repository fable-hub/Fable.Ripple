module Demo.Examples.Forms.Conditional

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain
open Demo.Examples.Components // demo-hide-line

// `showIf` shows a form while a condition holds. Hidden, it renders nothing,
// yields `None` and reports no error. Its Var keeps the value, so showing it
// again restores what was typed. `disableIf` follows a signal the same way.

let render () =
    let hasPet = Var.create false
    let petName = Var.create ""
    let locked = Var.create false
    let notes = Var.create ""

    let form =
        Form.succeed (fun _ petName _ notes ->
            match petName with
            | Some name -> $"%s{name}, %s{notes}"
            | None -> $"no pet, %s{notes}"
        )
        |> Form.append (
            CheckboxField.create "has-pet"
            |> CheckboxField.withText "I have a pet"
            |> Field.create hasPet Ok
            |> Form.checkboxField
        )
        |> Form.append (
            TextField.create "pet-name"
            |> TextField.withLabel "Its name"
            |> Field.create petName Ok
            |> Form.textField
            |> Form.showIf (fun () -> hasPet.Value)
        )
        |> Form.append (
            CheckboxField.create "locked"
            |> CheckboxField.withText "Lock the notes"
            |> Field.create locked Ok
            |> Form.checkboxField
        )
        |> Form.append (
            TextareaField.create "notes"
            |> TextareaField.withLabel "Notes"
            |> Field.create notes Ok
            |> Form.textareaField
            |> Form.optional
            |> Form.map (Option.defaultValue "no notes")
            |> Form.disableIf (fun () -> locked.Value)
        )

    let state = Var.create View.Idle

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Tick the pet box: a required field appears. Untick it: the field goes and the form is valid again. Tick it once more: the name is back."

            // demo-show
            Form.View.asHtml
                {
                    OnSubmit = fun summary -> state.Value <- View.Success summary
                    State = state
                    ErrorVisibility = View.errorVisibility ()
                    Action = View.Action.SubmitOnly "Submit"
                    Validation = ValidateOnSubmit
                }
                form
        ]
