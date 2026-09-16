
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Base_bindAttribute, Base_bindAttributeSignal, Base_attribute, Base_svgNamespace, Base_createElementNS } from "./Base.js";
import { int32ToString } from "../../fable_modules/fable-library-js.5.13.0/Util.js";

/**
 * SVG elements. Created in the SVG namespace (`createElementNS`) so they render as
 * real SVG. Mix in `attr.*` (setAttribute-based, e.g. `attr.id`, `attr.className`)
 * and `svgAttr.*` (presentation attributes) freely - both produce `DomItem`.
 */
export class Svg {
    constructor() {
    }
}

export function Svg_$reflection() {
    return class_type("Fable.Ripple.Dom.Svg", undefined, Svg);
}

/**
 * Generic SVG element by tag name.
 */
export function Svg_elem(tag, items) {
    return Base_createElementNS(Base_svgNamespace, tag, items);
}

export function Svg_svg_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "svg", items);
}

export function Svg_g_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "g", items);
}

export function Svg_defs_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "defs", items);
}

export function Svg_symbol_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "symbol", items);
}

export function Svg_use$0027_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "use", items);
}

export function Svg_image_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "image", items);
}

export function Svg_switch_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "switch", items);
}

export function Svg_foreignObject_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "foreignObject", items);
}

export function Svg_marker_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "marker", items);
}

export function Svg_view_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "view", items);
}

export function Svg_a_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "a", items);
}

export function Svg_title_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "title", items);
}

export function Svg_desc_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "desc", items);
}

export function Svg_metadata_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "metadata", items);
}

export function Svg_circle_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "circle", items);
}

export function Svg_ellipse_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "ellipse", items);
}

export function Svg_line_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "line", items);
}

export function Svg_rect_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "rect", items);
}

export function Svg_path_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "path", items);
}

export function Svg_polygon_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "polygon", items);
}

export function Svg_polyline_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "polyline", items);
}

export function Svg_text_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "text", items);
}

export function Svg_tspan_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "tspan", items);
}

export function Svg_textPath_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "textPath", items);
}

export function Svg_tref_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "tref", items);
}

export function Svg_linearGradient_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "linearGradient", items);
}

export function Svg_radialGradient_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "radialGradient", items);
}

export function Svg_stop_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "stop", items);
}

export function Svg_pattern_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "pattern", items);
}

export function Svg_clipPath_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "clipPath", items);
}

export function Svg_mask_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "mask", items);
}

export function Svg_filter_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "filter", items);
}

export function Svg_feBlend_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feBlend", items);
}

export function Svg_feColorMatrix_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feColorMatrix", items);
}

export function Svg_feComponentTransfer_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feComponentTransfer", items);
}

export function Svg_feComposite_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feComposite", items);
}

export function Svg_feConvolveMatrix_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feConvolveMatrix", items);
}

export function Svg_feDiffuseLighting_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feDiffuseLighting", items);
}

export function Svg_feDisplacementMap_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feDisplacementMap", items);
}

export function Svg_feDistantLight_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feDistantLight", items);
}

export function Svg_feDropShadow_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feDropShadow", items);
}

export function Svg_feFlood_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feFlood", items);
}

export function Svg_feFuncA_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feFuncA", items);
}

export function Svg_feFuncB_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feFuncB", items);
}

export function Svg_feFuncG_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feFuncG", items);
}

export function Svg_feFuncR_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feFuncR", items);
}

export function Svg_feGaussianBlur_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feGaussianBlur", items);
}

export function Svg_feImage_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feImage", items);
}

export function Svg_feMerge_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feMerge", items);
}

export function Svg_feMergeNode_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feMergeNode", items);
}

export function Svg_feMorphology_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feMorphology", items);
}

export function Svg_feOffset_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feOffset", items);
}

export function Svg_fePointLight_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "fePointLight", items);
}

export function Svg_feSpecularLighting_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feSpecularLighting", items);
}

export function Svg_feSpotLight_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feSpotLight", items);
}

export function Svg_feTile_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feTile", items);
}

export function Svg_feTurbulence_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "feTurbulence", items);
}

export function Svg_animate_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "animate", items);
}

export function Svg_animateMotion_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "animateMotion", items);
}

export function Svg_animateTransform_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "animateTransform", items);
}

export function Svg_mpath_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "mpath", items);
}

export function Svg_set$0027_Z714D7FBE(items) {
    return Base_createElementNS(Base_svgNamespace, "set", items);
}

/**
 * SVG presentation attributes. Static by default; make any of them reactive with
 * the generic `svgAttr.attr` overloads (a `Signal<string>` or a `unit -> string`).
 */
export class svgAttr {
    constructor() {
    }
}

