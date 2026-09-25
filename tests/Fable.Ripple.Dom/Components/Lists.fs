module Fable.Ripple.Dom.Test.Components.Lists

open Fable.Ripple
open Fable.Ripple.Dom

type private Row =
    {
        Id: int
        Name: string
    }

let private keyedList () : DomItem =
    let rows =
        Var.create
            [|
                {
                    Id = 1
                    Name = "one"
                }
                {
                    Id = 2
                    Name = "two"
                }
                {
                    Id = 3
                    Name = "three"
                }
            |]

    let mutable nextId = 4
    let builds = Var.create 0
    let cleanups = Var.create 0

    let button (id: string) (onClick: unit -> unit) =
        Html.button
            [
                attr.id id
                on.click (fun _ -> onClick ())
                Html.text id
            ]

    Html.div
        [
            button "reverse" (fun () -> rows.Value <- Array.rev rows.Value)

            button
                "append"
                (fun () ->
                    rows.Value <-
                        Array.append
                            rows.Value
                            [|
                                {
                                    Id = nextId
                                    Name = "new"
                                }
                            |]

                    nextId <- nextId + 1
                )

            button "remove-first" (fun () -> rows.Value <- Array.tail rows.Value)

            button "clear" (fun () -> rows.Value <- [||])

            // Keys disjoint from every current row: the full-replace path.
            button
                "replace"
                (fun () ->
                    rows.Value <-
                        [|
                            {
                                Id = nextId
                                Name = "fresh"
                            }
                            {
                                Id = nextId + 1
                                Name = "fresher"
                            }
                        |]

                    nextId <- nextId + 2
                )

            Html.ul
                [
                    attr.id "list"

                    Html.each
                        (fun () -> rows.Value)
                        _.Id
                        (fun row ->
                            builds.Value <- builds.Peek() + 1
                            Signal.onCleanup (fun () -> cleanups.Value <- cleanups.Peek() + 1)

                            Html.li
                                [
                                    attr.custom ("data-id", string row.Id)
                                    Html.span [ Html.text row.Name ]
                                    Html.input []
                                ]
                        )
                ]

            Html.output
                [
                    attr.id "builds"
                    Html.text builds
                ]
            Html.output
                [
                    attr.id "cleanups"
                    Html.text cleanups
                ]
        ]

let private eachPosition () : DomItem =
    // Starts EMPTY, so the first rows arrive after the following sibling has
    // already been appended.
    let items = Var.create ([||]: string[])

    Html.div
        [
            attr.id "host"
            Html.span
                [
                    attr.className "before"
                    Html.text "BEFORE"
                ]
            Html.each
                (fun () -> items.Value)
                id
                (fun x ->
                    Html.span
                        [
                            attr.className "item"
                            Html.text x
                        ]
                )
            Html.span
                [
                    attr.className "after"
                    Html.text "AFTER"
                ]
            Html.button
                [
                    attr.id "fill"
                    on.click (fun _ ->
                        items.Value <-
                            [|
                                "a"
                                "b"
                            |]
                    )
                    Html.text "fill"
                ]
            Html.button
                [
                    attr.id "clear"
                    on.click (fun _ -> items.Value <- [||])
                    Html.text "clear"
                ]
        ]

let all: (string * (unit -> DomItem)) list =
    [
        "KeyedList", keyedList
        "EachPosition", eachPosition
    ]
