var e=`module Demo.Examples.ControlFlow.Switch

open Fable.Core
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

type private Tab =
    | Profile
    | Settings

let private title =
    function
    | Profile -> "Profile"
    | Settings -> "Settings"

let render () =
    let tab = Var.create Profile
    let seconds = Var.create 0

    let timer = JS.setInterval (fun () -> seconds.Value <- seconds.Value + 1) 1000
    Signal.onCleanup (fun () -> JS.clearInterval timer)

    let dynamicBuilds = Var.create 0 // demo-hide-line
    let switchBuilds = Var.create 0 // demo-hide-line

    let tabButton (target: Tab) =
        Html.button
            [
                attr.custom (
                    "aria-current",
                    (fun () ->
                        if tab.Value = target then
                            "true"
                        else
                            "false"
                    )
                )
                on.click (fun _ -> tab.Value <- target)
                Html.text (title target)
            ]

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Type in both boxes and wait a second. The left box empties on every tick, because its \`Html.dynamic\` body reads \`seconds\`. The right box keeps your text: \`Html.switch\` rebuilds only when \`tab\` changes, and the clock updates in place."

            // demo-show
            Html.div
                [
                    attr.role "group"
                    tabButton Profile
                    tabButton Settings
                ]

            Html.div
                [
                    Html.article
                        [
                            Html.header "Html.dynamic"

                            Html.dynamic (fun () ->
                                dynamicBuilds.Value <- dynamicBuilds.Peek() + 1 // demo-hide-line
                                let current = tab.Value
                                let s = seconds.Value

                                Html.div
                                    [
                                        Html.p
                                            [
                                                Html.text (
                                                    sprintf
                                                        "%s tab, %d s since the page opened"
                                                        (title current)
                                                        s
                                                )
                                            ]
                                        Html.input [ attr.placeholder "Type here" ]
                                    ]
                            )

                            readout "rebuilds" (fun () -> string dynamicBuilds.Value) // demo-hide-line
                        ]

                    Html.article
                        [
                            Html.header "Html.switch"

                            Html.switch
                                tab
                                (fun current ->
                                    switchBuilds.Value <- switchBuilds.Value + 1 // demo-hide-line

                                    Html.div
                                        [
                                            Html.p
                                                [
                                                    Html.text (fun () ->
                                                        sprintf
                                                            "%s tab, %d s since the page opened"
                                                            (title current)
                                                            seconds.Value
                                                    )
                                                ]
                                            Html.input [ attr.placeholder "Type here" ]
                                        ]
                                )

                            readout "rebuilds" (fun () -> string switchBuilds.Value) // demo-hide-line
                        ]
                ]
        ]
`;export{e as default};