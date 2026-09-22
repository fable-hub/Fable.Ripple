
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { EmptyMarker, Base_toElement, Base_applyItems, Base_emptyMarker, Base_createElement } from "./Base.js";
import { Signal_untracked, Signal_computed, Signal_onCleanup, Signal_root, Signal_effect } from "../Fable.Ripple/Api.js";
import { singleton } from "../../fable_modules/fable-library-js.5.13.0/List.js";
import { keyedEach } from "./Dom.js";
import { Exception, disposeSafe } from "../../fable_modules/fable-library-js.5.13.0/Util.js";
import { Var$1__get_Value } from "../Fable.Ripple/Types.js";

/**
 * HTML elements, text, keyed lists, conditionals, fragments and mounting. Each
 * element takes one `DomItem list` where attributes (`attr.*`), events (`ev.*`) and
 * child elements live together.
 */
export class Html {
    constructor() {
    }
}

export function Html_$reflection() {
    return class_type("Fable.Ripple.Dom.Html", undefined, Html);
}

/**
 * Generic element by tag name.
 */
export function Html_elem(tag, items) {
    return Base_createElement(tag, items);
}

export function Html_text_Z721C83C5(s) {
    return document.createTextNode(s);
}

export function Html_text_5106B011(f) {
    const t = document.createTextNode("");
    Signal_effect(() => {
        t.nodeValue = f();
    });
    return t;
}

export function Html_header_Z714D7FBE(items) {
    return Base_createElement("header", items);
}

export function Html_header_Z721C83C5(s) {
    return Base_createElement("header", singleton(Html_text_Z721C83C5(s)));
}

export function Html_footer_Z714D7FBE(items) {
    return Base_createElement("footer", items);
}

export function Html_footer_Z721C83C5(s) {
    return Base_createElement("footer", singleton(Html_text_Z721C83C5(s)));
}

export function Html_main_Z714D7FBE(items) {
    return Base_createElement("main", items);
}

export function Html_section_Z714D7FBE(items) {
    return Base_createElement("section", items);
}

export function Html_article_Z714D7FBE(items) {
    return Base_createElement("article", items);
}

export function Html_aside_Z714D7FBE(items) {
    return Base_createElement("aside", items);
}

export function Html_nav_Z714D7FBE(items) {
    return Base_createElement("nav", items);
}

export function Html_h1_Z714D7FBE(items) {
    return Base_createElement("h1", items);
}

export function Html_h1_Z721C83C5(s) {
    return Base_createElement("h1", singleton(Html_text_Z721C83C5(s)));
}

export function Html_h2_Z714D7FBE(items) {
    return Base_createElement("h2", items);
}

export function Html_h2_Z721C83C5(s) {
    return Base_createElement("h2", singleton(Html_text_Z721C83C5(s)));
}

export function Html_h3_Z714D7FBE(items) {
    return Base_createElement("h3", items);
}

export function Html_h3_Z721C83C5(s) {
    return Base_createElement("h3", singleton(Html_text_Z721C83C5(s)));
}

export function Html_h4_Z714D7FBE(items) {
    return Base_createElement("h4", items);
}

export function Html_h4_Z721C83C5(s) {
    return Base_createElement("h4", singleton(Html_text_Z721C83C5(s)));
}

export function Html_h5_Z714D7FBE(items) {
    return Base_createElement("h5", items);
}

export function Html_h5_Z721C83C5(s) {
    return Base_createElement("h5", singleton(Html_text_Z721C83C5(s)));
}

export function Html_h6_Z714D7FBE(items) {
    return Base_createElement("h6", items);
}

export function Html_h6_Z721C83C5(s) {
    return Base_createElement("h6", singleton(Html_text_Z721C83C5(s)));
}

export function Html_hgroup_Z714D7FBE(items) {
    return Base_createElement("hgroup", items);
}

export function Html_address_Z714D7FBE(items) {
    return Base_createElement("address", items);
}

export function Html_search_Z714D7FBE(items) {
    return Base_createElement("search", items);
}

