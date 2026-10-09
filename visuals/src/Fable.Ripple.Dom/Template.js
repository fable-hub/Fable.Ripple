
import { Signal_setContext, Signal_autorun, Signal_context } from "../Fable.Ripple/Api.js";
import { Operators_IsNull } from "../../fable_modules/fable-library-js.5.18.0/FSharp.Core.js";
import { defaultOf, comparePrimitives, Exception } from "../../fable_modules/fable-library-js.5.18.0/Util.js";
import { class_type } from "../../fable_modules/fable-library-js.5.18.0/Reflection.js";
import { value } from "../../fable_modules/fable-library-js.5.18.0/Option.js";
import { min } from "../../fable_modules/fable-library-js.5.18.0/Double.js";
import { initialize, item } from "../../fable_modules/fable-library-js.5.18.0/Array.js";
import { Base_toElement, Base_Recording_stop, Base_Recording_start, Base_listen } from "./Base.js";

/**
 * The row handed to an `Html.template` render function: `Value` is the row being
 * built or updated. Read it inside a binding, never at the top level of the
 * render function, which runs once, and not after a callback has returned.
 */
export class RowSignal$1 {
    constructor() {
    }
    get Value() {
        const row = Signal_context();
        if (Operators_IsNull(row)) {
            throw new Exception("Html.template: the row was read with no row active. At the top level of the render function, wrap the read in a binding: Html.text (fun () -> ...). In code that runs later (a timer, a promise, an observer), read the row into a local first: let item = row.Value.");
        }
        return row;
    }
    Peek() {
        const this$ = this;
        return this$.Value;
    }
}

export function RowSignal$1_$reflection(gen0) {
    return class_type("Fable.Ripple.Dom.RowSignal`1", [gen0], RowSignal$1);
}

export function RowSignal$1_$ctor() {
    return new RowSignal$1();
}

const Template_opDown = 0;

const Template_opNext = 1;

const Template_opUp = 2;

function Template_pathOf(root, node) {
    const path = [];
    let n = node;
    while (!(n === root)) {
        let index = 0;
        let s = n.previousSibling;
        while (s != null) {
            index = ((index + 1) | 0);
            s = value(s).previousSibling;
        }
        void (path.push(index));
        n = value(n.parentNode);
    }
    path.reverse();
    return path;
}

function Template_comparePaths(a, b) {
    const n = min(a.length, b.length) | 0;
    let i = 0;
    let result = 0;
    while ((result === 0) && (i < n)) {
        result = (comparePrimitives(item(i, a), item(i, b)) | 0);
        i = ((i + 1) | 0);
    }
    if (result !== 0) {
        return result | 0;
    }
    else {
        return comparePrimitives(a.length, b.length) | 0;
    }
}

function Template_planOf(root, recorded) {
    const paths = initialize(recorded.length, (i) => Template_pathOf(root, item(i, recorded).Node));
    const order = initialize(recorded.length, (x) => (x | 0), Int32Array);
    order.sort((i_1, j) => {
        const matchValue = Template_comparePaths(item(i_1, paths), item(j, paths)) | 0;
        return ((matchValue === 0) ? comparePrimitives(i_1, j) : matchValue) | 0;
    });
    const plan = [];
    const bindings = [];
    let at = [];
    for (let idx = 0; idx <= (order.length - 1); idx++) {
        const k = item(idx, order) | 0;
        const target = item(k, paths);
        let common = 0;
        while (((common < at.length) && (common < target.length)) && (item(common, at) === item(common, target))) {
            common = ((common + 1) | 0);
        }
        let depth = common;
        if (at.length > common) {
            for (let forLoopVar = 1; forLoopVar <= ((at.length - common) - 1); forLoopVar++) {
                void (plan.push(Template_opUp));
            }
            for (let forLoopVar_1 = 1; forLoopVar_1 <= (item(common, target) - item(common, at)); forLoopVar_1++) {
                void (plan.push(Template_opNext));
            }
            depth = ((common + 1) | 0);
        }
        while (depth < target.length) {
            void (plan.push(Template_opDown));
            for (let forLoopVar_2 = 1; forLoopVar_2 <= item(depth, target); forLoopVar_2++) {
                void (plan.push(Template_opNext));
            }
            depth = ((depth + 1) | 0);
        }
        void (plan.push(3 + bindings.length));
        void (bindings.push(item(k, recorded)));
        at = target;
    }
    return [plan, bindings];
}

function Template_apply(binding, node, row) {
    const matchValue = binding.Kind;
    switch (matchValue) {
        case 0: {
            let prev = defaultOf();
            Signal_autorun(() => {
                const v = binding.Fn();
                if (!(v === prev)) {
                    prev = v;
                    node.nodeValue = v;
                }
            });
            break;
        }
        case 1: {
            Base_listen(node, binding.Name, binding.Fn);
            break;
        }
        case 2: {
            binding.Fn(node);
            break;
        }
        case 3: {
            binding.Fn(node);
            break;
        }
        default:
            undefined;
    }
}

/**
 * Clone the skeleton and replay the plan for `row`, under `row` as context.
 */
export function Template_realise(proto, plan, bindings, row) {
    const root = proto.cloneNode(true);
    let node = root;
    let i = 0;
    const context = row;
    const saved = Signal_context();
    Signal_setContext(context);
    try {
        while (i < plan.length) {
            const op = (plan[i]) | 0;
            if (op === Template_opDown) {
                node = node.firstChild;
            }
            else if (op === Template_opNext) {
                node = node.nextSibling;
            }
            else if (op === Template_opUp) {
                node = node.parentNode;
            }
            else {
                Template_apply(bindings[(op - 3)], node, context);
            }
            i = ((i + 1) | 0);
        }
    }
    finally {
        Signal_setContext(saved);
    }
    return root;
}

/**
 * Run `render` once against a `RowSignal`, recording the dynamic parts, and
 * return a row builder that clones the result, with the signals the skeleton
 * was built from.
 */
export function Template_compile(render) {
    const prev = Base_Recording_start();
    let recorded = defaultOf();
    let dependencies = defaultOf();
    let proto;
    try {
        proto = Base_toElement(render(RowSignal$1_$ctor()));
    }
    finally {
        const patternInput = Base_Recording_stop(prev);
        recorded = patternInput[0];
        dependencies = patternInput[1];
    }
    const patternInput_1 = Template_planOf(proto, recorded);
    return [(row) => Template_realise(proto, patternInput_1[0], patternInput_1[1], row), dependencies];
}

