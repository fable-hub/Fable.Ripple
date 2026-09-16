module Demo.Examples.Rendering.Elements

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line

// `Html.text` has three overloads, all three used below:
//
//   text "hello"              a literal string, no binding created
//   text (fun () -> ...)      a thunk, re-run when signals it reads change
//   text someVarOrSignal      SRTP - takes a Var or a Signal of any type

let render () =
    let count = Var.create 2

    let items =
        Var.create
            [
                "alpha"
                "beta"
            ]

    Html.fragment
        [
            // demo-hide
            Try.observe
                "Press `Add one`. The count follows, but the list above it does not: a plain `for` builds the list once. A list that updates is `Html.each`, in Control Flow."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.button
                        [
                            on.click (fun _ -> count.Value <- count.Value + 1)
                            Html.text "Add one"
                        ]

                    Html.button
                        [
                            attr.disabled (fun () -> count.Value <= 0)
                            on.click (fun _ -> count.Value <- count.Value - 1)
                            Html.text "Remove one"
                        ]
                ]

            // demo-hide
            Html.div
                [
                    attr.role "group"

                    // 1 - a literal. No signal is read, so no binding is made.
                    readout "literal" (fun () -> "static")

                    // 2 - a thunk. Anything it reads becomes a dependency.
                    readout "thunk" (fun () -> sprintf "%d item(s)" count.Value)
                ]

            // demo-show
            // 3 - the SRTP overload: the Var itself, no lambda and no `.Signal`.
            Html.p
                [
                    Html.text "count = "
                    Html.text count
                ]

            // The list is a list, so F# control flow builds it. This is the
            // whole reason there is no `<For>` component for static content.
            Html.ul
                [
                    for i in 1 .. count.Peek() do
                        Html.li [ Html.text (sprintf "row %d" i) ]

                    // `yield!` splices a DomItem list - so does Html.fragment,
                    // which is the reactive equivalent.
                    yield! (items.Peek() |> List.map (fun s -> Html.li s))
                ]

        ]
