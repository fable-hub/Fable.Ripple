
import { record_type, obj_type, string_type, enum_type, int32_type, class_type } from "../../fable_modules/fable-library-js.5.18.0/Reflection.js";
import { Record } from "../../fable_modules/fable-library-js.5.18.0/Types.js";
import { disposeSafe, getEnumerator, Exception, defaultOf, createAtom } from "../../fable_modules/fable-library-js.5.18.0/Util.js";
import { tail, head, isEmpty } from "../../fable_modules/fable-library-js.5.18.0/List.js";
import { Operators_IsNull } from "../../fable_modules/fable-library-js.5.18.0/FSharp.Core.js";
import { Signal_withContext, Signal_batch, Signal_context, Signal_autorun } from "../Fable.Ripple/Api.js";
import { split } from "../../fable_modules/fable-library-js.5.18.0/String.js";
import { item as item_1 } from "../../fable_modules/fable-library-js.5.18.0/Array.js";
import { addToSet } from "../../fable_modules/fable-library-js.5.18.0/MapUtil.js";
import { unionWith } from "../../fable_modules/fable-library-js.5.18.0/Set.js";

/**
 * Runtime-distinct marker so `Html.none` stays separable under `[<Erase>]`
 * (a nullary case would erase to null and collide with the function/Node dispatch).
 */
export class EmptyMarker {
    constructor() {
    }
}

export function EmptyMarker_$reflection() {
    return class_type("Fable.Ripple.Dom.EmptyMarker", undefined, EmptyMarker);
}

export function EmptyMarker_$ctor() {
    return new EmptyMarker();
}

export const Base_emptyMarker = EmptyMarker_$ctor();

export const Base_svgNamespace = "http://www.w3.org/2000/svg";

/**
 * One dynamic part of a row skeleton, captured while the row function ran
 * once: the node it belongs to and what to do on the clone of that node.
 */
export class Base_Recorded extends Record {
    constructor(Node$, Kind, Name, Fn) {
        super();
        this.Node = Node$;
        this.Kind = Kind;
        this.Name = Name;
        this.Fn = Fn;
    }
}

export function Base_Recorded_$reflection() {
    return record_type("Fable.Ripple.Dom.Base.Recorded", [], Base_Recorded, () => [["Node", class_type("Glutinum.Web.Node", undefined)], ["Kind", enum_type("Fable.Ripple.Dom.Base.RecordedKind", int32_type, [["Text", 0], ["On", 1], ["Apply", 2], ["Splice", 3]])], ["Name", string_type], ["Fn", obj_type]]);
}

export let Base_Recording_active = createAtom(false);

export let Base_Recording_dynamic = createAtom(false);

/**
 * True while a template function runs. A helper that would create an
 * effect or a listener must call `markDynamic` instead and return.
 */
export function Base_Recording_isActive() {
    return Base_Recording_active();
}

/**
 * Tell the walker to record the current `Apply` and run it again on every
 * row. Call it, and do nothing else, when `isActive ()` is true.
 */
export function Base_Recording_markDynamic() {
    Base_Recording_dynamic(true);
}

let Base_Recording_list = defaultOf();

let Base_Recording_deps = defaultOf();

export function Base_Recording_start() {
    const prev = [Base_Recording_active(), Base_Recording_list, Base_Recording_deps];
    Base_Recording_active(true);
    Base_Recording_list = [];
    Base_Recording_deps = [];
    return prev;
}

/**
 * The recorded parts and the signals the skeleton was built from.
 */
export function Base_Recording_stop(_arg) {
    const recorded = Base_Recording_list;
    const dependencies = Base_Recording_deps;
    Base_Recording_active(_arg[0]);
    Base_Recording_list = _arg[1];
    Base_Recording_deps = _arg[2];
    return [recorded, dependencies];
}

/**
 * A signal the skeleton depends on: when it changes, the skeleton is
 * stale and the list rebuilds. Hot reload uses it for a component's
 * implementation.
 */
