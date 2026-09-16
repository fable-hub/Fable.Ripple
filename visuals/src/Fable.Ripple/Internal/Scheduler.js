
import { ReactiveNode__get_RestObservers, ReactiveNode__get_FirstObserver, ReactiveNode__set_Queued_Z1FBCCD16, ReactiveNode__get_Queued, ReactiveNode__get_IsEffect, ReactiveNode__set_State_Z12CE0414, ReactiveNode__get_State } from "../ReactiveNode.js";
import { item } from "../../../fable_modules/fable-library-js.5.13.0/Array.js";
import { clear } from "../../../fable_modules/fable-library-js.5.13.0/Util.js";
import { updateIfNecessary } from "./Tracking.js";

const pending = [];

let batchDepth = 0;

let flushing = false;

function stale(node, target) {
    if (ReactiveNode__get_State(node) < target) {
        const wasClean = ReactiveNode__get_State(node) === 0;
        ReactiveNode__set_State_Z12CE0414(node, target);
        if (ReactiveNode__get_IsEffect(node) && !ReactiveNode__get_Queued(node)) {
            ReactiveNode__set_Queued_Z1FBCCD16(node, true);
            void (pending.push(node));
        }
        if (wasClean) {
            const n = node;
            const option = ReactiveNode__get_FirstObserver(n);
            if (option != null) {
                stale(option, 1);
            }
            const option_1 = ReactiveNode__get_RestObservers(n);
            if (option_1 != null) {
                const a = option_1;
                for (let i = 0; i <= (a.length - 1); i++) {
                    stale(item(i, a), 1);
                }
            }
        }
    }
}

export function flush() {
    if (!flushing) {
        flushing = true;
        try {
            let i = 0;
            while (i < pending.length) {
                const e = item(i, pending);
                i = ((i + 1) | 0);
                ReactiveNode__set_Queued_Z1FBCCD16(e, false);
                if (ReactiveNode__get_State(e) !== 0) {
                    updateIfNecessary(e);
                }
            }
        }
        finally {
            for (let j = 0; j <= (pending.length - 1); j++) {
                ReactiveNode__set_Queued_Z1FBCCD16(item(j, pending), false);
            }
            clear(pending);
            flushing = false;
        }
    }
}

/**
 * A source's value changed: mark observers and flush unless batching.
 */
export function notifyChange(source) {
    const n = source;
    const option = ReactiveNode__get_FirstObserver(n);
    if (option != null) {
        stale(option, 2);
    }
    const option_1 = ReactiveNode__get_RestObservers(n);
    if (option_1 != null) {
        const a = option_1;
        for (let i = 0; i <= (a.length - 1); i++) {
            stale(item(i, a), 2);
        }
    }
    if (batchDepth === 0) {
        flush();
    }
}

export function batch(fn) {
    batchDepth = ((batchDepth + 1) | 0);
    try {
        fn();
    }
    finally {
        batchDepth = ((batchDepth - 1) | 0);
        if (batchDepth === 0) {
            flush();
        }
    }
}

