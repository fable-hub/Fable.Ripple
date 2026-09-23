namespace Fable.Ripple

open Fable.Core
open Fable.Ripple.Internal

/// Read-only view of a reactive value. Every `Var<'T>` is a `Signal<'T>`, so a
/// source flows into any `Signal<'T>` parameter with no conversion; a computed is
/// only ever handed out as a `Signal<'T>`, so writing it fails to compile.
type Signal<'T> =
    /// The current value. Reading tracks a dependency and pulls up to date.
    abstract Value: 'T
    /// Read the current value without registering a dependency.
    abstract Peek: unit -> 'T

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
    /// date; setting notifies observers when the value differs. A computed is only
    /// handed out as a read-only `Signal<'T>`; writing one through a downcast throws.
    member this.Value
        with get (): 'T =
            Tracking.track this
            Tracking.updateIfNecessary this
            value
        and set (v: 'T) =
            if compute.IsSome then
                invalidOp "Cannot write to a computed signal."

            if not (equals value v) then
                value <- v
                Scheduler.notifyChange this

    /// Set the value (same as the `Value` setter).
    member this.Set(v: 'T) = this.Value <- v

    /// Read the current value without registering a dependency.
    member this.Peek() : 'T =
        Tracking.updateIfNecessary this
        value

    /// <summary>
    /// This source as a read-only <c>Signal</c>. Rarely needed: every
    /// <c>Signal</c> parameter accepts a <c>Var</c> directly. Use it to store a
    /// source in the same collection as derived signals.
    /// </summary>
    /// <example>
    /// <code lang="fsharp">
    /// let count = Var.create 0
    /// let readOnly: Signal&lt;int&gt; = count.Signal
    /// </code>
    /// </example>
    member this.Signal: Signal<'T> = this :> Signal<'T>

    interface Signal<'T> with
        member this.Value = this.Value
        member this.Peek() = this.Peek()

/// Side-effecting node: re-runs `fn` (re-tracking its reads) whenever a signal it
/// read changes. Backs `Signal.effect` / `Signal.subscribe`.
type internal Effect(fn: unit -> unit) as this =
    inherit ReactiveNode(NodeState.Dirty, true)

    do this.EffectFn <- ValueSome fn
    do Scope.register (this :> ReactiveNode)
