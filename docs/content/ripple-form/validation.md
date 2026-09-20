---
title: Validation
---

Validation is the parsers. A field's parser runs on every change and returns the value or a message; the form is valid when every parser returns `Ok`{fsharp}. What the view decides is when to show the messages.

## When errors show

The view's `Validation`{fsharp} sets the default for every field:

- `ValidateOnSubmit`{fsharp}: nothing until the user submits with errors. Then every error shows.
- `ValidateOnBlur`{fsharp}: also as soon as the user leaves a field.
- `ValidateOnChange`{fsharp}: also as soon as the user changes a field.

`Form.validateOn`{fsharp} overrides the mode for one field or a sub-form: a username field can check on change while the rest waits for submit.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let username = Var.create ""
let bio = Var.create ""

let form =
    Form.succeed (fun username bio -> username, bio)
    |> Form.append (
        TextField.create "username"
        |> TextField.withLabel "Username (checked as you type)"
        |> Field.create
            username
            (fun value ->
                if value.Length < 3 then
                    Error "At least 3 characters"
                else
                    Ok value
            )
        |> Form.textField
        |> Form.validateOn ValidateOnChange
    )
    |> Form.append (
        TextField.create "bio"
        |> TextField.withLabel "Bio (checked on submit)"
        |> Field.create
            bio
            (fun value ->
                if value.Length > 20 then
                    Error "20 characters at most"
                else
                    Ok value
            )
        |> Form.textField
    )

let state = Var.create View.Idle

let view () =
    Form.View.asHtml
        {
            OnSubmit = fun _ -> state.Value <- View.Success "Saved"
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Save"
            Validation = ValidateOnSubmit
        }
        form

Html.mount "app" view |> ignore
```

## Checks across fields

A parser is a tracked read: any `Var`{fsharp} it reads becomes a dependency, and the field is re-checked when that `Var`{fsharp} changes. The repeat-password field in [Your first form](first-form.md) reads `password.Value`{fsharp} and that is all the wiring there is.

## Errors from the server

Some errors are only known after a request: the address is taken, the coupon expired. `Form.withExternalError`{fsharp} takes a tracked read that returns the message while it applies. The form fails and the message shows on the field right away, whatever the validation mode.

```fsharp
let taken: Var<string option> = Var.create None

emailField
|> Form.withExternalError (fun () ->
    match taken.Value with
    | Some address when address = email.Value -> Some "This address is taken"
    | _ -> None
)
```

Comparing with the current value makes the error go away as soon as the user changes the field.

## Asynchronous checks

`Form.validateAsync`{fsharp} runs a check on the field's parsed value some milliseconds after the user stopped typing. While it runs the field is marked as validating; a rejection shows as an error; submitting waits for a pending check.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let username = Var.create ""

let form =
    TextField.create "username"
    |> TextField.withLabel "Username"
    |> Field.create username Ok
    |> Form.textField
    |> Form.validateAsync
        300
        (fun value ->
            promise {
                do! Promise.sleep 600

                return
                    if value = "admin" then
                        Error "This name is taken"
                    else
                        Ok()
            }
        )

let state = Var.create View.Idle

let view () =
    Form.View.asHtml
        {
            OnSubmit = fun name -> state.Value <- View.Success $"Registered %s{name}"
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Register"
            Validation = ValidateOnSubmit
        }
        form

Html.mount "app" view |> ignore
```

Type `admin`. The check starts 300 ms after the last keystroke and answers 600 ms later. A result that arrives after the value changed again is ignored.

## Hiding the errors again

Once a submit has failed, every field shows its error until it is fixed. When the form starts over, for a new entry or after a reset, that state has to be cleared too: keep the `ErrorVisibility`{fsharp} you gave the view and call `Reset()`{fsharp} on it. It forgets the failed submit and which fields were left.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let name = Var.create ""
let errors = View.errorVisibility ()

let form =
    TextField.create "name"
    |> TextField.withLabel "Name"
    |> Field.create name (fun value ->
        if value.Length < 2 then
            Error "At least 2 characters"
        else
            Ok value
    )
    |> Form.textField

let state = Var.create View.Idle

let view () =
    Form.View.asHtml
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
                                    on.click (fun _ ->
                                        Form.reset form
                                        errors.Reset()
                                        state.Value <- View.Idle
                                    )
                                    Html.text "Start over"
                                ]
                            Html.button
                                [
                                    attr.type' "submit"
                                    attr.className "rf-button rf-button--primary"
                                    Html.text "Save"
                                ]
                        ]
                )
            Validation = ValidateOnBlur
        }
        form

Html.mount "app" view |> ignore
```

Press Save with the field empty: the error shows. Press Start over: the field is cleared and the error is gone. Without `errors.Reset()`{fsharp} the field would be cleared but still marked as failed, and "This field is required" would stay.

The "Saved" line after a successful submit is the view's `State`{fsharp}, which is your `Var`{fsharp}; the button sets it back to `Idle`{fsharp} for the same reason.
