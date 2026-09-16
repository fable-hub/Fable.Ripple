
import { Record, Union } from "../../fable_modules/fable-library-js.5.13.0/Types.js";
import { float64_type, record_type, bool_type, option_type, lambda_type, unit_type, string_type, int32_type, union_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Repaint__Track, Probe__get_Hits } from "./Widgets.js";
import { printf, toText, substring } from "../../fable_modules/fable-library-js.5.13.0/String.js";
import { arrayHash, compareArrays, equalArrays, int32ToString, equals, numberHash, comparePrimitives, disposeSafe, getEnumerator } from "../../fable_modules/fable-library-js.5.13.0/Util.js";
import { tryPick, tryFindIndex, toArray as toArray_1, filter, average, sortBy, empty, singleton as singleton_1, ofArray, indexed, map, max, min, isEmpty, choose, length } from "../../fable_modules/fable-library-js.5.13.0/List.js";
import { addToSet, getItemFromDict } from "../../fable_modules/fable-library-js.5.13.0/MapUtil.js";
import { List_distinct, List_countBy, List_groupBy } from "../../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { empty as empty_1, exists, append, singleton, collect, delay as delay_1, toList } from "../../fable_modules/fable-library-js.5.13.0/Seq.js";
import { Signal_effect, Signal_untracked } from "../../src/Fable.Ripple/Api.js";
import { Svg_defs_Z714D7FBE, svgAttr_viewBox_Z721C83C5, Svg_svg_Z714D7FBE, Svg_circle_Z714D7FBE, Svg_path_Z714D7FBE, Svg_elem, Svg_text_Z714D7FBE, svgAttr_custom_Z384F8060, Svg_rect_Z714D7FBE, Svg_g_Z714D7FBE } from "../../src/Fable.Ripple.Dom/Svg.js";
import { attr_style_Z721C83C5, attr_ref_1F9A456B, attr_className_Z721C83C5 } from "../../src/Fable.Ripple.Dom/Attributes.js";
import { Html_span_Z714D7FBE, Html_each, Html_div_Z714D7FBE, Html_figure_Z714D7FBE, Html_get_none, Html_text_5106B011, Html_fragment_Z714D7FBE, Html_text_Z721C83C5 } from "../../src/Fable.Ripple.Dom/Html.js";
import { ofList, tryFind } from "../../fable_modules/fable-library-js.5.13.0/Map.js";
import { min as min_1, max as max_1 } from "../../fable_modules/fable-library-js.5.13.0/Double.js";
import { defaultArg, toArray } from "../../fable_modules/fable-library-js.5.13.0/Option.js";

export class NodeKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Source", "Derived", "Reader", "Ghost"];
    }
    static Source = new NodeKind(0, []);
    static Derived = new NodeKind(1, []);
    static Reader = new NodeKind(2, []);
    static Ghost = new NodeKind(3, []);
}

export function NodeKind_$reflection() {
    return union_type("Demo.Examples.Schematic.NodeKind", [], NodeKind, () => [[], [], [], []]);
}

/**
 * A node. Construct one with `source` / `derived` / `binding` / `ghost` and
 * bind it with `let`; the binding is what identifies it from then on.
 */
export class Node$ extends Record {
    constructor(Key, Label, Kind, Value, Runs, Badge) {
        super();
        this.Key = (Key | 0);
        this.Label = Label;
        this.Kind = Kind;
        this.Value = Value;
        this.Runs = Runs;
        this.Badge = Badge;
    }
}

export function Node$_$reflection() {
    return record_type("Demo.Examples.Schematic.Node", [], Node$, () => [["Key", int32_type], ["Label", string_type], ["Kind", NodeKind_$reflection()], ["Value", lambda_type(unit_type, string_type)], ["Runs", option_type(lambda_type(unit_type, int32_type))], ["Badge", bool_type]]);
}

let NodeModule_nextKey = 0;

function NodeModule_make(kind, label, value) {
    NodeModule_nextKey = ((NodeModule_nextKey + 1) | 0);
    return new Node$(NodeModule_nextKey, label, kind, value, undefined, true);
}

/**
 * A writable source, from a thunk - for a value with its own formatting.
 */
export function NodeModule_sourceWith(label, value) {
    return NodeModule_make(NodeKind.Source, label, value);
}

