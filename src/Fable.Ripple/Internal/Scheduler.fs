namespace Fable.Ripple.Internal

open Fable.Ripple

/// Staleness propagation and the effect queue: who needs re-running after a
/// write, and when that queue is drained.
module internal Scheduler =

    // Effects marked stale since the last flush, to be re-run on the next flush.
    let private pending = ResizeArray<ReactiveNode>()
    // Nesting level of `batch`; while > 0 writes mark but defer the flush.
    let mutable private batchDepth = 0
    // Guard so a write from inside a running flush joins it instead of nesting.
    let mutable private flushing = false

    /// Propagate staleness. Direct observers of a changed source get `dirty`;
    /// everything downstream of a freshly-stale node gets `check`.
    let rec private stale (node: ReactiveNode) (target: NodeState) =
        if int node.State < int target then
            let wasClean = node.State = NodeState.Clean
            node.State <- target

            if node.IsEffect && not node.Queued then
                node.Queued <- true
                pending.Add node

            if wasClean then
                Graph.iterObservers node (fun o -> stale o NodeState.Check)

    let flush () =
        if not flushing then
            flushing <- true

            try
                let mutable i = 0

                // `pending` may grow if an effect writes during the flush.
                while i < pending.Count do
                    let e = pending.[i]
                    i <- i + 1
                    e.Queued <- false

                    if e.State <> NodeState.Clean then
                        Tracking.updateIfNecessary e
            finally
                // A throwing effect must not leave the flush wedged. Clear the
                // queued flag on anything not yet reached so it can re-queue on a
                // later change, then reset the queue and guard. (The remaining
                // effects of this flush are dropped for this cycle.)
                for j in 0 .. pending.Count - 1 do
                    pending.[j].Queued <- false

                pending.Clear()
                flushing <- false

    /// A source's value changed: mark observers and flush unless batching.
    let notifyChange (source: ReactiveNode) =
        Graph.iterObservers source (fun o -> stale o NodeState.Dirty)

        if batchDepth = 0 then
            flush ()

    let batch (fn: unit -> unit) =
        batchDepth <- batchDepth + 1

        try
            fn ()
        finally
            batchDepth <- batchDepth - 1

            if batchDepth = 0 then
                flush ()
