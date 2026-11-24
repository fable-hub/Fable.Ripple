
import { noteDeadObserver, noteDeadReader, releaseSweeps, holdSweeps } from "./Graph.js";
import { class_type } from "../../../fable_modules/fable-library-js.5.28.0/Reflection.js";
import { setItem, item } from "../../../fable_modules/fable-library-js.5.28.0/Array.js";
import { disposeSafe, defaultOf, clear } from "../../../fable_modules/fable-library-js.5.28.0/Util.js";
import { value } from "../../../fable_modules/fable-library-js.5.28.0/Option.js";
import { ReactiveNode__set_NextOwned_Z46457FEC, ReactiveNode__set_Fn_4E60E31B, ReactiveNode__set_Queued_Z1FBCCD16, ReactiveNode__set_State_Z12CE0414, ReactiveNode__set_RestSources_6EF2C44D, ReactiveNode__set_FirstSource_Z46457FEC, ReactiveNode__get_RestSources, ReactiveNode__get_Disposed, ReactiveNode__get_FirstSource, ReactiveNode__get_NextOwned, ReactiveNode__get_Flags, ReactiveNode__set_Disposed_Z1FBCCD16 } from "../ReactiveNode.js";
import { max } from "../../../fable_modules/fable-library-js.5.28.0/Double.js";

/**
 * Everything created inside one dynamic region - a component, a list row - so
 * it can be torn down as a unit. No weak references: disposal is explicit and deterministic.
 */
export class Scope {
    constructor() {
        this["First@"] = undefined;
        this["Cleanups@"] = undefined;
        this["Children@"] = undefined;
        this["Disposed@"] = false;
        this["CompactAt@"] = 8;
    }
    Dispose() {
        const this$ = this;
        if (!Scope__get_Disposed(this$)) {
            holdSweeps();
            try {
                Scope__TearDown(this$);
            }
            finally {
                releaseSweeps();
            }
        }
    }
}

export function Scope_$reflection() {
    return class_type("Fable.Ripple.Internal.Scope", undefined, Scope);
}

export function Scope_$ctor() {
    return new Scope();
}

/**
 * The computeds and effects registered with this scope, unlinked from
 * their sources on teardown.
 */
export function Scope__get_First(__) {
    return __["First@"];
}

/**
 * The computeds and effects registered with this scope, unlinked from
 * their sources on teardown.
 */
export function Scope__set_First_Z46457FEC(__, v) {
    __["First@"] = v;
}

/**
 * Callbacks from `Signal.onCleanup`, run before the nodes are unlinked.
 */
export function Scope__get_Cleanups(__) {
    return __["Cleanups@"];
}

/**
 * Callbacks from `Signal.onCleanup`, run before the nodes are unlinked.
 */
export function Scope__set_Cleanups_Z24F84585(__, v) {
    __["Cleanups@"] = v;
}

/**
 * Scopes opened inside this one. Disposed first, so teardown runs innermost-out.
 */
export function Scope__get_Children(__) {
    return __["Children@"];
}

/**
 * Scopes opened inside this one. Disposed first, so teardown runs innermost-out.
 */
export function Scope__set_Children_Z761CFBEB(__, v) {
    __["Children@"] = v;
}

/**
 * Set once torn down, so a second `dispose` is a no-op and so a parent can
 * tell a dead child from a live one.
 */
export function Scope__get_Disposed(__) {
    return __["Disposed@"];
}

/**
 * Set once torn down, so a second `dispose` is a no-op and so a parent can
 * tell a dead child from a live one.
 */
export function Scope__set_Disposed_Z1FBCCD16(__, v) {
    __["Disposed@"] = v;
}

/**
 * `Children.Count` at which the next sweep of dead children runs.
 */
export function Scope__get_CompactAt(__) {
    return __["CompactAt@"] | 0;
}

/**
 * `Children.Count` at which the next sweep of dead children runs.
 */
export function Scope__set_CompactAt_Z524259A4(__, v) {
    __["CompactAt@"] = (v | 0);
}

/**
 * The cleanup list, allocated on first registration.
 */
export function Scope__EnsureCleanups(this$) {
    const matchValue = Scope__get_Cleanups(this$);
    if (matchValue == null) {
        const a_1 = [];
        Scope__set_Cleanups_Z24F84585(this$, a_1);
        return a_1;
    }
    else {
        return matchValue;
    }
}

/**
 * The child list, allocated when this scope first nests another.
 */
export function Scope__EnsureChildren(this$) {
    const matchValue = Scope__get_Children(this$);
    if (matchValue == null) {
        const a_1 = [];
        Scope__set_Children_Z761CFBEB(this$, a_1);
        return a_1;
    }
    else {
        return matchValue;
    }
}

