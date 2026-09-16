module Fable.Ripple.Dom.Routing

open System
open Browser
open Browser.Types
open Fable.Ripple

(*
    URL operators - make building route strings natural
*)

module Operators =

    /// Joins two path segments with a "/".
    let inline (</>) a b = a + "/" + b

    /// Appends a query string key with "?".
    let inline (<?>) a b = a + "?" + b

    /// Appends an additional query parameter with "&".
    let inline (<&>) a b = a + "&" + b

/// Fired on every programmatic navigation (`newUrl`/`modifyUrl`) so a router
/// picks it up the same way it reacts to `popstate`/`hashchange`.
[<Literal>]
let private NavigatedEvent = "signals:navigated"

/// Low-level history + location primitives. Use these to build a custom router;
/// the `HashRouter`/`PathRouter` types below are the batteries-included wrappers.
module Advanced =

    /// Push `url` onto the history stack and notify any listening router.
    let newUrl (url: string) =
        history.pushState (null, "", url)
        window.dispatchEvent (CustomEvent.Create NavigatedEvent) |> ignore

    /// Replace the current history entry with `url` (no new stack entry).
    let modifyUrl (url: string) =
        history.replaceState (null, "", url)
        window.dispatchEvent (CustomEvent.Create NavigatedEvent) |> ignore

    /// Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
    let jump (n: int) = history.go n

    (*
        Hash router
    *)

    /// Reactive hash-based location. Returns a `Signal<string>` that always reflects
    /// `location.hash`, plus an `IDisposable` that tears the listeners down.
    let useHash () : Signal<string> * IDisposable =
        let current = Var.create window.location.hash

        let mutable lastHref = window.location.href

        let onLocationChange _ =
            let href = window.location.href

            if href <> lastHref then
                lastHref <- href
                current.Value <- window.location.hash

        window.addEventListener ("popstate", onLocationChange)
        window.addEventListener ("hashchange", onLocationChange)
        window.addEventListener (NavigatedEvent, onLocationChange)

        let disposable =
            { new IDisposable with
                member _.Dispose() =
                    window.removeEventListener ("popstate", onLocationChange)
                    window.removeEventListener ("hashchange", onLocationChange)
                    window.removeEventListener (NavigatedEvent, onLocationChange)
            }

        current.Signal, disposable

    /// Reactive path-based (HTML5 history) location. Returns a `Signal<string>` that
    /// tracks `pathname + search`, plus an `IDisposable` that tears the listeners down.
    let usePath () : Signal<string> * IDisposable =
        let rebuildPathFromLocation (location: Location) = location.pathname + location.search

        let current = Var.create (rebuildPathFromLocation window.location)

        let mutable lastHref = window.location.href

        let onLocationChange _ =
            let href = window.location.href

            if href <> lastHref then
                lastHref <- href
                current.Value <- rebuildPathFromLocation window.location

        window.addEventListener ("popstate", onLocationChange)
        window.addEventListener (NavigatedEvent, onLocationChange)

        let disposable =
            { new IDisposable with
                member _.Dispose() =
                    window.removeEventListener ("popstate", onLocationChange)
                    window.removeEventListener (NavigatedEvent, onLocationChange)
            }

        current.Signal, disposable

