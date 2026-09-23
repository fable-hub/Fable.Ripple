var e=`module Demo.Examples.Widgets

open Browser
open Fable.Ripple
open Fable.Ripple.Dom

// Shared instrumentation for the examples. Most of what makes this library
// interesting - work that does NOT happen - is invisible, so these widgets put
// a number on the page next to every claim.

/// A hit counter, called from inside a computation to count how often it ran.
///
/// The count is a plain mutable rather than a \`Var\`, and that is not laziness:
/// incrementing a signal means reading it first, and a read inside a compute
/// function registers a dependency - so the computation would invalidate itself
/// and re-run forever. Counts are therefore mutated freely and the display is
/// refreshed explicitly, with \`Repaint\` below.
type Probe(label: string) =
    let mutable hits = 0

    member _.Label = label
    member _.Hits = hits
    member _.Hit() = hits <- hits + 1
    member _.Reset() = hits <- 0

/// A token the chips depend on. Bump it from an event handler *after* mutating:
/// writes flush synchronously, so by then every computation has already run and
/// the counts are final.
///
/// \`Now\` increments through \`Peek\`, and that is load-bearing. An example that
/// calls it while its own view is being built - \`Html.dynamic\` is tracking at
/// that moment - would otherwise register the read, making the whole component
/// depend on its repaint token and rebuild itself on the next bump.
type Repaint() =
    let tick = Var.create 0

    member _.Now() = tick.Value <- tick.Peek() + 1
    /// Read inside a binding to make it re-run when \`Now\` is called.
    member _.Track() = tick.Value |> ignore

/// Bump \`repaint\` whenever any signal \`outputs\` reads changes.
///
/// For an example driven by buttons, the handler bumps it and that is that. For
/// one driven by a text box there is no handler to hang it on, so this watches
/// instead. Read the OUTPUTS, not the inputs: the effect is then downstream of
/// the computations being measured and therefore runs after them, so the counts
/// it publishes are the final ones rather than the ones from a moment ago.
let repaintAfter (repaint: Repaint) (outputs: unit -> unit) =
    Signal.effect (fun () ->
        outputs ()
        repaint.Now()
    )
    |> ignore

let private chip (repaint: Repaint) (p: Probe) : DomItem =
    Html.div
        [
            attr.className "probe"

            Html.span
                [
                    attr.className "probe-label"
                    Html.text p.Label
                ]

            Html.span
                [
                    attr.className "probe-count"
                    Html.text (fun () ->
                        repaint.Track()
                        string p.Hits
                    )
                ]
        ]

/// A row of run counters.
let probes (repaint: Repaint) (ps: Probe list) : DomItem =
    Html.div
        [
            attr.className "probes"
            Html.fragment (ps |> List.map (chip repaint))
        ]

/// A binding that reads a source and renders nothing.
///
/// For the examples whose claim is "only the binding that reads it ran", but
/// where the schematic already shows the value - so a \`readout\` beside it would
/// only repeat the picture.
///
/// There has to be a real binding somewhere: counting from an event handler
/// instead would count CLICKS, and the number would no longer be evidence -
/// press-driven counters read the same whether the engine re-ran one binding,
/// both, or neither. This is the smallest thing that is genuinely a binding.
///
/// It reads only the source, deliberately. A repaint token in here would give
/// it a second reason to re-run and the count would stop meaning "this changed".
let readerOf (probe: Probe) (read: unit -> unit) : DomItem =
    Html.text (fun () ->
        probe.Hit()
        read ()
        ""
    )

/// The common case: hand over the \`Var\` or \`Signal\` itself.
let reader (probe: Probe) (s: Signal<'a>) : DomItem =
    readerOf probe (fun () -> s.Value |> ignore)

/// A single labelled number, for readouts that are not run counts.
let readout (label: string) (value: unit -> string) : DomItem =
    Html.div
        [
            attr.className "probe"

            Html.span
                [
                    attr.className "probe-label"
                    Html.text label
                ]

            Html.span
                [
                    attr.className "probe-count"
                    Html.text value
                ]
        ]

/// A readout that does NOT subscribe to what it shows: it refreshes off the
/// repaint token instead. Use it wherever the page prints \`Signal.observerCount\`
/// - a plain \`readout\` would itself become one of the observers it is counting.
///
/// This is the general form, for a value that is not simply a source's contents.
let peekedWith (repaint: Repaint) (label: string) (value: unit -> string) : DomItem =
    readout
        label
        (fun () ->
            repaint.Track()
            value ()
        )

/// The common case: show what a source currently holds, without becoming one of
/// its observers. Takes the \`Var\` or \`Signal\` itself - every call site was
/// otherwise writing out the same \`fun () -> string (x.Peek())\`.
let inline peeked (repaint: Repaint) (label: string) (s: Signal<'a>) : DomItem =
    peekedWith repaint label (fun () -> string (s.Peek()))

/// A live observer count, read without becoming one of the observers counted.
let observers (repaint: Repaint) (label: string) (s: Signal<'T>) : DomItem =
    peekedWith repaint label (fun () -> string (Signal.observerCount s))

/// Records, IN ORDER, which computations ran during a single write.
///
/// A run counter answers "how often"; this answers "what happened, and in what
/// sequence" - which is the part that makes a cutoff legible, because a
/// computation that did not run leaves a visible hole rather than a number that
/// merely failed to move.
///
/// Plain mutable state, for the reason \`Probe\` is: a \`Var\` read from inside a
/// computation would make that computation depend on itself.
type Trace() =
    let mutable order: string list = []

    /// Call immediately before the write being traced.
    member _.Begin() = order <- []

    /// Call from inside the computation, as \`Probe.Hit\` is called.
    member _.Note(name: string) = order <- order @ [ name ]

    /// 1-based position in this write's run order, or \`None\` if it never ran.
    member _.PositionOf(name: string) =
        order |> List.tryFindIndex (fun n -> n = name) |> Option.map ((+) 1)

    member _.Length = List.length order

/// The trace as a strip: every computation that COULD have run, in graph order,
/// each showing where it came in the sequence or that it was skipped.
let traceStrip (repaint: Repaint) (trace: Trace) (names: string list) : DomItem =
    Html.div
        [
            attr.className "trace"

            for name in names do
                Html.div
                    [
                        attr.className "trace-step"

                        attr.classList (fun () ->
                            repaint.Track()
                            [ "is-skipped", (trace.PositionOf name).IsNone ]
                        )

                        Html.span
                            [
                                attr.className "trace-order"
                                Html.text (fun () ->
                                    repaint.Track()

                                    match trace.PositionOf name with
                                    | Some position -> string position
                                    | None -> "-"
                                )
                            ]

                        Html.span
                            [
                                attr.className "trace-name"
                                Html.text name
                            ]
                    ]
        ]

/// An append-only log, for showing how many times something fired.
///
/// \`Add\` reads the current lines through \`Peek\`, not \`.Value\`, and that matters:
/// logs are usually appended to from inside a \`Signal.effect\`, and a tracked read
/// there would make the effect depend on its own output - so clearing the log
/// would re-trigger the effect and immediately re-fill it.
type Log() =
    let lines = Var.create ([]: string list)

    member _.Lines = lines
    member _.Add(line: string) = lines.Value <- lines.Peek() @ [ line ]
    member _.Clear() = lines.Value <- []
    member _.Count = List.length (lines.Peek())

let logPane (log: Log) : DomItem =
    Html.div
        [
            attr.className "log"

            // Follow the tail. The effect reads \`Lines\`, so it re-runs on every
            // append - but it has to scroll AFTER the new row exists, and the
            // row is appended by a different binding on the same signal. One
            // macrotask is the cheap way to be second.
            attr.ref (fun el ->
                Signal.effect (fun () ->
                    log.Lines.Value |> ignore

                    window.setTimeout ((fun () -> el.scrollTop <- el.scrollHeight), 0) |> ignore
                )
                |> ignore
            )

            Html.each
                (fun () -> log.Lines.Value |> List.toArray |> Array.mapi (fun i l -> i, l))
                fst
                (fun (_, line) -> Html.div [ Html.text line ])
        ]
`;export{e as default};