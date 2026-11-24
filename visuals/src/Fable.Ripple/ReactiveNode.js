
import { defaultOf, createAtom } from "../../fable_modules/fable-library-js.5.28.0/Util.js";
import { class_type } from "../../fable_modules/fable-library-js.5.28.0/Reflection.js";

export let Context_current = createAtom(defaultOf());

export function Context_get() {
    return Context_current();
}

/**
 * Non-generic graph node: dependency edges and marking state.
 */
export class ReactiveNode {
    constructor(flags, fn) {
        this["Flags@"] = (flags | 0);
        this["Context@"] = Context_current();
        this["DeadObservers@"] = 0;
        this["FirstSource@"] = undefined;
        this["RestSources@"] = undefined;
        this["FirstObserver@"] = undefined;
        this["RestObservers@"] = undefined;
        this["Fn@"] = fn;
        this["NextOwned@"] = undefined;
    }
}

export function ReactiveNode_$reflection() {
    return class_type("Fable.Ripple.ReactiveNode", undefined, ReactiveNode);
}

export function ReactiveNode_$ctor_2B1292C7(flags, fn) {
    return new ReactiveNode(flags, fn);
}

export function ReactiveNode__get_Flags(__) {
    return __["Flags@"] | 0;
}

export function ReactiveNode__set_Flags_Z524259A4(__, v) {
    __["Flags@"] = (v | 0);
}

/**
 * How stale this node is. Sources start `Clean`, computeds and effects
 * `Dirty` (nothing has evaluated them yet).
 */
export function ReactiveNode__get_State(this$) {
    return ReactiveNode__get_Flags(this$) & 3;
}

/**
 * How stale this node is. Sources start `Clean`, computeds and effects
 * `Dirty` (nothing has evaluated them yet).
 */
export function ReactiveNode__set_State_Z12CE0414(this$, v) {
    ReactiveNode__set_Flags_Z524259A4(this$, (ReactiveNode__get_Flags(this$) & ~3) | v);
}

/**
 * True for effects, which the scheduler queues and re-runs on flush.
 * Computeds and sources are pulled on read instead, never queued.
 */
export function ReactiveNode__get_IsEffect(this$) {
    return (ReactiveNode__get_Flags(this$) & 4) !== 0;
}

/**
 * Already sitting in the scheduler's pending queue, so repeated staleness
 * marking in one propagation pass enqueues it only once.
 */
export function ReactiveNode__get_Queued(this$) {
    return (ReactiveNode__get_Flags(this$) & 8) !== 0;
}

/**
 * Already sitting in the scheduler's pending queue, so repeated staleness
 * marking in one propagation pass enqueues it only once.
 */
export function ReactiveNode__set_Queued_Z1FBCCD16(this$, v) {
    ReactiveNode__set_Flags_Z524259A4(this$, v ? (ReactiveNode__get_Flags(this$) | 8) : (ReactiveNode__get_Flags(this$) & ~8));
}

/**
 * Set on every node of a scope at the start of teardown, so a shared
 * source's observer list can be compacted in one pass instead of one
 * removal per node. A disposed node may linger in a live source's observer
 * list until that list is swept; `Graph.iterObservers` skips it.
 */
export function ReactiveNode__get_Disposed(this$) {
    return (ReactiveNode__get_Flags(this$) & 16) !== 0;
}

/**
 * Set on every node of a scope at the start of teardown, so a shared
 * source's observer list can be compacted in one pass instead of one
 * removal per node. A disposed node may linger in a live source's observer
 * list until that list is swept; `Graph.iterObservers` skips it.
 */
export function ReactiveNode__set_Disposed_Z1FBCCD16(this$, v) {
    ReactiveNode__set_Flags_Z524259A4(this$, v ? (ReactiveNode__get_Flags(this$) | 16) : (ReactiveNode__get_Flags(this$) & ~16));
}

/**
 * Set while this node is in `Graph.sweepQueue`.
 */
export function ReactiveNode__get_Affected(this$) {
    return (ReactiveNode__get_Flags(this$) & 32) !== 0;
}

/**
 * Set while this node is in `Graph.sweepQueue`.
 */
export function ReactiveNode__set_Affected_Z1FBCCD16(this$, v) {
    ReactiveNode__set_Flags_Z524259A4(this$, v ? (ReactiveNode__get_Flags(this$) | 32) : (ReactiveNode__get_Flags(this$) & ~32));
}

/**
 * The `Context.current` this node was created under.
 */
export function ReactiveNode__get_Context(__) {
    return __["Context@"];
}

/**
 * The `Context.current` this node was created under.
 */
export function ReactiveNode__set_Context_4E60E31B(__, v) {
    __["Context@"] = v;
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
 * A computed's compute function or an effect's body; `null` on a source and
 * once the node is disposed.
 */
export function ReactiveNode__get_Fn(__) {
    return __["Fn@"];
}

/**
 * A computed's compute function or an effect's body; `null` on a source and
 * once the node is disposed.
 */
export function ReactiveNode__set_Fn_4E60E31B(__, v) {
    __["Fn@"] = v;
}

/**
 * The next node registered with the same scope.
 */
export function ReactiveNode__get_NextOwned(__) {
    return __["NextOwned@"];
}

/**
 * The next node registered with the same scope.
 */
export function ReactiveNode__set_NextOwned_Z46457FEC(__, v) {
    __["NextOwned@"] = v;
}

