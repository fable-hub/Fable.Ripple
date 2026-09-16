var e=`module Demo.Examples.Forms.FormState

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain
open Demo.Examples.Components // demo-hide-line

// Every field remembers its initial value. \`isDirty\` is a tracked read, \`reset\`
// goes back, \`commit\` makes the current values the new baseline, and \`load\`
// writes a plain record through a mapping you provide, then commits.

type Profile =
    {
        Name: string
        City: string
    }

let render () =
    let name = Var.create "Ada"
    let city = Var.create "London"

    let form =
        Form.succeed (fun name city ->
            {
                Name = name
                City = city
            }
        )
        |> Form.append (
            TextField.create "name"
            |> TextField.withLabel "Name"
            |> Field.create name Ok
            |> Form.textField
        )
        |> Form.append (
            TextField.create "city"
            |> TextField.withLabel "City"
            |> Field.create city Ok
            |> Form.textField
        )

    let values: FormValues<Profile> =
        {
            Snapshot =
                fun () ->
                    {
                        Name = name.Value
                        City = city.Value
                    }
            Load =
                fun profile ->
                    name.Value <- profile.Name
                    city.Value <- profile.City
        }

    let state = Var.create View.Idle
    let errors = View.errorVisibility ()

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Edit a field: Save and Reset light up. Reset puts the text back. Save commits, so the form is clean again with the new values. Load writes a record and commits with it, for values that are already saved."

            // demo-show
            Form.View.asHtml
                {
                    OnSubmit =
                        fun profile ->
                            Form.commit form

                            state.Value <-
                                View.Success $"Saved %s{profile.Name} in %s{profile.City}"
                    State = state
                    ErrorVisibility = errors
                    Action =
                        View.Action.Custom(fun _ ->
                            Row.row
                                [
                                    Button.primary (
                                        "Save",
                                        ignore,
                                        [
                                            attr.type' "submit"
                                            attr.disabled (fun () -> not (Form.isDirty form ()))
                                        ]
                                    )
                                    Button.action (
                                        "Reset",
                                        (fun _ ->
                                            Form.reset form
                                            errors.Reset()
                                        ),
                                        [
                                            attr.type' "button"
                                            attr.disabled (fun () -> not (Form.isDirty form ()))
                                        ]
                                    )
                                    Button.action (
                                        "Load Grace",
                                        (fun _ ->
                                            Form.load
                                                values
                                                form
                                                {
                                                    Name = "Grace"
                                                    City = "New York"
                                                }
                                        ),
                                        [ attr.type' "button" ]
                                    )
                                ]
                        )
                    Validation = ValidateOnSubmit
                }
                form
            Html.p
                [
                    Html.text (fun () ->
                        if Form.isDirty form () then
                            "unsaved changes"
                        else
                            "clean"
                    )
                ]
        ]
`;export{e as default};