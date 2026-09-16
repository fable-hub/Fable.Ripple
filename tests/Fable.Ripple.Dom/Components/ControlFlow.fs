module Fable.Ripple.Dom.Test.Components.ControlFlow

open Fable.Ripple
open Fable.Ripple.Dom

let private counter (id: string) (value: Var<int>) =
    Html.output
        [
            attr.id id
            Html.text value
        ]

let private button (id: string) (onClick: unit -> unit) =
    Html.button
        [
            attr.id id
            on.click (fun _ -> onClick ())
            Html.text id
        ]

let private show () : DomItem =
    let visible = Var.create false
    let builds = Var.create 0
    let cleanups = Var.create 0

    Html.div
        [
            button "toggle" (fun () -> visible.Value <- not visible.Value)

            Html.show (
                visible,
                fun () ->
                    builds.Value <- builds.Peek() + 1
                    Signal.onCleanup (fun () -> cleanups.Value <- cleanups.Peek() + 1)

                    Html.div
                        [
                            attr.id "panel"
                            Html.input [ attr.id "draft" ]
                        ]
            )

            counter "builds" builds
            counter "cleanups" cleanups
        ]

let private showFallback () : DomItem =
    let visible = Var.create false

    Html.div
        [
            button "toggle" (fun () -> visible.Value <- not visible.Value)

            Html.show (
                visible,
                (fun () ->
                    Html.p
                        [
                            attr.id "on"
                            Html.text "on"
                        ]
                ),
                fun () ->
                    Html.p
                        [
                            attr.id "off"
                            Html.text "off"
                        ]
            )
        ]

type private Status =
    | Idle
    | Loading of pct: int

let private switch () : DomItem =
    let status = Var.create Idle
    let other = Var.create 0
    let builds = Var.create 0

    Html.div
        [
            button "idle" (fun () -> status.Value <- Idle)
            button "loading-40" (fun () -> status.Value <- Loading 40)
            button "loading-50" (fun () -> status.Value <- Loading 50)
            button "bump-other" (fun () -> other.Value <- other.Value + 1)

            Html.switch
                status
                (fun s ->
                    builds.Value <- builds.Value + 1

                    match s with
                    | Idle ->
                        Html.p
                            [
                                attr.id "arm"
                                Html.text "idle"
                            ]
                    | Loading pct ->
                        Html.div
                            [
                                Html.p
                                    [
                                        attr.id "arm"
                                        Html.text (sprintf "loading %d" pct)
                                    ]
                                Html.output
                                    [
                                        attr.id "snapshot"
                                        Html.text (string other.Value)
                                    ]
                                Html.output
                                    [
                                        attr.id "live"
                                        Html.text (fun () -> string other.Value)
                                    ]
                            ]
                )

            counter "builds" builds
        ]

let private switchWith () : DomItem =
    let a = Var.create false
    let b = Var.create false
    let builds = Var.create 0

    Html.div
        [
            button "toggle-a" (fun () -> a.Value <- not a.Value)
            button "toggle-b" (fun () -> b.Value <- not b.Value)

            Html.switchWith (
                (fun () -> a.Value && b.Value),
                fun both ->
                    builds.Value <- builds.Value + 1

                    Html.p
                        [
                            attr.id "both"
                            Html.text (
                                if both then
                                    "both"
                                else
                                    "not both"
                            )
                        ]
            )

            counter "builds" builds
        ]

let private dynamic () : DomItem =
    let a = Var.create 0
    let b = Var.create 0
    let builds = Var.create 0

    Html.div
        [
            button "bump-a" (fun () -> a.Value <- a.Value + 1)
            button "bump-b" (fun () -> b.Value <- b.Value + 1)

            Html.dynamic (fun () ->
                builds.Value <- builds.Peek() + 1

                Html.p
                    [
                        attr.id "sum"
                        Html.text (string (a.Value + b.Value))
                    ]
            )

            counter "builds" builds
        ]

let private effectDisposal () : DomItem =
    let visible = Var.create true
    let source = Var.create 0
    let runs = Var.create 0

    Html.div
        [
            button "bump" (fun () -> source.Value <- source.Value + 1)
            button "hide" (fun () -> visible.Value <- false)

            Html.show (
                visible,
                fun () ->
                    Signal.effect (fun () ->
                        source.Value |> ignore
                        runs.Value <- runs.Peek() + 1
                    )
                    |> ignore

                    Html.p
                        [
                            attr.id "branch"
                            Html.text "branch"
                        ]
            )

            counter "runs" runs
        ]

let all: (string * (unit -> DomItem)) list =
    [
        "Show", show
        "ShowFallback", showFallback
        "Switch", switch
        "SwitchWith", switchWith
        "Dynamic", dynamic
        "EffectDisposal", effectDisposal
    ]
