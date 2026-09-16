var e=`module Demo.Examples.Recipes.ModalDialog

open Browser
open Browser.Types
open Fable.Core.JsInterop
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// \`showModal()\` puts the dialog in the browser's top layer, above every stacking
// context on the page - so it escapes \`overflow: hidden\` and z-index without
// needing to escape the DOM tree. You also get, for free and correctly: a
// backdrop, Escape to close, focus moved into the dialog and RESTORED to
// whatever opened it, and the rest of the page marked inert for assistive
// technology. A portal gives you none of that.
//
// The one thing to get right is the gotcha from Rendering / attr.ref: \`ref\` runs
// while the element is detached, and calling \`showModal()\` on a detached
// \`<dialog>\` THROWS. The throw unwinds the subtree being built, so the failure
// looks like "part of my page vanished" rather than an error about dialogs.

let render () =
    let open' = Var.create false
    let result = Var.create "nothing yet"
    let focusReturned = Var.create ""

    let mutable dialogEl: HTMLElement option = None

    let close (answer: string) =
        result.Value <- answer
        dialogEl |> Option.iter (fun d -> d?close ())
        open'.Value <- false

        // The browser puts focus back where it was; read it to prove that.
        window.setTimeout (
            (fun () ->
                focusReturned.Value <-
                    match document.activeElement with
                    | null -> "(none)"
                    | el -> el.tagName.ToLower() + " \\"" + el.textContent.Trim() + "\\""
            ),
            0
        )
        |> ignore

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Open the dialog, then close it with Escape or by clicking outside. Both report \`dismissed\`, and focus goes back to the button that opened it."

            // demo-show
            Row.row
                [
                    Button.primary ("Delete the thing", fun _ -> open'.Value <- true)

                    // demo-hide
                    readout "result" (fun () -> result.Value)
                    readout "focus went back to" (fun () -> focusReturned.Value)
                // demo-show
                ]

            // Built when open, disposed when closed - so the dialog's own state
            // starts fresh every time, which is almost always what you want.
            Html.show (
                open',
                fun () ->
                    Html.dialog
                        [
                            attr.className "sheet-dialog"

                            attr.ref (fun el ->
                                dialogEl <- Some el
                                // Deferred: showModal on a detached dialog throws.
                                window.setTimeout ((fun () -> el?showModal ()), 0) |> ignore
                            )

                            // Escape closes it natively; this keeps our state in
                            // step. \`close\` has no named member, so it goes
                            // through \`on.event\` - the same escape hatch shape
                            // as \`attr.custom\`.
                            on.event (
                                "close",
                                fun _ ->
                                    if open'.Value then
                                        close "dismissed"
                            )

                            // Light dismiss, which \`showModal\` does NOT give
                            // you. The backdrop is painted by the dialog itself,
                            // so a click on it targets the dialog rather than
                            // anything outside - and comparing the pointer to
                            // the dialog's own rect separates "on the backdrop"
                            // from "on the dialog's padding".
                            //
                            // Both guards are load-bearing. The buttons inside
                            // close the dialog themselves, and their click then
                            // BUBBLES here - by which point the dialog measures
                            // 0x0, so a rect test alone reads it as outside and
                            // overwrites the real answer with "dismissed".
                            on.click (fun ev ->
                                dialogEl
                                |> Option.iter (fun el ->
                                    let onDialogItself =
                                        System.Object.ReferenceEquals(ev.target, el)

                                    let r = el.getBoundingClientRect ()

                                    let inside =
                                        ev.clientX >= r.left
                                        && ev.clientX <= r.right
                                        && ev.clientY >= r.top
                                        && ev.clientY <= r.bottom

                                    if open'.Value && onDialogItself && not inside then
                                        close "dismissed"
                                )
                            )

                            Html.h3 "Are you sure?"

                            Html.p
                                [
                                    Html.small
                                        [
                                            Html.text
                                                "This dialog is in the browser's top layer. Press Escape, or click outside it."
                                        ]
                                ]

                            Row.row
                                [
                                    Button.primary ("Delete", fun _ -> close "deleted")

                                    Button.action ("Cancel", fun _ -> close "cancelled")
                                ]
                        ]
            )

            // A clipping ancestor, to show the dialog is not confined by it.
            Html.div
                [
                    attr.className "clipper"
                    Html.p
                        [
                            Html.small
                                [
                                    Html.text
                                        "This box has overflow: hidden and a z-index. The dialog is not inside it - but it would escape it even if it were."
                                ]
                        ]
                ]

        ]
`;export{e as default};