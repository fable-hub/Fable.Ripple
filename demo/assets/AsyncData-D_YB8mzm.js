var e=`module Demo.Examples.Recipes.AsyncData

open Fable.Core
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// No \`createResource\`, no \`Suspense\`, no \`useQuery\`. Two pieces, and the second
// is the one people get wrong.
//
// 1. A \`RemoteData\` DU as the state, so the view is a \`match\` and "loading AND
//    has previous data" is representable rather than two booleans.
//
// 2. A GENERATION COUNTER. Requests come back out of order: ask for #3, then #4,
//    and #3 may answer last. Without a guard it wins and the screen shows the
//    wrong thing, silently. Every request captures the generation it was issued
//    under and drops its own result if a newer one has started.

type private RemoteData<'T> =
    | NotAsked
    | Loading
    /// Keeps the previous value, so a refresh does not blank the screen.
    | Reloading of 'T
    | Loaded of 'T
    | Failed of string

/// Stands in for a server. \`delay\` is per-request on purpose: that is what makes
/// responses arrive out of order.
let private fetchUser (id: int) (delay: int) (fail: bool) : JS.Promise<string> =
    Promise.create (fun resolve reject ->
        JS.setTimeout
            (fun () ->
                if fail then
                    reject (exn (sprintf "user %d is unavailable" id))
                else
                    resolve (sprintf "user %d" id)
            )
            delay
        |> ignore
    )

let render () =
    let guarded = Var.create NotAsked
    let unguarded = Var.create NotAsked

    let log = Log()

    // The guard. Bumped on every request; a response is only accepted if the
    // generation it captured is still the current one.
    let mutable generation = 0

    let load (id: int) (delay: int) (fail: bool) =
        generation <- generation + 1
        let mine = generation

        guarded.Value <-
            match guarded.Value with
            | Loaded previous
            | Reloading previous -> Reloading previous
            | _ -> Loading

        unguarded.Value <- Loading

        fetchUser id delay fail
        |> Promise.map (fun value ->
            unguarded.Value <- Loaded value

            if mine = generation then
                guarded.Value <- Loaded value
                log.Add(sprintf "#%d accepted (%s)" mine value)
            else
                log.Add(sprintf "#%d DROPPED - #%d is newer" mine generation)
        )
        |> Promise.catch (fun error ->
            unguarded.Value <- Failed error.Message

            if mine = generation then
                guarded.Value <- Failed error.Message
                log.Add(sprintf "#%d failed" mine)
            else
                log.Add(sprintf "#%d dropped a failure" mine)
        )
        |> ignore

    // The race, in one click: a slow request then a fast one.
    let race () =
        log.Clear()
        load 3 900 false
        JS.setTimeout (fun () -> load 4 100 false) 50 |> ignore

    let describe (d: RemoteData<string>) =
        match d with
        | NotAsked -> "not asked"
        | Loading -> "loading..."
        | Reloading previous -> previous + "  (refreshing)"
        | Loaded value -> value
        | Failed message -> "failed: " + message

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Load a user, then a failing one, then press the race. The slow first request answers last. Only the column with the generation guard ignores it and keeps the newer user."

            // demo-show
            Row.row
                [
                    Button.primary (
                        "Load a user",
                        fun _ ->
                            log.Clear()
                            load 1 400 false
                    )

                    Button.action (
                        "Load one that fails",
                        fun _ ->
                            log.Clear()
                            load 2 400 true
                    )

                    Button.action ("Race: slow #3, then fast #4", fun _ -> race ())
                ]

            Compare.compare
                [
                    Compare.side
                        "with the generation guard"
                        "A response that is no longer the newest is dropped, so a slow first request cannot overwrite a fast second."
                        (Stack.stack
                            [
                                readout "state" (fun () -> describe guarded.Value)

                                // The four cases are a \`match\`, so the view cannot forget
                                // one - and \`Reloading\` keeps the old value on screen.
                                Html.dynamic (fun () ->
                                    match guarded.Value with
                                    | Failed message ->
                                        Html.p
                                            [
                                                attr.className "error"
                                                Html.text message
                                            ]
                                    | Reloading previous ->
                                        Html.p
                                            [
                                                Html.small
                                                    [ Html.text ("still showing " + previous) ]
                                            ]
                                    | _ -> Html.none
                                )
                            ])

                    Compare.side
                        "without it"
                        "Whichever response lands last wins, even when it answers an older question."
                        (Stack.stack
                            [
                                readout "state" (fun () -> describe unguarded.Value)
                                Html.p
                                    [
                                        Html.small
                                            [
                                                Html.text
                                                    "last response to arrive, whichever it was"
                                            ]
                                    ]
                            ])

                ]

            logPane log
        ]
`;export{e as default};