namespace Fable.Ripple.Internal

open Fable.Ripple

/// Dependency edges with an inline-first-edge layout. A node keeps its first
/// source/observer in a scalar field and allocates the `Rest*` overflow
/// `ResizeArray` only on a second edge (holding logical indices 1..). These
/// helpers are the ONLY place that knows the layout - every other module goes
/// through them, so the "no `Rest*` without a `First*`" invariant lives here.
module internal Graph =

    let sourceCount (n: ReactiveNode) : int =
        match n.FirstSource with
        | ValueNone -> 0
        | ValueSome _ ->
            match n.RestSources with
            | ValueSome a -> 1 + a.Count
            | ValueNone -> 1

    /// Logical source `i` (caller guarantees `0 <= i < sourceCount n`).
    let sourceAt (n: ReactiveNode) (i: int) : ReactiveNode =
        if i = 0 then
            n.FirstSource.Value
        else
            n.RestSources.Value.[i - 1]

    let private ensureRestSources (n: ReactiveNode) =
        match n.RestSources with
        | ValueSome a -> a
        | ValueNone ->
            let a = ResizeArray<ReactiveNode>()
            n.RestSources <- ValueSome a
            a

    let addSource (n: ReactiveNode) (s: ReactiveNode) =
        match n.FirstSource with
        | ValueNone -> n.FirstSource <- ValueSome s
        | ValueSome _ -> (ensureRestSources n).Add s

    /// Keep the first `len` logical sources, drop the rest (keeps the overflow
    /// array allocated but empty, so a stable regrow reuses it).
    let truncateSources (n: ReactiveNode) (len: int) =
        if len <= 0 then
            n.FirstSource <- ValueNone
            n.RestSources |> ValueOption.iter (fun a -> a.Clear())
        else
            n.RestSources
            |> ValueOption.iter (fun a ->
                while a.Count > len - 1 do
                    a.RemoveAt(a.Count - 1)
            )

    /// Apply `action` to each logical source at index `>= fromIdx`.
    let inline iterSourcesFrom
        (n: ReactiveNode)
        (fromIdx: int)
        ([<InlineIfLambda>] action: ReactiveNode -> unit)
        =
        if fromIdx <= 0 then
            n.FirstSource |> ValueOption.iter action

        n.RestSources
        |> ValueOption.iter (fun a ->
            let start =
                if fromIdx <= 1 then
                    0
                else
                    fromIdx - 1

            for i in start .. a.Count - 1 do
                action a.[i]
        )

    /// Length of the observer list, including disposed entries not yet swept.
    let private observerSlots (n: ReactiveNode) : int =
        match n.FirstObserver with
        | ValueNone -> 0
        | ValueSome _ ->
            match n.RestObservers with
            | ValueSome a -> 1 + a.Count
            | ValueNone -> 1

    /// Live observers of `n` (disposed entries awaiting a sweep are not counted).
    let observerCount (n: ReactiveNode) : int = observerSlots n - n.DeadObservers

    let private ensureRestObservers (n: ReactiveNode) =
        match n.RestObservers with
        | ValueSome a -> a
        | ValueNone ->
            let a = ResizeArray<ReactiveNode>()
            n.RestObservers <- ValueSome a
            a

    let addObserver (n: ReactiveNode) (o: ReactiveNode) =
        match n.FirstObserver with
        | ValueNone -> n.FirstObserver <- ValueSome o
        | ValueSome _ -> (ensureRestObservers n).Add o

    /// Apply `action` to each live observer. A disposed observer can sit in the
    /// list until the next sweep (see `sweepDeadObservers`); it is skipped, so
    /// propagation never marks or re-queues a torn-down node.
    let inline iterObservers (n: ReactiveNode) ([<InlineIfLambda>] action: ReactiveNode -> unit) =
        n.FirstObserver
        |> ValueOption.iter (fun o ->
            if not o.Disposed then
                action o
        )

        n.RestObservers
        |> ValueOption.iter (fun a ->
            for i in 0 .. a.Count - 1 do
                let o = a.[i]

                if not o.Disposed then
                    action o
        )

    /// Swap-remove `o` from `n`'s observers (order is irrelevant).
    let removeObserver (n: ReactiveNode) (o: ReactiveNode) =
        match n.FirstObserver with
        | ValueSome f when obj.ReferenceEquals(f, o) ->
            match n.RestObservers with
            | ValueSome a when a.Count > 0 ->
                n.FirstObserver <- ValueSome a.[a.Count - 1]
                a.RemoveAt(a.Count - 1)
            | _ -> n.FirstObserver <- ValueNone
        | ValueSome _ ->
            n.RestObservers
            |> ValueOption.iter (fun a ->
                let mutable i = 0
                let mutable go = true

                while go && i < a.Count do
                    if obj.ReferenceEquals(a.[i], o) then
                        a.[i] <- a.[a.Count - 1]
                        a.RemoveAt(a.Count - 1)
                        go <- false
                    else
                        i <- i + 1
            )
        | ValueNone -> ()

    /// Keep only the observers of `n` satisfying `keep`, compacting in place with
    /// no allocation: the first kept edge fills `FirstObserver` (promoting one out
    /// of the overflow if needed), the rest are compacted into `RestObservers` by a
    /// write cursor. `w <= readIdx` always, so writes never clobber unread slots.
    let compactObservers (n: ReactiveNode) (keep: ReactiveNode -> bool) =
        match n.FirstObserver with
        | ValueNone -> ()
        | ValueSome f0 ->
            let mutable newFirst =
                if keep f0 then
                    ValueSome f0
                else
                    ValueNone

            n.RestObservers
            |> ValueOption.iter (fun rest ->
                let mutable w = 0

                for readIdx in 0 .. rest.Count - 1 do
                    let o = rest.[readIdx]

                    if keep o then
                        match newFirst with
                        | ValueNone -> newFirst <- ValueSome o
                        | ValueSome _ ->
                            rest.[w] <- o
                            w <- w + 1

                while rest.Count > w do
                    rest.RemoveAt(rest.Count - 1)
            )

            n.FirstObserver <- newFirst

    /// Sweep the disposed entries (counted in `DeadObservers`) out of `n`'s
    /// observer list once they make up at least half of it. Each sweep is one
    /// pass that removes at least as many entries as it keeps, so disposing N
    /// scopes that share `n` costs O(N) in total, not O(N) per scope.
    let sweepDeadObservers (n: ReactiveNode) =
        if n.DeadObservers > 0 && 2 * n.DeadObservers >= observerSlots n then
            compactObservers n (fun o -> not o.Disposed)
            n.DeadObservers <- 0

    /// Unlink `node` from each of its sources at or after `fromIndex`.
    let unlinkSourcesTail (node: ReactiveNode) (fromIndex: int) =
        iterSourcesFrom node fromIndex (fun s -> removeObserver s node)

    /// Unlink a node from the graph entirely (used to dispose subscriptions).
    let dispose (node: ReactiveNode) =
        iterSourcesFrom node 0 (fun s -> removeObserver s node)
        node.FirstSource <- ValueNone
        node.RestSources |> ValueOption.iter (fun a -> a.Clear())
        node.State <- NodeState.Clean
        node.Queued <- false
