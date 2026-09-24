
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Base_bindAttributeSignal, Base_bindPropertySignal, Base_booleanAttribute, Base_bindProperty, Base_property, Base_ClassList_bindToggle, Base_ClassList_toggle, Base_ClassList_bind, Base_ClassList_add, Base_bindAttribute, Base_attribute } from "./Base.js";
import { join } from "../../fable_modules/fable-library-js.5.13.0/String.js";
import { map } from "../../fable_modules/fable-library-js.5.13.0/List.js";
import { Signal_batch, Signal_effect } from "../Fable.Ripple/Api.js";
import { Var$1__set_Value_2B595, Var$1__get_Value } from "../Fable.Ripple/Types.js";
import { int32ToString } from "../../fable_modules/fable-library-js.5.13.0/Util.js";

/**
 * HTML attributes and properties. Value-bearing attributes take a plain literal;
 * the *interactive* ones (value, checked, disabled, hidden, class, style, ...)
 * also take a `Signal<'T>` or a `unit -> 'T` thunk so they update in place. Any
 * other attribute can be made reactive through the generic `attr.custom` overloads.
 */
export class attr {
    constructor() {
    }
}

export function attr_$reflection() {
    return class_type("Fable.Ripple.Dom.attr", undefined, attr);
}

export function attr_className_Z721C83C5(v) {
    return Base_attribute("class", v);
}

export function attr_className_5106B011(f) {
    return Base_bindAttribute("class", f);
}

export function attr_className_Z108492D4(s) {
    return Base_bindAttribute("class", () => s.Value);
}

/**
 * Join a list of class names with spaces.
 */
export function attr_classes_7F866359(names) {
    return Base_attribute("class", join(" ", names));
}

/**
 * Reactive class list: re-joins whenever the signals it reads change.
 */
export function attr_classes_Z69D040CD(f) {
    return Base_bindAttribute("class", () => join(" ", f()));
}

/**
 * Conditional classes (clsx / Solid `classList` style): keep each name whose
 * flag is true. E.g. `[ "btn", true; "active", isActive; "disabled", busy ]`.
 * Adds the tokens via the `classList` DOM API, so it composes with a base
 * `class` (and other bindings) instead of overwriting the whole attribute.
 */
export function attr_classList_Z30A5DE4(pairs) {
    return Base_ClassList_add(pairs);
}

/**
 * Reactive conditional classes: re-evaluates the flags whenever the signals
 * they read change, toggling only the tokens that actually changed.
 * E.g. `attr.classList (fun () -> [ "active", isActive.Value ])`.
 */
export function attr_classList_ZA225E0A(f) {
    return Base_ClassList_bind(f);
}

/**
 * Add or remove a single class token by a flag - composes with other class
 * sources (like Sutil `toggleClass` / WebSharper `DynamicClass`).
 */
export function attr_toggleClass_Z55EFCE8F(name, on) {
    return Base_ClassList_toggle(name, on);
}

/**
 * Reactive single-token toggle: flips the class whenever the flag changes.
 */
export function attr_toggleClass_69695ADB(name, cond) {
    return Base_ClassList_bindToggle(name, cond);
}

/**
 * Single-token toggle driven directly by a `Var`/`Signal<bool>`.
 */
export function attr_toggleClass_Z7BA3283A(name, cond) {
    return Base_ClassList_bindToggle(name, () => cond.Value);
}

export function attr_style_Z721C83C5(v) {
    return Base_attribute("style", v);
}

export function attr_style_5106B011(f) {
    return Base_bindAttribute("style", f);
}

export function attr_style_Z108492D4(s) {
    return Base_bindAttribute("style", () => s.Value);
}

/**
 * Build the `style` string from `property, value` pairs.
 */
export function attr_style_Z51B1D053(props) {
    return Base_attribute("style", join(";", map((tupledArg) => ((tupledArg[0] + ":") + tupledArg[1]), props)));
}

export function attr_value_Z721C83C5(v) {
    return Base_property("value", v);
}

