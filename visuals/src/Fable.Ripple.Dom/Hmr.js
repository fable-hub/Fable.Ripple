
import { substring } from "../../fable_modules/fable-library-js.5.18.0/String.js";
import { FSharpRef, Record } from "../../fable_modules/fable-library-js.5.18.0/Types.js";
import { record_type, class_type, int32_type, string_type } from "../../fable_modules/fable-library-js.5.18.0/Reflection.js";
import { addToSet, tryGetValue } from "../../fable_modules/fable-library-js.5.18.0/MapUtil.js";
import { disposeSafe, getEnumerator, defaultOf, int32ToString } from "../../fable_modules/fable-library-js.5.18.0/Util.js";
import { filter, toArray } from "../../fable_modules/fable-library-js.5.18.0/Seq.js";
import { item as item_3 } from "../../fable_modules/fable-library-js.5.18.0/Array.js";
import { Var$1__set_Value_2B595, Var$1__get_Value, Var$1__Peek, Var$1 } from "../Fable.Ripple/Types.js";
import { empty, cons, tail, head, isEmpty } from "../../fable_modules/fable-library-js.5.18.0/List.js";
import { value as value_5, ofNullable } from "../../fable_modules/fable-library-js.5.18.0/Option.js";
import { Base_replaceTrackedNode, Base_Recording_dependOn, EmptyMarker, Base_applyItemRecording, Base_Recording_active } from "./Base.js";
import { Operators_IsNull } from "../../fable_modules/fable-library-js.5.18.0/FSharp.Core.js";
import { Var_create, Signal_onCleanup, Signal_autorun, Signal_untracked, Signal_root } from "../Fable.Ripple/Api.js";

function key(url, name) {
    let matchValue;
    return (((matchValue = (url.indexOf("?") | 0), (matchValue === -1) ? url : substring(url, 0, matchValue))) + "#") + name;
}

const stash = new Map([]);

class Frame extends Record {
    constructor(Path, Counts, Seen) {
        super();
        this.Path = Path;
        this.Counts = Counts;
        this.Seen = Seen;
    }
}

function Frame_$reflection() {
    return record_type("Fable.Ripple.Dom.Hmr.Frame", [], Frame, () => [["Path", string_type], ["Counts", class_type("System.Collections.Generic.Dictionary`2", [string_type, int32_type])], ["Seen", class_type("System.Collections.Generic.HashSet`1", [string_type])]]);
}

function newFrame(path) {
    return new Frame(path, new Map([]), new Set([]));
}

let frame = newFrame("");

function place(k) {
    let n_1;
    let matchValue;
    let outArg = 0;
    matchValue = [tryGetValue(frame.Counts, k, new FSharpRef(() => (outArg | 0), (v) => {
        outArg = (v | 0);
    })), outArg];
    n_1 = (matchValue[0] ? matchValue[1] : 0);
    frame.Counts.set(k, n_1 + 1);
    return (((frame.Path + "/") + k) + "#") + int32ToString(n_1);
}

function enter(path) {
    const saved = frame;
    frame = newFrame(path);
    return saved;
}

function leave(saved) {
    let source, f1, objectArg;
    let matchValue;
    let outArg = defaultOf();
    matchValue = [tryGetValue(stash, frame.Path, new FSharpRef(() => outArg, (v) => {
        outArg = v;
    })), outArg];
    if (matchValue[0]) {
        const bag = matchValue[1];
        const stale = toArray((source = bag.keys(), filter((f1 = ((objectArg = frame.Seen, (item) => objectArg.has(item))), (arg) => !f1(arg)), source)));
        for (let idx = 0; idx <= (stale.length - 1); idx++) {
            const name = item_3(idx, stale);
            bag.delete(name);
        }
    }
    frame = saved;
}

/**
 * Keep a `Var` across a swap, keyed by the name it was bound to.
 * 
 * `create` runs only the first time. A type change between versions falls back to
 * a fresh Var rather than throwing.
 */
