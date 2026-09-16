namespace Fable.Ripple

/// Marking state for a reactive node. Ordered Clean < Check < Dirty; an F# enum
/// so it is a plain int at runtime (erased by Fable) - named and type-checked,
/// zero cost vs raw ints.
type internal NodeState =
    /// The value is current; nothing downstream needs touching
    | Clean = 0
    /// A transitive dependency *might* have changed (verify before recomputing)
    | Check = 1
    /// A direct dependency changed
    | Dirty = 2

/// Non-generic graph node: dependency edges, marking state, and the type-erased
/// re-evaluation hook.
type ReactiveNode internal (initialState: NodeState, isEffect: bool) =
    /// How stale this node is. Sources start `Clean`, computeds and effects
    /// `Dirty` (nothing has evaluated them yet).
    member val internal State = initialState with get, set

    /// True for effects, which the scheduler queues and re-runs on flush.
    /// Computeds and sources are pulled on read instead, never queued.
    member val internal IsEffect = isEffect with get

    /// Already sitting in the scheduler's pending queue, so repeated staleness
    /// marking in one propagation pass enqueues it only once.
    member val internal Queued = false with get, set

    /// Set on every node of a scope at the start of teardown, so a shared
    /// source's observer list can be compacted in one pass instead of one
    /// removal per node.
    member val internal Disposed = false with get, set

    /// Marks a source already collected for that single compaction pass, so it
    /// is visited once however many of its observers the scope owned.
    member val internal Affected = false with get, set

    // Edge lists use an inline-first-edge layout: the first source/observer is
    // stored directly in `First*`, and only a *second* edge allocates the `Rest*`
    // overflow `ResizeArray` (holding logical indices 1..). Almost every node in
    // a fine-grained graph has 0 or 1 edges, so this avoids a `ResizeArray` (list
    // object + backing array) per single-edge node - the dominant per-node
    // allocation - and reads the one edge from an inline field, not through an
    // array. All access goes through the `Graph.*` edge helpers, which keep the
    // invariant "no `Rest*` without a `First*`". See Internal/Graph.fs.

    /// Logical source 0 (the nodes this one read last, in read order).
    member val internal FirstSource: ReactiveNode voption = ValueNone with get, set
    /// Sources at logical index 1.. (allocated only on a second source).
    member val internal RestSources: ResizeArray<ReactiveNode> voption = ValueNone with get, set

    /// Logical observer 0 (the nodes that read this one; order irrelevant).
    member val internal FirstObserver: ReactiveNode voption = ValueNone with get, set
    /// Observers at logical index 1.. (allocated only on a second observer).
    member val internal RestObservers: ResizeArray<ReactiveNode> voption = ValueNone with get, set

    /// A computed's recompute hook: re-evaluate and return true if the value
    /// changed. Sources and effects leave the shared no-op default - a source is
    /// never evaluated, and an effect runs through `EffectFn` instead.
    member val internal Recompute: unit -> bool = (fun () -> false) with get, set

    /// An effect's body, stored directly so an effect needs no `Recompute`
    /// wrapper closure. `ValueSome` only on effects.
    member val internal EffectFn: (unit -> unit) voption = ValueNone with get, set
