---
title: Lists
---

A form can hold a list of sub-forms: several addresses, several books, several line items. Each item is a record of `Var`{fsharp}s, the list itself is a `Var`{fsharp} of those records, and `Form.list`{fsharp} renders one sub-form per item.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

[<NoComparison; NoEquality>]
type BookFields =
    {
        Title: Var<string>
        Author: Var<string>
    }

type Book =
    {
        Title: string
        Author: string
    }

let book (title: string) (author: string) : BookFields =
    {
        Title = Var.create title
        Author = Var.create author
    }

let books = Var.create [ book "The Painted Man" "Peter V. Brett" ]

let form =
    FormList.create "books"
    |> FormList.withLabel "Books"
    |> FormList.withAdd "Add a book"
    |> FormList.withDelete "Remove"
    |> Form.list
        {
            Items = books
            Default = fun () -> book "" ""
        }
        (fun context (fields: BookFields) ->
            Form.succeed (fun title author ->
                ({
                    Title = title
                    Author = author
                }
                : Book)
            )
            |> Form.append (
                TextField.create $"%s{context.FieldIdPrefix}-title"
                |> Field.create fields.Title Ok
                |> Form.textField
                |> Form.withLabel (fun () -> $"Title of book #%d{context.Index.Value + 1}")
            )
            |> Form.append (
                TextField.create $"%s{context.FieldIdPrefix}-author"
                |> Field.create fields.Author Ok
                |> Form.textField
                |> Form.withLabel (fun () -> $"Author of book #%d{context.Index.Value + 1}")
            )
        )

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit = fun books -> state.Value <- View.Success $"%d{List.length books} book(s)"
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Save"
            Validation = ValidateOnSubmit
        }
        form)
```

Add a book, type in it, then remove the first one: the second keeps its text and its labels say #1.

## The pieces

- `FormList.create`{fsharp} names the list; `withAdd`{fsharp} and `withDelete`{fsharp} add the buttons, with their labels.
- `Items`{fsharp} is your `Var`{fsharp} of items. The buttons write it; so can you.
- `Default`{fsharp} builds the item the add button appends.
- The item function gets the item and a `context`{fsharp}: `FieldIdPrefix`{fsharp} for the ids of its fields, `Index`{fsharp} for its position.

The result is the list of the item results; one failing item fails the form.

## Your own list markup

`Form.listWith`{fsharp} is `Form.list`{fsharp} with a render function in front: you draw the list, the library keeps the items. The function receives the context and a config with the elements to place, an `Add`{fsharp} and a `Disabled`{fsharp} read; each element has its `Items`{fsharp} to render and a `Delete`{fsharp}.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

[<NoComparison; NoEquality>]
type Guest =
    {
        Name: Var<string>
    }

let guests =
    Var.create
        [
            {
                Name = Var.create "Ada"
            }
        ]

let asCards (context: RenderContext) (config: FormList.RenderConfig) : DomItem =
    Html.div
        [
            attr.className "rf-list"
            Html.div
                [
                    attr.className "rf-list__items"
                    Html.each
                        config.Elements
                        (fun element -> element.Key)
                        (fun element ->
                            Html.div
                                [
                                    attr.className "card"
                                    yield! context.RenderItems context element.Items
                                    Html.button
                                        [
                                            attr.type' "button"
                                            attr.className "rf-button rf-button--small"
                                            attr.style [ "margin-top", "var(--rf-gap-small)" ]
                                            on.click (fun _ -> element.Delete())
                                            Html.text "Not coming"
                                        ]
                                ]
                        )
                ]
            Html.button
                [
                    attr.type' "button"
                    on.click (fun _ -> config.Add())
                    Html.text "Invite someone"
                ]
        ]

let form =
    FormList.create "guests"
    |> Form.listWith
        asCards
        {
            Items = guests
            Default =
                fun () ->
                    {
                        Name = Var.create ""
                    }
        }
        (fun context (guest: Guest) ->
            TextField.create $"%s{context.FieldIdPrefix}-name"
            |> TextField.withAutoFocus
            |> Field.create guest.Name Ok
            |> Form.textField
            |> Form.withLabel (fun () -> $"Guest %d{context.Index.Value + 1}")
        )

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit = fun names -> state.Value <- View.Success(String.concat ", " names)
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Send invitations"
            Validation = ValidateOnSubmit
        }
        form)
```

`withAutoFocus`{fsharp} puts the cursor in the new guest's name as soon as it appears. The rows have a container of their own: `Html.each`{fsharp} adds new rows at the end of its parent, so a button placed after it in the same element would end up between the rows.

`Sortable.list`{fsharp} in the Plain package is a render function of the same kind, with a drag handle, up, down and remove buttons on each row.
