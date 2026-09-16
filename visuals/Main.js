
import { Html_render_62D6BEC0, Html_text_Z721C83C5, Html_button_Z714D7FBE, Html_div_Z714D7FBE } from "./src/Fable.Ripple.Dom/Html.js";
import { empty, singleton, append, delay, toList } from "./fable_modules/fable-library-js.5.13.0/Seq.js";
import { attr_style_Z721C83C5 } from "./src/Fable.Ripple.Dom/Attributes.js";
import { Probe__get_Hits, Repaint__Now, Probe__Hit, Probe_$ctor_Z721C83C5, Repaint_$ctor } from "./demo/Examples/Widgets.js";
import { Signal_batch, Signal_effect, Signal_computed, Var_create } from "./src/Fable.Ripple/Api.js";
import { Var$1__Peek, Var$1__set_Value_2B595, Var$1__get_Value } from "./src/Fable.Ripple/Types.js";
import { NodeModule_bindingWith, NodeModule_derivedWith, NodeModule_sourceWith } from "./demo/Examples/Schematic.js";
import { comparePrimitives, int32ToString } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { NodeModule_beating, NodeModule_bindingWith as NodeModule_bindingWith_1, schematic, legend, op_EqualsEqualsGreater, graph, NodeModule_counting } from "./demo/Examples/Schematic.js";
import { Var$1__get_Value as Var$1__get_Value_1, Var$1__Peek as Var$1__Peek_1 } from "./src/Fable.Ripple/Types.js";
import { on_click_58BC8925 } from "./src/Fable.Ripple.Dom/Events.js";
import { singleton as singleton_1, ofArray } from "./fable_modules/fable-library-js.5.13.0/List.js";
import { Signal_effect as Signal_effect_1, Signal_computed as Signal_computed_1 } from "./src/Fable.Ripple/Api.js";
import { tryFind, ofList } from "./fable_modules/fable-library-js.5.13.0/Map.js";
import { concat } from "./fable_modules/fable-library-js.5.13.0/String.js";

function controls(buttons) {
    return Html_div_Z714D7FBE(toList(delay(() => append(singleton(attr_style_Z721C83C5("display: flex; gap: 0.5rem; flex-wrap: wrap; margin: 0 0 0.75rem 0")), delay(() => buttons)))));
}

function introGraph() {
    const repaint = Repaint_$ctor();
    const runs = Probe_$ctor_Z721C83C5("runs");
    const a = Var_create(1);
    const b = Var_create(10);
    const total = Signal_computed(() => {
        Probe__Hit(runs);
        return (Var$1__get_Value(a) + Var$1__get_Value(b)) | 0;
    });
    const bump = (v) => {
        Var$1__set_Value_2B595(v, Var$1__get_Value(v) + 1);
        Repaint__Now(repaint);
    };
    const aNode = NodeModule_sourceWith("Var a", () => int32ToString(Var$1__Peek(a)));
    const bNode = NodeModule_sourceWith("Var b", () => int32ToString(Var$1__Peek(b)));
    const totalNode = NodeModule_counting(runs, NodeModule_derivedWith("Signal.computed", () => int32ToString(Var$1__Peek_1(total))));
    const readNode = NodeModule_bindingWith("binding", () => int32ToString(Var$1__Peek_1(total)));
    return Html_div_Z714D7FBE(ofArray([controls(ofArray([Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg) => {
        bump(a);
    }), Html_text_Z721C83C5("Bump a")])), Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_1) => {
        bump(b);
    }), Html_text_Z721C83C5("Bump b")]))])), graph(repaint, ofArray([op_EqualsEqualsGreater(aNode, totalNode), op_EqualsEqualsGreater(bNode, totalNode), op_EqualsEqualsGreater(totalNode, readNode)])), legend]));
}