export function Html_div_Z714D7FBE(items) {
    return Base_createElement("div", items);
}

export function Html_p_Z714D7FBE(items) {
    return Base_createElement("p", items);
}

export function Html_p_Z721C83C5(s) {
    return Base_createElement("p", singleton(Html_text_Z721C83C5(s)));
}

export function Html_hr_Z714D7FBE(items) {
    return Base_createElement("hr", items);
}

export function Html_pre_Z714D7FBE(items) {
    return Base_createElement("pre", items);
}

export function Html_blockquote_Z714D7FBE(items) {
    return Base_createElement("blockquote", items);
}

export function Html_blockquote_Z721C83C5(s) {
    return Base_createElement("blockquote", singleton(Html_text_Z721C83C5(s)));
}

export function Html_ol_Z714D7FBE(items) {
    return Base_createElement("ol", items);
}

export function Html_ul_Z714D7FBE(items) {
    return Base_createElement("ul", items);
}

export function Html_li_Z714D7FBE(items) {
    return Base_createElement("li", items);
}

export function Html_li_Z721C83C5(s) {
    return Base_createElement("li", singleton(Html_text_Z721C83C5(s)));
}

export function Html_menu_Z714D7FBE(items) {
    return Base_createElement("menu", items);
}

export function Html_dl_Z714D7FBE(items) {
    return Base_createElement("dl", items);
}

export function Html_dt_Z714D7FBE(items) {
    return Base_createElement("dt", items);
}

export function Html_dt_Z721C83C5(s) {
    return Base_createElement("dt", singleton(Html_text_Z721C83C5(s)));
}

export function Html_dd_Z714D7FBE(items) {
    return Base_createElement("dd", items);
}

export function Html_dd_Z721C83C5(s) {
    return Base_createElement("dd", singleton(Html_text_Z721C83C5(s)));
}

export function Html_figure_Z714D7FBE(items) {
    return Base_createElement("figure", items);
}

export function Html_figcaption_Z714D7FBE(items) {
    return Base_createElement("figcaption", items);
}

export function Html_figcaption_Z721C83C5(s) {
    return Base_createElement("figcaption", singleton(Html_text_Z721C83C5(s)));
}

export function Html_a_Z714D7FBE(items) {
    return Base_createElement("a", items);
}

export function Html_em_Z714D7FBE(items) {
    return Base_createElement("em", items);
}

export function Html_em_Z721C83C5(s) {
    return Base_createElement("em", singleton(Html_text_Z721C83C5(s)));
}

export function Html_strong_Z714D7FBE(items) {
    return Base_createElement("strong", items);
}

export function Html_strong_Z721C83C5(s) {
    return Base_createElement("strong", singleton(Html_text_Z721C83C5(s)));
}

export function Html_small_Z714D7FBE(items) {
    return Base_createElement("small", items);
}

export function Html_small_Z721C83C5(s) {
    return Base_createElement("small", singleton(Html_text_Z721C83C5(s)));
}

export function Html_s_Z714D7FBE(items) {
    return Base_createElement("s", items);
}

export function Html_cite_Z714D7FBE(items) {
    return Base_createElement("cite", items);
}

export function Html_cite_Z721C83C5(s) {
    return Base_createElement("cite", singleton(Html_text_Z721C83C5(s)));
}

export function Html_q_Z714D7FBE(items) {
    return Base_createElement("q", items);
}

export function Html_q_Z721C83C5(s) {
    return Base_createElement("q", singleton(Html_text_Z721C83C5(s)));
}

export function Html_dfn_Z714D7FBE(items) {
    return Base_createElement("dfn", items);
}

export function Html_abbr_Z714D7FBE(items) {
    return Base_createElement("abbr", items);
}

export function Html_abbr_Z721C83C5(s) {
    return Base_createElement("abbr", singleton(Html_text_Z721C83C5(s)));
}

export function Html_ruby_Z714D7FBE(items) {
    return Base_createElement("ruby", items);
}

