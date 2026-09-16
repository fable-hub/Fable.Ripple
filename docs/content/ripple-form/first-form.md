---
title: Your first form
---

A sign-up form: an email, a password, its repeat, and a newsletter checkbox.

Submitting gives a `User`{fsharp} record, and its fields are not strings: an `Email`{fsharp} can only be made through `Email.tryParse`{fsharp}, so a `User`{fsharp} holds a valid address by construction.

```fsharp
type Email = private Email of string

module Email =
    let tryParse (value: string) =
        if value.Contains "@" then
            Ok(Email value)
        else
            Error "An email needs an @"

    let value (Email email) = email

type Password = private Password of string

module Password =
    let tryParse (value: string) =
        if value.Length < 4 then
            Error "At least 4 characters"
        else
            Ok(Password value)

type User =
    {
        Email: Email
        Password: Password
        Newsletter: bool
    }
```

## One Var per field

Every field stores what the user typed in a `Var`{fsharp}. You create them, so you can read or write them from anywhere: set a default, prefill from a server, watch them, etc.

```fsharp
let email = Var.create ""
let password = Var.create ""
let repeat = Var.create ""
let newsletter = Var.create false
```

## A field

A field is built in three steps. The attributes say how it looks; `Field.create`{fsharp} ties it to its `Var`{fsharp} and gives it a parser; the constructor picks the field kind.

```fsharp
let emailField =
    EmailField.create "email"
    |> EmailField.withLabel "Email"
    |> Field.create email Email.tryParse
    |> Form.emailField
```

The parser turns the text into your type, or into a message for the user, every time the value changes. `Email.tryParse`{fsharp} is one already.

A parser can read another field. The repeat field reads `password.Value`{fsharp}, so it is checked again whenever the password changes:

```fsharp
let repeatField =
    PasswordField.create "repeat"
    |> PasswordField.withLabel "Repeat password"
    |> Field.create
        repeat
        (fun value ->
            if value = password.Value then
                Ok()
            else
                Error "The passwords do not match"
        )
    |> Form.passwordField
```

## Putting the fields together

`Form.succeed`{fsharp} takes the function that builds the result. Each `Form.append`{fsharp} gives it the next field's value, in order. The repeat field's value is not needed, so its argument is `_`{fsharp}.

```fsharp
let form =
    Form.succeed (fun email password _ newsletter ->
        {
            Email = email
            Password = password
            Newsletter = newsletter
        }
    )
    |> Form.append emailField
    |> Form.append passwordField
    |> Form.append repeatField
    |> Form.append newsletterField
```

## Showing it

`Form.View.asHtml`{fsharp} renders the form with a submit button. `OnSubmit`{fsharp} receives the `User`{fsharp} once every field parses; until then, submitting shows the errors instead.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

type Email = private Email of string

module Email =
    let tryParse (value: string) =
        if value.Contains "@" then
            Ok(Email value)
        else
            Error "An email needs an @"

    let value (Email email) = email

type Password = private Password of string

module Password =
    let tryParse (value: string) =
        if value.Length < 4 then
            Error "At least 4 characters"
        else
            Ok(Password value)

type User =
    {
        Email: Email
        Password: Password
        Newsletter: bool
    }

let email = Var.create ""
let password = Var.create ""
let repeat = Var.create ""
let newsletter = Var.create false

let form =
    Form.succeed (fun email password _ newsletter ->
        {
            Email = email
            Password = password
            Newsletter = newsletter
        }
    )
    |> Form.append (
        EmailField.create "email"
        |> EmailField.withLabel "Email"
        |> Field.create email Email.tryParse
        |> Form.emailField
    )
    |> Form.append (
        PasswordField.create "password"
        |> PasswordField.withLabel "Password"
        |> Field.create password Password.tryParse
        |> Form.passwordField
    )
    |> Form.append (
        PasswordField.create "repeat"
        |> PasswordField.withLabel "Repeat password"
        |> Field.create
            repeat
            (fun value ->
                if value = password.Value then
                    Ok()
                else
                    Error "The passwords do not match"
            )
        |> Form.passwordField
    )
    |> Form.append (
        CheckboxField.create "newsletter"
        |> CheckboxField.withText "Send me the newsletter"
        |> Field.create newsletter Ok
        |> Form.checkboxField
    )

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit =
                fun user -> state.Value <- View.Success $"Welcome, %s{Email.value user.Email}."
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Sign up"
            Validation = ValidateOnBlur
        }
        form)
```

Fill the password, then type something else in the repeat: its error appears as soon as you leave the field. Fix the password instead: the repeat's error goes away, because its parser re-ran.

The view config has five parts:

- `OnSubmit`{fsharp}: what to do with the result.
- `State`{fsharp}: a `Var`{fsharp} you write. `Loading`{fsharp} disables the fields and the button; `Success`{fsharp} and `Error`{fsharp} show a message under the form.
- `ErrorVisibility`{fsharp}: which errors are showing. One per form, from `View.errorVisibility ()`{fsharp}.
- `Action`{fsharp}: `SubmitOnly "label"`{fsharp} for one button, or `Custom`{fsharp} to draw your own; see [Saving and loading](saving.md).
- `Validation`{fsharp}: when a field starts showing its error; see [Validation](validation.md).

## Reading the result without submitting

The form is a value with signals: `form.Result`{fsharp} is the current `Ok user`{fsharp} or the errors, `form.IsEmpty`{fsharp} tells whether everything is blank. A "Save" button can be disabled while `form.Result.Value`{fsharp} is an `Error`{fsharp}, and a preview can read the record as it is typed.
