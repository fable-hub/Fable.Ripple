
import { ReactiveNode__get_Flags, ReactiveNode_$reflection, ReactiveNode, ReactiveNode__get_Fn } from "./ReactiveNode.js";
import { defaultOf } from "../../fable_modules/fable-library-js.5.28.0/Util.js";
import { class_type } from "../../fable_modules/fable-library-js.5.28.0/Reflection.js";
import { updateIfNecessary, track } from "./Internal/Tracking.js";
import { Operators_InvalidOp } from "../../fable_modules/fable-library-js.5.28.0/FSharp.Core.js";
import { notifyChange } from "./Internal/Scheduler.js";
import { dispose } from "./Internal/Graph.js";

/**
 * The reactive node and the writable handle. A **source** (`Var.create`) is a
 * `Var<'T>` you read and write; a **computed** is the same node type built with a
 * recompute function but handed back read-only as a `Signal<'T>`. ONE runtime
 * class backs both, so every read stays monomorphic (no dispatch, no hidden-class
 * polymorphism). Reading `.Value` inside a computation registers a dependency.
 */
export class Var$1 extends ReactiveNode {
    constructor(initial, compute, equals) {
        super((compute == null) ? 0 : (2 | 64), (compute == null) ? defaultOf() : compute);
        this.equals = equals;
        this.value = initial;
    }
    get Value() {
        const this$ = this;
        return Var$1__get_Value(this$);
    }
    Peek() {
        const this$ = this;
        return Var$1__Peek(this$);
    }
    Recompute() {
        const this$ = this;
        if (ReactiveNode__get_Fn(this$) == null) {
            return false;
        }
        else {
            const nv = ReactiveNode__get_Fn(this$)();
            if (this$.equals(this$.value, nv)) {
                return false;
            }
            else {
                this$.value = nv;
                return true;
            }
        }
    }
}

export function Var$1_$reflection(gen0) {
    return class_type("Fable.Ripple.Var`1", [gen0], Var$1, ReactiveNode_$reflection());
}

export function Var$1_$ctor_Z4606F8CC(initial, compute, equals) {
    return new Var$1(initial, compute, equals);
}

/**
 * The current value. Reading tracks a dependency and pulls the signal up to
 * date; setting notifies observers when the value differs. A computed is only
 * handed out as a read-only `Signal<'T>`; writing one through a downcast throws.
 */
export function Var$1__get_Value(this$) {
    track(this$);
    updateIfNecessary(this$);
    return this$.value;
}

/**
 * The current value. Reading tracks a dependency and pulls the signal up to
 * date; setting notifies observers when the value differs. A computed is only
 * handed out as a read-only `Signal<'T>`; writing one through a downcast throws.
 */
export function Var$1__set_Value_2B595(this$, v) {
    if ((ReactiveNode__get_Flags(this$) & 64) !== 0) {
        throw Operators_InvalidOp("Cannot write to a computed signal.");
    }
    if (!this$.equals(this$.value, v)) {
        this$.value = v;
        notifyChange(this$);
    }
}

/**
 * Set the value (same as the `Value` setter).
 */
export function Var$1__Set_2B595(this$, v) {
    Var$1__set_Value_2B595(this$, v);
}

/**
 * Read the current value without registering a dependency.
 */
export function Var$1__Peek(this$) {
    updateIfNecessary(this$);
    return this$.value;
}

/**
 * This source as a read-only <c>Signal</c>. Rarely needed: every
 * <c>Signal</c> parameter accepts a <c>Var</c> directly. Use it to store a
 * source in the same collection as derived signals.
 */
export function Var$1__get_Signal(this$) {
    return this$;
}

/**
 * Side-effecting node: re-runs `fn` (re-tracking its reads) whenever a signal it
 * read changes. Backs `Signal.effect` / `Signal.subscribe`.
 */
export class Effect extends ReactiveNode {
    constructor(fn) {
        super(2 | 4, fn);
    }
    Recompute() {
        const this$ = this;
        if (!(ReactiveNode__get_Fn(this$) == null)) {
            ReactiveNode__get_Fn(this$)();
        }
        return false;
    }
    Dispose() {
        const this$ = this;
        dispose(this$);
    }
}

export function Effect_$reflection() {
    return class_type("Fable.Ripple.Effect", undefined, Effect, ReactiveNode_$reflection());
}

export function Effect_$ctor_3A5B6456(fn) {
    return new Effect(fn);
}