export function Html_rt_Z714D7FBE(items) {
    return Base_createElement("rt", items);
}

export function Html_rp_Z714D7FBE(items) {
    return Base_createElement("rp", items);
}

export function Html_data_Z714D7FBE(items) {
    return Base_createElement("data", items);
}

export function Html_time_Z714D7FBE(items) {
    return Base_createElement("time", items);
}

export function Html_code_Z714D7FBE(items) {
    return Base_createElement("code", items);
}

export function Html_code_Z721C83C5(s) {
    return Base_createElement("code", singleton(Html_text_Z721C83C5(s)));
}

export function Html_var_Z714D7FBE(items) {
    return Base_createElement("var", items);
}

export function Html_var_Z721C83C5(s) {
    return Base_createElement("var", singleton(Html_text_Z721C83C5(s)));
}

export function Html_samp_Z714D7FBE(items) {
    return Base_createElement("samp", items);
}

export function Html_samp_Z721C83C5(s) {
    return Base_createElement("samp", singleton(Html_text_Z721C83C5(s)));
}

export function Html_kbd_Z714D7FBE(items) {
    return Base_createElement("kbd", items);
}

export function Html_kbd_Z721C83C5(s) {
    return Base_createElement("kbd", singleton(Html_text_Z721C83C5(s)));
}

export function Html_sub_Z714D7FBE(items) {
    return Base_createElement("sub", items);
}

export function Html_sup_Z714D7FBE(items) {
    return Base_createElement("sup", items);
}

export function Html_i_Z714D7FBE(items) {
    return Base_createElement("i", items);
}

export function Html_b_Z714D7FBE(items) {
    return Base_createElement("b", items);
}

export function Html_u_Z714D7FBE(items) {
    return Base_createElement("u", items);
}

export function Html_mark_Z714D7FBE(items) {
    return Base_createElement("mark", items);
}

export function Html_mark_Z721C83C5(s) {
    return Base_createElement("mark", singleton(Html_text_Z721C83C5(s)));
}

export function Html_bdi_Z714D7FBE(items) {
    return Base_createElement("bdi", items);
}

export function Html_bdo_Z714D7FBE(items) {
    return Base_createElement("bdo", items);
}

export function Html_span_Z714D7FBE(items) {
    return Base_createElement("span", items);
}

export function Html_span_Z721C83C5(s) {
    return Base_createElement("span", singleton(Html_text_Z721C83C5(s)));
}

export function Html_br_Z714D7FBE(items) {
    return Base_createElement("br", items);
}

export function Html_wbr_Z714D7FBE(items) {
    return Base_createElement("wbr", items);
}

export function Html_ins_Z714D7FBE(items) {
    return Base_createElement("ins", items);
}

export function Html_del_Z714D7FBE(items) {
    return Base_createElement("del", items);
}

export function Html_picture_Z714D7FBE(items) {
    return Base_createElement("picture", items);
}

export function Html_source_Z714D7FBE(items) {
    return Base_createElement("source", items);
}

export function Html_img_Z714D7FBE(items) {
    return Base_createElement("img", items);
}

export function Html_iframe_Z714D7FBE(items) {
    return Base_createElement("iframe", items);
}

export function Html_embed_Z714D7FBE(items) {
    return Base_createElement("embed", items);
}

export function Html_object_Z714D7FBE(items) {
    return Base_createElement("object", items);
}

export function Html_param_Z714D7FBE(items) {
    return Base_createElement("param", items);
}

export function Html_video_Z714D7FBE(items) {
    return Base_createElement("video", items);
}

export function Html_audio_Z714D7FBE(items) {
    return Base_createElement("audio", items);
}

export function Html_track_Z714D7FBE(items) {
    return Base_createElement("track", items);
}

export function Html_map_Z714D7FBE(items) {
    return Base_createElement("map", items);
}

export function Html_area_Z714D7FBE(items) {
    return Base_createElement("area", items);
}

export function Html_canvas_Z714D7FBE(items) {
    return Base_createElement("canvas", items);
}

