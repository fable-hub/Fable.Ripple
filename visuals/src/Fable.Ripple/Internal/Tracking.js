
import { addObserver, addSource, truncateSources, unlinkSourcesTail, sourceAt, sourceCount } from "./Graph.js";
import { ReactiveNode__get_RestObservers, ReactiveNode__get_FirstObserver, ReactiveNode__get_Recompute, ReactiveNode__get_EffectFn, ReactiveNode__set_State_Z12CE0414, ReactiveNode__get_State } from "../ReactiveNode.js";
import { item } from "../../../fable_modules/fable-library-js.5.13.0/Array.js";

let current = undefined;

let currentGets = undefined;

let currentGetsIndex = 0;

/**
 * Record that `current` reads `node`. While reads arrive in the same order
 * as the previous run, just advance the index (no edge mutation); on the
 * first mismatch, collect the new tail into `currentGets`.
 */
export function track(node) {
    if (current != null) {
        const cur = current;
        let matchResult;
        if (currentGets == null) {
            if ((currentGetsIndex < sourceCount(cur)) && (sourceAt(cur, currentGetsIndex) === node)) {
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
                currentGetsIndex = ((currentGetsIndex + 1) | 0);
                break;
            }
            case 1: {
                let gets;
                if (currentGets == null) {
                    const g_1 = [];
                    currentGets = g_1;
                    gets = g_1;
                }
                else {
                    gets = currentGets;
                }
                void (gets.push(node));
                break;
            }
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
    let matchValue;
    const prevCurrent = current;
    const prevGets = currentGets;
    const prevIndex = currentGetsIndex | 0;
    current = node;
    currentGets = undefined;
    currentGetsIndex = 0;
    let changed = false;
    try {
        changed = ((matchValue = ReactiveNode__get_EffectFn(node), (matchValue == null) ? ReactiveNode__get_Recompute(node)() : ((matchValue(), false))));
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
            for (let i = 0; i <= (gets.length - 1); i++) {
                const s = item(i, gets);
                addSource(node, s);
                addObserver(s, node);
            }
        }
    }
    finally {
        current = prevCurrent;
        currentGets = prevGets;
        currentGetsIndex = (prevIndex | 0);
    }
    if (changed) {
        const n = node;
        const option = ReactiveNode__get_FirstObserver(n);
        if (option != null) {
            const o = option;
            if (ReactiveNode__get_State(o) < 2) {
                ReactiveNode__set_State_Z12CE0414(o, 2);
            }
        }
        const option_1 = ReactiveNode__get_RestObservers(n);
        if (option_1 != null) {
            const a = option_1;
            for (let i_1 = 0; i_1 <= (a.length - 1); i_1++) {
                const o = item(i_1, a);
                if (ReactiveNode__get_State(o) < 2) {
                    ReactiveNode__set_State_Z12CE0414(o, 2);
                }
            }
        }
    }
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

