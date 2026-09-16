---
title: Query parameters
---

Query parameters are matched by key, so they can appear in any order in the URL. Each combinator states how many times its key may appear; a URL that breaks the rule is a parse error, not a silent default.

| Combinator | Key appears | Produces |
| --- | --- | --- |
| `Parser.Query.Required.string "q"`{fsharp} | exactly once | `string` |
| `Parser.Query.Optional.string "q"`{fsharp} | zero or one time | `string option` |
| `Parser.Query.strings "tag"`{fsharp} | any number of times | `string list` |
| `Parser.Query.flag "draft"`{fsharp} | bare key, present or absent | `bool` |

`int`{fsharp} variants exist beside every `string`{fsharp} one, and `custom`{fsharp} variants beside `Required`{fsharp} and `Optional`{fsharp}.

## Required, optional, flag

```fsharp live
open Fable.UrlParser

type Route = Search of query: string * page: int option * draft: bool

let routes =
    [
        Parser.succeed (fun q page draft -> Search(q, page, draft))
        |> Parser.segment "search"
        |> Parser.Query.Required.string "q"
        |> Parser.Query.Optional.int "page"
        |> Parser.Query.flag "draft"
    ]

[
    "/search?q=signals&page=3&draft" // Ok (Search ("signals", Some 3, true))
    "/search?q=signals" // Ok (Search ("signals", None, false))
    "/search?page=3" // Error "Required query parameter 'q' was missing."
    "/search?q=a&q=b" // Error "Query parameter 'q' appeared multiple times ..."
]
|> List.iter (fun url -> Parser.tryParsePath routes url |> printfn "%s\n  %A" url)
```

The last two fail. A missing required key and a duplicated single-value key are both errors, and the message names the key.

## Repeated keys

`strings`{fsharp} and `ints`{fsharp} collect every occurrence of a key. An absent key gives `[]`{fsharp}. For `ints`{fsharp}, every value must convert or the parse fails:

```fsharp live
open Fable.UrlParser

type Route = Items of tags: string list * ids: int list

let routes =
    [
        Parser.succeed (fun tags ids -> Items(tags, ids))
        |> Parser.segment "items"
        |> Parser.Query.strings "tag"
        |> Parser.Query.ints "id"
    ]

Parser.tryParsePath routes "/items?tag=fsharp&tag=web&id=1&id=2" |> printfn "%A"
// Ok (Items (["fsharp"; "web"], [1; 2]))

Parser.tryParsePath routes "/items" |> printfn "%A" // Ok (Items ([], []))

Parser.tryParsePath routes "/items?id=1&id=two" |> printfn "%A"
// Error "Not all values for query parameter 'id' could be converted to int: 1, two."
```

## Custom conversions

`Required.custom`{fsharp} and `Optional.custom`{fsharp} take the same `tryParse`{fsharp} as [custom path segments](paths.md#custom-segments):

```fsharp live
open Fable.UrlParser

type Sort =
    | ByName
    | ByDate

let tryParseSort =
    function
    | "name" -> Some ByName
    | "date" -> Some ByDate
    | _ -> None

type Route = Listing of sort: Sort option

let routes =
    [
        Parser.succeed Listing
        |> Parser.segment "listing"
        |> Parser.Query.Optional.custom "sort" "Sort" tryParseSort
    ]

Parser.tryParsePath routes "/listing?sort=date" |> printfn "%A" // Ok (Listing (Some ByDate))
Parser.tryParsePath routes "/listing" |> printfn "%A" // Ok (Listing None)
Parser.tryParsePath routes "/listing?sort=size" |> printfn "%A" // Error "Could not convert 'size' to Sort."
```

## All flags

`Parser.Query.allFlags`{fsharp} collects every bare key as a `string list`{fsharp}:

```fsharp live
open Fable.UrlParser

type Route = Debug of flags: string list

let routes =
    [
        Parser.succeed Debug |> Parser.segment "debug" |> Parser.Query.allFlags
    ]

Parser.tryParsePath routes "/debug?verbose&trace" |> printfn "%A" // Ok (Debug ["trace"; "verbose"])
```
