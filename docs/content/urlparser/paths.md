---
title: Paths
---

A parser is a pipeline read left to right, in the order of the URL. It starts from a constructor with `Parser.succeed`{fsharp}; each step consumes one path segment and feeds the constructor one argument.

## Literal segments

`Parser.segment`{fsharp} matches a fixed segment and produces no value:

```fsharp live
open Fable.UrlParser

type Route = About

let routes = [ Parser.succeed About |> Parser.segment "about" ]

Parser.tryParsePath routes "/about" |> printfn "%A" // Ok About
Parser.tryParsePath routes "/abut" |> printfn "%A" // Error "Expected segment 'about' but got 'abut'."
```

## Typed segments

`Parser.string`{fsharp} takes the next segment as text. `Parser.int`{fsharp} insists it is an integer. Values reach the constructor in pipeline order:

```fsharp live
open Fable.UrlParser

type Route = Doc of folder: string * page: int

let routes =
    [
        Parser.succeed (fun folder page -> Doc(folder, page))
        |> Parser.segment "doc"
        |> Parser.string
        |> Parser.int
    ]

Parser.tryParsePath routes "/doc/guides/2" |> printfn "%A" // Ok (Doc ("guides", 2))
Parser.tryParsePath routes "/doc/guides/two" |> printfn "%A" // Error "Could not convert 'two' to int."
```

## Custom segments

`Parser.custom`{fsharp} takes your own conversion: a `tryParse`{fsharp} that returns `None`{fsharp} for anything it does not recognise, and a type name for the error message:

```fsharp live
open Fable.UrlParser

type Level =
    | Low
    | High

let tryParseLevel =
    function
    | "low" -> Some Low
    | "high" -> Some High
    | _ -> None

type Route = Task of Level

let routes =
    [
        Parser.succeed Task
        |> Parser.segment "task"
        |> Parser.custom "Level" tryParseLevel
    ]

Parser.tryParsePath routes "/task/high" |> printfn "%A" // Ok (Task High)
Parser.tryParsePath routes "/task/extreme" |> printfn "%A" // Error "Could not convert 'extreme' to Level."
```

## The root

`Parser.succeed`{fsharp} on its own consumes nothing, so it matches `/`:

```fsharp
Parser.succeed Home
```

## The whole path, nothing more

Every segment must be consumed. A parser that matches a prefix but leaves segments behind fails:

```fsharp live
open Fable.UrlParser

type Route = Users

let routes = [ Parser.succeed Users |> Parser.segment "users" ]

Parser.tryParsePath routes "/users/42/details" |> printfn "%A"
// Error "URL matched but had leftover segments: '42/details'."
```

## Which error you get

Parsers are tried in order and the first success wins. When all fail, the reported error comes from the parser that consumed the most segments - the one closest to a match:

```fsharp live
open Fable.UrlParser

type Route =
    | Home
    | Blog of int

let routes =
    [
        Parser.succeed Home
        Parser.succeed Blog |> Parser.segment "blog" |> Parser.int
    ]

Parser.tryParsePath routes "/blog/first-post" |> printfn "%A"
// Error "Could not convert 'first-post' to int."
```

The error names the integer conversion, not the root mismatch: the blog parser got two segments deep before failing.

## Decoding

Segments are percent-decoded before your parser sees them. `/user/ada%20lovelace` arrives as the segment `ada lovelace`.
