module Demo.Examples.Schematic

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fable.Ripple
open Fable.Ripple.Dom
open Demo.Examples.Widgets

// A dependency graph, drawn.
//
// The counters in `Widgets` prove what ran, but they do not show the SHAPE of
// the thing that ran - and for a reader meeting signals for the first time, the
// shape is the whole idea. So these examples draw the graph: sources on the
// left, derived values in the middle, readers on the right, with an arrow for
// every real dependency.
//
// Two things move when you press a button:
//
//   - a node FLASHES when it recomputes, and
//   - a dot TRAVELS along each edge that carried the change.
//
// Both are driven from the same run counters the chips use, so the animation
// cannot claim anything the numbers do not.
//
// THE DSL. A node is a VALUE, bound with `let`, and an edge is written `a ==> b`
// between two of them:
//
//     let n = Node.source "Var" (fun () -> string count.Value)
//     let m = Node.derived "Signal.map" (fun () -> string doubled.Value)
//             |> Node.counting hits
//     graph repaint [ n ==> m ]
//
// So there are no identifiers to keep in step - `let` bindings are the
// identity, and a typo is a compile error rather than a silently missing arrow.
// Nor are there coordinates: the layout is DERIVED from the edges (column =
// depth from a root, rows centred within each column), which is both less to
// write and impossible to get out of step with the graph it draws.
//
// Built with `Svg.*` and `attr.ref` from the Rendering section - no diagramming
// library is involved.

type NodeKind =
    /// A `Var` - the only writable thing. Drawn square, because it is a value.
    | Source
    /// A `Signal` - a cached node. Drawn rounded.
    | Derived
    /// A DOM binding. Drawn dashed, because it produces no value of its own.
    | Reader
    /// A plain function - drawn with no border, because there is no node there.
    | Ghost

/// A node. Construct one with `source` / `derived` / `binding` / `ghost` and
/// bind it with `let`; the binding is what identifies it from then on.
type Node =
    {
        /// Identity. Generated, never written by hand - two nodes with the same
        /// label are still two nodes.
        Key: int
        Label: string
        Kind: NodeKind
        /// What the node currently holds, shown inside it.
        Value: unit -> string
        /// How many times it has run. Drives the flash and the badge.
        Runs: (unit -> int) option
        /// Whether that count is also drawn. Off for a node that should take
        /// part in the animation but whose number would distract - a page making
        /// its claim about one combinator does not want every neighbour badged
        /// with a figure the prose never mentions.
        Badge: bool
    }

