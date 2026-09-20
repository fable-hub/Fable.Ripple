---
title: Fields
---

Every field is built the same way, whatever its kind.

:::steps

### Start from the attributes

`create`{fsharp} takes the field's id; the `with*`{fsharp} builders say how it looks. Each kind has its own type: `TextField`{fsharp}, `NumberField`{fsharp}, `SelectField`{fsharp}, and so on.

```fsharp
TextField.create "product" |> TextField.withLabel "Product"
```

### Add the value and the parser

`Field.create`{fsharp} ties the attributes to a `Var`{fsharp} and a parser. The parser turns what the user typed into your type, or into a message to show. Pass `Ok`{fsharp} when the text is the value.

```fsharp
|> Field.create name (fun value -> if value = "" then Error "Required" else Ok value)
```

### Close with the constructor

The constructor names the kind, and turns the whole thing into a form of one field.

```fsharp
|> Form.textField
```

:::

All the kinds in one form:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

[<RequireQualifiedAccess>]
type Size =
    | Small
    | Large

    interface SelectField.OptionItem with
        member this.Key =
            match this with
            | Small -> "s"
            | Large -> "l"

        member this.Text =
            match this with
            | Small -> "Small"
            | Large -> "Large"

let name = Var.create ""
let quantity = Var.create "1"
let size: Var<SelectField.OptionItem option> = Var.create None
let gift: Var<RadioField.OptionItem option> = Var.create None
let notes = Var.create ""
let agreed = Var.create false

let form =
    Form.succeed (fun name quantity size gift notes agreed ->
        $"%d{quantity} x %s{name} (%A{size}), gift: %A{gift}, notes: %s{notes}, agreed: %b{agreed}"
    )
    |> Form.append (
        TextField.create "name"
        |> TextField.withLabel "Product"
        |> TextField.withPlaceholder "What are you ordering?"
        |> Field.create name Ok
        |> Form.textField
    )
    |> Form.append (
        NumberField.create "quantity"
        |> NumberField.withLabel "Quantity"
        |> NumberField.withMin 1.0
        |> Field.create
            quantity
            (fun value ->
                match System.Int32.TryParse value with
                | true, n when n > 0 -> Ok n
                | _ -> Error "A whole number, at least 1"
            )
        |> Form.numberField
    )
    |> Form.append (
        SelectField.create "size"
        |> SelectField.withLabel "Size"
        |> SelectField.withPlaceholder "Choose"
        |> SelectField.withOptions
            [
                Size.Small
                Size.Large
            ]
        |> Field.create
            size
            (fun value ->
                match value with
                | Some item -> Ok(item :?> Size)
                | None -> Error "Choose a size"
            )
        |> Form.selectField
    )
    |> Form.append (
        RadioField.create "gift"
        |> RadioField.withLabel "Gift wrap"
        |> RadioField.withBasicOptions
            [
                "yes", "Yes"
                "no", "No"
            ]
        |> Field.create
            gift
            (fun value ->
                match value with
                | Some item -> Ok(item.Key = "yes")
                | None -> Error "Yes or no"
            )
        |> Form.radioField
    )
    |> Form.append (
        TextareaField.create "notes"
        |> TextareaField.withLabel "Notes"
        |> TextareaField.withRows 2
        |> Field.create notes Ok
        |> Form.textareaField
        |> Form.optional
        |> Form.map (Option.defaultValue "none")
    )
    |> Form.append (
        CheckboxField.create "agreed"
        |> CheckboxField.withText "I agree to the terms"
        |> Field.create
            agreed
            (fun value ->
                if value then
                    Ok value
                else
                    Error "You have to agree"
            )
        |> Form.checkboxField
    )

let state = Var.create View.Idle

let view () =
    Form.View.asHtml
        {
            OnSubmit = fun summary -> state.Value <- View.Success summary
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Order"
            Validation = ValidateOnBlur
        }
        form

Html.mount "app" view |> ignore
```

## The kinds

| Constructor | Attributes | The `Var`{fsharp} holds |
| --- | --- | --- |
| `Form.textField`{fsharp} | `TextField`{fsharp} | `string`{fsharp} |
| `Form.emailField`{fsharp} | `EmailField`{fsharp} | `string`{fsharp} |
| `Form.passwordField`{fsharp} | `PasswordField`{fsharp} | `string`{fsharp} |
| `Form.numberField`{fsharp} | `NumberField`{fsharp} | `string`{fsharp} |
| `Form.textareaField`{fsharp} | `TextareaField`{fsharp} | `string`{fsharp} |
| `Form.checkboxField`{fsharp} | `CheckboxField`{fsharp} | `bool`{fsharp} |
| `Form.selectField`{fsharp} | `SelectField`{fsharp} | `SelectField.OptionItem option`{fsharp} |
| `Form.radioField`{fsharp} | `RadioField`{fsharp} | `RadioField.OptionItem option`{fsharp} |

Text-like fields hold a `string`{fsharp} even for numbers: the parser is where `"42"`{fsharp} becomes `42`{fsharp}, and where a bad entry becomes a message. The builders on each attributes type (`withLabel`{fsharp}, `withPlaceholder`{fsharp}, `withMin`{fsharp}, `withRows`{fsharp}, ...) are listed in the API reference.

A kind that is not in the table is a [custom field](custom-field.md), and a kind with other markup is a [customized view](custom-view.md).

## Choices

A select or a radio group holds an `OptionItem option`{fsharp}. An option is anything with a `Key`{fsharp} and a `Text`{fsharp}; a union that implements the interface, as `Size`{fsharp} above, gives you typed options. The parser casts the chosen item back: `item :?> Size`{fsharp}.

For a quick list of strings, `withBasicOptions`{fsharp} takes key and text pairs, and the parser reads `item.Key`{fsharp}.

## Empty and optional

An empty field, an empty string or `None`{fsharp}, never reaches the parser: it is an error of its own, shown as "This field is required". A field that may stay empty is wrapped in `Form.optional`{fsharp}: it then yields `None`{fsharp} while empty and `Some value`{fsharp} once filled, with the parser applied only to the filled case. The notes field above does that, and `Form.map`{fsharp} turns the `None`{fsharp} into a default.

A checkbox is never empty; a checkbox that must be ticked says so in its parser, as the last field does.

## Labels that change

`Form.withLabel`{fsharp} replaces a field's label with a tracked read, so the label can follow other values. [Lists](lists.md) use it to number their items.

## Disabled and read-only

`Form.disable`{fsharp}, `Form.disableIf`{fsharp}, `Form.readOnly`{fsharp} and `Form.readOnlyIf`{fsharp} apply to a field or to a whole sub-form. The `If`{fsharp} variants take a condition that is a tracked read, so `Form.disableIf (fun () -> locked.Value)`{fsharp} follows the `locked`{fsharp} `Var`{fsharp}.
