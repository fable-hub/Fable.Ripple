namespace Fable.Ripple

open System
open Fable.Ripple.Internal

/// Creating writable sources. A `Var<'T>` is a source you read and write; derive
/// read-only `Signal<'T>` values from it with the `Signal` module.
[<RequireQualifiedAccess>]
module Var =

    let private defaultEquals<'T> (a: 'T) (b: 'T) = Unchecked.equals a b

    /// <summary>Create a writable source with a custom cutoff equality (a write is
    /// a no-op when the new value is <paramref name="equals" /> the old).</summary>
    /// <example><code lang="fsharp">
    /// let name = Var.createWith (fun a b -> System.String.Equals(a, b, System.StringComparison.OrdinalIgnoreCase)) "ada"
    /// </code></example>
    let createWith (equals: 'T -> 'T -> bool) (initial: 'T) : Var<'T> =
        Var<'T>(initial, ValueNone, equals)

    /// <summary>Create a writable source (structural equality).</summary>
    /// <example><code lang="fsharp">
    /// let count = Var.create 0
    /// count.Value &lt;- 1
    /// </code></example>
    let create (initial: 'T) : Var<'T> = createWith defaultEquals initial

/// The public API: creating sources and computeds, deriving, observing, and scoping.
///
/// A source is a `Var<'T>` (writable); everything derived is a read-only
/// `Signal<'T>`. Every `Var<'T>` is a `Signal<'T>`, so the deriving combinators
/// (`map`, `map2`, ...) accept a source or a derived signal alike, and a `Var`
/// pipes straight in with no `.Signal` conversion at the call site.
[<RequireQualifiedAccess>]
module Signal =

    let private defaultEquals<'T> (a: 'T) (b: 'T) = Unchecked.equals a b

    /// Reference equality, to opt a signal out of the default structural compare.
    let referenceEquals<'T when 'T: not struct> (a: 'T) (b: 'T) : bool = obj.ReferenceEquals(a, b)

    /// A signal that never changes.
    let constant (value: 'T) : Signal<'T> =
        Var<'T>(value, ValueSome(fun () -> value), defaultEquals)

    /// Create a cached derived signal with a custom cutoff equality.
    let computedWith (equals: 'T -> 'T -> bool) (f: unit -> 'T) : Signal<'T> =
        Var<'T>(Unchecked.defaultof<'T>, ValueSome f, equals)

    /// Create a cached derived signal from an auto-tracked computation.
    let computed (f: unit -> 'T) : Signal<'T> = computedWith defaultEquals f

    /// Map with a custom equality on the result. Accepts a source or derived signal.
    let mapWith (equals: 'b -> 'b -> bool) (f: 'a -> 'b) (a: Signal<'a>) : Signal<'b> =
        computedWith equals (fun () -> f a.Value)

    /// Derive a signal by mapping one signal (structural equality).
    let map (f: 'a -> 'b) (a: Signal<'a>) : Signal<'b> = computed (fun () -> f a.Value)

    /// Combine two signals with a custom equality on the result.
    let map2With
        (equals: 'c -> 'c -> bool)
        (f: 'a -> 'b -> 'c)
        (a: Signal<'a>)
        (b: Signal<'b>)
        : Signal<'c>
        =
        computedWith equals (fun () -> f a.Value b.Value)

    /// Derive a signal by combining two signals (applicative, static deps).
    let map2 (f: 'a -> 'b -> 'c) (a: Signal<'a>) (b: Signal<'b>) : Signal<'c> =
        computed (fun () -> f a.Value b.Value)

    /// Derive a signal by combining three signals.
    let map3
        (f: 'a -> 'b -> 'c -> 'd)
        (a: Signal<'a>)
        (b: Signal<'b>)
        (c: Signal<'c>)
        : Signal<'d>
        =
        computed (fun () -> f a.Value b.Value c.Value)

    /// Derive a signal whose dependency is chosen dynamically (monadic).
    let bind (f: 'a -> Signal<'b>) (a: Signal<'a>) : Signal<'b> =
        computed (fun () -> (f a.Value).Value)

    /// Read the current value without registering a dependency.
    let inline peek (s: Signal<'a>) : 'a = s.Peek()

    /// Run `fn` now and re-run it whenever a signal it reads changes.
    let effect (fn: unit -> unit) : IDisposable =
        let eff = Effect(fn)
        Tracking.updateIfNecessary eff

        { new IDisposable with
            member _.Dispose() = Graph.dispose eff
        }

    /// Run `handler` now with the current value and again on every change.
    let subscribe (handler: 'a -> unit) (s: Signal<'a>) : IDisposable =
        effect (fun () -> handler s.Value)

    /// Coalesce multiple writes into a single flush.
    let batch (fn: unit -> unit) : unit = Scheduler.batch fn

    /// Run `fn` without registering any of its reads as dependencies.
    let untracked (fn: unit -> 'a) : 'a = Tracking.untracked fn

    /// Run `fn` inside a fresh scope; disposing the returned handle tears it down.
    let root (fn: unit -> 'a) : 'a * IDisposable = Scope.root fn

    /// Register a cleanup callback with the current scope.
    let onCleanup (fn: unit -> unit) : unit = Scope.onCleanup fn

    /// Number of live observers of a signal. Diagnostic.
    let observerCount (s: Signal<'T>) : int =
        Graph.observerCount (s :?> ReactiveNode)
