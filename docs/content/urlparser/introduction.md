---
title: Introduction
---

Fable.UrlParser turns URLs into typed values, and typed values back into URLs. A route is described once - the same description parses and builds, so the two directions cannot drift apart.

## Installation

```bash frame="terminal"
dotnet add package Fable.UrlParser --prerelease
```

The library has no dependencies and works with any framework, or none.

## A first parse

One parser per route, tried in order. Typed values come out, not strings:

```fsharp live
open Fable.UrlParser

type Route =
    | Home
    | Blog of id: int
    | User of name: string

let routes =
    [
        Parser.succeed Home
        Parser.succeed (fun id -> Blog id) |> Parser.segment "blog" |> Parser.int
        Parser.succeed (fun name -> User name) |> Parser.segment "user" |> Parser.string
    ]

[
    "/" // Ok Home
    "/blog/42" // Ok (Blog 42)
    "/user/ada" // Ok (User "ada")
    "/blog/latest" // Error "Could not convert 'latest' to int."
]
|> List.iter (fun url -> Parser.tryParsePath routes url |> printfn "%s -> %A" url)
```

`/blog/latest` fails, and the error says why: the report comes from the attempt that got furthest, not a blank "no match".

## Two layers

### `Parser`{fsharp}

Parse only. A `Parser<'Route>`{fsharp} list and `Parser.tryParsePath`{fsharp} give you a `Result<'Route, string>`{fsharp}. Start with [Paths](paths.md).

### `UrlCodec`{fsharp}

Parse and build. A `RouteCodec<'Route>`{fsharp} also turns a route value back into a URL string, so the links in your views and the parser in your router are the same code. See [Two-way codecs](two-way-codecs.md).
