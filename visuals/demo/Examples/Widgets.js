
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Signal_observerCount, Signal_effect, Var_create } from "../../src/Fable.Ripple/Api.js";
import { Var$1__get_Value, Var$1__Peek, Var$1__set_Value_2B595 } from "../../src/Fable.Ripple/Types.js";
import { Html_each, Html_fragment_Z714D7FBE, Html_text_5106B011, Html_text_Z721C83C5, Html_span_Z714D7FBE, Html_div_Z714D7FBE } from "../../src/Fable.Ripple.Dom/Html.js";
import { attr_ref_1F9A456B, attr_classList_ZA225E0A, attr_className_Z721C83C5 } from "../../src/Fable.Ripple.Dom/Attributes.js";
import { toArray, length, tryFindIndex, singleton, append, empty, map, ofArray } from "../../fable_modules/fable-library-js.5.13.0/List.js";
import { int32ToString } from "../../fable_modules/fable-library-js.5.13.0/Util.js";
import { map as map_1, singleton as singleton_1, append as append_1, delay, toList } from "../../fable_modules/fable-library-js.5.13.0/Seq.js";
import { mapIndexed } from "../../fable_modules/fable-library-js.5.13.0/Array.js";

/**
 * A hit counter, called from inside a computation to count how often it ran.
 * 
 * The count is a plain mutable rather than a `Var`, and that is not laziness:
 * incrementing a signal means reading it first, and a read inside a compute
 * function registers a dependency - so the computation would invalidate itself
 * and re-run forever. Counts are therefore mutated freely and the display is
 * refreshed explicitly, with `Repaint` below.
 */
export class Probe {
    constructor(label) {
        this.label = label;
        this.hits = 0;
    }
}

export function Probe_$reflection() {
    return class_type("Demo.Examples.Widgets.Probe", undefined, Probe);
}

export function Probe_$ctor_Z721C83C5(label) {
    return new Probe(label);
}

export function Probe__get_Label(_) {
    return _.label;
}

export function Probe__get_Hits(_) {
    return _.hits | 0;
}

export function Probe__Hit(_) {
    _.hits = ((_.hits + 1) | 0);
}

export function Probe__Reset(_) {
    _.hits = 0;
}

/**
 * A token the chips depend on. Bump it from an event handler *after* mutating:
 * writes flush synchronously, so by then every computation has already run and
 * the counts are final.
 * 
 * `Now` increments through `Peek`, and that is load-bearing. An example that
 * calls it while its own view is being built - `Html.dynamic` is tracking at
 * that moment - would otherwise register the read, making the whole component
 * depend on its repaint token and rebuild itself on the next bump.
 */
export class Repaint {
    constructor() {
        this.tick = Var_create(0);
    }
}

export function Repaint_$reflection() {
    return class_type("Demo.Examples.Widgets.Repaint", undefined, Repaint);
}

export function Repaint_$ctor() {
    return new Repaint();
}

export function Repaint__Now(_) {
    Var$1__set_Value_2B595(_.tick, Var$1__Peek(_.tick) + 1);
}

/**
 * Read inside a binding to make it re-run when `Now` is called.
 */
export function Repaint__Track(_) {
    Var$1__get_Value(_.tick);
}

/**
 * Bump `repaint` whenever any signal `outputs` reads changes.
 * 
 * For an example driven by buttons, the handler bumps it and that is that. For
 * one driven by a text box there is no handler to hang it on, so this watches
 * instead. Read the OUTPUTS, not the inputs: the effect is then downstream of
 * the computations being measured and therefore runs after them, so the counts
 * it publishes are the final ones rather than the ones from a moment ago.
 */
export function repaintAfter(repaint, outputs) {
    Signal_effect(() => {
        outputs();
        Repaint__Now(repaint);
    });
}

function chip(repaint, p) {
    return Html_div_Z714D7FBE(ofArray([attr_className_Z721C83C5("probe"), Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("probe-label"), Html_text_Z721C83C5(Probe__get_Label(p))])), Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("probe-count"), Html_text_5106B011(() => {
        Repaint__Track(repaint);
        return int32ToString(Probe__get_Hits(p));
    })]))]));
}

/**
 * A row of run counters.
 */
export function probes(repaint, ps) {
    return Html_div_Z714D7FBE(ofArray([attr_className_Z721C83C5("probes"), Html_fragment_Z714D7FBE(map((p) => chip(repaint, p), ps))]));
}

/**
 * A binding that reads a source and renders nothing.
 * 
 * For the examples whose claim is "only the binding that reads it ran", but
 * where the schematic already shows the value - so a `readout` beside it would
 * only repeat the picture.
 * 
 * There has to be a real binding somewhere: counting from an event handler
 * instead would count CLICKS, and the number would no longer be evidence -
 * press-driven counters read the same whether the engine re-ran one binding,
 * both, or neither. This is the smallest thing that is genuinely a binding.
 * 
 * It reads only the source, deliberately. A repaint token in here would give
 * it a second reason to re-run and the count would stop meaning "this changed".
 */
export function readerOf(probe, read) {
    return Html_text_5106B011(() => {
        Probe__Hit(probe);
        read();
        return "";
    });
}

/**
 * A single labelled number, for readouts that are not run counts.
 */