export function attr_value_Z524259A4(v) {
    return Base_property("value", v);
}

export function attr_value_5E38073B(v) {
    return Base_property("value", v);
}

export function attr_value_5106B011(f) {
    return Base_bindProperty("value", f);
}

export function attr_checked$0027_Z1FBCCD16(v) {
    return Base_property("checked", v);
}

export function attr_checked$0027_233A5940(f) {
    return Base_bindProperty("checked", f);
}

export function attr_checked$0027_Z31F02BA3(s) {
    return Base_bindProperty("checked", () => s.Value);
}

export function attr_disabled_Z1FBCCD16(v) {
    return Base_property("disabled", v);
}

export function attr_disabled_233A5940(f) {
    return Base_bindProperty("disabled", f);
}

export function attr_disabled_Z31F02BA3(s) {
    return Base_bindProperty("disabled", () => s.Value);
}

export function attr_hidden_Z1FBCCD16(v) {
    return Base_property("hidden", v);
}

export function attr_hidden_233A5940(f) {
    return Base_bindProperty("hidden", f);
}

export function attr_hidden_Z31F02BA3(s) {
    return Base_bindProperty("hidden", () => s.Value);
}

export function attr_selected_Z1FBCCD16(v) {
    return Base_property("selected", v);
}

export function attr_selected_233A5940(f) {
    return Base_bindProperty("selected", f);
}

export function attr_selected_Z31F02BA3(s) {
    return Base_bindProperty("selected", () => s.Value);
}

export function attr_readOnly_Z1FBCCD16(v) {
    return Base_property("readOnly", v);
}

export function attr_readOnly_233A5940(f) {
    return Base_bindProperty("readOnly", f);
}

export function attr_readOnly_Z31F02BA3(s) {
    return Base_bindProperty("readOnly", () => s.Value);
}

export function attr_required_Z1FBCCD16(v) {
    return Base_property("required", v);
}

export function attr_required_233A5940(f) {
    return Base_bindProperty("required", f);
}

export function attr_required_Z31F02BA3(s) {
    return Base_bindProperty("required", () => s.Value);
}

export function attr_multiple_Z1FBCCD16(v) {
    return Base_property("multiple", v);
}

export function attr_multiple_233A5940(f) {
    return Base_bindProperty("multiple", f);
}

export function attr_multiple_Z31F02BA3(s) {
    return Base_bindProperty("multiple", () => s.Value);
}

export function attr_isOpen_Z1FBCCD16(v) {
    return Base_property("open", v);
}

export function attr_isOpen_233A5940(f) {
    return Base_bindProperty("open", f);
}

export function attr_isOpen_Z31F02BA3(s) {
    return Base_bindProperty("open", () => s.Value);
}

/**
 * Two-way text binding: input follows the var, typing writes back.
 */
export function attr_bindValue_Z5BF31D29(c) {
    return (e) => {
        const inp = e;
        Signal_effect(() => {
            inp.value = Var$1__get_Value(c);
        });
        e.addEventListener("input", (_arg) => {
            Signal_batch(() => {
                Var$1__set_Value_2B595(c, inp.value);
            });
        });
    };
}

/**
 * Two-way checkbox binding.
 */
export function attr_bindChecked_5AB39E06(c) {
    return (e) => {
        const inp = e;
        Signal_effect(() => {
            inp.checked = Var$1__get_Value(c);
        });
        e.addEventListener("change", (_arg) => {
            Signal_batch(() => {
                Var$1__set_Value_2B595(c, inp.checked);
            });
        });
    };
}

export function attr_id_Z721C83C5(v) {
    return Base_attribute("id", v);
}

export function attr_title_Z721C83C5(v) {
    return Base_attribute("title", v);
}

export function attr_lang_Z721C83C5(v) {
    return Base_attribute("lang", v);
}

export function attr_dir_Z721C83C5(v) {
    return Base_attribute("dir", v);
}

export function attr_slot_Z721C83C5(v) {
    return Base_attribute("slot", v);
}

