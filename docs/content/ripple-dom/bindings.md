---
title: Bindings
---

`attr.bindValue`{fsharp} and `attr.bindChecked`{fsharp} keep an input and a `Var`{fsharp} in sync, both directions: typing writes the signal, and writing the signal updates the field.

They take a `Var`{fsharp} specifically - a read-only `Signal`{fsharp} would have nothing to write back to, and that is a compile error.

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let name = Var.create "ada"
    let agreed = Var.create false

    Html.div
        [
            Html.label
                [
                    Html.text "name"
                    Html.input [ attr.bindValue name ]
                ]

            Html.label
                [
                    Html.input
                        [
                            attr.type' "checkbox"
                            attr.bindChecked agreed
                        ]
                    Html.text "agree"
                ]

            Html.p [ Html.text (fun () -> $"%s{name.Value} - agreed: %b{agreed.Value}") ]

            Html.button
                [
                    on.click (fun _ ->
                        name.Value <- "grace"
                        agreed.Value <- true
                    )
                    Html.text "Set from code"
                ]
        ]

Html.mount "app" app |> ignore
```

Type in the field and the readout follows; press the button and the field follows. One line, both directions.

## The manual pair

`attr.value`{fsharp} plus `on.input`{fsharp} is the long form, for when the two directions are not symmetric: validation, normalising, or refusing a value outright. This field accepts only integers:

```fsharp live preset=app
open Fable.Ripple
open Fable.Ripple.Dom

let app () =
    let quantity = Var.create 1

    Html.div
        [
            Html.input
                [
                    attr.value (fun () -> string quantity.Value)
                    on.input (fun (value: string) ->
                        match System.Int32.TryParse value with
                        | true, n -> quantity.Value <- n
                        | _ -> ()
                    )
                ]
            Html.p [ Html.text (fun () -> $"quantity: %i{quantity.Value}") ]
        ]

Html.mount "app" app |> ignore
```

Anything that does not parse is refused: the signal keeps its last good value.
