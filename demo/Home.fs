module Demo.Home

open Fable.Ripple.Dom
open Demo.Router
open Demo.Catalogue

// The catalogue, at `#/examples`. One card per section, each listing its
// examples with the one line that says what it teaches. Derived from
// `Catalogue.catalogue`, so a new section appears here for free.
//
// Not the landing page: `#/` redirects to the first example, because readers
// arrive from the docs rather than from the front door.

let private sectionCard (section: Section) : DomItem =
    Html.div
        [
            attr.className "home-section"

            Html.h2 [ Html.text section.Title ]

            Html.ul
                [
                    attr.className "home-list"

                    for e in section.Entries do
                        Html.li
                            [
                                Html.a
                                    [
                                        attr.href (router.Href e.Route)
                                        Html.text e.Title
                                    ]
                                Html.p [ Html.small [ Html.text e.Description ] ]
                            ]
                ]
        ]

let render () =
    Html.div
        [
            attr.className "home"

            Html.p
                [
                    Html.small
                        [
                            Html.text (
                                sprintf
                                    "%d examples in %d sections. Each is a working page, with the source that produced it underneath."
                                    (List.length all)
                                    (List.length catalogue)
                            )
                        ]
                ]

            Html.div
                [
                    attr.className "home-grid"
                    Html.fragment (catalogue |> List.map sectionCard)
                ]
        ]
