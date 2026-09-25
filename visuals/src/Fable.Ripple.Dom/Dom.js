
import { Signal_onCleanup, Signal_root, Signal_effect } from "../Fable.Ripple/Api.js";
import { FSharpRef, Record } from "../../fable_modules/fable-library-js.5.13.0/Types.js";
import { record_type, int32_type, class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { fill, map, setItem, item as item_1 } from "../../fable_modules/fable-library-js.5.13.0/Array.js";
import { Dictionary } from "../../fable_modules/fable-library-js.5.13.0/MutableMap.js";
import { HashIdentity_Structural } from "../../fable_modules/fable-library-js.5.13.0/FSharp.Collections.js";
import { equals, disposeSafe, defaultOf } from "../../fable_modules/fable-library-js.5.13.0/Util.js";
import { min } from "../../fable_modules/fable-library-js.5.13.0/Double.js";
import { tryGetValue } from "../../fable_modules/fable-library-js.5.13.0/MapUtil.js";

export function el(tag) {
    return document.createElement(tag);
}

/**
 * Static text child.
 */
export function text(parent, s) {
    parent.appendChild(document.createTextNode(s));
}

/**
 * Reactive text child: re-runs only when a signal it reads changes.
 */
export function bindText(parent, f) {
    const t = document.createTextNode("");
    parent.appendChild(t);
    Signal_effect(() => {
        t.nodeValue = f();
    });
}

/**
 * Reactive attribute.
 */
export function bindAttr(e, name, f) {
    Signal_effect(() => {
        e.setAttribute(name, f());
    });
}

class Row extends Record {
    constructor(Node$, Dispose, Seen, Index) {
        super();
        this.Node = Node$;
        this.Dispose = Dispose;
        this.Seen = (Seen | 0);
        this.Index = (Index | 0);
    }
}

function Row_$reflection() {
    return record_type("Fable.Ripple.Dom.Dom.Row", [], Row, () => [["Node", class_type("Browser.Types.HTMLElement", undefined)], ["Dispose", class_type("System.IDisposable")], ["Seen", int32_type], ["Index", int32_type]]);
}

function markLis(src, keep) {
    const n = src.length | 0;
    const tails = new Int32Array(n);
    const prev = new Int32Array(n);
    let len = 0;
    for (let i = 0; i <= (n - 1); i++) {
        const v = item_1(i, src) | 0;
        if (v >= 0) {
            let lo = 0;
            let hi = len;
            while (lo < hi) {
                const mid = ~~((lo + hi) / 2) | 0;
                if (item_1(item_1(mid, tails), src) < v) {
                    lo = ((mid + 1) | 0);
                }
                else {
                    hi = (mid | 0);
                }
            }
            setItem(prev, i, ((lo > 0) ? item_1(lo - 1, tails) : -1) | 0);
            setItem(tails, lo, i | 0);
            if (lo === len) {
                len = ((len + 1) | 0);
            }
        }
    }
    if (len > 0) {
        let k = item_1(len - 1, tails);
        while (k >= 0) {
            setItem(keep, k, true);
            k = (item_1(k, prev) | 0);
        }
    }
}

/**
 * Render `items` into `parent` (rows placed before `anchor`, which may be
 * null to append). Each row's `render` runs in its own `Signal.root`, disposed
 * when the row leaves or the enclosing scope tears down.
 */
export function keyedEach(parent, anchor, getItems, keyOf, render) {
    const byKey = new Dictionary([], HashIdentity_Structural());
    let indexed = false;
    let order = [];
    let rows = [];
    let generation = 0;
    const make = (item, index) => {
        const patternInput = Signal_root(() => render(item));
        return new Row(patternInput[0], patternInput[1], 0, index);
    };
    const reindex = () => {
        if (!indexed) {
            for (let i = 0; i <= (order.length - 1); i++) {
                byKey.set(item_1(i, order), item_1(i, rows));
            }
            indexed = true;
        }
    };
    const removeAllRowNodes = () => {
        const hasAnchor = !(anchor === defaultOf());
        const owned = (hasAnchor ? (order.length + 1) : order.length) | 0;
        if (parent.childNodes.length === owned) {
            parent.textContent = "";
            if (hasAnchor) {
                parent.appendChild(anchor);
            }
        }
        else {
            for (let i_3 = 0; i_3 <= (rows.length - 1); i_3++) {
                parent.removeChild(item_1(i_3, rows).Node);
            }
        }
    };
    const sub = Signal_effect(() => {
        let newKeys, ok, i_1, newKeys_1, dj, i_2;
        const items = getItems();
        const n = items.length | 0;
        const newKeys_2 = map(keyOf, items);
        if (n === 0) {
            if (order.length > 0) {
                for (let i_9 = 0; i_9 <= (rows.length - 1); i_9++) {
                    disposeSafe(item_1(i_9, rows).Dispose);
                }
                removeAllRowNodes();
                byKey.clear();
                indexed = false;
                order = [];
                rows = [];
            }
        }
        else if ((newKeys = newKeys_2, (order.length > newKeys.length) ? false : ((ok = true, (i_1 = 0, ((() => {
            while (ok && (i_1 < order.length)) {
                if (!equals(item_1(i_1, order), item_1(i_1, newKeys))) {
                    ok = false;
                }
                i_1 = ((i_1 + 1) | 0);
            }
        })(), ok)))))) {
            if (n > order.length) {
                const frag = document.createDocumentFragment();
                const newRows_1 = fill(new Array(n), 0, n, null);
                for (let i_10 = 0; i_10 <= (order.length - 1); i_10++) {
                    setItem(newRows_1, i_10, item_1(i_10, rows));
                }
                for (let i_11 = order.length; i_11 <= (n - 1); i_11++) {
                    const e_4 = make(item_1(i_11, items), i_11);
                    if (indexed) {
                        byKey.set(item_1(i_11, newKeys_2), e_4);
                    }
                    setItem(newRows_1, i_11, e_4);
                    frag.appendChild(e_4.Node);
                }
                parent.insertBefore(frag, anchor);
                rows = newRows_1;
            }
            order = newKeys_2;
        }
        else {
            let start_1 = 0;
            const lim = min(order.length, n) | 0;
            while ((start_1 < lim) && equals(item_1(start_1, order), item_1(start_1, newKeys_2))) {
                start_1 = ((start_1 + 1) | 0);
            }
            let endOld_1 = order.length - 1;
            let endNew_1 = n - 1;
            while (((endOld_1 >= start_1) && (endNew_1 >= start_1)) && equals(item_1(endOld_1, order), item_1(endNew_1, newKeys_2))) {
                endOld_1 = ((endOld_1 - 1) | 0);
                endNew_1 = ((endNew_1 - 1) | 0);
            }
            if (((start_1 === 0) && (endNew_1 === (n - 1))) && ((newKeys_1 = newKeys_2, (reindex(), (dj = true, (i_2 = 0, ((() => {
                while (dj && (i_2 < newKeys_1.length)) {
                    if (byKey.has(item_1(i_2, newKeys_1))) {
                        dj = false;
                    }
                    i_2 = ((i_2 + 1) | 0);
                }
            })(), dj))))))) {
                for (let i_12 = 0; i_12 <= (rows.length - 1); i_12++) {
                    disposeSafe(item_1(i_12, rows).Dispose);
                }
                removeAllRowNodes();
                const frag_1 = document.createDocumentFragment();
                const newRows_2 = fill(new Array(n), 0, n, null);
                for (let i_13 = 0; i_13 <= (n - 1); i_13++) {
                    const e_5 = make(item_1(i_13, items), i_13);
                    setItem(newRows_2, i_13, e_5);
                    frag_1.appendChild(e_5.Node);
                }
                parent.insertBefore(frag_1, anchor);
                byKey.clear();
                indexed = false;
                rows = newRows_2;
                order = newKeys_2;
            }
            else {
                const start = start_1 | 0;
                const endOld = endOld_1 | 0;
                const endNew = endNew_1 | 0;
                const newRows = fill(new Array(n), 0, n, null);
                for (let i_4 = 0; i_4 <= (start - 1); i_4++) {
                    setItem(newRows, i_4, item_1(i_4, rows));
                }
                let oi = order.length - 1;
                let ni = n - 1;
                while (ni > endNew) {
                    setItem(newRows, ni, item_1(oi, rows));
                    oi = ((oi - 1) | 0);
                    ni = ((ni - 1) | 0);
                }
                generation = ((generation + 1) | 0);
                const count = ((endNew - start) + 1) | 0;
                const srcIdx = (count > 0) ? (new Int32Array(count)) : (new Int32Array([]));
                for (let i_5 = start; i_5 <= endNew; i_5++) {
                    const k = item_1(i_5, newKeys_2);
                    if ((i_5 <= endOld) && equals(item_1(i_5, order), k)) {
                        const e = item_1(i_5, rows);
                        e.Seen = (generation | 0);
                        setItem(newRows, i_5, e);
                        setItem(srcIdx, i_5 - start, i_5 | 0);
                    }
                    else {
                        reindex();
                        let matchValue;
                        let outArg = defaultOf();
                        matchValue = [tryGetValue(byKey, k, new FSharpRef(() => outArg, (v) => {
                            outArg = v;
                        })), outArg];
                        if (matchValue[0]) {
                            const e_1 = matchValue[1];
                            e_1.Seen = (generation | 0);
                            setItem(newRows, i_5, e_1);
                            setItem(srcIdx, i_5 - start, e_1.Index | 0);
                        }
                        else {
                            const e_2 = make(item_1(i_5, items), i_5);
                            e_2.Seen = (generation | 0);
                            byKey.set(k, e_2);
                            setItem(newRows, i_5, e_2);
                            setItem(srcIdx, i_5 - start, -1);
                        }
                    }
                }
                for (let i_6 = start; i_6 <= endOld; i_6++) {
                    const e_3 = item_1(i_6, rows);
                    if (e_3.Seen !== generation) {
                        disposeSafe(e_3.Dispose);
                        parent.removeChild(e_3.Node);
                        if (indexed) {
                            byKey.delete(item_1(i_6, order));
                        }
                    }
                }
                const keep = (count > 0) ? fill(new Array(count), 0, count, false) : [];
                if (count > 0) {
                    markLis(srcIdx, keep);
                }
                let nextSibling = ((endNew + 1) < n) ? item_1(endNew + 1, newRows).Node : anchor;
                for (let i_7 = endNew; i_7 >= start; i_7--) {
                    const node_1 = item_1(i_7, newRows).Node;
                    if (!item_1(i_7 - start, keep)) {
                        parent.insertBefore(node_1, nextSibling);
                    }
                    nextSibling = node_1;
                }
                order = newKeys_2;
                rows = newRows;
                for (let i_8 = start; i_8 <= (n - 1); i_8++) {
                    item_1(i_8, newRows).Index = (i_8 | 0);
                }
            }
        }
    });
    const disposeAll = () => {
        disposeSafe(sub);
        for (let i_14 = 0; i_14 <= (rows.length - 1); i_14++) {
            disposeSafe(item_1(i_14, rows).Dispose);
            parent.removeChild(item_1(i_14, rows).Node);
        }
        byKey.clear();
        indexed = false;
        order = [];
        rows = [];
    };
    Signal_onCleanup(disposeAll);
    return {
        Dispose() {
            disposeAll();
        },
    };
}

