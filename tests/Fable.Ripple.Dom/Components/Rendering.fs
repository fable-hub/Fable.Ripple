module Fable.Ripple.Dom.Test.Components.Rendering

open System
open Browser
open Fable.Ripple
open Fable.Ripple.Dom

let private reactiveText () : DomItem =
    let name = Var.create "Ada"

    Html.div
        [
            Html.output
                [
                    attr.id "thunk"
                    Html.text (fun () -> "Hello " + name.Value)
                ]
            Html.output
                [
                    attr.id "signal"
                    Html.text name
                ]
            Html.button
                [
                    attr.id "rename"
                    on.click (fun _ -> name.Value <- "Grace")
                    Html.text "Rename"
                ]
        ]

let private reactiveAttributes () : DomItem =
    let enabled = Var.create false

    Html.div
        [
            Html.button
                [
                    attr.id "target"
                    attr.title "literal"
                    attr.custom (
                        "data-state",
                        (fun () ->
                            if enabled.Value then
                                "on"
                            else
                                "off"
                        )
                    )
                    attr.custom (
                        "aria-label",
                        Signal.map
                            (fun on ->
                                if on then
                                    "enabled"
                                else
                                    "disabled"
                            )
                            enabled
                    )
                    attr.disabled (fun () -> enabled.Value)
                    attr.style (fun () ->
                        if enabled.Value then
                            "color: rgb(255, 0, 0)"
                        else
                            "color: rgb(0, 0, 255)"
                    )
                    Html.text "Target"
                ]
            Html.button
                [
                    attr.id "toggle"
                    on.click (fun _ -> enabled.Value <- not enabled.Value)
                    Html.text "Toggle"
                ]
        ]

let private innerHtml () : DomItem =
    let markup = Var.create "<b>bold</b>"

    Html.div
        [
            Html.div
                [
                    attr.id "static"
                    attr.innerHTML "<i>static</i>"
                ]
            Html.div
                [
                    attr.id "thunk"
                    attr.innerHTML (fun () -> "<span>" + markup.Value + "</span>")
                ]
            Html.div
                [
                    attr.id "signal"
                    attr.innerHTML markup
                ]
            Html.button
                [
                    attr.id "change"
                    on.click (fun _ -> markup.Value <- "<em>emphasis</em>")
                    Html.text "Change"
                ]
        ]

let private classList () : DomItem =
    let active = Var.create false
    let danger = Var.create false

    Html.div
        [
            Html.div
                [
                    attr.id "chip"
                    attr.className "chip"
                    attr.classList (fun () -> [ "is-active", active.Value ])
                    attr.toggleClass ("is-danger", (fun () -> danger.Value))
                    Html.text "chip"
                ]
            Html.button
                [
                    attr.id "active"
                    on.click (fun _ -> active.Value <- not active.Value)
                    Html.text "Active"
                ]
            Html.button
                [
                    attr.id "danger"
                    on.click (fun _ -> danger.Value <- not danger.Value)
                    Html.text "Danger"
                ]
        ]

let private eventBatching () : DomItem =
    let a = Var.create 0
    let b = Var.create 0
    let runs = Var.create 0

    Signal.effect (fun () ->
        a.Value + b.Value |> ignore
        runs.Value <- runs.Peek() + 1
    )
    |> ignore

    let writeBoth () =
        a.Value <- a.Value + 1
        b.Value <- b.Value + 1

    Html.div
        [
            Html.button
                [
                    attr.id "handler"
                    on.click (fun _ -> writeBoth ())
                    Html.text "From a handler"
                ]
            Html.button
                [
                    attr.id "timeout"
                    on.click (fun _ -> window.setTimeout (writeBoth, 0) |> ignore)
                    Html.text "From a timeout"
                ]
            Html.output
                [
                    attr.id "runs"
                    Html.text runs
                ]
        ]

let private fragment () : DomItem =
    Html.div
        [
            attr.id "parent"
            Html.fragment
                [
                    Html.span [ Html.text "a" ]
                    Html.span [ Html.text "b" ]
                    Html.span [ Html.text "c" ]
                ]
            Html.none
        ]

let private foreignNode () : DomItem =
    let em = document.createElement "em"
    em.textContent <- "adopted"

    Html.div
        [
            attr.id "host"
            Html.node em
        ]

let private reference () : DomItem =
    Html.input
        [
            attr.id "field"
            attr.ref (fun el -> el.setAttribute ("data-ref", el.tagName.ToLowerInvariant()))
        ]

let private svg () : DomItem =
    let ns = Var.create ""

    Html.div
        [
            Svg.svg [ Svg.rect [ attr.ref (fun el -> ns.Value <- el.namespaceURI) ] ]
            Html.output
                [
                    attr.id "ns"
                    Html.text ns
                ]
        ]

let private mountDisposal () : DomItem =
    let ticks = Var.create 0
    let cleanups = Var.create 0
    let observers = Var.create 0
    let mutable handle: IDisposable option = None

    let inner () =
        Signal.onCleanup (fun () -> cleanups.Value <- cleanups.Value + 1)

        Html.div
            [
                attr.id "inner"
                Html.text (fun () -> string ticks.Value)
            ]

    Html.div
        [
            Html.div [ attr.id "slot" ]

            Html.button
                [
                    attr.id "mount"
                    on.click (fun _ ->
                        if handle.IsNone then
                            handle <- Some(Html.mount "slot" inner)
                    )
                    Html.text "Mount"
                ]

            Html.button
                [
                    attr.id "dispose"
                    on.click (fun _ -> handle |> Option.iter (fun d -> d.Dispose()))
                    Html.text "Dispose"
                ]

            Html.button
                [
                    attr.id "tick"
                    on.click (fun _ -> ticks.Value <- ticks.Value + 1)
                    Html.text "Tick"
                ]

            // Snapshot on demand: `observerCount` reads no signal, so an effect
            // around it would never re-run.
            Html.button
                [
                    attr.id "measure"
                    on.click (fun _ -> observers.Value <- Signal.observerCount ticks.Signal)
                    Html.text "Measure"
                ]

            Html.output
                [
                    attr.id "cleanups"
                    Html.text (fun () -> string cleanups.Value)
                ]

            Html.output
                [
                    attr.id "observers"
                    Html.text (fun () -> string observers.Value)
                ]
        ]

let all: (string * (unit -> DomItem)) list =
    [
        "ReactiveText", reactiveText
        "ReactiveAttributes", reactiveAttributes
        "InnerHtml", innerHtml
        "ClassList", classList
        "EventBatching", eventBatching
        "Fragment", fragment
        "ForeignNode", foreignNode
        "Ref", reference
        "Svg", svg
        "MountDisposal", mountDisposal
    ]
