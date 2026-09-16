
import { disposeSafe, defaultOf } from "../../fable_modules/fable-library-js.5.13.0/Util.js";
import { Var_create } from "../Fable.Ripple/Api.js";
import { Var$1__set_Value_2B595 } from "../Fable.Ripple/Types.js";
import { Signal_computed } from "../Fable.Ripple/Api.js";
import { Var$1__get_Value } from "../Fable.Ripple/Types.js";
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";

/**
 * Push `url` onto the history stack and notify any listening router.
 */
export function Advanced_newUrl(url) {
    history.pushState(defaultOf(), "", url);
    window.dispatchEvent(new CustomEvent("signals:navigated"));
}

/**
 * Replace the current history entry with `url` (no new stack entry).
 */
export function Advanced_modifyUrl(url) {
    history.replaceState(defaultOf(), "", url);
    window.dispatchEvent(new CustomEvent("signals:navigated"));
}

/**
 * Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
 */
export function Advanced_jump(n) {
    history.go(n);
}

/**
 * Reactive hash-based location. Returns a `Signal<string>` that always reflects
 * `location.hash`, plus an `IDisposable` that tears the listeners down.
 */
export function Advanced_useHash() {
    const current = Var_create(window.location.hash);
    let lastHref = window.location.href;
    const onLocationChange = (_arg) => {
        const href = window.location.href;
        if (href !== lastHref) {
            lastHref = href;
            Var$1__set_Value_2B595(current, window.location.hash);
        }
    };
    window.addEventListener("popstate", onLocationChange);
    window.addEventListener("hashchange", onLocationChange);
    window.addEventListener("signals:navigated", onLocationChange);
    return [current, {
        Dispose() {
            window.removeEventListener("popstate", onLocationChange);
            window.removeEventListener("hashchange", onLocationChange);
            window.removeEventListener("signals:navigated", onLocationChange);
        },
    }];
}

/**
 * Reactive path-based (HTML5 history) location. Returns a `Signal<string>` that
 * tracks `pathname + search`, plus an `IDisposable` that tears the listeners down.
 */
export function Advanced_usePath() {
    const rebuildPathFromLocation = (location) => (location.pathname + location.search);
    const current = Var_create(rebuildPathFromLocation(window.location));
    let lastHref = window.location.href;
    const onLocationChange = (_arg) => {
        const href = window.location.href;
        if (href !== lastHref) {
            lastHref = href;
            Var$1__set_Value_2B595(current, rebuildPathFromLocation(window.location));
        }
    };
    window.addEventListener("popstate", onLocationChange);
    window.addEventListener("signals:navigated", onLocationChange);
    return [current, {
        Dispose() {
            window.removeEventListener("popstate", onLocationChange);
            window.removeEventListener("signals:navigated", onLocationChange);
        },
    }];
}

/**
 * A hash-based router that can both parse and build URLs.
 */
export class HashRouter$1 {
    constructor(parse, toUrl) {
        this.toUrl = toUrl;
        const patternInput = Advanced_useHash();
        this.disposable = patternInput[1];
        this.currentRoute = Signal_computed(() => parse(Var$1__get_Value(patternInput[0])));
    }
    Dispose() {
        const _ = this;
        disposeSafe(_.disposable);
    }
}

export function HashRouter$1_$reflection(gen0) {
    return class_type("Fable.Ripple.Dom.Routing.HashRouter`1", [gen0], HashRouter$1);
}

export function HashRouter$1_$ctor_601EBCC3(parse, toUrl) {
    return new HashRouter$1(parse, toUrl);
}

/**
 * The current parsed route, re-evaluated on every navigation.
 * `None` if the current hash does not match any known route.
 */
export function HashRouter$1__get_CurrentRoute(_) {
    return _.currentRoute;
}

/**
 * The hash string for the given route, e.g. for use as an <c>&lt;a href&gt;</c>.
 */
export function HashRouter$1__Href_2B595(_, route) {
    return _.toUrl(route);
}

/**
 * Push the URL for the given route onto the history stack.
 */
export function HashRouter$1__NewUrl_2B595(_, route) {
    Advanced_newUrl(_.toUrl(route));
}

/**
 * Replace the current history entry with the URL for the given route.
 */
export function HashRouter$1__ModifyUrl_2B595(_, route) {
    Advanced_modifyUrl(_.toUrl(route));
}

/**
 * Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
 */
export function HashRouter$1__Jump_Z524259A4(_, n) {
    Advanced_jump(n);
}

/**
 * A path-based (HTML5 history) router that can both parse and build URLs.
 */
