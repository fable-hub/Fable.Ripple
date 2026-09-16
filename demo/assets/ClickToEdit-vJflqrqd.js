var e=`module Demo.Examples.Recipes.ClickToEdit

open Browser
open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

type private Field =
    {
        Label: string
        Value: Var<string>
    }

let private field label value =
    {
        Label = label
        Value = Var.create value
    }

let render () =
    let fields =
        [
            field "name" "Ada Lovelace"
            field "role" "Analyst"
            field "email" "ada@example.com"
        ]

    // Which field is being edited, and the in-progress text. One \`Var\` for the
    // mode rather than one per row: only one can be open at a time, and making
    // that unrepresentable is cheaper than keeping three booleans honest.
    let editing = Var.create (None: string option)
    let draft = Var.create ""
    let saves = Var.create 0

    let start (f: Field) =
        draft.Value <- f.Value.Value
        editing.Value <- Some f.Label

    let cancel () = editing.Value <- None

    let save (f: Field) =
        f.Value.Value <- draft.Value
        saves.Value <- saves.Value + 1
        editing.Value <- None

    let row (f: Field) =
        Html.li
            [
                attr.className "todo-item"

                Html.span
                    [
                        attr.className "probe-label"
                        Html.text f.Label
                    ]

                Html.show (
                    (fun () -> editing.Value = Some f.Label),

                    // EDITING
                    (fun () ->
                        Row.row
                            [
                                Html.input
                                    [
                                        attr.bindValue draft

                                        // Deferred: the element is detached here.
                                        attr.ref (fun el ->
                                            window.setTimeout (
                                                (fun () ->
                                                    el.focus ()
                                                    (el :?> HTMLInputElement).select ()
                                                ),
                                                0
                                            )
                                            |> ignore
                                        )

                                        on.keyDown (fun e ->
                                            match e.key with
                                            | "Enter" -> save f
                                            | "Escape" -> cancel ()
                                            | _ -> ()
                                        )
                                    ]

                                Button.primary ("Save", fun _ -> save f)

                                Button.action ("Cancel", fun _ -> cancel ())
                            ]
                    ),

                    // DISPLAYING
                    fun () ->
                        Html.button
                            [
                                attr.className "click-to-edit"
                                on.click (fun _ -> start f)
                                Html.text f.Value
                            ]
                )
            ]

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Click a value to edit it. Enter saves and Escape cancels. Cancelling needs no undo: the input holding your draft is simply thrown away."

            // demo-show
            Html.ul (fields |> List.map row)

            // demo-hide
            Row.row
                [
                    readout
                        "editing"
                        (fun () ->
                            match editing.Value with
                            | Some label -> label
                            | None -> "nothing"
                        )

                    readout "saves" (fun () -> string saves.Value)
                ]

        // demo-show
        ]
`;export{e as default};