
import { ReactiveNode__get_Flags, ReactiveNode__set_Disposed_Z1FBCCD16, ReactiveNode__set_Queued_Z1FBCCD16, ReactiveNode__set_State_Z12CE0414, Owner_disposeOwned, ReactiveNode__set_Affected_Z1FBCCD16, ReactiveNode__get_Affected, ReactiveNode__get_Disposed, ReactiveNode__set_DeadObservers_Z524259A4, ReactiveNode__set_FirstObserver_Z46457FEC, ReactiveNode__set_RestObservers_6EF2C44D, ReactiveNode__get_DeadObservers, ReactiveNode__get_RestObservers, ReactiveNode__get_FirstObserver, ReactiveNode__set_FirstSource_Z46457FEC, ReactiveNode__set_RestSources_6EF2C44D, ReactiveNode__get_RestSources, ReactiveNode__get_FirstSource } from "../ReactiveNode.js";
import { value } from "../../../fable_modules/fable-library-js.5.28.0/Option.js";
import { setItem, item } from "../../../fable_modules/fable-library-js.5.28.0/Array.js";
import { clear } from "../../../fable_modules/fable-library-js.5.28.0/Util.js";

export function sourceCount(n) {
    const matchValue = ReactiveNode__get_FirstSource(n);
    if (matchValue != null) {
        const matchValue_1 = ReactiveNode__get_RestSources(n);
        if (matchValue_1 == null) {
            return 1;
        }
        else {
            const a = matchValue_1;
            return (1 + a.length) | 0;
        }
    }
    else {
        return 0;
    }
}

/**
 * Logical source `i` (caller guarantees `0 <= i < sourceCount n`).
 */
export function sourceAt(n, i) {
    let copyOfStruct_1;
    if (i === 0) {
        let copyOfStruct = ReactiveNode__get_FirstSource(n);
        return value(copyOfStruct);
    }
    else {
        return item((i - 1) | 0, (copyOfStruct_1 = ReactiveNode__get_RestSources(n), value(copyOfStruct_1)));
    }
}

function ensureRestSources(n) {
    const matchValue = ReactiveNode__get_RestSources(n);
    if (matchValue == null) {
        const a_1 = [];
        ReactiveNode__set_RestSources_6EF2C44D(n, a_1);
        return a_1;
    }
    else {
        return matchValue;
    }
}

export function addSource(n, s) {
    const matchValue = ReactiveNode__get_FirstSource(n);
    if (matchValue != null) {
        void (ensureRestSources(n).push(s));
    }
    else {
        ReactiveNode__set_FirstSource_Z46457FEC(n, s);
    }
}

/**
 * Keep the first `len` logical sources, drop the rest (keeps the overflow
 * array allocated but empty, so a stable regrow reuses it).
 */
export function truncateSources(n, len) {
    if (len <= 0) {
        ReactiveNode__set_FirstSource_Z46457FEC(n, undefined);
        const option = ReactiveNode__get_RestSources(n);
        if (option != null) {
            clear(option);
        }
    }
    else {
        const option_1 = ReactiveNode__get_RestSources(n);
        if (option_1 != null) {
            const a_1 = option_1;
            while (a_1.length > ((len - 1) | 0)) {
                a_1.splice((a_1.length - 1) | 0, 1);
            }
        }
    }
}

function observerSlots(n) {
    const matchValue = ReactiveNode__get_FirstObserver(n);
    if (matchValue != null) {
        const matchValue_1 = ReactiveNode__get_RestObservers(n);
        if (matchValue_1 == null) {
            return 1;
        }
        else {
            const a = matchValue_1;
            return (1 + a.length) | 0;
        }
    }
    else {
        return 0;
    }
}

/**
 * Live observers of `n` (disposed entries awaiting a sweep are not counted).
 */
export function observerCount(n) {
    return (observerSlots(n) - ReactiveNode__get_DeadObservers(n)) | 0;
}

