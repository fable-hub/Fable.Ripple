import { Exception, equals, isArrayLike } from "./Util.js";
import { ArgumentException_$ctor_Z384F8060, ArgumentOutOfRangeException_$ctor_Z384F8060 } from "./System.js";
export function count(col) {
    if (typeof col["System.Collections.Generic.ICollection`1.get_Count"] === "function") {
        return col["System.Collections.Generic.ICollection`1.get_Count"](); // collection
    }
    else {
        if (typeof col["System.Collections.Generic.IReadOnlyCollection`1.get_Count"] === "function") {
            return col["System.Collections.Generic.IReadOnlyCollection`1.get_Count"](); // collection
        }
        else {
            if (isArrayLike(col)) {
                return col.length; // array, resize array
            }
            else {
                if (typeof col.size === "number") {
                    return col.size; // map, set
                }
                else {
                    let count = 0;
                    for (const _ of col) {
                        count++;
                    }
                    return count; // other collections
                }
            }
        }
    }
}
export function item(col, index) {
    if (typeof col["System.Collections.Generic.IReadOnlyList`1.get_ItemZ524259A4"] === "function") {
        return col["System.Collections.Generic.IReadOnlyList`1.get_ItemZ524259A4"](index); // collection
    }
    else {
        const xs = col;
        if (isArrayLike(xs)) {
            // A GroupCollection is a RegExpExecArray, an index out of range is an unsuccessful group
            if ((index >= 0 && index < xs.length) || "input" in xs) {
                return xs[index]; // array, resize array
            }
            throw ArgumentOutOfRangeException_$ctor_Z384F8060("index", "Index was out of range. Must be non-negative and less than the size of the collection.");
        }
        else {
            let i = 0;
            for (const v of xs) {
                if (i === index) {
                    return v; // other collections
                }
                i++;
            }
            throw ArgumentException_$ctor_Z384F8060("The index was outside the range of elements in the list.", "n");
        }
    }
}
export function isReadOnly(col) {
    if (typeof col["System.Collections.Generic.ICollection`1.get_IsReadOnly"] === "function") {
        return col["System.Collections.Generic.ICollection`1.get_IsReadOnly"](); // collection
    }
    else {
        if (isArrayLike(col)) {
            return false; // array, resize array
        }
        else {
            if (typeof col.size === "number") {
                return false; // map, set
            }
            else {
                return true; // other collections
            }
        }
    }
}
export function copyTo(col, array, arrayIndex) {
    if (typeof col["System.Collections.Generic.ICollection`1.CopyToZ3B4C077E"] === "function") {
        col["System.Collections.Generic.ICollection`1.CopyToZ3B4C077E"](array, arrayIndex); // collection
    }
    else {
        let i = arrayIndex;
        for (const v of col) {
            array[i] = v;
            i++;
        }
    }
}
export function contains(col, item) {
    if (typeof col["System.Collections.Generic.ICollection`1.Contains2B595"] === "function") {
        return col["System.Collections.Generic.ICollection`1.Contains2B595"](item); // collection
    }
    else {
        if (isArrayLike(col)) {
            let i = col.findIndex(x => equals(x, item)); // array, resize array
            return i >= 0;
        }
        else {
            if (typeof col.has === "function") {
                if (typeof col.set === "function" && isArrayLike(item)) {
                    return col.has(item[0]) && equals(col.get(item[0]), item[1]); // map
                }
                else {
                    return col.has(item); // set
                }
            }
            else {
                return false; // other collections
            }
        }
    }
}
export function add(col, item) {
    if (typeof col["System.Collections.Generic.ICollection`1.Add2B595"] === "function") {
        return col["System.Collections.Generic.ICollection`1.Add2B595"](item); // collection
    }
    else {
        if (isArrayLike(col)) {
            if (ArrayBuffer.isView(col)) {
                // TODO: throw for typed arrays?
            }
            else {
                col.push(item); // array, resize array
            }
        }
        else {
            if (typeof col.add === "function") {
                return col.add(item); // set
            }
            else {
                if (typeof col.has === "function"
                    && typeof col.set === "function"
                    && isArrayLike(item)) {
                    if (col.has(item[0]) === false) {
                        col.set(item[0], item[1]); // map
                    }
                    else {
                        throw new Exception("An item with the same key has already been added. Key: " + item[0]);
                    }
                }
                else {
                    // TODO: throw for other collections?
                }
            }
        }
    }
}
export function remove(col, item) {
    if (typeof col["System.Collections.Generic.ICollection`1.Remove2B595"] === "function") {
        return col["System.Collections.Generic.ICollection`1.Remove2B595"](item); // collection
    }
    else {
        if (isArrayLike(col)) {
            if (ArrayBuffer.isView(col)) {
                // TODO: throw for typed arrays
                return false;
            }
            else {
                let i = col.findIndex(x => equals(x, item));
                if (i >= 0) {
                    col.splice(i, 1); // array, resize array
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        else {
            if (typeof col.delete === "function") {
                if (typeof col.set === "function" && isArrayLike(item)) {
                    if (col.has(item[0]) && equals(col.get(item[0]), item[1])) {
                        return col.delete(item[0]); // map
                    }
                    else {
                        return false;
                    }
                }
                else {
                    return col.delete(item); // set
                }
            }
            else {
                // TODO: throw for other collections?
                return false; // other collections
            }
        }
    }
}
export function clear(col) {
    if (typeof col["System.Collections.Generic.ICollection`1.Clear"] === "function") {
        return col["System.Collections.Generic.ICollection`1.Clear"](); // collection
    }
    else if (Array.isArray(col)) {
        col.length = 0;
    }
    else if (ArrayBuffer.isView(col)) {
        // TODO: throw for typed arrays?
    }
    else if (typeof col.clear === "function") {
        col.clear(); // map, set
    }
    else {
        // TODO: throw for other collections?
    }
}