export function Base_Recording_dependOn(signal) {
    void (Base_Recording_deps.push(signal));
}

/**
 * A reactive text node to bind per row.
 */
export function Base_Recording_text(node, read) {
    void (Base_Recording_list.push(new Base_Recorded(node, 0, "", read)));
}

/**
 * An event listener to attach per row.
 */
export function Base_Recording_on(node, name, handler) {
    void (Base_Recording_list.push(new Base_Recorded(node, 1, name, handler)));
}

/**
 * An `Apply` to run per row against the cloned element.
 */
export function Base_Recording_apply(node, run) {
    void (Base_Recording_list.push(new Base_Recorded(node, 2, "", run)));
}

/**
 * A list or dynamic subtree to build per row at the cloned anchor.
 */
export function Base_Recording_splice(anchor, run) {
    void (Base_Recording_list.push(new Base_Recorded(anchor, 3, "", run)));
}

/**
 * `applyItem` while recording: an `Apply` that flagged itself dynamic is
 * recorded with the element it ran against.
 */
export function Base_applyItemRecording(element, item) {
    if (item instanceof Node) {
        element.appendChild(item);
    }
    else if (item instanceof EmptyMarker) {
    }
    else {
        const run = item;
        Base_Recording_dynamic(false);
        run(element);
        if (Base_Recording_dynamic()) {
            Base_Recording_apply(element, run);
            Base_Recording_dynamic(false);
        }
    }
}

/**
 * Apply every item to `element`. A direct cons-cell walk (not `for … in list`,
 * which Fable lowers to an allocating enumerator + try/finally per element).
 */
export function Base_applyItems(element, items) {
    let rest = items;
    if (Base_Recording_active()) {
        while (!isEmpty(rest)) {
            Base_applyItemRecording(element, head(rest));
            rest = tail(rest);
        }
    }
    else {
        while (!isEmpty(rest)) {
            const element_1 = element;
            const item = head(rest);
            if (item instanceof Node) {
                element_1.appendChild(item);
            }
            else if (item instanceof EmptyMarker) {
            }
            else {
                item(element_1);
            }
            rest = tail(rest);
        }
    }
}

/**
 * Create an HTML element, apply every item to it, return it (wrapped as a child).
 */
export function Base_createElement(tag, items) {
    const element = document.createElement(tag);
    Base_applyItems(element, items);
    return element;
}

/**
 * Create a namespaced element (SVG), apply every item, return it as a child.
 */
export function Base_createElementNS(ns, tag, items) {
    const element = document.createElementNS(ns, tag);
    Base_applyItems(element, items);
    return element;
}

/**
 * Record how to repoint whoever holds `node` when it is replaced.
 */
export function Base_trackNode(node, update) {
    node.__rippleOwner = update;
}

/**
 * Repoint whoever holds `oldNode` at `newNode`. Nodes are tracked only in a
 * debug build, so this does nothing in a release one.
 */
export function Base_replaceTrackedNode(oldNode, newNode) {
    const update = oldNode.__rippleOwner;
    if (!Operators_IsNull(update)) {
        update(newNode);
        newNode.__rippleOwner = update;
    }
}

/**
 * Realise a root/child item to its element (used by `mount`/`each`/`dynamic`).
 */
export function Base_toElement(item) {
    if (item instanceof Node) {
        return item;
    }
    else {
        throw new Exception("expected an element item");
    }
}

/**
 * Static attribute via `setAttribute` - works on HTML and SVG alike.
 */
export function Base_attribute(name, value) {
    return (element) => {
        element.setAttribute(name, value);
    };
}

/**
 * Reactive attribute driven by an auto-tracked callback. The DOM is written
 * only when the callback's result differs from the last value written.
 */
export function Base_bindAttribute(name, callback) {
    return (element) => {
        if (Base_Recording_active()) {
            Base_Recording_dynamic(true);
        }
        else {
            let prev = defaultOf();
            Signal_autorun(() => {
                const v = callback();
                if (!(v === prev)) {
                    prev = v;
                    element.setAttribute(name, v);
                }
            });
        }
    };
}

