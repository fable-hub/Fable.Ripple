---
title: Two-way codecs
toc:
    to: 4
---

`UrlCodec`{fsharp} describes a route once and runs the description in both directions. The pipeline that parses a URL into a value also builds the URL back from it, so the links in your views and the parser in your router are the same code. Parse a URL, build it straight back, and you get the same string - the property the library's tests assert.

```fsharp live
open Fable.UrlParser.UrlCodec

type Route =
    | Home
    | User of id: int

module Prisms =
    let user: Prism<Route, int> =
        {
            Embed = User
            Project =
                function
                | User id -> Some id
                | _ -> None
        }

let codecs =
    [
        Tuple.root |> Tuple.ofCase Home
        Tuple.path "user" |> Tuple.int |> Tuple.ofCase1 Prisms.user
    ]

RouteCodec.tryParsePath codecs "/user/7" |> printfn "%A" // Ok (User 7)
RouteCodec.tryToPath codecs (User 7) |> printfn "%A" // Some "user/7"
RouteCodec.tryToHash codecs (User 7) |> printfn "%A" // Some "#/user/7"
```

`tryToPath`{fsharp} builds without a leading slash; `tryToHash`{fsharp} prefixes `#/`.

## Writing a codec

Two dialects, both finishing as a `RouteCodec`{fsharp} so they live in the same list:

- `Tuple`{fsharp} is positional and terser. Use it for a route of one or two values.
- `Values`{fsharp} names every value. Use it when the route carries a record, or more values than position can keep honest.

Either way the shape is the same: start at a path, add one step per value in URL order, then close with the union case.

### Tuple

:::steps

### Start the path

`Tuple.root`{fsharp} matches no segment; `Tuple.path "user"`{fsharp} matches a literal one.

### Add one step per value

In the order they appear in the URL.

`Tuple.string`{fsharp}, `Tuple.int`{fsharp} and `Tuple.custom`{fsharp} each take a path segment. `Tuple.fragment`{fsharp} takes the `#`{fsharp} fragment. `Tuple.Query.*`{fsharp} mirrors every [query combinator](query-parameters.md).

```fsharp
Tuple.path "user" |> Tuple.int
```

### Close it with the union case