export function readout(label, value) {
    return Html_div_Z714D7FBE(ofArray([attr_className_Z721C83C5("probe"), Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("probe-label"), Html_text_Z721C83C5(label)])), Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("probe-count"), Html_text_5106B011(value)]))]));
}

/**
 * A readout that does NOT subscribe to what it shows: it refreshes off the
 * repaint token instead. Use it wherever the page prints `Signal.observerCount`
 * - a plain `readout` would itself become one of the observers it is counting.
 * 
 * This is the general form, for a value that is not simply a source's contents.
 */
export function peekedWith(repaint, label, value) {
    return readout(label, () => {
        Repaint__Track(repaint);
        return value();
    });
}

/**
 * A live observer count, read without becoming one of the observers counted.
 * 
 * `Signal.observerCount` takes a read-only view, so call sites pass
 * `source.Signal` - which is worth leaving visible rather than hiding behind
 * this helper, since handing out `.Signal` is itself the thing Sharing state
 * is about.
 */
export function observers(repaint, label, s) {
    return peekedWith(repaint, label, () => int32ToString(Signal_observerCount(s)));
}

/**
 * Records, IN ORDER, which computations ran during a single write.
 * 
 * A run counter answers "how often"; this answers "what happened, and in what
 * sequence" - which is the part that makes a cutoff legible, because a
 * computation that did not run leaves a visible hole rather than a number that
 * merely failed to move.
 * 
 * Plain mutable state, for the reason `Probe` is: a `Var` read from inside a
 * computation would make that computation depend on itself.
 */
export class Trace {
    constructor() {
        this.order = empty();
    }
}

export function Trace_$reflection() {
    return class_type("Demo.Examples.Widgets.Trace", undefined, Trace);
}

export function Trace_$ctor() {
    return new Trace();
}

/**
 * Call immediately before the write being traced.
 */
export function Trace__Begin(_) {
    _.order = empty();
}

/**
 * Call from inside the computation, as `Probe.Hit` is called.
 */
export function Trace__Note_Z721C83C5(_, name) {
    _.order = append(_.order, singleton(name));
}

/**
 * 1-based position in this write's run order, or `None` if it never ran.
 */
export function Trace__PositionOf_Z721C83C5(_, name) {
    const option_1 = tryFindIndex((n) => (n === name), _.order);
    if (option_1 != null) {
        return 1 + option_1;
    }
    else {
        return undefined;
    }
}

export function Trace__get_Length(_) {
    return length(_.order) | 0;
}

/**
 * The trace as a strip: every computation that COULD have run, in graph order,
 * each showing where it came in the sequence or that it was skipped.
 */
export function traceStrip(repaint, trace, names) {
    return Html_div_Z714D7FBE(toList(delay(() => append_1(singleton_1(attr_className_Z721C83C5("trace")), delay(() => map_1((name) => Html_div_Z714D7FBE(ofArray([attr_className_Z721C83C5("trace-step"), attr_classList_ZA225E0A(() => {
        Repaint__Track(repaint);
        return singleton(["is-skipped", Trace__PositionOf_Z721C83C5(trace, name) == null]);
    }), Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("trace-order"), Html_text_5106B011(() => {
        Repaint__Track(repaint);
        const matchValue = Trace__PositionOf_Z721C83C5(trace, name);
        return (matchValue == null) ? "-" : int32ToString(matchValue);
    })])), Html_span_Z714D7FBE(ofArray([attr_className_Z721C83C5("trace-name"), Html_text_Z721C83C5(name)]))])), names))))));
}

/**
 * An append-only log, for showing how many times something fired.
 * 
 * `Add` reads the current lines through `Peek`, not `.Value`, and that matters:
 * logs are usually appended to from inside a `Signal.effect`, and a tracked read
 * there would make the effect depend on its own output - so clearing the log
 * would re-trigger the effect and immediately re-fill it.
 */
export class Log {
    constructor() {
        this.lines = Var_create(empty());
    }
}

export function Log_$reflection() {
    return class_type("Demo.Examples.Widgets.Log", undefined, Log);
}

export function Log_$ctor() {
    return new Log();
}

export function Log__get_Lines(_) {
    return _.lines;
}

export function Log__Add_Z721C83C5(_, line) {
    Var$1__set_Value_2B595(_.lines, append(Var$1__Peek(_.lines), singleton(line)));
}

export function Log__Clear(_) {
    Var$1__set_Value_2B595(_.lines, empty());
}

export function Log__get_Count(_) {
    return length(Var$1__Peek(_.lines)) | 0;
}

export function logPane(log) {
    return Html_div_Z714D7FBE(ofArray([attr_className_Z721C83C5("log"), attr_ref_1F9A456B((el) => {
        Signal_effect(() => {
            Var$1__get_Value(Log__get_Lines(log));
            window.setTimeout(() => {
                el.scrollTop = el.scrollHeight;
            }, 0);
        });
    }), Html_each(() => mapIndexed((i, l) => [i, l], toArray(Var$1__get_Value(Log__get_Lines(log)))), (tuple) => (tuple[0] | 0), (tupledArg) => Html_div_Z714D7FBE(singleton(Html_text_Z721C83C5(tupledArg[1]))))]));
}

