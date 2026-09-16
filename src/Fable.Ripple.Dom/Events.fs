namespace Fable.Ripple.Dom

open Browser.Types
open Fable.Ripple
open Base

/// DOM events, read as `on.click`, `on.input`, `on.keyDown`, ... Every handler's
/// writes are auto-batched, so one user gesture is a single flush no matter how
/// many signals it touches. Handlers receive the concrete event type.
type on =

    (*
        Mouse
    *)
    static member click(h: MouseEvent -> unit) : DomItem = onEvent "click" h
    static member dblClick(h: MouseEvent -> unit) : DomItem = onEvent "dblclick" h
    static member auxClick(h: MouseEvent -> unit) : DomItem = onEvent "auxclick" h
    static member contextMenu(h: MouseEvent -> unit) : DomItem = onEvent "contextmenu" h
    static member mouseDown(h: MouseEvent -> unit) : DomItem = onEvent "mousedown" h
    static member mouseUp(h: MouseEvent -> unit) : DomItem = onEvent "mouseup" h
    static member mouseEnter(h: MouseEvent -> unit) : DomItem = onEvent "mouseenter" h
    static member mouseLeave(h: MouseEvent -> unit) : DomItem = onEvent "mouseleave" h
    static member mouseMove(h: MouseEvent -> unit) : DomItem = onEvent "mousemove" h
    static member mouseOver(h: MouseEvent -> unit) : DomItem = onEvent "mouseover" h
    static member mouseOut(h: MouseEvent -> unit) : DomItem = onEvent "mouseout" h

    (*
        Pointer
    *)
    static member pointerDown(h: PointerEvent -> unit) : DomItem = onEvent "pointerdown" h
    static member pointerUp(h: PointerEvent -> unit) : DomItem = onEvent "pointerup" h
    static member pointerMove(h: PointerEvent -> unit) : DomItem = onEvent "pointermove" h
    static member pointerEnter(h: PointerEvent -> unit) : DomItem = onEvent "pointerenter" h
    static member pointerLeave(h: PointerEvent -> unit) : DomItem = onEvent "pointerleave" h
    static member pointerOver(h: PointerEvent -> unit) : DomItem = onEvent "pointerover" h
    static member pointerOut(h: PointerEvent -> unit) : DomItem = onEvent "pointerout" h
    static member pointerCancel(h: PointerEvent -> unit) : DomItem = onEvent "pointercancel" h

    (*
        Keyboard
    *)
    static member keyDown(h: KeyboardEvent -> unit) : DomItem = onEvent "keydown" h
    static member keyUp(h: KeyboardEvent -> unit) : DomItem = onEvent "keyup" h
    static member keyPress(h: KeyboardEvent -> unit) : DomItem = onEvent "keypress" h

    (*
        Focus
    *)
    static member focus(h: FocusEvent -> unit) : DomItem = onEvent "focus" h
    static member blur(h: FocusEvent -> unit) : DomItem = onEvent "blur" h
    static member focusIn(h: FocusEvent -> unit) : DomItem = onEvent "focusin" h
    static member focusOut(h: FocusEvent -> unit) : DomItem = onEvent "focusout" h

    (*
        Wheel
    *)
    static member wheel(h: WheelEvent -> unit) : DomItem = onEvent "wheel" h

    (*
        Form / input
    *)
    /// Fires on each keystroke; hands the input's current text.
    static member input(h: string -> unit) : DomItem =
        Apply(fun e ->
            e.addEventListener (
                "input",
                fun ev -> Signal.batch (fun () -> h ((ev.target :?> HTMLInputElement).value))
            )
        )

    /// Raw `input` event.
    static member input(h: Event -> unit) : DomItem = onEvent "input" h

    /// Fires on commit (blur/enter); hands the input's current text.
    static member change(h: string -> unit) : DomItem =
        Apply(fun e ->
            e.addEventListener (
                "change",
                fun ev -> Signal.batch (fun () -> h ((ev.target :?> HTMLInputElement).value))
            )
        )

    /// Checkbox/radio commit; hands the `checked` state.
    static member checkedChange(h: bool -> unit) : DomItem =
        Apply(fun e ->
            e.addEventListener (
                "change",
                fun ev -> Signal.batch (fun () -> h ((ev.target :?> HTMLInputElement).``checked``))
            )
        )

    static member submit(h: Event -> unit) : DomItem = onEvent "submit" h
    static member reset(h: Event -> unit) : DomItem = onEvent "reset" h
    static member invalid(h: Event -> unit) : DomItem = onEvent "invalid" h
    static member select(h: Event -> unit) : DomItem = onEvent "select" h

    (*
        Clipboard
    *)
    static member copy(h: ClipboardEvent -> unit) : DomItem = onEvent "copy" h
    static member cut(h: ClipboardEvent -> unit) : DomItem = onEvent "cut" h
    static member paste(h: ClipboardEvent -> unit) : DomItem = onEvent "paste" h

    (*
        Drag
    *)
    static member drag(h: DragEvent -> unit) : DomItem = onEvent "drag" h
    static member dragStart(h: DragEvent -> unit) : DomItem = onEvent "dragstart" h
    static member dragEnd(h: DragEvent -> unit) : DomItem = onEvent "dragend" h
    static member dragEnter(h: DragEvent -> unit) : DomItem = onEvent "dragenter" h
    static member dragOver(h: DragEvent -> unit) : DomItem = onEvent "dragover" h
    static member dragLeave(h: DragEvent -> unit) : DomItem = onEvent "dragleave" h
    static member drop(h: DragEvent -> unit) : DomItem = onEvent "drop" h

    (*
        Touch
    *)
    static member touchStart(h: TouchEvent -> unit) : DomItem = onEvent "touchstart" h
    static member touchMove(h: TouchEvent -> unit) : DomItem = onEvent "touchmove" h
    static member touchEnd(h: TouchEvent -> unit) : DomItem = onEvent "touchend" h
    static member touchCancel(h: TouchEvent -> unit) : DomItem = onEvent "touchcancel" h

    (*
        Scroll / media
    *)
    static member scroll(h: Event -> unit) : DomItem = onEvent "scroll" h
    static member load(h: Event -> unit) : DomItem = onEvent "load" h
    static member error(h: Event -> unit) : DomItem = onEvent "error" h
    static member loadedData(h: Event -> unit) : DomItem = onEvent "loadeddata" h
    static member loadedMetadata(h: Event -> unit) : DomItem = onEvent "loadedmetadata" h
    static member canPlay(h: Event -> unit) : DomItem = onEvent "canplay" h
    static member play(h: Event -> unit) : DomItem = onEvent "play" h
    static member pause(h: Event -> unit) : DomItem = onEvent "pause" h
    static member ended(h: Event -> unit) : DomItem = onEvent "ended" h
    static member timeUpdate(h: Event -> unit) : DomItem = onEvent "timeupdate" h
    static member volumeChange(h: Event -> unit) : DomItem = onEvent "volumechange" h
    static member seeked(h: Event -> unit) : DomItem = onEvent "seeked" h
    static member seeking(h: Event -> unit) : DomItem = onEvent "seeking" h
    static member durationChange(h: Event -> unit) : DomItem = onEvent "durationchange" h
    static member waiting(h: Event -> unit) : DomItem = onEvent "waiting" h

    (*
        Animation / transition
    *)
    static member animationStart(h: Event -> unit) : DomItem = onEvent "animationstart" h
    static member animationEnd(h: Event -> unit) : DomItem = onEvent "animationend" h

    static member animationIteration(h: Event -> unit) : DomItem = onEvent "animationiteration" h

    static member transitionEnd(h: Event -> unit) : DomItem = onEvent "transitionend" h

    /// Any event by name.
    static member event(name: string, h: Event -> unit) : DomItem = onEvent name h
