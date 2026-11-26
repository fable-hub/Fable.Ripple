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

/// An ambient value captured by every node at creation and restored around its
/// re-evaluation, so a computation built for one list row keeps seeing that
/// row. `null` outside any context.
module internal Context =

    let mutable current: obj = null

    // Fable inlines `let prev = current` into its use site, which in a `try/finally`
    // is after the mutation; going through a function pins the read.
    let get () : obj = current

module internal NodeFlags =

    [<Literal>]
    let StateMask = 3

    [<Literal>]
    let IsEffect = 4

    [<Literal>]
    let Queued = 8

    [<Literal>]
    let Disposed = 16

    [<Literal>]
    let Affected = 32

    [<Literal>]
    let IsComputed = 64

    [<Literal>]
    let External = 128

    /// Set while the node has a scope in `Owner.owned`.
    [<Literal>]
    let HasOwned = 256

    /// Set once the node's `Fn` disposes the previous run's scope before running.
    [<Literal>]
    let DisposesOwned = 512

    /// Bits 10 to 29 count the node's evaluations and wrap to 0.
    [<Literal>]
    let Run = 1024

    [<Literal>]
    let FlagMask = 1023

    /// Bits 30 and 31 stay clear so that V8 keeps the field a small integer.
    [<Literal>]
    let RunMask = 0x3FFFFC00

    let inline has (flag: int) (flags: int) = flags &&& flag <> 0

    let inline add (flag: int) (flags: int) = flags ||| flag

    /// The evaluation count, as stored in the flags. Changes on every run.
    let inline runStamp (flags: int) = flags &&& RunMask

    let inline countRun (flags: int) =
        (flags &&& FlagMask) ||| ((flags + Run) &&& RunMask)

/// Re-evaluation hook of a computed or an effect.
type internal IRecompute =
    /// Re-evaluate and return true if the value changed.
    abstract Recompute: unit -> bool

/// Non-generic graph node: dependency edges and marking state.
type ReactiveNode internal (flags: int, fn: obj) =

    member val internal Flags = flags with get, set

    /// How stale this node is. Sources start `Clean`, computeds and effects
    /// `Dirty` (nothing has evaluated them yet).
    member internal this.State
        with get (): NodeState = enum<NodeState> (this.Flags &&& NodeFlags.StateMask)
        and set (v: NodeState) = this.Flags <- (this.Flags &&& ~~~NodeFlags.StateMask) ||| int v

    /// True for effects, which the scheduler queues and re-runs on flush.
    /// Computeds and sources are pulled on read instead, never queued.
    member internal this.IsEffect = this.Flags &&& NodeFlags.IsEffect <> 0

    /// Already sitting in the scheduler's pending queue, so repeated staleness
    /// marking in one propagation pass enqueues it only once.
    member internal this.Queued
        with get () = this.Flags &&& NodeFlags.Queued <> 0
        and set (v: bool) =
            this.Flags <-
                if v then
                    this.Flags ||| NodeFlags.Queued
                else
                    this.Flags &&& ~~~NodeFlags.Queued

    /// Set on every node of a scope at the start of teardown, so a shared
    /// source's observer list can be compacted in one pass instead of one
    /// removal per node. A disposed node may linger in a live source's observer
    /// list until that list is swept; `Graph.iterObservers` skips it.
    member internal this.Disposed
        with get () = this.Flags &&& NodeFlags.Disposed <> 0
        and set (v: bool) =
            this.Flags <-
                if v then
                    this.Flags ||| NodeFlags.Disposed
                else
                    this.Flags &&& ~~~NodeFlags.Disposed

    /// Set while this node is in `Graph.sweepQueue`.
    member internal this.Affected
        with get () = this.Flags &&& NodeFlags.Affected <> 0
        and set (v: bool) =
            this.Flags <-
                if v then
                    this.Flags ||| NodeFlags.Affected
                else
                    this.Flags &&& ~~~NodeFlags.Affected

    /// The `Context.current` this node was created under.
    member val internal Context: obj = Context.current with get, set

    /// Entries in this node's observer list that belong to disposed nodes and
    /// have not been swept out yet.
    member val internal DeadObservers = 0 with get, set

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

    /// A computed's compute function or an effect's body; `null` on a source and
    /// once the node is disposed.
    member val internal Fn: obj = fn with get, set

    /// The next node registered with the same scope.
    member val internal NextOwned: ReactiveNode voption = ValueNone with get, set

/// The scope of what a node's last run created.
module internal Owner =

    // Weak: a node that owns something but is in no scope must still be collectable.
    let private owned =
        Fable.Core.JS.Constructors.WeakMap.Create<ReactiveNode, System.IDisposable>()

    let getOwned (node: ReactiveNode) : System.IDisposable =
        if NodeFlags.has NodeFlags.HasOwned node.Flags then
            owned.get node
        else
            null

    let setOwned (node: ReactiveNode) (s: System.IDisposable) =
        node.Flags <- node.Flags ||| NodeFlags.HasOwned
        owned.set (node, s) |> ignore

    let disposeOwned (node: ReactiveNode) =
        if NodeFlags.has NodeFlags.HasOwned node.Flags then
            node.Flags <- node.Flags &&& ~~~NodeFlags.HasOwned
            let s = owned.get node
            owned.delete node |> ignore
            s.Dispose()