/// <summary>A hash-based router that can both parse and build URLs.</summary>
/// <remarks>
/// The router listens for hash changes and runs <c>parse</c> on each new hash value.
/// Use any parsing library - or a plain match expression - to produce <c>'Route option</c>
/// and, conversely, a hash string (e.g. <c>"#/counter"</c>) from a <c>'Route</c>.
///
/// If you only need to parse and never need to build URLs from a <c>'Route</c>, use
/// <see cref="T:Fable.Ripple.Dom.Routing.Simple.HashRouter`1"/> instead.
/// </remarks>
/// <param name="parse">Function from raw hash string (e.g. <c>"#/counter"</c>) to <c>'Route option</c>.</param>
/// <param name="toUrl">Function from a <c>'Route</c> to its hash string (e.g. <c>"#/counter"</c>).</param>
type HashRouter<'Route>(parse: string -> 'Route option, toUrl: 'Route -> string) =

    let currentHash, disposable = Advanced.useHash ()
    let currentRoute = currentHash |> Signal.map parse

    /// The current parsed route, re-evaluated on every navigation.
    /// `None` if the current hash does not match any known route.
    member _.CurrentRoute: Signal<'Route option> = currentRoute

    /// The hash string for the given route, e.g. for use as an <c>&lt;a href&gt;</c>.
    member _.Href(route: 'Route) : string = toUrl route

    /// Push the URL for the given route onto the history stack.
    member _.NewUrl(route: 'Route) = Advanced.newUrl (toUrl route)

    /// Replace the current history entry with the URL for the given route.
    member _.ModifyUrl(route: 'Route) = Advanced.modifyUrl (toUrl route)

    /// Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
    member _.Jump(n: int) = Advanced.jump n

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()

/// <summary>A path-based (HTML5 history) router that can both parse and build URLs.</summary>
/// <remarks>
/// The router listens for popstate / signals:navigated events and runs <c>parse</c> on each
/// new <c>pathname + search</c> string.
/// Use any parsing library - or a plain match expression - to produce <c>'Route option</c>
/// and, conversely, a path string (e.g. <c>"/counter?page=2"</c>) from a <c>'Route</c>.
/// Requires server-side support for returning the app shell on any path (or a catch-all redirect).
///
/// If you only need to parse and never need to build URLs from a <c>'Route</c>, use
/// <see cref="T:Fable.Ripple.Dom.Routing.Simple.PathRouter`1"/> instead.
/// </remarks>
/// <param name="parse">Function from path string (e.g. <c>"/counter?page=2"</c>) to <c>'Route option</c>.</param>
/// <param name="toUrl">Function from a <c>'Route</c> to its path string (e.g. <c>"/counter?page=2"</c>).</param>
type PathRouter<'Route>(parse: string -> 'Route option, toUrl: 'Route -> string) =

    let currentPath, disposable = Advanced.usePath ()
    let currentRoute = currentPath |> Signal.map parse

    /// The current parsed route, re-evaluated on every navigation.
    /// `None` if the current pathname does not match any known route.
    member _.CurrentRoute: Signal<'Route option> = currentRoute

    /// The path string for the given route, e.g. for use as an <c>&lt;a href&gt;</c>.
    member _.Href(route: 'Route) : string = toUrl route

    /// Push the URL for the given route onto the history stack.
    member _.NewUrl(route: 'Route) = Advanced.newUrl (toUrl route)

    /// Replace the current history entry with the URL for the given route.
    member _.ModifyUrl(route: 'Route) = Advanced.modifyUrl (toUrl route)

    /// Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
    member _.Jump(n: int) = Advanced.jump n

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()

/// Parse-only routers, for when you don't need to build URLs from a typed
/// <c>'Route</c> (e.g. you only ever navigate via raw strings).
module Simple =

    /// <summary>A parse-only hash-based router.</summary>
    /// <remarks>
    /// The router listens for hash changes and runs <c>parse</c> on each new hash value.
    /// Use any parsing library - or a plain match expression - to produce <c>'Route option</c>.
    /// </remarks>
    /// <param name="parse">Function from raw hash string (e.g. <c>"#/counter"</c>) to <c>'Route option</c>.</param>
    type HashRouter<'Route>(parse: string -> 'Route option) =

        let currentHash, disposable = Advanced.useHash ()
        let currentRoute = currentHash |> Signal.map parse

        /// The current parsed route, re-evaluated on every navigation.
        /// `None` if the current hash does not match any known route.
        member _.CurrentRoute: Signal<'Route option> = currentRoute

        /// Push a new URL onto the history stack.
        member _.NewUrl(url: string) = Advanced.newUrl url

        /// Replace the current history entry without adding a new one.
        member _.ModifyUrl(url: string) = Advanced.modifyUrl url

        /// Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
        member _.Jump(n: int) = Advanced.jump n

        interface IDisposable with
            member _.Dispose() = disposable.Dispose()

    /// <summary>A parse-only path-based (HTML5 history) router.</summary>
    /// <remarks>
    /// The router listens for popstate / signals:navigated events and runs <c>parse</c> on each
    /// new <c>pathname + search</c> string.
    /// Use any parsing library - or a plain match expression - to produce <c>'Route option</c>.
    /// Requires server-side support for returning the app shell on any path (or a catch-all redirect).
    /// </remarks>
    /// <param name="parse">Function from path string (e.g. <c>"/counter?page=2"</c>) to <c>'Route option</c>.</param>
    type PathRouter<'Route>(parse: string -> 'Route option) =

        let currentPath, disposable = Advanced.usePath ()
        let currentRoute = currentPath |> Signal.map parse

        /// The current parsed route, re-evaluated on every navigation.
        /// `None` if the current pathname does not match any known route.
        member _.CurrentRoute: Signal<'Route option> = currentRoute

        /// Push a new URL onto the history stack.
        member _.NewUrl(url: string) = Advanced.newUrl url

        /// Replace the current history entry without adding a new one.
        member _.ModifyUrl(url: string) = Advanced.modifyUrl url

        /// Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
        member _.Jump(n: int) = Advanced.jump n

        interface IDisposable with
            member _.Dispose() = disposable.Dispose()
