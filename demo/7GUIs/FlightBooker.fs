module Demo.SevenGUIs.FlightBooker

open System
open Fable.Ripple
open Fable.Ripple.Dom

// 7GUIs #3 - Flight Booker: one-way / return, date validation, book button.
let private parseDate (s: string) =
    match s.Split('.') with
    | [| d; m; y |] ->
        match Int32.TryParse d, Int32.TryParse m, Int32.TryParse y with
        | (true, dd), (true, mm), (true, yy) when dd >= 1 && dd <= 31 && mm >= 1 && mm <= 12 ->
            try
                Some(DateTime(yy, mm, dd))
            with _ ->
                None
        | _ -> None
    | _ -> None

let render () =
    let mode = Var.create "one-way" // "one-way" | "return"
    let start = Var.create "27.03.2014"
    let ret = Var.create "27.03.2014"
    let booked = Var.create ""

    let isReturn () = mode.Value = "return"
    let startOk () = (parseDate start.Value).IsSome
    let retOk () = (parseDate ret.Value).IsSome

    let canBook () =
        match parseDate start.Value with
        | None -> false
        | Some s ->
            if isReturn () then
                match parseDate ret.Value with
                | Some r -> r >= s
                | None -> false
            else
                true

    let book () =
        booked.Value <-
            if isReturn () then
                sprintf "You have booked a return flight %s -> %s." start.Value ret.Value
            else
                sprintf "You have booked a one-way flight on %s." start.Value

    // An empty `aria-invalid` attribute means *true*, so emit the literal "false".
    let invalid valid () =
        if valid () then
            "false"
        else
            "true"

    Html.div
        [
            Html.div
                [
                    attr.className "stack is-narrow"

                    Html.label
                        [
                            Html.text "Trip"
                            Html.select
                                [
                                    on.change (fun v -> mode.Value <- v)
                                    Html.option "one-way"
                                    Html.option "return"
                                ]
                        ]

                    Html.label
                        [
                            Html.text "Start date"
                            Html.input
                                [
                                    attr.bindValue start
                                    attr.custom ("aria-invalid", invalid startOk)
                                ]
                        ]

                    Html.label
                        [
                            Html.text "Return date"
                            Html.input
                                [
                                    attr.bindValue ret
                                    attr.disabled (fun () -> not (isReturn ()))
                                    // A disabled field is never reported invalid.
                                    attr.custom (
                                        "aria-invalid",
                                        invalid (fun () -> not (isReturn ()) || retOk ())
                                    )
                                ]
                        ]

                    Html.button
                        [
                            attr.className "primary"
                            on.click (fun _ -> book ())
                            attr.disabled (fun () -> not (canBook ()))
                            Html.text "Book"
                        ]

                    Html.output
                        [
                            attr.className "affirm"
                            Html.text booked
                        ]
                ]
        ]
