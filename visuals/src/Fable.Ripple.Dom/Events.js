
import { class_type } from "../../fable_modules/fable-library-js.5.18.0/Reflection.js";
import { Base_onEvent } from "./Base.js";
import { value } from "../../fable_modules/fable-library-js.5.18.0/Option.js";

/**
 * DOM events, read as `on.click`, `on.input`, `on.keyDown`, ... Every handler's
 * writes are auto-batched, so one user gesture is a single flush no matter how
 * many signals it touches. Handlers receive the concrete event type.
 */
export class on {
    constructor() {
    }
}

export function on_$reflection() {
    return class_type("Fable.Ripple.Dom.on", undefined, on);
}

export function on_click_2527B487(h) {
    return Base_onEvent("click", h);
}

export function on_dblClick_2527B487(h) {
    return Base_onEvent("dblclick", h);
}

export function on_auxClick_2527B487(h) {
    return Base_onEvent("auxclick", h);
}

export function on_contextMenu_2527B487(h) {
    return Base_onEvent("contextmenu", h);
}

export function on_mouseDown_2527B487(h) {
    return Base_onEvent("mousedown", h);
}

export function on_mouseUp_2527B487(h) {
    return Base_onEvent("mouseup", h);
}

export function on_mouseEnter_2527B487(h) {
    return Base_onEvent("mouseenter", h);
}

export function on_mouseLeave_2527B487(h) {
    return Base_onEvent("mouseleave", h);
}

export function on_mouseMove_2527B487(h) {
    return Base_onEvent("mousemove", h);
}

export function on_mouseOver_2527B487(h) {
    return Base_onEvent("mouseover", h);
}

export function on_mouseOut_2527B487(h) {
    return Base_onEvent("mouseout", h);
}

export function on_pointerDown_Z277A7623(h) {
    return Base_onEvent("pointerdown", h);
}

export function on_pointerUp_Z277A7623(h) {
    return Base_onEvent("pointerup", h);
}

export function on_pointerMove_Z277A7623(h) {
    return Base_onEvent("pointermove", h);
}

export function on_pointerEnter_Z277A7623(h) {
    return Base_onEvent("pointerenter", h);
}

export function on_pointerLeave_Z277A7623(h) {
    return Base_onEvent("pointerleave", h);
}

export function on_pointerOver_Z277A7623(h) {
    return Base_onEvent("pointerover", h);
}

export function on_pointerOut_Z277A7623(h) {
    return Base_onEvent("pointerout", h);
}

export function on_pointerCancel_Z277A7623(h) {
    return Base_onEvent("pointercancel", h);
}

export function on_keyDown_Z3CE1335(h) {
    return Base_onEvent("keydown", h);
}

export function on_keyUp_Z3CE1335(h) {
    return Base_onEvent("keyup", h);
}

export function on_keyPress_Z3CE1335(h) {
    return Base_onEvent("keypress", h);
}

export function on_focus_41BEA0AA(h) {
    return Base_onEvent("focus", h);
}

export function on_blur_41BEA0AA(h) {
    return Base_onEvent("blur", h);
}

export function on_focusIn_41BEA0AA(h) {
    return Base_onEvent("focusin", h);
}

export function on_focusOut_41BEA0AA(h) {
    return Base_onEvent("focusout", h);
}

export function on_wheel_Z1887B2CB(h) {
    return Base_onEvent("wheel", h);
}

/**
 * Fires on each keystroke; hands the input's current text.
 */
export function on_input_41EFD311(h) {
    return Base_onEvent("input", (ev) => {
        h(value(ev.target).value);
    });
}

/**
 * Raw `input` event.
 */
export function on_input_6763566(h) {
    return Base_onEvent("input", h);
}

/**
 * Fires on commit (blur/enter); hands the input's current text.
 */
export function on_change_41EFD311(h) {
    return Base_onEvent("change", (ev) => {
        h(value(ev.target).value);
    });
}

/**
 * Checkbox/radio commit; hands the `checked` state.
 */
export function on_checkedChange_50F94480(h) {
    return Base_onEvent("change", (ev) => {
        h(value(ev.target).checked);
    });
}

export function on_submit_6763566(h) {
    return Base_onEvent("submit", h);
}

export function on_reset_6763566(h) {
    return Base_onEvent("reset", h);
}

export function on_invalid_6763566(h) {
    return Base_onEvent("invalid", h);
}

export function on_select_6763566(h) {
    return Base_onEvent("select", h);
}

export function on_copy_1C1A7A4A(h) {
    return Base_onEvent("copy", h);
}

export function on_cut_1C1A7A4A(h) {
    return Base_onEvent("cut", h);
}

export function on_paste_1C1A7A4A(h) {
    return Base_onEvent("paste", h);
}

export function on_drag_D9F39F6(h) {
    return Base_onEvent("drag", h);
}

export function on_dragStart_D9F39F6(h) {
    return Base_onEvent("dragstart", h);
}

export function on_dragEnd_D9F39F6(h) {
    return Base_onEvent("dragend", h);
}

export function on_dragEnter_D9F39F6(h) {
    return Base_onEvent("dragenter", h);
}

export function on_dragOver_D9F39F6(h) {
    return Base_onEvent("dragover", h);
}

export function on_dragLeave_D9F39F6(h) {
    return Base_onEvent("dragleave", h);
}

export function on_drop_D9F39F6(h) {
    return Base_onEvent("drop", h);
}

export function on_touchStart_Z72A8DC7D(h) {
    return Base_onEvent("touchstart", h);
}

export function on_touchMove_Z72A8DC7D(h) {
    return Base_onEvent("touchmove", h);
}

export function on_touchEnd_Z72A8DC7D(h) {
    return Base_onEvent("touchend", h);
}

export function on_touchCancel_Z72A8DC7D(h) {
    return Base_onEvent("touchcancel", h);
}

export function on_scroll_6763566(h) {
    return Base_onEvent("scroll", h);
}

export function on_load_6763566(h) {
    return Base_onEvent("load", h);
}

export function on_error_6763566(h) {
    return Base_onEvent("error", h);
}

export function on_loadedData_6763566(h) {
    return Base_onEvent("loadeddata", h);
}

export function on_loadedMetadata_6763566(h) {
    return Base_onEvent("loadedmetadata", h);
}

export function on_canPlay_6763566(h) {
    return Base_onEvent("canplay", h);
}

export function on_play_6763566(h) {
    return Base_onEvent("play", h);
}

export function on_pause_6763566(h) {
    return Base_onEvent("pause", h);
}

export function on_ended_6763566(h) {
    return Base_onEvent("ended", h);
}

export function on_timeUpdate_6763566(h) {
    return Base_onEvent("timeupdate", h);
}

export function on_volumeChange_6763566(h) {
    return Base_onEvent("volumechange", h);
}

export function on_seeked_6763566(h) {
    return Base_onEvent("seeked", h);
}

export function on_seeking_6763566(h) {
    return Base_onEvent("seeking", h);
}

export function on_durationChange_6763566(h) {
    return Base_onEvent("durationchange", h);
}

export function on_waiting_6763566(h) {
    return Base_onEvent("waiting", h);
}

export function on_animationStart_6763566(h) {
    return Base_onEvent("animationstart", h);
}

export function on_animationEnd_6763566(h) {
    return Base_onEvent("animationend", h);
}

export function on_animationIteration_6763566(h) {
    return Base_onEvent("animationiteration", h);
}

export function on_transitionEnd_6763566(h) {
    return Base_onEvent("transitionend", h);
}

/**
 * Any event by name.
 */
export function on_event_4C2536FD(name, h) {
    return Base_onEvent(name, h);
}

