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
/// `Signal<'T>`. The deriving combinators (`map`, `map2`, ...) are `inline` and
/// accept a source or a derived signal alike (via SRTP on `.Value`), so a `Var`
/// pipes straight in with no `.Signal` conversion at the call site.
[<RequireQualifiedAccess>]
module Signal =

    let private defaultEquals<'T> (a: 'T) (b: 'T) = Unchecked.equals a b

    /// Reference equality, to opt a signal out of the default structural compare.
    let referenceEquals<'T when 'T: not struct> (a: 'T) (b: 'T) : bool = obj.ReferenceEquals(a, b)

    /// A signal that never changes.
    let constant (value: 'T) : Signal<'T> =
        Signal(Var<'T>(value, ValueSome(fun () -> value), defaultEquals))

    /// Create a cached derived signal with a custom cutoff equality.
    let computedWith (equals: 'T -> 'T -> bool) (f: unit -> 'T) : Signal<'T> =
        Signal(Var<'T>(Unchecked.defaultof<'T>, ValueSome f, equals))

    /// Create a cached derived signal from an auto-tracked computation.
    let computed (f: unit -> 'T) : Signal<'T> = computedWith defaultEquals f

    /// Map with a custom equality on the result. Accepts a source or derived signal.
    let inline mapWith
        (equals: 'b -> 'b -> bool)
        (f: 'a -> 'b)
        (a: ^s when ^s: (member get_Value: unit -> 'a))
        : Signal<'b>
        =
        computedWith equals (fun () -> f (^s: (member get_Value: unit -> 'a) a))

    /// Derive a signal by mapping one signal (structural equality).
    let inline map (f: 'a -> 'b) (a: ^s when ^s: (member get_Value: unit -> 'a)) : Signal<'b> =
        computed (fun () -> f (^s: (member get_Value: unit -> 'a) a))

    /// Combine two signals with a custom equality on the result.
    let inline map2With
        (equals: 'c -> 'c -> bool)
        (f: 'a -> 'b -> 'c)
        (a: ^sa when ^sa: (member get_Value: unit -> 'a))
        (b: ^sb when ^sb: (member get_Value: unit -> 'b))
        : Signal<'c>
        =
        computedWith
            equals
            (fun () ->
                f (^sa: (member get_Value: unit -> 'a) a) (^sb: (member get_Value: unit -> 'b) b)
            )

    /// Derive a signal by combining two signals (applicative, static deps).
    let inline map2
        (f: 'a -> 'b -> 'c)
        (a: ^sa when ^sa: (member get_Value: unit -> 'a))
        (b: ^sb when ^sb: (member get_Value: unit -> 'b))
        : Signal<'c>
        =
        computed (fun () ->
            f (^sa: (member get_Value: unit -> 'a) a) (^sb: (member get_Value: unit -> 'b) b)
        )

    /// Derive a signal by combining three signals.
    let inline map3
        (f: 'a -> 'b -> 'c -> 'd)
        (a: ^sa when ^sa: (member get_Value: unit -> 'a))
        (b: ^sb when ^sb: (member get_Value: unit -> 'b))
        (c: ^sc when ^sc: (member get_Value: unit -> 'c))
        : Signal<'d>
        =
        computed (fun () ->
            f
                (^sa: (member get_Value: unit -> 'a) a)
                (^sb: (member get_Value: unit -> 'b) b)
                (^sc: (member get_Value: unit -> 'c) c)
        )

    /// Derive a signal whose dependency is chosen dynamically (monadic).
    let inline bind
        (f: 'a -> Signal<'b>)
        (a: ^s when ^s: (member get_Value: unit -> 'a))
        : Signal<'b>
        =
        computed (fun () -> (f (^s: (member get_Value: unit -> 'a) a)).Value)

    /// Read the current value without registering a dependency.
    let inline peek (s: ^s when ^s: (member Peek: unit -> 'a)) : 'a =
        (^s: (member Peek: unit -> 'a) s)

    /// Run `fn` now and re-run it whenever a signal it reads changes.
    let effect (fn: unit -> unit) : IDisposable =
        let eff = Effect(fn)
        Tracking.updateIfNecessary eff

        { new IDisposable with
            member _.Dispose() = Graph.dispose eff
        }

    /// Run `handler` now with the current value and again on every change.
    let inline subscribe
        (handler: 'a -> unit)
        (s: ^s when ^s: (member get_Value: unit -> 'a))
        : IDisposable
        =
        effect (fun () -> handler (^s: (member get_Value: unit -> 'a) s))

    /// Coalesce multiple writes into a single flush.
    let batch (fn: unit -> unit) : unit = Scheduler.batch fn

    /// Run `fn` without registering any of its reads as dependencies.
    let untracked (fn: unit -> 'a) : 'a = Tracking.untracked fn

    /// Run `fn` inside a fresh scope; disposing the returned handle tears it down.
    let root (fn: unit -> 'a) : 'a * IDisposable = Scope.root fn

    /// Register a cleanup callback with the current scope.
    let onCleanup (fn: unit -> unit) : unit = Scope.onCleanup fn

    /// Number of live observers of a signal. Diagnostic. Takes a read-only view;
    /// pass a source as `source.Signal`.
    let observerCount (s: Signal<'T>) : int =
        Graph.observerCount (s.Node :> ReactiveNode)
