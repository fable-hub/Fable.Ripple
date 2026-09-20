module Demo.Main

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Router
open Demo.Catalogue

// Entry for the Fable.Ripple showcase. A hash-routed SPA: every example has its own
// URL (`#/seven-guis/counter`), so back/forward and bookmarks work. The nav
// itself is the docs site's own sidebar (see docs/Site.fs), built statically
// from `Catalogue.catalogue` - clicking a link changes the hash, the
// HashRouter re-parses it, and `Html.dynamic` swaps the example subtree
// (disposing the previous one's scope, exercising deterministic teardown).
//
// Everything below is derived from `Catalogue.catalogue`. Adding an example
// means adding one record there and one file - nothing in this file changes.

let private defaultRoute = Route.Home

/// The route the SHELL reacts to: the raw one with per-example payloads stripped.
///
/// This is a derived signal rather than a direct read, and the equality cutoff on
/// it is what makes the URL-state example possible. That example rewrites its own
/// query string on every keystroke, which changes `router.CurrentRoute` - and the
/// `Html.dynamic` around the view would rebuild the whole example under the user's
/// cursor. Canonicalising first means the shell sees no change at all, while the
/// example itself reads the raw route and updates normally.
let private shellRoute =
    router.CurrentRoute
    |> Signal.map (Option.defaultValue defaultRoute >> canonical)

let private currentRoute () = shellRoute.Value

/// The raw route, payload and all - only the URL-state example needs it.
let private rawRoute () =
    router.CurrentRoute.Value |> Option.defaultValue defaultRoute

let private currentEntry () = entryOf (currentRoute ())

let private title () =
    match currentEntry (), sectionOf (currentRoute ()) with
    | Some e, Some s -> s.Title + " / " + e.Title
    | _ ->
        match currentRoute () with
        | Route.Examples -> "All examples"
        | _ -> "Fable.Ripple"

let private description () =
    match currentEntry () with
    | Some e -> e.Description
    | None -> "Examples for Fable.Ripple, Fable.Ripple.Dom and Fable.UrlParser."

let private view () =
    match currentEntry () with
    | Some e -> e.View()
    | None -> Home.render ()

let private source () =
    Sources.sourceForRoute (currentRoute ())

/// What the copy button is currently saying.
///
/// Three states rather than a bool, because a clipboard write can fail and a
/// button that says `copied` either way is worse than no button - it is the
/// same failure as a counter that reports what it was told rather than what
/// happened.
type private Copy =
    | Ready
    | Done
    | Failed

let private copyState = Var.create Ready

/// Return to `copy` shortly, whichever way it went.
let private settle () =
    Browser.Dom.window.setTimeout ((fun () -> copyState.Value <- Ready), 1500)
    |> ignore

/// The code panel's right-hand control: copy the listing, for pasting into a
/// scratch project.
let private codeAction () =
    match Sources.listingForRoute (currentRoute ()) with
    | None -> Html.none
    | Some listing ->
        Html.button
            [
                attr.className "code-copy"

                attr.classList (fun () -> [ "is-failed", copyState.Value = Failed ])

                on.click (fun _ ->
                    Sources.copyToClipboard listing.Text
                    |> Promise.map (fun () ->
                        copyState.Value <- Done
                        settle ()
                    )
                    |> Promise.catch (fun _ ->
                        copyState.Value <- Failed
                        settle ()
                    )
                    |> ignore
                )

                Html.text (fun () ->
                    match copyState.Value with
                    | Ready -> "copy"
                    | Done -> "copied"
                    | Failed -> "copy failed"
                )
            ]

/// Keeps `aria-current` on the docs site's own sidebar link for the current
/// route (see `Theme.menu "demo"` in docs/Site.fs, built from this same
/// catalogue): that sidebar is static markup rendered once for the whole
/// page, so nothing else marks which of its links is "now".
let private syncActiveNavLink () =
    let hash = router.Href(currentRoute ())
    let links = Browser.Dom.document.querySelectorAll ".nacara-sidebar__link"

    for i in 0 .. links.length - 1 do
        let el = links.item i :?> Browser.Types.HTMLElement
        let href = el.getAttribute "href"

        if not (isNull href) && href.EndsWith hash then
            el.setAttribute ("aria-current", "page")

            // The theme collapses a menu section into `<details>`, and it only
            // pre-opens one holding the current PAGE - a hash route is never
            // that, so the active category has to be opened from here.
            match el.closest "details.nacara-sidebar__group" with
            | Some group -> group.setAttribute ("open", "")
            | None -> ()
        else
            el.removeAttribute "aria-current"

let render () =
    // `#/` is not a page. Readers arrive from the docs with their bearings
    // already, so the front door opens straight onto the first example, and
    // the catalogue keeps its own URL for the times they want the map.
    //
    // `ModifyUrl`, not `NewUrl`: a pushed redirect makes Back bounce off it
    // and strands the reader on the page they were trying to leave.
    Signal.effect (fun () ->
        if shellRoute.Value = Route.Home then
            router.ModifyUrl firstRoute
    )
    |> ignore

    Signal.effect syncActiveNavLink |> ignore

    Shell.render title description view source codeAction

// The URL-state example is compiled before the router, so this file - the
// composition root - hands it the four things it needs. In an application the
// router would just be in scope where the view is written.
Examples.Routing.UrlState.connect
    (fun () ->
        match rawRoute () with
        | Route.Routing(RoutingExample.UrlState q) -> q
        | _ -> Examples.Routing.UrlState.ProductQuery.empty
    )
    (fun q -> router.NewUrl(Route.Routing(RoutingExample.UrlState q)))
    (fun q -> router.ModifyUrl(Route.Routing(RoutingExample.UrlState q)))
    (fun () -> router.Jump -1)

Router.validate ()
Html.mount "root" render |> ignore
