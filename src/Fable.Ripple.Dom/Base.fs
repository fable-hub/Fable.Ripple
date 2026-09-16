namespace Fable.Ripple.Dom

open System.Collections.Generic
open Browser
open Browser.Types
open Fable.Core.JsInterop
open Fable.Ripple

/// Runtime-distinct marker so `Html.none` stays separable under `[<Erase>]`
/// (a nullary case would erase to null and collide with the function/Node dispatch).
type EmptyMarker() = class end

/// One entry in an element's item list - an attribute, an event, a child node, or
/// nothing. Erased (`[<Fable.Core.Erase>]`): at runtime a `DomItem` IS its payload - a
/// function, a `Node`, or the empty marker - so the three are told apart by `instanceof`
/// with no per-item wrapper allocation, and the case names cost nothing.
[<Fable.Core.Erase>]
type DomItem =
    /// A mutation applied to the element being built: sets an attribute, attaches an
    /// event, splices a fragment, or wires a reactive binding (`each`/`dynamic`).
    | Apply of (Element -> unit)
    /// A DOM node appended as a child - an element, a text node, or a node built elsewhere.
    | Child of Node
    /// Renders nothing; the distinct `EmptyMarker` keeps it separable from `Apply`/`Child`
    | Empty of EmptyMarker