/**
 * A cached derived node, from a thunk - for a value with its own formatting.
 * 
 * Also the public seam `derived` needs: an `inline` member cannot reach the
 * private `make`, so the SRTP form forwards through here. Same reason
 * `sourceWith` exists.
 */
export function NodeModule_derivedWith(label, value) {
    return NodeModule_make(NodeKind.Derived, label, value);
}

/**
 * A DOM binding whose text is not simply a signal's contents - a reader
 * showing "running" or "disposed" rather than a value.
 */
export function NodeModule_bindingWith(label, value) {
    return NodeModule_make(NodeKind.Reader, label, value);
}

/**
 * A plain function. Drawn borderless, because there is no node behind it -
 * so there is no signal to hand over, and this keeps its thunk.
 */
export function NodeModule_ghost(label, value) {
    return NodeModule_make(NodeKind.Ghost, label, value);
}

/**
 * Give a node a run counter, which draws its badge and drives its flash.
 */
export function NodeModule_counting(probe, n) {
    return new Node$(n.Key, n.Label, n.Kind, n.Value, () => (Probe__get_Hits(probe) | 0), true);
}

/**
 * Take part in the animation, but draw no number.
 */
export function NodeModule_beating(runs, n) {
    return new Node$(n.Key, n.Label, n.Kind, n.Value, runs, false);
}

/**
 * An edge: `a ==> b` means "b depends on a".
 */
export function op_EqualsEqualsGreater(a, b) {
    return [a, b];
}

const colW = 244;

const rowH = 82;

const boxW = 184;

const boxH = 54;

const padX = 12;

const padY = 14;

const valueChars = ~~((boxW - (2 * 10)) / 9);

function fit(s) {
    if (s.length > valueChars) {
        return substring(s, 0, valueChars - 1) + "…";
    }
    else {
        return s;
    }
}

class Placed extends Record {
    constructor(Node$, Col, Row, Bare) {
        super();
        this.Node = Node$;
        this.Col = (Col | 0);
        this.Row = Row;
        this.Bare = Bare;
    }
}

function Placed_$reflection() {
    return record_type("Demo.Examples.Schematic.Placed", [], Placed, () => [["Node", Node$_$reflection()], ["Col", int32_type], ["Row", float64_type], ["Bare", bool_type]]);
}

function widthOf(p) {
    if (p.Bare) {
        return 84;
    }
    else {
        return boxW;
    }
}

function x(p) {
    return (padX + (p.Col * colW)) + ((boxW - widthOf(p)) / 2);
}

function y(p) {
    return padY + (p.Row * rowH);
}

function midY(p) {
    return y(p) + (boxH / 2);
}

function rightX(p) {
    return x(p) + widthOf(p);
}

function leftX(p) {
    return x(p);
}

const gap = 7;

function place(nodes, edges) {
    const cols = new Map([]);
    const enumerator = getEnumerator(nodes);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const n = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            cols.set(n.Key, 0);
        }
    }
    finally {
        disposeSafe(enumerator);
    }
    let changed = true;
    let guard = 0;
    while (changed && (guard < (length(nodes) + 2))) {
        changed = false;
        guard = ((guard + 1) | 0);
        const enumerator_1 = getEnumerator(edges);
        try {
            while (enumerator_1["System.Collections.IEnumerator.MoveNext"]()) {
                const forLoopVar = enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"]();
                const b = forLoopVar[1];
                const a = forLoopVar[0];
                if (cols.has(a.Key) && cols.has(b.Key)) {
                    const candidate = (getItemFromDict(cols, a.Key) + 1) | 0;
                    if (candidate > getItemFromDict(cols, b.Key)) {
                        cols.set(b.Key, candidate);
                        changed = true;
                    }
                }
            }
        }
        finally {
            disposeSafe(enumerator_1);
        }
    }
    changed = true;
    guard = 0;
    while (changed && (guard < (length(nodes) + 2))) {
        changed = false;
        guard = ((guard + 1) | 0);
        const enumerator_2 = getEnumerator(nodes);
        try {
            while (enumerator_2["System.Collections.IEnumerator.MoveNext"]()) {
                const n_1 = enumerator_2["System.Collections.Generic.IEnumerator`1.get_Current"]();
                const consumers = choose((tupledArg) => {
                    const b_1 = tupledArg[1];
                    if ((tupledArg[0].Key === n_1.Key) && cols.has(b_1.Key)) {
                        return getItemFromDict(cols, b_1.Key);
                    }
                    else {
                        return undefined;
                    }
                }, edges);
                if (isEmpty(consumers)) {
                }
                else {
                    const latest = (min(consumers, {
                        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
                    }) - 1) | 0;
                    if (latest > getItemFromDict(cols, n_1.Key)) {
                        cols.set(n_1.Key, latest);
                        changed = true;
                    }
                }
            }
        }
        finally {
            disposeSafe(enumerator_2);
        }
    }
    const byCol = List_groupBy((n_2) => (getItemFromDict(cols, n_2.Key) | 0), nodes, {
        Equals: (x_2, y_2) => (x_2 === y_2),
        GetHashCode: (x_2) => (numberHash(x_2) | 0),
    });
    const tallest = max(map((tupledArg_1) => (length(tupledArg_1[1]) | 0), byCol), {
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    }) | 0;
    return toList(delay_1(() => collect((matchValue) => {
        const ns_1 = matchValue[1];
        const offset = (tallest - length(ns_1)) / 2;
        return collect((matchValue_1) => {
            const n_3 = matchValue_1[1];
            return singleton(new Placed(n_3, matchValue[0], offset + matchValue_1[0], Signal_untracked(n_3.Value) === ""));
        }, indexed(ns_1));
    }, byCol)));
}

