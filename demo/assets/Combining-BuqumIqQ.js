var e=`module Demo.Examples.Signals.Combining

open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Components // demo-hide-line
open Demo.Examples.Widgets // demo-hide-line
open Demo.Examples.Schematic // demo-hide-line

let render () =
    // demo-hide
    let repaint = Repaint()
    let hits = Probe("map3 ran")

    // No badge of their own - they exist so the other two nodes join the
    // animation: an edge only draws as carrying a change if an end counts runs.
    let shoutHits = Probe("map ran")
    let initialsHits = Probe("map2 ran")

    // demo-show
    let first = Var.create "Ada"
    let last = Var.create "Lovelace"
    let isExcited = Var.create false

    // Derived from a source, then fed straight back into another combinator.
    let shout =
        last
        |> Signal.map (fun s ->
            shoutHits.Hit() // demo-hide-line
            s.ToUpper()
        )

    // A Var, a Signal and a Var - no conversions anywhere.
    let fullName =
        Signal.map3
            (fun (first: string) (last: string) isExcited ->
                // demo-hide
                hits.Hit()

                // demo-show
                let name = first + " " + last

                if isExcited then
                    name + "!"
                else
                    name
            )
            first
            shout
            isExcited

    let initials =
        Signal.map2
            (fun (f: string) (l: string) ->
                // demo-hide
                initialsHits.Hit()
                // demo-show
                string f.[0] + string l.[0]
            )
            first
            last

    // demo-hide
    let firstNode = Node.source "Var first" first
    let lastNode = Node.source "Var last" last

    let excitedNode = Node.flag "Is excited" isExcited

    let shoutNode =
        Node.derived "shout" shout |> Node.beating (fun () -> shoutHits.Hits)

    let fullNode = Node.derived "fullName" fullName |> Node.counting hits

    let initialsNode =
        Node.derived "initials" initials |> Node.beating (fun () -> initialsHits.Hits)

    // Typing goes through \`attr.bindValue\`, so unlike every button-driven
    // example there is no handler in which to bump the repaint token. Watch the
    // outputs instead - and the outputs specifically, so this runs after they
    // have recomputed rather than racing them.
    repaintAfter
        repaint
        (fun () ->
            fullName.Value |> ignore
            initials.Value |> ignore
        )

    // demo-show
    Html.fragment
        [
            // demo-hide
            Try.observe
                "Type in \`first\`: \`fullName\` runs once per keystroke. Type in \`last\` and follow the lit edges: the change reaches \`fullName\` through \`shout\`, and \`initials\` directly."

            // demo-show
            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "first"
                            Html.input [ attr.bindValue first ]
                        ]

                    Html.label
                        [
                            Html.text "last"
                            Html.input [ attr.bindValue last ]
                        ]

                    Html.label
                        [
                            Html.input
                                [
                                    attr.type' "checkbox"
                                    attr.bindChecked isExcited
                                ]

                            Html.text "excited"
                        ]
                ]

            Html.div
                [
                    attr.role "group"

                    Html.label
                        [
                            Html.text "shout"
                            Html.output shout
                        ]

                    Html.label
                        [
                            Html.text "fullName"
                            Html.output fullName
                        ]

                    Html.label
                        [
                            Html.text "initials"
                            Html.output initials
                        ]
                ]

            // demo-hide
            graph
                repaint
                [
                    lastNode ==> shoutNode
                    firstNode ==> fullNode
                    shoutNode ==> fullNode
                    excitedNode ==> fullNode
                    firstNode ==> initialsNode
                    lastNode ==> initialsNode
                ]

            legend
        // demo-show
        ]
`;export{e as default};