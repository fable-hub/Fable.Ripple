module Demo.Examples.Routing.PathParameters

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Fable.UrlParser.UrlCodec
open Demo.Examples.Widgets // demo-hide-line

// Segments come typed. `Tuple.string` takes one as text, `Tuple.int` insists it
// is an integer, and `Tuple.custom` takes your own conversion - a `tryParse`
// that returns `None` for anything it does not recognise, plus the `toString`
// that puts it back. `Tuple.fragment` picks up the trailing `#part`.
//
// A failed parse is a `Result` carrying the error from whichever codec got
// FURTHEST, not a blank "no match".
//
// Percent-encoding is handled for you in both directions, so a slug with a
// space survives the round trip.

type private Level =
    | Low
    | High

module private Level =
    let tryParse =
        function
        | "low" -> Some Low
        | "high" -> Some High
        | _ -> None

    let toString =
        function
        | Low -> "low"
        | High -> "high"

type private Route =
    | Doc of folder: string * page: int
    | Task of level: Level
    | Article of slug: string * section: string option

module private Prisms =
    let doc: Prism<Route, string * int> =
        {
            Embed = Doc
            Project =
                function
                | Doc(f, p) -> Some(f, p)
                | _ -> None
        }

    let task: Prism<Route, Level> =
        {
            Embed = Task
            Project =
                function
                | Task l -> Some l
                | _ -> None
        }

    let article: Prism<Route, string * string option> =
        {
            Embed = Article
            Project =
                function
                | Article(s, sec) -> Some(s, sec)
                | _ -> None
        }

let private codecs =
    [
        // Two typed segments with a literal between them.
        Tuple.path "doc"
        |> Tuple.string
        |> Tuple.segment "page"
        |> Tuple.int
        |> Tuple.ofCase2 Prisms.doc

        // A user-supplied conversion, named so the error can mention it.
        Tuple.path "priority"
        |> Tuple.custom "Level" Level.tryParse Level.toString
        |> Tuple.ofCase1 Prisms.task

        Tuple.path "article"
        |> Tuple.string
        |> Tuple.fragment
        |> Tuple.ofCase2 Prisms.article
    ]

let render () =
    let url = Var.create "#/doc/guides/page/3"

    let samples =
        [
            "#/doc/guides/page/3", "two typed segments"
            "#/doc/guides/page/three", "int segment, not an int"
            "#/priority/high", "a custom conversion"
            "#/priority/urgent", "custom conversion says no"
            "#/article/hello%20world", "a percent-encoded slug"
            "#/article/hello%20world#notes", "and a fragment"
            "#/doc/guides", "too few segments"
        ]

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press each sample, including the two that fail. The errors say which part did not match: a segment that is not a number, or a value the conversion rejected. `rebuilt from the route` runs the same codec backwards."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    for value, label in samples do
                        Html.button
                            [
                                attr.custom (
                                    "aria-current",
                                    fun () ->
                                        if url.Value = value then
                                            "true"
                                        else
                                            "false"
                                )
                                on.click (fun _ -> url.Value <- value)
                                Html.text label
                            ]
                ]

            Html.label
                [
                    Html.text "url"
                    Html.input [ attr.bindValue url ]
                ]

            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "parsed"

                            Html.output
                                [
                                    Html.text (fun () ->
                                        match RouteCodec.tryParseHash codecs url.Value with
                                        | Ok route -> sprintf "%A" route
                                        | Error message -> "Error: " + message
                                    )
                                ]
                        ]

                    Html.label
                        [
                            Html.text "rebuilt from the route"

                            Html.output
                                [
                                    Html.text (fun () ->
                                        match RouteCodec.tryParseHash codecs url.Value with
                                        | Ok route ->
                                            RouteCodec.tryToHash codecs route
                                            |> Option.defaultValue "(no codec builds it)"
                                        | Error _ -> "-"
                                    )
                                ]
                        ]
                ]
        ]
