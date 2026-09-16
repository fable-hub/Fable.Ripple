var e=`module Demo.Examples.Recipes.ActiveSearch

open Fable.Core
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// Debounce and throttle are timing policies dressed as combinators: how long,
// leading or trailing edge, does a pending call fire on teardown - and every
// answer is wrong for someone. So the library has neither.

let private catalogue =
    [
        "Reactive signals"
        "Signal combinators"
        "Signals under the hood"
        "Keyed list rendering"
        "URL codecs"
        "Scope disposal"
        "Attribute bindings"
        "Hash routing"
        "Equality cutoff"
    ]

let render () =
    let typed = Var.create ""
    let debounced = Var.create ""

    let keystrokes = Var.create 0
    let searches = Var.create 0
    let undebouncedSearches = Var.create 0

    // The whole operator, inline. \`pending\` is the handle to cancel, which is
    // what makes it a debounce rather than a queue of timers.
    let mutable pending = 0

    let schedule (text: string) =
        JS.clearTimeout pending

        pending <-
            JS.setTimeout
                (fun () ->
                    debounced.Value <- text
                    searches.Value <- searches.Value + 1
                )
                300

    // The scope owns the timer: navigate away mid-keystroke and it is cancelled
    // rather than firing into a disposed component.
    Signal.onCleanup (fun () -> JS.clearTimeout pending)

    let onType (text: string) =
        typed.Value <- text
        keystrokes.Value <- keystrokes.Value + 1
        undebouncedSearches.Value <- undebouncedSearches.Value + 1
        schedule text

    let results () =
        let needle = debounced.Value.ToLower()

        if needle = "" then
            []
        else
            catalogue |> List.filter (fun s -> s.ToLower().Contains needle)

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Type a word at normal speed. The debounced column searches once you pause; the other one searches on every keystroke."

            // demo-show
            Row.row
                [
                    Html.label
                        [
                            Html.text "search "

                            Html.input
                                [
                                    attr.value typed
                                    on.input (fun (v: string) -> onType v)
                                ]
                        ]

                    Button.action (
                        "Reset counters",
                        fun _ ->
                            keystrokes.Value <- 0
                            searches.Value <- 0
                            undebouncedSearches.Value <- 0
                    )
                ]

            Compare.compare
                [
                    Compare.side
                        "debounced"
                        "One request per pause in typing."
                        (Stack.stack
                            [
                                readout "searches" (fun () -> string searches.Value)
                                readout "term" (fun () -> debounced.Value)
                            ])

                    Compare.side
                        "undebounced"
                        "One request per keystroke - the same search, several times over."
                        (Stack.stack
                            [
                                readout "searches" (fun () -> string undebouncedSearches.Value)
                                readout "term" (fun () -> typed.Value)
                            ])

                ]

            readout "keystrokes" (fun () -> string keystrokes.Value)

            Html.ul
                [
                    attr.className "moves-list"

                    Html.each (fun () -> results () |> List.toArray) id (fun s -> Html.li s)
                ]

            Html.show (
                (fun () -> debounced.Value <> "" && List.isEmpty (results ())),
                fun () -> Html.p [ Html.small "no matches" ]
            )

        ]
`;export{e as default};