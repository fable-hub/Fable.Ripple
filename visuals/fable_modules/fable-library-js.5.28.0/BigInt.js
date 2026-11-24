import { absOverflow } from "./Int32.js";
import { fromParts, truncate } from "./Decimal.js";
import { Exception, bigintHash } from "./Util.js";
import { ArgumentException_$ctor_Z721C83C5, ArgumentNullException_$ctor_Z721C83C5, DivideByZeroException_$ctor, FormatException_$ctor_Z721C83C5, OverflowException_$ctor, OverflowException_$ctor_Z721C83C5 } from "./System.js";
import { NumberStyles, invalidStyleMessage, splitNumber, trimEndChars } from "./NumberStyles.js";
const isBigEndian = false;
BigInt.prototype.toJSON = function () {
    return `${this.toString()}`;
};
const zero = 0n;
const one = 1n;
const two = 2n;
const minusOne = -1n;
// V8 caps a BigInt at 2^30 bits and 10^323228497 is the first power of ten above that
const maxDecimalScale = 323228496;
export function isBigInt(x) {
    return typeof x === "bigint";
}
export function hash(x) {
    return bigintHash(x);
}
export function equals(x, y) {
    return x === y;
}
export function compare(x, y) {
    return x < y ? -1 : x > y ? 1 : 0;
}
export function abs(x) { return x < zero ? -x : x; }
export function absIntN(bits, x) {
    if (x >= zero) {
        return x;
    }
    const r = -x;
    if (BigInt.asIntN(bits, r) !== r) {
        throw absOverflow();
    }
    return r;
}
export function sign(x) { return x < zero ? -1 : x > zero ? 1 : 0; }
export function max(x, y) { return x > y ? x : y; }
export function min(x, y) { return x < y ? x : y; }
export function maxMagnitude(x, y) { return abs(x) > abs(y) ? x : y; }
export function minMagnitude(x, y) { return abs(x) < abs(y) ? x : y; }
export function clamp(x, min, max) {
    return x < min ? min : x > max ? max : x;
}
export function add(x, y) { return x + y; }
export function subtract(x, y) { return x - y; }
export function multiply(x, y) { return x * y; }
export function divide(x, y) { return x / y; }
export function remainder(x, y) { return x % y; }
export function negate(x) { return -x; }
export function op_UnaryNegation(x) { return -x; }
export function op_LogicalNot(x) { return ~x; }
export function op_UnaryPlus(x) { return x; }
export function op_Addition(x, y) { return x + y; }
export function op_Subtraction(x, y) { return x - y; }
export function op_Multiply(x, y) { return x * y; }
export function op_Division(x, y) { return x / y; }
export function op_Modulus(x, y) { return x % y; }
export function op_RightShift(x, n) { return x >> BigInt(n); }
export function op_LeftShift(x, n) { return x << BigInt(n); }
export function op_BitwiseAnd(x, y) { return x & y; }
export function op_BitwiseOr(x, y) { return x | y; }
export function op_ExclusiveOr(x, y) { return x ^ y; }
export function op_LessThan(x, y) { return x < y; }
export function op_LessThanOrEqual(x, y) { return x <= y; }
export function op_GreaterThan(x, y) { return x > y; }
export function op_GreaterThanOrEqual(x, y) { return x >= y; }
export function op_Equality(x, y) { return x === y; }
export function op_Inequality(x, y) { return x !== y; }
export function get_Zero() { return zero; }
export function get_One() { return one; }
export function get_MinusOne() { return minusOne; }
export function get_IsZero(x) { return x === zero; }
export function get_IsOne(x) { return x === one; }
export function get_IsEven(x) { return isEvenInteger(x); }
export function get_IsPowerOfTwo(x) { return isPow2(x); }
export function get_Sign(x) { return sign(x); }
export function isNegative(x) { return x < zero; }
export function isPositive(x) { return x > zero; }
export function isEvenInteger(x) { return (x % two) === zero; }
export function isOddInteger(x) { return (x % two) !== zero; }
export function isPow2(x) { return (x & (x - one)) === zero; }
export function fromZero() { return zero; }
export function fromOne() { return one; }
export function fromInt8(n) { return BigInt(n); }
export function fromUInt8(n) { return BigInt(n); }
export function fromInt16(n) { return BigInt(n); }
export function fromUInt16(n) { return BigInt(n); }
export function fromInt32(n) { return BigInt(n); }
export function fromUInt32(n) { return BigInt(n); }
export function fromInt64(n) { return n; }
export function fromUInt64(n) { return n; }
export function fromInt128(n) { return n; }
export function fromUInt128(n) { return n; }
export function fromNativeInt(n) { return n; }
export function fromUNativeInt(n) { return n; }
export function fromFloat16(n) { return BigInt(Math.trunc(n)); }
export function fromFloat32(n) { return BigInt(Math.trunc(n)); }
export function fromFloat64(n) { return BigInt(Math.trunc(n)); }
export function fromDecimal(d) { return BigInt(truncate(d).toString()); }
export function fromBigInt(x) { return x; }
export function fromBoolean(b) { return BigInt(b); }
export function fromChar(c) { return BigInt(c.charCodeAt(0)); }
export function fromString(s) { return BigInt(s); }
export function fromByteArray(bytes) {
    return fromSignedBytes(bytes, isBigEndian);
}
export function toByteArray(value) {
    return toSignedBytes(value, isBigEndian);
}
export function toIntN_unchecked(bits, x, signed) {
    return signed ? BigInt.asIntN(bits, x) : BigInt.asUintN(bits, x);
}
export function toIntN(bits, x, signed) {
    const truncated = signed ? BigInt.asIntN(bits, x) : BigInt.asUintN(bits, x);
    if (truncated !== x) {
        const s = signed ? "a signed" : "an unsigned";
        throw new Exception(`Value was either too large or too small for ${s} ${bits}-bit integer.`);
    }
    return truncated;
}
export function toInt8(x) { return Number(toIntN(8, x, true)); }
export function toUInt8(x) { return Number(toIntN(8, x, false)); }
export function toInt16(x) { return Number(toIntN(16, x, true)); }
export function toUInt16(x) { return Number(toIntN(16, x, false)); }
export function toInt32(x) { return Number(toIntN(32, x, true)); }
export function toUInt32(x) { return Number(toIntN(32, x, false)); }
export function toInt64(x) { return toIntN(64, x, true); }
export function toUInt64(x) { return toIntN(64, x, false); }
export function toInt128(x) { return toIntN(128, x, true); }
export function toUInt128(x) { return toIntN(128, x, false); }
export function toNativeInt(x) { return toIntN(64, x, true); }
export function toUNativeInt(x) { return toIntN(64, x, false); }
export function toInt8_unchecked(x) { return Number(toIntN_unchecked(8, x, true)); }
export function toUInt8_unchecked(x) { return Number(toIntN_unchecked(8, x, false)); }
export function toInt16_unchecked(x) { return Number(toIntN_unchecked(16, x, true)); }
export function toUInt16_unchecked(x) { return Number(toIntN_unchecked(16, x, false)); }
export function toInt32_unchecked(x) { return Number(toIntN_unchecked(32, x, true)); }
export function toUInt32_unchecked(x) { return Number(toIntN_unchecked(32, x, false)); }
export function toInt64_unchecked(x) { return toIntN_unchecked(64, x, true); }
export function toUInt64_unchecked(x) { return toIntN_unchecked(64, x, false); }
export function toInt128_unchecked(x) { return toIntN_unchecked(128, x, true); }
export function toUInt128_unchecked(x) { return toIntN_unchecked(128, x, false); }
export function toNativeInt_unchecked(x) { return toIntN_unchecked(64, x, true); }
export function toUNativeInt_unchecked(x) { return toIntN_unchecked(64, x, false); }
export function toFloat16(x) { return Number(x); }
export function toFloat32(x) { return Number(x); }
export function toFloat64(x) { return Number(x); }
export function toDecimal(x) {
    const isNegative = x < zero;
    const bits = abs(x);
    if ((bits >> 96n) !== zero) {
        throw new Exception("Value was either too large or too small for a Decimal.");
    }
    const low = Number(BigInt.asUintN(32, bits));
    const mid = Number(BigInt.asUintN(32, bits >> 32n));
    const high = Number(BigInt.asUintN(32, bits >> 64n));
    const scale = 0;
    return fromParts(low, mid, high, isNegative, scale);
}
export function toBigInt(x) { return x; }
export function toBoolean(x) { return x !== zero; }
export function toChar(x) {
    return String.fromCharCode(toUInt16(x));
}
export function toString(x) { return x.toString(); }
function checkStyle(style) {
    const message = invalidStyleMessage(style, false);
    if (message !== undefined) {
        throw ArgumentException_$ctor_Z721C83C5(message);
    }
}
function matchDigits(s, style, digit) {
    const leadingWhite = style & NumberStyles.AllowLeadingWhite ? "[\\t-\\r ]*" : "";
    const trailingWhite = style & NumberStyles.AllowTrailingWhite ? "[\\t-\\r ]*" : "";
    return new RegExp(`^${leadingWhite}(${digit}+)\\0*${trailingWhite}$`).exec(s)?.[1];
}
// .NET reads hex digits as two's complement at their written width, so a leading 8-F is negative
function parseHex(s, style) {
    const digits = matchDigits(s, style, "[0-9a-fA-F]");
    return digits === undefined ? undefined : BigInt.asIntN(digits.length * 4, BigInt(`0x${digits}`));
}
// .NET reads binary digits as two's complement at their written width, so a leading 1 is negative
function parseBinary(s, style) {
    const digits = matchDigits(s, style, "[01]");
    return digits === undefined ? undefined : BigInt.asIntN(digits.length, BigInt(`0b${digits}`));
}
function parseDecimal(s, style) {
    const parts = splitNumber(s, style);
    const match = parts && /^(\d*)\.?(\d*)(?:[eE]([+-]?\d+))?$/.exec(parts.digits);
    if (!parts || !match) {
        return undefined;
    }
    const [, integral, fraction, exponent] = match;
    const significant = trimEndChars(integral + fraction, "0");
    if (significant === "") {
        return zero;
    }
    const scale = Number(exponent ?? 0) + integral.length - significant.length;
    if (scale < 0) {
        return undefined;
    }
    if (scale > maxDecimalScale) {
        throw OverflowException_$ctor_Z721C83C5("The value could not be parsed.");
    }
    let magnitude;
    try {
        magnitude = BigInt(significant) * 10n ** BigInt(scale);
    }
    catch {
        throw OverflowException_$ctor_Z721C83C5("The value could not be parsed.");
    }
    return parts.negative ? -magnitude : magnitude;
}
function parseWithStyle(s, style) {
    if (style & NumberStyles.AllowHexSpecifier) {
        return parseHex(s, style);
    }
    return style & NumberStyles.AllowBinarySpecifier ? parseBinary(s, style) : parseDecimal(s, style);
}
export function tryParse(s, style, _provider, res) {
    checkStyle(style);
    let value;
    try {
        value = s == null ? undefined : parseWithStyle(s, style);
    }
    catch {
        value = undefined;
    }
    res.contents = value ?? zero;
    return value !== undefined;
}
export function parse(s, style = NumberStyles.Integer, _provider) {
    if (s == null) {
        throw ArgumentNullException_$ctor_Z721C83C5("value");
    }
    checkStyle(style);
    const value = parseWithStyle(s, style);
    if (value === undefined) {
        throw FormatException_$ctor_Z721C83C5("The value could not be parsed.");
    }
    return value;
}
export function pow(x, n) {
    return x ** BigInt(n);
}
export function pown(x, n) {
    return n < 0 ? one / x ** BigInt(-n) : x ** BigInt(n);
}
export function pownIntN(bits, x, n, signed) {
    const mul = (a, b) => {
        const r = a * b;
        if ((signed ? BigInt.asIntN(bits, r) : BigInt.asUintN(bits, r)) !== r) {
            throw OverflowException_$ctor();
        }
        return r;
    };
    let res = one;
    let e = Math.abs(n);
    while (e > 0) {
        if (e % 2 === 1) {
            res = mul(res, x);
        }
        e = Math.floor(e / 2);
        if (e > 0) {
            x = mul(x, x);
        }
    }
    if (n >= 0) {
        return res;
    }
    if (res === zero) {
        throw DivideByZeroException_$ctor();
    }
    return one / res;
}
export function modPow(x, e, m) {
    return (x ** e) % m;
}
export function divRem(x, y, out) {
    const div = x / y;
    const rem = x % y;
    if (out === void 0) {
        return [div, rem];
    }
    else {
        out.contents = rem;
        return div;
    }
}
export function greatestCommonDivisor(x, y) {
    x = abs(x);
    y = abs(y);
    while (y > zero) {
        const q = x / y;
        const r = x - q * y;
        x = y;
        y = r;
    }
    return x;
}
// Number of bits in the binary representation of x (x must be positive)
function bitLength(x) {
    const hex = x.toString(16);
    return (hex.length - 1) * 4 + (32 - Math.clz32(fromHexCode(hex.charCodeAt(0))));
}
export function getBitLength(x) {
    if (x < zero) {
        x = -x - one; // two's complement bit length excluding the sign bit
    }
    return x === zero ? zero : BigInt(bitLength(x));
}
export function log2(x) {
    const n = Number(x);
    if (Number.isFinite(n))
        return Math.log2(n); // fast path
    if (x < zero)
        return Number.NaN;
    let shift = one;
    while (x >= (one << shift)) {
        shift = shift << one;
    }
    let log = zero;
    while (shift > one) {
        shift = shift >> one;
        if (x >= (one << shift)) {
            log = log + shift;
            x = x >> shift;
        }
    }
    return Number(log);
}
export function log10(x) {
    return log2(x) * Math.log10(2);
}
export function ln(x) {
    return log2(x) * Math.log(2);
}
export function log(x, base) {
    return log2(x) / Math.log2(base);
}
export function ilog2(x) {
    if (x < zero) {
        throw new Exception("Value must be non-negative.");
    }
    return x === zero ? zero : BigInt(bitLength(x) - 1);
}
// export function copySign
// export function createChecked
// export function createSaturating
// export function createTruncating
// export function getByteCount
// export function leadingZeroCount
// export function popCount
// export function rotateLeft
// export function rotateRight
// export function trailingZeroCount
// export function tryFormat
// export function tryWriteBytes
// -------------------------------------------------
// Binary serialization
// -------------------------------------------------
const hexCodes = new Uint8Array([48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102]);
function fromHexCode(code) {
    if (48 <= code && code <= 57)
        return code - 48;
    if (97 <= code && code <= 102)
        return code - 97 + 10;
    if (65 <= code && code <= 70)
        return code - 65 + 10;
    throw Error(`Invalid hex code: ${code}`);
}
function toSignedBytes(x, isBigEndian) {
    const isNeg = x < 0n;
    const sentinel = isNeg ? -1n : 0n;
    const bytes = []; // little-endian
    do {
        bytes.push(Number(BigInt.asUintN(8, x)));
        x = x >> 8n;
    } while (x !== sentinel);
    // extra byte if the top byte's sign bit would misrepresent the sign
    if (isNeg !== (bytes[bytes.length - 1] > 127)) {
        bytes.push(isNeg ? 255 : 0);
    }
    if (isBigEndian) {
        bytes.reverse();
    }
    return new Uint8Array(bytes);
}
function fromSignedBytes(bytes, isBigEndian) {
    if (bytes == null) {
        throw new Exception("bytes is null");
    }
    const len = bytes.length;
    const first = isBigEndian ? 0 : len - 1;
    const isNeg = bytes[first] > 127;
    const codes = new Uint16Array(len * 2 + 2);
    codes[0] = 48; // 0
    codes[1] = 120; // x
    const inc = isBigEndian ? 1 : -1;
    let pos = isBigEndian ? 0 : len - 1;
    for (let i = 0; i < bytes.length; i++, pos += inc) {
        const byte = bytes[pos];
        codes[2 * i + 2] = hexCodes[byte >> 4];
        codes[2 * i + 3] = hexCodes[byte & 15];
    }
    const str = String.fromCharCode.apply(null, codes);
    let x = BigInt(str);
    if (isNeg) {
        const bits = len * 8;
        const pow2 = (1n << BigInt(bits));
        x = x - pow2; // two's complement
    }
    return x;
}
