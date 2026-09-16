module Demo.Examples.Routing.ValuesDialect

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Fable.UrlParser.UrlCodec
open Demo.Examples.Widgets // demo-hide-line

// `Values` starts from the curried constructor of a values type and gives each
// step a GETTER (`_.Query`), which is how the build direction knows where to
// find that value.
//
// `Values.createFor` is for ANONYMOUS record values, whose labels cannot be
// resolved from the environment; it takes the prism as an inference witness.

type private Filters =
    {
        Query: string
        Page: int option
        Sort: string
    }

type private Route =
    | Browse of Filters
    | Tag of name: string

module private Prisms =
    let browse: Prism<Route, Filters> =
        {
            Embed = Browse
            Project =
                function
                | Browse f -> Some f
                | _ -> None
        }

    let tag: Prism<Route, string> =
        {
            Embed = Tag
            Project =
                function
                | Tag n -> Some n
                | _ -> None
        }

// The Values dialect: a constructor, then one named getter per value.
let private browseCodec =
    Values.create (fun query page sort ->
        {
            Query = query
            Page = page
            Sort = sort
        }
    )
    |> Values.segment "browse"
    |> Values.Query.Required.string "q" _.Query
    |> Values.Query.Optional.int "page" _.Page
    |> Values.Query.Required.string "sort" _.Sort
    |> Values.asCase Prisms.browse

// The Tuple dialect, in the same list.
let private tagCodec = Tuple.path "tag" |> Tuple.string |> Tuple.ofCase1 Prisms.tag

let private codecs =
    [
        browseCodec
        tagCodec
    ]

let render () =
    let url = Var.create "#/browse?q=signals&sort=recent"

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press each sample. `Tuple` and `Values` codecs sit in the same list and parse the same URLs. The last sample is missing the required `sort`, and the error names it."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    for sample in
                        [
                            "#/browse?q=signals&sort=recent"
                            "#/browse?q=signals&page=3&sort=name"
                            "#/tag/reactive"
                            "#/browse?q=signals"
                        ] do
                        Html.button
                            [
                                attr.custom (
                                    "aria-current",
                                    fun () ->
                                        if url.Value = sample then
                                            "true"
                                        else
                                            "false"
                                )
                                on.click (fun _ -> url.Value <- sample)
                                Html.text sample
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
                            Html.text "parsed (either dialect)"

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
                            Html.text "rebuilt"

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

            Html.div
                [
                    Html.article
                        [
                            Html.header "Values - named"

                            Html.p
                                [
                                    Html.small
                                        "Each value carries a getter, so swapping two is a compile error."
                                ]

                            // demo-hide
                            Code.block
                                "Values.create (fun query page sort -> { ... })\n|> Values.segment \"browse\"\n|> Values.Query.Required.string \"q\" _.Query\n|> Values.Query.Optional.int \"page\" _.Page\n|> Values.Query.Required.string \"sort\" _.Sort\n|> Values.asCase Prisms.browse"
                        // demo-show
                        ]

                    Html.article
                        [
                            Html.header "Tuple - positional"

                            Html.p
                                [
                                    Html.small
                                        "Terser, and fine for one or two values. Past three, position is easy to get wrong."
                                ]

                            // demo-hide
                            Code.block
                                "Tuple.path \"tag\"\n|> Tuple.string\n|> Tuple.ofCase1 Prisms.tag"
                        // demo-show
                        ]
                ]
        ]
