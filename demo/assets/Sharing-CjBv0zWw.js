var e=`module Demo.Examples.Signals.Sharing

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line

/// Module-level, so it belongs to the module rather than to any page - and any
/// other file that opened this one would read the very same \`Var\`.
let private theme = Var.create "light"

/// Never resets. A theme still dark when you come back could be a default; a
/// count still climbing cannot be.
let private uses = Var.create 0

/// The read-only view. \`Signal<'T>\` has a getter and no setter, so this is what
/// you hand to anything that should read the theme but never write it.
let private currentTheme: Signal<string> = theme.Signal

let render () =
    Html.fragment
        [
            // demo-hide
            Try.observe
                "Toggle the theme, open another example and come back. The theme and \`writes\` are where you left them, because the Var lives in a module, not in the page."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ ->
                                theme.Value <-
                                    if theme.Value = "light" then
                                        "dark"
                                    else
                                        "light"

                                uses.Value <- uses.Value + 1
                            )

                            Html.text "Toggle the shared theme"
                        ]

                    Html.label
                        [
                            Html.text "theme"
                            Html.output currentTheme
                        ]

                    Html.label
                        [
                            Html.text "writes"
                            Html.output uses
                        ]
                ]
        ]
`;export{e as default};