function onGrow(repaint, count, onFire) {
    let previous = -1;
    Signal_effect(() => {
        Repaint__Track(repaint);
        const now = count() | 0;
        if ((previous >= 0) && (now > previous)) {
            onFire();
        }
        previous = (now | 0);
    });
}

const stagger = 600;

const dotDuration = 500;

function flash(delay, el) {
    el.animate([{
        opacity: 1,
        transform: "scale(1)",
    }, {
        opacity: 1,
        transform: "scale(1.06)",
    }, {
        opacity: 1,
        transform: "scale(1)",
    }], {
        delay: delay,
        duration: 420,
        easing: "ease-out",
        fill: "backwards",
    });
}

function travel(delay, el) {
    el.animate([{
        offset: 0,
        offsetDistance: "0%",
        opacity: 0,
    }, {
        offset: 0.18,
        offsetDistance: "18%",
        opacity: 1,
    }, {
        offset: 0.82,
        offsetDistance: "82%",
        opacity: 1,
    }, {
        offset: 1,
        offsetDistance: "100%",
        opacity: 0,
    }], {
        delay: delay,
        duration: dotDuration,
        easing: "linear",
        fill: "backwards",
    });
}

function nodeClass(_arg) {
    switch (_arg.tag) {
        case 1:
            return "sch-node sch-derived";
        case 2:
            return "sch-node sch-reader";
        case 3:
            return "sch-node sch-ghost";
        default:
            return "sch-node sch-source";
    }
}

function drawNode(repaint, flashOn, p) {
    const n = p.Node;
    return Svg_g_Z714D7FBE(toList(delay_1(() => append(singleton(attr_className_Z721C83C5(nodeClass(n.Kind))), delay_1(() => append(singleton(attr_ref_1F9A456B((el) => {
        if (flashOn == null) {
        }
        else {
            onGrow(repaint, flashOn, () => {
                flash(p.Col * stagger, el);
            });
        }
    })), delay_1(() => append(singleton(Svg_rect_Z714D7FBE(ofArray([svgAttr_custom_Z384F8060("x", x(p).toString()), svgAttr_custom_Z384F8060("y", y(p).toString()), svgAttr_custom_Z384F8060("width", widthOf(p).toString()), svgAttr_custom_Z384F8060("height", boxH.toString()), svgAttr_custom_Z384F8060("rx", equals(n.Kind, NodeKind.Derived) ? "10" : "0")]))), delay_1(() => append(p.Bare ? singleton(Svg_text_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-label"), svgAttr_custom_Z384F8060("x", (x(p) + (widthOf(p) / 2)).toString()), svgAttr_custom_Z384F8060("y", ((y(p) + (boxH / 2)) + 4).toString()), svgAttr_custom_Z384F8060("text-anchor", "middle"), Html_text_Z721C83C5(n.Label)]))) : singleton(Html_fragment_Z714D7FBE(ofArray([Svg_text_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-label"), svgAttr_custom_Z384F8060("x", (x(p) + 10).toString()), svgAttr_custom_Z384F8060("y", (y(p) + 21).toString()), Html_text_Z721C83C5(n.Label)])), Svg_text_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-value"), svgAttr_custom_Z384F8060("x", (x(p) + 10).toString()), svgAttr_custom_Z384F8060("y", (y(p) + 42).toString()), Svg_elem("title", singleton_1(Html_text_5106B011(() => {
        Repaint__Track(repaint);
        return n.Value();
    }))), Html_text_5106B011(() => {
        Repaint__Track(repaint);
        return fit(n.Value());
    })]))]))), delay_1(() => {
        const matchValue = n.Badge ? n.Runs : undefined;
        if (matchValue == null) {
            return singleton(Html_get_none());
        }
        else {
            const runs_1 = matchValue;
            return singleton(Svg_g_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-badge"), Svg_elem("title", singleton_1(Html_text_Z721C83C5("Times this has run"))), Svg_rect_Z714D7FBE(ofArray([svgAttr_custom_Z384F8060("x", (rightX(p) - 31).toString()), svgAttr_custom_Z384F8060("y", (y(p) + 8).toString()), svgAttr_custom_Z384F8060("width", "24"), svgAttr_custom_Z384F8060("height", "15")])), Svg_text_Z714D7FBE(ofArray([svgAttr_custom_Z384F8060("x", (rightX(p) - 19).toString()), svgAttr_custom_Z384F8060("y", (y(p) + 19.5).toString()), svgAttr_custom_Z384F8060("text-anchor", "middle"), Html_text_5106B011(() => {
                Repaint__Track(repaint);
                return int32ToString(runs_1());
            })]))])));
        }
    })))))))))));
}

