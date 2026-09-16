
import { defaultOf, equals as equals_1 } from "../../fable_modules/fable-library-js.5.13.0/Util.js";
import { Signal$1__get_Node, Effect_$ctor_3A5B6456, Var$1_$ctor_Z4606F8CC } from "./Types.js";
import { untracked, updateIfNecessary } from "./Internal/Tracking.js";
import { observerCount, dispose } from "./Internal/Graph.js";
import { batch } from "./Internal/Scheduler.js";
import { ScopeModule_onCleanup, ScopeModule_root } from "./Internal/Scope.js";

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
    return Var$1_$ctor_Z4606F8CC(value, () => value, Signal_defaultEquals);
}

/**
 * Create a cached derived signal with a custom cutoff equality.
 */
export function Signal_computedWith(equals, f) {
    return Var$1_$ctor_Z4606F8CC(defaultOf(), f, equals);
}

/**
 * Create a cached derived signal from an auto-tracked computation.
 */
export function Signal_computed(f) {
    return Signal_computedWith(Signal_defaultEquals, f);
}

/**
 * Run `fn` now and re-run it whenever a signal it reads changes.
 */
export function Signal_effect(fn) {
    const eff = Effect_$ctor_3A5B6456(fn);
    updateIfNecessary(eff);
    return {
        Dispose() {
            dispose(eff);
        },
    };
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
 * Number of live observers of a signal. Diagnostic. Takes a read-only view;
 * pass a source as `source.Signal`.
 */
export function Signal_observerCount(s) {
    return observerCount(Signal$1__get_Node(s)) | 0;
}

