module Demo.Examples.Forms.FormList

open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain
open Demo.Examples.Components // demo-hide-line

// A list of sub-forms. Each item is a record of Vars; the list is a Var of them.
// Items are keyed by reference, so removing the first one moves the others
// without rebuilding them, and `context.Index` renumbers their labels.

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

let render () =
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

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Add a book, type in it, then remove the first one: the second keeps its text and becomes #1."

            // demo-show
            Form.View.asHtml
                {
                    OnSubmit =
                        fun books -> state.Value <- View.Success $"%d{List.length books} book(s)"
                    State = state
                    ErrorVisibility = View.errorVisibility ()
                    Action = View.Action.SubmitOnly "Save"
                    Validation = ValidateOnSubmit
                }
                form
        ]
