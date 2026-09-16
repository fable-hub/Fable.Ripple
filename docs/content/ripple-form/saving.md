---
title: Saving and loading
---

Every field remembers a baseline: the value it had when the form was built. Comparing with it gives a dirty flag; writing it back is a reset; moving it is a save. Each piece below adds one button to the same profile form.

## Knowing when there is something to save

`Form.isDirty form ()`{fsharp} is true while any field differs from its baseline. It is a tracked read, so a button whose `disabled`{fsharp} reads it follows the fields. The submit button is drawn by hand here, with `View.Action.Custom`{fsharp}, to give it that attribute.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let name = Var.create "Ada"

let form =
    TextField.create "name"
    |> TextField.withLabel "Name"
    |> Field.create name Ok
    |> Form.textField

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit = fun name -> state.Value <- View.Success $"Saved %s{name}"
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action =
                View.Action.Custom(fun _ ->
                    Html.div
                        [
                            attr.className "rf-actions"
                            Html.button
                                [
                                    attr.type' "submit"
                                    attr.className "rf-button rf-button--primary"
                                    attr.disabled (fun () -> not (Form.isDirty form ()))
                                    Html.text "Save"
                                ]
                        ]
                )
            Validation = ValidateOnSubmit
        }
        form)
```

Save is disabled. Change the name: it lights up. Type `Ada` back: it goes off again, because the value equals the baseline.

## Going back

`Form.reset form`{fsharp} writes every baseline back. Errors that were showing stay shown until the error visibility is reset too, so the two calls go together.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let name = Var.create "Ada"
let errors = View.errorVisibility ()

let form =
    TextField.create "name"
    |> TextField.withLabel "Name"
    |> Field.create
        name
        (fun value ->
            if value.Length < 2 then
                Error "At least 2 characters"
            else
                Ok value
        )
    |> Form.textField

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit = fun name -> state.Value <- View.Success $"Saved %s{name}"
            State = state
            ErrorVisibility = errors
            Action =
                View.Action.Custom(fun _ ->
                    Html.div
                        [
                            attr.className "rf-actions"
                            Html.button
                                [
                                    attr.type' "button"
                                    attr.className "rf-button"
                                    attr.disabled (fun () -> not (Form.isDirty form ()))
                                    on.click (fun _ ->
                                        Form.reset form
                                        errors.Reset()
                                        state.Value <- View.Idle
                                    )
                                    Html.text "Reset"
                                ]
                            Html.button
                                [
                                    attr.type' "submit"
                                    attr.className "rf-button rf-button--primary"
                                    attr.disabled (fun () -> not (Form.isDirty form ()))
                                    Html.text "Save"
                                ]
                        ]
                )
            Validation = ValidateOnBlur
        }
        form)
```

Clear the name and leave the field: the error shows and Reset lights up. Press it: `Ada`{fsharp} is back and the error is gone.

## Saving

Saving is not the library's business; what it needs to know is that the current values are now the baseline. `Form.commit form`{fsharp} does that. Call it once the save succeeded, and the form is clean again with the new values.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let name = Var.create "Ada"

let form =
    TextField.create "name"
    |> TextField.withLabel "Name"
    |> Field.create name Ok
    |> Form.textField

let state = Var.create View.Idle

Html.mount
    "app"
    (Html.div
        [
            Form.View.asHtml
                {
                    OnSubmit =
                        fun name ->
                            // send it to the server here
                            Form.commit form
                            state.Value <- View.Success $"Saved %s{name}"
                    State = state
                    ErrorVisibility = View.errorVisibility ()
                    Action =
                        View.Action.Custom(fun _ ->
                            Html.div
                                [
                                    attr.className "rf-actions"
                                    Html.button
                                        [
                                            attr.type' "submit"
                                            attr.className "rf-button rf-button--primary"
                                            attr.disabled (fun () -> not (Form.isDirty form ()))
                                            Html.text "Save"
                                        ]
                                ]
                        )
                    Validation = ValidateOnSubmit
                }
                form
            Html.p
                [
                    Html.text (fun () ->
                        if Form.isDirty form () then
                            "Unsaved changes"
                        else
                            "Everything saved"
                    )
                ]
        ])
```

Change the name and press Save: the line under the form goes back to "Everything saved" and the button goes off, with the new name in the field.

## Loading a record

To fill the form from a record, say a profile fetched from the server, describe once how the record maps to the `Var`{fsharp}s. `FormValues`{fsharp} is that pair of functions. `Form.load values form record`{fsharp} then writes every field in one batch and commits, so the loaded record is the new baseline.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

type Profile =
    {
        Name: string
        City: string
    }

let name = Var.create ""
let city = Var.create ""

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

Html.mount
    "app"
    (Html.div
        [
            Html.button
                [
                    attr.className "rf-button"
                    on.click (fun _ ->
                        Form.load
                            values
                            form
                            {
                                Name = "Grace"
                                City = "New York"
                            }
                    )
                    Html.text "Load Grace"
                ]
            Form.View.asHtml
                {
                    OnSubmit =
                        fun profile ->
                            state.Value <-
                                View.Success $"Saved %s{profile.Name} in %s{profile.City}"
                    State = state
                    ErrorVisibility = View.errorVisibility ()
                    Action = View.Action.SubmitOnly "Save"
                    Validation = ValidateOnSubmit
                }
                form
            Html.p
                [
                    Html.text (fun () ->
                        if Form.isDirty form () then
                            "Unsaved changes"
                        else
                            "Everything saved"
                    )
                ]
        ])
```

Press Load Grace: both fields fill and the form is clean. Edit a field: unsaved. `Form.snapshot values`{fsharp} is the other direction, the record as it is typed, for a draft or a preview.

## Writing a Var from outside

The form owns nothing. Any code can write a field's `Var`{fsharp}; the field, its parser, every check that reads it and the dirty flag follow. `Form.load`{fsharp} is exactly that, for every field at once, followed by a commit.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let name = Var.create "Ada"

let form =
    TextField.create "name"
    |> TextField.withLabel "Name"
    |> Field.create name Ok
    |> Form.textField

Html.mount
    "app"
    (Html.div
        [
            Html.button
                [
                    attr.className "rf-button"
                    on.click (fun _ -> name.Value <- name.Value.ToUpper())
                    Html.text "Upper case"
                ]
            Form.View.asHtml
                {
                    OnSubmit = ignore
                    State = Var.create View.Idle
                    ErrorVisibility = View.errorVisibility ()
                    Action = View.Action.Custom(fun _ -> Html.none)
                    Validation = ValidateOnSubmit
                }
                form
            Html.p
                [
                    Html.text (fun () ->
                        if Form.isDirty form () then
                            "Changed"
                        else
                            "Unchanged"
                    )
                ]
        ])
```
