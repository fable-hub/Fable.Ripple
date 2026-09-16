---
title: Customize the view
---

You can provide your own markup for any built-in field.

Write a render function for the same attributes and a constructor with the same name, then shadow the built-in field by opening your module after `Fable.Ripple.Form.Plain`{fsharp}.

`Form.textField`{fsharp} then refers to your implementation, while the rest of `Form`{fsharp} stays the package's.

This page gives the text field a different layout: the label above the control, the error under it, in your own classes.

:::steps

### Write the render function

Same signature as the package's `TextField.render`{fsharp}. Four things wire a control into the form:

- `View.controlAttributes`{fsharp} for the id and the ARIA attributes.
- `config.OnBlur`{fsharp} on the control's `blur`{html} event.
- `config.Disabled`{fsharp} and `config.ReadOnly`{fsharp} on the control.
- `View.shownError`{fsharp} for the message to display, `None`{fsharp} until the validation mode says to show it.

```fsharp
module Fields =

    let text (config: FieldRenderConfig<string, TextField.Attributes>) : DomItem =
        Html.div
            [
                attr.className "my-field"
                Html.p
                    [
                        attr.className "my-field__label"
                        View.labelText config.Attributes.Label config
                    ]
                Html.input
                    [
                        View.controlAttributes config
                        attr.className "my-field__input"
                        attr.bindValue config.Value
                        on.blur (fun _ -> config.OnBlur())
                        attr.disabled config.Disabled
                        attr.readOnly config.ReadOnly
                    ]
                Html.p
                    [
                        attr.className "my-field__error"
                        Html.text (fun () -> View.shownError config () |> Option.defaultValue "")
                    ]
            ]
```

### Shadow the constructor

Same name and same empty check as the package's, with your render function.

```fsharp
[<RequireQualifiedAccess>]
module Form =

    let textField (config: FieldConfig<TextField.Attributes, string, 'Output>) : Form<'Output> =
        Base.field System.String.IsNullOrEmpty Fields.text config
```

:::

## Using it

Both pieces together, in a form. The text field has the new markup; the number field is still the package's.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

module Fields =

    let text (config: FieldRenderConfig<string, TextField.Attributes>) : DomItem =
        Html.div
            [
                attr.className "my-field"
                Html.p
                    [
                        attr.className "my-field__label"
                        View.labelText config.Attributes.Label config
                    ]
                Html.input
                    [
                        View.controlAttributes config
                        attr.className "my-field__input"
                        attr.bindValue config.Value
                        on.blur (fun _ -> config.OnBlur())
                        attr.disabled config.Disabled
                        attr.readOnly config.ReadOnly
                    ]
                Html.p
                    [
                        attr.className "my-field__error"
                        Html.text (fun () -> View.shownError config () |> Option.defaultValue "")
                    ]
            ]

[<RequireQualifiedAccess>]
module Form =

    let textField (config: FieldConfig<TextField.Attributes, string, 'Output>) : Form<'Output> =
        Base.field System.String.IsNullOrEmpty Fields.text config

let name = Var.create ""
let age = Var.create ""

let form =
    Form.succeed (fun name age -> name, age)
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
        NumberField.create "age"
        |> NumberField.withLabel "Age"
        |> Field.create
            age
            (fun value ->
                match System.Int32.TryParse value with
                | true, age -> Ok age
                | _ -> Error "A whole number"
            )
        |> Form.numberField
    )

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit = fun (name, age) -> state.Value <- View.Success $"%s{name}, %d{age}"
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Save"
            Validation = ValidateOnBlur
        }
        form)
```
