---
title: Introduction
---

Fable.Ripple.Form turns a form into a typed value: a record, a union, a list. Each field is a `Var`{fsharp} with a parser; fields compose into a form. Change a field and only what depends on it updates.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let name = Var.create ""

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

let view () =
    Form.View.asHtml
        {
            OnSubmit = fun name -> state.Value <- View.Success $"Hello, %s{name}."
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Greet"
            Validation = ValidateOnBlur
        }
        form

Html.mount "app" view |> ignore
```

Press Greet with the field empty: the error shows and the field gets focus. Type two letters and press again.

## Two packages

`Fable.Ripple.Form` is the model: the form type, the combinators, validation, wizards. It knows nothing about HTML.

`Fable.Ripple.Form.Plain` is the renderer: the field kinds, sections and lists, the form and wizard views, and a stylesheet. Your application references this one; it brings the model with it.

```bash frame="terminal"
dotnet add package Fable.Ripple.Form.Plain --prerelease
```

The renderer uses plain `rf-*` classes and ships `plain.css` to style them; see [Styling](styling.md).

## Where to go next

- [Your first form](first-form.md) builds a sign-up form from start to finish.
- The guides that follow cover one thing each: fields, validation, dynamic forms, lists, wizards, saving.
- The Forms section of the [demo](https://fable-hub.github.io/Fable.Ripple/demo/#/forms/sign-up) has a working page per feature, with its source.
- The signatures are in the API reference, under Reference in the navbar.
