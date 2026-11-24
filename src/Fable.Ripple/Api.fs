namespace Fable.Ripple

open System
open Fable.Core
open Fable.Core.JsInterop
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

/// Readers of a `Signal.selector`, as flat `reader, key, run` triples.
type internal Selector<'k>(source: Signal<'k>) =
    let entries = ResizeArray<obj>()
    let mutable limit = 96
    let mutable current = source.Peek()
    let mutable deadAtCompact = 0

    [<Emit("$0[$1]")>]
    static member private At(a: ResizeArray<obj>, i: int) : obj = jsNative

    [<Emit("$0[$1] = $2")>]
    static member private SetAt(a: ResizeArray<obj>, i: int, v: obj) : unit = jsNative

    [<Emit("$0.push($1, $2, $3)")>]
    static member private Push(a: ResizeArray<obj>, r: obj, k: obj, run: int) : unit = jsNative

    [<Emit("$0.length = $1")>]
    static member private Truncate(a: ResizeArray<obj>, n: int) : unit = jsNative

    static member inline private Same(a: 'k, b: 'k) =
        obj.ReferenceEquals(a, b) || (jsTypeof a = "object" && Unchecked.equals a b)

    member private _.Live(i: int) =
        let r: ReactiveNode = unbox (Selector<'k>.At(entries, i))
        let flags = r.Flags

        not (NodeFlags.has NodeFlags.Disposed flags)
        && NodeFlags.runStamp flags = unbox<int> (Selector<'k>.At(entries, i + 2))

    member private this.Compact() =
        let count = entries.Count
        let mutable i = 0
        let mutable w = 0

        while i < count do
            if this.Live i then
                if i <> w then
                    Selector<'k>.SetAt(entries, w, Selector<'k>.At(entries, i))
                    Selector<'k>.SetAt(entries, w + 1, Selector<'k>.At(entries, i + 1))
                    Selector<'k>.SetAt(entries, w + 2, Selector<'k>.At(entries, i + 2))

                w <- w + 3

            i <- i + 3

        Selector<'k>.Truncate(entries, w)
        limit <- max 96 (2 * w)
        deadAtCompact <- Graph.deadReaderCount ()

    member private this.Notify(i: int) =
        if this.Live i then
            Scheduler.markDirty (unbox (Selector<'k>.At(entries, i)))

    member this.Update() =
        let next = source.Value
        let prev = current

        if not (Selector<'k>.Same(prev, next)) then
            current <- next
            let count = entries.Count
            let mutable i = 0

            if jsTypeof prev <> "object" && jsTypeof next <> "object" then
                while i < count do
                    let k = Selector<'k>.At(entries, i + 1)

                    if obj.ReferenceEquals(k, prev) || obj.ReferenceEquals(k, next) then
                        this.Notify i

                    i <- i + 3
            else
                while i < count do
                    let k: 'k = unbox (Selector<'k>.At(entries, i + 1))

                    if Selector<'k>.Same(k, prev) || Selector<'k>.Same(k, next) then
                        this.Notify i

                    i <- i + 3

    member this.Sweep(deadReaders: int) =
        if 6 * (deadReaders - deadAtCompact) >= entries.Count then
            this.Compact()

    member this.Read(key: 'k) : bool =
        Tracking.observer ()
        |> ValueOption.iter (fun o ->
            o.Flags <- NodeFlags.add NodeFlags.External o.Flags
            Selector<'k>.Push(entries, o, box key, NodeFlags.runStamp o.Flags)

            if entries.Count >= limit then
                this.Compact()
        )

        Selector<'k>.Same(key, source.Peek())

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
        let v = Var<'T>(value, ValueSome(fun () -> value), defaultEquals)
        Scope.register v
        v :> Signal<'T>

    /// Create a cached derived signal with a custom cutoff equality.
    let computedWith (equals: 'T -> 'T -> bool) (f: unit -> 'T) : Signal<'T> =
        let v = Var<'T>(Unchecked.defaultof<'T>, ValueSome f, equals)
        Scope.register v
        v :> Signal<'T>

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

    /// Run `fn` now and re-run it whenever a signal it reads changes. The effect
    /// lives as long as the enclosing scope; there is no handle to dispose it earlier.
    /// A body that reads no signal on its first run is not kept.
    let autorun (fn: unit -> unit) : unit =
        let eff = new Effect(fn)
        Tracking.updateIfNecessary eff

        if Graph.sourceCount eff > 0 || NodeFlags.has NodeFlags.External eff.Flags then
            Scope.register eff

    /// Run `fn` now and re-run it whenever a signal it reads changes.
    let effect (fn: unit -> unit) : IDisposable =
        let eff = new Effect(fn)
        Scope.register eff
        Tracking.updateIfNecessary eff
        eff

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

    /// The ambient context, see `withContext`. `null` outside one.
    let internal context () : obj = Context.current

    /// Set the ambient context. Pair with a read of `context ()` to restore it;
    /// `withContext` does that for a function.
    let internal setContext (value: obj) : unit = Context.current <- value

    /// Run `fn` with `value` as the ambient context. Every effect and computed
    /// created inside captures it and runs under it again when it re-evaluates.
    let internal withContext (value: obj) (fn: unit -> 'a) : 'a =
        let prev = Context.get ()
        Context.current <- value

        try
            fn ()
        finally
            Context.current <- prev

    /// Number of live observers of a signal. Diagnostic.
    let observerCount (s: Signal<'T>) : int =
        Graph.observerCount (s :?> ReactiveNode)

    /// `isSelected key` is true while `source` equals `key`. When `source`
    /// changes, only the readers of the previous and the new key re-run.
    let selector (source: Signal<'k>) : 'k -> bool =
        let s = Selector<'k>(source)
        effect s.Update |> ignore
        let sweep = s.Sweep
        Graph.readerLists.Add sweep
        Scope.onCleanup (fun () -> Graph.removeReaderList sweep)
        s.Read
