module App

// js-framework-benchmark keyed entry for Fable.Ripple + Fable.Ripple.Dom.
// Wires the standard buttons (#run/#runlots/#add/#update/#clear/#swaprows) and
// renders the reactive keyed row list into the existing #tbody. Each row's label
// and the selected highlight are fine-grained reactive (a `Var` per label, one
// shared `selected`), so update/select/remove touch only the affected rows.

open Browser
open Browser.Types
open Fable.Ripple
open Fable.Ripple.Dom

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
        "clean"
        "elegant"
        "easy"
        "angry"
        "crazy"
        "helpful"
        "mushy"
        "odd"
        "unsightly"
        "adorable"
        "important"
        "inexpensive"
        "cheap"
        "expensive"
        "fancy"
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
        "brown"
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
        "pizza"
        "mouse"
        "keyboard"
    |]

let private rnd = System.Random()
let private pick (a: string[]) = a.[rnd.Next a.Length]

let private label () =
    pick adjectives + " " + pick colours + " " + pick nouns

let mutable private nextId = 1
let private rows = Var.create ([||]: Row[])
let private selected = Var.create 0 // 0 = none; ids start at 1

let private buildRows (n: int) =
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

let private removeRow (id: int) =
    rows.Value <- rows.Value |> Array.filter (fun r -> r.Id <> id)

let private renderRow (r: Row) : HTMLElement =
    Html.render (
        Html.tr
            [
                attr.className (fun () ->
                    if selected.Value = r.Id then
                        "danger"
                    else
                        ""
                )
                Html.td
                    [
                        attr.className "col-md-1"
                        Html.text (string r.Id)
                    ]
                Html.td
                    [
                        attr.className "col-md-4"
                        Html.a
                            [
                                on.click (fun _ -> selected.Value <- r.Id)
                                Html.text (fun () -> r.Label.Value)
                            ]
                    ]
                Html.td
                    [
                        attr.className "col-md-1"
                        Html.a
                            [
                                on.click (fun _ -> removeRow r.Id)
                                Html.span
                                    [
                                        attr.className "glyphicon glyphicon-remove"
                                        attr.custom ("aria-hidden", "true")
                                    ]
                            ]
                    ]
                Html.td [ attr.className "col-md-6" ]
            ]
    )

let private replace (n: int) =
    Signal.batch (fun () ->
        selected.Value <- 0
        rows.Value <- buildRows n
    )

let private append (n: int) =
    rows.Value <- Array.append rows.Value (buildRows n)

let private updateEvery10th () =
    let rs = rows.Value
    let mutable i = 0

    while i < rs.Length do
        rs.[i].Label.Value <- rs.[i].Label.Value + " !!!"
        i <- i + 10

let private swap () =
    let rs = rows.Value

    if rs.Length > 998 then
        let copy = Array.copy rs
        let t = copy.[1]
        copy.[1] <- copy.[998]
        copy.[998] <- t
        rows.Value <- copy

let private on (id: string) (f: unit -> unit) =
    (document.getElementById id).addEventListener ("click", fun _ -> f ())

// Mount the reactive keyed list into the pre-existing #tbody, then wire buttons.
Dom.keyedEach
    (document.getElementById "tbody")
    null
    (fun () -> rows.Value)
    (fun r -> r.Id)
    renderRow
|> ignore

on "run" (fun () -> replace 1000)
on "runlots" (fun () -> replace 10000)
on "add" (fun () -> append 1000)
on "update" updateEvery10th
on "clear" (fun () -> replace 0)
on "swaprows" swap
