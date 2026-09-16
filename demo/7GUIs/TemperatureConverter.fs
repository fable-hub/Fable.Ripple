module Demo.SevenGUIs.TemperatureConverter

open System
open Fable.Ripple
open Fable.Ripple.Dom

// 7GUIs #2 - Temperature Converter: two fields that convert into each other.
let render () =
    let celsius = Var.create ""
    let fahrenheit = Var.create ""
    let cValid = Var.create true
    let fValid = Var.create true

    let setC (v: string) =
        celsius.Value <- v

        match Double.TryParse v with
        | true, c ->
            fahrenheit.Value <- string (c * 9.0 / 5.0 + 32.0)
            cValid.Value <- true
        | _ -> cValid.Value <- (v = "")

    let setF (v: string) =
        fahrenheit.Value <- v

        match Double.TryParse v with
        | true, f ->
            celsius.Value <- string ((f - 32.0) * 5.0 / 9.0)
            fValid.Value <- true
        | _ -> fValid.Value <- (v = "")

    // An empty `aria-invalid` attribute means *true*, so emit the literal "false".
    let invalid (valid: Var<bool>) () =
        if valid.Value then
            "false"
        else
            "true"

    Html.div
        [
            Html.div
                [
                    attr.className "row"

                    Html.label
                        [
                            Html.text "Celsius"
                            Html.input
                                [
                                    attr.type' "number"
                                    attr.value celsius
                                    attr.custom ("aria-invalid", invalid cValid)
                                    on.input setC
                                ]
                        ]

                    Html.span
                        [
                            attr.className "eq"
                            Html.text "="
                        ]

                    Html.label
                        [
                            Html.text "Fahrenheit"
                            Html.input
                                [
                                    attr.type' "number"
                                    attr.value fahrenheit
                                    attr.custom ("aria-invalid", invalid fValid)
                                    on.input setF
                                ]
                        ]
                ]
        ]