function drawEdge(repaint, placed, laneOf, fromKey, toKey) {
    const matchValue = tryFind(fromKey, placed);
    const matchValue_1 = tryFind(toKey, placed);
    let matchResult, a, b;
    if (matchValue != null) {
        if (matchValue_1 != null) {
            matchResult = 0;
            a = matchValue;
            b = matchValue_1;
        }
        else {
            matchResult = 1;
        }
    }
    else {
        matchResult = 1;
    }
    switch (matchResult) {
        case 0: {
            const x1 = rightX(a) + gap;
            const y1 = midY(a);
            const x2 = leftX(b) - gap;
            const y2 = midY(b);
            let path;
            if (Math.abs(y2 - y1) < 0.5) {
                path = toText(printf("M %f %f H %f"))(x1)(y1)(x2);
            }
            else {
                const gapStart = (padX + ((b.Col - 1) * colW)) + boxW;
                let elbow;
                let e_3;
                const e_1 = gapStart + (((padX + (b.Col * colW)) - gapStart) * laneOf(fromKey, toKey));
                e_3 = max_1(min_1(x1, x2), e_1);
                elbow = min_1(max_1(x1, x2), e_3);
                const step = (y2 > y1) ? 1 : -1;
                const radius = min(ofArray([12, Math.abs(y2 - y1) / 2, Math.abs(elbow - x1) * 0.55, Math.abs(x2 - elbow) * 0.55]), {
                    Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
                });
                const arg_5 = elbow - radius;
                const arg_9 = y1 + (step * radius);
                const arg_10 = y2 - (step * radius);
                const arg_13 = elbow + radius;
                path = toText(printf("M %f %f H %f Q %f %f %f %f V %f Q %f %f %f %f H %f"))(x1)(y1)(arg_5)(elbow)(y1)(elbow)(arg_9)(arg_10)(elbow)(y2)(arg_13)(y2)(x2);
            }
            return Svg_g_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-edge"), Svg_path_Z714D7FBE(ofArray([svgAttr_custom_Z384F8060("d", path), svgAttr_custom_Z384F8060("marker-end", "url(#sch-arrow)")])), Svg_circle_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-pulse"), svgAttr_custom_Z384F8060("r", "4"), attr_style_Z721C83C5(toText(printf("offset-path: path(\'%s\')"))(path)), attr_ref_1F9A456B((el) => {
                const matchValue_3 = b.Node.Runs;
                const matchValue_4 = a.Node.Runs;
                let matchResult_1, runs;
                if (matchValue_3 == null) {
                    if (matchValue_4 == null) {
                        matchResult_1 = 1;
                    }
                    else {
                        matchResult_1 = 0;
                        runs = matchValue_4;
                    }
                }
                else {
                    matchResult_1 = 0;
                    runs = matchValue_3;
                }
                switch (matchResult_1) {
                    case 0: {
                        let previousRuns = -1;
                        let valueAtLastRun = "";
                        Signal_effect(() => {
                            Repaint__Track(repaint);
                            Signal_untracked(b.Node.Value);
                            const now = runs() | 0;
                            const current = Signal_untracked(a.Node.Value);
                            if (previousRuns < 0) {
                                valueAtLastRun = current;
                            }
                            else if (now > previousRuns) {
                                if (current !== valueAtLastRun) {
                                    travel((a.Col * stagger) + 30, el);
                                }
                                valueAtLastRun = current;
                            }
                            previousRuns = (now | 0);
                        });
                        break;
                    }
                    case 1: {
                        break;
                    }
                }
            })]))]));
        }
        default:
            return Svg_g_Z714D7FBE(empty());
    }
}

