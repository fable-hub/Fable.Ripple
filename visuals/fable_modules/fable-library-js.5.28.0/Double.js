import { FSharpRef } from "./Types.js";
import { ArgumentNullException_$ctor_Z721C83C5, FormatException_$ctor_Z721C83C5 } from "./System.js";
import { NumberStyles, splitNumber } from "./NumberStyles.js";
// .NET's NaN and infinity symbols, ignoring case, allowing a sign and trimmed like String.Trim
const symbolRegex = /^[\t-\r \u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]*(?:[+-]?nan|([+-]?)infinity)[\t-\r \u0085\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]*$/i;
const defaultStyle = NumberStyles.Float | NumberStyles.AllowThousands;
export function tryParse(str, style, defValue) {
    if (str != null) {
        const parts = splitNumber(str, style);
        if (parts !== undefined) {
            const value = Number.parseFloat(parts.digits);
            defValue.contents = parts.negative ? -value : value;
            return true;
        }
        const symbol = symbolRegex.exec(str);
        if (symbol != null) {
            defValue.contents = symbol[1] == null ? NaN : symbol[1] === "-" ? -Infinity : Infinity;
            return true;
        }
    }
    defValue.contents = 0;
    return false;
}
export function parse(str, style = defaultStyle) {
    const defValue = new FSharpRef(0);
    if (tryParse(str, style, defValue)) {
        return defValue.contents;
    }
    else if (str == null) {
        throw ArgumentNullException_$ctor_Z721C83C5("s");
    }
    else {
        throw FormatException_$ctor_Z721C83C5(`The input string '${str}' was not in a correct format.`);
    }
}
export function tryParseSingle(str, style, defValue) {
    if (tryParse(str, style, defValue)) {
        defValue.contents = Math.fround(defValue.contents);
        return true;
    }
    return false;
}
export function parseSingle(str, style) {
    return Math.fround(parse(str, style));
}
export function parseIgnoringUnderscores(str) {
    return parse(str == null ? str : str.replace(/_/g, ""), NumberStyles.Float);
}
// JS Number.isFinite function evals false for NaN
export function isPositiveInfinity(x) {
    return x === Number.POSITIVE_INFINITY;
}
export function isNegativeInfinity(x) {
    return x === Number.NEGATIVE_INFINITY;
}
export function isInfinity(x) {
    return x === Number.POSITIVE_INFINITY || x === Number.NEGATIVE_INFINITY;
}
export function max(x, y) {
    return Math.max(x, y);
}
export function min(x, y) {
    return Math.min(x, y);
}
export function maxMagnitude(x, y) {
    if (Number.isNaN(x) || Number.isNaN(y)) {
        return NaN;
    }
    return Math.abs(x) > Math.abs(y) ? x : y;
}
export function minMagnitude(x, y) {
    if (Number.isNaN(x) || Number.isNaN(y)) {
        return NaN;
    }
    return Math.abs(x) < Math.abs(y) ? x : y;
}
export function clamp(x, min, max) {
    return x < min ? min : x > max ? max : x;
}