function ensureRestObservers(n) {
    const matchValue = ReactiveNode__get_RestObservers(n);
    if (matchValue == null) {
        const a_1 = [];
        ReactiveNode__set_RestObservers_6EF2C44D(n, a_1);
        return a_1;
    }
    else {
        return matchValue;
    }
}

export function addObserver(n, o) {
    const matchValue = ReactiveNode__get_FirstObserver(n);
    if (matchValue != null) {
        void (ensureRestObservers(n).push(o));
    }
    else {
        ReactiveNode__set_FirstObserver_Z46457FEC(n, o);
    }
}

/**
 * Swap-remove `o` from `n`'s observers (order is irrelevant).
 */
export function removeObserver(n, o) {
    let a;
    const matchValue = ReactiveNode__get_FirstObserver(n);
    if (matchValue == null) {
    }
    else if (matchValue === o) {
        const matchValue_1 = ReactiveNode__get_RestObservers(n);
        let matchResult;
        if (matchValue_1 != null) {
            if ((a = matchValue_1, a.length > 0)) {
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
                const a_1 = matchValue_1;
                ReactiveNode__set_FirstObserver_Z46457FEC(n, item((a_1.length - 1) | 0, a_1));
                a_1.splice((a_1.length - 1) | 0, 1);
                break;
            }
            case 1: {
                ReactiveNode__set_FirstObserver_Z46457FEC(n, undefined);
                break;
            }
        }
    }
    else {
        const option = ReactiveNode__get_RestObservers(n);
        if (option != null) {
            const a_2 = option;
            let i = 0;
            let go = true;
            while (go && (i < a_2.length)) {
                if ((a_2[i] ?? item(i, a_2)) === o) {
                    setItem(a_2, i, item((a_2.length - 1) | 0, a_2));
                    a_2.splice((a_2.length - 1) | 0, 1);
                    go = false;
                }
                else {
                    i = ((i + 1) | 0);
                }
            }
        }
    }
}

/**
 * Keep only the observers of `n` satisfying `keep`, compacting in place with
 * no allocation: the first kept edge fills `FirstObserver` (promoting one out
 * of the overflow if needed), the rest are compacted into `RestObservers` by a
 * write cursor. `w <= readIdx` always, so writes never clobber unread slots.
 */
export function compactObservers(n, keep) {
    const matchValue = ReactiveNode__get_FirstObserver(n);
    if (matchValue != null) {
        const f0 = matchValue;
        let newFirst = keep(f0) ? f0 : undefined;
        const option = ReactiveNode__get_RestObservers(n);
        if (option != null) {
            const rest = option;
            let w = 0;
            for (let readIdx = 0, limit = ((rest.length - 1) | 0); readIdx <= limit; readIdx++) {
                const o = rest[readIdx] ?? item(readIdx, rest);
                if (keep(o)) {
                    if (newFirst != null) {
                        setItem(rest, w, o);
                        w = ((w + 1) | 0);
                    }
                    else {
                        newFirst = o;
                    }
                }
            }
            while (rest.length > w) {
                rest.splice((rest.length - 1) | 0, 1);
            }
        }
        ReactiveNode__set_FirstObserver_Z46457FEC(n, newFirst);
    }
}

function sweepDeadObservers(n) {
    const dead = ReactiveNode__get_DeadObservers(n) | 0;
    if (dead > 0) {
        const slots = observerSlots(n) | 0;
        if (dead >= slots) {
            ReactiveNode__set_FirstObserver_Z46457FEC(n, undefined);
            ReactiveNode__set_RestObservers_6EF2C44D(n, undefined);
            ReactiveNode__set_DeadObservers_Z524259A4(n, 0);
        }
        else if (Math.imul(2, dead) >= slots) {
            compactObservers(n, (o) => !ReactiveNode__get_Disposed(o));
            ReactiveNode__set_DeadObservers_Z524259A4(n, 0);
        }
    }
}

const sweepQueue = [];