/// Low-level primitives the whole DSL is built from - for writing your own
/// `attr`/`Html`/`on` helpers or binding libraries. Not auto-opened: `open Fable.Ripple.Dom.Base`.
module Base =

    let emptyMarker = EmptyMarker()

    /// The SVG namespace, for `document.createElementNS`.
    let svgNamespace = "http://www.w3.org/2000/svg"

    (*
        Element construction
    *)

    /// Apply one item to an element: attributes/events run, child nodes append.
    let inline internal applyItem (element: Element) (item: DomItem) =
        match item with
        | Apply run -> run element
        | Child node -> element.appendChild node |> ignore
        | Empty _ -> ()

    /// Apply every item to `element`. A direct cons-cell walk (not `for … in list`,
    /// which Fable lowers to an allocating enumerator + try/finally per element).
    let applyItems (element: Element) (items: DomItem list) =
        let mutable rest = items

        while not (List.isEmpty rest) do
            applyItem element (List.head rest)
            rest <- List.tail rest

    /// Create an HTML element, apply every item to it, return it (wrapped as a child).
    let createElement (tag: string) (items: DomItem list) : DomItem =
        let element = document.createElement tag
        applyItems element items
        Child(element :> Node)

    /// Create a namespaced element (SVG), apply every item, return it as a child.
    let createElementNS (ns: string) (tag: string) (items: DomItem list) : DomItem =
        let element = document.createElementNS (ns, tag)
        applyItems element items
        Child(element :> Node)

    /// Realise a root/child item to its element (used by `mount`/`each`/`dynamic`).
    let toElement (item: DomItem) : HTMLElement =
        match item with
        | Child node -> node :?> HTMLElement
        | _ -> failwith "expected an element item"

    (*
        Attributes (setAttribute)
    *)

    /// Static attribute via `setAttribute` - works on HTML and SVG alike.
    let attribute (name: string) (value: string) : DomItem =
        Apply(fun element -> element.setAttribute (name, value))

    /// Reactive attribute driven by an auto-tracked callback.
    let bindAttribute (name: string) (callback: unit -> string) : DomItem =
        Apply(fun element ->
            Signal.effect (fun () -> element.setAttribute (name, callback ())) |> ignore
        )

    /// Reactive attribute driven by a signal.
    let bindAttributeSignal (name: string) (signal: Signal<string>) : DomItem =
        bindAttribute name (fun () -> signal.Value)

    (*
        Boolean (present/absent) attributes
    *)

    let inline private setFlag (element: Element) (name: string) (value: bool) =
        if value then
            element.setAttribute (name, "")
        else
            element.removeAttribute name

    /// Static present/absent boolean attribute (e.g. `required`, `hidden`).
    let booleanAttribute (name: string) (value: bool) : DomItem =
        Apply(fun element -> setFlag element name value)

    /// Reactive present/absent boolean attribute driven by a callback.
    let bindBooleanAttribute (name: string) (callback: unit -> bool) : DomItem =
        Apply(fun element -> Signal.effect (fun () -> setFlag element name (callback ())) |> ignore)

    /// Reactive present/absent boolean attribute driven by a signal.
    let bindBooleanAttributeSignal (name: string) (signal: Signal<bool>) : DomItem =
        bindBooleanAttribute name (fun () -> signal.Value)

    (*
        Live DOM properties (element[name] = value)
    *)

    /// Live DOM *property* (`element[name] = value`), not an attribute - the only way
    /// `value`/`checked` reflect the current state rather than the initial default.
    let property (name: string) (value: obj) : DomItem =
        Apply(fun element -> element?(name) <- value)

    /// Reactive property driven by an auto-tracked callback.
    let bindProperty (name: string) (callback: unit -> 'a) : DomItem =
        Apply(fun element ->
            Signal.effect (fun () -> element?(name) <- box (callback ())) |> ignore
        )

    /// Reactive property driven by a signal.
    let bindPropertySignal (name: string) (signal: Signal<'a>) : DomItem =
        bindProperty name (fun () -> signal.Value)

    /// Attach an event listener; the handler is cast to its concrete event type
    /// (erased) and its writes are auto-batched into one flush per event.
    let onEvent (name: string) (handler: 'e -> unit) : DomItem =
        Apply(fun element ->
            element.addEventListener (
                name,
                fun event -> Signal.batch (fun () -> handler (unbox event))
            )
        )

    /// `classList` token manipulation (not whole-string `setAttribute`), so these
    /// COMPOSE: they add/remove named tokens and leave the base `class` (and any
    /// other source) untouched.
    ///
    /// Code below is optimized for JavaScript output, which explains why it looks strange
    module ClassList =

        /// Run `action` for each non-empty space-separated token in `name`. A name with
        /// no space - the common case - takes the fast path and allocates nothing.
        let inline private iterTokens (name: string) ([<InlineIfLambda>] action: string -> unit) =
            if name.IndexOf ' ' < 0 then
                if name.Length > 0 then
                    action name
            else
                for token in name.Split ' ' do
                    if token.Length > 0 then
                        action token

        let private setToken (element: Element) (enabled: bool) (name: string) =
            iterTokens
                name
                (fun token ->
                    if enabled then
                        element.classList.add token
                    else
                        element.classList.remove token
                )

        /// Run `action` for each enabled token across `pairs`. A cons-cell walk (no list
        /// enumerator) plus the single-token fast path, so the common case is alloc-free.
        let inline private iterEnabled
            (pairs: (string * bool) list)
            ([<InlineIfLambda>] action: string -> unit)
            =
            let mutable rest = pairs

            while not (List.isEmpty rest) do
                let (name, enabled) = List.head rest

                if enabled then
                    iterTokens name action

                rest <- List.tail rest

        /// Static conditional classes: add the enabled tokens (composes with existing).
        let add (pairs: (string * bool) list) : DomItem =
            Apply(fun element -> iterEnabled pairs (fun token -> element.classList.add token))

        /// Reactive conditional classes: diff the enabled token set against the one this
        /// binding last applied, toggling only what changed - other tokens are untouched.
        let bind (callback: unit -> (string * bool) list) : DomItem =
            Apply(fun element ->
                let applied = HashSet<string>()

                Signal.effect (fun () ->
                    let next = HashSet<string>()
                    iterEnabled (callback ()) (fun token -> next.Add token |> ignore)

                    for token in applied do
                        if not (next.Contains token) then
                            element.classList.remove token

                    for token in next do
                        if not (applied.Contains token) then
                            element.classList.add token

                    applied.Clear()
                    applied.UnionWith next
                )
                |> ignore
            )

        /// Toggle a single class token by a static flag.
        let toggle (name: string) (enabled: bool) : DomItem =
            Apply(fun element -> setToken element enabled name)

        /// Toggle a single class token by a reactive flag.
        let bindToggle (name: string) (callback: unit -> bool) : DomItem =
            Apply(fun element ->
                Signal.effect (fun () -> setToken element (callback ()) name) |> ignore
            )
