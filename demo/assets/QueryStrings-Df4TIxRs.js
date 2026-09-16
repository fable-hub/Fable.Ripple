var e=`module Demo.Examples.Routing.QueryStrings

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Fable.UrlParser.UrlCodec
open Demo.Examples.Widgets // demo-hide-line

// Query parameters are part of the codec, so they round-trip with everything
// else. Four kinds, all shown below:
//
//   Query.flag "draft"            present or absent, as a bool
//   Query.strings "tag"           a REPEATED key, as a list
//   Query.Optional.int "page"     zero or one, as an option
//   Query.Required.string "q"     exactly one, or the parse fails
//
// An unticked flag and an empty list VANISH rather than becoming \`draft=false\`
// or \`tag=\`. That matters because a URL is compared as a string by caches,
// analytics and the back button, so the empty state needs exactly one spelling.

type private Route = Search of query: string * tags: string list * page: int option * draft: bool

module private Prisms =
    let search: Prism<Route, string * string list * int option * bool> =
        {
            Embed = Search
            Project =
                function
                | Search(q, t, p, d) -> Some(q, t, p, d)
        }

// Four values is past \`ofCase3\`, so the accumulated tuple is reshaped with
// \`Tuple.map\` and finished as a single value.
let private codec =
    Tuple.path "search"
    |> Tuple.Query.Required.string "q"
    |> Tuple.Query.strings "tag"
    |> Tuple.Query.Optional.int "page"
    |> Tuple.Query.flag "draft"
    |> Tuple.map
        (fun ((((_, q), t), p), d) -> (), (q, t, p, d))
        (fun ((), (q, t, p, d)) -> (((((), q), t), p), d))
    |> Tuple.ofCase1 Prisms.search

let private codecs = [ codec ]

let render () =
    let query = Var.create "signals"
    let tags = Var.create "reactive,fsharp"
    let page = Var.create ""
    let draft = Var.create false

    let parsedInput = Var.create "#/search?q=signals&tag=reactive&tag=fsharp"

    let route () =
        let tagList =
            tags.Value.Split(',')
            |> Array.map (fun s -> s.Trim())
            |> Array.filter (fun s -> s <> "")
            |> Array.toList

        let pageValue =
            match System.Int32.TryParse(page.Value) with
            | true, n -> Some n
            | _ -> None

        Search(query.Value, tagList, pageValue, draft.Value)

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press the samples, then edit the fields. \`tag\` can repeat and arrives as a list, \`draft\` is a flag with no value, and an empty optional field drops out of the URL."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "q (required)"
                            Html.input [ attr.bindValue query ]
                        ]

                    Html.label
                        [
                            Html.text "tags (repeated)"
                            Html.input [ attr.bindValue tags ]
                        ]

                    Html.label
                        [
                            Html.text "page (optional)"
                            Html.input [ attr.bindValue page ]
                        ]

                    Html.label
                        [
                            Html.input
                                [
                                    attr.type' "checkbox"
                                    attr.bindChecked draft
                                ]

                            Html.text "draft (flag)"
                        ]
                ]

            Html.label
                [
                    Html.text "built URL"

                    Html.output
                        [
                            Html.text (fun () ->
                                RouteCodec.tryToHash codecs (route ())
                                |> Option.defaultValue "(no codec builds it)"
                            )
                        ]
                ]

            Html.div
                [
                    attr.role "group"

                    for sample in
                        [
                            "#/search?q=signals&tag=reactive&tag=fsharp"
                            "#/search?q=signals&page=2&draft"
                            "#/search?tag=reactive"
                            "#/search?q=a&q=b"
                        ] do
                        Html.button
                            [
                                attr.custom (
                                    "aria-current",
                                    fun () ->
                                        if parsedInput.Value = sample then
                                            "true"
                                        else
                                            "false"
                                )
                                on.click (fun _ -> parsedInput.Value <- sample)
                                Html.text sample
                            ]
                ]

            Html.label
                [
                    Html.text "url"
                    Html.input [ attr.bindValue parsedInput ]
                ]

            Html.label
                [
                    Html.text "parsed back"

                    Html.output
                        [
                            Html.text (fun () ->
                                match RouteCodec.tryParseHash codecs parsedInput.Value with
                                | Ok route -> sprintf "%A" route
                                | Error message -> "Error: " + message
                            )
                        ]
                ]
        ]
`;export{e as default};