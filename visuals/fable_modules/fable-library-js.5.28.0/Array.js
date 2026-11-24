import { KeyNotFoundException_$ctor_Z721C83C5 } from "./System.Collections.Generic.js";
import { SR_inputSequenceEmpty, SR_indexOutOfBounds, SR_notEnoughElements, SR_inputMustBeNonNegative, SR_keyNotFoundAlt } from "./Global.js";
import { Operators_InvalidOp, Operators_InvalidArg } from "./FSharp.Core.js";
import { Helpers_allocateArrayFromCons } from "./Native.js";
import { disposeSafe, getEnumerator, int32ToString, defaultOf } from "./Util.js";
import { setItem as setItem_1, item as item_3 } from "./Array.js";
import { ArgumentException_$ctor_Z721C83C5, ArgumentOutOfRangeException_$ctor_Z384F8060, IndexOutOfRangeException_$ctor } from "./System.js";
import { value as value_2, defaultArg, some } from "./Option.js";
import { min as min_1, max as max_1 } from "./Double.js";
import { float64ToString } from "./String.js";
import { nonSeeded } from "./Random.js";
import { addToSet } from "./MapUtil.js";
function indexNotFound() {
    throw KeyNotFoundException_$ctor_Z721C83C5(SR_keyNotFoundAlt);
}
function differentLengths(paramName) {
    throw Operators_InvalidArg(paramName, "The arrays have different lengths.");
}
export function append(array1, array2, cons) {
    const len1 = array1.length | 0;
    const len2 = array2.length | 0;
    const newArray = Helpers_allocateArrayFromCons(cons, (len1 + len2) | 0);
    for (let i = 0; i <= ((len1 - 1) | 0); i++) {
        setItem_1(newArray, i, array1[i] ?? item_3(i, array1));
    }
    for (let i_1 = 0; i_1 <= ((len2 - 1) | 0); i_1++) {
        setItem_1(newArray, (i_1 + len1) | 0, array2[i_1] ?? item_3(i_1, array2));
    }
    return newArray;
}
export function filter(predicate, array) {
    return array.filter(predicate);
}
export function fill(target, targetIndex, count, value) {
    if (targetIndex < 0) {
        throw Operators_InvalidArg("targetIndex", SR_inputMustBeNonNegative);
    }
    if (count < 0) {
        throw Operators_InvalidArg("count", SR_inputMustBeNonNegative);
    }
    if ((count > 0) && (((targetIndex + count) | 0) > target.length)) {
        throw IndexOutOfRangeException_$ctor();
    }
    const start = targetIndex | 0;
    return target.fill(value, start, ((start + count) | 0));
}
export function getSubArray(array, start, count) {
    if (start < 0) {
        throw Operators_InvalidArg("startIndex", SR_inputMustBeNonNegative);
    }
    if (count < 0) {
        throw Operators_InvalidArg("count", SR_inputMustBeNonNegative);
    }
    if (((start + count) | 0) > array.length) {
        throw ArgumentOutOfRangeException_$ctor_Z384F8060("count", "The index is outside the legal range.");
    }
    const start_1 = start | 0;
    return array.slice(start_1, ((start_1 + count) | 0));
}
export function last(array) {
    if (array.length === 0) {
        throw Operators_InvalidArg("array", "The input array was empty");
    }
    return item_3((array.length - 1) | 0, array);
}
export function tryLast(array) {
    if (array.length === 0) {
        return undefined;
    }
    else {
        return some(item_3((array.length - 1) | 0, array));
    }
}
export function mapIndexed(f, source, cons) {
    const len = source.length | 0;
    const target = Helpers_allocateArrayFromCons(cons, len);
    for (let i = 0; i <= ((len - 1) | 0); i++) {
        setItem_1(target, i, f(i, source[i] ?? item_3(i, source)));
    }
    return target;
}
export function map(f, source, cons) {
    const len = source.length | 0;
    const target = Helpers_allocateArrayFromCons(cons, len);
    for (let i = 0; i <= ((len - 1) | 0); i++) {
        setItem_1(target, i, f(source[i] ?? item_3(i, source)));
    }
    return target;
}
export function mapIndexed2(f, source1, source2, cons) {
    if (source1.length !== source2.length) {
        differentLengths("array1");
    }
    const result = Helpers_allocateArrayFromCons(cons, source1.length);
    for (let i = 0; i <= ((source1.length - 1) | 0); i++) {
        setItem_1(result, i, f(i, source1[i] ?? item_3(i, source1), source2[i] ?? item_3(i, source2)));
    }
    return result;
}
export function map2(f, source1, source2, cons) {
    if (source1.length !== source2.length) {
        differentLengths("array1");
    }
    const result = Helpers_allocateArrayFromCons(cons, source1.length);
    for (let i = 0; i <= ((source1.length - 1) | 0); i++) {
        setItem_1(result, i, f(source1[i] ?? item_3(i, source1), source2[i] ?? item_3(i, source2)));
    }
    return result;
}
export function mapIndexed3(f, source1, source2, source3, cons) {
    if ((source1.length !== source2.length) ? true : (source2.length !== source3.length)) {
        differentLengths("array1, array2, array3");
    }
    const result = Helpers_allocateArrayFromCons(cons, source1.length);
    for (let i = 0; i <= ((source1.length - 1) | 0); i++) {
        setItem_1(result, i, f(i, source1[i] ?? item_3(i, source1), source2[i] ?? item_3(i, source2), source3[i] ?? item_3(i, source3)));
    }
    return result;
}
export function map3(f, source1, source2, source3, cons) {
    if ((source1.length !== source2.length) ? true : (source2.length !== source3.length)) {
        differentLengths("array1, array2, array3");
    }
    const result = Helpers_allocateArrayFromCons(cons, source1.length);
    for (let i = 0; i <= ((source1.length - 1) | 0); i++) {
        setItem_1(result, i, f(source1[i] ?? item_3(i, source1), source2[i] ?? item_3(i, source2), source3[i] ?? item_3(i, source3)));
    }
    return result;
}
export function mapFold(mapping, state, array, cons) {
    const matchValue = array.length | 0;
    if (matchValue === 0) {
        return [[], state];
    }
    else {
        let acc = state;
        const res = Helpers_allocateArrayFromCons(cons, matchValue);
        for (let i = 0; i <= ((array.length - 1) | 0); i++) {
            const patternInput = mapping(acc, array[i] ?? item_3(i, array));
            setItem_1(res, i, patternInput[0]);
            acc = patternInput[1];
        }
        return [res, acc];
    }
}
export function mapFoldBack(mapping, array, state, cons) {
    const matchValue = array.length | 0;
    if (matchValue === 0) {
        return [[], state];
    }
    else {
        let acc = state;
        const res = Helpers_allocateArrayFromCons(cons, matchValue);
        for (let i = (array.length - 1) | 0; i >= 0; i--) {
            const patternInput = mapping(array[i] ?? item_3(i, array), acc);
            setItem_1(res, i, patternInput[0]);
            acc = patternInput[1];
        }
        return [res, acc];
    }
}
export function indexed(source) {
    const len = source.length | 0;
    const target = new Array(len);
    for (let i = 0; i <= ((len - 1) | 0); i++) {
        setItem_1(target, i, [i, source[i] ?? item_3(i, source)]);
    }
    return target;
}
export function truncate(count, array) {
    const count_1 = max_1(0, count) | 0;
    return array.slice(0, ((0 + count_1) | 0));
}
export function concat(arrays, cons) {
    const arrays_1 = Array.isArray(arrays) ? arrays : (Array.from(arrays));
    const matchValue = arrays_1.length | 0;
    switch (matchValue) {
        case 0:
            return Helpers_allocateArrayFromCons(cons, 0);
        case 1:
            return arrays_1[0] ?? item_3(0, arrays_1);
        default: {
            let totalIdx = 0;
            let totalLength = 0;
            for (let idx = 0; idx <= ((arrays_1.length - 1) | 0); idx++) {
                const arr_1 = arrays_1[idx] ?? item_3(idx, arrays_1);
                totalLength = ((totalLength + arr_1.length) | 0);
            }
            const result = Helpers_allocateArrayFromCons(cons, totalLength);
            for (let idx_1 = 0; idx_1 <= ((arrays_1.length - 1) | 0); idx_1++) {
                const arr_2 = arrays_1[idx_1] ?? item_3(idx_1, arrays_1);
                for (let j = 0; j <= ((arr_2.length - 1) | 0); j++) {
                    setItem_1(result, totalIdx, arr_2[j] ?? item_3(j, arr_2));
                    totalIdx = ((totalIdx + 1) | 0);
                }
            }
            return result;
        }
    }
}
export function collect(mapping, array, cons) {
    return concat(map(mapping, array, defaultOf()), cons);
}
export function where(predicate, array) {
    return array.filter(predicate);
}
export function indexOf(array, item_1, start, count, eq) {
    let option_1 = undefined;
    const start_1 = defaultArg(start, 0) | 0;
    const end$0027 = defaultArg((option_1 = count, (option_1 != null) ? ((start_1 + option_1) | 0) : undefined), array.length) | 0;
    const loop = (i_mut) => {
        loop: while (true) {
            const i = i_mut;
            if (i >= end$0027) {
                return -1;
            }
            else if (eq.Equals(item_1, array[i] ?? item_3(i, array))) {
                return i | 0;
            }
            else {
                i_mut = ((i + 1) | 0);
                continue loop;
            }
            break;
        }
    };
    return loop(start_1) | 0;
}
export function contains(value, array, eq) {
    return indexOf(array, value, undefined, undefined, eq) >= 0;
}
function binarySearchFrom(array, index, length, value, comparer) {
    const loop = (lo_mut, hi_mut) => {
        loop: while (true) {
            const lo = lo_mut, hi = hi_mut;
            if (lo > hi) {
                return ~lo;
            }
            else {
                const i = (lo + ((hi - lo) >> 1)) | 0;
                const c = comparer.Compare(array[i] ?? item_3(i, array), value) | 0;
                if (c === 0) {
                    return i | 0;
                }
                else if (c < 0) {
                    lo_mut = ((i + 1) | 0);
                    hi_mut = hi;
                    continue loop;
                }
                else {
                    lo_mut = lo;
                    hi_mut = ((i - 1) | 0);
                    continue loop;
                }
            }
            break;
        }
    };
    return loop(index, ((index + length) - 1) | 0) | 0;
}
function checkBinarySearchRange(lengthName, array, index, length) {
    if (index < 0) {
        throw ArgumentOutOfRangeException_$ctor_Z384F8060("index", "Non-negative number required.");
    }
    if (length < 0) {
        throw ArgumentOutOfRangeException_$ctor_Z384F8060(lengthName, "Non-negative number required.");
    }
    if (((array.length - index) | 0) < length) {
        throw ArgumentException_$ctor_Z721C83C5("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
    }
}
export function binarySearch(array, value, comparer) {
    return binarySearchFrom(array, 0, array.length, value, comparer) | 0;
}
export function binarySearchRange(array, index, length, value, comparer) {
    checkBinarySearchRange("length", array, index, length);
    return binarySearchFrom(array, index, length, value, comparer) | 0;
}
export function binarySearchRangeCount(array, index, count, value, comparer) {
    checkBinarySearchRange("count", array, index, count);
    return binarySearchFrom(array, index, count, value, comparer) | 0;
}
export function empty(cons) {
    return Helpers_allocateArrayFromCons(cons, 0);
}
export function singleton(value, cons) {
    const ar = Helpers_allocateArrayFromCons(cons, 1);
    setItem_1(ar, 0, value);
    return ar;
}
export function initialize(count, initializer, cons) {
    if (count < 0) {
        throw Operators_InvalidArg("count", "The input must be non-negative");
    }
    const result = Helpers_allocateArrayFromCons(cons, count);
    for (let i = 0; i <= ((count - 1) | 0); i++) {
        setItem_1(result, i, initializer(i));
    }
    return result;
}
export function pairwise(array) {
    if (array.length < 2) {
        return [];
    }
    else {
        const count = (array.length - 1) | 0;
        const result = new Array(count);
        for (let i = 0; i <= ((count - 1) | 0); i++) {
            setItem_1(result, i, [array[i] ?? item_3(i, array), item_3((i + 1) | 0, array)]);
        }
        return result;
    }
}
export function replicate(count, initial, cons) {
    if (count < 0) {
        throw Operators_InvalidArg("count", "The input must be non-negative");
    }
    const result = Helpers_allocateArrayFromCons(cons, count);
    for (let i = 0; i <= ((result.length - 1) | 0); i++) {
        setItem_1(result, i, initial);
    }
    return result;
}
export function copy(array) {
    return array.slice();
}
function copyTypedArray(source, sourceIndex, target, targetIndex, count) {
    target.set((source.subarray(sourceIndex, ((sourceIndex + count) | 0))), targetIndex);
}
function argumentOutOfRange(paramName, value, requirement) {
    throw ArgumentOutOfRangeException_$ctor_Z384F8060(paramName, (((paramName + " (\'") + int32ToString(value)) + "\') must be ") + requirement);
}
export function copyTo(source, sourceIndex, target, targetIndex, count) {
    if (count < 0) {
        argumentOutOfRange("length", count, "a non-negative value.");
    }
    if (sourceIndex < 0) {
        argumentOutOfRange("sourceIndex", sourceIndex, "greater than or equal to \'0\'.");
    }
    if (((sourceIndex + count) | 0) > source.length) {
        throw Operators_InvalidArg("sourceArray", "Source array was not long enough. Check the source index, length, and the array\'s lower bounds.");
    }
    if (targetIndex < 0) {
        argumentOutOfRange("destinationIndex", targetIndex, "greater than or equal to \'0\'.");
    }
    if (((targetIndex + count) | 0) > target.length) {
        throw Operators_InvalidArg("destinationArray", "Destination array was not long enough. Check the destination index, length, and the array\'s lower bounds.");
    }
    if (ArrayBuffer.isView(source) && ArrayBuffer.isView(target)) {
        copyTypedArray(source, sourceIndex, target, targetIndex, count);
    }
    else if (sourceIndex < targetIndex) {
        for (let i = (count - 1) | 0; i >= 0; i--) {
            const index_1 = (targetIndex + i) | 0;
            let item_1;
            const index = (sourceIndex + i) | 0;
            item_1 = (source[index]);
            target[index_1] = item_1;
        }
    }
    else {
        for (let i_1 = 0; i_1 <= ((count - 1) | 0); i_1++) {
            const index_3 = (targetIndex + i_1) | 0;
            let item_2;
            const index_2 = (sourceIndex + i_1) | 0;
            item_2 = (source[index_2]);
            target[index_3] = item_2;
        }
    }
}
export function reverse(array) {
    const array_2 = array.slice();
    return array_2.reverse();
}
export function scan(folder, state, array, cons) {
    const res = Helpers_allocateArrayFromCons(cons, (array.length + 1) | 0);
    setItem_1(res, 0, state);
    for (let i = 0; i <= ((array.length - 1) | 0); i++) {
        setItem_1(res, (i + 1) | 0, folder(res[i] ?? item_3(i, res), array[i] ?? item_3(i, array)));
    }
    return res;
}
export function scanBack(folder, array, state, cons) {
    const res = Helpers_allocateArrayFromCons(cons, (array.length + 1) | 0);
    setItem_1(res, array.length, state);
    for (let i = (array.length - 1) | 0; i >= 0; i--) {
        setItem_1(res, i, folder(array[i] ?? item_3(i, array), item_3((i + 1) | 0, res)));
    }
    return res;
}
export function skip(count, array, cons) {
    if (count > array.length) {
        throw ArgumentOutOfRangeException_$ctor_Z384F8060("count", "The index is outside the legal range.");
    }
    if (count === array.length) {
        return Helpers_allocateArrayFromCons(cons, 0);
    }
    else {
        const count_1 = ((count < 0) ? 0 : count) | 0;
        return array.slice(count_1);
    }
}
export function skipWhile(predicate, array, cons) {
    let count = 0;
    while ((count < array.length) && predicate(array[count] ?? item_3(count, array))) {
        count = ((count + 1) | 0);
    }
    if (count === array.length) {
        return Helpers_allocateArrayFromCons(cons, 0);
    }
    else {
        const count_1 = count | 0;
        return array.slice(count_1);
    }
}
export function take(count, array, cons) {
    if (count < 0) {
        throw Operators_InvalidArg("count", "The input must be non-negative");
    }
    if (count > array.length) {
        throw Operators_InvalidOp(SR_notEnoughElements);
    }
    if (count === 0) {
        return Helpers_allocateArrayFromCons(cons, 0);
    }
    else {
        return array.slice(0, ((0 + count) | 0));
    }
}
export function takeWhile(predicate, array, cons) {
    let count = 0;
    while ((count < array.length) && predicate(array[count] ?? item_3(count, array))) {
        count = ((count + 1) | 0);
    }
    if (count === 0) {
        return Helpers_allocateArrayFromCons(cons, 0);
    }
    else {
        const count_1 = count | 0;
        return array.slice(0, ((0 + count_1) | 0));
    }
}
export function findAll(predicate, array) {
    return array.filter(predicate);
}
export function getRange(array, start, count) {
    const start_1 = start | 0;
    return array.slice(start_1, ((start_1 + count) | 0));
}
export function addInPlace(x, array) {
    array.push(x);
}
export function addRangeInPlace(range, array) {
    const enumerator = getEnumerator(range);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            addInPlace(enumerator["System.Collections.Generic.IEnumerator`1.get_Current"](), array);
        }
    }
    finally {
        disposeSafe(enumerator);
    }
}
export function insertRangeInPlace(index, range, array) {
    let i = index;
    const enumerator = getEnumerator(range);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            let index_1 = undefined;
            const x = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            (index_1 = (i | 0), array.splice(index_1, 0, x));
            i = ((i + 1) | 0);
        }
    }
    finally {
        disposeSafe(enumerator);
    }
}
export function removeInPlace(item_1, array, eq) {
    const i = indexOf(array, item_1, undefined, undefined, eq) | 0;
    if (i > -1) {
        array.splice(i, 1);
        return true;
    }
    else {
        return false;
    }
}
export function removeAllInPlace(predicate, array) {
    const countRemoveAll = (count) => {
        let i;
        const array_1 = array;
        i = (array_1.findIndex(predicate));
        if (i > -1) {
            array.splice(i, 1);
            return (countRemoveAll(count) + 1) | 0;
        }
        else {
            return count | 0;
        }
    };
    return countRemoveAll(0) | 0;
}
export function partition(f, source, cons) {
    const len = source.length | 0;
    const res1 = Helpers_allocateArrayFromCons(cons, len);
    const res2 = Helpers_allocateArrayFromCons(cons, len);
    let iTrue = 0;
    let iFalse = 0;
    for (let i = 0; i <= ((len - 1) | 0); i++) {
        if (f(source[i] ?? item_3(i, source))) {
            setItem_1(res1, iTrue, source[i] ?? item_3(i, source));
            iTrue = ((iTrue + 1) | 0);
        }
        else {
            setItem_1(res2, iFalse, source[i] ?? item_3(i, source));
            iFalse = ((iFalse + 1) | 0);
        }
    }
    return [truncate(iTrue, res1), truncate(iFalse, res2)];
}
export function find(predicate, array) {
    const matchValue = array.find(predicate);
    if (matchValue == null) {
        return indexNotFound();
    }
    else {
        return value_2(matchValue);
    }
}
export function tryFind(predicate, array) {
    return array.find(predicate);
}
export function findIndex(predicate, array) {
    const matchValue = (array.findIndex(predicate)) | 0;
    if (matchValue > -1) {
        return matchValue | 0;
    }
    else {
        indexNotFound();
        return -1;
    }
}
export function tryFindIndex(predicate, array) {
    const matchValue = (array.findIndex(predicate)) | 0;
    if (matchValue > -1) {
        return matchValue;
    }
    else {
        return undefined;
    }
}
export function pick(chooser, array) {
    const loop = (i_mut) => {
        loop: while (true) {
            const i = i_mut;
            if (i >= array.length) {
                return indexNotFound();
            }
            else {
                const matchValue = chooser(array[i] ?? item_3(i, array));
                if (matchValue != null) {
                    return value_2(matchValue);
                }
                else {
                    i_mut = ((i + 1) | 0);
                    continue loop;
                }
            }
            break;
        }
    };
    return loop(0);
}
export function tryPick(chooser, array) {
    const loop = (i_mut) => {
        loop: while (true) {
            const i = i_mut;
            if (i >= array.length) {
                return undefined;
            }
            else {
                const matchValue = chooser(array[i] ?? item_3(i, array));
                if (matchValue == null) {
                    i_mut = ((i + 1) | 0);
                    continue loop;
                }
                else {
                    return matchValue;
                }
            }
            break;
        }
    };
    return loop(0);
}
export function findBack(predicate, array) {
    const loop = (i_mut) => {
        loop: while (true) {
            const i = i_mut;
            if (i < 0) {
                return indexNotFound();
            }
            else if (predicate(array[i] ?? item_3(i, array))) {
                return array[i] ?? item_3(i, array);
            }
            else {
                i_mut = ((i - 1) | 0);
                continue loop;
            }
            break;
        }
    };
    return loop((array.length - 1) | 0);
}
export function tryFindBack(predicate, array) {
    const loop = (i_mut) => {
        loop: while (true) {
            const i = i_mut;
            if (i < 0) {
                return undefined;
            }
            else if (predicate(array[i] ?? item_3(i, array))) {
                return some(array[i] ?? item_3(i, array));
            }
            else {
                i_mut = ((i - 1) | 0);
                continue loop;
            }
            break;
        }
    };
    return loop((array.length - 1) | 0);
}
export function findLastIndex(predicate, array) {
    const loop = (i_mut) => {
        loop: while (true) {
            const i = i_mut;
            if (i < 0) {
                return -1;
            }
            else if (predicate(array[i] ?? item_3(i, array))) {
                return i | 0;
            }
            else {
                i_mut = ((i - 1) | 0);
                continue loop;
            }
            break;
        }
    };
    return loop((array.length - 1) | 0) | 0;
}
export function findIndexBack(predicate, array) {
    const loop = (i_mut) => {
        loop: while (true) {
            const i = i_mut;
            if (i < 0) {
                indexNotFound();
                return -1;
            }
            else if (predicate(array[i] ?? item_3(i, array))) {
                return i | 0;
            }
            else {
                i_mut = ((i - 1) | 0);
                continue loop;
            }
            break;
        }
    };
    return loop((array.length - 1) | 0) | 0;
}
export function tryFindIndexBack(predicate, array) {
    const loop = (i_mut) => {
        loop: while (true) {
            const i = i_mut;
            if (i < 0) {
                return undefined;
            }
            else if (predicate(array[i] ?? item_3(i, array))) {
                return i;
            }
            else {
                i_mut = ((i - 1) | 0);
                continue loop;
            }
            break;
        }
    };
    return loop((array.length - 1) | 0);
}
export function choose(chooser, array, cons) {
    const res = [];
    for (let i = 0; i <= ((array.length - 1) | 0); i++) {
        const matchValue = chooser(array[i] ?? item_3(i, array));
        if (matchValue != null) {
            const y = value_2(matchValue);
            res.push(y);
        }
    }
    if (cons == null) {
        return res;
    }
    else {
        return map((x) => x, res, cons);
    }
}
export function foldIndexed(folder, state, array) {
    return array.reduce(((delegateArg, delegateArg_1, delegateArg_2) => folder(delegateArg_2, delegateArg, delegateArg_1)), state);
}
export function fold(folder, state, array) {
    const folder_1 = folder;
    return array.reduce((folder_1), state);
}
export function iterate(action, array) {
    for (let i = 0; i <= ((array.length - 1) | 0); i++) {
        action(array[i] ?? item_3(i, array));
    }
}
export function iterateIndexed(action, array) {
    for (let i = 0; i <= ((array.length - 1) | 0); i++) {
        action(i, array[i] ?? item_3(i, array));
    }
}
export function iterate2(action, array1, array2) {
    if (array1.length !== array2.length) {
        differentLengths("array1");
    }
    for (let i = 0; i <= ((array1.length - 1) | 0); i++) {
        action(array1[i] ?? item_3(i, array1), array2[i] ?? item_3(i, array2));
    }
}
export function iterateIndexed2(action, array1, array2) {
    if (array1.length !== array2.length) {
        differentLengths("array1");
    }
    for (let i = 0; i <= ((array1.length - 1) | 0); i++) {
        action(i, array1[i] ?? item_3(i, array1), array2[i] ?? item_3(i, array2));
    }
}
export function isEmpty(array) {
    return array.length === 0;
}
export function forAll(predicate, array) {
    return array.every(predicate);
}
export function permute(f, array) {
    const size = array.length | 0;
    const res = array.slice();
    let checkFlags;
    const array_2 = new Array(size);
    checkFlags = (array_2.fill(0, 0, ((0 + size) | 0)));
    iterateIndexed((i, x) => {
        const j = f(i) | 0;
        if ((j < 0) ? true : (j >= size)) {
            throw Operators_InvalidArg("indexMap", "The function did not compute a permutation.");
        }
        setItem_1(res, j, x);
        setItem_1(checkFlags, j, 1);
    }, array);
    if (!(checkFlags.every((y) => (1 === y)))) {
        throw Operators_InvalidArg("indexMap", "The function did not compute a permutation.");
    }
    return res;
}
export function setSlice(target, lower, upper, source) {
    const lower_1 = defaultArg(lower, 0) | 0;
    const upper_1 = defaultArg(upper, -1) | 0;
    const length = (((upper_1 >= 0) ? upper_1 : ((target.length - 1) | 0)) - lower_1) | 0;
    for (let i = 0; i <= length; i++) {
        setItem_1(target, (i + lower_1) | 0, source[i] ?? item_3(i, source));
    }
}
export function sortInPlaceBy(projection, xs, comparer) {
    xs.sort((x, y) => (comparer.Compare(projection(x), projection(y)) | 0));
}
export function sortInPlace(xs, comparer) {
    xs.sort((x, y) => (comparer.Compare(x, y) | 0));
}
export function sort(xs, comparer) {
    const xs_1 = xs.slice();
    xs_1.sort((x, y) => (comparer.Compare(x, y) | 0));
    return xs_1;
}
export function sortBy(projection, xs, comparer) {
    const xs_1 = xs.slice();
    xs_1.sort((x, y) => (comparer.Compare(projection(x), projection(y)) | 0));
    return xs_1;
}
export function sortDescending(xs, comparer) {
    const xs_1 = xs.slice();
    xs_1.sort((x, y) => Math.imul(comparer.Compare(x, y), -1));
    return xs_1;
}
export function sortByDescending(projection, xs, comparer) {
    const xs_1 = xs.slice();
    xs_1.sort((x, y) => Math.imul(comparer.Compare(projection(x), projection(y)), -1));
    return xs_1;
}
export function sortWith(comparer, xs) {
    const comparer_1 = comparer;
    const xs_1 = xs.slice();
    xs_1.sort(comparer_1);
    return xs_1;
}
export function allPairs(xs, ys) {
    const len1 = xs.length | 0;
    const len2 = ys.length | 0;
    const res = new Array(Math.imul(len1, len2));
    for (let i = 0; i <= ((xs.length - 1) | 0); i++) {
        for (let j = 0; j <= ((ys.length - 1) | 0); j++) {
            setItem_1(res, (Math.imul(i, len2) + j) | 0, [xs[i] ?? item_3(i, xs), ys[j] ?? item_3(j, ys)]);
        }
    }
    return res;
}
export function unfold(generator, state) {
    const res = [];
    const loop = (state_1_mut) => {
        loop: while (true) {
            const state_1 = state_1_mut;
            const matchValue = generator(state_1);
            if (matchValue != null) {
                const x = matchValue[0];
                const s = matchValue[1];
                res.push(x);
                state_1_mut = s;
                continue loop;
            }
            break;
        }
    };
    loop(state);
    return res;
}
export function unzip(array) {
    const len = array.length | 0;
    const res1 = new Array(len);
    const res2 = new Array(len);
    iterateIndexed((i, tupledArg) => {
        setItem_1(res1, i, tupledArg[0]);
        setItem_1(res2, i, tupledArg[1]);
    }, array);
    return [res1, res2];
}
export function unzip3(array) {
    const len = array.length | 0;
    const res1 = new Array(len);
    const res2 = new Array(len);
    const res3 = new Array(len);
    iterateIndexed((i, tupledArg) => {
        setItem_1(res1, i, tupledArg[0]);
        setItem_1(res2, i, tupledArg[1]);
        setItem_1(res3, i, tupledArg[2]);
    }, array);
    return [res1, res2, res3];
}
export function zip(array1, array2) {
    if (array1.length !== array2.length) {
        differentLengths("array1");
    }
    const result = new Array(array1.length);
    for (let i = 0; i <= ((array1.length - 1) | 0); i++) {
        setItem_1(result, i, [array1[i] ?? item_3(i, array1), array2[i] ?? item_3(i, array2)]);
    }
    return result;
}
export function zip3(array1, array2, array3) {
    if ((array1.length !== array2.length) ? true : (array2.length !== array3.length)) {
        differentLengths("array1, array2, array3");
    }
    const result = new Array(array1.length);
    for (let i = 0; i <= ((array1.length - 1) | 0); i++) {
        setItem_1(result, i, [array1[i] ?? item_3(i, array1), array2[i] ?? item_3(i, array2), array3[i] ?? item_3(i, array3)]);
    }
    return result;
}
export function chunkBySize(chunkSize, array) {
    if (chunkSize < 1) {
        throw Operators_InvalidArg("chunkSize", "The input must be positive.");
    }
    const result = [];
    if (array.length > 0) {
        const chunks = ~~Math.ceil(array.length / chunkSize);
        for (let x = 0; x <= ((chunks - 1) | 0); x++) {
            let slice;
            const start_1 = Math.imul(x, chunkSize);
            slice = (array.slice(start_1, ((start_1 + chunkSize) | 0)));
            result.push(slice);
        }
    }
    return result;
}
export function splitAt(index, array) {
    if (index < 0) {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    }
    if (index > array.length) {
        throw Operators_InvalidOp(SR_notEnoughElements);
    }
    return [array.slice(0, ((0 + index) | 0)), array.slice(index)];
}
export function compareWith(comparer, source1, source2) {
    if (source1 == null) {
        if (source2 == null) {
            return 0;
        }
        else {
            return -1;
        }
    }
    else if (source2 == null) {
        return 1;
    }
    else {
        const len1 = source1.length | 0;
        const len2 = source2.length | 0;
        const len = ((len1 < len2) ? len1 : len2) | 0;
        let i = 0;
        let res = 0;
        while ((res === 0) && (i < len)) {
            res = (comparer(source1[i] ?? item_3(i, source1), source2[i] ?? item_3(i, source2)) | 0);
            i = ((i + 1) | 0);
        }
        if (res !== 0) {
            return res | 0;
        }
        else if (len1 > len2) {
            return 1;
        }
        else if (len1 < len2) {
            return -1;
        }
        else {
            return 0;
        }
    }
}
export function compareTo(comparer, source1, source2) {
    if (source1 == null) {
        if (source2 == null) {
            return 0;
        }
        else {
            return -1;
        }
    }
    else if (source2 == null) {
        return 1;
    }
    else {
        const len1 = source1.length | 0;
        const len2 = source2.length | 0;
        if (len1 > len2) {
            return 1;
        }
        else if (len1 < len2) {
            return -1;
        }
        else {
            let i = 0;
            let res = 0;
            while ((res === 0) && (i < len1)) {
                res = (comparer(source1[i] ?? item_3(i, source1), source2[i] ?? item_3(i, source2)) | 0);
                i = ((i + 1) | 0);
            }
            return res | 0;
        }
    }
}
export function equalsWith(equals, source1, source2) {
    if (source1 == null) {
        if (source2 == null) {
            return true;
        }
        else {
            return false;
        }
    }
    else if (source2 == null) {
        return false;
    }
    else {
        let i = 0;
        let result = true;
        const length1 = source1.length | 0;
        const length2 = source2.length | 0;
        if (length1 > length2) {
            return false;
        }
        else if (length1 < length2) {
            return false;
        }
        else {
            while ((i < length1) && result) {
                result = equals(source1[i] ?? item_3(i, source1), source2[i] ?? item_3(i, source2));
                i = ((i + 1) | 0);
            }
            return result;
        }
    }
}
export function exactlyOne(array) {
    switch (array.length) {
        case 1:
            return array[0] ?? item_3(0, array);
        case 0:
            throw Operators_InvalidArg("array", "The input sequence was empty");
        default:
            throw Operators_InvalidArg("array", "Input array too long");
    }
}
export function tryExactlyOne(array) {
    if (array.length === 1) {
        return some(array[0] ?? item_3(0, array));
    }
    else {
        return undefined;
    }
}
export function head(array) {
    if (array.length === 0) {
        throw Operators_InvalidArg("array", "The input array was empty");
    }
    else {
        return array[0] ?? item_3(0, array);
    }
}
export function tryHead(array) {
    if (array.length === 0) {
        return undefined;
    }
    else {
        return some(array[0] ?? item_3(0, array));
    }
}
export function tail(array) {
    if (array.length === 0) {
        throw Operators_InvalidArg("array", "Not enough elements");
    }
    return array.slice(1);
}
export function item(index, array) {
    const value = array[index];
    if ((value === undefined) && ((index < 0) ? true : (index >= array.length))) {
        throw Operators_InvalidArg("index", "Index was outside the bounds of the array.");
    }
    else {
        return value;
    }
}
export function setItem(array, index, value) {
    if ((index < 0) ? true : (index >= array.length)) {
        throw Operators_InvalidArg("index", "Index was outside the bounds of the array.");
    }
    else {
        array[index] = value;
    }
}
export function tryItem(index, array) {
    if ((index < 0) ? true : (index >= array.length)) {
        return undefined;
    }
    else {
        return some(array[index]);
    }
}
export function foldBackIndexed(folder, array, state) {
    return array.reduceRight(((delegateArg, delegateArg_1, delegateArg_2) => folder(delegateArg_2, delegateArg_1, delegateArg)), state);
}
export function foldBack(folder, array, state) {
    return array.reduceRight(((delegateArg, delegateArg_1) => folder(delegateArg_1, delegateArg)), state);
}
export function foldIndexed2(folder, state, array1, array2) {
    let acc = state;
    if (array1.length !== array2.length) {
        differentLengths("array1");
    }
    for (let i = 0; i <= ((array1.length - 1) | 0); i++) {
        acc = folder(i, acc, array1[i] ?? item_3(i, array1), array2[i] ?? item_3(i, array2));
    }
    return acc;
}
export function fold2(folder, state, array1, array2) {
    return foldIndexed2((_arg, acc, x, y) => folder(acc, x, y), state, array1, array2);
}
export function foldBackIndexed2(folder, array1, array2, state) {
    let acc = state;
    if (array1.length !== array2.length) {
        differentLengths("array1");
    }
    const size = array1.length | 0;
    for (let i = 1; i <= size; i++) {
        acc = folder((i - 1) | 0, item_3((size - i) | 0, array1), item_3((size - i) | 0, array2), acc);
    }
    return acc;
}
export function foldBack2(f, array1, array2, state) {
    return foldBackIndexed2((_arg, x, y, acc) => f(x, y, acc), array1, array2, state);
}
export function reduce(reduction, array) {
    if (array.length === 0) {
        throw Operators_InvalidArg("array", "The input array was empty");
    }
    const reduction_1 = reduction;
    return array.reduce(reduction_1);
}
export function reduceBack(reduction, array) {
    if (array.length === 0) {
        throw Operators_InvalidArg("array", "The input array was empty");
    }
    const reduction_1 = reduction;
    return array.reduceRight(reduction_1);
}
export function forAll2(predicate, array1, array2) {
    return fold2((acc, x, y) => (acc && predicate(x, y)), true, array1, array2);
}
export function existsOffset(predicate_mut, array_mut, index_mut) {
    existsOffset: while (true) {
        const predicate = predicate_mut, array = array_mut, index = index_mut;
        if (index === array.length) {
            return false;
        }
        else if (predicate(array[index] ?? item_3(index, array))) {
            return true;
        }
        else {
            predicate_mut = predicate;
            array_mut = array;
            index_mut = ((index + 1) | 0);
            continue existsOffset;
        }
        break;
    }
}
export function exists(predicate, array) {
    return existsOffset(predicate, array, 0);
}
export function existsOffset2(predicate_mut, array1_mut, array2_mut, index_mut) {
    existsOffset2: while (true) {
        const predicate = predicate_mut, array1 = array1_mut, array2 = array2_mut, index = index_mut;
        if (index === array1.length) {
            return false;
        }
        else if (predicate(array1[index] ?? item_3(index, array1), array2[index] ?? item_3(index, array2))) {
            return true;
        }
        else {
            predicate_mut = predicate;
            array1_mut = array1;
            array2_mut = array2;
            index_mut = ((index + 1) | 0);
            continue existsOffset2;
        }
        break;
    }
}
export function exists2(predicate, array1, array2) {
    if (array1.length !== array2.length) {
        differentLengths("array1");
    }
    return existsOffset2(predicate, array1, array2, 0);
}
export function sum(array, adder) {
    let acc = adder.GetZero();
    for (let i = 0; i <= ((array.length - 1) | 0); i++) {
        acc = adder.Add(acc, array[i] ?? item_3(i, array));
    }
    return acc;
}
export function sumBy(projection, array, adder) {
    let acc = adder.GetZero();
    for (let i = 0; i <= ((array.length - 1) | 0); i++) {
        acc = adder.Add(acc, projection(array[i] ?? item_3(i, array)));
    }
    return acc;
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
export function average(array, averager) {
    if (array.length === 0) {
        throw Operators_InvalidArg("array", "The input array was empty");
    }
    let total = averager.GetZero();
    for (let i = 0; i <= ((array.length - 1) | 0); i++) {
        total = averager.Add(total, array[i] ?? item_3(i, array));
    }
    return averager.DivideByInt(total, array.length);
}
export function averageBy(projection, array, averager) {
    if (array.length === 0) {
        throw Operators_InvalidArg("array", "The input array was empty");
    }
    let total = averager.GetZero();
    for (let i = 0; i <= ((array.length - 1) | 0); i++) {
        total = averager.Add(total, projection(array[i] ?? item_3(i, array)));
    }
    return averager.DivideByInt(total, array.length);
}
export function windowed(windowSize, source) {
    if (windowSize <= 0) {
        throw Operators_InvalidArg("windowSize", "The input must be positive.");
    }
    let res;
    const len = max_1(0, ((source.length - windowSize) + 1) | 0) | 0;
    res = (new Array(len));
    for (let i = windowSize; i <= source.length; i++) {
        setItem_1(res, (i - windowSize) | 0, source.slice(Math.max((i - windowSize) | 0, 0), Math.max(((i - 1) | 0) + 1, 0)));
    }
    return res;
}
export function splitInto(chunks, array) {
    if (chunks < 1) {
        throw Operators_InvalidArg("count", "The input must be positive.");
    }
    const result = [];
    if (array.length > 0) {
        const chunks_1 = min_1(chunks, array.length) | 0;
        const minChunkSize = ~~(array.length / chunks_1);
        const chunksWithExtraItem = (array.length % chunks_1) | 0;
        for (let i = 0; i <= ((chunks_1 - 1) | 0); i++) {
            const chunkSize = ((i < chunksWithExtraItem) ? ((minChunkSize + 1) | 0) : minChunkSize) | 0;
            let slice;
            const start_1 = (Math.imul(i, minChunkSize) + min_1(chunksWithExtraItem, i)) | 0;
            slice = (array.slice(start_1, ((start_1 + chunkSize) | 0)));
            result.push(slice);
        }
    }
    return result;
}
export function transpose(arrays, cons) {
    const arrays_1 = Array.isArray(arrays) ? arrays : (Array.from(arrays));
    const len = arrays_1.length | 0;
    if (len === 0) {
        return new Array(0);
    }
    else {
        const firstArray = arrays_1[0] ?? item_3(0, arrays_1);
        const lenInner = firstArray.length | 0;
        if (!forAll((a) => (a.length === lenInner), arrays_1)) {
            differentLengths("array.[0]");
        }
        const result = new Array(lenInner);
        for (let i = 0; i <= ((lenInner - 1) | 0); i++) {
            setItem_1(result, i, Helpers_allocateArrayFromCons(cons, len));
            for (let j = 0; j <= ((len - 1) | 0); j++) {
                (result[i] ?? item_3(i, result))[j] = item_3(i, arrays_1[j] ?? item_3(j, arrays_1));
            }
        }
        return result;
    }
}
export function insertAt(index, y, xs, cons) {
    const len = xs.length | 0;
    if ((index < 0) ? true : (index > len)) {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    }
    const target = Helpers_allocateArrayFromCons(cons, (len + 1) | 0);
    for (let i = 0; i <= ((index - 1) | 0); i++) {
        setItem_1(target, i, xs[i] ?? item_3(i, xs));
    }
    setItem_1(target, index, y);
    for (let i_1 = index; i_1 <= ((len - 1) | 0); i_1++) {
        setItem_1(target, (i_1 + 1) | 0, xs[i_1] ?? item_3(i_1, xs));
    }
    return target;
}
export function insertManyAt(index, ys, xs, cons) {
    const len = xs.length | 0;
    if ((index < 0) ? true : (index > len)) {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    }
    const ys_1 = Array.from(ys);
    const len2 = ys_1.length | 0;
    const target = Helpers_allocateArrayFromCons(cons, (len + len2) | 0);
    for (let i = 0; i <= ((index - 1) | 0); i++) {
        setItem_1(target, i, xs[i] ?? item_3(i, xs));
    }
    for (let i_1 = 0; i_1 <= ((len2 - 1) | 0); i_1++) {
        setItem_1(target, (index + i_1) | 0, ys_1[i_1] ?? item_3(i_1, ys_1));
    }
    for (let i_2 = index; i_2 <= ((len - 1) | 0); i_2++) {
        setItem_1(target, (i_2 + len2) | 0, xs[i_2] ?? item_3(i_2, xs));
    }
    return target;
}
function randomNext(randomizer, minValue, maxValue) {
    const r = randomizer();
    if (!((r >= 0) && (r < 1))) {
        throw ArgumentOutOfRangeException_$ctor_Z384F8060("randomizer", ("The index is outside the legal range.\nrandomizer returned " + float64ToString(r)) + ", should be in range [0.0, 1.0).");
    }
    return (~~(r * ((maxValue - minValue) | 0)) + minValue) | 0;
}
function checkRandomCount(count) {
    if (count < 0) {
        throw Operators_InvalidArg("count", (SR_inputMustBeNonNegative + "\ncount = ") + int32ToString(count));
    }
}
function shuffleInPlace(next, xs) {
    const len = xs.length | 0;
    for (let i = 0; i <= ((len - 2) | 0); i++) {
        const j = next(i, len) | 0;
        if (j !== i) {
            const tmp = xs[i] ?? item_3(i, xs);
            setItem_1(xs, i, xs[j] ?? item_3(j, xs));
            setItem_1(xs, j, tmp);
        }
    }
}
export function randomShuffleInPlaceBy(randomizer, xs) {
    shuffleInPlace((minValue, maxValue) => (randomNext(randomizer, minValue, maxValue) | 0), xs);
}
export function randomShuffleInPlaceWith(random, xs) {
    shuffleInPlace((i, n) => (random.Next2(i, n) | 0), xs);
}
export function randomShuffleInPlace(xs) {
    randomShuffleInPlaceWith(nonSeeded(), xs);
}
export function randomShuffleBy(randomizer, xs) {
    const arr = copy(xs);
    randomShuffleInPlaceBy(randomizer, arr);
    return arr;
}
export function randomShuffleWith(random, xs) {
    const arr = copy(xs);
    randomShuffleInPlaceWith(random, arr);
    return arr;
}
export function randomShuffle(xs) {
    return randomShuffleWith(nonSeeded(), xs);
}
export function randomChoiceBy(randomizer, xs) {
    if (isEmpty(xs)) {
        throw Operators_InvalidArg("source", "The input array was empty.");
    }
    return item_3(randomNext(randomizer, 0, xs.length), xs);
}
export function randomChoiceWith(random, xs) {
    return randomChoiceBy(() => random.NextDouble(), xs);
}
export function randomChoice(xs) {
    return randomChoiceWith(nonSeeded(), xs);
}
export function randomChoicesBy(randomizer, count, xs, cons) {
    checkRandomCount(count);
    if ((count > 0) && isEmpty(xs)) {
        throw Operators_InvalidArg("source", SR_inputSequenceEmpty);
    }
    const len = xs.length | 0;
    return initialize(count, (_arg) => item_3(randomNext(randomizer, 0, len), xs), cons);
}
export function randomChoicesWith(random, count, xs, cons) {
    return randomChoicesBy(() => random.NextDouble(), count, xs, cons);
}
export function randomChoices(count, xs, cons) {
    return randomChoicesWith(nonSeeded(), count, xs, cons);
}
function getMaxSetSizeForSampling(count) {
    let arg1__1 = undefined;
    if (count > 5) {
        return (21 + ~~((arg1__1 = Math.ceil(Math.log(Math.imul(count, 3)) / Math.log(4)), Math.pow(4, arg1__1)))) | 0;
    }
    else {
        return 21;
    }
}
function randomSampleWithNext(next, count, xs) {
    checkRandomCount(count);
    const len = xs.length | 0;
    if ((len === 0) && (count > 0)) {
        throw Operators_InvalidArg("source", SR_inputSequenceEmpty);
    }
    if (count > len) {
        throw Operators_InvalidArg("count", SR_notEnoughElements);
    }
    const result = getSubArray(xs, 0, count);
    if (len <= getMaxSetSizeForSampling(count)) {
        const pool = copy(xs);
        for (let i = 0; i <= ((count - 1) | 0); i++) {
            const j = next(0, (len - i) | 0) | 0;
            setItem_1(result, i, pool[j] ?? item_3(j, pool));
            setItem_1(pool, j, item_3(((len - i) - 1) | 0, pool));
        }
    }
    else {
        const selected = new Set([]);
        for (let i_1 = 0; i_1 <= ((count - 1) | 0); i_1++) {
            let j_1 = next(0, len);
            while (!addToSet(j_1, selected)) {
                j_1 = (next(0, len) | 0);
            }
            setItem_1(result, i_1, xs[j_1] ?? item_3(j_1, xs));
        }
    }
    return result;
}
export function randomSampleBy(randomizer, count, xs) {
    return randomSampleWithNext((minValue, maxValue) => (randomNext(randomizer, minValue, maxValue) | 0), count, xs);
}
export function randomSampleWith(random, count, xs) {
    return randomSampleWithNext((a, b) => (random.Next2(a, b) | 0), count, xs);
}
export function randomSample(count, xs) {
    return randomSampleWith(nonSeeded(), count, xs);
}
export function removeAt(index, xs) {
    if ((index < 0) ? true : (index >= xs.length)) {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    }
    let i = -1;
    return filter((_arg) => {
        i = ((i + 1) | 0);
        return i !== index;
    }, xs);
}
export function removeManyAt(index, count, xs) {
    if (((index < 0) ? true : (index > ((xs.length - count) | 0))) ? true : (index > xs.length)) {
        throw Operators_InvalidArg("index", "index must be within bounds of the array");
    }
    let i = -1;
    return filter((_arg) => {
        i = ((i + 1) | 0);
        if (i < index) {
            return true;
        }
        else {
            return i >= ((index + count) | 0);
        }
    }, xs);
}
export function updateAt(index, y, xs, cons) {
    const len = xs.length | 0;
    if ((index < 0) ? true : (index >= len)) {
        throw Operators_InvalidArg("index", SR_indexOutOfBounds);
    }
    const target = Helpers_allocateArrayFromCons(cons, len);
    for (let i = 0; i <= ((len - 1) | 0); i++) {
        setItem_1(target, i, (i === index) ? y : (xs[i] ?? item_3(i, xs)));
    }
    return target;
}
export function resize(xs, newSize, zero, cons) {
    let array = undefined, array_1 = undefined, start_2 = undefined, count_2 = undefined;
    if (newSize < 0) {
        throw Operators_InvalidArg("newSize", "The input must be non-negative.");
    }
    const zero_1 = defaultArg(zero, defaultOf());
    if (xs.contents == null) {
        xs.contents = ((array = Helpers_allocateArrayFromCons(cons, newSize), array.fill(zero_1, 0, ((0 + newSize) | 0))));
    }
    else {
        const len = xs.contents.length | 0;
        if (newSize < len) {
            xs.contents = ((array_1 = xs.contents, array_1.slice(0, ((0 + newSize) | 0))));
        }
        else if (newSize > len) {
            const target = Helpers_allocateArrayFromCons(cons, newSize);
            if (len > 0) {
                copyTo(xs.contents, 0, target, 0, len);
            }
            xs.contents = ((start_2 = (len | 0), (count_2 = ((newSize - len) | 0), target.fill(zero_1, start_2, ((start_2 + count_2) | 0)))));
        }
    }
}
