var e=`module Demo.SevenGUIs.Todos

open Browser
open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom

// TodoMVC-style list. Each entry's Label is its own Var, so editing mutates in
// place: \`Html.each\` reconciles by Id and never sees an item change, so it only
// builds/removes DOM on add/remove - an item keeps its local done/editing state
// across label edits.

type private TodoEntry =
    {
        Id: int
        Label: Var<string>
    }

// One row: a done toggle, the label (double-click to edit), and a remove button.
let private todoItem (todo: TodoEntry) (onRemove: unit -> unit) : DomItem =
    let isDone = Var.create false
    let isEditing = Var.create false

    let commit (input: HTMLInputElement) =
        let text = input.value.Trim()

        Signal.batch (fun () ->
            if text <> "" then
                todo.Label.Value <- text

            isEditing.Value <- false
        )

    Html.li
        [
            attr.className "todo-item"

            Html.input
                [
                    attr.type' "checkbox"
                    attr.bindChecked isDone
                    attr.aria ("label", "Done")
                ]

            // \`show\` splices straight into the <li>, so both branches carry
            // their own \`flex: 1\` rather than needing a wrapper element.
            Html.show (
                isEditing,
                // Edit mode: a fresh input, focused once it is in the document.
                (fun () ->
                    Html.input
                        [
                            attr.className "todo-edit"
                            attr.type' "text"
                            attr.value todo.Label
                            attr.ref (fun el ->
                                window.setTimeout ((fun () -> el.focus ()), 0) |> ignore
                            )
                            on.keyDown (fun e ->
                                let input = e.target :?> HTMLInputElement

                                match e.key with
                                | "Enter" -> commit input
                                | "Escape" -> isEditing.Value <- false
                                | _ -> ()
                            )
                            on.blur (fun e -> commit (e.target :?> HTMLInputElement))
                        ]
                ),
                // Read mode: double-click to edit; strike through when done.
                (fun () ->
                    Html.span
                        [
                            attr.className "todo-text"
                            attr.classList (fun () -> [ "is-done", isDone.Value ])
                            attr.title "Double-click to edit"
                            on.dblClick (fun _ -> isEditing.Value <- true)
                            Html.text todo.Label
                        ]
                )
            )

            Html.button
                [
                    attr.className "icon-button"
                    attr.aria ("label", "Remove")
                    on.click (fun _ -> onRemove ())
                    Html.text "✕"
                ]
        ]

let render () =
    let entries = Var.create ([]: TodoEntry list)
    let nextId = Var.create 0
    let draft = Var.create ""

    let addTodo () =
        let label = draft.Value.Trim()

        if label <> "" then
            Signal.batch (fun () ->
                let id = nextId.Value

                entries.Value <-
                    entries.Value
                    @ [
                        {
                            Id = id
                            Label = Var.create label
                        }
                    ]

                nextId.Value <- id + 1
                draft.Value <- ""
            )

    let removeTodo (id: int) =
        entries.Value <- entries.Value |> List.filter (fun e -> e.Id <> id)

    Html.div
        [
            attr.className "stack todo-app"
            Html.div
                [
                    attr.className "row todo-new"

                    Html.input
                        [
                            attr.type' "text"
                            attr.placeholder "What needs to be done?"
                            attr.aria ("label", "New todo")
                            attr.bindValue draft
                            on.keyDown (fun e ->
                                if e.key = "Enter" then
                                    addTodo ()
                            )
                        ]

                    Html.button
                        [
                            attr.className "primary"
                            on.click (fun _ -> addTodo ())
                            attr.disabled (fun () -> draft.Value.Trim() = "")
                            Html.text "Add"
                        ]
                ]

            Html.ul
                [
                    Html.each
                        (fun () -> entries.Value |> List.toArray)
                        (fun e -> e.Id)
                        (fun e -> todoItem e (fun () -> removeTodo e.Id))
                ]

            Html.p
                [
                    Html.small
                        [
                            Html.text (fun () ->
                                match List.length entries.Value with
                                | 0 -> "No items yet"
                                | 1 -> "1 item"
                                | n -> sprintf "%d items" n
                            )
                        ]
                ]
        ]
`;export{e as default};