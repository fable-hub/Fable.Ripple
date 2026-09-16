module Demo.Examples.Routing.UrlState

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// Notice what is absent: there is no local `Var` holding the search text and no
// effect keeping it in step with the URL. That two-way sync is the bug factory
// this design removes - `current ()` reads the route, `navigate` writes it, and
// there is only one copy of the truth.
//
// Two ways to write it, and the difference is the back button:
//   navigate  -> pushes a history entry (Back returns to the previous query)
//   replace   -> rewrites the current one (Back leaves the example entirely)
// Paging pushes; typing replaces, so a search does not bury the back button
// under one entry per keystroke.

type ProductQuery =
    {
        Search: string option
        Page: int option
        Sort: string option
    }

module ProductQuery =
    let empty =
        {
            Search = None
            Page = None
            Sort = None
        }

// Supplied by Main.fs, which is where the router lives - this file is compiled
// before it. In an application the router would simply be in scope; the demo
// has to inject it because the catalogue that builds the routes is downstream.
let mutable internal current: unit -> ProductQuery = fun () -> ProductQuery.empty
let mutable internal navigate: ProductQuery -> unit = ignore
let mutable internal replace: ProductQuery -> unit = ignore
let mutable internal back: unit -> unit = ignore

let internal connect c n r b =
    current <- c
    navigate <- n
    replace <- r
    back <- b

let private catalogue =
    [
        "Reactive signals", "library", 12
        "Signal combinators", "library", 30
        "Keyed list rendering", "rendering", 8
        "URL codecs", "routing", 21
        "Scope disposal", "library", 5
        "Attribute bindings", "rendering", 17
        "Hash router", "routing", 3
        "Equality cutoff", "library", 26
    ]

let private pageSize = 3

let render () =
    // Every read below goes through `current ()`, which reads the router's
    // route signal - so these bindings are subscribed to the URL itself.
    let search () =
        current().Search |> Option.defaultValue ""

    let page () = current().Page |> Option.defaultValue 1

    let sort () =
        current().Sort |> Option.defaultValue "name"

    let matching () =
        let needle = (search ()).ToLower()

        catalogue
        |> List.filter (fun (name, tag, _) ->
            needle = "" || name.ToLower().Contains needle || tag.Contains needle
        )
        |> List.sortBy (fun (name, _, hits) ->
            match sort () with
            | "hits" -> string (1000 - hits)
            | _ -> name
        )

    let pageCount () =
        max 1 ((List.length (matching ()) + pageSize - 1) / pageSize)

    let visible () =
        matching ()
        |> List.skip (min (List.length (matching ())) ((page () - 1) * pageSize))
        |> List.truncate pageSize

    let withSearch text =
        // Typing REPLACES, so the back button is not buried per keystroke. Page
        // resets to 1, which is a real decision the URL now records.
        replace
            { current () with
                Search =
                    if text = "" then
                        None
                    else
                        Some text
                Page = None
            }

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Search, change page and sort, and watch the address bar follow. Reload and nothing is lost. Back steps through the pages, not through every keystroke."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "search "

                            Html.input
                                [
                                    attr.value (fun () -> search ())
                                    on.input (fun (v: string) -> withSearch v)
                                ]
                        ]

                    Html.label
                        [
                            Html.text "sort "

                            Html.select
                                [
                                    attr.value (fun () -> sort ())

                                    on.change (fun (v: string) ->
                                        navigate
                                            { current () with
                                                Sort = Some v
                                            }
                                    )

                                    Html.option
                                        [
                                            attr.value "name"
                                            Html.text "name"
                                        ]

                                    Html.option
                                        [
                                            attr.value "hits"
                                            Html.text "hits"
                                        ]
                                ]
                        ]

                    Html.button
                        [
                            attr.disabled (fun () -> page () <= 1)
                            on.click (fun _ ->
                                navigate
                                    { current () with
                                        Page = Some(page () - 1)
                                    }
                            )
                            Html.text "Previous"
                        ]

                    Html.button
                        [
                            attr.disabled (fun () -> page () >= pageCount ())
                            on.click (fun _ ->
                                navigate
                                    { current () with
                                        Page = Some(page () + 1)
                                    }
                            )
                            Html.text "Next"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> back ())
                            Html.text "Back"
                        ]

                    Html.button
                        [
                            on.click (fun _ -> navigate ProductQuery.empty)
                            Html.text "Clear"
                        ]
                ]

            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "page"

                            Html.output
                                [
                                    Html.text (fun () ->
                                        sprintf "%d of %d" (page ()) (pageCount ())
                                    )
                                ]
                        ]

                    Html.label
                        [
                            Html.text "matches"
                            Html.output [ Html.text (fun () -> string (List.length (matching ()))) ]
                        ]

                    Html.label
                        [
                            Html.text "sort"
                            Html.output [ Html.text (fun () -> sort ()) ]
                        ]
                ]

            Html.ul
                [
                    attr.className "moves-list"

                    Html.each
                        (fun () -> visible () |> List.toArray)
                        (fun (name, _, _) -> name)
                        (fun (name, tag, hits) ->
                            Html.li [ Html.text (sprintf "%s  [%s]  %d" name tag hits) ]
                        )
                ]

        ]