/**
 * Reactive attribute driven by a signal.
 */
export function Base_bindAttributeSignal(name, signal) {
    return Base_bindAttribute(name, () => signal.Value);
}

/**
 * Reactive number-valued attribute. The DOM is written only when the number
 * differs from the last one written.
 */
export function Base_bindNumberAttribute(name, callback) {
    return (element) => {
        if (Base_Recording_active()) {
            Base_Recording_dynamic(true);
        }
        else {
            let prev = Number.NaN;
            Signal_autorun(() => {
                const v = callback();
                if (v !== prev) {
                    prev = v;
                    element.setAttribute(name, v.toString());
                }
            });
        }
    };
}

/**
 * Reactive number-valued attribute driven by a signal.
 */
export function Base_bindNumberAttributeSignal(name, signal) {
    return Base_bindNumberAttribute(name, () => signal.Value);
}

/**
 * Static present/absent boolean attribute (e.g. `required`, `hidden`).
 */
export function Base_booleanAttribute(name, value) {
    return (element) => {
        const element_1 = element;
        const name_1 = name;
        if (value) {
            element_1.setAttribute(name_1, "");
        }
        else {
            element_1.removeAttribute(name_1);
        }
    };
}

/**
 * Reactive present/absent boolean attribute driven by a callback.
 */
export function Base_bindBooleanAttribute(name, callback) {
    return (element) => {
        if (Base_Recording_active()) {
            Base_Recording_dynamic(true);
        }
        else {
            let prev = false;
            let first = true;
            Signal_autorun(() => {
                const v = callback();
                if (first ? true : (v !== prev)) {
                    first = false;
                    prev = v;
                    const element_1 = element;
                    const name_1 = name;
                    if (v) {
                        element_1.setAttribute(name_1, "");
                    }
                    else {
                        element_1.removeAttribute(name_1);
                    }
                }
            });
        }
    };
}

/**
 * Reactive present/absent boolean attribute driven by a signal.
 */
export function Base_bindBooleanAttributeSignal(name, signal) {
    return Base_bindBooleanAttribute(name, () => signal.Value);
}

/**
 * Live DOM *property* (`element[name] = value`), not an attribute - the only way
 * `value`/`checked` reflect the current state rather than the initial default.
 */
export function Base_property(name, value) {
    return (element) => {
        element[name] = value;
    };
}

/**
 * Reactive property driven by an auto-tracked callback.
 */
export function Base_bindProperty(name, callback) {
    return (element) => {
        if (Base_Recording_active()) {
            Base_Recording_dynamic(true);
        }
        else {
            let prev = defaultOf();
            let first = true;
            Signal_autorun(() => {
                const v = callback();
                if (first ? true : !(v === prev)) {
                    first = false;
                    prev = v;
                    element[name] = v;
                }
            });
        }
    };
}

/**
 * Reactive property driven by a signal.
 */
export function Base_bindPropertySignal(name, signal) {
    return Base_bindProperty(name, () => signal.Value);
}

/**
 * Attach an event listener; the handler is cast to its concrete event type
 * (erased) and its writes are auto-batched into one flush per event.
 * Attach a listener whose handler runs batched, and under the row context
 * the listener was created in, if any.
 */
export function Base_listen(element, name, handler) {
    const context = Signal_context();
    element.addEventListener(name, (event) => {
        if (Operators_IsNull(context)) {
            Signal_batch(() => {
                handler(event);
            });
        }
        else {
            Signal_withContext(context, () => {
                Signal_batch(() => {
                    handler(event);
                });
            });
        }
    });
}

export function Base_onEvent(name, handler) {
    return (element) => {
        if (Base_Recording_active()) {
            Base_Recording_on(element, name, (arg) => {
                handler(arg);
            });
        }
        else {
            Base_listen(element, name, (arg_1) => {
                handler(arg_1);
            });
        }
    };
}