/**
 * Draw a graph whose SHAPE can change.
 * 
 * `nodes` lists everything that should be drawn - including a node with no
 * edges at the moment, which is exactly the case `Signal.bind` produces - and
 * `edges` is re-read whenever the signals it touches change. Arrows go through
 * `Html.each` keyed by their endpoints, so a change really adds and removes
 * arrows rather than redrawing the picture.
 */
export function schematic(repaint, nodes, edges) {
    const initialEdges = Signal_untracked(edges);
    const placed = place(nodes, initialEdges);
    const byKey = ofList(map((p) => [p.Node.Key, p], placed), {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    });
    const outDegree = ofList(List_countBy((tupledArg_1) => (tupledArg_1[0].Key | 0), initialEdges, {
        Equals: (x_2, y_2) => (x_2 === y_2),
        GetHashCode: (x_2) => (numberHash(x_2) | 0),
    }), {
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    });
    const inDegree = ofList(List_countBy((tupledArg_2) => (tupledArg_2[1].Key | 0), initialEdges, {
        Equals: (x_4, y_4) => (x_4 === y_4),
        GetHashCode: (x_4) => (numberHash(x_4) | 0),
    }), {
        Compare: (x_5, y_5) => (comparePrimitives(x_5, y_5) | 0),
    });
    const groupOf = (fromKey, toKey) => {
        const fansIn = exists((n) => (n > 1), toArray(tryFind(toKey, inDegree)));
        const fansOut = exists((n_1) => (n_1 > 1), toArray(tryFind(fromKey, outDegree)));
        if (fansIn) {
            return [1, toKey];
        }
        else if (fansOut) {
            return [0, fromKey];
        }
        else {
            return [1, toKey];
        }
    };
    const colOf = (key_1) => {
        let option_6;
        return defaultArg((option_6 = tryFind(key_1, byKey), (option_6 != null) ? option_6.Col : undefined), 0) | 0;
    };
    const lanesByGap = ofList(map((tupledArg_4) => [tupledArg_4[0], sortBy((g) => [g[0], average(map((tupledArg_6) => {
        let option_3;
        return defaultArg((option_3 = tryFind(tupledArg_6[0].Key, byKey), (option_3 != null) ? option_3.Row : undefined), 0);
    }, filter((tupledArg_5) => equalArrays(groupOf(tupledArg_5[0].Key, tupledArg_5[1].Key), g), initialEdges)), {
        GetZero: () => 0,
        Add: (x_9, y_8) => (x_9 + y_8),
        DivideByInt: (x_8, i) => (x_8 / i),
    })], map((tuple_1) => tuple_1[1], tupledArg_4[1]), {
        Compare: (x_10, y_9) => (compareArrays(x_10, y_9) | 0),
    })], List_groupBy((tuple) => (tuple[0] | 0), List_distinct(map((tupledArg_3) => {
        const b_2 = tupledArg_3[1];
        return [colOf(b_2.Key), groupOf(tupledArg_3[0].Key, b_2.Key)];
    }, initialEdges), {
        Equals: equalArrays,
        GetHashCode: (x_6) => (arrayHash(x_6) | 0),
    }), {
        Equals: (x_7, y_7) => (x_7 === y_7),
        GetHashCode: (x_7) => (numberHash(x_7) | 0),
    })), {
        Compare: (x_11, y_10) => (comparePrimitives(x_11, y_10) | 0),
    });
    const width = ((padX * 2) + (max(map((p_4) => p_4.Col, placed), {
        Compare: (x_12, y_11) => (comparePrimitives(x_12, y_11) | 0),
    }) * colW)) + boxW;
    const height = ((padY * 2) + (max(map((p_5) => p_5.Row, placed), {
        Compare: (x_13, y_12) => (comparePrimitives(x_13, y_12) | 0),
    }) * rowH)) + boxH;
    return Html_figure_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-figure"), Html_div_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-frame"), Svg_svg_Z714D7FBE(ofArray([attr_className_Z721C83C5("schematic"), svgAttr_viewBox_Z721C83C5(toText(printf("0 0 %f %f"))(width)(height)), svgAttr_custom_Z384F8060("width", width.toString()), Svg_defs_Z714D7FBE(singleton_1(Svg_elem("marker", ofArray([svgAttr_custom_Z384F8060("id", "sch-arrow"), svgAttr_custom_Z384F8060("viewBox", "0 0 10 10"), svgAttr_custom_Z384F8060("refX", "10"), svgAttr_custom_Z384F8060("refY", "5"), svgAttr_custom_Z384F8060("markerUnits", "userSpaceOnUse"), svgAttr_custom_Z384F8060("markerWidth", "9"), svgAttr_custom_Z384F8060("markerHeight", "9"), svgAttr_custom_Z384F8060("orient", "auto-start-reverse"), Svg_path_Z714D7FBE(singleton_1(svgAttr_custom_Z384F8060("d", "M 0 0 L 10 5 L 0 10 z")))])))), Svg_g_Z714D7FBE(singleton_1(Html_each(() => toArray_1(map((tupledArg_7) => [tupledArg_7[0].Key, tupledArg_7[1].Key], edges())), (tupledArg_8) => ((int32ToString(tupledArg_8[0]) + "->") + int32ToString(tupledArg_8[1])), (tupledArg_9) => drawEdge(repaint, byKey, (fromKey_1, toKey_1) => {
        const group = groupOf(fromKey_1, toKey_1);
        const peers = defaultArg(tryFind(colOf(toKey_1), lanesByGap), singleton_1(group));
        const matchValue_3 = tryFindIndex((g_1) => equalArrays(g_1, group), peers);
        let matchResult, i_2;
        if (matchValue_3 != null) {
            if (length(peers) > 1) {
                matchResult = 0;
                i_2 = matchValue_3;
            }
            else {
                matchResult = 1;
            }
        }
        else {
            matchResult = 1;
        }
        switch (matchResult) {
            case 0:
                return 0.3 + (((0.62 - 0.3) * i_2) / (length(peers) - 1));
            default:
                if (group[0] === 0) {
                    return 0.3 + 0.04;
                }
                else {
                    return 0.58;
                }
        }
    }, tupledArg_9[0], tupledArg_9[1])))), Html_fragment_Z714D7FBE(map((p_6) => {
        let p_1, matchValue;
        return drawNode(repaint, (p_1 = p_6, (matchValue = p_1.Node.Runs, (matchValue == null) ? tryPick((tupledArg) => {
            if (tupledArg[1].Key === p_1.Node.Key) {
                return tupledArg[0].Runs;
            }
            else {
                return undefined;
            }
        }, initialEdges) : matchValue)), p_6);
    }, placed))]))]))]));
}