function bindRewiring() {
    const repaint = Repaint_$ctor();
    const runs = Probe_$ctor_Z721C83C5("runs");
    const celsius = Var_create(20);
    const fahrenheit = Var_create(68);
    const useCelsius = Var_create(true);
    const shown = Signal_computed_1(() => {
        let copyOfStruct;
        const c = Var$1__get_Value(useCelsius);
        Probe__Hit(runs);
        copyOfStruct = (c ? celsius : fahrenheit);
        return Var$1__get_Value_1(copyOfStruct) | 0;
    });
    const bump = (v) => {
        Var$1__set_Value_2B595(v, Var$1__get_Value(v) + 1);
        Repaint__Now(repaint);
    };
    const selectorNode = NodeModule_sourceWith("useCelsius", () => (Var$1__Peek(useCelsius) ? "true" : "false"));
    const celsiusNode = NodeModule_sourceWith("Var celsius", () => int32ToString(Var$1__Peek(celsius)));
    const fahrenheitNode = NodeModule_sourceWith("Var fahrenheit", () => int32ToString(Var$1__Peek(fahrenheit)));
    const bindNode = NodeModule_counting(runs, NodeModule_derivedWith("Signal.bind", () => int32ToString(Var$1__Peek_1(shown))));
    const readNode = NodeModule_bindingWith("binding", () => int32ToString(Var$1__Peek_1(shown)));
    return Html_div_Z714D7FBE(ofArray([controls(ofArray([Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg) => {
        Var$1__set_Value_2B595(useCelsius, !Var$1__get_Value(useCelsius));
        Repaint__Now(repaint);
    }), Html_text_Z721C83C5("Flip the selector")])), Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_1) => {
        bump(celsius);
    }), Html_text_Z721C83C5("Bump celsius")])), Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_2) => {
        bump(fahrenheit);
    }), Html_text_Z721C83C5("Bump fahrenheit")]))])), schematic(repaint, ofArray([selectorNode, celsiusNode, fahrenheitNode, bindNode, readNode]), () => toList(delay(() => append(singleton(op_EqualsEqualsGreater(selectorNode, bindNode)), delay(() => append(Var$1__get_Value(useCelsius) ? singleton(op_EqualsEqualsGreater(celsiusNode, bindNode)) : singleton(op_EqualsEqualsGreater(fahrenheitNode, bindNode)), delay(() => singleton(op_EqualsEqualsGreater(bindNode, readNode))))))))), legend]));
}

function cutoffChain() {
    const repaint = Repaint_$ctor();
    const parityRuns = Probe_$ctor_Z721C83C5("runs");
    const upperRuns = Probe_$ctor_Z721C83C5("runs");
    const n = Var_create(4);
    const parity = Signal_computed_1(() => {
        const v = Var$1__get_Value(n) | 0;
        Probe__Hit(parityRuns);
        return ((v % 2) === 0) ? "even" : "odd";
    });
    const upper = Signal_computed_1(() => {
        const p = Var$1__get_Value_1(parity);
        Probe__Hit(upperRuns);
        return p.toLocaleUpperCase() + "!";
    });
    const step = (by) => {
        Var$1__set_Value_2B595(n, Var$1__get_Value(n) + by);
        Repaint__Now(repaint);
    };
    const nNode = NodeModule_sourceWith("n", () => int32ToString(Var$1__Peek(n)));
    const parityNode = NodeModule_counting(parityRuns, NodeModule_derivedWith("parity", () => Var$1__Peek_1(parity)));
    const upperNode = NodeModule_counting(upperRuns, NodeModule_derivedWith("upper", () => Var$1__Peek_1(upper)));
    const readNode = NodeModule_bindingWith("binding", () => Var$1__Peek_1(upper));
    return Html_div_Z714D7FBE(ofArray([controls(ofArray([Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg) => {
        step(2);
    }), Html_text_Z721C83C5("+2 (parity kept)")])), Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_1) => {
        step(1);
    }), Html_text_Z721C83C5("+1 (parity flips)")]))])), graph(repaint, ofArray([op_EqualsEqualsGreater(nNode, parityNode), op_EqualsEqualsGreater(parityNode, upperNode), op_EqualsEqualsGreater(upperNode, readNode)])), legend]));
}

function equalWrite() {
    const repaint = Repaint_$ctor();
    const runs = Probe_$ctor_Z721C83C5("runs");
    const name = Var_create("ada");
    Signal_effect(() => {
        Probe__Hit(runs);
        Var$1__get_Value(name);
    });
    const write = (value_2) => {
        Var$1__set_Value_2B595(name, value_2);
        Repaint__Now(repaint);
    };
    const nameNode = NodeModule_sourceWith("Var name", () => Var$1__Peek(name));
    const effectNode = NodeModule_counting(runs, NodeModule_bindingWith_1("effect", () => Var$1__Peek(name)));
    return Html_div_Z714D7FBE(ofArray([controls(ofArray([Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg) => {
        write(Var$1__Peek(name));
    }), Html_text_Z721C83C5("Write the same value again")])), Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_1) => {
        write(Var$1__Peek(name) + "!");
    }), Html_text_Z721C83C5("Write a new value")]))])), graph(repaint, singleton_1(op_EqualsEqualsGreater(nameNode, effectNode))), legend]));
}

