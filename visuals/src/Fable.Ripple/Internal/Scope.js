
import { class_type } from "../../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { item } from "../../../fable_modules/fable-library-js.5.13.0/Array.js";
import { clear } from "../../../fable_modules/fable-library-js.5.13.0/Util.js";
import { ReactiveNode__set_Queued_Z1FBCCD16, ReactiveNode__set_State_Z12CE0414, ReactiveNode__set_FirstSource_Z46457FEC, ReactiveNode__get_RestSources, ReactiveNode__set_Affected_Z1FBCCD16, ReactiveNode__get_Affected, ReactiveNode__get_Disposed, ReactiveNode__get_FirstSource, ReactiveNode__set_Disposed_Z1FBCCD16 } from "../ReactiveNode.js";
import { compactObservers } from "./Graph.js";

/**
 * Everything created inside one dynamic region - a component, a list row - so
 * it can be torn down as a unit. No weak references: disposal is explicit and deterministic.
 */
export class Scope {
    constructor() {
        this["Nodes@"] = [];
        this["Cleanups@"] = undefined;
        this["Children@"] = undefined;
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
export function Scope__get_Nodes(__) {
    return __["Nodes@"];
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

let ScopeModule_currentScope = undefined;

/**
 * Register a node with the current scope, if any, so it is unlinked when
 * the scope is disposed.
 */
export function ScopeModule_register(node) {
    const option = ScopeModule_currentScope;
    if (option != null) {
        const s = option;
        void (Scope__get_Nodes(s).push(node));
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

/**
 * Tear down a scope: dispose child scopes, run cleanups, then unlink every
 * registered computed/effect from its sources. To avoid O(n^2) when many
 * nodes share one external source, mark the whole scope disposed first and
 * compact each affected source's observer list in a single pass.
 */
export function ScopeModule_dispose(scope) {
    const option = Scope__get_Children(scope);
    if (option != null) {
        const children = option;
        for (let i = 0; i <= (children.length - 1); i++) {
            ScopeModule_dispose(item(i, children));
        }
        clear(children);
    }
    const option_1 = Scope__get_Cleanups(scope);
    if (option_1 != null) {
        const cleanups = option_1;
        for (let i_1 = 0; i_1 <= (cleanups.length - 1); i_1++) {
            item(i_1, cleanups)();
        }
        clear(cleanups);
    }
    const nodes = Scope__get_Nodes(scope);
    for (let i_2 = 0; i_2 <= (nodes.length - 1); i_2++) {
        ReactiveNode__set_Disposed_Z1FBCCD16(item(i_2, nodes), true);
    }
    const affected = [];
    for (let i_3 = 0; i_3 <= (nodes.length - 1); i_3++) {
        const node = item(i_3, nodes);
        const n = node;
        if (0 <= 0) {
            const option_2 = ReactiveNode__get_FirstSource(n);
            if (option_2 != null) {
                const source = option_2;
                if (!ReactiveNode__get_Disposed(source) && !ReactiveNode__get_Affected(source)) {
                    ReactiveNode__set_Affected_Z1FBCCD16(source, true);
                    void (affected.push(source));
                }
            }
        }
        const option_3 = ReactiveNode__get_RestSources(n);
        if (option_3 != null) {
            const a = option_3;
            const start = ((0 <= 1) ? 0 : (0 - 1)) | 0;
            for (let i_4 = start; i_4 <= (a.length - 1); i_4++) {
                const source = item(i_4, a);
                if (!ReactiveNode__get_Disposed(source) && !ReactiveNode__get_Affected(source)) {
                    ReactiveNode__set_Affected_Z1FBCCD16(source, true);
                    void (affected.push(source));
                }
            }
        }
        ReactiveNode__set_FirstSource_Z46457FEC(node, undefined);
        const option_4 = ReactiveNode__get_RestSources(node);
        if (option_4 != null) {
            clear(option_4);
        }
        ReactiveNode__set_State_Z12CE0414(node, 0);
        ReactiveNode__set_Queued_Z1FBCCD16(node, false);
    }
    for (let i_5 = 0; i_5 <= (affected.length - 1); i_5++) {
        const source_1 = item(i_5, affected);
        compactObservers(source_1, (o) => !ReactiveNode__get_Disposed(o));
        ReactiveNode__set_Affected_Z1FBCCD16(source_1, false);
    }
    clear(nodes);
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
        void (Scope__EnsureChildren(p).push(s));
    }
    ScopeModule_currentScope = s;
    try {
        return [fn(), {
            Dispose() {
                ScopeModule_dispose(s);
            },
        }];
    }
    finally {
        ScopeModule_currentScope = prev;
    }
}

