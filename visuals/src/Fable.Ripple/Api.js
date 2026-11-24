
import { defaultOf, equals as equals_1 } from "../../fable_modules/fable-library-js.5.28.0/Util.js";
import { Effect_$ctor_3A5B6456, Var$1_$ctor_Z4606F8CC } from "./Types.js";
import { class_type } from "../../fable_modules/fable-library-js.5.28.0/Reflection.js";
import { Context_get, Context_current, ReactiveNode__set_Flags_Z524259A4, ReactiveNode__get_Flags } from "./ReactiveNode.js";
import { max } from "../../fable_modules/fable-library-js.5.28.0/Double.js";
import { removeReaderList, readerLists, observerCount, sourceCount, deadReaderCount } from "./Internal/Graph.js";
import { batch, markDirty } from "./Internal/Scheduler.js";
import { untracked, updateIfNecessary, observer } from "./Internal/Tracking.js";
import { ScopeModule_onCleanup, ScopeModule_root, ScopeModule_register } from "./Internal/Scope.js";

function Var_defaultEquals(a, b) {
    return equals_1(a, b);
}

/**
 * Create a writable source with a custom cutoff equality (a write is
 * a no-op when the new value is <paramref name="equals" /> the old).
 */
export function Var_createWith(equals, initial) {
    return Var$1_$ctor_Z4606F8CC(initial, undefined, equals);
}

/**
 * Create a writable source (structural equality).
 */
export function Var_create(initial) {
    return Var_createWith(Var_defaultEquals, initial);
}

/**
 * Readers of a `Signal.selector`, as flat `reader, key, run` triples.
 */
export class Selector$1 {
    constructor(source) {
        this.source = source;
        this.entries = [];
        this.limit = 96;
        this.current = this.source.Peek();
        this.deadAtCompact = 0;
    }
}

export function Selector$1_$reflection(gen0) {
    return class_type("Fable.Ripple.Selector`1", [gen0], Selector$1);
}

export function Selector$1_$ctor_Z66562DBE(source) {
    return new Selector$1(source);
}

function Selector$1__Live_Z524259A4(_, i) {
    const flags = ReactiveNode__get_Flags(_.entries[i]) | 0;
    if (!((flags & 16) !== 0)) {
        return (flags & 1073741568) === (_.entries[((i + 2) | 0)]);
    }
    else {
        return false;
    }
}

function Selector$1__Compact(this$) {
    const count = this$.entries.length | 0;
    let i = 0;
    let w = 0;
    while (i < count) {
        if (Selector$1__Live_Z524259A4(this$, i)) {
            if (i !== w) {
                this$.entries[w] = (this$.entries[i]);
                this$.entries[((w + 1) | 0)] = (this$.entries[((i + 1) | 0)]);
                this$.entries[((w + 2) | 0)] = (this$.entries[((i + 2) | 0)]);
            }
            w = ((w + 3) | 0);
        }
        i = ((i + 3) | 0);
    }
    this$.entries.length = w;
    this$.limit = (max(96, Math.imul(2, w)) | 0);
    this$.deadAtCompact = (deadReaderCount() | 0);
}

function Selector$1__Notify_Z524259A4(this$, i) {
    if (Selector$1__Live_Z524259A4(this$, i)) {
        markDirty(this$.entries[i]);
    }
}

export function Selector$1__Update(this$) {
    let a, b;
    const next = this$.source.Value;
    const prev = this$.current;
    if (!((a = prev, (b = next, (a === b) ? true : (((typeof a) === "object") && equals_1(a, b)))))) {
        this$.current = next;
        const count = this$.entries.length | 0;
        let i = 0;
        if (((typeof prev) !== "object") && ((typeof next) !== "object")) {
            while (i < count) {
                const k = this$.entries[((i + 1) | 0)];
                if ((k === prev) ? true : (k === next)) {
                    Selector$1__Notify_Z524259A4(this$, i);
                }
                i = ((i + 3) | 0);
            }
        }
        else {
            while (i < count) {
                let a_1, b_1, a_2, b_2;
                const k_1 = this$.entries[((i + 1) | 0)];
                if (((a_1 = k_1, (b_1 = prev, (a_1 === b_1) ? true : (((typeof a_1) === "object") && equals_1(a_1, b_1))))) ? true : ((a_2 = k_1, (b_2 = next, (a_2 === b_2) ? true : (((typeof a_2) === "object") && equals_1(a_2, b_2)))))) {
                    Selector$1__Notify_Z524259A4(this$, i);
                }
                i = ((i + 3) | 0);
            }
        }
    }
}

export function Selector$1__Sweep_Z524259A4(this$, deadReaders) {
    if (Math.imul(6, deadReaders - this$.deadAtCompact) >= this$.entries.length) {
        Selector$1__Compact(this$);
    }
}

export function Selector$1__Read_2B595(this$, key) {
    const option = observer();
    if (option != null) {
        const o = option;
        ReactiveNode__set_Flags_Z524259A4(o, ReactiveNode__get_Flags(o) | 128);
        this$.entries.push(o, key, (ReactiveNode__get_Flags(o) & 1073741568));
        if (this$.entries.length >= this$.limit) {
            Selector$1__Compact(this$);
        }
    }
    const a = key;
    const b = this$.source.Peek();
    if (a === b) {
        return true;
    }
    else if ((typeof a) === "object") {
        return equals_1(a, b);
    }
    else {
        return false;
    }
}