export function attr_role_Z721C83C5(v) {
    return Base_attribute("role", v);
}

export function attr_accessKey_Z721C83C5(v) {
    return Base_attribute("accesskey", v);
}

export function attr_tabIndex_Z524259A4(v) {
    return Base_attribute("tabindex", int32ToString(v));
}

export function attr_inputMode_Z721C83C5(v) {
    return Base_attribute("inputmode", v);
}

export function attr_enterKeyHint_Z721C83C5(v) {
    return Base_attribute("enterkeyhint", v);
}

export function attr_autoCapitalize_Z721C83C5(v) {
    return Base_attribute("autocapitalize", v);
}

export function attr_translate_Z1FBCCD16(v) {
    return Base_attribute("translate", v ? "yes" : "no");
}

export function attr_draggable_Z1FBCCD16(v) {
    return Base_attribute("draggable", v ? "true" : "false");
}

export function attr_spellcheck_Z1FBCCD16(v) {
    return Base_attribute("spellcheck", v ? "true" : "false");
}

export function attr_contentEditable_Z1FBCCD16(v) {
    return Base_attribute("contenteditable", v ? "true" : "false");
}

export function attr_contentEditable_Z721C83C5(v) {
    return Base_attribute("contenteditable", v);
}

export function attr_href_Z721C83C5(v) {
    return Base_attribute("href", v);
}

export function attr_hrefLang_Z721C83C5(v) {
    return Base_attribute("hreflang", v);
}

export function attr_target_Z721C83C5(v) {
    return Base_attribute("target", v);
}

export function attr_rel_Z721C83C5(v) {
    return Base_attribute("rel", v);
}

export function attr_download_Z721C83C5(v) {
    return Base_attribute("download", v);
}

export function attr_ping_Z721C83C5(v) {
    return Base_attribute("ping", v);
}

export function attr_referrerPolicy_Z721C83C5(v) {
    return Base_attribute("referrerpolicy", v);
}

export function attr_src_Z721C83C5(v) {
    return Base_attribute("src", v);
}

export function attr_srcset_Z721C83C5(v) {
    return Base_attribute("srcset", v);
}

export function attr_sizes_Z721C83C5(v) {
    return Base_attribute("sizes", v);
}

export function attr_alt_Z721C83C5(v) {
    return Base_attribute("alt", v);
}

export function attr_media_Z721C83C5(v) {
    return Base_attribute("media", v);
}

export function attr_crossOrigin_Z721C83C5(v) {
    return Base_attribute("crossorigin", v);
}

export function attr_decoding_Z721C83C5(v) {
    return Base_attribute("decoding", v);
}

export function attr_loading_Z721C83C5(v) {
    return Base_attribute("loading", v);
}

export function attr_useMap_Z721C83C5(v) {
    return Base_attribute("usemap", v);
}

export function attr_poster_Z721C83C5(v) {
    return Base_attribute("poster", v);
}

export function attr_preload_Z721C83C5(v) {
    return Base_attribute("preload", v);
}

export function attr_kind_Z721C83C5(v) {
    return Base_attribute("kind", v);
}

export function attr_srcLang_Z721C83C5(v) {
    return Base_attribute("srclang", v);
}

export function attr_allow_Z721C83C5(v) {
    return Base_attribute("allow", v);
}

export function attr_sandbox_Z721C83C5(v) {
    return Base_attribute("sandbox", v);
}

export function attr_srcdoc_Z721C83C5(v) {
    return Base_attribute("srcdoc", v);
}

export function attr_coords_Z721C83C5(v) {
    return Base_attribute("coords", v);
}

export function attr_shape_Z721C83C5(v) {
    return Base_attribute("shape", v);
}

export function attr_name_Z721C83C5(v) {
    return Base_attribute("name", v);
}

export function attr_type$0027_Z721C83C5(v) {
    return Base_attribute("type", v);
}

export function attr_placeholder_Z721C83C5(v) {
    return Base_attribute("placeholder", v);
}