export function svgAttr_$reflection() {
    return class_type("Fable.Ripple.Dom.svgAttr", undefined, svgAttr);
}

export function svgAttr_x_5E38073B(v) {
    return Base_attribute("x", v.toString());
}

export function svgAttr_x_Z524259A4(v) {
    return Base_attribute("x", int32ToString(v));
}

export function svgAttr_x_Z721C83C5(v) {
    return Base_attribute("x", v);
}

export function svgAttr_y_5E38073B(v) {
    return Base_attribute("y", v.toString());
}

export function svgAttr_y_Z524259A4(v) {
    return Base_attribute("y", int32ToString(v));
}

export function svgAttr_y_Z721C83C5(v) {
    return Base_attribute("y", v);
}

export function svgAttr_x1_5E38073B(v) {
    return Base_attribute("x1", v.toString());
}

export function svgAttr_x1_Z524259A4(v) {
    return Base_attribute("x1", int32ToString(v));
}

export function svgAttr_y1_5E38073B(v) {
    return Base_attribute("y1", v.toString());
}

export function svgAttr_y1_Z524259A4(v) {
    return Base_attribute("y1", int32ToString(v));
}

export function svgAttr_x2_5E38073B(v) {
    return Base_attribute("x2", v.toString());
}

export function svgAttr_x2_Z524259A4(v) {
    return Base_attribute("x2", int32ToString(v));
}

export function svgAttr_y2_5E38073B(v) {
    return Base_attribute("y2", v.toString());
}

export function svgAttr_y2_Z524259A4(v) {
    return Base_attribute("y2", int32ToString(v));
}

export function svgAttr_cx_5E38073B(v) {
    return Base_attribute("cx", v.toString());
}

export function svgAttr_cx_Z524259A4(v) {
    return Base_attribute("cx", int32ToString(v));
}

export function svgAttr_cx_Z721C83C5(v) {
    return Base_attribute("cx", v);
}

export function svgAttr_cy_5E38073B(v) {
    return Base_attribute("cy", v.toString());
}

export function svgAttr_cy_Z524259A4(v) {
    return Base_attribute("cy", int32ToString(v));
}

export function svgAttr_cy_Z721C83C5(v) {
    return Base_attribute("cy", v);
}

export function svgAttr_r_5E38073B(v) {
    return Base_attribute("r", v.toString());
}

export function svgAttr_r_Z524259A4(v) {
    return Base_attribute("r", int32ToString(v));
}

export function svgAttr_r_Z721C83C5(v) {
    return Base_attribute("r", v);
}

export function svgAttr_rx_5E38073B(v) {
    return Base_attribute("rx", v.toString());
}

export function svgAttr_rx_Z524259A4(v) {
    return Base_attribute("rx", int32ToString(v));
}

export function svgAttr_ry_5E38073B(v) {
    return Base_attribute("ry", v.toString());
}

export function svgAttr_ry_Z524259A4(v) {
    return Base_attribute("ry", int32ToString(v));
}

export function svgAttr_dx_5E38073B(v) {
    return Base_attribute("dx", v.toString());
}

export function svgAttr_dx_Z721C83C5(v) {
    return Base_attribute("dx", v);
}

export function svgAttr_dy_5E38073B(v) {
    return Base_attribute("dy", v.toString());
}

export function svgAttr_dy_Z721C83C5(v) {
    return Base_attribute("dy", v);
}

export function svgAttr_width_5E38073B(v) {
    return Base_attribute("width", v.toString());
}

export function svgAttr_width_Z524259A4(v) {
    return Base_attribute("width", int32ToString(v));
}

export function svgAttr_width_Z721C83C5(v) {
    return Base_attribute("width", v);
}

export function svgAttr_height_5E38073B(v) {
    return Base_attribute("height", v.toString());
}

export function svgAttr_height_Z524259A4(v) {
    return Base_attribute("height", int32ToString(v));
}

export function svgAttr_height_Z721C83C5(v) {
    return Base_attribute("height", v);
}

export function svgAttr_fx_Z721C83C5(v) {
    return Base_attribute("fx", v);
}

export function svgAttr_fy_Z721C83C5(v) {
    return Base_attribute("fy", v);
}

export function svgAttr_offset_Z721C83C5(v) {
    return Base_attribute("offset", v);
}

export function svgAttr_pathLength_5E38073B(v) {
    return Base_attribute("pathLength", v.toString());
}

export function svgAttr_refX_Z721C83C5(v) {
    return Base_attribute("refX", v);
}

export function svgAttr_refY_Z721C83C5(v) {
    return Base_attribute("refY", v);
}

export function svgAttr_markerWidth_5E38073B(v) {
    return Base_attribute("markerWidth", v.toString());
}