export function adopt(name, create) {
    addToSet(name, frame.Seen);
    let bag_2;
    let matchValue;
    let outArg = defaultOf();
    matchValue = [tryGetValue(stash, frame.Path, new FSharpRef(() => outArg, (v) => {
        outArg = v;
    })), outArg];
    if (matchValue[0]) {
        bag_2 = matchValue[1];
    }
    else {
        const bag_1 = new Map([]);
        stash.set(frame.Path, bag_1);
        bag_2 = bag_1;
    }
    let matchValue_1;
    let outArg_1 = defaultOf();
    matchValue_1 = [tryGetValue(bag_2, name, new FSharpRef(() => outArg_1, (v_1) => {
        outArg_1 = v_1;
    })), outArg_1];
    if (matchValue_1[0]) {
        const v_2 = matchValue_1[1];
        if (v_2 instanceof Var$1) {
            const typed = v_2;
            return typed;
        }
        else {
            const fresh = create();
            bag_2.set(name, fresh);
            return fresh;
        }
    }
    else {
        const fresh_1 = create();
        bag_2.set(name, fresh_1);
        return fresh_1;
    }
}

export function splice(run) {
    return (f => (f.__splice = true, f))((p, a) => run(p, a));
}

export function fragment(items) {
    return splice((parent, anchor) => {
        let rest = items;
        while (!isEmpty(rest)) {
            const matchValue = head(rest);
            if (matchValue instanceof Node) {
                parent.insertBefore(matchValue, ofNullable(anchor));
            }
            else {
                const item = matchValue;
                if (Base_Recording_active()) {
                    Base_applyItemRecording(parent, item);
                }
                else {
                    const element = parent;
                    const item_1 = item;
                    if (item_1 instanceof Node) {
                        element.appendChild(item_1);
                    }
                    else if (item_1 instanceof EmptyMarker) {
                    }
                    else {
                        item_1(element);
                    }
                }
            }
            rest = tail(rest);
        }
    });
}

function capture(root) {
    let active;
    const pathTo = (n_mut, acc_mut) => {
        pathTo:
        while (true) {
            const n = n_mut, acc = acc_mut;
            if (n === root) {
                return acc;
            }
            else {
                const matchValue = n.parentNode;
                if (matchValue != null) {
                    const p = matchValue;
                    let i = 0;
                    let c = p.firstChild;
                    while ((c != null) && !(value_5(c) === n)) {
                        i = ((i + 1) | 0);
                        c = value_5(c).nextSibling;
                    }
                    const arg_1 = cons(i, acc);
                    n_mut = p;
                    acc_mut = arg_1;
                    continue pathTo;
                }
                else {
                    return undefined;
                }
            }
            break;
        }
    };
    const matchValue_1 = document.activeElement;
    let matchResult, active_1;
    if (matchValue_1 != null) {
        if ((active = matchValue_1, !Operators_IsNull(root) && root.contains(active))) {
            matchResult = 0;
            active_1 = matchValue_1;
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
            const option_1 = pathTo(active_1, empty());
            if (option_1 != null) {
                return [option_1, active_1.selectionStart, active_1.selectionEnd];
            }
            else {
                return undefined;
            }
        }
        default:
            return undefined;
    }
}

function restore(root, captured) {
    const option_1 = captured;
    if (option_1 != null) {
        const tupledArg = option_1;
        const selStart = tupledArg[1];
        let node = root;
        let ok = !Operators_IsNull(root);
        const enumerator = getEnumerator(tupledArg[0]);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const i = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]() | 0;
                if (ok) {
                    const children = node.childNodes;
                    if (i < children.length) {
                        node = children.item(i);
                    }
                    else {
                        ok = false;
                    }
                }
            }
        }
        finally {
            disposeSafe(enumerator);
        }
        if (ok && !Operators_IsNull(node)) {
            const el = node;
            if (!Operators_IsNull(el.focus)) {
                el.focus();
                if (!Operators_IsNull(selStart)) {
                    try {
                        el.setSelectionRange(selStart, tupledArg[2]);
                    }
                    catch (matchValue) {
                    }
                }
            }
        }
    }
}