/// The constructors, in a module rather than at top level: `source`, `derived`
/// and `binding` are exactly what an example calls its own locals, and a DSL
/// that shadows the code it documents is a poor DSL.
[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Node =

    let mutable private nextKey = 0

    let private make kind label value =
        nextKey <- nextKey + 1

        {
            Key = nextKey
            Label = label
            Kind = kind
            Value = value
            Runs = None
            Badge = true
        }

    /// A writable source, from a thunk - for a value with its own formatting.
    let sourceWith label value = make Source label value

    /// A writable source: the `Var` or `Signal` itself, not a thunk over it.
    ///
    /// PEEKED, and that is the whole reason this takes the signal. `drawNode`
    /// refreshes a box off the repaint token, so a tracked read here would make
    /// the drawing an observer of the source it draws - and give the box a
    /// second reason to re-run, which is fatal to anything counting those runs.
    /// Handing the signal over means the call site never spells the read, so it
    /// cannot spell it wrong.
    let inline source label (s: Signal<'a>) =
        sourceWith label (fun () -> string (s.Peek()))

    /// A boolean source. `string true` is "True"; a schematic reads better
    /// lowercase, and F# has no way to special-case `bool` inside `source`.
    let flag label (s: Signal<bool>) =
        sourceWith
            label
            (fun () ->
                if s.Peek() then
                    "true"
                else
                    "false"
            )

    /// A cached derived node, from a thunk - for a value with its own formatting.
    ///
    /// Also the public seam `derived` needs: an `inline` member cannot reach the
    /// private `make`, so `derived` forwards through here. Same reason
    /// `sourceWith` exists.
    let derivedWith label value = make Derived label value

    /// A cached derived node - anything the `Signal` module returns.
    ///
    /// Peeked, for the reason `source` is: a tracked read here would make the
    /// drawing an OBSERVER of the value it draws, which is how a picture starts
    /// keeping alive - or inflating - the very thing it exists to explain.
    let inline derived label (s: Signal<'a>) =
        derivedWith label (fun () -> string (s.Peek()))

    /// A DOM binding whose text is not simply a signal's contents - a reader
    /// showing "running" or "disposed" rather than a value.
    let bindingWith label value = make Reader label value

    /// A DOM binding: it consumes a value and produces none.
    let inline binding label (s: Signal<'a>) =
        bindingWith label (fun () -> string (s.Peek()))

    /// A plain function. Drawn borderless, because there is no node behind it -
    /// so there is no signal to hand over, and this keeps its thunk.
    let ghost label value = make Ghost label value

    /// Give a node a run counter, which draws its badge and drives its flash.
    let counting (probe: Probe) (n: Node) =
        { n with
            Runs = Some(fun () -> probe.Hits)
            Badge = true
        }

    /// Take part in the animation, but draw no number.
    let beating (runs: unit -> int) (n: Node) =
        { n with
            Runs = Some runs
            Badge = false
        }

/// An edge: `a ==> b` means "b depends on a".
let (==>) (a: Node) (b: Node) = a, b

// Geometry. One place, so the arrows cannot drift away from the boxes.
let private colW = 244.0
let private rowH = 82.0
let private boxW = 184.0
let private boxH = 54.0
let private padX = 12.0
let private padY = 14.0

/// Values are drawn in IBM Plex Mono at 15px, whose advance is 0.6em, so what
/// fits a fixed-width box is a character count rather than a guess. Growing the
/// box instead would relayout the whole graph on every keystroke.
let private valueChars = int ((boxW - 2.0 * 10.0) / 9.0)

let private fit (s: string) =
    if s.Length > valueChars then
        s.Substring(0, valueChars - 1) + "\u2026"
    else
        s

/// A node with its computed place on the grid.
type private Placed =
    {
        Node: Node
        Col: int
        Row: float
        /// No value to show. Such a node is drawn narrow with its label centred,
        /// so it sits on the arrows' line instead of floating above an empty
        /// box - decided by what it HOLDS rather than by its kind, because a
        /// ghost with a value should read like any other node.
        Bare: bool
    }

/// A valueless node gets a box just wide enough for its label, centred in the
/// column, so the arrows meet the text rather than the empty space around it.
let private widthOf (p: Placed) =
    if p.Bare then
        84.0
    else
        boxW

/// Left edge, with the narrower boxes centred in their column slot.
let private x (p: Placed) =
    padX + float p.Col * colW + (boxW - widthOf p) / 2.0

let private y (p: Placed) = padY + p.Row * rowH
let private midY (p: Placed) = y p + boxH / 2.0
let private rightX (p: Placed) = x p + widthOf p
let private leftX (p: Placed) = x p

/// Arrows stop this far short of a box at each end, so they read as pointing AT
/// something rather than being glued to it.
let private gap = 7.0

(*
    Layout.

    Column is the longest path to a node from any root, so a value always sits
    to the right of everything it depends on. Row is its position among the
    nodes sharing that column, then CENTRED against the tallest column - which
    is what puts a single source opposite the middle of the three readers it
    feeds, with no one having to say so.
*)

let private place (nodes: Node list) (edges: (Node * Node) list) : Placed list =
    let cols = System.Collections.Generic.Dictionary<int, int>()

    for n in nodes do
        cols.[n.Key] <- 0

    // The graphs here are a handful of nodes, so a fixpoint over the edge list
    // is both fast enough and obviously correct.
    let mutable changed = true
    let mutable guard = 0

    while changed && guard < nodes.Length + 2 do
        changed <- false
        guard <- guard + 1

        for a, b in edges do
            if cols.ContainsKey a.Key && cols.ContainsKey b.Key then
                let candidate = cols.[a.Key] + 1

                if candidate > cols.[b.Key] then
                    cols.[b.Key] <- candidate
                    changed <- true

    // Then pull every node as far RIGHT as its earliest consumer allows.
    //
    // Depth alone puts a node in the first column it could occupy, which leaves
    // an input used only by something far downstream sitting way out on the
    // left - and its edge then has to cross the columns in between, passing
    // behind whatever boxes are there. Nodes with no consumers stay where depth
    // put them, which is what keeps a row of readers aligned.
    changed <- true
    guard <- 0

    while changed && guard < nodes.Length + 2 do
        changed <- false
        guard <- guard + 1

        for n in nodes do
            let consumers =
                edges
                |> List.choose (fun (a, b) ->
                    if a.Key = n.Key && cols.ContainsKey b.Key then
                        Some cols.[b.Key]
                    else
                        None
                )

            match consumers with
            | [] -> ()
            | _ ->
                let latest = List.min consumers - 1

                if latest > cols.[n.Key] then
                    cols.[n.Key] <- latest
                    changed <- true

    let byCol = nodes |> List.groupBy (fun n -> cols.[n.Key])
    let tallest = byCol |> List.map (fun (_, ns) -> List.length ns) |> List.max

    [
        for col, ns in byCol do
            let offset = float (tallest - List.length ns) / 2.0

            for i, n in List.indexed ns do
                {
                    Node = n
                    Col = col
                    Row = offset + float i
                    // Untracked: this runs while the page is being built, and a
                    // tracked read here would subscribe the whole example.
                    Bare = Signal.untracked n.Value = ""
                }
    ]

/// Re-runs `onFire` whenever `count` grows - but not on the first run, so
/// nothing flashes just because the page loaded.
let private onGrow (repaint: Repaint) (count: unit -> int) (onFire: unit -> unit) =
    let mutable previous = -1

    Signal.effect (fun () ->
        repaint.Track()
        let now = count ()

        if previous >= 0 && now > previous then
            onFire ()

        previous <- now
    )
    |> ignore

/// Milliseconds of delay per column of depth.
///
/// The propagation itself is INSTANT: by the time the write returns, every
/// computation has run and every binding has been updated - that is the point
/// Under the Hood / When a computed runs makes. Playing the whole graph in one
/// frame is therefore truthful and useless, because a wave that arrives
/// everywhere at once shows no direction.
///
/// So the animation is deliberately spread out by graph depth, and only the
/// animation: the numbers beside it were final before the first frame was
/// painted. Think of it as a slow-motion replay rather than a live feed.
let private stagger = 600

/// Shorter than a step, so a dot arrives fractionally before its target flashes
/// rather than racing it.
let private dotDuration = 500

let private flash (delay: int) (el: HTMLElement) =
    el?animate (
        [|
            {|
                opacity = 1.0
                transform = "scale(1)"
            |}
            {|
                opacity = 1.0
                transform = "scale(1.06)"
            |}
            {|
                opacity = 1.0
                transform = "scale(1)"
            |}
        |],
        {|
            duration = 420
            delay = delay
            easing = "ease-out"
            fill = "backwards"
        |}
    )
    |> ignore

let private travel (delay: int) (el: HTMLElement) =
    el?animate (
        [|
            {|
                offset = 0.0
                offsetDistance = "0%"
                opacity = 0.0
            |}
            {|
                offset = 0.18
                offsetDistance = "18%"
                opacity = 1.0
            |}
            {|
                offset = 0.82
                offsetDistance = "82%"
                opacity = 1.0
            |}
            {|
                offset = 1.0
                offsetDistance = "100%"
                opacity = 0.0
            |}
        |],
        {|
            duration = dotDuration
            delay = delay
            easing = "linear"
            fill = "backwards"
        |}
    )
    |> ignore

let private nodeClass =
    function
    | Source -> "sch-node sch-source"
    | Derived -> "sch-node sch-derived"
    | Reader -> "sch-node sch-reader"
    | Ghost -> "sch-node sch-ghost"

/// `flashOn` is the counter that makes this node light up. Normally its own -
/// but a node with no counter of its own (a plain DOM binding) borrows its
/// parent's, so the wave does not stop halfway across the picture.
let private drawNode (repaint: Repaint) (flashOn: (unit -> int) option) (p: Placed) : DomItem =
    let n = p.Node

    Svg.g
        [
            attr.className (nodeClass n.Kind)

            attr.ref (fun el ->
                // The flash is an effect on the run counter, so it fires exactly
                // when the node really recomputed - the delay only decides when
                // it is DRAWN, never whether it is.
                match flashOn with
                | Some runs -> onGrow repaint runs (fun () -> flash (p.Col * stagger) el)
                | None -> ()
            )

            Svg.rect
                [
                    svgAttr.custom ("x", string (x p))
                    svgAttr.custom ("y", string (y p))
                    svgAttr.custom ("width", string (widthOf p))
                    svgAttr.custom ("height", string boxH)
                    svgAttr.custom (
                        "rx",
                        (if n.Kind = Derived then
                             "10"
                         else
                             "0")
                    )
                ]

            // A node with a value stacks label over value; one without - a ghost
            // - centres its label instead, so it sits on the arrows' line rather
            // than floating above it.
            if p.Bare then
                Svg.text
                    [
                        attr.className "sch-label"
                        svgAttr.custom ("x", string (x p + widthOf p / 2.0))
                        svgAttr.custom ("y", string (y p + boxH / 2.0 + 4.0))
                        svgAttr.custom ("text-anchor", "middle")
                        Html.text n.Label
                    ]
            else
                Html.fragment
                    [
                        Svg.text
                            [
                                attr.className "sch-label"
                                svgAttr.custom ("x", string (x p + 10.0))
                                svgAttr.custom ("y", string (y p + 21.0))
                                Html.text n.Label
                            ]

                        Svg.text
                            [
                                attr.className "sch-value"
                                svgAttr.custom ("x", string (x p + 10.0))
                                svgAttr.custom ("y", string (y p + 42.0))

                                // Through the repaint token, not straight off
                                // `n.Value`. A source node is usually read with
                                // `Peek` so that drawing the graph does not
                                // become one of the observers it is counting -
                                // and an untracked read cannot refresh itself,
                                // so the box would sit there showing the value
                                // the page started with. Tracked readers are
                                // unaffected: they simply have two reasons to
                                // re-run instead of one.
                                // The whole value on hover, since the box may
                                // have had to cut it.
                                Svg.elem
                                    "title"
                                    [
                                        Html.text (fun () ->
                                            repaint.Track()
                                            n.Value()
                                        )
                                    ]

                                Html.text (fun () ->
                                    repaint.Track()
                                    fit (n.Value())
                                )
                            ]
                    ]

            match
                (if n.Badge then
                     n.Runs
                 else
                     None)
            with
            | Some runs ->
                Svg.g
                    [
                        attr.className "sch-badge"

                        // The native SVG tooltip, so the badge answers for
                        // itself on hover as well as in the legend.
                        Svg.elem "title" [ Html.text "Times this has run" ]

                        Svg.rect
                            [
                                svgAttr.custom ("x", string (rightX p - 31.0))
                                svgAttr.custom ("y", string (y p + 8.0))
                                svgAttr.custom ("width", "24")
                                svgAttr.custom ("height", "15")
                            ]

                        Svg.text
                            [
                                svgAttr.custom ("x", string (rightX p - 19.0))
                                svgAttr.custom ("y", string (y p + 19.5))
                                svgAttr.custom ("text-anchor", "middle")

                                // Run counts are plain mutables (a `Var` read
                                // inside a computation would make it depend on
                                // itself), so the badge refreshes off the
                                // repaint token exactly as the chips do.
                                Html.text (fun () ->
                                    repaint.Track()
                                    string (runs ())
                                )
                            ]
                    ]
            | None -> Html.none
        ]

/// One arrow, plus the dot that runs along it when the target recomputes.
///
/// Always returns a `<g>`: this is rendered through `Html.each`, which needs a
/// real element per key.
let private drawEdge
    (repaint: Repaint)
    (placed: Map<int, Placed>)
    (laneOf: int -> int -> float)
    (fromKey: int, toKey: int)
    : DomItem
    =
    match Map.tryFind fromKey placed, Map.tryFind toKey placed with
    | Some a, Some b ->
        let x1 = rightX a + gap
        let y1 = midY a
        let x2 = leftX b - gap
        let y2 = midY b

        // Orthogonal routing with rounded corners, rather than an S-curve: this
        // is a schematic, and a schematic reads as right angles. It also makes a
        // fan-out legible - the three arrows out of one node share a horizontal
        // run and then branch, instead of three swooping curves crossing the
        // same space.
        let path =
            if abs (y2 - y1) < 0.5 then
                sprintf "M %f %f H %f" x1 y1 x2
            else
                // Where the vertical run sits, as a fraction of the gap between
                // the columns rather than a nudge off its centre - which spreads
                // the verticals across the space actually available instead of
                // crowding them all into the middle of it.
                //
                // From the lane assignment, so arrows that converge on one node
                // share a vertical and arrows that fan out of one node share a
                // stem. Keying this on the source's ROW instead - which it did
                // for a while - bends every arrival at a different x, and a
                // fan-in of three then reaches its target at three depths.
                // The fraction applies to the gap immediately BEFORE the
                // target, not to the whole run. An edge that skips a column -
                // `first` straight to `map3`, past `excited` - would otherwise
                // put its vertical inside that middle column, behind whatever
                // box happens to sit there.
                let gapStart = padX + float (b.Col - 1) * colW + boxW
                let gapEnd = padX + float b.Col * colW

                let elbow =
                    gapStart + (gapEnd - gapStart) * laneOf fromKey toKey
                    |> max (min x1 x2)
                    |> min (max x1 x2)

                let down = y2 > y1

                let step =
                    if down then
                        1.0
                    else
                        -1.0

                // Only part of each horizontal leg may be spent on the curve,
                // so a straight run always survives at both ends - one for the
                // arrowhead to sit on, one for the arrow to leave its box along.
                let radius =
                    List.min
                        [
                            12.0
                            abs (y2 - y1) / 2.0
                            abs (elbow - x1) * 0.55
                            abs (x2 - elbow) * 0.55
                        ]

                sprintf
                    "M %f %f H %f Q %f %f %f %f V %f Q %f %f %f %f H %f"
                    x1
                    y1
                    (elbow - radius)
                    elbow
                    y1
                    elbow
                    (y1 + step * radius)
                    (y2 - step * radius)
                    elbow
                    y2
                    (elbow + radius)
                    y2
                    x2

        Svg.g
            [
                attr.className "sch-edge"

                Svg.path
                    [
                        svgAttr.custom ("d", path)
                        svgAttr.custom ("marker-end", "url(#sch-arrow)")
                    ]

                // The travelling dot rides the same path via `offset-path`, so
                // it cannot drift away from the line it is meant to be on.
                Svg.circle
                    [
                        attr.className "sch-pulse"
                        svgAttr.custom ("r", "4")
                        attr.style (sprintf "offset-path: path('%s')" path)

                        attr.ref (fun el ->
                            // A dot means "THIS edge carried the change", which
                            // needs both halves to be true:
                            //
                            //   the target ran   - so a cutoff leaves the arrow
                            //                      dark, and
                            //   the source moved - so bumping one input does not
                            //                      light up every other arrow
                            //                      into the same node.
                            //
                            // Without the second test, `map2` recomputing lights
                            // both its inputs however many of them actually
                            // changed, which claims a flow that never happened.
                            match b.Node.Runs, a.Node.Runs with
                            | Some runs, _
                            | None, Some runs ->
                                let mutable previousRuns = -1
                                let mutable valueAtLastRun = ""

                                Signal.effect (fun () ->
                                    repaint.Track()

                                    // Read before the counter is: a lazy target
                                    // has not recomputed until something reads
                                    // it, and an unread counter is a stale one.
                                    Signal.untracked b.Node.Value |> ignore

                                    let now = runs ()

                                    // Untracked: the source's value is evidence,
                                    // not a trigger - this effect fires off the
                                    // repaint token and nothing else.
                                    let current = Signal.untracked a.Node.Value

                                    // Latched at the target's run, not at every
                                    // observation: the comparison is against the
                                    // source as it stood when the target last
                                    // ran.
                                    if previousRuns < 0 then
                                        valueAtLastRun <- current
                                    elif now > previousRuns then
                                        if current <> valueAtLastRun then
                                            travel (a.Col * stagger + 30) el

                                        valueAtLastRun <- current

                                    previousRuns <- now
                                )
                                |> ignore
                            | None, None -> ()
                        )
                    ]
            ]
    | _ -> Svg.g []

/// Draw a graph whose SHAPE can change.
///
/// `nodes` lists everything that should be drawn - including a node with no
/// edges at the moment, which is exactly the case `Signal.bind` produces - and
/// `edges` is re-read whenever the signals it touches change. Arrows go through
/// `Html.each` keyed by their endpoints, so a change really adds and removes
/// arrows rather than redrawing the picture.
let schematic (repaint: Repaint) (nodes: Node list) (edges: unit -> (Node * Node) list) : DomItem =
    // `untracked` is load-bearing. This runs while `Html.dynamic` is building
    // the page, so a tracked read here would subscribe the WHOLE example to
    // whatever the edge list depends on - and a graph whose shape follows a
    // signal would then rebuild the page it lives on every time it changed.
    // The Control Flow / Html.switch example shows the same trap.
    let initialEdges = Signal.untracked edges

    let placed = place nodes initialEdges
    let byKey = placed |> List.map (fun p -> p.Node.Key, p) |> Map.ofList

    // Which counter lights each node up: its own, or its nearest counted parent.
    let flashSource (p: Placed) =
        match p.Node.Runs with
        | Some runs -> Some runs
        | None ->
            initialEdges
            |> List.tryPick (fun (a, b) ->
                if b.Key = p.Node.Key then
                    a.Runs
                else
                    None
            )

    (*
        Where each arrow puts its vertical run.

        Keying it on the source alone makes a fan-OUT tidy - every arrow out of
        one node branches from the same point - but a fan-IN then arrives at
        three different depths and looks lopsided, which is what you notice
        first. Keying it on the target fixes the fan-in and breaks the fan-out.

        So: edges are GROUPED by whichever end they share, and a fan-IN wins the
        tie. An arrow arriving at a node with several sources is grouped by its
        target; an arrow leaving a node with several consumers is grouped by its
        source; anything else by its target.

        The fan-in wins because that is the one you notice. Three arrows landing
        at three different depths reads as a mistake; three leaving from slightly
        different points reads as three arrows.
    *)
    let outDegree = initialEdges |> List.countBy (fun (a, _) -> a.Key) |> Map.ofList
    let inDegree = initialEdges |> List.countBy (fun (_, b) -> b.Key) |> Map.ofList

    let groupOf (fromKey: int) (toKey: int) =
        let fansIn = Map.tryFind toKey inDegree |> Option.exists (fun n -> n > 1)
        let fansOut = Map.tryFind fromKey outDegree |> Option.exists (fun n -> n > 1)

        if fansIn then
            1, toKey
        elif fansOut then
            0, fromKey
        else
            1, toKey

    let rowOf key =
        Map.tryFind key byKey |> Option.map (fun p -> p.Row) |> Option.defaultValue 0.0

    let colOf key =
        Map.tryFind key byKey |> Option.map (fun p -> p.Col) |> Option.defaultValue 0

    // Lanes are allocated PER GAP. Two groups landing in the same column gap
    // cover the same rows, so giving them both the fan-in position draws one on
    // top of the other - which is what `signal { }` against `map2` did: two
    // sources, two consumers, and a single bracket where there should be two.
    //
    // Within a gap the groups are ordered fan-outs first, then by source row, so
    // a group that branches sits nearer its source and one that converges sits
    // nearer its target.
    let lanesByGap =
        initialEdges
        |> List.map (fun (a, b) -> colOf b.Key, groupOf a.Key b.Key)
        |> List.distinct
        |> List.groupBy fst
        |> List.map (fun (gap, gs) ->
            let ordered =
                gs
                |> List.map snd
                |> List.sortBy (fun g ->
                    let rows =
                        initialEdges
                        |> List.filter (fun (a, b) -> groupOf a.Key b.Key = g)
                        |> List.map (fun (a, _) -> rowOf a.Key)

                    fst g, List.average rows
                )

            gap, ordered
        )
        |> Map.ofList

    // The usable span of a gap: far enough from the source for the arrow to
    // leave its box, far enough from the target for the corner and the 9px
    // arrowhead. A lone group sits by whichever end its kind belongs to.
    let laneNear, laneFar = 0.30, 0.62

    let laneOf (fromKey: int) (toKey: int) =
        let group = groupOf fromKey toKey
        let peers = Map.tryFind (colOf toKey) lanesByGap |> Option.defaultValue [ group ]

        match List.tryFindIndex (fun g -> g = group) peers with
        | Some i when peers.Length > 1 ->
            laneNear + (laneFar - laneNear) * float i / float (peers.Length - 1)
        | _ ->
            if fst group = 0 then
                laneNear + 0.04
            else
                0.58

    let width =
        padX * 2.0
        + (placed |> List.map (fun p -> float p.Col) |> List.max) * colW
        + boxW

    let height =
        padY * 2.0 + (placed |> List.map (fun p -> p.Row) |> List.max) * rowH + boxH

    Html.figure
        [
            attr.className "sch-figure"

            // The frame is a wrapper, not the `svg` itself: an `svg` is sized by
            // its own `width`, so it cannot stretch to the panel without scaling
            // the drawing up with it. The div stretches; the drawing keeps its
            // natural size.
            Html.div
                [
                    attr.className "sch-frame"

                    Svg.svg
                        [
                            attr.className "schematic"
                            svgAttr.viewBox (sprintf "0 0 %f %f" width height)
                            svgAttr.custom ("width", string width)

                            Svg.defs
                                [
                                    Svg.elem
                                        "marker"
                                        [
                                            svgAttr.custom ("id", "sch-arrow")
                                            svgAttr.custom ("viewBox", "0 0 10 10")
                                            // refX at the tip, so the arrowhead POINT lands on
                                            // the path's end rather than overshooting it by
                                            // however wide the marker happens to be.
                                            svgAttr.custom ("refX", "10")
                                            svgAttr.custom ("refY", "5")
                                            svgAttr.custom ("markerUnits", "userSpaceOnUse")
                                            svgAttr.custom ("markerWidth", "9")
                                            svgAttr.custom ("markerHeight", "9")
                                            svgAttr.custom ("orient", "auto-start-reverse")
                                            Svg.path
                                                [ svgAttr.custom ("d", "M 0 0 L 10 5 L 0 10 z") ]
                                        ]
                                ]

                            // Edges first, so the boxes sit on top of the arrowheads.
                            Svg.g
                                [
                                    Html.each
                                        (fun () ->
                                            edges ()
                                            |> List.map (fun (a, b) -> a.Key, b.Key)
                                            |> List.toArray
                                        )
                                        (fun (a, b) -> string a + "->" + string b)
                                        (drawEdge repaint byKey laneOf)
                                ]

                            Html.fragment (
                                placed |> List.map (fun p -> drawNode repaint (flashSource p) p)
                            )
                        ]
                ]
        ]

/// Draw a graph whose shape never changes - the usual case.
///
/// The nodes are the ones the edges mention, in the order they first appear, so
/// this is the whole call:
///
///     graph repaint [ src ==> mapped; mapped ==> readerA; mapped ==> readerB ]
let graph (repaint: Repaint) (edges: (Node * Node) list) : DomItem =
    let seen = System.Collections.Generic.HashSet<int>()

    let nodes =
        [
            for a, b in edges do
                for n in
                    [
                        a
                        b
                    ] do
                    if seen.Add n.Key then
                        n
        ]

    schematic repaint nodes (fun () -> edges)

/// A legend, so the shapes are readable without reading this file.
let legend: DomItem =
    Html.div
        [
            attr.className "sch-legend"

            for cls, label in
                [
                    "sch-source", "Var - a source"
                    "sch-derived", "Signal - a cached node"
                    "sch-reader", "a DOM binding"
                    "sch-ghost", "a plain function - no node"
                ] do
                Html.span
                    [
                        attr.className "sch-legend-item"
                        Html.span [ attr.className ("sch-swatch " + cls) ]
                        Html.text label
                    ]

            // The badge is a number, not a shape, so it cannot join the loop
            // above - and nothing else on the drawing says what the black box in
            // a node's corner counts.
            Html.span
                [
                    attr.className "sch-legend-item"

                    Html.span
                        [
                            attr.className "sch-swatch sch-count"
                            Html.text "0"
                        ]

                    Html.text "times it has run"
                ]
        ]
