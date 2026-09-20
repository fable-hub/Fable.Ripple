---
title: Custom field
---

`Fable.Ripple.Form` allows you to create your own custom fields.

A field concist in three things: attributes, a render function, and a constructor.

This page adds an autocomplete field: a text input whose suggestions come from a `Var`{fsharp}, drawn with a `datalist`{html}.

## Step by Step Guide

:::steps

### Describe the field's attributes

Any record works, as long as it gives the form the field's id.

`Suggestions`{fsharp} is a function so that it can read a `Var`{fsharp}: whatever it reads becomes a dependency of the field. The `create`{fsharp} and `with*`{fsharp} builders keep the field consistent with the built-in ones.

```fsharp
module AutoCompleteField =

    type Attributes =
        {
            FieldId: string
            Label: string
            Suggestions: unit -> string list
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

type AutoCompleteField =

    static member create
        (fieldId: string)
        (suggestions: unit -> string list)
        : AutoCompleteField.Attributes
        =
        {
            FieldId = fieldId
            Label = ""
            Suggestions = suggestions
        }

    static member withLabel (label: string) (attributes: AutoCompleteField.Attributes) =
        { attributes with
            Label = label
        }
```

### Draw it

The render function receives the field's `Var`{fsharp}, its attributes, and tracked reads of its state.

Three things wire a control into the form:

- `View.controlAttributes`{fsharp} for the id and the ARIA attributes.
- `config.OnBlur`{fsharp} on the control's `blur`{html} event.
- `PlainView.withLabelAndError`{fsharp} for the label and the help text around it.

```fsharp
module Fields =

    let autoComplete (config: FieldRenderConfig<string, AutoCompleteField.Attributes>) : DomItem =
        let listId = config.Id + "-list"

        Html.fragment
            [
                Html.input
                    [
                        View.controlAttributes config
                        attr.className "rf-input"
                        attr.type' "text"
                        attr.list listId
                        attr.bindValue config.Value
                        on.blur (fun _ -> config.OnBlur())
                        attr.disabled config.Disabled
                        attr.readOnly config.ReadOnly
                    ]
                Html.datalist
                    [
                        attr.id listId
                        Html.each
                            (fun () ->
                                config.Attributes.Suggestions()
                                |> List.filter (fun s -> s.Contains config.Value.Value)
                                |> List.toArray
                            )
                            id
                            (fun s -> Html.option [ attr.value s ])
                    ]
            ]
        |> PlainView.withLabelAndError config.Attributes.Label config
```

### Give it a constructor

`Base.field`{fsharp} takes what counts as empty, the render function and the config.

The empty check is what makes a blank field "required".

```fsharp
[<RequireQualifiedAccess>]
module Form =

    let autoCompleteField
        (config: FieldConfig<AutoCompleteField.Attributes, string, 'Output>)
        : Form<'Output>
        =
        Base.field System.String.IsNullOrEmpty Fields.autoComplete config
```

:::

## Using it

The three pieces together, in a form:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

module AutoCompleteField =

    type Attributes =
        {
            FieldId: string
            Label: string
            Suggestions: unit -> string list
        }

        interface IAttributes with
            member this.GetFieldId() = this.FieldId

type AutoCompleteField =

    static member create
        (fieldId: string)
        (suggestions: unit -> string list)
        : AutoCompleteField.Attributes
        =
        {
            FieldId = fieldId
            Label = ""
            Suggestions = suggestions
        }

    static member withLabel (label: string) (attributes: AutoCompleteField.Attributes) =
        { attributes with
            Label = label
        }

module Fields =

    let autoComplete (config: FieldRenderConfig<string, AutoCompleteField.Attributes>) : DomItem =
        let listId = config.Id + "-list"

        Html.fragment
            [
                Html.input
                    [
                        View.controlAttributes config
                        attr.className "rf-input"
                        attr.type' "text"
                        attr.list listId
                        attr.bindValue config.Value
                        on.blur (fun _ -> config.OnBlur())
                        attr.disabled config.Disabled
                        attr.readOnly config.ReadOnly
                    ]
                Html.datalist
                    [
                        attr.id listId
                        Html.each
                            (fun () ->
                                config.Attributes.Suggestions()
                                |> List.filter (fun s -> s.Contains config.Value.Value)
                                |> List.toArray
                            )
                            id
                            (fun s -> Html.option [ attr.value s ])
                    ]
            ]
        |> PlainView.withLabelAndError config.Attributes.Label config

[<RequireQualifiedAccess>]
module Form =

    let autoCompleteField
        (config: FieldConfig<AutoCompleteField.Attributes, string, 'Output>)
        : Form<'Output>
        =
        Base.field System.String.IsNullOrEmpty Fields.autoComplete config

let city = Var.create ""

let cities =
    Var.create
        [
            "Paris"
            "Prague"
            "Porto"
            "Lyon"
            "Lisbon"
        ]

let form =
    AutoCompleteField.create "city" (fun () -> cities.Value)
    |> AutoCompleteField.withLabel "City"
    |> Field.create
        city
        (fun value ->
            if value.Length < 2 then
                Error "At least 2 characters"
            else
                Ok value
        )
    |> Form.autoCompleteField

let state = Var.create View.Idle

let view () =
    Form.View.asHtml
        {
            OnSubmit = fun city -> state.Value <- View.Success $"Going to %s{city}"
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Go"
            Validation = ValidateOnBlur
        }
        form

Html.mount "app" view |> ignore
```

Type `P`: the suggestions narrow as you type.

The field validates on blur, can be made optional, disabled or part of a list, like any other.

## Beyond text

The `Var`{fsharp} holds whatever the control edits. A tag picker holds a `string list` and is empty when the list is; a date picker holds a `DateTime option` and is empty on `None`{fsharp}. The parser then receives that type instead of a string.
