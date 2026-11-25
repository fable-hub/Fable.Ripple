
import { item } from "../../../fable_modules/fable-library-js.5.28.0/Array.js";
import { truncateSources, unlinkSourcesTail, addObserver, addSource, sourceCount, sourceAt } from "./Graph.js";
import { ReactiveNode__get_RestObservers, ReactiveNode__get_FirstObserver, ReactiveNode__get_Flags, ReactiveNode__set_Flags_Z524259A4, ReactiveNode__get_Context, Context_current, Context_get, ReactiveNode__set_State_Z12CE0414, ReactiveNode__get_State, ReactiveNode__get_Disposed } from "../ReactiveNode.js";

let current = undefined;

let currentGets = undefined;

let currentGetsIndex = 0;

/**
 * Record that `current` reads `node`. While reads arrive in the same order
 * as the previous run, just advance the index (no edge mutation); on the
 * first mismatch, collect the new tail into `currentGets`. A read of the
 * source just recorded adds no edge.
 */
export function track(node) {
    if (current != null) {
        const cur = current;
        if (currentGets != null) {
            const gets = currentGets;
            const last = (gets.length > 0) ? item((gets.length - 1) | 0, gets) : ((currentGetsIndex > 0) ? sourceAt(cur, (currentGetsIndex - 1) | 0) : undefined);
            let matchResult;
            if (last != null) {
                if (last === node) {
                    matchResult = 0;
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
            switch (matchResult) {
                case 0: {
                    break;
                }
                case 1: {
                    void (gets.push(node));
                    break;
                }
            }
        }
        else if ((currentGetsIndex > 0) && (sourceAt(cur, (currentGetsIndex - 1) | 0) === node)) {
        }
        else if (currentGetsIndex === sourceCount(cur)) {
            addSource(cur, node);
            if (!ReactiveNode__get_Disposed(node)) {
                addObserver(node, cur);
            }
            currentGetsIndex = ((currentGetsIndex + 1) | 0);
        }
        else if (sourceAt(cur, currentGetsIndex) === node) {
            currentGetsIndex = ((currentGetsIndex + 1) | 0);
        }
        else {
            const g = [];
            currentGets = g;
            void (g.push(node));
        }
    }
}

/**
 * Bring `node` up to date, recomputing only if a dependency truly changed.
 */
export function updateIfNecessary(node) {
    if (ReactiveNode__get_State(node) === 1) {
        const count = sourceCount(node) | 0;
        let i = 0;
        while ((ReactiveNode__get_State(node) === 1) && (i < count)) {
            updateIfNecessary(sourceAt(node, i));
            i = ((i + 1) | 0);
        }
    }
    if (ReactiveNode__get_State(node) === 2) {
        recompute(node);
    }
    ReactiveNode__set_State_Z12CE0414(node, 0);
}

function recompute(node) {
    let flags;
    const prevCurrent = current;
    const prevGets = currentGets;
    const prevIndex = currentGetsIndex | 0;
    const prevContext = Context_get();
    current = node;
    currentGets = undefined;
    currentGetsIndex = 0;
    Context_current(ReactiveNode__get_Context(node));
    ReactiveNode__set_Flags_Z524259A4(node, (flags = (ReactiveNode__get_Flags(node) | 0), (flags & 255) | ((flags + 256) & 1073741568)));
    const startCount = sourceCount(node) | 0;
    let changed = false;
    let completed = false;
    try {
        changed = node.Recompute();
        if (currentGets == null) {
            if (sourceCount(node) > currentGetsIndex) {
                unlinkSourcesTail(node, currentGetsIndex);
                truncateSources(node, currentGetsIndex);
            }
        }
        else {
            const gets = currentGets;
            unlinkSourcesTail(node, currentGetsIndex);
            truncateSources(node, currentGetsIndex);
            for (let i = 0, limit = ((gets.length - 1) | 0); i <= limit; i++) {
                const s = gets[i] ?? item(i, gets);
                addSource(node, s);
                if (!ReactiveNode__get_Disposed(s)) {
                    addObserver(s, node);
                }
            }
        }
        completed = true;
    }
    finally {
        if (!completed && (sourceCount(node) > startCount)) {
            unlinkSourcesTail(node, startCount);
            truncateSources(node, startCount);
        }
        current = prevCurrent;
        currentGets = prevGets;
        currentGetsIndex = (prevIndex | 0);
        Context_current(prevContext);
    }
    if (changed) {
        const n = node;
        const option = ReactiveNode__get_FirstObserver(n);
        if (option != null) {
            const o_1 = option;
            if (!ReactiveNode__get_Disposed(o_1)) {
                const o = o_1;
                if (ReactiveNode__get_State(o) < 2) {
                    ReactiveNode__set_State_Z12CE0414(o, 2);
                }
            }
        }
        const option_1 = ReactiveNode__get_RestObservers(n);
        if (option_1 != null) {
            const a = option_1;
            for (let i_1 = 0, limit_1 = ((a.length - 1) | 0); i_1 <= limit_1; i_1++) {
                const o_1_1 = a[i_1] ?? item(i_1, a);
                if (!ReactiveNode__get_Disposed(o_1_1)) {
                    const o = o_1_1;
                    if (ReactiveNode__get_State(o) < 2) {
                        ReactiveNode__set_State_Z12CE0414(o, 2);
                    }
                }
            }
        }
    }
}

/**
 * The node whose reads are being tracked, if any.
 */
export function observer() {
    return current;
}

export function untracked(fn) {
    const prev = current;
    current = undefined;
    try {
        return fn();
    }
    finally {
        current = prev;
    }
}