export function svgAttr_markerHeight_5E38073B(v) {
    return Base_attribute("markerHeight", v.toString());
}

export function svgAttr_startOffset_Z721C83C5(v) {
    return Base_attribute("startOffset", v);
}

export function svgAttr_d_Z721C83C5(v) {
    return Base_attribute("d", v);
}

export function svgAttr_points_Z721C83C5(v) {
    return Base_attribute("points", v);
}

export function svgAttr_transform_Z721C83C5(v) {
    return Base_attribute("transform", v);
}

export function svgAttr_transformOrigin_Z721C83C5(v) {
    return Base_attribute("transform-origin", v);
}

export function svgAttr_viewBox_Z721C83C5(v) {
    return Base_attribute("viewBox", v);
}

export function svgAttr_preserveAspectRatio_Z721C83C5(v) {
    return Base_attribute("preserveAspectRatio", v);
}

export function svgAttr_fill_Z721C83C5(v) {
    return Base_attribute("fill", v);
}

export function svgAttr_fillOpacity_5E38073B(v) {
    return Base_attribute("fill-opacity", v.toString());
}

export function svgAttr_fillOpacity_Z721C83C5(v) {
    return Base_attribute("fill-opacity", v);
}

export function svgAttr_fillRule_Z721C83C5(v) {
    return Base_attribute("fill-rule", v);
}

export function svgAttr_stroke_Z721C83C5(v) {
    return Base_attribute("stroke", v);
}

export function svgAttr_strokeWidth_5E38073B(v) {
    return Base_attribute("stroke-width", v.toString());
}

export function svgAttr_strokeWidth_Z524259A4(v) {
    return Base_attribute("stroke-width", int32ToString(v));
}

export function svgAttr_strokeWidth_Z721C83C5(v) {
    return Base_attribute("stroke-width", v);
}

export function svgAttr_strokeLinecap_Z721C83C5(v) {
    return Base_attribute("stroke-linecap", v);
}

export function svgAttr_strokeLinejoin_Z721C83C5(v) {
    return Base_attribute("stroke-linejoin", v);
}

export function svgAttr_strokeDasharray_Z721C83C5(v) {
    return Base_attribute("stroke-dasharray", v);
}

export function svgAttr_strokeDashoffset_Z721C83C5(v) {
    return Base_attribute("stroke-dashoffset", v);
}

export function svgAttr_strokeOpacity_5E38073B(v) {
    return Base_attribute("stroke-opacity", v.toString());
}

export function svgAttr_strokeMiterlimit_Z721C83C5(v) {
    return Base_attribute("stroke-miterlimit", v);
}

export function svgAttr_opacity_5E38073B(v) {
    return Base_attribute("opacity", v.toString());
}

export function svgAttr_opacity_Z721C83C5(v) {
    return Base_attribute("opacity", v);
}

export function svgAttr_color_Z721C83C5(v) {
    return Base_attribute("color", v);
}

export function svgAttr_stopColor_Z721C83C5(v) {
    return Base_attribute("stop-color", v);
}

export function svgAttr_stopOpacity_5E38073B(v) {
    return Base_attribute("stop-opacity", v.toString());
}

export function svgAttr_floodColor_Z721C83C5(v) {
    return Base_attribute("flood-color", v);
}

export function svgAttr_floodOpacity_5E38073B(v) {
    return Base_attribute("flood-opacity", v.toString());
}

export function svgAttr_gradientUnits_Z721C83C5(v) {
    return Base_attribute("gradientUnits", v);
}

export function svgAttr_gradientTransform_Z721C83C5(v) {
    return Base_attribute("gradientTransform", v);
}

export function svgAttr_spreadMethod_Z721C83C5(v) {
    return Base_attribute("spreadMethod", v);
}

export function svgAttr_patternUnits_Z721C83C5(v) {
    return Base_attribute("patternUnits", v);
}

export function svgAttr_patternContentUnits_Z721C83C5(v) {
    return Base_attribute("patternContentUnits", v);
}

export function svgAttr_patternTransform_Z721C83C5(v) {
    return Base_attribute("patternTransform", v);
}

export function svgAttr_clipPathUnits_Z721C83C5(v) {
    return Base_attribute("clipPathUnits", v);
}

export function svgAttr_clipPath_Z721C83C5(v) {
    return Base_attribute("clip-path", v);
}

export function svgAttr_clipRule_Z721C83C5(v) {
    return Base_attribute("clip-rule", v);
}

export function svgAttr_maskUnits_Z721C83C5(v) {
    return Base_attribute("maskUnits", v);
}

export function svgAttr_maskContentUnits_Z721C83C5(v) {
    return Base_attribute("maskContentUnits", v);
}