export function Html_table_Z714D7FBE(items) {
    return Base_createElement("table", items);
}

export function Html_caption_Z714D7FBE(items) {
    return Base_createElement("caption", items);
}

export function Html_caption_Z721C83C5(s) {
    return Base_createElement("caption", singleton(Html_text_Z721C83C5(s)));
}

export function Html_colgroup_Z714D7FBE(items) {
    return Base_createElement("colgroup", items);
}

export function Html_col_Z714D7FBE(items) {
    return Base_createElement("col", items);
}

export function Html_thead_Z714D7FBE(items) {
    return Base_createElement("thead", items);
}

export function Html_tbody_Z714D7FBE(items) {
    return Base_createElement("tbody", items);
}

export function Html_tfoot_Z714D7FBE(items) {
    return Base_createElement("tfoot", items);
}

export function Html_tr_Z714D7FBE(items) {
    return Base_createElement("tr", items);
}

export function Html_td_Z714D7FBE(items) {
    return Base_createElement("td", items);
}

export function Html_td_Z721C83C5(s) {
    return Base_createElement("td", singleton(Html_text_Z721C83C5(s)));
}

export function Html_th_Z714D7FBE(items) {
    return Base_createElement("th", items);
}

export function Html_th_Z721C83C5(s) {
    return Base_createElement("th", singleton(Html_text_Z721C83C5(s)));
}

export function Html_form_Z714D7FBE(items) {
    return Base_createElement("form", items);
}

export function Html_label_Z714D7FBE(items) {
    return Base_createElement("label", items);
}

export function Html_input_Z714D7FBE(items) {
    return Base_createElement("input", items);
}

export function Html_button_Z714D7FBE(items) {
    return Base_createElement("button", items);
}

export function Html_select_Z714D7FBE(items) {
    return Base_createElement("select", items);
}

export function Html_datalist_Z714D7FBE(items) {
    return Base_createElement("datalist", items);
}

export function Html_optgroup_Z714D7FBE(items) {
    return Base_createElement("optgroup", items);
}

export function Html_option_Z714D7FBE(items) {
    return Base_createElement("option", items);
}

export function Html_option_Z721C83C5(s) {
    return Base_createElement("option", singleton(Html_text_Z721C83C5(s)));
}

export function Html_textarea_Z714D7FBE(items) {
    return Base_createElement("textarea", items);
}

export function Html_output_Z714D7FBE(items) {
    return Base_createElement("output", items);
}

export function Html_output_Z721C83C5(s) {
    return Base_createElement("output", singleton(Html_text_Z721C83C5(s)));
}

export function Html_progress_Z714D7FBE(items) {
    return Base_createElement("progress", items);
}

export function Html_meter_Z714D7FBE(items) {
    return Base_createElement("meter", items);
}

export function Html_fieldset_Z714D7FBE(items) {
    return Base_createElement("fieldset", items);
}

export function Html_legend_Z714D7FBE(items) {
    return Base_createElement("legend", items);
}

export function Html_legend_Z721C83C5(s) {
    return Base_createElement("legend", singleton(Html_text_Z721C83C5(s)));
}

export function Html_details_Z714D7FBE(items) {
    return Base_createElement("details", items);
}

export function Html_summary_Z714D7FBE(items) {
    return Base_createElement("summary", items);
}

export function Html_summary_Z721C83C5(s) {
    return Base_createElement("summary", singleton(Html_text_Z721C83C5(s)));
}

export function Html_dialog_Z714D7FBE(items) {
    return Base_createElement("dialog", items);
}

export function Html_slot_Z714D7FBE(items) {
    return Base_createElement("slot", items);
}

/**
 * Empty item, renders nothing.
 */
export function Html_get_none() {
    return Base_emptyMarker;
}

/**
 * Splice a `Node` built elsewhere - SVG, a server-rendered fragment, or the
 * output of a non-Fable library - as a child. Standard DOM rules apply: a node
 * already mounted is *moved*, not copied.
 */
