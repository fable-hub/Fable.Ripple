
import { equals, structuralHash, defaultOf } from "../../../fable_modules/fable-library-js.5.28.0/Util.js";
import { class_type } from "../../../fable_modules/fable-library-js.5.28.0/Reflection.js";
import { setItem, item } from "../../../fable_modules/fable-library-js.5.28.0/Array.js";

/**
 * Map keyed by F# equality. Primitive keys live in a JS `Map`; object keys are
 * bucketed by structural hash and compared with structural equality.
 */
export class KeyMap$2 {
    constructor() {
        this.direct = (new Map());
        this.buckets = defaultOf();
    }
}

export function KeyMap$2_$reflection(gen0, gen1) {
    return class_type("Fable.Ripple.Internal.KeyMap`2", [gen0, gen1], KeyMap$2);
}

export function KeyMap$2_$ctor() {
    return new KeyMap$2();
}

/**
 * The value for `k`, or `null`/default when absent.
 */
export function KeyMap$2__Get_2B595(_, k) {
    if (KeyMap$2__isObject_2B595(_, k)) {
        const b = KeyMap$2__bucketOf(_, k, false);
        if (b == null) {
            return defaultOf();
        }
        else {
            const matchValue = KeyMap$2__indexIn(_, b, k) | 0;
            if (matchValue === -1) {
                return defaultOf();
            }
            else {
                const i = matchValue | 0;
                return (b[i] ?? item(i, b))[1];
            }
        }
    }
    else {
        return _.direct.get(k);
    }
}

export function KeyMap$2__Set_5BDDA1(_, k, v) {
    if (KeyMap$2__isObject_2B595(_, k)) {
        const b = KeyMap$2__bucketOf(_, k, true);
        const matchValue = KeyMap$2__indexIn(_, b, k) | 0;
        if (matchValue === -1) {
            void (b.push([k, v]));
        }
        else {
            setItem(b, matchValue, [k, v]);
        }
    }
    else {
        _.direct.set(k, v);
    }
}

export function KeyMap$2__Delete_2B595(_, k) {
    if (KeyMap$2__isObject_2B595(_, k)) {
        if (!(_.buckets == null)) {
            const h = structuralHash(k) | 0;
            const b = _.buckets.get(h);
            if (!(b == null)) {
                const matchValue = KeyMap$2__indexIn(_, b, k) | 0;
                if (matchValue === -1) {
                }
                else if (b.length === 1) {
                    _.buckets.delete(h);
                }
                else {
                    b.splice(matchValue, 1);
                }
            }
        }
    }
    else {
        _.direct.delete(k);
    }
}

export function KeyMap$2__Clear(_) {
    _.direct.clear();
    if (!(_.buckets == null)) {
        _.buckets.clear();
    }
}

/**
 * Number of entries.
 */
export function KeyMap$2__get_Count(_) {
    let n = _.direct.size;
    if (!(_.buckets == null)) {
        _.buckets.forEach((b, _arg, _arg_1) => {
            n = ((n + b.length) | 0);
        });
    }
    return n | 0;
}

export function KeyMap$2__isObject_2B595(this$, k) {
    if ((typeof k) === "object") {
        return !(k == null);
    }
    else {
        return false;
    }
}

export function KeyMap$2__indexIn(this$, bucket, k) {
    let i = (bucket.length - 1) | 0;
    while ((i >= 0) && !equals((bucket[i] ?? item(i, bucket))[0], k)) {
        i = ((i - 1) | 0);
    }
    return i | 0;
}

export function KeyMap$2__bucketOf(this$, k, create) {
    if (this$.buckets == null) {
        if (create) {
            this$.buckets = (new Map());
        }
    }
    if (this$.buckets == null) {
        return defaultOf();
    }
    else {
        const h = structuralHash(k) | 0;
        const b = this$.buckets.get(h);
        if ((b == null) && create) {
            const b_1 = [];
            this$.buckets.set(h, b_1);
            return b_1;
        }
        else {
            return b;
        }
    }
}

