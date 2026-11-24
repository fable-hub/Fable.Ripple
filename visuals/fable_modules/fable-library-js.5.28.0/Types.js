import { Exception, combineHashCode, compare, compareArrays, equalArrays, equals, sameConstructor, numberHash, structuralHash } from "./Util.js";
export function seqToString(self) {
    let count = 0;
    let str = "[";
    for (const x of self) {
        if (count === 0) {
            str += toStringQuoted(x);
        }
        else if (count === 100) {
            str += "; ...";
            break;
        }
        else {
            str += "; " + toStringQuoted(x);
        }
        count++;
    }
    return str + "]";
}
// Structured (%A-style) rendering of a value used as an element/field of a
// container. F#'s structured formatting (which records, unions, lists, etc.
// use in their ToString) quotes strings, e.g. `["a"; "b"]` and `{ Name = "John" }`.
function toStringQuoted(x, callStack = 0) {
    return typeof x === "string" ? "\"" + x + "\"" : toString(x, callStack);
}
export function toString(x, callStack = 0) {
    if (x != null && typeof x === "object") {
        if (typeof x.toString === "function" && x.toString !== Object.prototype.toString) {
            return x.toString();
        }
        else if (Symbol.iterator in x) {
            return seqToString(x);
        }
        else { // TODO: Date?
            const cons = Object.getPrototypeOf(x)?.constructor;
            return cons === Object && callStack < 10
                // Same format as recordToString
                ? "{ " + Object.entries(x).map(([k, v]) => k + " = " + toStringQuoted(v, callStack + 1)).join("\n  ") + " }"
                : cons?.name ?? "";
        }
    }
    return String(x);
}
export function toStringOrEmpty(x) {
    return x == null ? "" : toString(x);
}
export function unionToString(name, fields) {
    if (fields.length === 0) {
        return name;
    }
    else {
        let fieldStr;
        let withParens = true;
        if (fields.length === 1) {
            fieldStr = toStringQuoted(fields[0]);
            withParens = fieldStr.indexOf(" ") >= 0;
        }
        else {
            fieldStr = fields.map((x) => toStringQuoted(x)).join(", ");
        }
        return name + (withParens ? " (" : " ") + fieldStr + (withParens ? ")" : "");
    }
}
export class Union {
    get name() {
        return this.cases()[this.tag];
    }
    toJSON() {
        return this.fields.length === 0 ? this.name : [this.name].concat(this.fields);
    }
    toString() {
        return unionToString(this.name, this.fields);
    }
    GetHashCode() {
        let h = combineHashCode(0, numberHash(this.tag));
        for (let i = 0; i < this.fields.length; i++) {
            h = combineHashCode(h, structuralHash(this.fields[i]));
        }
        return h;
    }
    Equals(other) {
        if (this === other) {
            return true;
        }
        else if (!sameConstructor(this, other)) {
            return false;
        }
        else if (this.tag === other.tag) {
            return equalArrays(this.fields, other.fields);
        }
        else {
            return false;
        }
    }
    CompareTo(other) {
        if (this === other) {
            return 0;
        }
        else if (!sameConstructor(this, other)) {
            return -1;
        }
        else if (this.tag === other.tag) {
            return compareArrays(this.fields, other.fields);
        }
        else {
            return this.tag < other.tag ? -1 : 1;
        }
    }
}
function recordToJSON(self) {
    const o = {};
    const keys = Object.keys(self);
    for (let i = 0; i < keys.length; i++) {
        o[keys[i]] = self[keys[i]];
    }
    return o;
}
const recordsBeingFormatted = new Set();
// FSharp.Core prints "..." for a value that is already being formatted
function recordToString(self) {
    if (recordsBeingFormatted.has(self)) {
        return "...";
    }
    recordsBeingFormatted.add(self);
    try {
        return "{ " + Object.entries(self).map(([k, v]) => k + " = " + toStringQuoted(v)).join("\n  ") + " }";
    }
    finally {
        recordsBeingFormatted.delete(self);
    }
}
function recordGetHashCode(self) {
    // Snapshot all values before calling any user-defined hash functions.
    const values = Object.values(self);
    let hash = 0;
    for (let i = 0; i < values.length; i++) {
        hash = combineHashCode(hash, structuralHash(values[i]));
    }
    return hash;
}
function recordEquals(self, other) {
    if (self === other) {
        return true;
    }
    else if (!sameConstructor(self, other)) {
        return false;
    }
    else {
        const thisNames = Object.keys(self);
        for (let i = 0; i < thisNames.length; i++) {
            if (!equals(self[thisNames[i]], other[thisNames[i]])) {
                return false;
            }
        }
        return true;
    }
}
function recordCompareTo(self, other) {
    if (self === other) {
        return 0;
    }
    else if (!sameConstructor(self, other)) {
        return -1;
    }
    else {
        const thisNames = Object.keys(self);
        for (let i = 0; i < thisNames.length; i++) {
            const result = compare(self[thisNames[i]], other[thisNames[i]]);
            if (result !== 0) {
                return result;
            }
        }
        return 0;
    }
}
export class Record {
    toJSON() { return recordToJSON(this); }
    toString() { return recordToString(this); }
    GetHashCode() { return recordGetHashCode(this); }
    Equals(other) { return recordEquals(this, other); }
    CompareTo(other) { return recordCompareTo(this, other); }
}
export class FSharpRef {
    get contents() {
        return this.getter === undefined ? this.value : this.getter();
    }
    set contents(v) {
        if (this.setter === undefined) {
            this.value = v;
        }
        else {
            this.setter(v);
        }
    }
    // Same field layout on both paths so every cell shares one hidden class
    constructor(contentsOrGetter, setter) {
        if (typeof setter === "function") {
            this.getter = contentsOrGetter;
            this.setter = setter;
            this.value = undefined;
        }
        else {
            this.getter = undefined;
            this.setter = undefined;
            this.value = contentsOrGetter;
        }
    }
}
// EXCEPTIONS
export class FSharpException extends Exception {
    toJSON() { return recordToJSON(this); }
    toString() { return recordToString(this); }
    GetHashCode() { return recordGetHashCode(this); }
    Equals(other) { return recordEquals(this, other); }
    CompareTo(other) { return recordCompareTo(this, other); }
}
export class MatchFailureException extends FSharpException {
    constructor(arg1, arg2, arg3) {
        super();
        this.arg1 = arg1;
        this.arg2 = arg2 | 0;
        this.arg3 = arg3 | 0;
        this.message = "The match cases were incomplete";
    }
}
export class Attribute {
}
