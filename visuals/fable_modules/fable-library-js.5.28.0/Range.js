import { compare } from "./Util.js";
import { Operators_InvalidArg } from "./FSharp.Core.js";
import { empty, singleton, unfold, delay } from "./Seq.js";
import { toUInt64_unchecked, op_Subtraction, toInt64_unchecked, compare as compare_1, equals, op_Addition, fromZero } from "./BigInt.js";
import { op_Addition as op_Addition_1, fromParts } from "./Decimal.js";
export function makeRangeStepFunction(step, stop, zero, add) {
    const stepComparedWithZero = compare(step, zero) | 0;
    if (stepComparedWithZero === 0) {
        throw Operators_InvalidArg("step", "The step of a range cannot be zero.");
    }
    const stepGreaterThanZero = stepComparedWithZero > 0;
    return (x) => {
        const comparedWithLast = compare(x, stop) | 0;
        return ((stepGreaterThanZero && (comparedWithLast <= 0)) ? true : (!stepGreaterThanZero && (comparedWithLast >= 0))) ? [x, add(x, step)] : undefined;
    };
}
export function integralRangeStep(start, step, stop, zero, add) {
    const stepFn = makeRangeStepFunction(step, stop, zero, add);
    return delay(() => unfold(stepFn, start));
}
export function rangeBigInt(start, step, stop) {
    return integralRangeStep(start, step, stop, fromZero(), op_Addition);
}
export function rangeDecimal(start, step, stop) {
    return integralRangeStep(start, step, stop, fromParts(0, 0, 0, false, 0), op_Addition_1);
}
function floatingRangeStep(round, start, step, stop) {
    if (start !== start) {
        throw Operators_InvalidArg("n", "The start of a range cannot be NaN.");
    }
    if (step !== step) {
        throw Operators_InvalidArg("step", "The step of a range cannot be NaN.");
    }
    if (stop !== stop) {
        throw Operators_InvalidArg("m", "The end of a range cannot be NaN.");
    }
    if (start === stop) {
        return singleton(start);
    }
    else {
        const up = start < stop;
        if ((up && (step < 0)) ? true : (!up && (step > 0))) {
            return empty();
        }
        else {
            const before = (a, b) => {
                if (up) {
                    return a < b;
                }
                else {
                    return a > b;
                }
            };
            return unfold((x) => {
                if (x != null) {
                    const x_1 = x;
                    const next = round(x_1 + step);
                    if ((next !== x_1) && ((before(x_1, next) && before(next, stop)) ? true : (next === stop))) {
                        return [x_1, next];
                    }
                    else {
                        return [x_1, undefined];
                    }
                }
                else {
                    return undefined;
                }
            }, start);
        }
    }
}
function floatingRange(round, start, step, stop) {
    return delay(() => {
        if (step === 0) {
            throw Operators_InvalidArg("step", "The step of a range cannot be zero.");
        }
        const numSteps = round(round(stop - start) / step);
        return (((-2147483648 <= numSteps) && (numSteps <= 2147483647)) && (Math.floor(numSteps) === numSteps)) ? unfold((i) => {
            if (i <= numSteps) {
                return [round(start + round(round(i) * step)), i + 1];
            }
            else {
                return undefined;
            }
        }, 0) : floatingRangeStep(round, start, step, stop);
    });
}
export function rangeDouble(start, step, stop) {
    return integralRangeStep(start, step, stop, 0, (x, y) => (x + y));
}
export function rangeFloat(start, step, stop) {
    return floatingRange((x) => x, start, step, stop);
}
export function rangeSingle(start, step, stop) {
    return floatingRange((x) => (Math.fround(x)), start, step, stop);
}
export function rangeInt64(start, step, stop) {
    const start_1 = start;
    const step_1 = step;
    const stop_1 = stop;
    if (equals(step_1, 0n)) {
        throw Operators_InvalidArg("step", "The step of a range cannot be zero.");
    }
    const up = compare_1(step_1, 0n) > 0;
    if ((up && (compare_1(start_1, stop_1) <= 0)) ? true : (!up && (compare_1(start_1, stop_1) >= 0))) {
        return unfold((x_1) => {
            let x = undefined;
            if (x_1 != null) {
                const x_2 = x_1;
                if ((x = x_2, up ? ((compare_1(x, 0n) >= 0) ? (compare_1(toInt64_unchecked(op_Subtraction(stop_1, x)), step_1) >= 0) : (compare_1(toInt64_unchecked(op_Addition(x, step_1)), stop_1) <= 0)) : ((compare_1(x, 0n) <= 0) ? (compare_1(toInt64_unchecked(op_Subtraction(stop_1, x)), step_1) <= 0) : (compare_1(toInt64_unchecked(op_Addition(x, step_1)), stop_1) >= 0)))) {
                    return [x_2, toInt64_unchecked(op_Addition(x_2, step_1))];
                }
                else {
                    return [x_2, undefined];
                }
            }
            else {
                return undefined;
            }
        }, start_1);
    }
    else {
        return empty();
    }
}
export function rangeUInt64(start, step, stop) {
    const start_1 = start;
    const step_1 = step;
    const stop_1 = stop;
    if (equals(step_1, 0n)) {
        throw Operators_InvalidArg("step", "The step of a range cannot be zero.");
    }
    const up = compare_1(step_1, 0n) > 0;
    if ((up && (compare_1(start_1, stop_1) <= 0)) ? true : (!up && (compare_1(start_1, stop_1) >= 0))) {
        return unfold((x_1) => {
            let x = undefined;
            if (x_1 != null) {
                const x_2 = x_1;
                if ((x = x_2, up ? ((compare_1(x, 0n) >= 0) ? (compare_1(toUInt64_unchecked(op_Subtraction(stop_1, x)), step_1) >= 0) : (compare_1(toUInt64_unchecked(op_Addition(x, step_1)), stop_1) <= 0)) : ((compare_1(x, 0n) <= 0) ? (compare_1(toUInt64_unchecked(op_Subtraction(stop_1, x)), step_1) <= 0) : (compare_1(toUInt64_unchecked(op_Addition(x, step_1)), stop_1) >= 0)))) {
                    return [x_2, toUInt64_unchecked(op_Addition(x_2, step_1))];
                }
                else {
                    return [x_2, undefined];
                }
            }
            else {
                return undefined;
            }
        }, start_1);
    }
    else {
        return empty();
    }
}
export function rangeChar(start, stop) {
    const intStop = ~~stop.charCodeAt(0);
    return delay(() => unfold((i) => {
        if (i <= intStop) {
            return [String.fromCharCode(~~i & 0xFFFF), (i + 1) | 0];
        }
        else {
            return undefined;
        }
    }, ~~start.charCodeAt(0)));
}