function autoTracking() {
    const repaint = Repaint_$ctor();
    const runs = Probe_$ctor_Z721C83C5("runs");
    const a = Var_create(1);
    const b = Var_create(10);
    const includeB = Var_create(false);
    const total = Signal_computed(() => {
        Probe__Hit(runs);
        return (Var$1__get_Value(includeB) ? (Var$1__get_Value(a) + Var$1__get_Value(b)) : Var$1__get_Value(a)) | 0;
    });
    const bump = (v) => {
        Var$1__set_Value_2B595(v, Var$1__get_Value(v) + 1);
        Repaint__Now(repaint);
    };
    const switchNode = NodeModule_sourceWith("read b", () => (Var$1__Peek(includeB) ? "true" : "false"));
    const aNode = NodeModule_sourceWith("Var a", () => int32ToString(Var$1__Peek(a)));
    const bNode = NodeModule_sourceWith("Var b", () => int32ToString(Var$1__Peek(b)));
    const totalNode = NodeModule_counting(runs, NodeModule_derivedWith("Signal.computed", () => int32ToString(Var$1__Peek_1(total))));
    const readNode = NodeModule_bindingWith("binding", () => int32ToString(Var$1__Peek_1(total)));
    return Html_div_Z714D7FBE(ofArray([controls(ofArray([Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg) => {
        Var$1__set_Value_2B595(includeB, !Var$1__get_Value(includeB));
        Repaint__Now(repaint);
    }), Html_text_Z721C83C5("Toggle read b")])), Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_1) => {
        bump(a);
    }), Html_text_Z721C83C5("Bump a")])), Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_2) => {
        bump(b);
    }), Html_text_Z721C83C5("Bump b")]))])), schematic(repaint, ofArray([switchNode, aNode, bNode, totalNode, readNode]), () => toList(delay(() => append(singleton(op_EqualsEqualsGreater(switchNode, totalNode)), delay(() => append(singleton(op_EqualsEqualsGreater(aNode, totalNode)), delay(() => append(Var$1__get_Value(includeB) ? singleton(op_EqualsEqualsGreater(bNode, totalNode)) : empty(), delay(() => singleton(op_EqualsEqualsGreater(totalNode, readNode))))))))))), legend]));
}

function batching() {
    const repaint = Repaint_$ctor();
    const fired = Probe_$ctor_Z721C83C5("fired");
    const recomputed = Probe_$ctor_Z721C83C5("Signal.map3");
    const a = Var_create(0);
    const b = Var_create(0);
    const c = Var_create(0);
    const total = Signal_computed_1(() => {
        const x = Var$1__get_Value(a) | 0;
        const y = Var$1__get_Value(b) | 0;
        const z = Var$1__get_Value(c) | 0;
        Probe__Hit(recomputed);
        return ((x + y) + z) | 0;
    });
    Signal_effect_1(() => {
        Var$1__get_Value_1(total);
        Probe__Hit(fired);
    });
    const writeAll = (batchIt) => {
        window.setTimeout(() => {
            const writes = () => {
                Var$1__set_Value_2B595(a, Var$1__get_Value(a) + 1);
                Var$1__set_Value_2B595(b, Var$1__get_Value(b) + 1);
                Var$1__set_Value_2B595(c, Var$1__get_Value(c) + 1);
            };
            if (batchIt) {
                Signal_batch(writes);
            }
            else {
                writes();
            }
            Repaint__Now(repaint);
        }, 0);
    };
    const aNode = NodeModule_sourceWith("Var a", () => int32ToString(Var$1__Peek(a)));
    const bNode = NodeModule_sourceWith("Var b", () => int32ToString(Var$1__Peek(b)));
    const cNode = NodeModule_sourceWith("Var c", () => int32ToString(Var$1__Peek(c)));
    const totalNode = NodeModule_beating(() => (Probe__get_Hits(recomputed) | 0), NodeModule_derivedWith("Signal.map3", () => int32ToString(Var$1__Peek_1(total))));
    const subNode = NodeModule_counting(fired, NodeModule_bindingWith_1("subscriber", () => int32ToString(Var$1__Peek_1(total))));
    return Html_div_Z714D7FBE(ofArray([controls(ofArray([Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_1) => {
        writeAll(false);
    }), Html_text_Z721C83C5("Three writes, no batch")])), Html_button_Z714D7FBE(ofArray([on_click_58BC8925((_arg_2) => {
        writeAll(true);
    }), Html_text_Z721C83C5("Three writes, batched")]))])), graph(repaint, ofArray([op_EqualsEqualsGreater(aNode, totalNode), op_EqualsEqualsGreater(bNode, totalNode), op_EqualsEqualsGreater(cNode, totalNode), op_EqualsEqualsGreater(totalNode, subNode)])), legend]));
}

const visuals = ofList(ofArray([["signals-intro", introGraph], ["signals-bind", bindRewiring], ["signals-cutoff", cutoffChain], ["signals-equal-write", equalWrite], ["signals-autotrack", autoTracking], ["signals-batch", batching]]), {
    Compare: (x, y) => (comparePrimitives(x, y) | 0),
});

function mountAll() {
    const hosts = document.querySelectorAll("[data-visual]");
    for (let i = 0; i <= (hosts.length - 1); i++) {
        const host = hosts.item(i);
        const name = host.getAttribute("data-visual");
        const matchValue = tryFind(name, visuals);
        if (matchValue == null) {
            console.warn(concat("No visual named \'", name, "\'"));
        }
        else {
            const make = matchValue;
            host.appendChild(Html_render_62D6BEC0(make()));
        }
    }
}

mountAll();