function Signal_defaultEquals(a, b) {
    return equals_1(a, b);
}

/**
 * Reference equality, to opt a signal out of the default structural compare.
 */
export function Signal_referenceEquals(a, b) {
    return a === b;
}

/**
 * A signal that never changes.
 */
export function Signal_constant(value) {
    const v = Var$1_$ctor_Z4606F8CC(value, () => value, Signal_defaultEquals);
    ScopeModule_register(v);
    return v;
}

/**
 * Create a cached derived signal with a custom cutoff equality.
 */
export function Signal_computedWith(equals, f) {
    const v = Var$1_$ctor_Z4606F8CC(defaultOf(), f, equals);
    ScopeModule_register(v);
    return v;
}

/**
 * Create a cached derived signal from an auto-tracked computation.
 */
export function Signal_computed(f) {
    return Signal_computedWith(Signal_defaultEquals, f);
}

/**
 * Map with a custom equality on the result. Accepts a source or derived signal.
 */
export function Signal_mapWith(equals, f, a) {
    return Signal_computedWith(equals, () => f(a.Value));
}

/**
 * Derive a signal by mapping one signal (structural equality).
 */
export function Signal_map(f, a) {
    return Signal_computed(() => f(a.Value));
}

/**
 * Combine two signals with a custom equality on the result.
 */
export function Signal_map2With(equals, f, a, b) {
    return Signal_computedWith(equals, () => f(a.Value, b.Value));
}

/**
 * Derive a signal by combining two signals (applicative, static deps).
 */
export function Signal_map2(f, a, b) {
    return Signal_computed(() => f(a.Value, b.Value));
}

/**
 * Derive a signal by combining three signals.
 */
export function Signal_map3(f, a, b, c) {
    return Signal_computed(() => f(a.Value, b.Value, c.Value));
}

/**
 * Derive a signal whose dependency is chosen dynamically (monadic).
 */
export function Signal_bind(f, a) {
    return Signal_computed(() => f(a.Value).Value);
}

/**
 * Run `fn` now and re-run it whenever a signal it reads changes. The effect
 * lives as long as the enclosing scope; there is no handle to dispose it earlier.
 * A body that reads no signal on its first run is not kept.
 */
export function Signal_autorun(fn) {
    const eff = Effect_$ctor_3A5B6456(fn);
    updateIfNecessary(eff);
    if ((sourceCount(eff) > 0) ? true : ((ReactiveNode__get_Flags(eff) & 128) !== 0)) {
        ScopeModule_register(eff);
    }
}

/**
 * Run `fn` now and re-run it whenever a signal it reads changes.
 */
export function Signal_effect(fn) {
    const eff = Effect_$ctor_3A5B6456(fn);
    ScopeModule_register(eff);
    updateIfNecessary(eff);
    return eff;
}

/**
 * Run `handler` now with the current value and again on every change.
 */
export function Signal_subscribe(handler, s) {
    return Signal_effect(() => {
        handler(s.Value);
    });
}

/**
 * Coalesce multiple writes into a single flush.
 */
export function Signal_batch(fn) {
    batch(fn);
}

/**
 * Run `fn` without registering any of its reads as dependencies.
 */
export function Signal_untracked(fn) {
    return untracked(fn);
}

/**
 * Run `fn` inside a fresh scope; disposing the returned handle tears it down.
 */
export function Signal_root(fn) {
    return ScopeModule_root(fn);
}

/**
 * Register a cleanup callback with the current scope.
 */
export function Signal_onCleanup(fn) {
    ScopeModule_onCleanup(fn);
}

/**
 * The ambient context, see `withContext`. `null` outside one.
 */
export function Signal_context() {
    return Context_current();
}

/**
 * Set the ambient context. Pair with a read of `context ()` to restore it;
 * `withContext` does that for a function.
 */
export function Signal_setContext(value) {
    Context_current(value);
}

/**
 * Run `fn` with `value` as the ambient context. Every effect and computed
 * created inside captures it and runs under it again when it re-evaluates.
 */
export function Signal_withContext(value, fn) {
    const prev = Context_get();
    Context_current(value);
    try {
        return fn();
    }
    finally {
        Context_current(prev);
    }
}

/**
 * Number of live observers of a signal. Diagnostic.
 */
export function Signal_observerCount(s) {
    return observerCount(s) | 0;
}

/**
 * `isSelected key` is true while `source` equals `key`. When `source`
 * changes, only the readers of the previous and the new key re-run.
 */
export function Signal_selector(source) {
    const s = Selector$1_$ctor_Z66562DBE(source);
    Signal_effect(() => {
        Selector$1__Update(s);
    });
    const sweep = (deadReaders) => {
        Selector$1__Sweep_Z524259A4(s, deadReaders);
    };
    void (readerLists.push(sweep));
    ScopeModule_onCleanup(() => {
        removeReaderList(sweep);
    });
    return (key) => Selector$1__Read_2B595(s, key);
}

