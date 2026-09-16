---
title: Async data
---

Async data is two pieces of plain F#: a union for the state, and a generation counter so a late response cannot overwrite a newer one.

:::note Not in the menu
This page describes a pattern you assemble yourself, not library API. It is kept for reference while the approach settles, and may change or be replaced by a dedicated primitive.
:::

## A state that can say what it means

A `RemoteData`{fsharp} union as the state, so the view is a `match`{fsharp} - and "loading, but with previous data to keep showing" is representable rather than two booleans:

```fsharp
type RemoteData<'T> =
    | NotAsked
    | Loading
    | Reloading of 'T
    | Loaded of 'T
    | Failed of string
```

## A generation counter

Requests come back out of order: ask for #3, then #4, and #3 may answer last. Without a guard it wins and the screen shows the wrong thing, silently. Every request captures the generation it was issued under and drops its own result if a newer one has started.

Press the race button - the slow request resolves last, and is dropped:

```fsharp live preset=app
open Fable.Core
open Fable.Ripple
open Fable.Ripple.Dom

type RemoteData<'T> =
    | NotAsked
    | Loading
    | Reloading of 'T
    | Loaded of 'T

let fetchUser (id: int) (delay: int) : JS.Promise<string> =
    Promise.create (fun resolve _ ->
        JS.setTimeout (fun () -> resolve $"user %d{id}") delay |> ignore
    )

let app () =
    let user = Var.create NotAsked
    let mutable generation = 0

    let load (id: int) (delay: int) =
        generation <- generation + 1
        let mine = generation

        user.Value <-
            match user.Value with
            | Loaded previous
            | Reloading previous -> Reloading previous
            | _ -> Loading

        fetchUser id delay
        |> Promise.iter (fun value ->
            if mine = generation then
                user.Value <- Loaded value
        )

    Html.div
        [
            Html.div
                [
                    Html.button
                        [
                            on.click (fun _ -> load 1 400)
                            Html.text "Load user 1"
                        ]
                    Html.button
                        [
                            on.click (fun _ ->
                                load 3 900
                                load 4 150
                            )
                            Html.text "Race: slow #3, then fast #4"
                        ]
                ]

            Html.switch
                user
                (function
                | NotAsked -> Html.p "Not asked yet."
                | Loading -> Html.p "Loading..."
                | Reloading previous -> Html.p $"Refreshing - still showing %s{previous}."
                | Loaded value -> Html.p $"Loaded: %s{value}.")
        ]

Html.mount "app" (app ())
```

The race lands on `user 4` - request #3 resolves afterwards, finds `generation`{fsharp} has moved on, and throws its own result away. Without the `mine = generation`{fsharp} check it would overwrite the newer answer.

`Reloading`{fsharp} is why a refresh does not blank the screen: the previous value rides along in the state, and the view can keep showing it.

A failure is one more case (`Failed of string`{fsharp}) and one more `match`{fsharp} arm - the demo application's Async data example shows the full shape with errors.