let sweepHolds = 0;

export const readerLists = [];

let deadReaders = 0;

let sweptReaders = 0;

export function noteDeadReader() {
    deadReaders = ((deadReaders + 1) | 0);
}

export function deadReaderCount() {
    return deadReaders | 0;
}

export function removeReaderList(sweep) {
    const i = (readerLists.indexOf(sweep)) | 0;
    if (i >= 0) {
        readerLists.splice(i, 1);
    }
}

export function noteDeadObserver(source) {
    ReactiveNode__set_DeadObservers_Z524259A4(source, (ReactiveNode__get_DeadObservers(source) + 1) | 0);
    if (!ReactiveNode__get_Affected(source)) {
        ReactiveNode__set_Affected_Z1FBCCD16(source, true);
        void (sweepQueue.push(source));
    }
}

/**
 * Nests; the outermost `releaseSweeps` runs the queued checks.
 */
export function holdSweeps() {
    sweepHolds = ((sweepHolds + 1) | 0);
}

/**
 * Release a hold; the last one runs the queued sweep checks.
 */
export function releaseSweeps() {
    sweepHolds = ((sweepHolds - 1) | 0);
    if ((sweepHolds === 0) && (sweepQueue.length > 0)) {
        for (let i = 0, limit = ((sweepQueue.length - 1) | 0); i <= limit; i++) {
            const source = sweepQueue[i] ?? item(i, sweepQueue);
            ReactiveNode__set_Affected_Z1FBCCD16(source, false);
            sweepDeadObservers(source);
        }
        clear(sweepQueue);
    }
    if ((sweepHolds === 0) && (deadReaders !== sweptReaders)) {
        sweptReaders = (deadReaders | 0);
        for (let i_1 = 0, limit_1 = ((readerLists.length - 1) | 0); i_1 <= limit_1; i_1++) {
            (readerLists[i_1] ?? item(i_1, readerLists))(deadReaders);
        }
    }
}

/**
 * Unlink `node` from each of its sources at or after `fromIndex`.
 */
export function unlinkSourcesTail(node, fromIndex) {
    const n = node;
    const fromIdx = fromIndex | 0;
    if (fromIdx <= 0) {
        const option = ReactiveNode__get_FirstSource(n);
        if (option != null) {
            removeObserver(option, node);
        }
    }
    const option_1 = ReactiveNode__get_RestSources(n);
    if (option_1 != null) {
        const a = option_1;
        const start = ((fromIdx <= 1) ? 0 : ((fromIdx - 1) | 0)) | 0;
        for (let i = start, limit = ((a.length - 1) | 0); i <= limit; i++) {
            removeObserver(a[i] ?? item(i, a), node);
        }
    }
}

/**
 * Unlink a node from the graph entirely (used to dispose subscriptions).
 */
export function dispose(node) {
    Owner_disposeOwned(node);
    const n = node;
    if (0 <= 0) {
        const option = ReactiveNode__get_FirstSource(n);
        if (option != null) {
            removeObserver(option, node);
        }
    }
    const option_1 = ReactiveNode__get_RestSources(n);
    if (option_1 != null) {
        const a = option_1;
        const start = ((0 <= 1) ? 0 : ((0 - 1) | 0)) | 0;
        for (let i = start, limit = ((a.length - 1) | 0); i <= limit; i++) {
            removeObserver(a[i] ?? item(i, a), node);
        }
    }
    ReactiveNode__set_FirstSource_Z46457FEC(node, undefined);
    const option_2 = ReactiveNode__get_RestSources(node);
    if (option_2 != null) {
        clear(option_2);
    }
    ReactiveNode__set_State_Z12CE0414(node, 0);
    ReactiveNode__set_Queued_Z1FBCCD16(node, false);
    ReactiveNode__set_Disposed_Z1FBCCD16(node, true);
    if ((ReactiveNode__get_Flags(node) & 128) !== 0) {
        noteDeadReader();
    }
}

