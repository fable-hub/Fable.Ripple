namespace Fable.Ripple

/// Computation expression for building computed signals. `let! ... and! ...` is
/// applicative (compiles to `map2`/`map3`); a lone `let!` is monadic (`bind`).
/// Every `Var` is a `Signal`, so a source or a derived signal bind alike.
type SignalBuilder() =
    member inline _.Return(x: 'T) : Signal<'T> = Signal.constant x
    member inline _.ReturnFrom(s: Signal<'T>) : Signal<'T> = s

    member inline _.Bind(s: Signal<'a>, f: 'a -> Signal<'b>) : Signal<'b> = Signal.bind f s

    member inline _.BindReturn(s: Signal<'a>, f: 'a -> 'b) : Signal<'b> = Signal.map f s

    member inline _.MergeSources(a: Signal<'a>, b: Signal<'b>) : Signal<'a * 'b> =
        Signal.map2 (fun x y -> x, y) a b

[<AutoOpen>]
module SignalBuilderAuto =
    let signal = SignalBuilder()
