import { Exception } from "./Util.js";
import { integerDigits, invalidStyleMessage, NumberStyles, radixStyles, splitNumber } from "./NumberStyles.js";
import { ArgumentException_$ctor_Z721C83C5, ArgumentNullException_$ctor_Z721C83C5, DivideByZeroException_$ctor, FormatException_$ctor_Z721C83C5, OverflowException_$ctor, OverflowException_$ctor_Z721C83C5 } from "./System.js";
function validResponse(regexMatch, radix) {
    const [/*all*/ , sign, prefix, digits] = regexMatch;
    return {
        sign: sign || "",
        prefix: prefix || "",
        digits,
        radix,
    };
}
function getRange(unsigned, bitsize) {
    switch (bitsize) {
        case 8: return unsigned ? [0, 255] : [-128, 127];
        case 16: return unsigned ? [0, 65535] : [-32768, 32767];
        case 32: return unsigned ? [0, 4294967295] : [-2147483648, 2147483647];
        default: throw new Exception("Invalid bit size.");
    }
}
function getInvalidDigits(radix) {
    switch (radix) {
        case 2: return /[^0-1]/;
        case 8: return /[^0-7]/;
        case 10: return /[^0-9]/;
        case 16: return /[^0-9a-fA-F]/;
        default:
            throw new Exception("Invalid Base.");
    }
}
export function getPrefix(radix) {
    switch (radix) {
        case 2: return "0b";
        case 8: return "0o";
        case 10: return "";
        case 16: return "0x";
        default: return "";
    }
}
function getRadix(prefix) {
    switch (prefix) {
        case "0b":
        case "0B": return 2;
        case "0o":
        case "0O": return 8;
        case "0x":
        case "0X": return 16;
        default: return 10;
    }
}
// FSharp.Core's grammar for the string conversions (int, int64, ...), or digits in the given radix for Convert.ToInt32(s, radix)
export function splitFSharpInteger(str, radix) {
    const integerRegex = /^\s*([\+\-])?(0[xXoObB])?([0-9a-fA-F]+)\s*$/;
    const res = integerRegex.exec(str.replace(/_/g, ""));
    if (res != null) {
        const [/*all*/ , sign, prefix, digits] = res;
        // FSharp.Core reads only '-' before a radix specifier, but Convert.ToInt32(s, 16) also accepts "+0x".
        if (radix == null && sign === "+" && prefix != null) {
            return null;
        }
        radix = radix || getRadix(prefix);
        const invalidDigits = getInvalidDigits(radix);
        if (!invalidDigits.test(digits)) {
            return validResponse(res, radix);
        }
    }
    return null;
}
// The digits of a string parsed with NumberStyles.AllowHexSpecifier or AllowBinarySpecifier
export function splitRadixDigits(str, style) {
    const radix = style & NumberStyles.AllowHexSpecifier ? 16 : 2;
    const digit = radix === 16 ? "[0-9a-fA-F]" : "[01]";
    const leading = style & NumberStyles.AllowLeadingWhite ? "[\\t-\\r ]*" : "";
    const trailing = style & NumberStyles.AllowTrailingWhite ? "[\\t-\\r ]*" : "";
    const res = new RegExp(`^${leading}(${digit}+)${trailing}\\0*$`).exec(str);
    return res != null ? { digits: res[1], radix } : null;
}
export function checkIntegerStyle(style) {
    const message = invalidStyleMessage(style, true);
    if (message !== undefined) {
        throw ArgumentException_$ctor_Z721C83C5(message);
    }
}
export function parseOverflow(unsigned, bitsize) {
    const type = bitsize === 8 ? (unsigned ? "an unsigned byte" : "a signed byte") : (unsigned ? "a UInt" : "an Int") + bitsize;
    return OverflowException_$ctor_Z721C83C5(`Value was either too large or too small for ${type}.`);
}
export function absOverflow() {
    return OverflowException_$ctor_Z721C83C5("Negating the minimum value of a twos complement number is invalid.");
}
export function abs(x, bitsize) {
    if (x === -(2 ** (bitsize - 1))) {
        throw absOverflow();
    }
    return Math.abs(x);
}
export function pown(x, n, unsigned, bitsize) {
    let res = 1;
    let e = Math.abs(n);
    while (e > 0) {
        if (e % 2 === 1) {
            res *= x;
        }
        e = Math.floor(e / 2);
        if (e > 0) {
            x *= x;
        }
    }
    const min = unsigned ? 0 : -(2 ** (bitsize - 1));
    const max = unsigned ? 2 ** bitsize - 1 : 2 ** (bitsize - 1) - 1;
    if (res < min || res > max) {
        throw OverflowException_$ctor();
    }
    if (n >= 0) {
        return res;
    }
    if (res === 0) {
        throw DivideByZeroException_$ctor();
    }
    return ~~(1 / res);
}
function formatError(str) {
    return FormatException_$ctor_Z721C83C5(`The input string '${str}' was not in a correct format.`);
}
function checkRange(v, radix, unsigned, bitsize) {
    const [umin, umax] = getRange(true, bitsize);
    if (!unsigned && radix !== 10 && v >= umin && v <= umax) {
        v = v << (32 - bitsize) >> (32 - bitsize);
    }
    const [min, max] = getRange(unsigned, bitsize);
    if (v >= min && v <= max) {
        return v;
    }
    throw parseOverflow(unsigned, bitsize);
}
export function fromString(str, unsigned, bitsize, radix) {
    if (str == null) {
        throw ArgumentNullException_$ctor_Z721C83C5("s");
    }
    const res = splitFSharpInteger(str, radix);
    if (res != null) {
        const v = Number.parseInt(res.sign + res.digits, res.radix);
        if (!Number.isNaN(v)) {
            return checkRange(v, res.radix, unsigned, bitsize);
        }
    }
    throw formatError(str);
}
export function parse(str, style, unsigned, bitsize) {
    if (str == null) {
        throw ArgumentNullException_$ctor_Z721C83C5("s");
    }
    checkIntegerStyle(style);
    if (style & radixStyles) {
        const res = splitRadixDigits(str, style);
        if (res != null) {
            return checkRange(Number.parseInt(res.digits, res.radix), res.radix, unsigned, bitsize);
        }
        throw formatError(str);
    }
    const parts = splitNumber(str, style);
    if (parts == null) {
        throw formatError(str);
    }
    const digits = integerDigits(parts, unsigned);
    if (digits == null) {
        throw parseOverflow(unsigned, bitsize);
    }
    return checkRange(parts.negative ? 0 - Number(digits) : Number(digits), 10, unsigned, bitsize);
}
// Convert.ToInt32(null) returns 0 where Int32.Parse(null) throws
export function convertString(str, unsigned, bitsize) {
    return str == null ? 0 : parse(str, NumberStyles.Integer, unsigned, bitsize);
}
export function tryParse(str, style, unsigned, bitsize, defValue) {
    checkIntegerStyle(style);
    try {
        defValue.contents = parse(str, style, unsigned, bitsize);
        return true;
    }
    catch {
        defValue.contents = 0;
        return false;
    }
}
export function op_UnaryNegation_Int8(x) {
    return x === -128 ? x : -x;
}
export function op_UnaryNegation_Int16(x) {
    return x === -32768 ? x : -x;
}
export function op_UnaryNegation_Int32(x) {
    return x === -2147483648 ? x : -x;
}
export function divRem(x, y, out) {
    const div = ~~(x / y);
    const rem = x % y;
    if (out === void 0) {
        return [div, rem];
    }
    else {
        out.contents = rem;
        return div;
    }
}
