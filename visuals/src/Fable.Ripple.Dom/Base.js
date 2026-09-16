
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { tail, head, isEmpty } from "../../fable_modules/fable-library-js.5.13.0/List.js";
import { disposeSafe, getEnumerator, Exception } from "../../fable_modules/fable-library-js.5.13.0/Util.js";
import { Signal_batch, Signal_effect } from "../Fable.Ripple/Api.js";
import { Var$1__get_Value } from "../Fable.Ripple/Types.js";
import { split } from "../../fable_modules/fable-library-js.5.13.0/String.js";
import { item as item_1 } from "../../fable_modules/fable-library-js.5.13.0/Array.js";
import { addToSet } from "../../fable_modules/fable-library-js.5.13.0/MapUtil.js";
import { unionWith } from "../../fable_modules/fable-library-js.5.13.0/Set.js";

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
 * Apply every item to `element`. A direct cons-cell walk (not `for … in list`,
 * which Fable lowers to an allocating enumerator + try/finally per element).
 */
export function Base_applyItems(element, items) {
    let rest = items;
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
 * Reactive attribute driven by an auto-tracked callback.
 */
export function Base_bindAttribute(name, callback) {
    return (element) => {
        Signal_effect(() => {
            element.setAttribute(name, callback());
        });
    };
}

/**
 * Reactive attribute driven by a signal.
 */
export function Base_bindAttributeSignal(name, signal) {
    return Base_bindAttribute(name, () => Var$1__get_Value(signal));
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
        Signal_effect(() => {
            const element_1 = element;
            const name_1 = name;
            if (callback()) {
                element_1.setAttribute(name_1, "");
            }
            else {
                element_1.removeAttribute(name_1);
            }
        });
    };
}

/**
 * Reactive present/absent boolean attribute driven by a signal.
 */
export function Base_bindBooleanAttributeSignal(name, signal) {
    return Base_bindBooleanAttribute(name, () => Var$1__get_Value(signal));
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
        Signal_effect(() => {
            element[name] = callback();
        });
    };
}

/**
 * Reactive property driven by a signal.
 */
export function Base_bindPropertySignal(name, signal) {
    return Base_bindProperty(name, () => Var$1__get_Value(signal));
}

/**
 * Attach an event listener; the handler is cast to its concrete event type
 * (erased) and its writes are auto-batched into one flush per event.
 */
export function Base_onEvent(name, handler) {
    return (element) => {
        element.addEventListener(name, (event) => {
            Signal_batch(() => {
                handler(event);
            });
        });
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
        const applied = new Set([]);
        Signal_effect(() => {
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
        Signal_effect(() => {
            Base_ClassList_setToken(element, callback(), name);
        });
    };
}

