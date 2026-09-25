
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";

export function Defaults_noRecompute() {
    return false;
}

/**
 * Non-generic graph node: dependency edges, marking state, and the type-erased
 * re-evaluation hook.
 */
export class ReactiveNode {
    constructor(initialState, isEffect) {
        this["State@"] = initialState;
        this["IsEffect@"] = isEffect;
        this["Queued@"] = false;
        this["Disposed@"] = false;
        this["Affected@"] = false;
        this["DeadObservers@"] = 0;
        this["FirstSource@"] = undefined;
        this["RestSources@"] = undefined;
        this["FirstObserver@"] = undefined;
        this["RestObservers@"] = undefined;
        this["Recompute@"] = (Defaults_noRecompute);
        this["EffectFn@"] = undefined;
    }
}

export function ReactiveNode_$reflection() {
    return class_type("Fable.Ripple.ReactiveNode", undefined, ReactiveNode);
}

export function ReactiveNode_$ctor_73324B86(initialState, isEffect) {
    return new ReactiveNode(initialState, isEffect);
}

/**
 * How stale this node is. Sources start `Clean`, computeds and effects
 * `Dirty` (nothing has evaluated them yet).
 */
export function ReactiveNode__get_State(__) {
    return __["State@"];
}

/**
 * How stale this node is. Sources start `Clean`, computeds and effects
 * `Dirty` (nothing has evaluated them yet).
 */
export function ReactiveNode__set_State_Z12CE0414(__, v) {
    __["State@"] = v;
}

/**
 * True for effects, which the scheduler queues and re-runs on flush.
 * Computeds and sources are pulled on read instead, never queued.
 */
export function ReactiveNode__get_IsEffect(__) {
    return __["IsEffect@"];
}

/**
 * Already sitting in the scheduler's pending queue, so repeated staleness
 * marking in one propagation pass enqueues it only once.
 */
export function ReactiveNode__get_Queued(__) {
    return __["Queued@"];
}

/**
 * Already sitting in the scheduler's pending queue, so repeated staleness
 * marking in one propagation pass enqueues it only once.
 */
export function ReactiveNode__set_Queued_Z1FBCCD16(__, v) {
    __["Queued@"] = v;
}

/**
 * Set on every node of a scope at the start of teardown, so a shared
 * source's observer list can be compacted in one pass instead of one
 * removal per node. A disposed node may linger in a live source's observer
 * list until that list is swept; `Graph.iterObservers` skips it.
 */
export function ReactiveNode__get_Disposed(__) {
    return __["Disposed@"];
}

/**
 * Set on every node of a scope at the start of teardown, so a shared
 * source's observer list can be compacted in one pass instead of one
 * removal per node. A disposed node may linger in a live source's observer
 * list until that list is swept; `Graph.iterObservers` skips it.
 */
export function ReactiveNode__set_Disposed_Z1FBCCD16(__, v) {
    __["Disposed@"] = v;
}

/**
 * Set while this node is in `Graph.sweepQueue`.
 */
export function ReactiveNode__get_Affected(__) {
    return __["Affected@"];
}

/**
 * Set while this node is in `Graph.sweepQueue`.
 */
export function ReactiveNode__set_Affected_Z1FBCCD16(__, v) {
    __["Affected@"] = v;
}

/**
 * Entries in this node's observer list that belong to disposed nodes and
 * have not been swept out yet.
 */
export function ReactiveNode__get_DeadObservers(__) {
    return __["DeadObservers@"] | 0;
}

/**
 * Entries in this node's observer list that belong to disposed nodes and
 * have not been swept out yet.
 */
export function ReactiveNode__set_DeadObservers_Z524259A4(__, v) {
    __["DeadObservers@"] = (v | 0);
}

/**
 * Logical source 0 (the nodes this one read last, in read order).
 */
export function ReactiveNode__get_FirstSource(__) {
    return __["FirstSource@"];
}

/**
 * Logical source 0 (the nodes this one read last, in read order).
 */
export function ReactiveNode__set_FirstSource_Z46457FEC(__, v) {
    __["FirstSource@"] = v;
}

/**
 * Sources at logical index 1.. (allocated only on a second source).
 */
export function ReactiveNode__get_RestSources(__) {
    return __["RestSources@"];
}

/**
 * Sources at logical index 1.. (allocated only on a second source).
 */
export function ReactiveNode__set_RestSources_6EF2C44D(__, v) {
    __["RestSources@"] = v;
}

/**
 * Logical observer 0 (the nodes that read this one; order irrelevant).
 */
export function ReactiveNode__get_FirstObserver(__) {
    return __["FirstObserver@"];
}

/**
 * Logical observer 0 (the nodes that read this one; order irrelevant).
 */
export function ReactiveNode__set_FirstObserver_Z46457FEC(__, v) {
    __["FirstObserver@"] = v;
}

/**
 * Observers at logical index 1.. (allocated only on a second observer).
 */
export function ReactiveNode__get_RestObservers(__) {
    return __["RestObservers@"];
}

/**
 * Observers at logical index 1.. (allocated only on a second observer).
 */
export function ReactiveNode__set_RestObservers_6EF2C44D(__, v) {
    __["RestObservers@"] = v;
}

/**
 * A computed's recompute hook: re-evaluate and return true if the value
 * changed. Sources and effects leave the shared no-op default - a source is
 * never evaluated, and an effect runs through `EffectFn` instead.
 */
export function ReactiveNode__get_Recompute(__) {
    return __["Recompute@"];
}

/**
 * A computed's recompute hook: re-evaluate and return true if the value
 * changed. Sources and effects leave the shared no-op default - a source is
 * never evaluated, and an effect runs through `EffectFn` instead.
 */
export function ReactiveNode__set_Recompute_233A5940(__, v) {
    __["Recompute@"] = v;
}

/**
 * An effect's body, stored directly so an effect needs no `Recompute`
 * wrapper closure. `ValueSome` only on effects.
 */
export function ReactiveNode__get_EffectFn(__) {
    return __["EffectFn@"];
}

/**
 * An effect's body, stored directly so an effect needs no `Recompute`
 * wrapper closure. `ValueSome` only on effects.
 */
export function ReactiveNode__set_EffectFn_A3DF6A2(__, v) {
    __["EffectFn@"] = v;
}