export class PathRouter$1 {
    constructor(parse, toUrl) {
        this.toUrl = toUrl;
        const patternInput = Advanced_usePath();
        this.disposable = patternInput[1];
        this.currentRoute = Signal_computed(() => parse(Var$1__get_Value(patternInput[0])));
    }
    Dispose() {
        const _ = this;
        disposeSafe(_.disposable);
    }
}

export function PathRouter$1_$reflection(gen0) {
    return class_type("Fable.Ripple.Dom.Routing.PathRouter`1", [gen0], PathRouter$1);
}

export function PathRouter$1_$ctor_601EBCC3(parse, toUrl) {
    return new PathRouter$1(parse, toUrl);
}

/**
 * The current parsed route, re-evaluated on every navigation.
 * `None` if the current pathname does not match any known route.
 */
export function PathRouter$1__get_CurrentRoute(_) {
    return _.currentRoute;
}

/**
 * The path string for the given route, e.g. for use as an <c>&lt;a href&gt;</c>.
 */
export function PathRouter$1__Href_2B595(_, route) {
    return _.toUrl(route);
}

/**
 * Push the URL for the given route onto the history stack.
 */
export function PathRouter$1__NewUrl_2B595(_, route) {
    Advanced_newUrl(_.toUrl(route));
}

/**
 * Replace the current history entry with the URL for the given route.
 */
export function PathRouter$1__ModifyUrl_2B595(_, route) {
    Advanced_modifyUrl(_.toUrl(route));
}

/**
 * Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
 */
export function PathRouter$1__Jump_Z524259A4(_, n) {
    Advanced_jump(n);
}

/**
 * A parse-only hash-based router.
 */
export class Simple_HashRouter$1 {
    constructor(parse) {
        const patternInput = Advanced_useHash();
        this.disposable = patternInput[1];
        this.currentRoute = Signal_computed(() => parse(Var$1__get_Value(patternInput[0])));
    }
    Dispose() {
        const _ = this;
        disposeSafe(_.disposable);
    }
}

export function Simple_HashRouter$1_$reflection(gen0) {
    return class_type("Fable.Ripple.Dom.Routing.Simple.HashRouter`1", [gen0], Simple_HashRouter$1);
}

export function Simple_HashRouter$1_$ctor_Z3C74DFC5(parse) {
    return new Simple_HashRouter$1(parse);
}

/**
 * The current parsed route, re-evaluated on every navigation.
 * `None` if the current hash does not match any known route.
 */
export function Simple_HashRouter$1__get_CurrentRoute(_) {
    return _.currentRoute;
}

/**
 * Push a new URL onto the history stack.
 */
export function Simple_HashRouter$1__NewUrl_Z721C83C5(_, url) {
    Advanced_newUrl(url);
}

/**
 * Replace the current history entry without adding a new one.
 */
export function Simple_HashRouter$1__ModifyUrl_Z721C83C5(_, url) {
    Advanced_modifyUrl(url);
}

/**
 * Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
 */
export function Simple_HashRouter$1__Jump_Z524259A4(_, n) {
    Advanced_jump(n);
}

/**
 * A parse-only path-based (HTML5 history) router.
 */
export class Simple_PathRouter$1 {
    constructor(parse) {
        const patternInput = Advanced_usePath();
        this.disposable = patternInput[1];
        this.currentRoute = Signal_computed(() => parse(Var$1__get_Value(patternInput[0])));
    }
    Dispose() {
        const _ = this;
        disposeSafe(_.disposable);
    }
}

export function Simple_PathRouter$1_$reflection(gen0) {
    return class_type("Fable.Ripple.Dom.Routing.Simple.PathRouter`1", [gen0], Simple_PathRouter$1);
}

export function Simple_PathRouter$1_$ctor_Z3C74DFC5(parse) {
    return new Simple_PathRouter$1(parse);
}

/**
 * The current parsed route, re-evaluated on every navigation.
 * `None` if the current pathname does not match any known route.
 */
export function Simple_PathRouter$1__get_CurrentRoute(_) {
    return _.currentRoute;
}

/**
 * Push a new URL onto the history stack.
 */
export function Simple_PathRouter$1__NewUrl_Z721C83C5(_, url) {
    Advanced_newUrl(url);
}

/**
 * Replace the current history entry without adding a new one.
 */
export function Simple_PathRouter$1__ModifyUrl_Z721C83C5(_, url) {
    Advanced_modifyUrl(url);
}

/**
 * Jump n steps in the history stack (e.g. -1 for back, 1 for forward).
 */
export function Simple_PathRouter$1__Jump_Z524259A4(_, n) {
    Advanced_jump(n);
}

