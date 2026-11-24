import { isArrayLike, defaultOf, toIterator, compare, structuralHash, equals, disposeSafe, getEnumerator } from "./Util.js";
import { toStringOrEmpty } from "./Types.js";
import { defaultArg, value as value_1, some } from "./Option.js";
import { class_type } from "./Reflection.js";
import { Operators_InvalidArg, Operators_InvalidOp } from "./FSharp.Core.js";
import { SR_inputSequenceTooLong, SR_notEnoughElements, SR_inputSequenceEmpty, SR_inputMustBeNonNegative, SR_keyNotFoundAlt, SR_indexOutOfBounds, SR_inputWasEmpty } from "./Global.js";
import { KeyNotFoundException_$ctor_Z721C83C5 } from "./System.Collections.Generic.js";
import { randomSampleBy as randomSampleBy_1, randomChoicesBy as randomChoicesBy_1, randomChoiceBy as randomChoiceBy_1, randomShuffleInPlaceBy, transpose as transpose_1, splitInto as splitInto_1, windowed as windowed_1, pairwise as pairwise_1, chunkBySize as chunkBySize_1, map as map_1, permute as permute_1, tryFindIndexBack as tryFindIndexBack_1, tryFindBack as tryFindBack_1, scanBack as scanBack_1, item as item_1, foldBack2 as foldBack2_1, foldBack as foldBack_1, setItem, fill } from "./Array.js";
import { ArgumentOutOfRangeException_$ctor_Z384F8060 } from "./System.js";
import { nonSeeded } from "./Random.js";
export class FSharpList {
    constructor(head, tail) {
        this.head = head;
        this.tail = tail;
    }
    toString() {
        const xs = this;
        let result = "[";
        let first = true;
        const enumerator = getEnumerator(xs);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                let x = undefined, matchValue = undefined, s = undefined;
                const x_1 = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                result = ((first ? result : (result + "; ")) + ((x = x_1, (matchValue = x, (typeof matchValue === "string") ? ((s = matchValue, ("\"" + s) + "\"")) : toStringOrEmpty(x)))));
                first = false;
            }
        }
        finally {
            disposeSafe(enumerator);
        }
        return result + "]";
    }
    Equals(other) {
        const xs = this;
        if (xs === other) {
            return true;
        }
        else if (other instanceof FSharpList) {
            const ys = other;
            const loop = (xs_1_mut, ys_1_mut) => {
                loop: while (true) {
                    const xs_1 = xs_1_mut, ys_1 = ys_1_mut;
                    const matchValue = xs_1.tail;
                    const matchValue_1 = ys_1.tail;
                    if (matchValue != null) {
                        if (matchValue_1 != null) {
                            const xt = matchValue;
                            const yt = matchValue_1;
                            if (equals(xs_1.head, ys_1.head)) {
                                xs_1_mut = xt;
                                ys_1_mut = yt;
                                continue loop;
                            }
                            else {
                                return false;
                            }
                        }
                        else {
                            return false;
                        }
                    }
                    else if (matchValue_1 != null) {
                        return false;
                    }
                    else {
                        return true;
                    }
                    break;
                }
            };
            return loop(xs, ys);
        }
        else {
            return false;
        }
    }
    GetHashCode() {
        const xs = this;
        const loop = (i_mut, h_mut, xs_1_mut) => {
            loop: while (true) {
                const i = i_mut, h = h_mut, xs_1 = xs_1_mut;
                const matchValue = xs_1.tail;
                if (matchValue != null) {
                    const t = matchValue;
                    if (i > 18) {
                        return h | 0;
                    }
                    else {
                        i_mut = ((i + 1) | 0);
                        h_mut = (((((h << 1) + structuralHash(xs_1.head)) | 0) + Math.imul(631, i)) | 0);
                        xs_1_mut = t;
                        continue loop;
                    }
                }
                else {
                    return h | 0;
                }
                break;
            }
        };
        return loop(0, 0, xs) | 0;
    }
    toJSON() {
        const this$ = this;
        return Array.from(this$);
    }
    CompareTo(other) {
        const xs = this;
        if (other instanceof FSharpList) {
            const ys = other;
            const loop = (xs_1_mut, ys_1_mut) => {
                loop: while (true) {
                    const xs_1 = xs_1_mut, ys_1 = ys_1_mut;
                    const matchValue = xs_1.tail;
                    const matchValue_1 = ys_1.tail;
                    if (matchValue != null) {
                        if (matchValue_1 != null) {
                            const xt = matchValue;
                            const yt = matchValue_1;
                            const c = compare(xs_1.head, ys_1.head) | 0;
                            if (c === 0) {
                                xs_1_mut = xt;
                                ys_1_mut = yt;
                                continue loop;
                            }
                            else {
                                return c | 0;
                            }
                        }
                        else {
                            return 1;
                        }
                    }
                    else if (matchValue_1 != null) {
                        return -1;
                    }
                    else {
                        return 0;
                    }
                    break;
                }
            };
            return loop(xs, ys) | 0;
        }
        else {
            return 1;
        }
    }
    GetEnumerator() {
        const xs = this;
        return ListEnumerator$1_$ctor_3002E699(xs);
    }
    [Symbol.iterator]() {
        return toIterator(getEnumerator(this));
    }
    "System.Collections.IEnumerable.GetEnumerator"() {
        const xs = this;
        return getEnumerator(xs);
    }
}
export function FSharpList_$reflection(gen0) {
    return class_type("ListModule.FSharpList", [gen0], FSharpList);
}
export class ListEnumerator$1 {
    constructor(xs) {
        this.xs = xs;
        this.it = this.xs;
        this.current = defaultOf();
    }
    "System.Collections.Generic.IEnumerator`1.get_Current"() {
        const _ = this;
        return _.current;
    }
    "System.Collections.IEnumerator.get_Current"() {
        const _ = this;
        return _.current;
    }
    "System.Collections.IEnumerator.MoveNext"() {
        const _ = this;
        const matchValue = _.it.tail;
        if (matchValue != null) {
            const t = matchValue;
            _.current = _.it.head;
            _.it = t;
            return true;
        }
        else {
            return false;
        }
    }
    "System.Collections.IEnumerator.Reset"() {
        const _ = this;
        _.it = _.xs;
        _.current = defaultOf();
    }
    Dispose() {
    }
}
export function ListEnumerator$1_$reflection(gen0) {
    return class_type("ListModule.ListEnumerator`1", [gen0], ListEnumerator$1);
}
export function ListEnumerator$1_$ctor_3002E699(xs) {
    return new ListEnumerator$1(xs);
}
export function FSharpList_$ctor_Z23DD7291(head_1, tail_1) {
    return new FSharpList(head_1, tail_1);
}
export function FSharpList__get_IsEmpty(xs) {
    return xs.tail == null;
}
export function FSharpList__get_Length(xs) {
    const loop = (i_mut, xs_1_mut) => {
        loop: while (true) {
            const i = i_mut, xs_1 = xs_1_mut;
            const matchValue = xs_1.tail;
            if (matchValue != null) {
                i_mut = ((i + 1) | 0);
                xs_1_mut = matchValue;
                continue loop;
            }
            else {
                return i | 0;
            }
            break;
        }
    };
    return loop(0, xs) | 0;
}
export function FSharpList__get_Head(xs) {
    const matchValue = xs.tail;
    if (matchValue != null) {
        return xs.head;
    }
    else {
        throw Operators_InvalidOp(SR_inputWasEmpty);
    }
}
export function FSharpList__get_Tail(xs) {
    const matchValue = xs.tail;
    if (matchValue != null) {
        return matchValue;
    }
    else {
        throw Operators_InvalidOp(SR_inputWasEmpty);
    }
}
export function FSharpList__get_Item_Z524259A4(xs, index) {
    const loop = (i_mut, xs_1_mut) => {
        loop: while (true) {
            const i = i_mut, xs_1 = xs_1_mut;
            const matchValue = xs_1.tail;
            if (matchValue != null) {
                const t = matchValue;
                if (i === index) {
                    return xs_1.head;
                }
                else {
                    i_mut = ((i + 1) | 0);
                    xs_1_mut = t;
                    continue loop;
                }
            }
            else {
                throw Operators_InvalidArg("index", SR_indexOutOfBounds);
            }
            break;
        }
    };
    return loop(0, xs);
}
const emptyNode = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
export function empty() {
    return emptyNode;
}
export function indexNotFound() {
    throw KeyNotFoundException_$ctor_Z721C83C5(SR_keyNotFoundAlt);
}
function differentLengths(paramName) {
    throw Operators_InvalidArg(paramName, "The lists had different lengths.");
}
export function cons(x, xs) {
    return FSharpList_$ctor_Z23DD7291(x, xs);
}
export function singleton(x) {
    return FSharpList_$ctor_Z23DD7291(x, empty());
}
export function isEmpty(xs) {
    return FSharpList__get_IsEmpty(xs);
}
export function length(xs) {
    return FSharpList__get_Length(xs) | 0;
}
export function head(xs) {
    const matchValue = xs.tail;
    if (matchValue != null) {
        return xs.head;
    }
    else {
        throw Operators_InvalidArg("list", SR_inputWasEmpty);
    }
}
export function tryHead(xs) {
    if (FSharpList__get_IsEmpty(xs)) {
        return undefined;
    }
    else {
        return some(FSharpList__get_Head(xs));
    }
}
export function tail(xs) {
    const matchValue = xs.tail;
    if (matchValue != null) {
        return matchValue;
    }
    else {
        throw Operators_InvalidArg("list", SR_inputWasEmpty);
    }
}
export function tryLast(xs_mut) {
    tryLast: while (true) {
        const xs = xs_mut;
        if (FSharpList__get_IsEmpty(xs)) {
            return undefined;
        }
        else {
            const t = FSharpList__get_Tail(xs);
            if (FSharpList__get_IsEmpty(t)) {
                return some(FSharpList__get_Head(xs));
            }
            else {
                xs_mut = t;
                continue tryLast;
            }
        }
        break;
    }
}
export function last(xs) {
    const matchValue = tryLast(xs);
    if (matchValue == null) {
        throw Operators_InvalidArg("list", SR_inputWasEmpty);
    }
    else {
        return value_1(matchValue);
    }
}
export function compareWith(comparer, xs, ys) {
    const loop = (xs_1_mut, ys_1_mut) => {
        loop: while (true) {
            const xs_1 = xs_1_mut, ys_1 = ys_1_mut;
            const matchValue = FSharpList__get_IsEmpty(xs_1);
            const matchValue_1 = FSharpList__get_IsEmpty(ys_1);
            if (matchValue) {
                if (matchValue_1) {
                    return 0;
                }
                else {
                    return -1;
                }
            }
            else if (matchValue_1) {
                return 1;
            }
            else {
                const c = comparer(FSharpList__get_Head(xs_1), FSharpList__get_Head(ys_1)) | 0;
                if (c === 0) {
                    xs_1_mut = FSharpList__get_Tail(xs_1);
                    ys_1_mut = FSharpList__get_Tail(ys_1);
                    continue loop;
                }
                else {
                    return c | 0;
                }
            }
            break;
        }
    };
    return loop(xs, ys) | 0;
}
export function toArray(xs) {
    const len = FSharpList__get_Length(xs) | 0;
    const res = fill(new Array(len), 0, len, null);
    const loop = (i_mut, xs_1_mut) => {
        loop: while (true) {
            const i = i_mut, xs_1 = xs_1_mut;
            if (!FSharpList__get_IsEmpty(xs_1)) {
                setItem(res, i, FSharpList__get_Head(xs_1));
                i_mut = ((i + 1) | 0);
                xs_1_mut = FSharpList__get_Tail(xs_1);
                continue loop;
            }
            break;
        }
    };
    loop(0, xs);
    return res;
}
export function fold(folder, state, xs) {
    let acc = state;
    let xs_1 = xs;
    while (!FSharpList__get_IsEmpty(xs_1)) {
        acc = folder(acc, head(xs_1));
        xs_1 = FSharpList__get_Tail(xs_1);
    }
    return acc;
}
export function reverse(xs) {
    return fold((acc, x) => FSharpList_$ctor_Z23DD7291(x, acc), empty(), xs);
}
export function foldBack(folder, xs, state) {
    return foldBack_1(folder, toArray(xs), state);
}
export function foldIndexed(folder, state, xs) {
    const loop = (i_mut, acc_mut, xs_1_mut) => {
        loop: while (true) {
            const i = i_mut, acc = acc_mut, xs_1 = xs_1_mut;
            if (FSharpList__get_IsEmpty(xs_1)) {
                return acc;
            }
            else {
                i_mut = ((i + 1) | 0);
                acc_mut = folder(i, acc, FSharpList__get_Head(xs_1));
                xs_1_mut = FSharpList__get_Tail(xs_1);
                continue loop;
            }
            break;
        }
    };
    return loop(0, state, xs);
}
export function fold2(folder, state, xs, ys) {
    let acc = state;
    let xs_1 = xs;
    let ys_1 = ys;
    while (!FSharpList__get_IsEmpty(xs_1) && !FSharpList__get_IsEmpty(ys_1)) {
        acc = folder(acc, FSharpList__get_Head(xs_1), FSharpList__get_Head(ys_1));
        xs_1 = FSharpList__get_Tail(xs_1);
        ys_1 = FSharpList__get_Tail(ys_1);
    }
    if (!FSharpList__get_IsEmpty(xs_1)) {
        differentLengths("list2");
    }
    else if (!FSharpList__get_IsEmpty(ys_1)) {
        differentLengths("list1");
    }
    return acc;
}
export function foldBack2(folder, xs, ys, state) {
    const xs_1 = toArray(xs);
    const ys_1 = toArray(ys);
    if (xs_1.length !== ys_1.length) {
        differentLengths((xs_1.length === 0) ? "list1" : ((ys_1.length === 0) ? "list2" : "list1, list2"));
    }
    return foldBack2_1(folder, xs_1, ys_1, state);
}
export function unfold(generator, state) {
    const loop = (acc_mut, node_mut) => {
        loop: while (true) {
            const acc = acc_mut, node = node_mut;
            let t = undefined;
            const matchValue = generator(acc);
            if (matchValue != null) {
                acc_mut = matchValue[1];
                node_mut = ((t = FSharpList_$ctor_Z23DD7291(matchValue[0], undefined), (node.tail = t, t)));
                continue loop;
            }
            else {
                return node;
            }
            break;
        }
    };
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node_1 = loop(state, root);
    const t_2 = empty();
    node_1.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function iterate(action, xs) {
    fold((unitVar, x) => {
        action(x);
    }, undefined, xs);
}
export function iterate2(action, xs, ys) {
    fold2((unitVar, x, y) => {
        action(x, y);
    }, undefined, xs, ys);
}
export function iterateIndexed(action, xs) {
    fold((i, x) => {
        action(i, x);
        return (i + 1) | 0;
    }, 0, xs);
}
export function iterateIndexed2(action, xs, ys) {
    fold2((i, x, y) => {
        action(i, x, y);
        return (i + 1) | 0;
    }, 0, xs, ys);
}
export function toSeq(xs) {
    return xs;
}
export function ofArrayWithTail(xs, tail_1) {
    let res = tail_1;
    for (let i = (xs.length - 1) | 0; i >= 0; i--) {
        res = FSharpList_$ctor_Z23DD7291(xs[i] ?? item_1(i, xs), res);
    }
    return res;
}
export function ofArray(xs) {
    return ofArrayWithTail(xs, empty());
}
export function ofSeq(xs) {
    if (isArrayLike(xs)) {
        return ofArray(xs);
    }
    else if (xs instanceof FSharpList) {
        return xs;
    }
    else {
        const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
        let node = root;
        const enumerator = getEnumerator(xs);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                let xs_3 = undefined, t = undefined;
                const x_1 = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                node = ((xs_3 = node, (t = FSharpList_$ctor_Z23DD7291(x_1, undefined), (xs_3.tail = t, t))));
            }
        }
        finally {
            disposeSafe(enumerator);
        }
        const xs_5 = node;
        const t_2 = empty();
        xs_5.tail = t_2;
        return FSharpList__get_Tail(root);
    }
}
export function concat(lists) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    let node = root;
    const action = (xs) => {
        node = fold((acc, x_1) => {
            const t = FSharpList_$ctor_Z23DD7291(x_1, undefined);
            acc.tail = t;
            return t;
        }, node, xs);
    };
    if (isArrayLike(lists)) {
        const xs_3 = lists;
        xs_3.forEach(action);
    }
    else if (lists instanceof FSharpList) {
        iterate(action, lists);
    }
    else {
        const enumerator = getEnumerator(lists);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                action(enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]());
            }
        }
        finally {
            disposeSafe(enumerator);
        }
    }
    const xs_6 = node;
    const t_2 = empty();
    xs_6.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function scan(folder, state, xs) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    let node;
    const t = FSharpList_$ctor_Z23DD7291(state, undefined);
    root.tail = t;
    node = t;
    let acc = state;
    let xs_3 = xs;
    while (!FSharpList__get_IsEmpty(xs_3)) {
        let xs_4 = undefined, t_2 = undefined;
        acc = folder(acc, FSharpList__get_Head(xs_3));
        node = ((xs_4 = node, (t_2 = FSharpList_$ctor_Z23DD7291(acc, undefined), (xs_4.tail = t_2, t_2))));
        xs_3 = FSharpList__get_Tail(xs_3);
    }
    const xs_6 = node;
    const t_4 = empty();
    xs_6.tail = t_4;
    return FSharpList__get_Tail(root);
}
export function scanBack(folder, xs, state) {
    return ofArray(scanBack_1(folder, toArray(xs), state));
}
export function append(xs, ys) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    let node = root;
    let rest = xs;
    while (rest.tail != null) {
        let xs_1 = undefined, t = undefined;
        node = ((xs_1 = node, (t = FSharpList_$ctor_Z23DD7291(rest.head, undefined), (xs_1.tail = t, t))));
        rest = rest.tail;
    }
    const xs_3 = node;
    xs_3.tail = ys;
    return FSharpList__get_Tail(root);
}
export function collect(mapping, xs) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    let node = root;
    let ys = xs;
    while (!FSharpList__get_IsEmpty(ys)) {
        let zs = mapping(FSharpList__get_Head(ys));
        while (!FSharpList__get_IsEmpty(zs)) {
            let xs_1 = undefined, t = undefined;
            node = ((xs_1 = node, (t = FSharpList_$ctor_Z23DD7291(FSharpList__get_Head(zs), undefined), (xs_1.tail = t, t))));
            zs = FSharpList__get_Tail(zs);
        }
        ys = FSharpList__get_Tail(ys);
    }
    const xs_3 = node;
    const t_2 = empty();
    xs_3.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function mapIndexed(mapping, xs) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node = foldIndexed((i, acc, x_1) => {
        const t = FSharpList_$ctor_Z23DD7291(mapping(i, x_1), undefined);
        acc.tail = t;
        return t;
    }, root, xs);
    const t_2 = empty();
    node.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function map(mapping, xs) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node = fold((acc, x_1) => {
        const t = FSharpList_$ctor_Z23DD7291(mapping(x_1), undefined);
        acc.tail = t;
        return t;
    }, root, xs);
    const t_2 = empty();
    node.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function indexed(xs) {
    return mapIndexed((i, x) => [i, x], xs);
}
export function map2(mapping, xs, ys) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node = fold2((acc, x_1, y) => {
        const t = FSharpList_$ctor_Z23DD7291(mapping(x_1, y), undefined);
        acc.tail = t;
        return t;
    }, root, xs, ys);
    const t_2 = empty();
    node.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function mapIndexed2(mapping, xs, ys) {
    const loop = (i_mut, acc_mut, xs_1_mut, ys_1_mut) => {
        loop: while (true) {
            const i = i_mut, acc = acc_mut, xs_1 = xs_1_mut, ys_1 = ys_1_mut;
            let t = undefined;
            if (FSharpList__get_IsEmpty(xs_1) ? true : FSharpList__get_IsEmpty(ys_1)) {
                if (!FSharpList__get_IsEmpty(xs_1)) {
                    differentLengths("list2");
                }
                else if (!FSharpList__get_IsEmpty(ys_1)) {
                    differentLengths("list1");
                }
                return acc;
            }
            else {
                i_mut = ((i + 1) | 0);
                acc_mut = ((t = FSharpList_$ctor_Z23DD7291(mapping(i, FSharpList__get_Head(xs_1), FSharpList__get_Head(ys_1)), undefined), (acc.tail = t, t)));
                xs_1_mut = FSharpList__get_Tail(xs_1);
                ys_1_mut = FSharpList__get_Tail(ys_1);
                continue loop;
            }
            break;
        }
    };
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node_1 = loop(0, root, xs, ys);
    const t_2 = empty();
    node_1.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function map3(mapping, xs, ys, zs) {
    const loop = (acc_mut, xs_1_mut, ys_1_mut, zs_1_mut) => {
        loop: while (true) {
            const acc = acc_mut, xs_1 = xs_1_mut, ys_1 = ys_1_mut, zs_1 = zs_1_mut;
            let t = undefined;
            if ((FSharpList__get_IsEmpty(xs_1) ? true : FSharpList__get_IsEmpty(ys_1)) ? true : FSharpList__get_IsEmpty(zs_1)) {
                if (!((FSharpList__get_IsEmpty(xs_1) && FSharpList__get_IsEmpty(ys_1)) && FSharpList__get_IsEmpty(zs_1))) {
                    differentLengths("list1, list2, list3");
                }
                return acc;
            }
            else {
                acc_mut = ((t = FSharpList_$ctor_Z23DD7291(mapping(FSharpList__get_Head(xs_1), FSharpList__get_Head(ys_1), FSharpList__get_Head(zs_1)), undefined), (acc.tail = t, t)));
                xs_1_mut = FSharpList__get_Tail(xs_1);
                ys_1_mut = FSharpList__get_Tail(ys_1);
                zs_1_mut = FSharpList__get_Tail(zs_1);
                continue loop;
            }
            break;
        }
    };
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node_1 = loop(root, xs, ys, zs);
    const t_2 = empty();
    node_1.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function mapFold(mapping, state, xs) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const patternInput_1 = fold((tupledArg, x) => {
        let t = undefined;
        const patternInput = mapping(tupledArg[1], x);
        return [(t = FSharpList_$ctor_Z23DD7291(patternInput[0], undefined), (tupledArg[0].tail = t, t)), patternInput[1]];
    }, [root, state], xs);
    const t_2 = empty();
    patternInput_1[0].tail = t_2;
    return [FSharpList__get_Tail(root), patternInput_1[1]];
}
export function mapFoldBack(mapping, xs, state) {
    return mapFold((acc, x) => mapping(x, acc), state, reverse(xs));
}
export function tryPick(f, xs) {
    const loop = (xs_1_mut) => {
        loop: while (true) {
            const xs_1 = xs_1_mut;
            if (FSharpList__get_IsEmpty(xs_1)) {
                return undefined;
            }
            else {
                const matchValue = f(FSharpList__get_Head(xs_1));
                if (matchValue == null) {
                    xs_1_mut = FSharpList__get_Tail(xs_1);
                    continue loop;
                }
                else {
                    return matchValue;
                }
            }
            break;
        }
    };
    return loop(xs);
}
export function pick(f, xs) {
    const matchValue = tryPick(f, xs);
    if (matchValue == null) {
        return indexNotFound();
    }
    else {
        return value_1(matchValue);
    }
}
export function tryFind(f, xs) {
    return tryPick((x) => (f(x) ? some(x) : undefined), xs);
}
export function find(f, xs) {
    const matchValue = tryFind(f, xs);
    if (matchValue == null) {
        return indexNotFound();
    }
    else {
        return value_1(matchValue);
    }
}
export function tryFindBack(f, xs) {
    return tryFindBack_1(f, toArray(xs));
}
export function findBack(f, xs) {
    const matchValue = tryFindBack(f, xs);
    if (matchValue == null) {
        return indexNotFound();
    }
    else {
        return value_1(matchValue);
    }
}
export function tryFindIndex(f, xs) {
    const loop = (i_mut, xs_1_mut) => {
        loop: while (true) {
            const i = i_mut, xs_1 = xs_1_mut;
            if (FSharpList__get_IsEmpty(xs_1)) {
                return undefined;
            }
            else if (f(FSharpList__get_Head(xs_1))) {
                return i;
            }
            else {
                i_mut = ((i + 1) | 0);
                xs_1_mut = FSharpList__get_Tail(xs_1);
                continue loop;
            }
            break;
        }
    };
    return loop(0, xs);
}
export function findIndex(f, xs) {
    const matchValue = tryFindIndex(f, xs);
    if (matchValue == null) {
        indexNotFound();
        return -1;
    }
    else {
        return matchValue | 0;
    }
}
export function tryFindIndexBack(f, xs) {
    return tryFindIndexBack_1(f, toArray(xs));
}
export function findIndexBack(f, xs) {
    const matchValue = tryFindIndexBack(f, xs);
    if (matchValue == null) {
        indexNotFound();
        return -1;
    }
    else {
        return matchValue | 0;
    }
}
export function tryItem(n, xs) {
    const loop = (i_mut, xs_1_mut) => {
        loop: while (true) {
            const i = i_mut, xs_1 = xs_1_mut;
            if (FSharpList__get_IsEmpty(xs_1)) {
                return undefined;
            }
            else if (i === n) {
                return some(FSharpList__get_Head(xs_1));
            }
            else {
                i_mut = ((i + 1) | 0);
                xs_1_mut = FSharpList__get_Tail(xs_1);
                continue loop;
            }
            break;
        }
    };
    return loop(0, xs);
}
export function item(n, xs) {
    return FSharpList__get_Item_Z524259A4(xs, n);
}
export function filter(f, xs) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node = fold((acc, x_1) => {
        if (f(x_1)) {
            const t = FSharpList_$ctor_Z23DD7291(x_1, undefined);
            acc.tail = t;
            return t;
        }
        else {
            return acc;
        }
    }, root, xs);
    const t_2 = empty();
    node.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function partition(f, xs) {
    const matchValue = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const root2 = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const root1 = matchValue;
    const patternInput_1 = fold((tupledArg, x_2) => {
        let t = undefined, t_2 = undefined;
        const lacc = tupledArg[0];
        const racc = tupledArg[1];
        if (f(x_2)) {
            return [(t = FSharpList_$ctor_Z23DD7291(x_2, undefined), (lacc.tail = t, t)), racc];
        }
        else {
            return [lacc, (t_2 = FSharpList_$ctor_Z23DD7291(x_2, undefined), (racc.tail = t_2, t_2))];
        }
    }, [root1, root2], xs);
    const t_4 = empty();
    patternInput_1[0].tail = t_4;
    const t_5 = empty();
    patternInput_1[1].tail = t_5;
    return [FSharpList__get_Tail(root1), FSharpList__get_Tail(root2)];
}
export function choose(f, xs) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node = fold((acc, x_1) => {
        const matchValue = f(x_1);
        if (matchValue == null) {
            return acc;
        }
        else {
            const t = FSharpList_$ctor_Z23DD7291(value_1(matchValue), undefined);
            acc.tail = t;
            return t;
        }
    }, root, xs);
    const t_2 = empty();
    node.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function contains(value, xs, eq) {
    return tryFindIndex((v) => eq.Equals(value, v), xs) != null;
}
export function initialize(n, f) {
    if (n < 0) {
        throw Operators_InvalidArg("count", SR_inputMustBeNonNegative);
    }
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    let node = root;
    for (let i = 0; i <= ((n - 1) | 0); i++) {
        let xs = undefined, t = undefined;
        node = ((xs = node, (t = FSharpList_$ctor_Z23DD7291(f(i), undefined), (xs.tail = t, t))));
    }
    const xs_2 = node;
    const t_2 = empty();
    xs_2.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function replicate(n, x) {
    return initialize(n, (_arg) => x);
}
export function reduce(f, xs) {
    if (FSharpList__get_IsEmpty(xs)) {
        throw Operators_InvalidArg("list", SR_inputWasEmpty);
    }
    else {
        return fold(f, head(xs), tail(xs));
    }
}
export function reduceBack(f, xs) {
    if (FSharpList__get_IsEmpty(xs)) {
        throw Operators_InvalidArg("list", SR_inputWasEmpty);
    }
    else {
        return foldBack(f, tail(xs), head(xs));
    }
}
export function forAll(f, xs) {
    return fold((acc, x) => (acc && f(x)), true, xs);
}
export function forAll2(f_mut, xs_mut, ys_mut) {
    forAll2: while (true) {
        const f = f_mut, xs = xs_mut, ys = ys_mut;
        const matchValue = FSharpList__get_IsEmpty(xs);
        const matchValue_1 = FSharpList__get_IsEmpty(ys);
        if (matchValue) {
            if (matchValue_1) {
                return true;
            }
            else {
                return differentLengths("list1");
            }
        }
        else if (matchValue_1) {
            return differentLengths("list2");
        }
        else if (f(FSharpList__get_Head(xs), FSharpList__get_Head(ys))) {
            f_mut = f;
            xs_mut = FSharpList__get_Tail(xs);
            ys_mut = FSharpList__get_Tail(ys);
            continue forAll2;
        }
        else {
            return false;
        }
        break;
    }
}
export function exists(f, xs) {
    return tryFindIndex(f, xs) != null;
}
export function exists2(f_mut, xs_mut, ys_mut) {
    exists2: while (true) {
        const f = f_mut, xs = xs_mut, ys = ys_mut;
        const matchValue = FSharpList__get_IsEmpty(xs);
        const matchValue_1 = FSharpList__get_IsEmpty(ys);
        let matchResult = undefined;
        if (matchValue) {
            if (matchValue_1) {
                matchResult = 0;
            }
            else {
                matchResult = 2;
            }
        }
        else if (matchValue_1) {
            matchResult = 2;
        }
        else {
            matchResult = 1;
        }
        switch (matchResult) {
            case 0:
                return false;
            case 1:
                if (f(FSharpList__get_Head(xs), FSharpList__get_Head(ys))) {
                    return true;
                }
                else {
                    f_mut = f;
                    xs_mut = FSharpList__get_Tail(xs);
                    ys_mut = FSharpList__get_Tail(ys);
                    continue exists2;
                }
            default:
                return differentLengths("list2");
        }
        break;
    }
}
export function unzip(xs) {
    return foldBack((tupledArg, tupledArg_1) => [FSharpList_$ctor_Z23DD7291(tupledArg[0], tupledArg_1[0]), FSharpList_$ctor_Z23DD7291(tupledArg[1], tupledArg_1[1])], xs, [empty(), empty()]);
}
export function unzip3(xs) {
    return foldBack((tupledArg, tupledArg_1) => [FSharpList_$ctor_Z23DD7291(tupledArg[0], tupledArg_1[0]), FSharpList_$ctor_Z23DD7291(tupledArg[1], tupledArg_1[1]), FSharpList_$ctor_Z23DD7291(tupledArg[2], tupledArg_1[2])], xs, [empty(), empty(), empty()]);
}
export function zip(xs, ys) {
    return map2((x, y) => [x, y], xs, ys);
}
export function zip3(xs, ys, zs) {
    return map3((x, y, z) => [x, y, z], xs, ys, zs);
}
export function sortWith(comparer, xs) {
    const arr = toArray(xs);
    arr.sort(comparer);
    return ofArray(arr);
}
export function sort(xs, comparer) {
    return sortWith((x, y) => (comparer.Compare(x, y) | 0), xs);
}
export function sortBy(projection, xs, comparer) {
    return sortWith((x, y) => (comparer.Compare(projection(x), projection(y)) | 0), xs);
}
export function sortDescending(xs, comparer) {
    return sortWith((x, y) => Math.imul(comparer.Compare(x, y), -1), xs);
}
export function sortByDescending(projection, xs, comparer) {
    return sortWith((x, y) => Math.imul(comparer.Compare(projection(x), projection(y)), -1), xs);
}
export function sum(xs, adder) {
    return fold((acc, x) => adder.Add(acc, x), adder.GetZero(), xs);
}
export function sumBy(f, xs, adder) {
    return fold((acc, x) => adder.Add(acc, f(x)), adder.GetZero(), xs);
}
export function maxBy(projection, xs, comparer) {
    return reduce((x, y) => ((comparer.Compare(projection(y), projection(x)) > 0) ? y : x), xs);
}
export function max(xs, comparer) {
    return reduce((x, y) => ((comparer.Compare(y, x) > 0) ? y : x), xs);
}
export function minBy(projection, xs, comparer) {
    return reduce((x, y) => ((comparer.Compare(projection(y), projection(x)) > 0) ? x : y), xs);
}
export function min(xs, comparer) {
    return reduce((x, y) => ((comparer.Compare(y, x) > 0) ? x : y), xs);
}
export function average(xs, averager) {
    if (FSharpList__get_IsEmpty(xs)) {
        throw Operators_InvalidArg("source", SR_inputSequenceEmpty);
    }
    let count = 0;
    const total = fold((acc, x) => {
        count = ((count + 1) | 0);
        return averager.Add(acc, x);
    }, averager.GetZero(), xs);
    return averager.DivideByInt(total, count);
}
export function averageBy(f, xs, averager) {
    if (FSharpList__get_IsEmpty(xs)) {
        throw Operators_InvalidArg("source", SR_inputSequenceEmpty);
    }
    let count = 0;
    const total = fold((acc, x) => {
        count = ((count + 1) | 0);
        return averager.Add(acc, f(x));
    }, averager.GetZero(), xs);
    return averager.DivideByInt(total, count);
}
export function permute(f, xs) {
    return ofArray(permute_1(f, toArray(xs)));
}
export function chunkBySize(chunkSize, xs) {
    return ofArray(map_1(ofArray, chunkBySize_1(chunkSize, toArray(xs))));
}
export function allPairs(xs, ys) {
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    let node = root;
    iterate((x_1) => {
        iterate((y) => {
            let xs_1 = undefined, t = undefined;
            node = ((xs_1 = node, (t = FSharpList_$ctor_Z23DD7291([x_1, y], undefined), (xs_1.tail = t, t))));
        }, ys);
    }, xs);
    const xs_3 = node;
    const t_2 = empty();
    xs_3.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function skip(count_mut, xs_mut) {
    skip: while (true) {
        const count = count_mut, xs = xs_mut;
        if (count <= 0) {
            return xs;
        }
        else if (FSharpList__get_IsEmpty(xs)) {
            throw ArgumentOutOfRangeException_$ctor_Z384F8060("count", "The index is outside the legal range.");
        }
        else {
            count_mut = ((count - 1) | 0);
            xs_mut = FSharpList__get_Tail(xs);
            continue skip;
        }
        break;
    }
}
export function skipWhile(predicate_mut, xs_mut) {
    skipWhile: while (true) {
        const predicate = predicate_mut, xs = xs_mut;
        if (FSharpList__get_IsEmpty(xs)) {
            return xs;
        }
        else if (!predicate(FSharpList__get_Head(xs))) {
            return xs;
        }
        else {
            predicate_mut = predicate;
            xs_mut = FSharpList__get_Tail(xs);
            continue skipWhile;
        }
        break;
    }
}
export function take(count, xs) {
    if (count < 0) {
        throw Operators_InvalidArg("count", SR_inputMustBeNonNegative);
    }
    const loop = (i_mut, acc_mut, xs_1_mut) => {
        loop: while (true) {
            const i = i_mut, acc = acc_mut, xs_1 = xs_1_mut;
            let t = undefined;
            if (i <= 0) {
                return acc;
            }
            else if (FSharpList__get_IsEmpty(xs_1)) {
                throw Operators_InvalidOp(SR_notEnoughElements);
            }
            else {
                i_mut = ((i - 1) | 0);
                acc_mut = ((t = FSharpList_$ctor_Z23DD7291(FSharpList__get_Head(xs_1), undefined), (acc.tail = t, t)));
                xs_1_mut = FSharpList__get_Tail(xs_1);
                continue loop;
            }
            break;
        }
    };
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node = loop(count, root, xs);
    const t_2 = empty();
    node.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function takeWhile(predicate, xs) {
    const loop = (acc_mut, xs_1_mut) => {
        loop: while (true) {
            const acc = acc_mut, xs_1 = xs_1_mut;
            let t = undefined;
            if (FSharpList__get_IsEmpty(xs_1)) {
                return acc;
            }
            else if (!predicate(FSharpList__get_Head(xs_1))) {
                return acc;
            }
            else {
                acc_mut = ((t = FSharpList_$ctor_Z23DD7291(FSharpList__get_Head(xs_1), undefined), (acc.tail = t, t)));
                xs_1_mut = FSharpList__get_Tail(xs_1);
                continue loop;
            }
            break;
        }
    };
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node = loop(root, xs);
    const t_2 = empty();
    node.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function truncate(count, xs) {
    const loop = (i_mut, acc_mut, xs_1_mut) => {
        loop: while (true) {
            const i = i_mut, acc = acc_mut, xs_1 = xs_1_mut;
            let t = undefined;
            if (i <= 0) {
                return acc;
            }
            else if (FSharpList__get_IsEmpty(xs_1)) {
                return acc;
            }
            else {
                i_mut = ((i - 1) | 0);
                acc_mut = ((t = FSharpList_$ctor_Z23DD7291(FSharpList__get_Head(xs_1), undefined), (acc.tail = t, t)));
                xs_1_mut = FSharpList__get_Tail(xs_1);
                continue loop;
            }
            break;
        }
    };
    const root = FSharpList_$ctor_Z23DD7291(defaultOf(), undefined);
    const node = loop(count, root, xs);
    const t_2 = empty();
    node.tail = t_2;
    return FSharpList__get_Tail(root);
}
export function getSlice(startIndex, endIndex, xs) {
    const len = length(xs) | 0;
    let startIndex_1;
    const index = defaultArg(startIndex, 0) | 0;
    startIndex_1 = ((index < 0) ? 0 : index);
    let endIndex_1;
    const index_1 = defaultArg(endIndex, (len - 1) | 0) | 0;
    endIndex_1 = ((index_1 >= len) ? ((len - 1) | 0) : index_1);
    if (endIndex_1 < startIndex_1) {
        return empty();
    }
    else {
        return take(((endIndex_1 - startIndex_1) + 1) | 0, skip(startIndex_1, xs));
    }
}
export function splitAt(index, xs) {
    if (index < 0) {
        throw Operators_InvalidArg("index", SR_inputMustBeNonNegative);
    }
    if (index > FSharpList__get_Length(xs)) {
        throw Operators_InvalidOp(SR_notEnoughElements);
    }
    return [take(index, xs), skip(index, xs)];
}
export function exactlyOne(xs) {
    if (FSharpList__get_IsEmpty(xs)) {
        throw Operators_InvalidArg("list", SR_inputSequenceEmpty);
    }
    else if (FSharpList__get_IsEmpty(FSharpList__get_Tail(xs))) {
        return FSharpList__get_Head(xs);
    }
    else {
        throw Operators_InvalidArg("list", SR_inputSequenceTooLong);
    }
}
export function tryExactlyOne(xs) {
    if (!FSharpList__get_IsEmpty(xs) && FSharpList__get_IsEmpty(FSharpList__get_Tail(xs))) {
        return some(FSharpList__get_Head(xs));
    }
    else {
        return undefined;
    }
}
export function where(predicate, xs) {
    return filter(predicate, xs);
}
export function pairwise(xs) {
    return ofArray(pairwise_1(toArray(xs)));
}
export function windowed(windowSize, xs) {
    return ofArray(map_1(ofArray, windowed_1(windowSize, toArray(xs))));
}
export function splitInto(chunks, xs) {
    return ofArray(map_1(ofArray, splitInto_1(chunks, toArray(xs))));
}
export function transpose(lists) {
    return ofArray(map_1(ofArray, transpose_1(map_1(toArray, Array.from(lists)))));
}
export function insertAt(index, y, xs) {
    let i = -1;
    let isDone = false;
    const result = fold((acc, x) => {
        i = ((i + 1) | 0);
        if (i === index) {
            isDone = true;
            return FSharpList_$ctor_Z23DD7291(x, FSharpList_$ctor_Z23DD7291(y, acc));
        }
        else {
            return FSharpList_$ctor_Z23DD7291(x, acc);
        }
    }, empty(), xs);
    return reverse(isDone ? result : ((((i + 1) | 0) === index) ? FSharpList_$ctor_Z23DD7291(y, result) : (() => {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    })()));
}
export function insertManyAt(index, ys, xs) {
    let i = -1;
    let isDone = false;
    const ys_1 = ofSeq(ys);
    const result = fold((acc, x) => {
        i = ((i + 1) | 0);
        if (i === index) {
            isDone = true;
            return FSharpList_$ctor_Z23DD7291(x, append(ys_1, acc));
        }
        else {
            return FSharpList_$ctor_Z23DD7291(x, acc);
        }
    }, empty(), xs);
    return reverse(isDone ? result : ((((i + 1) | 0) === index) ? append(ys_1, result) : (() => {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    })()));
}
export function removeAt(index, xs) {
    let i = -1;
    let isDone = false;
    const ys = filter((_arg) => {
        i = ((i + 1) | 0);
        if (i === index) {
            isDone = true;
            return false;
        }
        else {
            return true;
        }
    }, xs);
    if (!isDone) {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    }
    return ys;
}
export function removeManyAt(index, count, xs) {
    if (index < 0) {
        throw Operators_InvalidArg("index", "index must be within bounds of the list");
    }
    const endIndex = (index + count) | 0;
    let i = -1;
    const ys = filter((_arg) => {
        i = ((i + 1) | 0);
        if (i < index) {
            return true;
        }
        else {
            return i >= endIndex;
        }
    }, xs);
    if (endIndex > ((i + 1) | 0)) {
        throw Operators_InvalidArg("index", "index must be within bounds of the list");
    }
    return ys;
}
export function updateAt(index, y, xs) {
    let isDone = false;
    const ys = mapIndexed((i, x) => {
        if (i === index) {
            isDone = true;
            return y;
        }
        else {
            return x;
        }
    }, xs);
    if (!isDone) {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    }
    return ys;
}
export function randomShuffleBy(randomizer, xs) {
    const arr = toArray(xs);
    randomShuffleInPlaceBy(randomizer, arr);
    return ofArray(arr);
}
export function randomShuffleWith(random, xs) {
    return randomShuffleBy(() => random.NextDouble(), xs);
}
export function randomShuffle(xs) {
    return randomShuffleWith(nonSeeded(), xs);
}
export function randomChoiceBy(randomizer, xs) {
    if (isEmpty(xs)) {
        throw Operators_InvalidArg("source", SR_inputSequenceEmpty);
    }
    return randomChoiceBy_1(randomizer, toArray(xs));
}
export function randomChoiceWith(random, xs) {
    return randomChoiceBy(() => random.NextDouble(), xs);
}
export function randomChoice(xs) {
    return randomChoiceWith(nonSeeded(), xs);
}
export function randomChoicesBy(randomizer, count, xs) {
    return ofArray(randomChoicesBy_1(randomizer, count, toArray(xs)));
}
export function randomChoicesWith(random, count, xs) {
    return randomChoicesBy(() => random.NextDouble(), count, xs);
}
export function randomChoices(count, xs) {
    return randomChoicesWith(nonSeeded(), count, xs);
}
export function randomSampleBy(randomizer, count, xs) {
    return ofArray(randomSampleBy_1(randomizer, count, toArray(xs)));
}
export function randomSampleWith(random, count, xs) {
    return randomSampleBy(() => random.NextDouble(), count, xs);
}
export function randomSample(count, xs) {
    return randomSampleWith(nonSeeded(), count, xs);
}
