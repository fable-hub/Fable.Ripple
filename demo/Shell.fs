module Demo.Shell

open Fable.Ripple.Dom

/// Assemble the shell. The demo views arrive as parameters rather than being
/// referenced directly, so this file compiles ahead of them and `Main.fs`
/// stays the only place that knows the route -> view table.
///
/// Demo and source are stacked rather than tabbed: reading the code while
/// using the demo was the whole point, and tabs made that a switching exercise.
///
/// No sidebar or appbar here: the demo mounts inside the docs site's own
/// page, and the site's navbar and sidebar are what carry that chrome now.
let render
    (title: unit -> string)
    (description: unit -> string)
    (demo: unit -> DomItem)
    (source: unit -> (string * string) option)
    // Supplied rather than built here, so this file still knows nothing about
    // where a listing comes from - the same reason the views are parameters.
    (codeAction: unit -> DomItem)
    : DomItem
    =
    Html.div
        [
            attr.className "content"

            // h2, not h1: the docs page already has one, above this mount point.
            Html.h2 [ Html.text title ]

            // What the example teaches. At this catalogue's size it is
            // the product, and it used to live only in code comments.
            Html.p
                [
                    attr.className "lede"
                    Html.text description
                ]

            Html.div
                [
                    attr.className "panel"

                    // The wrapper an example used to open with itself.
                    // Two reasons it lives here: `Html.dynamic` needs a
                    // single element to anchor on and an example returns
                    // a fragment, and the panel's vertical rhythm is the
                    // shell's business - so a listing can start at its
                    // own first control rather than at a container the
                    // reader has no use for.
                    Html.dynamic (fun () ->
                        Html.div
                            [
                                attr.className "example"
                                demo ()
                            ]
                    )
                ]

            // Rebuilt on route change; loading is memoised per file in
            // `Sources`, so revisiting a route costs nothing. Routes with
            // no listing (the home page) render nothing here.
            Html.dynamic (fun () ->
                match source () with
                | None -> Html.none
                | Some(name, code) ->
                    Html.div
                        [
                            attr.className "code-panel"

                            Html.div
                                [
                                    attr.className "code-name"
                                    Html.span [ Html.text name ]

                                    Html.span
                                        [
                                            Html.span
                                                [
                                                    attr.className "code-status"
                                                    Html.text "highlighting…"
                                                ]
                                            codeAction ()
                                        ]
                                ]

                            Html.div
                                [
                                    attr.className "code-body"

                                    Html.div
                                        [
                                            attr.className "code-lines"
                                            attr.aria ("hidden", "true")
                                            Html.text (
                                                Seq.init
                                                    (code.Split('\n').Length)
                                                    (fun i -> string (i + 1))
                                                |> String.concat "\n"
                                            )
                                        ]

                                    Demo.Highlight.fsharp code
                                ]
                        ]
            )
        ]
