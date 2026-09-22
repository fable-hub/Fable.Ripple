module Main

open Fable.Core.JsInterop
open Browser
open Fable.Ripple.Dom

window?__loadId <- string (System.Random().Next(100000, 999999))

let private rowCount =
    match window.location.search with
    | s when s.StartsWith "?n=" -> int (s.Substring 3)
    | _ -> 0

let private app () =
    Html.div
        [
            attr.id "root"
            Counter.render ()
            Table.view rowCount
            Pair.solo ()
            Pair.both ()
            Rows.list ()
            Rows.switcher ()
        ]

Html.mount "app" app |> ignore
