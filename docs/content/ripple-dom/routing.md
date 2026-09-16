---
title: Routing
---

`Fable.Ripple.Dom.Routing`{fsharp} connects the address bar to a signal. A router holds `CurrentRoute: Signal<'Route option>`{fsharp}; the view reads it like any other signal, and navigating writes the URL.

## A hash router

`HashRouter`{fsharp} takes two functions, parse and print. Pair it with [UrlParser's two-way codecs](../urlparser/two-way-codecs.md) and both come from the same definition:

```fsharp
open Browser
open Fable.Ripple.Dom
open Fable.Ripple.Dom.Routing
open Fable.UrlParser.UrlCodec

type Route =
    | Home
    | User of id: int

// The codecs, as on the UrlParser pages.
let toUrl (route: Route) =
    match RouteCodec.tryToHash codecs route with
    | Some url -> url
    | None -> failwith $"no codec builds %A{route}"

let parse (hash: string) =
    match RouteCodec.tryParseHash codecs hash with
    | Ok route -> Some route
    | Error message ->
        console.warn $"cannot parse '%s{hash}': %s{message}"
        None

let router = new HashRouter<Route>(parse, toUrl)
```

## Reading the route

`CurrentRoute`{fsharp} is a `Signal<'Route option>`{fsharp} - `None`{fsharp} when the URL parses as nothing. Switch views on it with `Html.switch`{fsharp}:

```fsharp
Html.switch
    router.CurrentRoute
    (function
    | Some Home -> Html.p "home"
    | Some(User id) -> Html.p [ Html.text (fun () -> $"user %d{id}") ]
    | None -> Html.p "not found")
```

## Navigating

### `router.Href route`{fsharp}

The URL string, for a plain `<a href>`{html}. Clicking the link changes the hash and the router follows - no click handler needed.

### `router.NewUrl route`{fsharp}

Navigate, pushing a history entry. Back returns to the previous route.

### `router.ModifyUrl route`{fsharp}

Navigate, rewriting the current entry. Paging pushes; typing replaces, so a search does not bury the back button under one entry per keystroke.

### `router.Jump n`{fsharp}

Move through history, like the back and forward buttons.

The router is an `IDisposable`{fsharp}; disposing it stops listening to the address bar.

## Path routing

`PathRouter`{fsharp} is the same API over the path instead of the hash, for a host that serves your page on every URL.

## The pieces

`Routing.Advanced`{fsharp} exposes what the routers are built from: `useHash ()`{fsharp} and `usePath ()`{fsharp} return the raw location as a `Signal<string>`{fsharp} plus a disposable; `newUrl`{fsharp}, `modifyUrl`{fsharp} and `jump`{fsharp} wrap the History API.
