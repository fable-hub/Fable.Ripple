
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";

/**
 * Computation expression for building computed signals. `let! ... and! ...` is
 * applicative (compiles to `map2`/`map3`); a lone `let!` is monadic (`bind`).
 * Every `Var` is a `Signal`, so a source or a derived signal bind alike.
 */
export class SignalBuilder {
    constructor() {
    }
}

export function SignalBuilder_$reflection() {
    return class_type("Fable.Ripple.SignalBuilder", undefined, SignalBuilder);
}

export function SignalBuilder_$ctor() {
    return new SignalBuilder();
}

export const SignalBuilderAuto_signal = SignalBuilder_$ctor();