/**
 * Draw a graph whose shape never changes - the usual case.
 * 
 * The nodes are the ones the edges mention, in the order they first appear, so
 * this is the whole call:
 * 
 * graph repaint [ src ==> mapped; mapped ==> readerA; mapped ==> readerB ]
 */
export function graph(repaint, edges) {
    const seen = new Set([]);
    return schematic(repaint, toList(delay_1(() => collect((matchValue) => collect((n) => (addToSet(n.Key, seen) ? singleton(n) : empty_1()), [matchValue[0], matchValue[1]]), edges))), () => edges);
}

export const legend = Html_div_Z714D7FBE(toList(delay_1(() => append(singleton(attr_className_Z721C83C5("sch-legend")), delay_1(() => append(collect((matchValue) => singleton(Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-legend-item"), Html_span_Z714D7FBE(singleton_1(attr_className_Z721C83C5("sch-swatch " + matchValue[0]))), Html_text_Z721C83C5(matchValue[1])]))), [["sch-source", "Var - a source"], ["sch-derived", "Signal - a cached node"], ["sch-reader", "a DOM binding"], ["sch-ghost", "a plain function - no node"]]), delay_1(() => singleton(Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-legend-item"), Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("sch-swatch sch-count"), Html_text_Z721C83C5("0")])), Html_text_Z721C83C5("times it has run")]))))))))));

