module Demo.Examples.Forms.SignUp

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain
open Demo.Examples.Components // demo-hide-line

// One Var per field. A parser turns the text into a typed value or an error;
// the form's output is the record below, built only when every field parses.
// The repeat-password parser reads `password.Value`, so it re-runs when the
// password changes: no `meta`, no wiring.

type User =
    {
        Email: string
        Name: string
        Password: string
        Newsletter: bool
    }

let render () =
    let email = Var.create ""
    let name = Var.create ""
    let password = Var.create ""
    let repeat = Var.create ""
    let newsletter = Var.create false

    let form =
        Form.succeed (fun email name password _ newsletter ->
            {
                Email = email
                Name = name
                Password = password
                Newsletter = newsletter
            }
        )
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
            // Simulates a server check: the field shows a spinner, then the answer.
            |> Form.validateAsync
                300
                (fun value ->
                    promise {
                        do! Promise.sleep 600

                        return
                            if value = "taken@example.com" then
                                Error "This address is taken"
                            else
                                Ok()
                    }
                )
        )
        |> Form.append (
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
        )
        |> Form.append (
            PasswordField.create "password"
            |> PasswordField.withLabel "Password"
            |> Field.create
                password
                (fun value ->
                    if value.Length < 4 then
                        Error "At least 4 characters"
                    else
                        Ok value
                )
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

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Submit with empty fields: every error shows and the first field gets focus. Type an email: the check runs after you stop typing. Try `taken@example.com`."

            // demo-show
            Form.View.asHtml
                {
                    OnSubmit = fun user -> state.Value <- View.Success $"Welcome, %s{user.Name}."
                    State = state
                    ErrorVisibility = View.errorVisibility ()
                    Action = View.Action.SubmitOnly "Sign up"
                    Validation = ValidateOnBlur
                }
                form
        ]
