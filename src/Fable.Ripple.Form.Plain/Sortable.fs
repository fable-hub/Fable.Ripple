namespace Fable.Ripple.Form.Plain

open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form

/// A list renderer with drag and drop: rows reorder by dragging their handle, and a drag source
/// created with `Sortable.source` drops a new item at the pointer's position.
module Sortable =

    [<NoComparison; NoEquality>]
    type Config<'Item> =
        {
            Items: Var<'Item list>
            /// Builds the item for a source payload, or `None` to ignore the drop.
            Create: string -> 'Item option
            /// Tracked read.
            Title: 'Item -> string
            Empty: string
        }

    let mutable private nextListId = 0

    /// Marks an element as a drag source carrying `payload` for `Sortable.list`.
    let source (payload: string) : DomItem =
        Html.fragment
            [
                attr.draggable true
                on.dragStart (fun ev ->
                    ev.dataTransfer.setData ("text/plain", "new:" + payload) |> ignore
                    ev.dataTransfer.effectAllowed <- "copy"
                )
            ]

    let list (config: Config<'Item>) : RenderContext -> FormList.RenderConfig -> DomItem =
        fun context renderConfig ->
            nextListId <- nextListId + 1
            let listId = nextListId
            let dropAt: Var<(int * bool) option> = Var.create None
            let mutable canvas: HTMLElement option = None

            let keys () =
                renderConfig.Elements() |> Array.map (fun element -> element.Key)

            let indexOf (key: int) = keys () |> Array.findIndex ((=) key)

            let insertAt (index: int) (item: 'Item) =
                let items = config.Items.Peek()
                let index = max 0 (min index (List.length items))
                config.Items.Value <- List.take index items @ [ item ] @ List.skip index items

            let move (from: int) (target: int) =
                let items = config.Items.Peek()
                let item = List.item from items

                let without =
                    items
                    |> List.indexed
                    |> List.filter (fun (index, _) -> index <> from)
                    |> List.map snd

                let target =
                    if from < target then
                        target - 1
                    else
                        target

                let target = max 0 (min target (List.length without))
                config.Items.Value <- List.take target without @ [ item ] @ List.skip target without

            /// The row under or nearest to the pointer, and whether the pointer is in its top half.
            let targetAt (clientY: float) : (int * bool) option =
                canvas
                |> Option.bind (fun canvas ->
                    let rows = canvas.querySelectorAll ":scope > .rf-sortable__row"

                    let rects =
                        [
                            for i in 0 .. rows.length - 1 do
                                let row = rows.[i] :?> HTMLElement
                                int (row.dataset.["key"]), row.getBoundingClientRect ()
                        ]

                    match rects with
                    | [] -> None
                    | _ ->
                        rects
                        |> List.tryFind (fun (_, rect) -> clientY < rect.top + rect.height / 2.0)
                        |> Option.map (fun (key, _) -> key, true)
                        |> Option.orElse (Some(fst (List.last rects), false))
                )

            let dropIndex () =
                match dropAt.Peek() with
                | Some(key, before) ->
                    if before then
                        indexOf key
                    else
                        indexOf key + 1
                | None -> List.length (config.Items.Peek())

            let handleDrop (ev: DragEvent) =
                ev.preventDefault ()
                ev.stopPropagation ()
                let data = ev.dataTransfer.getData "text/plain"
                let index = dropIndex ()
                dropAt.Value <- None

                if data.StartsWith "new:" then
                    config.Create(data.Substring 4) |> Option.iter (insertAt index)
                elif data.StartsWith $"row:{listId}:" then
                    move (int (data.Substring($"row:{listId}:".Length))) index

            let row (element: FormList.Element) =
                let index () = indexOf element.Key
                let item = List.item (index ()) (config.Items.Peek())

                Html.div
                    [
                        attr.className "rf-sortable__row"
                        attr.custom ("data-key", string element.Key)
                        attr.toggleClass (
                            "rf-sortable__row--before",
                            (fun () -> dropAt.Value = Some(element.Key, true))
                        )
                        attr.toggleClass (
                            "rf-sortable__row--after",
                            (fun () -> dropAt.Value = Some(element.Key, false))
                        )

                        Html.div
                            [
                                attr.className "rf-sortable__bar"
                                Html.span
                                    [
                                        attr.className "rf-sortable__handle"
                                        attr.title "Drag to reorder"
                                        attr.draggable true
                                        on.dragStart (fun ev ->
                                            ev.dataTransfer.setData (
                                                "text/plain",
                                                $"row:{listId}:{index ()}"
                                            )
                                            |> ignore

                                            ev.dataTransfer.effectAllowed <- "move"
                                        )
                                        on.dragEnd (fun _ -> dropAt.Value <- None)
                                        Html.text "⠿"
                                    ]
                                Html.span
                                    [
                                        attr.className "rf-sortable__title"
                                        Html.text (fun () -> config.Title item)
                                    ]
                                Html.span [ attr.className "rf-sortable__spacer" ]
                                Html.button
                                    [
                                        attr.type' "button"
                                        attr.className "rf-sortable__button"
                                        attr.title "Move up"
                                        on.click (fun _ ->
                                            if index () > 0 then
                                                move (index ()) (index () - 1)
                                        )
                                        Html.text "↑"
                                    ]
                                Html.button
                                    [
                                        attr.type' "button"
                                        attr.className "rf-sortable__button"
                                        attr.title "Move down"
                                        on.click (fun _ -> move (index ()) (index () + 2))
                                        Html.text "↓"
                                    ]
                                Html.button
                                    [
                                        attr.type' "button"
                                        attr.className
                                            "rf-sortable__button rf-sortable__button--danger"
                                        attr.title "Remove"
                                        on.click (fun _ -> element.Delete())
                                        Html.text "✕"
                                    ]
                            ]
                        Html.div
                            [
                                attr.className "rf-sortable__body"
                                yield! context.RenderItems context element.Items
                            ]
                    ]

            Html.div
                [
                    attr.className "rf-sortable"
                    attr.ref (fun element -> canvas <- Some element)

                    on.dragOver (fun ev ->
                        ev.preventDefault ()
                        ev.stopPropagation ()
                        dropAt.Value <- targetAt ev.clientY
                    )

                    on.dragLeave (fun ev ->
                        // Leaving for a descendant still counts as inside.
                        match canvas, ev.relatedTarget with
                        | Some canvas, related when
                            not (isNull related) && canvas.contains (related :?> Node)
                            ->
                            ()
                        | _ -> dropAt.Value <- None
                    )

                    on.drop handleDrop
                    Html.each renderConfig.Elements (fun element -> element.Key) row

                    Html.show (
                        (fun () -> Array.isEmpty (renderConfig.Elements())),
                        fun () ->
                            Html.p
                                [
                                    attr.className "rf-sortable__empty"
                                    Html.text config.Empty
                                ]
                    )
                ]
