import { disposeSafe, defaultOf, equals, toIterator, getEnumerator } from "./Util.js";
import { iterate, map, delay, toArray, iterateIndexed, concat } from "./Seq.js";
import { item as item_1, setItem } from "./Array.js";
import { FSharpRef } from "./Types.js";
import { class_type } from "./Reflection.js";
import { getItemFromDict } from "./MapUtil.js";
import { KeyNotFoundException_$ctor_Z721C83C5 } from "./System.Collections.Generic.js";
import { ArgumentException_$ctor_Z721C83C5 } from "./System.js";
import { format } from "./String.js";
export class Dictionary {
    constructor(pairs, comparer) {
        const this$ = new FSharpRef(defaultOf());
        this.comparer = comparer;
        this$.contents = this;
        this.hashMap = (new Map([]));
        this.count = 0;
        this["init@9"] = 1;
        const enumerator = getEnumerator(pairs);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const pair = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                Dictionary__Add_5BDDA1(this$.contents, pair[0], pair[1]);
            }
        }
        finally {
            disposeSafe(enumerator);
        }
    }
    get [Symbol.toStringTag]() {
        return "Dictionary";
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
        Dictionary__Add_5BDDA1(this$, item[0], item[1]);
    }
    "System.Collections.Generic.ICollection`1.Clear"() {
        const this$ = this;
        Dictionary__Clear(this$);
    }
    "System.Collections.Generic.ICollection`1.Contains2B595"(item) {
        const this$ = this;
        const matchValue = Dictionary__TryFind_2B595(this$, item[0]);
        let matchResult = undefined, pair_1 = undefined;
        if (matchValue != null) {
            if (equals(matchValue[1], item[1])) {
                matchResult = 0;
                pair_1 = matchValue;
            }
            else {
                matchResult = 1;
            }
        }
        else {
            matchResult = 1;
        }
        switch (matchResult) {
            case 0:
                return true;
            default:
                return false;
        }
    }
    "System.Collections.Generic.ICollection`1.CopyToZ3B4C077E"(array, arrayIndex) {
        const this$ = this;
        iterateIndexed((i, e) => {
            setItem(array, (arrayIndex + i) | 0, e);
        }, this$);
    }
    "System.Collections.Generic.ICollection`1.get_Count"() {
        const this$ = this;
        return Dictionary__get_Count(this$) | 0;
    }
    "System.Collections.Generic.ICollection`1.get_IsReadOnly"() {
        return false;
    }
    "System.Collections.Generic.ICollection`1.Remove2B595"(item) {
        const this$ = this;
        const matchValue = Dictionary__TryFind_2B595(this$, item[0]);
        let matchResult = undefined, pair_1 = undefined;
        if (matchValue != null) {
            if (equals(matchValue[1], item[1])) {
                matchResult = 0;
                pair_1 = matchValue;
            }
            else {
                matchResult = 1;
            }
        }
        else {
            matchResult = 1;
        }
        switch (matchResult) {
            case 0:
                return Dictionary__Remove_2B595(this$, item[0]);
            default:
                return false;
        }
    }
    "System.Collections.Generic.IDictionary`2.Add5BDDA1"(key, value) {
        const this$ = this;
        Dictionary__Add_5BDDA1(this$, key, value);
    }
    "System.Collections.Generic.IDictionary`2.ContainsKey2B595"(key) {
        const this$ = this;
        return Dictionary__ContainsKey_2B595(this$, key);
    }
    "System.Collections.Generic.IDictionary`2.get_Item2B595"(key) {
        const this$ = this;
        return Dictionary__get_Item_2B595(this$, key);
    }
    "System.Collections.Generic.IDictionary`2.set_Item5BDDA1"(key, v) {
        const this$ = this;
        Dictionary__set_Item_5BDDA1(this$, key, v);
    }
    "System.Collections.Generic.IDictionary`2.get_Keys"() {
        const this$ = this;
        return toArray(delay(() => map((pair) => pair[0], this$)));
    }
    "System.Collections.Generic.IDictionary`2.Remove2B595"(key) {
        const this$ = this;
        return Dictionary__Remove_2B595(this$, key);
    }
    "System.Collections.Generic.IDictionary`2.TryGetValue6DC89625"(key, value) {
        const this$ = this;
        const matchValue = Dictionary__TryFind_2B595(this$, key);
        if (matchValue != null) {
            const pair = matchValue;
            value.contents = pair[1];
            return true;
        }
        else {
            return false;
        }
    }
    "System.Collections.Generic.IDictionary`2.get_Values"() {
        const this$ = this;
        return toArray(delay(() => map((pair) => pair[1], this$)));
    }
    get size() {
        const this$ = this;
        return Dictionary__get_Count(this$) | 0;
    }
    clear() {
        const this$ = this;
        Dictionary__Clear(this$);
    }
    delete(k) {
        const this$ = this;
        return Dictionary__Remove_2B595(this$, k);
    }
    entries() {
        const this$ = this;
        return map((p) => [p[0], p[1]], this$);
    }
    get(k) {
        const this$ = this;
        return Dictionary__get_Item_2B595(this$, k);
    }
    has(k) {
        const this$ = this;
        return Dictionary__ContainsKey_2B595(this$, k);
    }
    keys() {
        const this$ = this;
        return map((p) => p[0], this$);
    }
    set(k, v) {
        const this$ = this;
        Dictionary__set_Item_5BDDA1(this$, k, v);
        return this$;
    }
    values() {
        const this$ = this;
        return map((p) => p[1], this$);
    }
    forEach(f, thisArg) {
        const this$ = this;
        iterate((p) => {
            f(p[1], p[0], this$);
        }, this$);
    }
}
export function Dictionary_$reflection(gen0, gen1) {
    return class_type("Fable.Collections.Dictionary", [gen0, gen1], Dictionary);
}
export function Dictionary_$ctor_6623D9B3(pairs, comparer) {
    return new Dictionary(pairs, comparer);
}
function Dictionary__TryFindBucket_Z524259A4(this$, h) {
    if (this$.hashMap.has(h)) {
        return getItemFromDict(this$.hashMap, h);
    }
    else {
        return undefined;
    }
}
function Dictionary__FindIndex_Z61A8C007(this$, k, bucket) {
    let i = 0;
    let found = -1;
    while ((found < 0) && (i < bucket.length)) {
        if (this$.comparer.Equals(k, (bucket[i] ?? item_1(i, bucket))[0])) {
            found = (i | 0);
        }
        i = ((i + 1) | 0);
    }
    return found | 0;
}
export function Dictionary__TryFind_2B595(this$, k) {
    const matchValue = Dictionary__TryFindBucket_Z524259A4(this$, this$.comparer.GetHashCode(k));
    if (matchValue == null) {
        return undefined;
    }
    else {
        const bucket = matchValue;
        const i = Dictionary__FindIndex_Z61A8C007(this$, k, bucket) | 0;
        if (i > -1) {
            return bucket[i] ?? item_1(i, bucket);
        }
        else {
            return undefined;
        }
    }
}
export function Dictionary__get_Comparer(this$) {
    return this$.comparer;
}
export function Dictionary__Clear(this$) {
    this$.hashMap.clear();
    this$.count = 0;
}
export function Dictionary__get_Count(this$) {
    return this$.count | 0;
}
export function Dictionary__get_Item_2B595(this$, k) {
    const matchValue = Dictionary__TryFind_2B595(this$, k);
    if (matchValue != null) {
        return matchValue[1];
    }
    else {
        throw KeyNotFoundException_$ctor_Z721C83C5("The item was not found in collection");
    }
}
export function Dictionary__set_Item_5BDDA1(this$, k, v) {
    const h = this$.comparer.GetHashCode(k) | 0;
    const matchValue = Dictionary__TryFindBucket_Z524259A4(this$, h);
    if (matchValue == null) {
        this$.hashMap.set(h, [[k, v]]);
        this$.count = ((this$.count + 1) | 0);
    }
    else {
        const bucket = matchValue;
        const i = Dictionary__FindIndex_Z61A8C007(this$, k, bucket) | 0;
        if (i > -1) {
            setItem(bucket, i, [k, v]);
        }
        else {
            void (bucket.push([k, v]));
            this$.count = ((this$.count + 1) | 0);
        }
    }
}
export function Dictionary__Add_5BDDA1(this$, k, v) {
    const h = this$.comparer.GetHashCode(k) | 0;
    const matchValue = Dictionary__TryFindBucket_Z524259A4(this$, h);
    if (matchValue == null) {
        this$.hashMap.set(h, [[k, v]]);
        this$.count = ((this$.count + 1) | 0);
    }
    else {
        const bucket = matchValue;
        if (Dictionary__FindIndex_Z61A8C007(this$, k, bucket) > -1) {
            throw ArgumentException_$ctor_Z721C83C5(format("An item with the same key has already been added. Key: {0}", k));
        }
        void (bucket.push([k, v]));
        this$.count = ((this$.count + 1) | 0);
    }
}
export function Dictionary__ContainsKey_2B595(this$, k) {
    const matchValue = Dictionary__TryFindBucket_Z524259A4(this$, this$.comparer.GetHashCode(k));
    if (matchValue == null) {
        return false;
    }
    else {
        return Dictionary__FindIndex_Z61A8C007(this$, k, matchValue) > -1;
    }
}
export function Dictionary__Remove_2B595(this$, k) {
    const h = this$.comparer.GetHashCode(k) | 0;
    const matchValue = Dictionary__TryFindBucket_Z524259A4(this$, h);
    if (matchValue == null) {
        return false;
    }
    else {
        const bucket = matchValue;
        const i = Dictionary__FindIndex_Z61A8C007(this$, k, bucket) | 0;
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
