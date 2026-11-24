import { checkIntegerStyle, getPrefix, parseOverflow, splitFSharpInteger, splitRadixDigits } from "./Int32.js";
import { integerDigits, NumberStyles, radixStyles, splitNumber } from "./NumberStyles.js";
import { Exception } from "./Util.js";
import { ArgumentNullException_$ctor_Z721C83C5, FormatException_$ctor_Z721C83C5 } from "./System.js";
function getRange(unsigned, bitsize) {
    switch (bitsize) {
        case 64: return unsigned ?
            [0n, 18446744073709551615n] :
            [-9223372036854775808n, 9223372036854775807n];
        default: throw new Exception("Invalid bit size.");
    }
}
function formatError(str) {
    return FormatException_$ctor_Z721C83C5(`The input string '${str}' was not in a correct format.`);
}
function checkRange(v, radix, unsigned, bitsize) {
    const [umin, umax] = getRange(true, bitsize);
    if (!unsigned && radix !== 10 && v >= umin && v <= umax) {
        v = BigInt.asIntN(bitsize, v);
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
        const v = BigInt(getPrefix(res.radix) + res.digits);
        return checkRange(res.sign === "-" ? -v : v, res.radix, unsigned, bitsize);
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
            return checkRange(BigInt(getPrefix(res.radix) + res.digits), res.radix, unsigned, bitsize);
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
    return checkRange(parts.negative ? -BigInt(digits) : BigInt(digits), 10, unsigned, bitsize);
}
// Convert.ToInt32(null) returns 0 where Int32.Parse(null) throws
export function convertString(str, unsigned, bitsize) {
    return str == null ? 0n : parse(str, NumberStyles.Integer, unsigned, bitsize);
}
export function tryParse(str, style, unsigned, bitsize, defValue) {
    checkIntegerStyle(style);
    try {
        defValue.contents = parse(str, style, unsigned, bitsize);
        return true;
    }
    catch {
        defValue.contents = 0n;
        return false;
    }
}
