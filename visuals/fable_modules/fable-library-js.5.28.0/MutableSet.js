import { disposeSafe, defaultOf, toIterator, getEnumerator } from "./Util.js";
import { iterate, map, iterateIndexed, concat } from "./Seq.js";
import { item as item_1, setItem } from "./Array.js";
import { some } from "./Option.js";
import { FSharpRef } from "./Types.js";
import { class_type } from "./Reflection.js";
import { getItemFromDict } from "./MapUtil.js";
export class HashSet {
    constructor(items, comparer) {
        const this$ = new FSharpRef(defaultOf());
        this.comparer = comparer;
        this$.contents = this;
        this.hashMap = (new Map([]));
        this.count = 0;
        this["init@9"] = 1;
        const enumerator = getEnumerator(items);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const item = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                HashSet__Add_2B595(this$.contents, item);
            }
        }
        finally {
            disposeSafe(enumerator);
        }
    }
    get [Symbol.toStringTag]() {
        return "HashSet";
    }
    toJSON() {
        const this$ = this;
        return Array.from(this$);
    }
    "System.Collections.IEnumerable.GetEnumerator"() {
        const this$ = this;
        return getEnumerator(this$);
    }
    GetEnumerator() {
        const this$ = this;
        return getEnumerator(concat(this$.hashMap.values()));
    }
    [Symbol.iterator]() {
        return toIterator(getEnumerator(this));
    }
    "System.Collections.Generic.ICollection`1.Add2B595"(item) {
        const this$ = this;
        HashSet__Add_2B595(this$, item);
    }
    "System.Collections.Generic.ICollection`1.Clear"() {
        const this$ = this;
        HashSet__Clear(this$);
    }
    "System.Collections.Generic.ICollection`1.Contains2B595"(item) {
        const this$ = this;
        return HashSet__Contains_2B595(this$, item);
    }
    "System.Collections.Generic.ICollection`1.CopyToZ3B4C077E"(array, arrayIndex) {
        const this$ = this;
        iterateIndexed((i, e) => {
            setItem(array, (arrayIndex + i) | 0, e);
        }, this$);
    }
    "System.Collections.Generic.ICollection`1.get_Count"() {
        const this$ = this;
        return HashSet__get_Count(this$) | 0;
    }
    "System.Collections.Generic.ICollection`1.get_IsReadOnly"() {
        return false;
    }
    "System.Collections.Generic.ICollection`1.Remove2B595"(item) {
        const this$ = this;
        return HashSet__Remove_2B595(this$, item);
    }
    get size() {
        const this$ = this;
        return HashSet__get_Count(this$) | 0;
    }
    add(k) {
        const this$ = this;
        HashSet__Add_2B595(this$, k);
        return this$;
    }
    clear() {
        const this$ = this;
        HashSet__Clear(this$);
    }
    delete(k) {
        const this$ = this;
        return HashSet__Remove_2B595(this$, k);
    }
    has(k) {
        const this$ = this;
        return HashSet__Contains_2B595(this$, k);
    }
    keys() {
        const this$ = this;
        return map((x) => x, this$);
    }
    values() {
        const this$ = this;
        return map((x) => x, this$);
    }
    entries() {
        const this$ = this;
        return map((v) => [v, v], this$);
    }
    forEach(f, thisArg) {
        const this$ = this;
        iterate((x) => {
            f(x, x, this$);
        }, this$);
    }
}
export function HashSet_$reflection(gen0) {
    return class_type("Fable.Collections.HashSet", [gen0], HashSet);
}
export function HashSet_$ctor_Z6150332D(items, comparer) {
    return new HashSet(items, comparer);
}
function HashSet__TryFindBucket_Z524259A4(this$, h) {
    if (this$.hashMap.has(h)) {
        return getItemFromDict(this$.hashMap, h);
    }
    else {
        return undefined;
    }
}
function HashSet__FindIndex_Z142BBCA7(this$, k, bucket) {
    let i = 0;
    let found = -1;
    while ((found < 0) && (i < bucket.length)) {
        if (this$.comparer.Equals(k, bucket[i] ?? item_1(i, bucket))) {
            found = (i | 0);
        }
        i = ((i + 1) | 0);
    }
    return found | 0;
}
function HashSet__TryFind_2B595(this$, k) {
    const matchValue = HashSet__TryFindBucket_Z524259A4(this$, this$.comparer.GetHashCode(k));
    if (matchValue == null) {
        return undefined;
    }
    else {
        const bucket = matchValue;
        const i = HashSet__FindIndex_Z142BBCA7(this$, k, bucket) | 0;
        if (i > -1) {
            return some(bucket[i] ?? item_1(i, bucket));
        }
        else {
            return undefined;
        }
    }
}
export function HashSet__get_Comparer(this$) {
    return this$.comparer;
}
export function HashSet__Clear(this$) {
    this$.hashMap.clear();
    this$.count = 0;
}
export function HashSet__get_Count(this$) {
    return this$.count | 0;
}
export function HashSet__Add_2B595(this$, k) {
    const h = this$.comparer.GetHashCode(k) | 0;
    const matchValue = HashSet__TryFindBucket_Z524259A4(this$, h);
    if (matchValue == null) {
        this$.hashMap.set(h, [k]);
        this$.count = ((this$.count + 1) | 0);
        return true;
    }
    else {
        const bucket = matchValue;
        if (HashSet__FindIndex_Z142BBCA7(this$, k, bucket) > -1) {
            return false;
        }
        else {
            void (bucket.push(k));
            this$.count = ((this$.count + 1) | 0);
            return true;
        }
    }
}
export function HashSet__Contains_2B595(this$, k) {
    const matchValue = HashSet__TryFindBucket_Z524259A4(this$, this$.comparer.GetHashCode(k));
    if (matchValue == null) {
        return false;
    }
    else {
        return HashSet__FindIndex_Z142BBCA7(this$, k, matchValue) > -1;
    }
}
export function HashSet__Remove_2B595(this$, k) {
    const h = this$.comparer.GetHashCode(k) | 0;
    const matchValue = HashSet__TryFindBucket_Z524259A4(this$, h);
    if (matchValue == null) {
        return false;
    }
    else {
        const bucket = matchValue;
        const i = HashSet__FindIndex_Z142BBCA7(this$, k, bucket) | 0;
        if (i > -1) {
            bucket.splice(i, 1);
            this$.count = ((this$.count - 1) | 0);
            if (bucket.length === 0) {
                this$.hashMap.delete(h);
            }
            return true;
        }
        else {
            return false;
        }
    }
}
