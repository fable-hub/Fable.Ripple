namespace Fable.Ripple

/// Computation expression for building computed signals. `let! ... and! ...` is
/// applicative (compiles to `map2`/`map3`); a lone `let!` is monadic (`bind`).
/// Members are `inline` so a source (`Var`) or a derived `Signal` bind alike.
type SignalBuilder() =
    member inline _.Return(x: 'T) : Signal<'T> = Signal.constant x
    member inline _.ReturnFrom(s: Signal<'T>) : Signal<'T> = s

    member inline _.Bind
        (s: ^s when ^s: (member get_Value: unit -> 'a), f: 'a -> Signal<'b>)
        : Signal<'b>
        =
        Signal.bind f s

    member inline _.BindReturn
        (s: ^s when ^s: (member get_Value: unit -> 'a), f: 'a -> 'b)
        : Signal<'b>
        =
        Signal.map f s

    member inline _.MergeSources
        (
            a: ^sa when ^sa: (member get_Value: unit -> 'a),
            b: ^sb when ^sb: (member get_Value: unit -> 'b)
        )
        : Signal<'a * 'b>
        =
        Signal.map2 (fun x y -> x, y) a b

[<AutoOpen>]
module SignalBuilderAuto =
    let signal = SignalBuilder()