function Base_ClassList_setToken(element, enabled, name) {
    const name_1 = name;
    if (name_1.indexOf(" ") < 0) {
        if (name_1.length > 0) {
            const token = name_1;
            if (enabled) {
                element.classList.add(token);
            }
            else {
                element.classList.remove(token);
            }
        }
    }
    else {
        const arr = split(name_1, [" "], undefined, 0);
        for (let idx = 0; idx <= (arr.length - 1); idx++) {
            const token_1 = item_1(idx, arr);
            if (token_1.length > 0) {
                const token = token_1;
                if (enabled) {
                    element.classList.add(token);
                }
                else {
                    element.classList.remove(token);
                }
            }
        }
    }
}

/**
 * Static conditional classes: add the enabled tokens (composes with existing).
 */
export function Base_ClassList_add(pairs) {
    return (element) => {
        let rest = pairs;
        while (!isEmpty(rest)) {
            const patternInput = head(rest);
            if (patternInput[1]) {
                const name_1 = patternInput[0];
                if (name_1.indexOf(" ") < 0) {
                    if (name_1.length > 0) {
                        element.classList.add(name_1);
                    }
                }
                else {
                    const arr = split(name_1, [" "], undefined, 0);
                    for (let idx = 0; idx <= (arr.length - 1); idx++) {
                        const token_1 = item_1(idx, arr);
                        if (token_1.length > 0) {
                            element.classList.add(token_1);
                        }
                    }
                }
            }
            rest = tail(rest);
        }
    };
}

/**
 * Reactive conditional classes: diff the enabled token set against the one this
 * binding last applied, toggling only what changed - other tokens are untouched.
 */
export function Base_ClassList_bind(callback) {
    return (element) => {
        if (Base_Recording_active()) {
            Base_Recording_dynamic(true);
        }
        else {
            const applied = new Set([]);
            Signal_autorun(() => {
                const next = new Set([]);
                let rest = callback();
                while (!isEmpty(rest)) {
                    const patternInput = head(rest);
                    if (patternInput[1]) {
                        const name_1 = patternInput[0];
                        if (name_1.indexOf(" ") < 0) {
                            if (name_1.length > 0) {
                                addToSet(name_1, next);
                            }
                        }
                        else {
                            const arr = split(name_1, [" "], undefined, 0);
                            for (let idx = 0; idx <= (arr.length - 1); idx++) {
                                const token_1 = item_1(idx, arr);
                                if (token_1.length > 0) {
                                    addToSet(token_1, next);
                                }
                            }
                        }
                    }
                    rest = tail(rest);
                }
                let enumerator = getEnumerator(applied);
                try {
                    while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                        const token_2 = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                        if (!next.has(token_2)) {
                            element.classList.remove(token_2);
                        }
                    }
                }
                finally {
                    disposeSafe(enumerator);
                }
                let enumerator_1 = getEnumerator(next);
                try {
                    while (enumerator_1["System.Collections.IEnumerator.MoveNext"]()) {
                        const token_3 = enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"]();
                        if (!applied.has(token_3)) {
                            element.classList.add(token_3);
                        }
                    }
                }
                finally {
                    disposeSafe(enumerator_1);
                }
                applied.clear();
                unionWith(applied, next);
            });
        }
    };
}

/**
 * Toggle a single class token by a static flag.
 */
export function Base_ClassList_toggle(name, enabled) {
    return (element) => {
        Base_ClassList_setToken(element, enabled, name);
    };
}

/**
 * Toggle a single class token by a reactive flag.
 */
export function Base_ClassList_bindToggle(name, callback) {
    return (element) => {
        if (Base_Recording_active()) {
            Base_Recording_dynamic(true);
        }
        else {
            let prev = false;
            let first = true;
            Signal_autorun(() => {
                const v = callback();
                if (first ? true : (v !== prev)) {
                    first = false;
                    prev = v;
                    Base_ClassList_setToken(element, v, name);
                }
            });
        }
    };
}

