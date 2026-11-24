import { toString as dateToString } from "./Date.js";
import { compare as numericCompare, divide, isNumeric, isIntegral, multiply, toExponential, toExponentialHalfEven, toFixed, toFixedHalfEven, toHex, toPrecision, toPrecisionHalfEven, toRadix } from "./Numeric.js";
import { escape } from "./RegExp.js";
import { ArgumentException_$ctor_Z384F8060, ArgumentNullException_$ctor_Z721C83C5, ArgumentOutOfRangeException_$ctor_Z384F8060, ArgumentOutOfRangeException_$ctor_Z721C83C5, FormatException_$ctor_Z721C83C5 } from "./System.js";
import { toString } from "./Types.js";
import { Exception } from "./Util.js";
// Temporal date/time values (only present under --test:js-temporal) are not JS Dates. To avoid
// importing the side-effecting Temporal runtime modules into this core file — which would pull
// them into every bundle — they advertise a .NET-style formatter through this well-known symbol,
// attached to their prototype when (and only when) those modules are loaded.
const dateTimeFormattableSymbol = Symbol.for("Fable.DateTimeFormattable");
function isNetDateFormattable(rep) {
    return rep != null && typeof rep[dateTimeFormattableSymbol] === "function";
}
function formatDateLike(rep, format) {
    return rep instanceof Date ? dateToString(rep, format) : rep[dateTimeFormattableSymbol](format);
}
const fsFormatRegExp = /(^|[^%])%([0+\- ]*)(\*|\d+)?(?:\.(\d+))?(\w)/g;
const interpolateRegExp = /(?:(^|[^%])%([0+\- ]*)(\d+)?(?:\.(\d+))?(\w))?%P\(\)/g;
const formatRegExp = /\{\{|\}\}|\{(\d+)(,-?\d+)?(?:\:([a-zA-Z])(\d{0,2})|\:(.+?))?\}/g;
function isLessThan(x, y) {
    return numericCompare(x, y) < 0;
}
export const StringComparison = {
    CurrentCulture: 0,
    CurrentCultureIgnoreCase: 1,
    InvariantCulture: 2,
    InvariantCultureIgnoreCase: 3,
    Ordinal: 4,
    OrdinalIgnoreCase: 5,
};
function cmp(x, y, ic) {
    function isIgnoreCase(i) {
        return i === true ||
            i === StringComparison.CurrentCultureIgnoreCase ||
            i === StringComparison.InvariantCultureIgnoreCase ||
            i === StringComparison.OrdinalIgnoreCase;
    }
    function isOrdinal(i) {
        return i === StringComparison.Ordinal ||
            i === StringComparison.OrdinalIgnoreCase;
    }
    if (x == null) {
        return y == null ? 0 : -1;
    }
    if (y == null) {
        return 1;
    } // everything is bigger than null
    if (isOrdinal(ic)) {
        if (isIgnoreCase(ic)) {
            x = ordinalUpper(x);
            y = ordinalUpper(y);
        }
        return (x === y) ? 0 : (x < y ? -1 : 1);
    }
    else if (isIgnoreCase(ic)) {
        return x.localeCompare(y, undefined, { sensitivity: "accent" });
    }
    else {
        return x.localeCompare(y);
    }
}
// .NET OrdinalIgnoreCase maps each code point to its simple uppercase and leaves U+0131 and U+017F unchanged.
function ordinalUpper(str) {
    if (/^[\x00-\x7F]*$/.test(str)) {
        return str.toUpperCase();
    }
    let res = "";
    for (const ch of str) {
        if (ch === "ı" || ch === "ſ") {
            res += ch;
            continue;
        }
        const upper = ch.toUpperCase();
        if ([...upper].length === 1) {
            res += upper;
            continue;
        }
        const lower = ch.toLowerCase();
        res += [...lower].length === 1 ? lower : ch;
    }
    return res;
}
export function compare(...args) {
    switch (args.length) {
        case 2: return cmp(args[0], args[1], false);
        case 3: return cmp(args[0], args[1], args[2]);
        case 4: return cmp(args[0], args[1], args[2] === true);
        case 5: return cmp(args[0].slice(args[1], args[1] + args[4]), args[2].slice(args[3], args[3] + args[4]), false);
        case 6: return cmp(args[0].slice(args[1], args[1] + args[4]), args[2].slice(args[3], args[3] + args[4]), args[5]);
        case 7: return cmp(args[0].slice(args[1], args[1] + args[4]), args[2].slice(args[3], args[3] + args[4]), args[5] === true);
        default: throw new Exception("String.compare: Unsupported number of parameters");
    }
}
export function compareOrdinal(x, y) {
    return cmp(x, y, StringComparison.Ordinal);
}
export function compareTo(x, y) {
    return cmp(x, y, StringComparison.CurrentCulture);
}
export function startsWith(str, pattern, ic) {
    if (ic === StringComparison.Ordinal) { // to avoid substring allocation
        return str.startsWith(pattern);
    }
    if (str.length >= pattern.length) {
        return cmp(str.slice(0, pattern.length), pattern, ic) === 0;
    }
    return false;
}
export function endsWith(str, pattern, ic) {
    if (ic === StringComparison.Ordinal) { // to avoid substring allocation
        return str.endsWith(pattern);
    }
    if (str.length >= pattern.length) {
        return cmp(str.slice(str.length - pattern.length), pattern, ic) === 0;
    }
    return false;
}
export function contains(str, pattern, ic) {
    if (ic === StringComparison.Ordinal) { // fast path
        return str.includes(pattern);
    }
    const len = pattern.length;
    for (let i = 0; i <= str.length - len; i++) {
        if (cmp(str.slice(i, i + len), pattern, ic) === 0) {
            return true;
        }
    }
    return false;
}
export function indexOf(str, searchValue, comparison, startIndex = 0) {
    if (comparison === StringComparison.Ordinal) { // fast path
        return str.indexOf(searchValue, startIndex);
    }
    const len = searchValue.length;
    for (let i = Math.max(startIndex, 0); i <= str.length - len; i++) {
        if (cmp(str.slice(i, i + len), searchValue, comparison) === 0) {
            return i;
        }
    }
    return -1;
}
export function lastIndexOf(str, searchValue, comparison, startIndex = str.length - 1) {
    if (comparison === StringComparison.Ordinal) { // fast path
        return str.lastIndexOf(searchValue, startIndex);
    }
    const len = searchValue.length;
    for (let i = Math.min(startIndex, str.length - len); i >= 0; i--) {
        if (cmp(str.slice(i, i + len), searchValue, comparison) === 0) {
            return i;
        }
    }
    return -1;
}
export function indexOfAny(str, anyOf, ...args) {
    if (str == null || str === "") {
        return -1;
    }
    const startIndex = (args.length > 0) ? args[0] : 0;
    if (startIndex < 0) {
        throw new Exception("Start index cannot be negative");
    }
    const length = (args.length > 1) ? args[1] : str.length - startIndex;
    if (length < 0) {
        throw new Exception("Length cannot be negative");
    }
    if (startIndex + length > str.length) {
        throw new Exception("Invalid startIndex and length");
    }
    const endIndex = startIndex + length;
    const anyOfAsStr = "".concat.apply("", anyOf);
    for (let i = startIndex; i < endIndex; i++) {
        if (anyOfAsStr.indexOf(str[i]) > -1) {
            return i;
        }
    }
    return -1;
}
export function printf(input) {
    return {
        input,
        cont: fsFormat(input),
    };
}
export function interpolatedPrintf(input, text) {
    return {
        input,
        cont: (f) => f(text),
    };
}
export function interpolate(str, values) {
    let valIdx = 0;
    let strIdx = 0;
    let result = "";
    interpolateRegExp.lastIndex = 0;
    let match = interpolateRegExp.exec(str);
    while (match) {
        // The first group corresponds to the no-escape char (^|[^%]), the actual pattern starts in the next char
        // Note: we don't use negative lookbehind because some browsers don't support it yet
        const matchIndex = match.index + (match[1] || "").length;
        result += str.substring(strIdx, matchIndex).replace(/%%/g, "%");
        const [, , flags, padLength, precision, format] = match;
        // Save interpolateRegExp.lastIndex before running formatReplacement because the values
        // may also involve interpolation and make use of interpolateRegExp (see #3078)
        strIdx = interpolateRegExp.lastIndex;
        result += formatReplacement(values[valIdx++], flags, padLength, precision, format);
        // Move interpolateRegExp.lastIndex one char behind to make sure we match the no-escape char next time
        interpolateRegExp.lastIndex = strIdx - 1;
        match = interpolateRegExp.exec(str);
    }
    result += str.substring(strIdx).replace(/%%/g, "%");
    return result;
}
function continuePrint(cont, arg) {
    return typeof arg === "string" ? cont(arg) : arg.cont(cont);
}
export function toConsole(arg) {
    // Don't remove the lambda here, see #1357
    return continuePrint((x) => console.log(x), arg);
}
export function toConsoleError(arg) {
    return continuePrint((x) => console.error(x), arg);
}
export function toText(arg) {
    return continuePrint((x) => x, arg);
}
export function toFail(arg) {
    return continuePrint((x) => {
        throw new Exception(x);
    }, arg);
}
function formatReplacement(rep, flags, padLength, precision, format) {
    let sign = "";
    flags = flags || "";
    format = format || "";
    if (isNumeric(rep)) {
        const radix = format === "x" || format === "X" ? 16 : format === "o" ? 8 : 10;
        if (radix === 10 && isLessThan(rep, 0)) {
            rep = multiply(rep, -1);
            sign = "-";
        }
        else if (flags.indexOf(" ") >= 0) {
            sign = " ";
        }
        else if (flags.indexOf("+") >= 0) {
            sign = "+";
        }
        precision = precision == null ? null : parseInt(precision, 10);
        switch (format) {
            case "f":
            case "F":
                precision = precision != null ? precision : 6;
                rep = toFixedHalfEven(rep, precision);
                break;
            case "g":
            case "G":
                rep = precision != null ? toPrecisionHalfEven(rep, precision) : toPrecision(rep);
                break;
            case "e":
            case "E":
                rep = precision != null ? toExponentialHalfEven(rep, precision) : toExponential(rep);
                break;
            case "x":
            case "o":
                rep = toRadix(rep, radix);
                break;
            case "X":
                rep = toRadix(rep, radix).toUpperCase();
                break;
            case "":
            case "O":
                rep = typeof rep === "number" ? formatGeneral(rep, "R", null) : String(rep);
                break;
            default: // Aid
                rep = String(rep);
                break;
        }
    }
    else if (rep instanceof Date || isNetDateFormattable(rep)) {
        rep = formatDateLike(rep, undefined);
    }
    else if (format === "A" && typeof rep === "string") {
        rep = "\"" + rep + "\"";
    }
    else if (format === "s" && rep == null) {
        rep = "";
    }
    else {
        rep = toString(rep);
    }
    padLength = typeof padLength === "number" ? padLength : parseInt(padLength, 10);
    if (!isNaN(padLength)) {
        const zeroFlag = flags.indexOf("0") >= 0; // Use '0' for left padding
        const minusFlag = flags.indexOf("-") >= 0; // Right padding
        const ch = minusFlag || !zeroFlag ? " " : "0";
        if (ch === "0") {
            rep = pad(rep, padLength - sign.length, ch, minusFlag);
            rep = sign + rep;
        }
        else {
            rep = pad(sign + rep, padLength, ch, minusFlag);
        }
    }
    else {
        rep = sign + rep;
    }
    return rep;
}
function createPrinter(cont, _strParts, _matches, _result = "", padArg = -1) {
    return (...args) => {
        // Make copies of the values passed by reference because the function can be used multiple times
        let result = _result;
        const strParts = _strParts.slice();
        const matches = _matches.slice();
        for (const arg of args) {
            const [, , flags, _padLength, precision, format] = matches[0];
            let padLength = _padLength;
            if (padArg >= 0) {
                padLength = padArg;
                padArg = -1;
            }
            else if (padLength === "*") {
                if (arg < 0) {
                    throw new Exception("Non-negative number required");
                }
                padArg = arg;
                continue;
            }
            result += strParts[0];
            result += formatReplacement(arg, flags, padLength, precision, format);
            strParts.splice(0, 1);
            matches.splice(0, 1);
        }
        if (matches.length === 0) {
            result += strParts[0];
            return cont(result);
        }
        else {
            return createPrinter(cont, strParts, matches, result, padArg);
        }
    };
}
export function fsFormat(str) {
    return (cont) => {
        fsFormatRegExp.lastIndex = 0;
        const strParts = [];
        const matches = [];
        let strIdx = 0;
        let match = fsFormatRegExp.exec(str);
        while (match) {
            // The first group corresponds to the no-escape char (^|[^%]), the actual pattern starts in the next char
            // Note: we don't use negative lookbehind because some browsers don't support it yet
            const matchIndex = match.index + (match[1] || "").length;
            strParts.push(str.substring(strIdx, matchIndex).replace(/%%/g, "%"));
            matches.push(match);
            strIdx = fsFormatRegExp.lastIndex;
            // Likewise we need to move fsFormatRegExp.lastIndex one char behind to make sure we match the no-escape char next time
            fsFormatRegExp.lastIndex -= 1;
            match = fsFormatRegExp.exec(str);
        }
        if (strParts.length === 0) {
            return cont(str.replace(/%%/g, "%"));
        }
        else {
            strParts.push(str.substring(strIdx).replace(/%%/g, "%"));
            return createPrinter(cont, strParts, matches);
        }
    };
}
function splitIntAndDecimalPart(value) {
    let [repInt, repDecimal] = value.split(".");
    repDecimal === undefined && (repDecimal = "");
    return {
        integral: repInt,
        decimal: repDecimal
    };
}
// .NET rounds a percentage on the unscaled value, at precision + 2 decimals
function shiftPercent(value) {
    const sign = value.startsWith("-") ? "-" : "";
    const [integral, decimal = ""] = value.slice(sign.length).split(".");
    const shifted = (integral + decimal.slice(0, 2)).replace(/^0+(?=\d)/, "");
    return sign + shifted + (decimal.length > 2 ? "." + decimal.slice(2) : "");
}
function thousandSeparate(value) {
    return value.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
function splitExponential(s) {
    const eIdx = s.indexOf("e");
    return [s.slice(0, eIdx).replace(".", ""), Number(s.slice(eIdx + 1))];
}
function shortestSingle(x) {
    // 100 digits tell where a float32 lies between two candidates, and 9 digits always round-trip
    const [digits, exponent] = splitExponential(x.toExponential(99));
    for (let p = 1;; p++) {
        const down = Number(digits.slice(0, p));
        const rest = digits.slice(p);
        const half = "5".padEnd(rest.length, "0");
        const roundUp = rest > half || rest === half && down % 2 === 1;
        for (const n of roundUp ? [down + 1, down] : [down, down + 1]) {
            if (Math.fround(Number(n + "e" + (exponent - p + 1))) === x) {
                return [String(n), exponent + String(n).length - p];
            }
        }
    }
}
// .NET "G" and "R": fixed notation unless the exponent is below -4 or reaches the precision (17 for the shortest digits, 9 for a float32)
function formatGeneral(x, format, precision, isSingle = false) {
    if (!Number.isFinite(x)) {
        return String(x);
    }
    const sign = x < 0 || Object.is(x, -0) ? "-" : "";
    x = Math.abs(x);
    // R ignores the precision and G0 is G
    const p = format === "r" || format === "R" ? 0 : precision ?? 0;
    const [significand, exponent] = p > 0 ? splitExponential(toExponentialHalfEven(x, p - 1)) : isSingle ? shortestSingle(x) : splitExponential(x.toExponential());
    const digits = trimEnd(significand, "0") || "0";
    if (exponent < -4 || exponent >= (p > 0 ? p : isSingle ? 9 : 17)) {
        const mantissa = digits.length > 1 ? digits[0] + "." + digits.slice(1) : digits;
        const eChar = format === "G" || format === "R" ? "E" : "e";
        return sign + mantissa + eChar + (exponent < 0 ? "-" : "+") + padLeft(String(Math.abs(exponent)), 2, "0");
    }
    else if (exponent < 0) {
        return sign + "0." + "0".repeat(-exponent - 1) + digits;
    }
    else {
        const decimals = digits.slice(exponent + 1);
        return sign + digits.slice(0, exponent + 1).padEnd(exponent + 1, "0") + (decimals ? "." + decimals : "");
    }
}
export function formatSingle(str, x) {
    const m = /^\{0:([GgRr])(\d{0,2})\}$/.exec(str);
    return m != null ? formatGeneral(Math.fround(x), m[1], m[2] ? parseInt(m[2], 10) : null, true) : format(str, x);
}
export function float64ToString(x) {
    return formatGeneral(x, "R", null);
}
export function float32ToString(x) {
    return formatGeneral(Math.fround(x), "R", null, true);
}
export function format(str, ...args) {
    let str2;
    if (typeof str === "object") {
        // Called with culture info
        str2 = String(args[0]);
        args.shift();
    }
    else {
        str2 = str;
    }
    return str2.replace(formatRegExp, (match, idx, padLength, format, precision, pattern) => {
        if (match === "{{" || match === "}}") {
            return match[0];
        }
        if (idx < 0 || idx >= args.length) {
            throw new Exception("Index must be greater or equal to zero and less than the arguments' length.");
        }
        let rep = args[idx];
        let parts;
        if (isNumeric(rep)) {
            precision = precision == "" ? null : parseInt(precision, 10);
            switch (format) {
                case "b":
                case "B":
                    if (!isIntegral(rep)) {
                        throw new Exception("Format specifier was invalid.");
                    }
                    rep = (rep >>> 0).toString(2).replace(/^0+/, "").padStart(precision || 1, "0");
                    break;
                case "c":
                case "C":
                    const isNegative = isLessThan(rep, 0);
                    if (isLessThan(rep, 0)) {
                        rep = multiply(rep, -1);
                    }
                    precision = precision == null ? 2 : precision;
                    rep = toFixedHalfEven(rep, precision);
                    parts = splitIntAndDecimalPart(rep);
                    if (precision > 0) {
                        rep = "¤" + thousandSeparate(parts.integral) + "." + padRight(parts.decimal, precision, "0");
                    }
                    else {
                        rep = "¤" + thousandSeparate(parts.integral);
                    }
                    if (isNegative) {
                        rep = "(" + rep + ")";
                    }
                    break;
                case "d":
                case "D":
                    if (!isIntegral(rep)) {
                        throw new Exception("Format specifier was invalid.");
                    }
                    rep = String(rep);
                    if (precision != null) {
                        if (rep.startsWith("-")) {
                            rep = "-" + padLeft(rep.substring(1), precision, "0");
                        }
                        else {
                            rep = padLeft(rep, precision, "0");
                        }
                    }
                    break;
                case "e":
                case "E": {
                    precision = precision != null ? precision : 6;
                    rep = toExponentialHalfEven(rep, precision);
                    // .NET always signs the exponent and pads it to at least three digits
                    const eIdx = rep.indexOf("e");
                    rep = rep.slice(0, eIdx) + format + rep[eIdx + 1] + padLeft(rep.slice(eIdx + 2), 3, "0");
                    break;
                }
                case "f":
                case "F":
                    precision = precision != null ? precision : 2;
                    rep = toFixedHalfEven(rep, precision);
                    if (precision > 0) {
                        parts = splitIntAndDecimalPart(rep);
                        rep = parts.integral + "." + padRight(parts.decimal, precision, "0");
                    }
                    break;
                case "g":
                case "G":
                case "r":
                case "R": {
                    if (typeof rep === "number") {
                        rep = formatGeneral(rep, format, precision);
                        break;
                    }
                    if (precision == null) {
                        rep = String(rep);
                        break;
                    }
                    rep = toPrecision(rep, precision);
                    // Handle exponential notation: only trim trailing zeros from mantissa, not from exponent.
                    // .NET G format guarantees an exponent of at least 2 digits with an explicit sign (e.g. "E-07").
                    const eIdx = rep.indexOf("e");
                    if (eIdx >= 0) {
                        const mantissa = trimEnd(trimEnd(rep.slice(0, eIdx), "0"), ".");
                        const expSign = rep[eIdx + 1]; // toPrecision always emits "+" or "-"
                        const expDigits = rep.slice(eIdx + 2);
                        const paddedExpDigits = expDigits.length < 2 ? "0" + expDigits : expDigits;
                        const eChar = format === "G" || format === "R" ? "E" : "e";
                        rep = mantissa + eChar + expSign + paddedExpDigits;
                    }
                    else if (rep.indexOf(".") >= 0) {
                        rep = trimEnd(trimEnd(rep, "0"), ".");
                    }
                    break;
                }
                case "n":
                case "N":
                    precision = precision != null ? precision : 2;
                    rep = toFixedHalfEven(rep, precision);
                    parts = splitIntAndDecimalPart(rep);
                    if (precision > 0) {
                        rep = thousandSeparate(parts.integral) + "." + padRight(parts.decimal, precision, "0");
                    }
                    else {
                        rep = thousandSeparate(parts.integral);
                    }
                    break;
                case "p":
                case "P":
                    precision = precision != null ? precision : 2;
                    rep = typeof rep === "number" ? shiftPercent(toFixedHalfEven(rep, precision + 2)) : toFixed(multiply(rep, 100), precision);
                    parts = splitIntAndDecimalPart(rep);
                    if (precision > 0) {
                        rep = thousandSeparate(parts.integral) + "." + padRight(parts.decimal, precision, "0") + " %";
                    }
                    else {
                        rep = thousandSeparate(parts.integral) + " %";
                    }
                    break;
                case "x":
                case "X":
                    if (!isIntegral(rep)) {
                        throw new Exception("Format specifier was invalid.");
                    }
                    precision = precision != null ? precision : 1;
                    rep = padLeft(toHex(rep), precision, "0");
                    if (format === "X") {
                        rep = rep.toUpperCase();
                    }
                    break;
                default:
                    // If we have format and were not able to handle it throw
                    // See: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers
                    if (format) {
                        throw new Exception("Format specifier was invalid.");
                    }
                    if (pattern) {
                        let sign = "";
                        const patternStr = pattern;
                        // Each `%` scales the value by 100 and is kept as a literal in the output
                        const percents = (patternStr.match(/%/g) ?? []).length;
                        if (percents > 0) {
                            rep = multiply(rep, Math.pow(100, percents));
                        }
                        // .NET ignores commas placed after the decimal placeholders, hence the trailing group
                        rep = patternStr.replace(/([0#,]+)(\.[0#]+)?(,*)/, (_, intPart, decimalPart) => {
                            // Commas between the last integer placeholder and the decimal point scale the
                            // value down by 1000 each; commas anywhere else only turn on digit grouping
                            const scaleCommas = /,*$/.exec(intPart)[0].length;
                            if (scaleCommas > 0) {
                                intPart = intPart.substring(0, intPart.length - scaleCommas);
                                rep = divide(rep, Math.pow(1000, scaleCommas));
                            }
                            if (isLessThan(rep, 0)) {
                                rep = multiply(rep, -1);
                                sign = "-";
                            }
                            decimalPart = decimalPart == null ? "" : decimalPart.substring(1);
                            rep = toFixed(rep, Math.max(decimalPart.length, 0));
                            let [repInt, repDecimal] = rep.split(".");
                            repDecimal || (repDecimal = "");
                            const leftZeroes = intPart.replace(/,/g, "").replace(/^#+/, "").length;
                            repInt = padLeft(repInt, leftZeroes, "0");
                            const rightZeros = decimalPart.replace(/#+$/, "").length;
                            if (rightZeros > repDecimal.length) {
                                repDecimal = padRight(repDecimal, rightZeros, "0");
                            }
                            else if (rightZeros < repDecimal.length) {
                                repDecimal = repDecimal.substring(0, rightZeros) + repDecimal.substring(rightZeros).replace(/0+$/, "");
                            }
                            // Thousands separator
                            if (intPart.indexOf(",") > 0) {
                                const i = repInt.length % 3;
                                const thousandGroups = Math.floor(repInt.length / 3);
                                let thousands = i > 0 ? repInt.substr(0, i) + (thousandGroups > 0 ? "," : "") : "";
                                for (let j = 0; j < thousandGroups; j++) {
                                    thousands += repInt.substr(i + j * 3, 3) + (j < thousandGroups - 1 ? "," : "");
                                }
                                repInt = thousands;
                            }
                            return repDecimal.length > 0 ? repInt + "." + repDecimal : repInt;
                        });
                        rep = sign + rep;
                    }
                    else if (typeof rep === "number") {
                        rep = formatGeneral(rep, "R", null);
                    }
            }
        }
        else if (rep instanceof Date || isNetDateFormattable(rep)) {
            rep = formatDateLike(rep, pattern || format);
        }
        else {
            rep = rep == null ? "" : toString(rep);
        }
        padLength = parseInt((padLength || " ").substring(1), 10);
        if (!isNaN(padLength)) {
            rep = pad(String(rep), Math.abs(padLength), " ", padLength < 0);
        }
        return rep;
    });
}
export function initialize(n, f) {
    if (n < 0) {
        throw new Exception("String length must be non-negative");
    }
    const xs = new Array(n);
    for (let i = 0; i < n; i++) {
        xs[i] = f(i);
    }
    return xs.join("");
}
export function insert(str, startIndex, value) {
    if (startIndex < 0 || startIndex > str.length) {
        throw new Exception("startIndex is negative or greater than the length of this instance.");
    }
    return str.substring(0, startIndex) + value + str.substring(startIndex);
}
export function isNullOrEmpty(str) {
    return typeof str !== "string" || str.length === 0;
}
export function isNullOrWhiteSpace(str) {
    return typeof str !== "string" || /^\s*$/.test(str);
}
export function concat(...xs) {
    return xs.map((x) => (x == null ? "" : String(x))).join("");
}
export function join(delimiter, xs) {
    if (Array.isArray(xs)) {
        return xs.join(delimiter ?? "");
    }
    else {
        return Array.from(xs).join(delimiter ?? "");
    }
}
export function joinWithIndices(delimiter, xs, startIndex, count) {
    const endIndexPlusOne = startIndex + count;
    if (endIndexPlusOne > xs.length) {
        throw new Exception("Index and count must refer to a location within the buffer.");
    }
    return xs.slice(startIndex, endIndexPlusOne).join(delimiter ?? "");
}
function notSupported(name) {
    throw new Exception("The environment doesn't support '" + name + "', please use a polyfill.");
}
export function toBase64String(inArray) {
    let str = "";
    for (let i = 0; i < inArray.length; i++) {
        str += String.fromCharCode(inArray[i]);
    }
    return typeof btoa === "function" ? btoa(str) : notSupported("btoa");
}
export function fromBase64String(b64Encoded) {
    const binary = typeof atob === "function" ? atob(b64Encoded) : notSupported("atob");
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return bytes;
}
export function toHexString(inArray, offset, length) {
    if (inArray == null) {
        throw ArgumentNullException_$ctor_Z721C83C5("inArray");
    }
    const start = offset ?? 0;
    const count = length ?? inArray.length - start;
    if (count < 0) {
        throw ArgumentOutOfRangeException_$ctor_Z384F8060("length", "Non-negative number required.");
    }
    if (start < 0) {
        throw ArgumentOutOfRangeException_$ctor_Z384F8060("offset", "Non-negative number required.");
    }
    if (start > inArray.length - count) {
        throw ArgumentOutOfRangeException_$ctor_Z721C83C5("offset");
    }
    let str = "";
    for (let i = start; i < start + count; i++) {
        str += inArray[i].toString(16).padStart(2, "0");
    }
    return str.toUpperCase();
}
export function toHexStringLower(inArray, offset, length) {
    return toHexString(inArray, offset, length).toLowerCase();
}
export function fromHexString(s) {
    if (s == null) {
        throw ArgumentNullException_$ctor_Z721C83C5("s");
    }
    if (s.length % 2 !== 0) {
        throw FormatException_$ctor_Z721C83C5("The input is not a valid hex string as its length is not a multiple of 2.");
    }
    if (!/^[0-9A-Fa-f]*$/.test(s)) {
        throw FormatException_$ctor_Z721C83C5("The input is not a valid hex string as it contains a non-hex character.");
    }
    const bytes = new Uint8Array(s.length / 2);
    for (let i = 0; i < bytes.length; i++) {
        bytes[i] = parseInt(s.substring(2 * i, 2 * i + 2), 16);
    }
    return bytes;
}
function pad(str, len, ch, isRight) {
    ch = ch || " ";
    len = len - str.length;
    for (let i = 0; i < len; i++) {
        str = isRight ? str + ch : ch + str;
    }
    return str;
}
export function padLeft(str, len, ch) {
    return pad(str, len, ch);
}
export function padRight(str, len, ch) {
    return pad(str, len, ch, true);
}
export function remove(str, startIndex, count) {
    if (startIndex >= str.length) {
        throw new Exception("startIndex must be less than length of string");
    }
    if (typeof count === "number" && (startIndex + count) > str.length) {
        throw new Exception("Index and count must refer to a location within the string.");
    }
    return str.slice(0, startIndex) + (typeof count === "number" ? str.substr(startIndex + count) : "");
}
export function replace(str, search, replacement, comparison = StringComparison.Ordinal, _culture) {
    if (search == null) {
        throw ArgumentNullException_$ctor_Z721C83C5("oldValue");
    }
    if (search.length === 0) {
        throw ArgumentException_$ctor_Z384F8060("The value cannot be an empty string.", "oldValue");
    }
    const comp = comparison;
    let result = "";
    let start = 0;
    let match = indexOf(str, search, comp);
    while (match >= 0) {
        result += str.slice(start, match) + (replacement ?? "");
        start = match + search.length;
        match = indexOf(str, search, comp, start);
    }
    return result + str.slice(start);
}
export function replicate(n, x) {
    return initialize(n, () => x);
}
export function getCharAtIndex(input, index) {
    if (index < 0 || index >= input.length) {
        throw new Exception("Index was outside the bounds of the array.");
    }
    return input[index];
}
export function split(str, splitters, count, options) {
    count = typeof count === "number" ? count : undefined;
    options = typeof options === "number" ? options : 0;
    if (count && count < 0) {
        throw new Exception("Count cannot be less than zero");
    }
    if (count === 0) {
        return [];
    }
    const removeEmpty = (options & 1) === 1;
    const trim = (options & 2) === 2;
    splitters = splitters || [];
    splitters = splitters.filter(x => x).map(escape);
    splitters = splitters.length > 0 ? splitters : ["\\s"];
    const splits = [];
    const reg = new RegExp(splitters.join("|"), "g");
    let findSplits = true;
    let i = 0;
    do {
        const match = reg.exec(str);
        if (match === null) {
            const candidate = trim ? str.substring(i).trim() : str.substring(i);
            if (!removeEmpty || candidate.length > 0) {
                splits.push(candidate);
            }
            findSplits = false;
        }
        else {
            const candidate = trim ? str.substring(i, match.index).trim() : str.substring(i, match.index);
            if (!removeEmpty || candidate.length > 0) {
                if (count != null && splits.length + 1 === count) {
                    splits.push(trim ? str.substring(i).trim() : str.substring(i));
                    findSplits = false;
                }
                else {
                    splits.push(candidate);
                }
            }
            i = reg.lastIndex;
        }
    } while (findSplits);
    return splits;
}
export function trim(str, ...chars) {
    if (chars.length === 0) {
        return str.trim();
    }
    const pattern = "[" + escape(chars.join("")) + "]+";
    return str.replace(new RegExp("^" + pattern), "").replace(new RegExp(pattern + "$"), "");
}
export function trimStart(str, ...chars) {
    return chars.length === 0
        ? str.trimStart()
        : str.replace(new RegExp("^[" + escape(chars.join("")) + "]+"), "");
}
export function trimEnd(str, ...chars) {
    return chars.length === 0
        ? str.trimEnd()
        : str.replace(new RegExp("[" + escape(chars.join("")) + "]+$"), "");
}
export function filter(pred, x) {
    return x.split("").filter((c) => pred(c)).join("");
}
export function substring(str, startIndex, length) {
    if ((startIndex + (length || 0) > str.length)) {
        throw new Exception("Invalid startIndex and/or length");
    }
    return length != null ? str.substr(startIndex, length) : str.substr(startIndex);
}
export function toCharArray2(str, startIndex, length) {
    return substring(str, startIndex, length).split("");
}
export function fmt(strs, ...args) {
    return ({ strs, args });
}
export function fmtWith(fmts) {
    return (strs, ...args) => ({ strs, args, fmts });
}
export function getFormat(s) {
    const strs = s.strs.map((value) => value.replace(/{/g, '{{').replace(/}/g, '}}'));
    return s.fmts
        ? strs
            .reduce((acc, newPart, index) => acc + `{${String(index - 1) + s.fmts[index - 1]}}` + newPart)
        : strs
            .reduce((acc, newPart, index) => acc + `{${index - 1}}` + newPart);
}
