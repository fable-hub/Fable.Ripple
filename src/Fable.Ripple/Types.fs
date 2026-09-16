namespace Fable.Ripple

open Fable.Core
open Fable.Ripple.Internal

/// The reactive node and the writable handle. A **source** (`Var.create`) is a
/// `Var<'T>` you read and write; a **computed** is the same node type built with a
/// recompute function but handed back read-only as a `Signal<'T>`. ONE runtime
/// class backs both, so every read stays monomorphic (no dispatch, no hidden-class
/// polymorphism). Reading `.Value` inside a computation registers a dependency.
type Var<'T> internal (initial: 'T, compute: (unit -> 'T) voption, equals: 'T -> 'T -> bool) as this
    =
    inherit
        ReactiveNode(
            (match compute with
             | ValueSome _ -> NodeState.Dirty
             | ValueNone -> NodeState.Clean),
            false
        )

    let mutable value = initial

    // Only a computed needs a `Recompute` hook; a source's is never called.
    do
        match compute with
        | ValueSome fn ->
            this.Recompute <-
                fun () ->
                    let nv = fn ()

                    if equals value nv then
                        false
                    else
                        value <- nv
                        true
        | ValueNone -> ()

    do
        if compute.IsSome then
            Scope.register (this :> ReactiveNode)

    /// The current value. Reading tracks a dependency and pulls the signal up to
    /// date; setting notifies observers when the value differs. Computeds never
    /// reach the setter - they are only ever handed out as read-only `Signal<'T>`.
    member this.Value
        with get (): 'T =
            Tracking.track this
            Tracking.updateIfNecessary this
            value
        and set (v: 'T) =
            if not (equals value v) then
                value <- v
                Scheduler.notifyChange this

    /// Set the value (same as the `Value` setter).
    member this.Set(v: 'T) = this.Value <- v

    /// Read the current value without registering a dependency.
    member this.Peek() : 'T =
        Tracking.updateIfNecessary this
        value

/// Read-only view of a signal. A zero-cost `[<Struct; Erase>]` wrapper: at runtime
/// it IS the underlying `Var` node (no allocation, no indirection - Fable erases
/// the wrapper), but it exposes only a getter, so writing a computed fails to
/// compile. Combinators accept a source (`Var`) or a `Signal` alike via SRTP.
[<Struct; Erase>]
type Signal<'T> =
    | Signal of node: Var<'T>

    /// The current value. Reading tracks a dependency and pulls up to date.
    member inline this.Value =
        let (Signal n) = this
        n.Value

    /// Read the current value without registering a dependency.
    member inline this.Peek() =
        let (Signal n) = this
        n.Peek()

    /// The backing node (internal - lets in-assembly diagnostics reach the graph).
    member internal this.Node =
        let (Signal n) = this
        n

[<AutoOpen>]
module VarExtensions =
    type Var<'T> with
        /// <summary>
        /// A read-only view of this source, for the few places that require a
        /// <c>Signal</c> value - for example <c>Signal.observerCount</c>, or storing
        /// a source in the same collection as derived signals. You rarely need it:
        /// <c>map</c>, <c>bind</c> and the <c>signal { }</c> builder accept a source
        /// directly.
        /// </summary>
        /// <example>
        /// <code lang="fsharp">
        /// let count = Var.create 0
        /// let readOnly: Signal&lt;int&gt; = count.Signal
        /// </code>
        /// </example>
        member inline this.Signal: Signal<'T> = Signal this

/// Side-effecting node: re-runs `fn` (re-tracking its reads) whenever a signal it
/// read changes. Backs `Signal.effect` / `Signal.subscribe`.
type internal Effect(fn: unit -> unit) as this =
    inherit ReactiveNode(NodeState.Dirty, true)

    do this.EffectFn <- ValueSome fn
    do Scope.register (this :> ReactiveNode)
