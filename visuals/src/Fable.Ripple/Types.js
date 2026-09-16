
import { defaultOf } from "../../fable_modules/fable-library-js.5.13.0/Util.js";
import { FSharpRef } from "../../fable_modules/fable-library-js.5.13.0/Types.js";
import { ReactiveNode__set_EffectFn_A3DF6A2, ReactiveNode_$reflection, ReactiveNode, ReactiveNode__set_Recompute_233A5940 } from "./ReactiveNode.js";
import { ScopeModule_register } from "./Internal/Scope.js";
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { updateIfNecessary, track } from "./Internal/Tracking.js";
import { notifyChange } from "./Internal/Scheduler.js";

/**
 * The reactive node and the writable handle. A **source** (`Var.create`) is a
 * `Var<'T>` you read and write; a **computed** is the same node type built with a
 * recompute function but handed back read-only as a `Signal<'T>`. ONE runtime
 * class backs both, so every read stays monomorphic (no dispatch, no hidden-class
 * polymorphism). Reading `.Value` inside a computation registers a dependency.
 */
export class Var$1 extends ReactiveNode {
    constructor(initial, compute, equals) {
        super((compute == null) ? 0 : 2, false);
        const this$ = new FSharpRef(defaultOf());
        this.equals = equals;
        this$.contents = this;
        this.value = initial;
        this["init@11"] = 1;
        if (compute == null) {
        }
        else {
            const fn = compute;
            ReactiveNode__set_Recompute_233A5940(this$.contents, () => {
                const nv = fn();
                if (this.equals(this.value, nv)) {
                    return false;
                }
                else {
                    this.value = nv;
                    return true;
                }
            });
        }
        if (compute != null) {
            ScopeModule_register(this$.contents);
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
 * date; setting notifies observers when the value differs. Computeds never
 * reach the setter - they are only ever handed out as read-only `Signal<'T>`.
 */
export function Var$1__get_Value(this$) {
    track(this$);
    updateIfNecessary(this$);
    return this$.value;
}

/**
 * The current value. Reading tracks a dependency and pulls the signal up to
 * date; setting notifies observers when the value differs. Computeds never
 * reach the setter - they are only ever handed out as read-only `Signal<'T>`.
 */
export function Var$1__set_Value_2B595(this$, v) {
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
 * The backing node (internal - lets in-assembly diagnostics reach the graph).
 */
export function Signal$1__get_Node(this$) {
    return this$;
}

/**
 * Side-effecting node: re-runs `fn` (re-tracking its reads) whenever a signal it
 * read changes. Backs `Signal.effect` / `Signal.subscribe`.
 */
export class Effect extends ReactiveNode {
    constructor(fn) {
        super(2, true);
        const this$ = new FSharpRef(defaultOf());
        this$.contents = this;
        this["init@106-1"] = 1;
        ReactiveNode__set_EffectFn_A3DF6A2(this$.contents, fn);
        ScopeModule_register(this$.contents);
    }
}

export function Effect_$reflection() {
    return class_type("Fable.Ripple.Effect", undefined, Effect, ReactiveNode_$reflection());
}

export function Effect_$ctor_3A5B6456(fn) {
    return new Effect(fn);
}