export function svgAttr_mask_Z721C83C5(v) {
    return Base_attribute("mask", v);
}

export function svgAttr_filter_Z721C83C5(v) {
    return Base_attribute("filter", v);
}

export function svgAttr_markerStart_Z721C83C5(v) {
    return Base_attribute("marker-start", v);
}

export function svgAttr_markerMid_Z721C83C5(v) {
    return Base_attribute("marker-mid", v);
}

export function svgAttr_markerEnd_Z721C83C5(v) {
    return Base_attribute("marker-end", v);
}

export function svgAttr_textAnchor_Z721C83C5(v) {
    return Base_attribute("text-anchor", v);
}

export function svgAttr_dominantBaseline_Z721C83C5(v) {
    return Base_attribute("dominant-baseline", v);
}

export function svgAttr_fontFamily_Z721C83C5(v) {
    return Base_attribute("font-family", v);
}

export function svgAttr_fontSize_5E38073B(v) {
    return Base_attribute("font-size", v.toString());
}

export function svgAttr_fontSize_Z721C83C5(v) {
    return Base_attribute("font-size", v);
}

export function svgAttr_fontWeight_Z721C83C5(v) {
    return Base_attribute("font-weight", v);
}

export function svgAttr_letterSpacing_Z721C83C5(v) {
    return Base_attribute("letter-spacing", v);
}

export function svgAttr_visibility_Z721C83C5(v) {
    return Base_attribute("visibility", v);
}

export function svgAttr_display_Z721C83C5(v) {
    return Base_attribute("display", v);
}

export function svgAttr_pointerEvents_Z721C83C5(v) {
    return Base_attribute("pointer-events", v);
}

export function svgAttr_cursor_Z721C83C5(v) {
    return Base_attribute("cursor", v);
}

export function svgAttr_overflow_Z721C83C5(v) {
    return Base_attribute("overflow", v);
}

export function svgAttr_href_Z721C83C5(v) {
    return Base_attribute("href", v);
}

export function svgAttr_xlinkHref_Z721C83C5(v) {
    return Base_attribute("xlink:href", v);
}

export function svgAttr_in$0027_Z721C83C5(v) {
    return Base_attribute("in", v);
}

export function svgAttr_in2_Z721C83C5(v) {
    return Base_attribute("in2", v);
}

export function svgAttr_result_Z721C83C5(v) {
    return Base_attribute("result", v);
}

export function svgAttr_stdDeviation_Z721C83C5(v) {
    return Base_attribute("stdDeviation", v);
}

export function svgAttr_mode_Z721C83C5(v) {
    return Base_attribute("mode", v);
}

export function svgAttr_values_Z721C83C5(v) {
    return Base_attribute("values", v);
}

export function svgAttr_type$0027_Z721C83C5(v) {
    return Base_attribute("type", v);
}

export function svgAttr_operator_Z721C83C5(v) {
    return Base_attribute("operator", v);
}

export function svgAttr_baseFrequency_Z721C83C5(v) {
    return Base_attribute("baseFrequency", v);
}

export function svgAttr_numOctaves_Z524259A4(v) {
    return Base_attribute("numOctaves", int32ToString(v));
}

export function svgAttr_attributeName_Z721C83C5(v) {
    return Base_attribute("attributeName", v);
}

export function svgAttr_from$0027_Z721C83C5(v) {
    return Base_attribute("from", v);
}

export function svgAttr_to$0027_Z721C83C5(v) {
    return Base_attribute("to", v);
}

export function svgAttr_by_Z721C83C5(v) {
    return Base_attribute("by", v);
}

export function svgAttr_dur_Z721C83C5(v) {
    return Base_attribute("dur", v);
}

export function svgAttr_repeatCount_Z721C83C5(v) {
    return Base_attribute("repeatCount", v);
}

export function svgAttr_begin$0027_Z721C83C5(v) {
    return Base_attribute("begin", v);
}

export function svgAttr_end$0027_Z721C83C5(v) {
    return Base_attribute("end", v);
}

export function svgAttr_calcMode_Z721C83C5(v) {
    return Base_attribute("calcMode", v);
}

export function svgAttr_keyTimes_Z721C83C5(v) {
    return Base_attribute("keyTimes", v);
}

export function svgAttr_keySplines_Z721C83C5(v) {
    return Base_attribute("keySplines", v);
}

export function svgAttr_custom_Z384F8060(name, v) {
    return Base_attribute(name, v);
}

export function svgAttr_custom_Z5AD79149(name, c) {
    return Base_bindAttributeSignal(name, c);
}

export function svgAttr_custom_1B55B38A(name, f) {
    return Base_bindAttribute(name, f);
}

