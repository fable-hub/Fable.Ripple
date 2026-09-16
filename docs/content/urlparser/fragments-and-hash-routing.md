---
title: Fragments and hash routing
---

## The fragment

`Parser.fragment`{fsharp} reads the part after `#` as a `string option`. It consumes no path segment:

```fsharp live
open Fable.UrlParser

type Route = Article of slug: string * section: string option

let routes =
    [
        Parser.succeed (fun slug section -> Article(slug, section))
        |> Parser.segment "article"
        |> Parser.string
        |> Parser.fragment
    ]

Parser.tryParsePath routes "/article/parsers#errors" |> printfn "%A"
// Ok (Article ("parsers", Some "errors"))

Parser.tryParsePath routes "/article/parsers" |> printfn "%A" // Ok (Article ("parsers", None))
```

## Hash routing

A single-page application without server configuration keeps its route after the `#`. `Parser.tryParseHash`{fsharp} applies the same parsers to that part:

```fsharp live
open Fable.UrlParser

type Route =
    | Overview
    | UserPage of id: int

let routes =
    [
        Parser.succeed Overview
        Parser.succeed UserPage |> Parser.segment "user" |> Parser.int
    ]

Parser.tryParseHash routes "#/user/7" |> printfn "%A" // Ok (UserPage 7)
Parser.tryParseHash routes "https://example.com/app#/user/7" |> printfn "%A" // Ok (UserPage 7)
```

Everything before the `#` is ignored, so `window.location.href`{js} can be passed as it is.

To wire parsers to the address bar - reacting to hash changes, building links back - see [Routing](../ripple-dom/routing.md) in Fable.Ripple.Dom.
