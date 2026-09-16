module Demo.SevenGUIs.CircleDrawer

open Browser
open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom

// 7GUIs #6 - Circle Drawer. Following the task exactly:
//
//   - left-clicking an *empty* area creates an unfilled circle of fixed diameter
//   - the circle nearest the pointer whose centre is within its radius is filled
//     grey - selection is by HOVER, not by clicking
//   - right-clicking that circle opens a popup menu with one entry, "Adjust
//     diameter.."
//   - which opens a dialog holding a slider; changes apply immediately
//   - *closing the dialog* is what marks the diameter change as significant for
//     undo/redo, so a whole drag session is one history entry
//   - undo/redo cover both circle creation and diameter adjustment
type private Circle =
    {
        Id: int
        X: float
        Y: float
        R: float
    }

/// The fixed radius every circle is created with.
let private newRadius = 25.0

let render () =
    let circles = Var.create ([||]: Circle[])
    let hovered = Var.create (None: int option)
    // Position *and* the circle the menu was opened on. Capturing the target
    // here matters: the menu is a child of the canvas, so moving onto it fires
    // the canvas mousemove and `hovered` would slide off the circle before the
    // entry is ever clicked.
    let menuAt = Var.create (None: (float * float * int) option)
    let adjusting = Var.create (None: int option)
    let undo = Var.create ([]: Circle[] list)
    let redo = Var.create ([]: Circle[] list)
    let nextId = ref 1

    // Circle set as the adjust dialog opened, and the dialog element itself.
    // Both are plain refs, written from event handlers - nothing renders from
    // them, and reads there are outside any tracking scope.
    let beforeAdjust = ref ([||]: Circle[])
    let dialogEl = ref (None: HTMLDialogElement option)
    // Where the frame should open: the point that was right-clicked, so it
    // appears under the pointer rather than making you travel to it.
    let adjustAt = ref (0.0, 0.0)

    let pushHistory (before: Circle[]) =
        undo.Value <- before :: undo.Value
        redo.Value <- []

    /// Nearest circle containing the point, per the task's wording.
    let hitTest x y =
        circles.Value
        |> Array.filter (fun c ->
            let dx = c.X - x
            let dy = c.Y - y
            sqrt (dx * dx + dy * dy) <= c.R
        )
        |> Array.sortBy (fun c ->
            let dx = c.X - x
            let dy = c.Y - y
            dx * dx + dy * dy
        )
        |> Array.tryHead

    let localXY (e: MouseEvent) =
        let rect = (e.currentTarget :?> HTMLElement).getBoundingClientRect ()
        e.clientX - rect.left, e.clientY - rect.top

    let onMove (e: MouseEvent) =
        // While the dialog is open the target stays put, so it cannot slip away
        // under the pointer mid-adjustment.
        if (adjusting.Value).IsNone then
            let x, y = localXY e
            hovered.Value <- hitTest x y |> Option.map (fun c -> c.Id)

    let onLeave (_: MouseEvent) =
        if (adjusting.Value).IsNone then
            hovered.Value <- None

    let onClick (e: MouseEvent) =
        menuAt.Value <- None
        let x, y = localXY e

        // Only an empty area creates a circle; clicking an existing one does
        // nothing, since selection is by proximity. The dialog is non-modal, so
        // the canvas has to decline input itself while it is open.
        if (adjusting.Value).IsNone && (hitTest x y).IsNone then
            pushHistory circles.Value

            circles.Value <-
                Array.append
                    circles.Value
                    [|
                        {
                            Id = nextId.Value
                            X = x
                            Y = y
                            R = newRadius
                        }
                    |]

            hovered.Value <- Some nextId.Value
            nextId.Value <- nextId.Value + 1

    let onContextMenu (e: MouseEvent) =
        e.preventDefault ()

        if (adjusting.Value).IsNone then
            let x, y = localXY e

            match hitTest x y with
            | Some c ->
                hovered.Value <- Some c.Id
                menuAt.Value <- Some(x, y, c.Id)
            | None -> menuAt.Value <- None

    let openAdjust () =
        match menuAt.Value with
        | Some(x, y, id) ->
            beforeAdjust.Value <- circles.Value
            adjustAt.Value <- (x, y)
            hovered.Value <- Some id
            adjusting.Value <- Some id
            menuAt.Value <- None
        | None -> ()

    let requestClose () =
        dialogEl.Value |> Option.iter (fun d -> d.close ())

    let closeAdjust () =
        // Closing the frame is what marks the last diameter as significant, so
        // an entire drag session collapses into one undo entry.
        if not (System.Object.ReferenceEquals(beforeAdjust.Value, circles.Value)) then
            pushHistory beforeAdjust.Value

        adjusting.Value <- None

    let setRadius (r: float) =
        match adjusting.Value with
        | Some id ->
            circles.Value <-
                circles.Value
                |> Array.map (fun c ->
                    if c.Id = id then
                        { c with
                            R = r
                        }
                    else
                        c
                )
        | None -> ()

    let adjustingRadius () =
        match adjusting.Value with
        | Some id ->
            circles.Value
            |> Array.tryFind (fun c -> c.Id = id)
            |> Option.map (fun c -> c.R)
            |> Option.defaultValue newRadius
        | None -> newRadius

    let undoAction () =
        match undo.Value with
        | prev :: rest ->
            redo.Value <- circles.Value :: redo.Value
            circles.Value <- prev
            undo.Value <- rest
        | [] -> ()

    let redoAction () =
        match redo.Value with
        | next :: rest ->
            undo.Value <- circles.Value :: undo.Value
            circles.Value <- next
            redo.Value <- rest
        | [] -> ()

    Html.div
        [
            attr.className "stack"

            Html.div
                [
                    attr.className "row"

                    Html.button
                        [
                            on.click (fun _ -> undoAction ())
                            attr.disabled (fun () -> List.isEmpty undo.Value)
                            Html.text "Undo"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> redoAction ())
                            attr.disabled (fun () -> List.isEmpty redo.Value)
                            Html.text "Redo"
                        ]
                ]

            Html.div
                [
                    attr.className "canvas"
                    on.click onClick
                    on.mouseMove onMove
                    on.mouseLeave onLeave
                    on.contextMenu onContextMenu

                    Html.each
                        (fun () -> circles.Value)
                        (fun c -> c.Id)
                        (fun c ->
                            Html.div
                                [
                                    attr.className "circle"
                                    attr.classList (fun () ->
                                        [
                                            "is-filled",
                                            match adjusting.Value with
                                            | Some id -> id = c.Id
                                            | None -> hovered.Value = Some c.Id
                                        ]
                                    )
                                    // Geometry only - appearance lives in .circle.
                                    attr.style (fun () ->
                                        let cur =
                                            circles.Value
                                            |> Array.tryFind (fun x -> x.Id = c.Id)
                                            |> Option.defaultValue c

                                        sprintf
                                            "left:%fpx;top:%fpx;width:%fpx;height:%fpx;margin-left:%fpx;margin-top:%fpx"
                                            cur.X
                                            cur.Y
                                            (cur.R * 2.0)
                                            (cur.R * 2.0)
                                            (-cur.R)
                                            (-cur.R)
                                    )
                                ]
                        )

                    // Dims the drawing area while the frame is open, so it reads
                    // as taking priority. Clicking it closes, like a modal
                    // backdrop - which a non-modal dialog does not provide.
                    Html.div
                        [
                            attr.className "canvas-scrim"
                            attr.hidden (fun () -> (adjusting.Value).IsNone)
                            on.click (fun e ->
                                e.stopPropagation ()
                                requestClose ()
                            )
                        ]

                    Html.show (
                        (fun () -> (menuAt.Value).IsSome),
                        fun () ->
                            let x, y, _ = (menuAt.Value) |> Option.defaultValue (0.0, 0.0, 0)

                            Html.div
                                [
                                    attr.className "context-menu"
                                    attr.style (sprintf "left:%fpx;top:%fpx" x y)
                                    // Neither may reach the canvas: a click there
                                    // would test for a new circle, and a move
                                    // would drag the highlight off the target.
                                    on.click (fun e -> e.stopPropagation ())
                                    on.mouseMove (fun e -> e.stopPropagation ())

                                    Html.button
                                        [
                                            on.click (fun _ -> openAdjust ())
                                            Html.text "Adjust diameter.."
                                        ]
                                ]
                    )

                    // A real <dialog>, as the task's "dialog control" challenge
                    // asks. It lives inside the canvas and is opened non-modally
                    // so it can centre on the drawing area rather than on the
                    // viewport; the canvas declines input while it is open.
                    //
                    // Nothing in this branch reads `circles` at build time - the
                    // slider's value is a reactive property - so dragging it
                    // never rebuilds the element.
                    Html.show (
                        (fun () -> (adjusting.Value).IsSome),
                        fun () ->
                            Html.dialog
                                [
                                    attr.className "adjust-dialog"
                                    on.click (fun e -> e.stopPropagation ())
                                    on.mouseMove (fun e -> e.stopPropagation ())
                                    on.contextMenu (fun e -> e.stopPropagation ())
                                    // A non-modal dialog does not close on Escape
                                    // by itself, so wire it up.
                                    on.keyDown (fun e ->
                                        if e.key = "Escape" then
                                            requestClose ()
                                    )
                                    // `ref` fires while the element is still detached,
                                    // and show() throws on a node outside the
                                    // document - so defer it by a tick, as Todos does
                                    // for autofocus. The same tick is where the
                                    // frame can finally be measured and clamped.
                                    attr.ref (fun el ->
                                        let d = el :?> HTMLDialogElement
                                        dialogEl.Value <- Some d

                                        window.setTimeout (
                                            (fun () ->
                                                d.show ()

                                                // Anchor to the pointer, then pull
                                                // back inside the canvas - near an
                                                // edge it would otherwise be clipped.
                                                match d.parentElement with
                                                | null -> ()
                                                | parent ->
                                                    let pr = parent.getBoundingClientRect ()
                                                    let dr = d.getBoundingClientRect ()
                                                    let x, y = adjustAt.Value
                                                    let pad = 8.0

                                                    let clamp v limit =
                                                        max pad (min v (limit - pad))

                                                    d.style.left <-
                                                        sprintf
                                                            "%fpx"
                                                            (clamp x (pr.width - dr.width))

                                                    d.style.top <-
                                                        sprintf
                                                            "%fpx"
                                                            (clamp y (pr.height - dr.height))
                                            ),
                                            0
                                        )
                                        |> ignore
                                    )
                                    // Fires for the Close button and for Escape alike.
                                    on.event ("close", fun _ -> closeAdjust ())

                                    Html.p [ Html.small "Adjust diameter.." ]

                                    Html.input
                                        [
                                            attr.type' "range"
                                            attr.min "5"
                                            attr.max "150"
                                            attr.prop ("value", fun () -> box (adjustingRadius ()))
                                            on.input (fun v -> setRadius (float v))
                                        ]

                                    Html.button
                                        [
                                            attr.className "primary"
                                            on.click (fun _ -> requestClose ())
                                            Html.text "Close"
                                        ]
                                ]
                    )
                ]

            // Not part of the task - our own affordance, because right-click is
            // not a discoverable gesture. Always visible.
            Html.small "click to add a circle, then right-click it to adjust"
        ]
