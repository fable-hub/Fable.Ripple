namespace Fable.Ripple.Dom

open Browser.Types
open Fable.Ripple
open Base

/// HTML attributes and properties. Value-bearing attributes take a plain literal;
/// the *interactive* ones (value, checked, disabled, hidden, class, style, ...)
/// also take a `Signal<'T>` or a `unit -> 'T` thunk so they update in place. Any
/// other attribute can be made reactive through the generic `attr.custom` overloads.
type attr =

    (*
        Class
    *)

    static member className(v: string) : DomItem = attribute "class" v
    static member className(f: unit -> string) : DomItem = bindAttribute "class" f

    static member inline className(s: ^s when ^s: (member get_Value: unit -> string)) : DomItem =
        bindAttribute "class" (fun () -> (^s: (member get_Value: unit -> string) s))

    /// Join a list of class names with spaces.
    static member classes(names: string list) : DomItem =
        attribute "class" (String.concat " " names)

    /// Reactive class list: re-joins whenever the signals it reads change.
    static member classes(f: unit -> string list) : DomItem =
        bindAttribute "class" (fun () -> String.concat " " (f ()))

    /// Conditional classes (clsx / Solid `classList` style): keep each name whose
    /// flag is true. E.g. `[ "btn", true; "active", isActive; "disabled", busy ]`.
    /// Adds the tokens via the `classList` DOM API, so it composes with a base
    /// `class` (and other bindings) instead of overwriting the whole attribute.
    static member classList(pairs: (string * bool) list) : DomItem = ClassList.add pairs

    /// Reactive conditional classes: re-evaluates the flags whenever the signals
    /// they read change, toggling only the tokens that actually changed.
    /// E.g. `attr.classList (fun () -> [ "active", isActive.Value ])`.
    static member classList(f: unit -> (string * bool) list) : DomItem = ClassList.bind f

    /// Add or remove a single class token by a flag - composes with other class
    /// sources (like Sutil `toggleClass` / WebSharper `DynamicClass`).
    static member toggleClass(name: string, on: bool) : DomItem = ClassList.toggle name on

    /// Reactive single-token toggle: flips the class whenever the flag changes.
    static member toggleClass(name: string, cond: unit -> bool) : DomItem =
        ClassList.bindToggle name cond

    /// Single-token toggle driven directly by a `Var`/`Signal<bool>`.
    static member inline toggleClass
        (name: string, cond: ^s when ^s: (member get_Value: unit -> bool))
        : DomItem
        =
        ClassList.bindToggle name (fun () -> (^s: (member get_Value: unit -> bool) cond))

    (*
        Style
    *)
    static member style(v: string) : DomItem = attribute "style" v
    static member style(f: unit -> string) : DomItem = bindAttribute "style" f

    static member inline style(s: ^s when ^s: (member get_Value: unit -> string)) : DomItem =
        bindAttribute "style" (fun () -> (^s: (member get_Value: unit -> string) s))

    /// Build the `style` string from `property, value` pairs.
    static member style(props: (string * string) list) : DomItem =
        attribute "style" (props |> List.map (fun (k, v) -> k + ":" + v) |> String.concat ";")

    (*
        Value (interactive: live DOM property)
    *)
    static member value(v: string) : DomItem = property "value" v
    static member value(v: int) : DomItem = property "value" v
    static member value(v: float) : DomItem = property "value" v
    static member value(f: unit -> string) : DomItem = bindProperty "value" f

    /// Reactive value from any signal source (`Var` or `Signal`, any type, stringified).
    static member inline value(s: ^s when ^s: (member get_Value: unit -> ^a)) : DomItem =
        bindProperty "value" (fun () -> string (^s: (member get_Value: unit -> ^a) s))

    (*
        Checked (interactive)
    *)
    static member checked'(v: bool) : DomItem = property "checked" v
    static member checked'(f: unit -> bool) : DomItem = bindProperty "checked" f

    static member inline checked'(s: ^s when ^s: (member get_Value: unit -> bool)) : DomItem =
        bindProperty "checked" (fun () -> (^s: (member get_Value: unit -> bool) s))

    (*
        Other stateful boolean properties
    *)
    static member disabled(v: bool) : DomItem = property "disabled" v
    static member disabled(f: unit -> bool) : DomItem = bindProperty "disabled" f

    static member inline disabled(s: ^s when ^s: (member get_Value: unit -> bool)) : DomItem =
        bindProperty "disabled" (fun () -> (^s: (member get_Value: unit -> bool) s))

    static member hidden(v: bool) : DomItem = property "hidden" v
    static member hidden(f: unit -> bool) : DomItem = bindProperty "hidden" f

    static member inline hidden(s: ^s when ^s: (member get_Value: unit -> bool)) : DomItem =
        bindProperty "hidden" (fun () -> (^s: (member get_Value: unit -> bool) s))

    static member selected(v: bool) : DomItem = property "selected" v
    static member selected(f: unit -> bool) : DomItem = bindProperty "selected" f

    static member inline selected(s: ^s when ^s: (member get_Value: unit -> bool)) : DomItem =
        bindProperty "selected" (fun () -> (^s: (member get_Value: unit -> bool) s))

    static member readOnly(v: bool) : DomItem = property "readOnly" v
    static member readOnly(f: unit -> bool) : DomItem = bindProperty "readOnly" f

    static member inline readOnly(s: ^s when ^s: (member get_Value: unit -> bool)) : DomItem =
        bindProperty "readOnly" (fun () -> (^s: (member get_Value: unit -> bool) s))

    static member required(v: bool) : DomItem = property "required" v
    static member required(f: unit -> bool) : DomItem = bindProperty "required" f

    static member inline required(s: ^s when ^s: (member get_Value: unit -> bool)) : DomItem =
        bindProperty "required" (fun () -> (^s: (member get_Value: unit -> bool) s))

    static member multiple(v: bool) : DomItem = property "multiple" v
    static member multiple(f: unit -> bool) : DomItem = bindProperty "multiple" f

    static member inline multiple(s: ^s when ^s: (member get_Value: unit -> bool)) : DomItem =
        bindProperty "multiple" (fun () -> (^s: (member get_Value: unit -> bool) s))

    static member isOpen(v: bool) : DomItem = property "open" v
    static member isOpen(f: unit -> bool) : DomItem = bindProperty "open" f

    static member inline isOpen(s: ^s when ^s: (member get_Value: unit -> bool)) : DomItem =
        bindProperty "open" (fun () -> (^s: (member get_Value: unit -> bool) s))

    (*
        Two-way bindings
    *)
    /// Two-way text binding: input follows the var, typing writes back.
    static member bindValue(c: Var<string>) : DomItem =
        Apply(fun e ->
            let inp = e :?> HTMLInputElement
            Signal.effect (fun () -> inp.value <- c.Value) |> ignore
            e.addEventListener ("input", fun _ -> Signal.batch (fun () -> c.Value <- inp.value))
        )

    /// Two-way checkbox binding.
    static member bindChecked(c: Var<bool>) : DomItem =
        Apply(fun e ->
            let inp = e :?> HTMLInputElement
            Signal.effect (fun () -> inp.``checked`` <- c.Value) |> ignore

            e.addEventListener (
                "change",
                fun _ -> Signal.batch (fun () -> c.Value <- inp.``checked``)
            )
        )

    (*
        Identity / globals
    *)
    static member id(v: string) : DomItem = attribute "id" v
    static member title(v: string) : DomItem = attribute "title" v
    static member lang(v: string) : DomItem = attribute "lang" v
    static member dir(v: string) : DomItem = attribute "dir" v
    static member slot(v: string) : DomItem = attribute "slot" v
    static member role(v: string) : DomItem = attribute "role" v
    static member accessKey(v: string) : DomItem = attribute "accesskey" v
    static member tabIndex(v: int) : DomItem = attribute "tabindex" (string v)
    static member inputMode(v: string) : DomItem = attribute "inputmode" v
    static member enterKeyHint(v: string) : DomItem = attribute "enterkeyhint" v
    static member autoCapitalize(v: string) : DomItem = attribute "autocapitalize" v

    static member translate(v: bool) : DomItem =
        attribute
            "translate"
            (if v then
                 "yes"
             else
                 "no")

    static member draggable(v: bool) : DomItem =
        attribute
            "draggable"
            (if v then
                 "true"
             else
                 "false")

    static member spellcheck(v: bool) : DomItem =
        attribute
            "spellcheck"
            (if v then
                 "true"
             else
                 "false")

    static member contentEditable(v: bool) : DomItem =
        attribute
            "contenteditable"
            (if v then
                 "true"
             else
                 "false")

    static member contentEditable(v: string) : DomItem = attribute "contenteditable" v

    (*
        Links / media
    *)
    static member href(v: string) : DomItem = attribute "href" v
    static member hrefLang(v: string) : DomItem = attribute "hreflang" v
    static member target(v: string) : DomItem = attribute "target" v
    static member rel(v: string) : DomItem = attribute "rel" v
    static member download(v: string) : DomItem = attribute "download" v
    static member ping(v: string) : DomItem = attribute "ping" v
    static member referrerPolicy(v: string) : DomItem = attribute "referrerpolicy" v
    static member src(v: string) : DomItem = attribute "src" v
    static member srcset(v: string) : DomItem = attribute "srcset" v
    static member sizes(v: string) : DomItem = attribute "sizes" v
    static member alt(v: string) : DomItem = attribute "alt" v
    static member media(v: string) : DomItem = attribute "media" v
    static member crossOrigin(v: string) : DomItem = attribute "crossorigin" v
    static member decoding(v: string) : DomItem = attribute "decoding" v
    static member loading(v: string) : DomItem = attribute "loading" v
    static member useMap(v: string) : DomItem = attribute "usemap" v
    static member poster(v: string) : DomItem = attribute "poster" v
    static member preload(v: string) : DomItem = attribute "preload" v
    static member kind(v: string) : DomItem = attribute "kind" v
    static member srcLang(v: string) : DomItem = attribute "srclang" v
    static member allow(v: string) : DomItem = attribute "allow" v
    static member sandbox(v: string) : DomItem = attribute "sandbox" v
    static member srcdoc(v: string) : DomItem = attribute "srcdoc" v
    static member coords(v: string) : DomItem = attribute "coords" v
    static member shape(v: string) : DomItem = attribute "shape" v

    (*
        Forms
    *)
    static member name(v: string) : DomItem = attribute "name" v
    static member type'(v: string) : DomItem = attribute "type" v
    static member placeholder(v: string) : DomItem = attribute "placeholder" v
    static member pattern(v: string) : DomItem = attribute "pattern" v
    static member autoComplete(v: string) : DomItem = attribute "autocomplete" v
    static member htmlFor(v: string) : DomItem = attribute "for" v
    static member dirName(v: string) : DomItem = attribute "dirname" v
    static member label(v: string) : DomItem = attribute "label" v
    static member list(v: string) : DomItem = attribute "list" v
    static member accept(v: string) : DomItem = attribute "accept" v
    static member capture(v: string) : DomItem = attribute "capture" v
    static member wrap(v: string) : DomItem = attribute "wrap" v
    static member action(v: string) : DomItem = attribute "action" v
    static member method'(v: string) : DomItem = attribute "method" v
    static member encType(v: string) : DomItem = attribute "enctype" v
    static member acceptCharset(v: string) : DomItem = attribute "accept-charset" v
    static member form(v: string) : DomItem = attribute "form" v
    static member formAction(v: string) : DomItem = attribute "formaction" v
    static member formMethod(v: string) : DomItem = attribute "formmethod" v
    static member formEncType(v: string) : DomItem = attribute "formenctype" v
    static member formTarget(v: string) : DomItem = attribute "formtarget" v
    static member min(v: string) : DomItem = attribute "min" v
    static member min(v: int) : DomItem = attribute "min" (string v)
    static member max(v: string) : DomItem = attribute "max" v
    static member max(v: int) : DomItem = attribute "max" (string v)
    static member step(v: string) : DomItem = attribute "step" v
    static member step(v: int) : DomItem = attribute "step" (string v)
    static member minLength(v: int) : DomItem = attribute "minlength" (string v)
    static member maxLength(v: int) : DomItem = attribute "maxlength" (string v)
    static member size(v: int) : DomItem = attribute "size" (string v)
    static member rows(v: int) : DomItem = attribute "rows" (string v)
    static member cols(v: int) : DomItem = attribute "cols" (string v)
    static member low(v: float) : DomItem = attribute "low" (string v)
    static member high(v: float) : DomItem = attribute "high" (string v)
    static member optimum(v: float) : DomItem = attribute "optimum" (string v)

    (*
        Boolean attributes (present/absent)
    *)
    static member autoFocus(v: bool) : DomItem = booleanAttribute "autofocus" v
    static member autoPlay(v: bool) : DomItem = booleanAttribute "autoplay" v
    static member controls(v: bool) : DomItem = booleanAttribute "controls" v
    static member loop(v: bool) : DomItem = booleanAttribute "loop" v
    static member muted(v: bool) : DomItem = booleanAttribute "muted" v
    static member playsInline(v: bool) : DomItem = booleanAttribute "playsinline" v
    static member reversed(v: bool) : DomItem = booleanAttribute "reversed" v
    static member isDefault(v: bool) : DomItem = booleanAttribute "default" v
    static member isMap(v: bool) : DomItem = booleanAttribute "ismap" v
    static member allowFullScreen(v: bool) : DomItem = booleanAttribute "allowfullscreen" v
    static member noValidate(v: bool) : DomItem = booleanAttribute "novalidate" v
    static member formNoValidate(v: bool) : DomItem = booleanAttribute "formnovalidate" v
    static member async'(v: bool) : DomItem = booleanAttribute "async" v
    static member defer(v: bool) : DomItem = booleanAttribute "defer" v

    (*
        Media / dimensions
    *)
    static member width(v: int) : DomItem = attribute "width" (string v)
    static member width(v: string) : DomItem = attribute "width" v
    static member height(v: int) : DomItem = attribute "height" (string v)
    static member height(v: string) : DomItem = attribute "height" v

    (*
        Tables
    *)
    static member colspan(v: int) : DomItem = attribute "colspan" (string v)
    static member colSpan(v: int) : DomItem = attribute "colspan" (string v)
    static member rowSpan(v: int) : DomItem = attribute "rowspan" (string v)
    static member span(v: int) : DomItem = attribute "span" (string v)
    static member headers(v: string) : DomItem = attribute "headers" v
    static member scope(v: string) : DomItem = attribute "scope" v

    (*
        Lists / misc
    *)
    static member start(v: int) : DomItem = attribute "start" (string v)
    static member cite(v: string) : DomItem = attribute "cite" v
    static member dateTime(v: string) : DomItem = attribute "datetime" v
    static member content(v: string) : DomItem = attribute "content" v
    static member charSet(v: string) : DomItem = attribute "charset" v
    static member httpEquiv(v: string) : DomItem = attribute "http-equiv" v

    (*
        Data-* / aria-*
    *)
    static member data(name: string, v: string) : DomItem = attribute ("data-" + name) v
    static member aria(name: string, v: string) : DomItem = attribute ("aria-" + name) v

    (*
        Generic escape hatches
    *)
    /// Arbitrary attribute (static or reactive), for anything without a named
    /// member above - `aria-*` state, `data-*`, or a bleeding-edge attribute.
    static member custom(name: string, v: string) : DomItem = attribute name v
    static member custom(name: string, c: Signal<string>) : DomItem = bindAttributeSignal name c
    static member custom(name: string, f: unit -> string) : DomItem = bindAttribute name f

    /// Arbitrary live DOM property - sets `element[name]`, not an attribute.
    static member prop(name: string, v: obj) : DomItem = property name v
    static member prop(name: string, f: unit -> obj) : DomItem = bindProperty name f

    /// Escape hatch: run arbitrary code against the element - capture a reference,
    /// focus it, attach an observer, mount a third-party widget. Runs while the
    /// element is being built, inside the enclosing `Signal.root`, so any effect or
    /// `Signal.onCleanup` registered here tears down with the element.
    static member ref(f: HTMLElement -> unit) : DomItem = Apply(fun e -> f (e :?> HTMLElement))
