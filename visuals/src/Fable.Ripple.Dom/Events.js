
import { class_type } from "../../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Base_onEvent } from "./Base.js";
import { Signal_batch } from "../Fable.Ripple/Api.js";

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

export function on_click_58BC8925(h) {
    return Base_onEvent("click", h);
}

export function on_dblClick_58BC8925(h) {
    return Base_onEvent("dblclick", h);
}

export function on_auxClick_58BC8925(h) {
    return Base_onEvent("auxclick", h);
}

export function on_contextMenu_58BC8925(h) {
    return Base_onEvent("contextmenu", h);
}

export function on_mouseDown_58BC8925(h) {
    return Base_onEvent("mousedown", h);
}

export function on_mouseUp_58BC8925(h) {
    return Base_onEvent("mouseup", h);
}

export function on_mouseEnter_58BC8925(h) {
    return Base_onEvent("mouseenter", h);
}

export function on_mouseLeave_58BC8925(h) {
    return Base_onEvent("mouseleave", h);
}

export function on_mouseMove_58BC8925(h) {
    return Base_onEvent("mousemove", h);
}

export function on_mouseOver_58BC8925(h) {
    return Base_onEvent("mouseover", h);
}

export function on_mouseOut_58BC8925(h) {
    return Base_onEvent("mouseout", h);
}

export function on_pointerDown_20E144FF(h) {
    return Base_onEvent("pointerdown", h);
}

export function on_pointerUp_20E144FF(h) {
    return Base_onEvent("pointerup", h);
}

export function on_pointerMove_20E144FF(h) {
    return Base_onEvent("pointermove", h);
}

export function on_pointerEnter_20E144FF(h) {
    return Base_onEvent("pointerenter", h);
}

export function on_pointerLeave_20E144FF(h) {
    return Base_onEvent("pointerleave", h);
}

export function on_pointerOver_20E144FF(h) {
    return Base_onEvent("pointerover", h);
}

export function on_pointerOut_20E144FF(h) {
    return Base_onEvent("pointerout", h);
}

export function on_pointerCancel_20E144FF(h) {
    return Base_onEvent("pointercancel", h);
}

export function on_keyDown_Z2153A397(h) {
    return Base_onEvent("keydown", h);
}

export function on_keyUp_Z2153A397(h) {
    return Base_onEvent("keyup", h);
}

export function on_keyPress_Z2153A397(h) {
    return Base_onEvent("keypress", h);
}

export function on_focus_13C15648(h) {
    return Base_onEvent("focus", h);
}

export function on_blur_13C15648(h) {
    return Base_onEvent("blur", h);
}

export function on_focusIn_13C15648(h) {
    return Base_onEvent("focusin", h);
}

export function on_focusOut_13C15648(h) {
    return Base_onEvent("focusout", h);
}

export function on_wheel_4B7763D7(h) {
    return Base_onEvent("wheel", h);
}

/**
 * Fires on each keystroke; hands the input's current text.
 */
export function on_input_41EFD311(h) {
    return (e) => {
        e.addEventListener("input", (ev) => {
            Signal_batch(() => {
                h(ev.target.value);
            });
        });
    };
}

/**
 * Raw `input` event.
 */
export function on_input_7DDE0344(h) {
    return Base_onEvent("input", h);
}

/**
 * Fires on commit (blur/enter); hands the input's current text.
 */
export function on_change_41EFD311(h) {
    return (e) => {
        e.addEventListener("change", (ev) => {
            Signal_batch(() => {
                h(ev.target.value);
            });
        });
    };
}

/**
 * Checkbox/radio commit; hands the `checked` state.
 */
export function on_checkedChange_50F94480(h) {
    return (e) => {
        e.addEventListener("change", (ev) => {
            Signal_batch(() => {
                h(ev.target.checked);
            });
        });
    };
}

export function on_submit_7DDE0344(h) {
    return Base_onEvent("submit", h);
}

export function on_reset_7DDE0344(h) {
    return Base_onEvent("reset", h);
}

export function on_invalid_7DDE0344(h) {
    return Base_onEvent("invalid", h);
}

export function on_select_7DDE0344(h) {
    return Base_onEvent("select", h);
}

export function on_copy_650C9FE8(h) {
    return Base_onEvent("copy", h);
}

export function on_cut_650C9FE8(h) {
    return Base_onEvent("cut", h);
}

export function on_paste_650C9FE8(h) {
    return Base_onEvent("paste", h);
}

export function on_drag_Z3384A56C(h) {
    return Base_onEvent("drag", h);
}

export function on_dragStart_Z3384A56C(h) {
    return Base_onEvent("dragstart", h);
}

export function on_dragEnd_Z3384A56C(h) {
    return Base_onEvent("dragend", h);
}

export function on_dragEnter_Z3384A56C(h) {
    return Base_onEvent("dragenter", h);
}

export function on_dragOver_Z3384A56C(h) {
    return Base_onEvent("dragover", h);
}

export function on_dragLeave_Z3384A56C(h) {
    return Base_onEvent("dragleave", h);
}

export function on_drop_Z3384A56C(h) {
    return Base_onEvent("drop", h);
}

export function on_touchStart_Z2CA827DF(h) {
    return Base_onEvent("touchstart", h);
}

export function on_touchMove_Z2CA827DF(h) {
    return Base_onEvent("touchmove", h);
}

export function on_touchEnd_Z2CA827DF(h) {
    return Base_onEvent("touchend", h);
}

export function on_touchCancel_Z2CA827DF(h) {
    return Base_onEvent("touchcancel", h);
}

export function on_scroll_7DDE0344(h) {
    return Base_onEvent("scroll", h);
}

export function on_load_7DDE0344(h) {
    return Base_onEvent("load", h);
}

export function on_error_7DDE0344(h) {
    return Base_onEvent("error", h);
}

export function on_loadedData_7DDE0344(h) {
    return Base_onEvent("loadeddata", h);
}

export function on_loadedMetadata_7DDE0344(h) {
    return Base_onEvent("loadedmetadata", h);
}

export function on_canPlay_7DDE0344(h) {
    return Base_onEvent("canplay", h);
}

export function on_play_7DDE0344(h) {
    return Base_onEvent("play", h);
}

export function on_pause_7DDE0344(h) {
    return Base_onEvent("pause", h);
}

export function on_ended_7DDE0344(h) {
    return Base_onEvent("ended", h);
}

export function on_timeUpdate_7DDE0344(h) {
    return Base_onEvent("timeupdate", h);
}

export function on_volumeChange_7DDE0344(h) {
    return Base_onEvent("volumechange", h);
}

export function on_seeked_7DDE0344(h) {
    return Base_onEvent("seeked", h);
}

export function on_seeking_7DDE0344(h) {
    return Base_onEvent("seeking", h);
}

export function on_durationChange_7DDE0344(h) {
    return Base_onEvent("durationchange", h);
}

export function on_waiting_7DDE0344(h) {
    return Base_onEvent("waiting", h);
}

export function on_animationStart_7DDE0344(h) {
    return Base_onEvent("animationstart", h);
}

export function on_animationEnd_7DDE0344(h) {
    return Base_onEvent("animationend", h);
}

export function on_animationIteration_7DDE0344(h) {
    return Base_onEvent("animationiteration", h);
}

export function on_transitionEnd_7DDE0344(h) {
    return Base_onEvent("transitionend", h);
}

/**
 * Any event by name.
 */
export function on_event_378D00DF(name, h) {
    return Base_onEvent(name, h);
}

