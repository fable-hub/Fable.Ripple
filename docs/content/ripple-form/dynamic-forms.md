---
title: Dynamic forms
---

Parts of a form can appear, disappear or change with what the user entered. Three combinators cover it.

## A part that depends on a value

`Form.andThen`{fsharp} builds the rest of the form from the value of the first part. The function runs when that value changes, and the previous part is thrown away.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

[<RequireQualifiedAccess>]
type Contact =
    | Email
    | Phone

    interface RadioField.OptionItem with
        member this.Key =
            match this with
            | Email -> "email"
            | Phone -> "phone"

        member this.Text =
            match this with
            | Email -> "Email"
            | Phone -> "Phone"

let contact: Var<RadioField.OptionItem option> = Var.create None
let email = Var.create ""
let phone = Var.create ""

let form =
    RadioField.create "contact"
    |> RadioField.withLabel "Contact me by"
    |> RadioField.withOptions
        [
            Contact.Email
            Contact.Phone
        ]
    |> Field.create
        contact
        (fun value ->
            match value with
            | Some item -> Ok(item :?> Contact)
            | None -> Error "Choose one"
        )
    |> Form.radioField
    |> Form.andThen (
        function
        | Contact.Email ->
            TextField.create "email"
            |> TextField.withLabel "Email"
            |> Field.create email Ok
            |> Form.textField
        | Contact.Phone ->
            TextField.create "phone"
            |> TextField.withLabel "Phone"
            |> Field.create phone Ok
            |> Form.textField
    )

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit = fun value -> state.Value <- View.Success $"Contact: %s{value}"
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Save"
            Validation = ValidateOnSubmit
        }
        form)
```

Pick Email, type an address, pick Phone, then Email again: the address is still there. The email field was rebuilt, but its value lives in the `email`{fsharp} `Var`{fsharp}, which was not.

The result of the whole form is the result of the part that is currently shown. A branch that needs several fields is a form of its own, built with `Form.succeed`{fsharp} and `Form.append`{fsharp}, and `Form.section`{fsharp} gives it a frame and a title.

## A field that shows on a condition

`Form.showIf`{fsharp} takes a condition and a form. While the condition holds the form is there; otherwise it renders nothing, yields `None`{fsharp}, and cannot fail. The condition is a tracked read, so a `Var`{fsharp} read inside it drives it.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let hasPet = Var.create false
let petName = Var.create ""

let form =
    Form.succeed (fun _ petName -> defaultArg petName "no pet")
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

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit = fun pet -> state.Value <- View.Success pet
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Save"
            Validation = ValidateOnSubmit
        }
        form)
```

Tick the box: a required field appears. Untick it: the form is valid again. Tick it once more: the name you typed is back.

## Disabling on a condition

`Form.disableIf`{fsharp} and `Form.readOnlyIf`{fsharp} keep the field visible and its value in the result, but the user cannot change it while the condition holds:

```fsharp
notesField |> Form.disableIf (fun () -> locked.Value)
```

## Which one to use

- The shape of the rest of the form depends on a value: `andThen`{fsharp}.
- One field or group comes and goes: `showIf`{fsharp}.
- The field stays, the user just cannot edit it: `disableIf`{fsharp}.