`Tuple.ofCase Home`{fsharp} for a case with no fields - it needs no prism, since recognising it is equality. `Tuple.ofCase1`{fsharp} to `Tuple.ofCase3`{fsharp} for one to three values, each taking a [prism](#prisms).

```fsharp
Tuple.path "user" |> Tuple.int |> Tuple.ofCase1 Prisms.user
```

:::

Past `ofCase3`{fsharp}, reshape the accumulated tuple with `Tuple.map`{fsharp} and finish it as a single value:

```fsharp live
open Fable.UrlParser.UrlCodec

type Route = Search of query: string * tags: string list * page: int option * draft: bool

module Prisms =
    let search: Prism<Route, string * string list * int option * bool> =
        {
            Embed = Search
            Project =
                function
                | Search(q, t, p, d) -> Some(q, t, p, d)
        }

let codecs =
    [
        Tuple.path "search"
        |> Tuple.Query.Required.string "q"
        |> Tuple.Query.strings "tag"
        |> Tuple.Query.Optional.int "page"
        |> Tuple.Query.flag "draft"
        |> Tuple.map
            (fun ((((_, q), t), p), d) -> (), (q, t, p, d))
            (fun ((), (q, t, p, d)) -> (((((), q), t), p), d))
        |> Tuple.ofCase1 Prisms.search
    ]

RouteCodec.tryParsePath codecs "/search?q=signals&tag=reactive&tag=fsharp" |> printfn "%A"
// Ok (Search ("signals", ["reactive"; "fsharp"], None, false))
```

At this arity, position is easy to get wrong. [`Values`](#values) names every value instead.

### Values

`Values`{fsharp} starts from a curried constructor and gives every step a getter. Swapping two values of the same type is a compile error here, and a silent bug in `Tuple`{fsharp}.

:::steps

### Start from the constructor

`Values.create`{fsharp} takes a function building the payload from its values, one argument per step to come.

### Add one step per value

Each with the getter that reads it back.

`Values.segment`{fsharp} matches a literal segment. `Values.string`{fsharp}, `Values.int`{fsharp} and `Values.Query.*`{fsharp} each take a value and a getter, as in `Values.int _.Width`{fsharp}.

### Close it

`Values.asCase`{fsharp} with the case's [prism](#prisms).

:::

```fsharp live
open Fable.UrlParser.UrlCodec

type Filters =
    {
        Query: string
        Page: int option
        Sort: string
    }

type Route =
    | Browse of Filters
    | Tag of name: string

module Prisms =
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

let browseCodec =
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

let tagCodec = Tuple.path "tag" |> Tuple.string |> Tuple.ofCase1 Prisms.tag

let codecs =
    [
        browseCodec
        tagCodec
    ]

RouteCodec.tryParsePath codecs "/browse?q=signals&sort=recent" |> printfn "%A"
// Ok (Browse { Query = "signals"; Page = None; Sort = "recent" })

Browse
    {
        Query = "signals"
        Page = Some 2
        Sort = "name"
    }
|> RouteCodec.tryToPath codecs
|> printfn "%A" // Some "browse?q=signals&page=2&sort=name"
```

The parse side applies the constructor argument by argument, in pipeline order. The build side reads each value through its getter (`_.Query`{fsharp}), which is how it knows where in the record that value lives.

#### Anonymous records

Getter labels on an anonymous record cannot be resolved from the constructor alone. `Values.createFor`{fsharp} takes the case's prism as an inference witness; pass the same prism again to `Values.asCase`{fsharp} at the end:

```fsharp live
open Fable.UrlParser.UrlCodec

type Route =
    | Home
    | Display of {| Width: int; Height: int |}

module Prisms =
    let display: Prism<Route, {| Width: int; Height: int |}> =
        {
            Embed = Display
            Project =
                function
                | Display v -> Some v
                | _ -> None
        }

let displayCodec =
    Values.createFor Prisms.display (fun width height ->
        {|
            Width = width
            Height = height
        |}
    )
    |> Values.segment "display"
    |> Values.int _.Width
    |> Values.int _.Height
    |> Values.asCase Prisms.display

let codecs = [ displayCodec ]

RouteCodec.tryParsePath codecs "/display/1920/1080" |> printfn "%A"
// Ok (Display {| Height = 1080; Width = 1920 |})

RouteCodec.tryToPath codecs (Display {| Width = 800; Height = 600 |}) |> printfn "%A"
// Some "display/800/600"
```

#### Reshaping

`Values.map`{fsharp} converts a finished values codec through a total, two-way conversion - for example lifting the values into a wrapper type just before `Values.asCase`{fsharp}:

```fsharp
codec |> Values.map UserId (fun (UserId id) -> id)
```

## Prisms

`Prism<'Route, 'Values>`{fsharp} is a record of two functions, one per direction:

```fsharp
type Prism<'Route, 'Values> =
    {
        Embed: 'Values -> 'Route
        Project: 'Route -> 'Values option
    }
```

`'Route`{fsharp} is your route union. `'Values`{fsharp} is what the case carries: one value for `Tuple.ofCase1`{fsharp}, a pair for `Tuple.ofCase2`{fsharp}, a triple for `Tuple.ofCase3`{fsharp}.

`Embed`{fsharp} builds a route from the values, so it is usually the case name on its own - `Embed = User`{fsharp}.

`Project`{fsharp} goes the other way, returning `None`{fsharp} when the route is a different case. That `None`{fsharp} is how building a URL fails over to the next codec in the list. A union with other cases therefore needs a `| _ -> None`{fsharp} branch; a single-case union is already covered by its one pattern.

The library does not derive prisms for you, so a union with five cases that carry values needs five of them. Keeping them in a `Prisms`{fsharp} module beside the route type is the convention the API's own documentation uses.

## Omitted values

A `false`{fsharp} flag, an empty list and a `None`{fsharp} option are left out of the built URL rather than becoming `draft=false` or `page=`. A URL is compared as a string by caches, analytics and the back button, so the absent state gets exactly one spelling:

```fsharp live
open Fable.UrlParser.UrlCodec

type Route = Search of query: string * page: int option * draft: bool

module Prisms =
    let search: Prism<Route, string * int option * bool> =
        {
            Embed = Search
            Project =
                function
                | Search(q, p, d) -> Some(q, p, d)
        }

let codecs =
    [
        Tuple.path "search"
        |> Tuple.Query.Required.string "q"
        |> Tuple.Query.Optional.int "page"
        |> Tuple.Query.flag "draft"
        |> Tuple.ofCase3 Prisms.search
    ]

RouteCodec.tryToPath codecs (Search("signals", Some 3, true)) |> printfn "%A"
// Some "search?q=signals&page=3&draft"

RouteCodec.tryToPath codecs (Search("signals", None, false)) |> printfn "%A"
// Some "search?q=signals" - the empty states vanished
```
