
import { Signal_onCleanup, Signal_effect, Signal_root, Signal_autorun } from "../Fable.Ripple/Api.js";
import { Record } from "../../fable_modules/fable-library-js.5.28.0/Types.js";
import { record_type, int32_type, class_type } from "../../fable_modules/fable-library-js.5.28.0/Reflection.js";
import { fill, setItem, item as item_1 } from "../../fable_modules/fable-library-js.5.28.0/Array.js";
import { KeyMap$2__Delete_2B595, KeyMap$2__Clear, KeyMap$2__Set_5BDDA1, KeyMap$2_$ctor } from "../Fable.Ripple/Internal/KeyMap.js";
import { equals, disposeSafe } from "../../fable_modules/fable-library-js.5.28.0/Util.js";
import { min } from "../../fable_modules/fable-library-js.5.28.0/Double.js";
import { KeyMap$2__Get_2B595 } from "../Fable.Ripple/Internal/KeyMap.js";

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
    Signal_autorun(() => {
        t.nodeValue = f();
    });
}

/**
 * Reactive attribute.
 */
export function bindAttr(e, name, f) {
    Signal_autorun(() => {
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
    return record_type("Fable.Ripple.Dom.Dom.Row", [], Row, () => [["Node", class_type("Glutinum.Web.HTMLElement", undefined)], ["Dispose", class_type("System.IDisposable")], ["Seen", int32_type], ["Index", int32_type]]);
}

function markLis(src, keep) {
    const n = src.length | 0;
    const tails = new Int32Array(n);
    const prev = new Int32Array(n);
    let len = 0;
    for (let i = 0; i <= ((n - 1) | 0); i++) {
        const v = (src[i] ?? item_1(i, src)) | 0;
        if (v >= 0) {
            let lo = 0;
            let hi = len;
            while (lo < hi) {
                const mid = ~~(((lo + hi) | 0) / 2);
                if (item_1(tails[mid] ?? item_1(mid, tails), src) < v) {
                    lo = ((mid + 1) | 0);
                }
                else {
                    hi = (mid | 0);
                }
            }
            setItem(prev, i, ((lo > 0) ? item_1((lo - 1) | 0, tails) : -1) | 0);
            setItem(tails, lo, i | 0);
            if (lo === len) {
                len = ((len + 1) | 0);
            }
        }
    }
    if (len > 0) {
        let k = item_1((len - 1) | 0, tails);
        while (k >= 0) {
            setItem(keep, k, true);
            k = ((prev[k] ?? item_1(k, prev)) | 0);
        }
    }
}

/**
 * Render `items` into `parent` (rows placed before `anchor`, `None` to
 * append). Each row's `render` runs in its own `Signal.root`, disposed
 * when the row leaves or the enclosing scope tears down.
 */
export function keyedEach(parent, anchor, getItems, keyOf, render) {
    const byKey = KeyMap$2_$ctor();
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
            for (let i = 0; i <= ((order.length - 1) | 0); i++) {
                KeyMap$2__Set_5BDDA1(byKey, order[i] ?? item_1(i, order), rows[i] ?? item_1(i, rows));
            }
            indexed = true;
        }
    };
    const removeAllRowNodes = () => {
        const owned = ((anchor != null) ? ((order.length + 1) | 0) : order.length) | 0;
        if (~~parent.childNodes.length === owned) {
            parent.textContent = "";
            const option_1 = anchor;
            if (option_1 != null) {
                const anchor_1 = option_1;
                parent.appendChild(anchor_1);
            }
        }
        else {
            for (let i_4 = 0; i_4 <= ((rows.length - 1) | 0); i_4++) {
                parent.removeChild((rows[i_4] ?? item_1(i_4, rows)).Node);
            }
        }
    };
    const sub = Signal_effect(() => {
        let newKeys, ok, i_1, newKeys_1, last, ok_1, i_2, newKeys_2, dj, i_3;
        const items = getItems();
        const n = items.length | 0;
        const newKeys_3 = new Array(n);
        for (let i_5 = 0; i_5 <= ((n - 1) | 0); i_5++) {
            newKeys_3[i_5] = keyOf(items[i_5] ?? item_1(i_5, items));
        }
        if (n === 0) {
            if (order.length > 0) {
                for (let i_11 = 0; i_11 <= ((rows.length - 1) | 0); i_11++) {
                    disposeSafe((rows[i_11] ?? item_1(i_11, rows)).Dispose);
                }
                removeAllRowNodes();
                KeyMap$2__Clear(byKey);
                indexed = false;
                order = [];
                rows = [];
            }
        }
        else if ((newKeys = newKeys_3, (order.length > newKeys.length) ? false : ((ok = true, (i_1 = 0, ((() => {
            while (ok && (i_1 < order.length)) {
                if (!equals(order[i_1] ?? item_1(i_1, order), newKeys[i_1] ?? item_1(i_1, newKeys))) {
                    ok = false;
                }
                i_1 = ((i_1 + 1) | 0);
            }
        })(), ok)))))) {
            if (n > order.length) {
                const frag = document.createDocumentFragment();
                const newRows_1 = fill(new Array(n), 0, n, null);
                for (let i_12 = 0; i_12 <= ((order.length - 1) | 0); i_12++) {
                    setItem(newRows_1, i_12, rows[i_12] ?? item_1(i_12, rows));
                }
                for (let i_13 = order.length; i_13 <= ((n - 1) | 0); i_13++) {
                    const e_4 = make(items[i_13] ?? item_1(i_13, items), i_13);
                    if (indexed) {
                        KeyMap$2__Set_5BDDA1(byKey, newKeys_3[i_13] ?? item_1(i_13, newKeys_3), e_4);
                    }
                    setItem(newRows_1, i_13, e_4);
                    frag.appendChild(e_4.Node);
                }
                parent.insertBefore(frag, anchor);
                rows = newRows_1;
            }
            order = newKeys_3;
        }
        else {
            let start_1 = 0;
            const lim = min(order.length, n) | 0;
            while ((start_1 < lim) && equals(order[start_1] ?? item_1(start_1, order), newKeys_3[start_1] ?? item_1(start_1, newKeys_3))) {
                start_1 = ((start_1 + 1) | 0);
            }
            let endOld_1 = (order.length - 1) | 0;
            let endNew_1 = (n - 1) | 0;
            while (((endOld_1 >= start_1) && (endNew_1 >= start_1)) && equals(order[endOld_1] ?? item_1(endOld_1, order), newKeys_3[endNew_1] ?? item_1(endNew_1, newKeys_3))) {
                endOld_1 = ((endOld_1 - 1) | 0);
                endNew_1 = ((endNew_1 - 1) | 0);
            }
            if (((((endOld_1 === endNew_1) && (start_1 < endNew_1)) && equals(order[start_1] ?? item_1(start_1, order), newKeys_3[endNew_1] ?? item_1(endNew_1, newKeys_3))) && equals(order[endNew_1] ?? item_1(endNew_1, order), newKeys_3[start_1] ?? item_1(start_1, newKeys_3))) && ((newKeys_1 = newKeys_3, (last = ((endNew_1 - 1) | 0), (ok_1 = true, (i_2 = ((start_1 + 1) | 0), ((() => {
                while (ok_1 && (i_2 <= last)) {
                    if (!equals(order[i_2] ?? item_1(i_2, order), newKeys_1[i_2] ?? item_1(i_2, newKeys_1))) {
                        ok_1 = false;
                    }
                    i_2 = ((i_2 + 1) | 0);
                }
            })(), ok_1))))))) {
                const a = rows[start_1] ?? item_1(start_1, rows);
                const b = rows[endNew_1] ?? item_1(endNew_1, rows);
                const afterB = b.Node.nextSibling;
                parent.insertBefore(b.Node, a.Node);
                parent.insertBefore(a.Node, afterB);
                setItem(rows, start_1, b);
                setItem(rows, endNew_1, a);
                b.Index = (start_1 | 0);
                a.Index = (endNew_1 | 0);
                order = newKeys_3;
            }
            else if (((start_1 === 0) && (endNew_1 === ((n - 1) | 0))) && ((newKeys_2 = newKeys_3, (reindex(), (dj = true, (i_3 = 0, ((() => {
                while (dj && (i_3 < newKeys_2.length)) {
                    if (!(KeyMap$2__Get_2B595(byKey, newKeys_2[i_3] ?? item_1(i_3, newKeys_2)) == null)) {
                        dj = false;
                    }
                    i_3 = ((i_3 + 1) | 0);
                }
            })(), dj))))))) {
                for (let i_14 = 0; i_14 <= ((rows.length - 1) | 0); i_14++) {
                    disposeSafe((rows[i_14] ?? item_1(i_14, rows)).Dispose);
                }
                removeAllRowNodes();
                const frag_1 = document.createDocumentFragment();
                const newRows_2 = fill(new Array(n), 0, n, null);
                for (let i_15 = 0; i_15 <= ((n - 1) | 0); i_15++) {
                    const e_5 = make(items[i_15] ?? item_1(i_15, items), i_15);
                    setItem(newRows_2, i_15, e_5);
                    frag_1.appendChild(e_5.Node);
                }
                parent.insertBefore(frag_1, anchor);
                KeyMap$2__Clear(byKey);
                indexed = false;
                rows = newRows_2;
                order = newKeys_3;
            }
            else {
                const start = start_1 | 0;
                const endOld = endOld_1 | 0;
                const endNew = endNew_1 | 0;
                const newRows = fill(new Array(n), 0, n, null);
                for (let i_6 = 0; i_6 <= ((start - 1) | 0); i_6++) {
                    setItem(newRows, i_6, rows[i_6] ?? item_1(i_6, rows));
                }
                let oi = (order.length - 1) | 0;
                let ni = (n - 1) | 0;
                while (ni > endNew) {
                    setItem(newRows, ni, rows[oi] ?? item_1(oi, rows));
                    oi = ((oi - 1) | 0);
                    ni = ((ni - 1) | 0);
                }
                generation = ((generation + 1) | 0);
                const count = ((endNew - start) + 1) | 0;
                const srcIdx = (count > 0) ? (new Int32Array(count)) : (new Int32Array([]));
                for (let i_7 = start; i_7 <= endNew; i_7++) {
                    const k_1 = newKeys_3[i_7] ?? item_1(i_7, newKeys_3);
                    if ((i_7 <= endOld) && equals(order[i_7] ?? item_1(i_7, order), k_1)) {
                        const e = rows[i_7] ?? item_1(i_7, rows);
                        e.Seen = (generation | 0);
                        setItem(newRows, i_7, e);
                        setItem(srcIdx, (i_7 - start) | 0, i_7 | 0);
                    }
                    else {
                        reindex();
                        const matchValue = KeyMap$2__Get_2B595(byKey, k_1);
                        if (matchValue == null) {
                            const e_2 = make(items[i_7] ?? item_1(i_7, items), i_7);
                            e_2.Seen = (generation | 0);
                            KeyMap$2__Set_5BDDA1(byKey, k_1, e_2);
                            setItem(newRows, i_7, e_2);
                            setItem(srcIdx, (i_7 - start) | 0, -1);
                        }
                        else {
                            const e_1 = matchValue;
                            e_1.Seen = (generation | 0);
                            setItem(newRows, i_7, e_1);
                            setItem(srcIdx, (i_7 - start) | 0, e_1.Index | 0);
                        }
                    }
                }
                for (let i_8 = start; i_8 <= endOld; i_8++) {
                    const e_3 = rows[i_8] ?? item_1(i_8, rows);
                    if (e_3.Seen !== generation) {
                        disposeSafe(e_3.Dispose);
                        parent.removeChild(e_3.Node);
                        if (indexed) {
                            KeyMap$2__Delete_2B595(byKey, order[i_8] ?? item_1(i_8, order));
                        }
                    }
                }
                const keep = (count > 0) ? fill(new Array(count), 0, count, false) : [];
                if (count > 0) {
                    markLis(srcIdx, keep);
                }
                let nextSibling = (((endNew + 1) | 0) < n) ? item_1((endNew + 1) | 0, newRows).Node : anchor;
                for (let i_9 = endNew; i_9 >= start; i_9--) {
                    const node_1 = (newRows[i_9] ?? item_1(i_9, newRows)).Node;
                    if (!item_1((i_9 - start) | 0, keep)) {
                        parent.insertBefore(node_1, nextSibling);
                    }
                    nextSibling = node_1;
                }
                order = newKeys_3;
                rows = newRows;
                for (let i_10 = start; i_10 <= ((n - 1) | 0); i_10++) {
                    (newRows[i_10] ?? item_1(i_10, newRows)).Index = (i_10 | 0);
                }
            }
        }
    });
    const disposeAll = () => {
        disposeSafe(sub);
        for (let i_16 = 0; i_16 <= ((rows.length - 1) | 0); i_16++) {
            disposeSafe((rows[i_16] ?? item_1(i_16, rows)).Dispose);
            parent.removeChild((rows[i_16] ?? item_1(i_16, rows)).Node);
        }
        KeyMap$2__Clear(byKey);
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

