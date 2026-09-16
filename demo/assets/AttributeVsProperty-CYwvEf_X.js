var e=`module Demo.Examples.Rendering.AttributeVsProperty

open Browser
open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// The library keeps the two apart:
//
//   attr.custom ("value", ...)   -> setAttribute - the markup
//   attr.value / attr.prop       -> element.value - the live state
//
// The named members that matter (\`value\`, \`checked'\`, \`disabled\`) are already
// properties, so the default is the right one. You only meet this by reaching
// for \`custom\` with a name that happens to be a form property.

let render () =
    let text = Var.create "start"

    let attrMarkup = Var.create ""
    let propMarkup = Var.create ""

    let mutable attrEl: HTMLInputElement option = None
    let mutable propEl: HTMLInputElement option = None

    let describe (el: HTMLInputElement) =
        sprintf "attribute=%s  property=%s" (el.getAttribute "value") el.value

    // Deferred by one macrotask on purpose. Handler bodies are batched, so
    // the attribute and property bindings have not run yet while the handler is
    // still executing - reading the DOM here would report the PREVIOUS state.
    let refresh () =
        window.setTimeout (
            (fun () ->
                attrEl |> Option.iter (fun e -> attrMarkup.Value <- describe e)
                propEl |> Option.iter (fun e -> propMarkup.Value <- describe e)
            ),
            0
        )
        |> ignore

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Type in both boxes, then press \`Reset both\`. Only the property-bound box resets: once the user edits a field, the browser stops copying its \`value\` attribute into what it shows."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ ->
                                text.Value <- "reset"
                                refresh ()
                            )
                            Html.text "Reset both to \\"reset\\""
                        ]

                    Html.button
                        [
                            on.click (fun _ -> refresh ())
                            Html.text "Re-read both elements"
                        ]

                    // demo-hide
                    readout "the Var" (fun () -> text.Value)
                // demo-show
                ]

            Html.div
                [
                    Html.article
                        [
                            Html.header "attr.custom (\\"value\\", ...)"

                            Html.p
                                [
                                    Html.small
                                        "Sets the ATTRIBUTE. It seeds the field once, then stops tracking what the user typed."
                                ]

                            Html.input
                                [
                                    attr.custom ("value", fun () -> text.Value)
                                    on.input (fun (_: string) -> refresh ())
                                    attr.ref (fun e -> attrEl <- Some(e :?> HTMLInputElement))
                                ]

                            readout "reads back as" (fun () -> attrMarkup.Value)
                        ]

                    Html.article
                        [
                            Html.header "attr.value"

                            Html.p
                                [
                                    Html.small
                                        "Sets the PROPERTY - the value the field actually holds."
                                ]

                            Html.input
                                [
                                    attr.value text
                                    on.input (fun (_: string) -> refresh ())
                                    attr.ref (fun e -> propEl <- Some(e :?> HTMLInputElement))
                                ]

                            readout "reads back as" (fun () -> propMarkup.Value)
                        ]

                ]

        ]
`;export{e as default};