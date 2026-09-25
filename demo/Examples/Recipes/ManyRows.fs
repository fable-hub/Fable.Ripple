module Demo.Examples.Recipes.ManyRows

open Fable.Core.JsInterop
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// The js-framework-benchmark row shape: each row's LABEL is its own `Var`,
// and there is one shared `selected`. So "update every 10th row" writes 100
// signals and touches 100 text nodes - it does not re-render 1,000 rows and
// diff them, because there is no render pass to re-run.
//
// `swap rows` is the interesting one. Two rows exchange places in a 1,000-row
// list, and the keyed reconciler moves exactly two elements. Control Flow /
// Minimal DOM moves counts them with a MutationObserver if you want the proof.
//
// Timings are wall-clock around a synchronous write. A click handler runs
// inside `Signal.batch`, where a write only marks and the flush happens after
// the handler returns, so `timed` defers to a macrotask to be outside it.

type private Row =
    {
        Id: int
        Label: Var<string>
    }

let private adjectives =
    [|
        "pretty"
        "large"
        "big"
        "small"
        "tall"
        "short"
        "long"
        "handsome"
        "plain"
        "quaint"
    |]

let private colours =
    [|
        "red"
        "yellow"
        "blue"
        "green"
        "pink"
        "brown"
        "purple"
        "white"
        "black"
        "orange"
    |]

let private nouns =
    [|
        "table"
        "chair"
        "house"
        "bbq"
        "desk"
        "car"
        "pony"
        "cookie"
        "sandwich"
        "burger"
    |]

/// `performance.now()` is not in Fable's DOM bindings - one line of interop.
let private now () : float = emitJsExpr () "performance.now()"

let private rnd = System.Random()
let private pick (a: string[]) = a.[rnd.Next a.Length]

let private label () =
    pick adjectives + " " + pick colours + " " + pick nouns

let render () =
    let rows = Var.create ([||]: Row[])
    let selected = Var.create 0
    let mutable nextId = 1
    let timings = Log()

    let timed name f =
        Browser.Dom.window.setTimeout (
            (fun () ->
                let before = now ()
                f ()
                let elapsed = now () - before
                timings.Add(sprintf "%s: %.1f ms" name elapsed)
            ),
            0
        )
        |> ignore

    let build n =
        Array.init
            n
            (fun _ ->
                let id = nextId
                nextId <- nextId + 1

                {
                    Id = id
                    Label = Var.create (label ())
                }
            )

    let run n =
        timed (sprintf "create %d" n) (fun () -> rows.Value <- build n)

    let append n =
        timed (sprintf "append %d" n) (fun () -> rows.Value <- Array.append (rows.Value) (build n))

    // Writes 100 Vars. No list is rebuilt and no row is re-created.
    let updateEveryTenth () =
        timed
            "update every 10th"
            (fun () ->
                let current = rows.Value

                let mutable i = 0

                while i < current.Length do
                    current.[i].Label.Value <- current.[i].Label.Value + " !!!"
                    i <- i + 10
            )

    let swapRows () =
        timed
            "swap rows 1 and 998"
            (fun () ->
                let current = Array.copy (rows.Value)

                if current.Length > 998 then
                    let a = current.[1]
                    current.[1] <- current.[998]
                    current.[998] <- a
                    rows.Value <- current
            )

    let clear () =
        timed "clear" (fun () -> rows.Value <- [||])

    let remove (id: int) =
        timed
            "remove one"
            (fun () -> rows.Value <- rows.Value |> Array.filter (fun r -> r.Id <> id))

    let row (r: Row) =
        Html.tr
            [
                attr.classList (fun () -> [ "is-selected", selected.Value = r.Id ])

                Html.td
                    [
                        attr.className "row-id"
                        Html.text (string r.Id)
                    ]

                Html.td
                    [
                        Html.a
                            [
                                on.click (fun _ -> selected.Value <- r.Id)
                                // The only reactive part of a row: one text node.
                                Html.text r.Label
                            ]
                    ]

                Html.td [ Button.action ("x", fun _ -> remove r.Id) ]
            ]

    Stack.stack
        [
            // demo-hide
            Try.observe
                "Create 10,000 rows, then update and swap. Both take a fraction of the create time, because neither one rebuilds the list."

            // demo-show
            Row.row
                [
                    Button.primary ("Create 1,000", fun _ -> run 1000)
                    Button.action ("Create 10,000", fun _ -> run 10000)
                    Button.action ("Append 1,000", fun _ -> append 1000)
                    Button.action ("Update every 10th", fun _ -> updateEveryTenth ())
                    Button.action ("Swap rows", fun _ -> swapRows ())
                    Button.action ("Clear", fun _ -> clear ())
                ]

            // demo-hide
            Row.row
                [
                    readout "rows" (fun () -> string (Array.length rows.Value))
                    readout "selected" (fun () -> string selected.Value)
                ]

            // demo-show
            logPane timings

            Html.div
                [
                    attr.className "scroll-window"

                    Html.table
                        [
                            attr.className "bench-table"
                            Html.tbody [ Html.each (fun () -> rows.Value) (fun r -> r.Id) row ]
                        ]
                ]

        ]