function Scope__TearDown(this$) {
    Scope__set_Disposed_Z1FBCCD16(this$, true);
    const option = Scope__get_Children(this$);
    if (option != null) {
        const children = option;
        for (let i = 0, limit = ((children.length - 1) | 0); i <= limit; i++) {
            Scope__TearDown(children[i] ?? item(i, children));
        }
        clear(children);
    }
    const option_1 = Scope__get_Cleanups(this$);
    if (option_1 != null) {
        const cleanups = option_1;
        for (let i_1 = 0, limit_1 = ((cleanups.length - 1) | 0); i_1 <= limit_1; i_1++) {
            (cleanups[i_1] ?? item(i_1, cleanups))();
        }
        clear(cleanups);
    }
    let cursor = Scope__get_First(this$);
    while (cursor != null) {
        const node = value(cursor);
        ReactiveNode__set_Disposed_Z1FBCCD16(node, true);
        if ((ReactiveNode__get_Flags(node) & 128) !== 0) {
            noteDeadReader();
        }
        cursor = ReactiveNode__get_NextOwned(node);
    }
    cursor = Scope__get_First(this$);
    Scope__set_First_Z46457FEC(this$, undefined);
    while (cursor != null) {
        const node_1 = value(cursor);
        const n = node_1;
        if (0 <= 0) {
            const option_2 = ReactiveNode__get_FirstSource(n);
            if (option_2 != null) {
                const source = option_2;
                if (!ReactiveNode__get_Disposed(source)) {
                    noteDeadObserver(source);
                }
            }
        }
        const option_3 = ReactiveNode__get_RestSources(n);
        if (option_3 != null) {
            const a = option_3;
            const start = ((0 <= 1) ? 0 : ((0 - 1) | 0)) | 0;
            for (let i_2 = start, limit_2 = ((a.length - 1) | 0); i_2 <= limit_2; i_2++) {
                const source = a[i_2] ?? item(i_2, a);
                if (!ReactiveNode__get_Disposed(source)) {
                    noteDeadObserver(source);
                }
            }
        }
        ReactiveNode__set_FirstSource_Z46457FEC(node_1, undefined);
        ReactiveNode__set_RestSources_6EF2C44D(node_1, undefined);
        ReactiveNode__set_State_Z12CE0414(node_1, 0);
        ReactiveNode__set_Queued_Z1FBCCD16(node_1, false);
        ReactiveNode__set_Fn_4E60E31B(node_1, defaultOf());
        cursor = ReactiveNode__get_NextOwned(node_1);
        ReactiveNode__set_NextOwned_Z46457FEC(node_1, undefined);
    }
}

let ScopeModule_currentScope = undefined;

/**
 * Register a node with the current scope, if any, so it is unlinked when
 * the scope is disposed.
 */
export function ScopeModule_register(node) {
    const option = ScopeModule_currentScope;
    if (option != null) {
        const s = option;
        ReactiveNode__set_NextOwned_Z46457FEC(node, Scope__get_First(s));
        Scope__set_First_Z46457FEC(s, node);
    }
}

/**
 * Register a cleanup callback with the current scope, if any.
 */
export function ScopeModule_onCleanup(fn) {
    const option = ScopeModule_currentScope;
    if (option != null) {
        const s = option;
        void (Scope__EnsureCleanups(s).push(fn));
    }
}

function ScopeModule_compact(parent, children) {
    let w = 0;
    for (let readIdx = 0, limit = ((children.length - 1) | 0); readIdx <= limit; readIdx++) {
        const child = children[readIdx] ?? item(readIdx, children);
        if (!Scope__get_Disposed(child)) {
            setItem(children, w, child);
            w = ((w + 1) | 0);
        }
    }
    while (children.length > w) {
        children.splice((children.length - 1) | 0, 1);
    }
    Scope__set_CompactAt_Z524259A4(parent, max(8, Math.imul(children.length, 2)));
}

/**
 * Tear a scope down. Idempotent.
 */
export function ScopeModule_dispose(scope) {
    disposeSafe(scope);
}

/**
 * Run `fn` inside a fresh scope nested under the current one. Returns its
 * result and a disposer that tears the scope down.
 */
export function ScopeModule_root(fn) {
    const prev = ScopeModule_currentScope;
    const s = Scope_$ctor();
    const option = prev;
    if (option != null) {
        const p = option;
        const children = Scope__EnsureChildren(p);
        if (children.length >= Scope__get_CompactAt(p)) {
            ScopeModule_compact(p, children);
        }
        void (children.push(s));
    }
    ScopeModule_currentScope = s;
    try {
        return [fn(), s];
    }
    finally {
        ScopeModule_currentScope = prev;
    }
}

