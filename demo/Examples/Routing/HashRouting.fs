module Demo.Examples.Routing.HashRouting

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Fable.UrlParser.UrlCodec
open Demo.Examples.Widgets // demo-hide-line

// A codec is a pipeline describing ONE case of the route DU, read left-to-right
// as the URL: `Tuple.path "user"` matches the first segment,
// `Tuple.int` takes a typed one, and `Tuple.ofCase*` closes the pipeline by
// saying which case the collected values belong to.
//
// A `Prism` is how a codec names a case: `Embed` constructs, `Project` recovers
// the values and returns `None` for any other case. That is the whole reason one
// definition runs in both directions.
//
// `RouteCodec.tryParseHash` walks the list and stops at the first match;
// `tryToHash` asks each codec whether the route is its case. `HashRouter` wires
// those to the address bar - which is what this site does, in `demo/Router.fs`.
//
// The "address bar" below is a plain `Var<string>`, not a second `HashRouter`:
// two routers would both listen to the real hash and each would report the
// other's URLs as failures.

type private Route =
    | Overview
    | User of id: int
    | Post of slug: string

module private Prisms =
    let user: Prism<Route, int> =
        {
            Embed = User
            Project =
                function
                | User id -> Some id
                | _ -> None
        }

    let post: Prism<Route, string> =
        {
            Embed = Post
            Project =
                function
                | Post slug -> Some slug
                | _ -> None
        }

let private codecs =
    [
        Tuple.path "overview" |> Tuple.ofCase Overview
        Tuple.path "user" |> Tuple.int |> Tuple.ofCase1 Prisms.user
        Tuple.path "post" |> Tuple.string |> Tuple.ofCase1 Prisms.post
    ]

let render () =
    let url = Var.create "#/overview"

    let parsed = url |> Signal.map (RouteCodec.tryParseHash codecs)

    // Every link's href comes from the codec list, not from a string literal:
    // rename a segment in one place and the links follow.
    let link (route: Route) (label: string) =
        let href = RouteCodec.tryToHash codecs route |> Option.defaultValue "#"

        Html.button
            [
                attr.custom (
                    "aria-current",
                    fun () ->
                        if parsed.Value = Ok route then
                            "true"
                        else
                            "false"
                )
                on.click (fun _ -> url.Value <- href)
                Html.text (label + "  " + href)
            ]

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press a link, or edit the address bar by hand. The page follows either way, because both go through the same codecs. Type a URL that matches nothing and the error says which codec got furthest."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    link Overview "Overview"
                    link (User 42) "User"
                    link (Post "hello-world") "Post"
                ]

            Html.label
                [
                    Html.text "address bar"
                    Html.input [ attr.bindValue url ]
                ]

            // A native `match` on the parse result is the whole router.
            Html.dynamic (fun () ->
                match parsed.Value with
                | Ok Overview -> Html.p "The overview page."
                | Ok(User id) -> Html.p [ Html.text (sprintf "User page for id %d." id) ]
                | Ok(Post slug) -> Html.p [ Html.text (sprintf "Post page for %A." slug) ]
                | Error message ->
                    Html.p
                        [
                            attr.className "error"
                            Html.text ("No codec matched: " + message)
                        ]
            )

        ]