export function attr_pattern_Z721C83C5(v) {
    return Base_attribute("pattern", v);
}

export function attr_autoComplete_Z721C83C5(v) {
    return Base_attribute("autocomplete", v);
}

export function attr_htmlFor_Z721C83C5(v) {
    return Base_attribute("for", v);
}

export function attr_dirName_Z721C83C5(v) {
    return Base_attribute("dirname", v);
}

export function attr_label_Z721C83C5(v) {
    return Base_attribute("label", v);
}

export function attr_list_Z721C83C5(v) {
    return Base_attribute("list", v);
}

export function attr_accept_Z721C83C5(v) {
    return Base_attribute("accept", v);
}

export function attr_capture_Z721C83C5(v) {
    return Base_attribute("capture", v);
}

export function attr_wrap_Z721C83C5(v) {
    return Base_attribute("wrap", v);
}

export function attr_action_Z721C83C5(v) {
    return Base_attribute("action", v);
}

export function attr_method$0027_Z721C83C5(v) {
    return Base_attribute("method", v);
}

export function attr_encType_Z721C83C5(v) {
    return Base_attribute("enctype", v);
}

export function attr_acceptCharset_Z721C83C5(v) {
    return Base_attribute("accept-charset", v);
}

export function attr_form_Z721C83C5(v) {
    return Base_attribute("form", v);
}

export function attr_formAction_Z721C83C5(v) {
    return Base_attribute("formaction", v);
}

export function attr_formMethod_Z721C83C5(v) {
    return Base_attribute("formmethod", v);
}

export function attr_formEncType_Z721C83C5(v) {
    return Base_attribute("formenctype", v);
}

export function attr_formTarget_Z721C83C5(v) {
    return Base_attribute("formtarget", v);
}

export function attr_min_Z721C83C5(v) {
    return Base_attribute("min", v);
}

export function attr_min_Z524259A4(v) {
    return Base_attribute("min", int32ToString(v));
}

export function attr_max_Z721C83C5(v) {
    return Base_attribute("max", v);
}

export function attr_max_Z524259A4(v) {
    return Base_attribute("max", int32ToString(v));
}

export function attr_step_Z721C83C5(v) {
    return Base_attribute("step", v);
}

export function attr_step_Z524259A4(v) {
    return Base_attribute("step", int32ToString(v));
}

export function attr_minLength_Z524259A4(v) {
    return Base_attribute("minlength", int32ToString(v));
}

export function attr_maxLength_Z524259A4(v) {
    return Base_attribute("maxlength", int32ToString(v));
}

export function attr_size_Z524259A4(v) {
    return Base_attribute("size", int32ToString(v));
}

export function attr_rows_Z524259A4(v) {
    return Base_attribute("rows", int32ToString(v));
}

export function attr_cols_Z524259A4(v) {
    return Base_attribute("cols", int32ToString(v));
}

export function attr_low_5E38073B(v) {
    return Base_attribute("low", v.toString());
}

export function attr_high_5E38073B(v) {
    return Base_attribute("high", v.toString());
}

export function attr_optimum_5E38073B(v) {
    return Base_attribute("optimum", v.toString());
}

export function attr_autoFocus_Z1FBCCD16(v) {
    return Base_booleanAttribute("autofocus", v);
}

export function attr_autoPlay_Z1FBCCD16(v) {
    return Base_booleanAttribute("autoplay", v);
}

export function attr_controls_Z1FBCCD16(v) {
    return Base_booleanAttribute("controls", v);
}

export function attr_loop_Z1FBCCD16(v) {
    return Base_booleanAttribute("loop", v);
}

export function attr_muted_Z1FBCCD16(v) {
    return Base_booleanAttribute("muted", v);
}

export function attr_playsInline_Z1FBCCD16(v) {
    return Base_booleanAttribute("playsinline", v);
}

export function attr_reversed_Z1FBCCD16(v) {
    return Base_booleanAttribute("reversed", v);
}

export function attr_isDefault_Z1FBCCD16(v) {
    return Base_booleanAttribute("default", v);
}

