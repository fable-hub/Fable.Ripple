var e=`module Demo.Examples.Rendering.Refs

open Browser
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// \`attr.ref\` is also this library's \`onMount\`, and it runs inside the enclosing
// scope, so a \`Signal.onCleanup\` registered here tears down with the element.
//
// It fires while the element is being BUILT, before it is in the document, so
// \`el.isConnected\` is false and anything needing document membership fails
// quietly:
//
//   - \`focus()\` does nothing
//   - \`getBoundingClientRect()\` returns all zeros
//   - \`dialog.showModal()\` THROWS

/// \`isConnected\` is not in Fable's DOM bindings; it is the standard property
/// that answers "is this node in the document yet".
let private isConnected (el: HTMLElement) : bool = emitJsExpr el "$0.isConnected"

let render () =
    let naive = Var.create "not run yet"
    let deferred = Var.create "not run yet"

    let report (el: HTMLElement) =
        el.focus ()
        let box = el.getBoundingClientRect ()

        sprintf
            "isConnected=%b  width=%.0f  focused=%b"
            (isConnected el)
            box.width
            (obj.ReferenceEquals(document.activeElement, el))

    Html.fragment
        [
            // demo-hide
            Try.observe
                "No clicking needed. The left column measured the element straight from \`attr.ref\`, before it was on the page, so it reads zero. The right column waited one tick and gets real numbers."

            // demo-show
            Html.div
                [
                    Html.article
                        [
                            Html.header "straight from attr.ref"

                            Html.p
                                [
                                    Html.small
                                        "The element exists but is not laid out yet, so every measurement reads zero."
                                ]

                            Html.input
                                [
                                    attr.value "naive"
                                    attr.ref (fun el -> naive.Value <- report el)
                                ]

                            readout "saw" (fun () -> naive.Value)
                        ]

                    Html.article
                        [
                            Html.header "deferred one macrotask"

                            Html.p
                                [
                                    Html.small
                                        "Measured after the browser has laid the page out. The numbers are real."
                                ]

                            Html.input
                                [
                                    attr.value "deferred"

                                    attr.ref (fun el ->
                                        window.setTimeout (
                                            (fun () -> deferred.Value <- report el),
                                            0
                                        )
                                        |> ignore
                                    )
                                ]

                            readout "saw" (fun () -> deferred.Value)
                        ]

                ]

            // demo-hide
            Try.outcome
                "Focusing, measuring and \`showModal\` all need that tick. Todos and Circle Drawer use the same one-line fix."
        // demo-show
        ]
`;export{e as default};