function boundaryOf(k, impl, args) {
    const path = place(k);
    if (Base_Recording_active()) {
        Base_Recording_dependOn(impl);
        const saved = enter(path);
        const item = Var$1__Peek(impl).apply(null, args);
        leave(saved);
        return item;
    }
    else {
        const build = (f) => Signal_root(() => {
            const saved_1 = enter(path);
            const item_1 = Signal_untracked(() => (f.apply(null, args)));
            leave(saved_1);
            return item_1;
        });
        const patternInput = build(Var$1__get_Value(impl));
        const firstItem = patternInput[0];
        const firstDispose = patternInput[1];
        if (firstItem instanceof Node) {
            let current = firstItem;
            let dispose = firstDispose;
            let built = true;
            Signal_autorun(() => {
                const f_1 = Var$1__get_Value(impl);
                if (built) {
                    built = false;
                }
                else {
                    globalThis.__rebuilds = (globalThis.__rebuilds || 0) + 1;
                    const t0 = performance.now();
                    const parent = current.parentNode;
                    const captured = capture(current);
                    disposeSafe(dispose);
                    const patternInput_1 = build(f_1);
                    const next = patternInput_1[0];
                    dispose = patternInput_1[1];
                    if (next instanceof Node) {
                        const fresh = next;
                        const option_1 = parent;
                        if (option_1 != null) {
                            const parent_1 = option_1;
                            parent_1.replaceChild(fresh, current);
                            Base_replaceTrackedNode(current, fresh);
                        }
                        current = fresh;
                        restore(current, captured);
                    }
                    (globalThis.__rebuildMs = globalThis.__rebuildMs || []).push((performance.now()) - t0);
                }
            });
            Signal_onCleanup(() => {
                disposeSafe(dispose);
            });
            return current;
        }
        else {
            disposeSafe(firstDispose);
            return (parent_2) => {
                const startA = document.createComment("b[");
                const endA = document.createComment("]b");
                parent_2.appendChild(startA);
                parent_2.appendChild(endA);
                let current_1 = undefined;
                const clear = () => {
                    const option_3 = current_1;
                    if (option_3 != null) {
                        disposeSafe(option_3);
                    }
                    current_1 = undefined;
                    let n = startA.nextSibling;
                    while ((n != null) && !(value_5(n) === endA)) {
                        const next_1 = value_5(n).nextSibling;
                        parent_2.removeChild(value_5(n));
                        n = next_1;
                    }
                };
                Signal_autorun(() => {
                    const f_2 = Var$1__get_Value(impl);
                    globalThis.__rebuilds = (globalThis.__rebuilds || 0) + 1;
                    const captured_1 = capture(parent_2);
                    const t0_1 = performance.now();
                    clear();
                    const patternInput_2 = build(f_2);
                    const item_2 = patternInput_2[0];
                    if (item_2 instanceof EmptyMarker) {
                    }
                    else if (typeof item_2 === "function") {
                        if (item_2 != null && item_2.__splice === true) {
                            item_2(parent_2, endA);
                        }
                        else {
                            item_2(parent_2);
                        }
                    }
                    else {
                        parent_2.insertBefore(item_2, endA);
                    }
                    current_1 = patternInput_2[1];
                    restore(parent_2, captured_1);
                    (globalThis.__rebuildMs = globalThis.__rebuildMs || []).push((performance.now()) - t0_1);
                });
                Signal_onCleanup(clear);
            };
        }
    }
}

export function define(url, name, fn) {
    const k = key(url, name);
    let impl;
    let matchValue;
    let outArg = defaultOf();
    matchValue = [tryGetValue((globalThis.__RIPPLE_HMR__ || (globalThis.__RIPPLE_HMR__ = new Map())), k, new FSharpRef(() => outArg, (v) => {
        outArg = v;
    })), outArg];
    if (matchValue[0]) {
        const v_1 = matchValue[1];
        Var$1__set_Value_2B595(v_1, fn);
        impl = v_1;
    }
    else {
        const v_2 = Var_create(fn);
        ((globalThis.__RIPPLE_HMR__ || (globalThis.__RIPPLE_HMR__ = new Map()))).set(k, v_2);
        impl = v_2;
    }
    return (...a) => (boundaryOf)(k, impl, a);
}

