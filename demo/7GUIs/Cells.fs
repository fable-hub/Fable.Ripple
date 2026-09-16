module Demo.SevenGUIs.Cells

open System
open System.Collections.Generic
open Browser
open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom

// 7GUIs #7 - Cells: a spreadsheet, to spec - rows 0-99, columns A-Z, scrollable,
// and double-clicking a cell reveals its formula.
//
// Each cell's raw text is a `Var<string>`; its computed value is a memoised
// computed whose formula reads the referenced cells' computed values - so a cell
// reference *is* a reactive dependency. Editing one cell recomputes only its
// dependents, with no manual wiring. That is exactly what the task asks for
// ("one should not just recompute the value of every cell"), and here it falls
// out of auto-tracking rather than being implemented.
//
// Known gap: a cycle (`A0` containing `=A0`) recurses without a depth guard.
// The task does not require cycle handling and adding one would obscure the
// point of the demo, so it is called out rather than papered over.
let render () =
    let cols = [| 'A' .. 'Z' |]
    let rows = [| 0..99 |]

    let keys =
        [
            for c in cols do
                for r in rows do
                    sprintf "%c%d" c r
        ]

    let raw = Dictionary<string, Var<string>>()
    // Value read behind a thunk: keeps the memoised computed (reactive), while
    // avoiding `Signal<'T>` as a generic arg - the erased Signal type has no runtime
    // representation, so Fable emits a dangling `Signal$1` import for it here.
    let computed = Dictionary<string, unit -> float>()

    // One Var for the whole grid rather than one per cell: editing is mutually
    // exclusive, so 2,600 booleans would be 2,599 wasted signals.
    let editing = Var.create (None: string option)

    for k in keys do
        raw.[k] <- Var.create ""

    // Formula grammar: sum of +/- terms, each term a product of *-factors; a
    // factor is a number or an uppercase cell reference (e.g. =A1*2+B1).
    let evalFactor (f: string) =
        let f = f.Trim()

        if f = "" then
            0.0
        elif Char.IsLetter f.[0] then
            match computed.TryGetValue(f.ToUpper()) with
            | true, c -> c ()
            | _ -> 0.0
        else
            match Double.TryParse f with
            | true, v -> v
            | _ -> 0.0

    let evalTerm (t: string) =
        t.Split('*') |> Array.fold (fun acc f -> acc * evalFactor f) 1.0

    let evalFormula (expr: string) =
        let mutable sum = 0.0
        let mutable sign = 1.0
        let mutable cur = ""

        let flush () =
            if cur.Trim() <> "" then
                sum <- sum + sign * evalTerm cur

            cur <- ""

        for ch in expr do
            if ch = '+' then
                flush ()
                sign <- 1.0
            elif ch = '-' then
                flush ()
                sign <- -1.0
            else
                cur <- cur + string ch

        flush ()
        sum

    for k in keys do
        let cell =
            Signal.computed (fun () ->
                let s = (raw.[k]).Value.Trim()

                if s.StartsWith "=" then
                    evalFormula (s.Substring 1)
                elif s = "" then
                    0.0
                else
                    match Double.TryParse s with
                    | true, v -> v
                    | _ -> 0.0
            )

        computed.[k] <- fun () -> cell.Value

    // Seed a dependency chain: C0 = A0 + B0, C1 = A0*2, C2 = C0 - C1.
    (raw.["A0"]).Value <- "5"
    (raw.["B0"]).Value <- "10"
    (raw.["C0"]).Value <- "=A0+B0"
    (raw.["C1"]).Value <- "=A0*2"
    (raw.["C2"]).Value <- "=C0-C1"

    let fmt (v: float) =
        if Double.IsNaN v then
            ""
        elif v = floor v then
            string (int v)
        else
            sprintf "%.2f" v

    // A cell shows its value; double-clicking swaps in the formula for editing,
    // and Enter / Escape / blur put it back.
    let cell (key: string) : DomItem =
        Html.td
            [
                Html.show (
                    (fun () -> editing.Value = Some key),
                    (fun () ->
                        Html.input
                            [
                                attr.value (raw.[key])
                                attr.aria ("label", key)
                                attr.ref (fun el ->
                                    window.setTimeout ((fun () -> el.focus ()), 0) |> ignore
                                )
                                on.input (fun v -> (raw.[key]).Value <- v)
                                on.keyDown (fun e ->
                                    match e.key with
                                    | "Enter"
                                    | "Escape" -> editing.Value <- None
                                    | _ -> ()
                                )
                                on.blur (fun _ -> editing.Value <- None)
                            ]
                    ),
                    (fun () ->
                        Html.span
                            [
                                attr.className "cell-value"
                                attr.title "Double-click to edit"
                                on.dblClick (fun _ -> editing.Value <- Some key)
                                Html.text (fun () ->
                                    if (raw.[key]).Value.Trim() = "" then
                                        ""
                                    else
                                        fmt ((computed.[key]) ())
                                )
                            ]
                    )
                )
            ]

    Html.div
        [
            attr.className "stack"

            Html.p
                [
                    Html.small
                        [
                            Html.text
                                "Double-click a cell to edit its formula, e.g. =A0+B0 or =A0*2. Edit a referenced cell and only its dependents recompute."
                        ]
                ]

            Html.div
                [
                    attr.className "sheet-scroll"

                    Html.table
                        [
                            attr.className "sheet"

                            Html.thead
                                [
                                    Html.tr
                                        [
                                            yield Html.th ""

                                            for c in cols do
                                                yield Html.th [ Html.text (string c) ]
                                        ]
                                ]

                            Html.tbody
                                [
                                    for r in rows do
                                        yield
                                            Html.tr
                                                [
                                                    yield Html.th [ Html.text (string r) ]

                                                    for c in cols do
                                                        yield cell (sprintf "%c%d" c r)
                                                ]
                                ]
                        ]
                ]
        ]
