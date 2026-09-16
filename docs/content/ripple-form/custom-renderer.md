---
title: Writing a renderer
---

The model draws nothing: no markup, no messages, no view state. A renderer supplies all of it, and `Fable.Ripple.Form.Plain` is one. 

This page explains how to write your own, for a CSS framework or a component library. It is recommend to use `Fable.Ripple.Form.Plain` sources are the reference, one file per piece.

## What the model gives you

A form is a tree of items and a handful of signals. There are two kinds of item:

- `FieldItem(id, render)`{fsharp}: one field.
- `Dynamic render`{fsharp}: everything else, from `andThen`{fsharp}, `showIf`{fsharp}, a layout or a list.

Both carry a `render`{fsharp} that takes a `RenderContext`{fsharp} and returns an element, so a renderer's job starts with a function that calls them:

```fsharp
let renderItems (context: RenderContext) (items: Item list) : DomItem list =
    items
    |> List.map (fun item ->
        match item with
        | FieldItem(_, render) -> render context
        | Dynamic render -> render context
    )
```

The context is what the combinators say about a subtree: disabled, read-only, which errors are visible, the validation mode, a replacement label, an external error, a running async check.

Nested items get it back through `context.RenderItems`{fsharp}. You build one only at the root, with `Base.renderContext`{fsharp}.

:::steps

### Add the field kinds

Each kind is attributes, a render function and a constructor calling `Base.field`{fsharp}, in a file of its own. [Custom field](custom-field.md) walks through one; the `Fields/` folder of the Plain package is the same three pieces, repeated.

The render function receives a `FieldRenderConfig`{fsharp}: the `Var`{fsharp} to bind, the attributes, tracked reads of the field's state, and `OnBlur`{fsharp}.

Plain's `View`{fsharp} module has the parts every HTML renderer needs:

- `controlAttributes`{fsharp} for the id and the ARIA attributes.
- `shownError`{fsharp} for the message to display.
- `labelText`{fsharp} for the label.

### Offer the layouts

The model has no section and no group. `Base.wrap`{fsharp} replaces a form's items with one element you draw around them, and a layout is one line of markup over it:

```fsharp
let section (title: string) (form: Form<'A>) : Form<'A> =
    form
    |> Base.wrap (fun context items ->
        Html.fieldset
            [
                Html.legend title
                yield! context.RenderItems context items
            ]
    )
```

### Draw the list

`FormList.form`{fsharp} in the Plain package is the combinator: it keeps one sub-form per item, disposes it when the item leaves, tracks the indexes and folds the results.

What it needs from you is a function `RenderContext -> FormList.RenderConfig -> DomItem`{fsharp}, which receives:

- the elements to place, each with its `Items`{fsharp} and a `Delete`{fsharp};
- an `Add`{fsharp} that appends a new item;
- whether the list is disabled.

`FormList.render`{fsharp} is the default one; [Lists](lists.md#your-own-list-markup) shows another.

### Write the view

A view renders `form.Items`{fsharp} inside a `<form>`{html}, with a status line and the action. Two calls do the work that is not markup:

- `View.renderContext`{fsharp} builds the root context from a `ViewConfig`{fsharp}.
- `View.onSubmit`{fsharp} is the submit handler: it waits for pending async checks, then calls `OnSubmit`{fsharp} on success, or shows every error and focuses the first invalid control.

```fsharp
let asHtml (config: View.ViewConfig<'Output>) (form: Form<'Output>) : DomItem =
    let mutable formElement: HTMLElement option = None
    let context = View.renderContext renderItems config

    Html.form
        [
            attr.ref (fun element -> formElement <- Some element)
            View.onSubmit config form (fun () -> formElement)
            yield! renderItems context form.Items

            // the status line and the action go here
        ]
```

:::

## Reusing the Plain package

A renderer for another framework can depend on `Fable.Ripple.Form.Plain` and reuse its `View`{fsharp} module, its attribute types and `FormList.form`{fsharp}, replacing only the markup. That is the cheapest path when the field API should stay as it is.
