namespace Fable.Ripple.Internal

open Fable.Ripple

/// Everything created inside one dynamic region - a component, a list row - so
/// it can be torn down as a unit. No weak references: disposal is explicit and deterministic.
type internal Scope() =

    // Only `Nodes` is eager. A scope always registers at least one
    // effect/computed, so making it lazy would buy nothing and add a check to
    // the hot registration path. Cleanups and Children are the exception (a
    // plain list-row has neither), so they stay `ValueNone` until their cold
    // paths allocate them.

    /// The computeds and effects registered with this scope, unlinked from
    /// their sources on teardown.
    member val Nodes = ResizeArray<ReactiveNode>() with get

    /// Callbacks from `Signal.onCleanup`, run before the nodes are unlinked.
    member val Cleanups: ResizeArray<unit -> unit> voption = ValueNone with get, set

    /// Scopes opened inside this one. Disposed first, so teardown runs innermost-out.
    member val Children: ResizeArray<Scope> voption = ValueNone with get, set

    /// The cleanup list, allocated on first registration.
    member this.EnsureCleanups() =
        match this.Cleanups with
        | ValueSome a -> a
        | ValueNone ->
            let a = ResizeArray<unit -> unit>()
            this.Cleanups <- ValueSome a
            a

    /// The child list, allocated when this scope first nests another.
    member this.EnsureChildren() =
        match this.Children with
        | ValueSome a -> a
        | ValueNone ->
            let a = ResizeArray<Scope>()
            this.Children <- ValueSome a
            a

/// Ownership: which scope new computeds/effects belong to, and how a scope is torn down.
module internal Scope =

    // The scope that new computeds/effects register with (`ValueNone` = none).
    let mutable private currentScope: Scope voption = ValueNone

    /// Register a node with the current scope, if any, so it is unlinked when
    /// the scope is disposed.
    let register (node: ReactiveNode) =
        currentScope |> ValueOption.iter (fun s -> s.Nodes.Add node)

    /// Register a cleanup callback with the current scope, if any.
    let onCleanup (fn: unit -> unit) =
        currentScope |> ValueOption.iter (fun s -> s.EnsureCleanups().Add fn)

    /// Tear down a scope: dispose child scopes, run cleanups, then unlink every
    /// registered computed/effect from its sources. To avoid O(n^2) when many
    /// nodes share one external source, mark the whole scope disposed first and
    /// compact each affected source's observer list in a single pass.
    let rec dispose (scope: Scope) =
        scope.Children
        |> ValueOption.iter (fun children ->
            for i in 0 .. children.Count - 1 do
                dispose children.[i]

            children.Clear()
        )

        scope.Cleanups
        |> ValueOption.iter (fun cleanups ->
            for i in 0 .. cleanups.Count - 1 do
                cleanups.[i] ()

            cleanups.Clear()
        )

        let nodes = scope.Nodes

        for i in 0 .. nodes.Count - 1 do
            nodes.[i].Disposed <- true

        // Detach each node from its sources; collect the external sources whose
        // observer lists still reference (now-disposed) nodes.
        let affected = ResizeArray<ReactiveNode>()

        for i in 0 .. nodes.Count - 1 do
            let node = nodes.[i]

            Graph.iterSourcesFrom
                node
                0
                (fun source ->
                    if not source.Disposed && not source.Affected then
                        source.Affected <- true
                        affected.Add source
                )

            node.FirstSource <- ValueNone
            node.RestSources |> ValueOption.iter (fun a -> a.Clear())
            node.State <- NodeState.Clean
            node.Queued <- false

        // One compaction pass per affected source (O(observers), not O(nodes)).
        for i in 0 .. affected.Count - 1 do
            let source = affected.[i]
            Graph.compactObservers source (fun o -> not o.Disposed)
            source.Affected <- false

        nodes.Clear()

    /// Run `fn` inside a fresh scope nested under the current one. Returns its
    /// result and a disposer that tears the scope down.
    let root (fn: unit -> 'a) : 'a * System.IDisposable =
        let prev = currentScope
        let s = Scope()
        prev |> ValueOption.iter (fun p -> p.EnsureChildren().Add s)
        currentScope <- ValueSome s

        try
            let result = fn ()

            result,
            { new System.IDisposable with
                member _.Dispose() = dispose s
            }
        finally
            currentScope <- prev