export function Html_node_171AE942(n) {
    return n;
}

/**
 * Group children with no wrapper element - splices `items` into the parent.
 */
export function Html_fragment_Z714D7FBE(items) {
    return (e) => {
        Base_applyItems(e, items);
    };
}

/**
 * Reactive keyed list: one element per item, reconciled by key.
 */
export function Html_each(getItems, keyOf, render) {
    return (parent) => {
        const anchor = document.createComment("each");
        parent.appendChild(anchor);
        keyedEach(parent, anchor, getItems, keyOf, (x) => Base_toElement(render(x)));
    };
}

/**
 * Reactive subtree: rebuilds `f ()` in place whenever the signals it reads
 * change; each rebuild runs in its own `Signal.root`, disposed on rebuild/
 * teardown. Use it with a native `match`/`if` to switch views on a signal.
 */
export function Html_dynamic_70E9CA6A(f) {
    return (parent) => {
        const anchor = document.createComment("dynamic");
        parent.appendChild(anchor);
        let current = undefined;
        const clear = () => {
            const option_3 = current;
            if (option_3 != null) {
                const tupledArg = option_3;
                disposeSafe(tupledArg[1]);
                const option_1 = tupledArg[0];
                if (option_1 != null) {
                    const n = option_1;
                    parent.removeChild(n);
                }
            }
            current = undefined;
        };
        Signal_effect(() => {
            clear();
            const patternInput = Signal_root(() => {
                const matchValue = f();
                if (matchValue instanceof Node) {
                    return matchValue;
                }
                else if (matchValue instanceof EmptyMarker) {
                    return undefined;
                }
                else {
                    throw new Exception("Html.dynamic expects an element or Html.none");
                }
            });
            const node_2 = patternInput[0];
            const option_5 = node_2;
            if (option_5 != null) {
                const n_1 = option_5;
                parent.insertBefore(n_1, anchor);
            }
            current = [node_2, patternInput[1]];
        });
        Signal_onCleanup(clear);
    };
}

/**
 * `switch` over a value computed by `read`, for a shape that depends on more
 * than one signal without naming a derived signal for it.
 */
export function Html_switchWith_33B7F5DC(read, f) {
    const value = Signal_computed(read);
    return Html_dynamic_70E9CA6A(() => {
        const current = Var$1__get_Value(value);
        return Signal_untracked(() => f(current));
    });
}

/**
 * Reactive conditional: renders `whenTrue ()` while `cond` holds, otherwise
 * nothing. The active branch is built fresh when it appears and disposed when
 * it leaves; `cond` is auto-tracked, so it re-evaluates on signal changes.
 */
export function Html_show_Z593B4D6(cond, whenTrue) {
    return Html_dynamic_70E9CA6A(() => (cond() ? whenTrue() : Html_get_none()));
}

/**
 * Reactive conditional with a fallback: `whenTrue ()` while `cond` holds,
 * else `whenFalse ()`.
 */
export function Html_show_371C7A00(cond, whenTrue, whenFalse) {
    return Html_dynamic_70E9CA6A(() => (cond() ? whenTrue() : whenFalse()));
}

/**
 * Realise a root item to its element.
 */
export function Html_render_62D6BEC0(item) {
    return Base_toElement(item);
}

/**
 * Mount `view` into the element with the given id, inside its own
 * `Signal.root`. Disposing the result tears that scope down and removes the
 * mounted element; disposing twice is a no-op.
 * 
 * `view` is a function, not a `DomItem`: a `DomItem` argument would be built -
 * effects and all - before `mount` opened the scope that is meant to own it.
 */
export function Html_mount(id, view) {
    const container = document.getElementById(id);
    const patternInput = Signal_root(() => Base_toElement(view()));
    let node = patternInput[0];
    container.appendChild(node);
    let live = true;
    return {
        Dispose() {
            if (live) {
                live = false;
                disposeSafe(patternInput[1]);
                container.removeChild(node);
            }
        },
    };
}

