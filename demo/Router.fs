module Demo.Router

open Browser
open Fable.Ripple.Dom.Routing
open Fable.UrlParser.UrlCodec
open Demo.Catalogue
open Demo.Examples.Routing.UrlState

(*
    Codecs. Every example is literal-segments-only - `#/<section>/<example>` -
    except the URL-state one, whose query string is its state and therefore part
    of its route.
*)

let private urlStateCodec =
    let prism: Prism<Route, string option * int option * string option> =
        {
            Embed =
                fun (search, page, sort) ->
                    Route.Routing(
                        RoutingExample.UrlState
                            {
                                Search = search
                                Page = page
                                Sort = sort
                            }
                    )
            Project =
                function
                | Route.Routing(RoutingExample.UrlState q) -> Some(q.Search, q.Page, q.Sort)
                | _ -> None
        }

    Tuple.path "routing"
    |> Tuple.segment "url-state"
    |> Tuple.Query.Optional.string "q"
    |> Tuple.Query.Optional.int "page"
    |> Tuple.Query.Optional.string "sort"
    |> Tuple.ofCase3 prism

let private codecs =
    [
        // First: a generic codec for the same slug would match the path and drop
        // the query on the floor, and `tryParseHash` stops at the first match.
        yield urlStateCodec

        for section in catalogue do
            for e in section.Entries do
                match e.Route with
                | Route.Routing(RoutingExample.UrlState _) -> () // covered above
                | _ -> yield Tuple.path section.Slug |> Tuple.segment e.Slug |> Tuple.ofCase e.Route
    ]

let private toUrl (route: Route) : string =
    match route with
    | Route.Home -> "#/"
    | Route.Examples -> "#/examples"
    | _ ->
        match RouteCodec.tryToHash codecs route with
        | Some url -> url
        | None -> failwith $"No codec produced a URL for route: %A{route}"

let private parse (hash: string) : Route option =
    let trimmed =
        hash.Trim(
            [|
                '#'
                '/'
            |]
        )

    if trimmed = "" then
        Some Route.Home
    elif trimmed = "examples" then
        Some Route.Examples
    else
        match RouteCodec.tryParseHash codecs hash with
        | Ok route -> Some route
        | Error err ->
            console.warn $"Failed to parse route '{hash}': {err}"
            None

let router = new HashRouter<Route>(parse, toUrl)

/// The catalogue is the one table the compiler cannot check, so prove at startup
/// that every entry round-trips and that no route appears twice.
let validate () =
    all
    |> List.countBy (fun e -> e.Route)
    |> List.filter (fun (_, n) -> n > 1)
    |> List.iter (fun (route, n) -> console.error $"Route %A{route} appears {n} times")

    for route in
        [
            Route.Home
            Route.Examples
        ] do
        match parse (toUrl route) with
        | Some parsed when parsed = route -> ()
        | other -> console.error $"Route %A{route} did not round-trip: %A{other}"

    for e in all do
        let url = toUrl e.Route

        match parse url with
        | Some parsed when parsed = e.Route -> ()
        | other -> console.error $"Route %A{e.Route} did not round-trip: {url} -> %A{other}"
