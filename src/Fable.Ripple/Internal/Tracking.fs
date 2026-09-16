namespace Fable.Ripple.Internal

open Fable.Ripple

/// What is currently being evaluated, and therefore whose reads become
/// dependencies. Owns read tracking, recomputation and edge reconciliation.
module internal Tracking =

    // The node being (re-)evaluated, whose reads become dependencies.
    // `ValueNone` when nothing is tracking.
    let mutable private current: ReactiveNode voption = ValueNone

    // Reads are matched against `current`'s sources in order, so a stable graph
    // reuses its edges instead of rebuilding them.

    // The reads that diverged from the previous sources, collected from the first
    // mismatch on. `ValueNone` while the run still matches - the common case.
    let mutable private currentGets: ResizeArray<ReactiveNode> voption = ValueNone

    // How many leading sources this run has re-read in order; also where the
    // stale tail starts once reconciliation kicks in.
    let mutable private currentGetsIndex = 0

    /// Record that `current` reads `node`. While reads arrive in the same order
    /// as the previous run, just advance the index (no edge mutation); on the
    /// first mismatch, collect the new tail into `currentGets`.
    let track (node: ReactiveNode) =
        match current with
        | ValueNone -> ()
        | ValueSome cur ->
            match currentGets with
            | ValueNone when
                currentGetsIndex < Graph.sourceCount cur
                && obj.ReferenceEquals(Graph.sourceAt cur currentGetsIndex, node)
                ->
                currentGetsIndex <- currentGetsIndex + 1
            | _ ->
                let gets =
                    match currentGets with
                    | ValueSome g -> g
                    | ValueNone ->
                        let g = ResizeArray<ReactiveNode>()
                        currentGets <- ValueSome g
                        g

                gets.Add node

    /// Bring `node` up to date, recomputing only if a dependency truly changed.
    let rec updateIfNecessary (node: ReactiveNode) =
        if node.State = NodeState.Check then
            // A source that actually changed will flip us to Dirty; stop early then.
            let count = Graph.sourceCount node
            let mutable i = 0

            while node.State = NodeState.Check && i < count do
                updateIfNecessary (Graph.sourceAt node i)
                i <- i + 1

        if node.State = NodeState.Dirty then
            recompute node

        node.State <- NodeState.Clean

    and private recompute (node: ReactiveNode) =
        let prevCurrent = current
        let prevGets = currentGets
        let prevIndex = currentGetsIndex
        current <- ValueSome node
        currentGets <- ValueNone
        currentGetsIndex <- 0
        let mutable changed = false

        try
            changed <-
                match node.EffectFn with
                | ValueSome fn ->
                    fn () // effect: run its body; it has no observers to propagate to
                    false
                | ValueNone -> node.Recompute()

            // Reconcile edges only where the dependency set diverged, and only on
            // success - a partial `currentGets` from a throwing evaluation must
            // not touch the graph. A stable graph mutates nothing here.
            match currentGets with
            | ValueSome gets ->
                // Reads diverged at `currentGetsIndex`: drop the old tail, append new.
                Graph.unlinkSourcesTail node currentGetsIndex
                Graph.truncateSources node currentGetsIndex

                for i in 0 .. gets.Count - 1 do
                    let s = gets.[i]
                    Graph.addSource node s
                    Graph.addObserver s node
            | ValueNone ->
                // Read fewer sources than before: drop the stale tail.
                if Graph.sourceCount node > currentGetsIndex then
                    Graph.unlinkSourcesTail node currentGetsIndex
                    Graph.truncateSources node currentGetsIndex
        finally
            // Always restore the tracking globals, even if evaluation threw, so a
            // user exception cannot leave the engine wedged mid-computation. The
            // node stays dirty (its `State <- clean` is skipped) and retries later.
            current <- prevCurrent
            currentGets <- prevGets
            currentGetsIndex <- prevIndex

        if changed then
            Graph.iterObservers
                node
                (fun o ->
                    if int o.State < int NodeState.Dirty then
                        o.State <- NodeState.Dirty
                )

    let untracked (fn: unit -> 'a) =
        let prev = current
        current <- ValueNone

        try
            fn ()
        finally
            current <- prev