export function attr_isMap_Z1FBCCD16(v) {
    return Base_booleanAttribute("ismap", v);
}

export function attr_allowFullScreen_Z1FBCCD16(v) {
    return Base_booleanAttribute("allowfullscreen", v);
}

export function attr_noValidate_Z1FBCCD16(v) {
    return Base_booleanAttribute("novalidate", v);
}

export function attr_formNoValidate_Z1FBCCD16(v) {
    return Base_booleanAttribute("formnovalidate", v);
}

export function attr_async$0027_Z1FBCCD16(v) {
    return Base_booleanAttribute("async", v);
}

export function attr_defer_Z1FBCCD16(v) {
    return Base_booleanAttribute("defer", v);
}

export function attr_width_Z524259A4(v) {
    return Base_attribute("width", int32ToString(v));
}

export function attr_width_Z721C83C5(v) {
    return Base_attribute("width", v);
}

export function attr_height_Z524259A4(v) {
    return Base_attribute("height", int32ToString(v));
}

export function attr_height_Z721C83C5(v) {
    return Base_attribute("height", v);
}

export function attr_colspan_Z524259A4(v) {
    return Base_attribute("colspan", int32ToString(v));
}

export function attr_colSpan_Z524259A4(v) {
    return Base_attribute("colspan", int32ToString(v));
}

export function attr_rowSpan_Z524259A4(v) {
    return Base_attribute("rowspan", int32ToString(v));
}

export function attr_span_Z524259A4(v) {
    return Base_attribute("span", int32ToString(v));
}

export function attr_headers_Z721C83C5(v) {
    return Base_attribute("headers", v);
}

export function attr_scope_Z721C83C5(v) {
    return Base_attribute("scope", v);
}

export function attr_start_Z524259A4(v) {
    return Base_attribute("start", int32ToString(v));
}

export function attr_cite_Z721C83C5(v) {
    return Base_attribute("cite", v);
}

export function attr_dateTime_Z721C83C5(v) {
    return Base_attribute("datetime", v);
}

export function attr_content_Z721C83C5(v) {
    return Base_attribute("content", v);
}

export function attr_charSet_Z721C83C5(v) {
    return Base_attribute("charset", v);
}

export function attr_httpEquiv_Z721C83C5(v) {
    return Base_attribute("http-equiv", v);
}

export function attr_data_Z384F8060(name, v) {
    return Base_attribute("data-" + name, v);
}

export function attr_aria_Z384F8060(name, v) {
    return Base_attribute("aria-" + name, v);
}

export function attr_innerHTML_Z721C83C5(html) {
    return Base_property("innerHTML", html);
}

export function attr_innerHTML_5106B011(f) {
    return Base_bindProperty("innerHTML", f);
}

export function attr_innerHTML_Z108492D4(s) {
    return Base_bindPropertySignal("innerHTML", s);
}

/**
 * Arbitrary attribute (static or reactive), for anything without a named
 * member above - `aria-*` state, `data-*`, or a bleeding-edge attribute.
 */
export function attr_custom_Z384F8060(name, v) {
    return Base_attribute(name, v);
}

export function attr_custom_Z5AD79149(name, c) {
    return Base_bindAttributeSignal(name, c);
}

export function attr_custom_1B55B38A(name, f) {
    return Base_bindAttribute(name, f);
}

/**
 * Arbitrary live DOM property - sets `element[name]`, not an attribute.
 */
export function attr_prop_433E080(name, v) {
    return Base_property(name, v);
}

export function attr_prop_7CDAB96A(name, f) {
    return Base_bindProperty(name, f);
}

/**
 * Escape hatch: run arbitrary code against the element - capture a reference,
 * focus it, attach an observer, mount a third-party widget. Runs while the
 * element is being built, inside the enclosing `Signal.root`, so any effect or
 * `Signal.onCleanup` registered here tears down with the element.
 */
export function attr_ref_1F9A456B(f) {
    return (e) => {
        f(e);
    };
}

