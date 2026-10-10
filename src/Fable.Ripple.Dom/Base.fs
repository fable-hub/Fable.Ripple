namespace Fable.Ripple.Dom

open System.Collections.Generic
open Glutinum.Web
open type Glutinum.Web.Exports
open Fable.Core
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
        Recording: how `Html.template` learns a row's dynamic parts
    *)

    /// What a recorded part does on the clone of its node.
    type internal RecordedKind =
        /// `Fn` is a `unit -> string` read bound to a text node
        | Text = 0
        /// `Fn` is an `Event -> unit` handler for the event `Name`
        | On = 1
        /// `Fn` is an `Element -> unit` to run against the cloned element
        | Apply = 2
        /// `Fn` is a `Node -> unit` to run with the cloned anchor
        | Splice = 3

    /// One dynamic part of a row skeleton, captured while the row function ran
    /// once: the node it belongs to and what to do on the clone of that node.
    [<Struct>]
    type internal Recorded =
        {
            Node: Node
            Kind: RecordedKind
            Name: string
            Fn: obj
        }

    /// While a template function runs for `Html.template`, the primitives record
    /// their dynamic work against the skeleton instead of doing it. A helper built
    /// on `Base` that creates an effect or a listener of its own takes part with
    /// `isActive` and `markDynamic`.
    module Recording =

        let mutable internal active = false

        /// Set by a primitive that would have created an effect or a listener, so
        /// `applyItems` records the `Apply` it just ran.
        let mutable internal dynamic = false

        /// True while a template function runs. A helper that would create an
        /// effect or a listener must call `markDynamic` instead and return.
        let isActive () = active

        /// Tell the walker to record the current `Apply` and run it again on every
        /// row. Call it, and do nothing else, when `isActive ()` is true.
        let markDynamic () = dynamic <- true

        let mutable private list: ResizeArray<Recorded> = null
        let mutable private deps: ResizeArray<Signal<obj>> = null

        let internal start () =
            let prev = struct (active, list, deps)
            active <- true
            list <- ResizeArray<Recorded>()
            deps <- ResizeArray<Signal<obj>>()
            prev

        /// The recorded parts and the signals the skeleton was built from.
        let internal stop
            (struct (prevActive, prevList, prevDeps))
            : ResizeArray<Recorded> * ResizeArray<Signal<obj>>
            =
            let recorded = list
            let dependencies = deps
            active <- prevActive
            list <- prevList
            deps <- prevDeps
            recorded, dependencies

        /// A signal the skeleton depends on: when it changes, the skeleton is
        /// stale and the list rebuilds. Hot reload uses it for a component's
        /// implementation.
        let internal dependOn (signal: Signal<obj>) = deps.Add signal

        /// A reactive text node to bind per row.
        let internal text (node: Node) (read: unit -> string) =
            list.Add
                {
                    Node = node
                    Kind = RecordedKind.Text
                    Name = ""
                    Fn = box read
                }

        /// An event listener to attach per row.
        let internal on (node: Node) (name: string) (handler: Event -> unit) =
            list.Add
                {
                    Node = node
                    Kind = RecordedKind.On
                    Name = name
                    Fn = box handler
                }

        /// An `Apply` to run per row against the cloned element.
        let internal apply (node: Node) (run: Element -> unit) =
            list.Add
                {
                    Node = node
                    Kind = RecordedKind.Apply
                    Name = ""
                    Fn = box run
                }

        /// A list or dynamic subtree to build per row at the cloned anchor.
        let internal splice (anchor: Node) (run: Node -> unit) =
            list.Add
                {
                    Node = anchor
                    Kind = RecordedKind.Splice
                    Name = ""
                    Fn = box run
                }

    (*
        Element construction
    *)

    /// Apply one item to an element: attributes/events run, child nodes append.
    let inline internal applyItem (element: Element) (item: DomItem) =
        match item with
        | Apply run -> run element
        | Child node -> element.appendChild node |> ignore
        | Empty _ -> ()

    /// `applyItem` while recording: an `Apply` that flagged itself dynamic is
    /// recorded with the element it ran against.
    let internal applyItemRecording (element: Element) (item: DomItem) =
        match item with
        | Apply run ->
            Recording.dynamic <- false
            run element

            if Recording.dynamic then
                Recording.apply element run
                Recording.dynamic <- false
        | Child node -> element.appendChild node |> ignore
        | Empty _ -> ()

    /// Apply every item to `element`. A direct cons-cell walk (not `for … in list`,
    /// which Fable lowers to an allocating enumerator + try/finally per element).
    let applyItems (element: Element) (items: DomItem list) =
        let mutable rest = items

        if Recording.active then
            while not (List.isEmpty rest) do
                applyItemRecording element (List.head rest)
                rest <- List.tail rest
        else
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
        let element = document.createElementNS (Some ns, tag)
        applyItems element items
        Child(element :> Node)

    // Hot reload swaps the element a component rendered, so a list row, a dynamic
    // branch or a mount holding that element to remove later has to be told.

    [<Fable.Core.Emit("$0.__rippleOwner = $1")>]
    let private setOwner (node: Node) (update: obj) : unit = jsNative

    [<Fable.Core.Emit("$0.__rippleOwner")>]
    let private ownerOf (node: Node) : obj = jsNative

    [<Fable.Core.Emit("$0($1)")>]
    let private invokeOwner (update: obj) (node: Node) : unit = jsNative

    /// Record how to repoint whoever holds `node` when it is replaced.
    let internal trackNode (node: Node) (update: Node -> unit) : unit = setOwner node (box update)

    /// Repoint whoever holds `oldNode` at `newNode`. Nodes are tracked only in a
    /// debug build, so this does nothing in a release one.
    let internal replaceTrackedNode (oldNode: Node) (newNode: Node) : unit =
        let update = ownerOf oldNode

        if not (isNull update) then
            invokeOwner update newNode
            setOwner newNode update

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

    /// Reactive attribute driven by an auto-tracked callback. The DOM is written
    /// only when the callback's result differs from the last value written.
    let bindAttribute (name: string) (callback: unit -> string) : DomItem =
        Apply(fun element ->
            if Recording.active then
                Recording.dynamic <- true
            else
                let mutable prev: string = null

                Signal.autorun (fun () ->
                    let v = callback ()

                    if not (obj.ReferenceEquals(v, prev)) then
                        prev <- v
                        element.setAttribute (name, v)
                )

        )

    /// Reactive attribute driven by a signal.
    let bindAttributeSignal (name: string) (signal: Signal<string>) : DomItem =
        bindAttribute name (fun () -> signal.Value)

    (*
        Numeric attributes
    *)

    /// Reactive number-valued attribute. The DOM is written only when the number
    /// differs from the last one written.
    let bindNumberAttribute (name: string) (callback: unit -> float) : DomItem =
        Apply(fun element ->
            if Recording.active then
                Recording.dynamic <- true
            else
                // `nan <> nan`, so the first run always writes.
                let mutable prev = nan

                Signal.autorun (fun () ->
                    let v = callback ()

                    if v <> prev then
                        prev <- v
                        element.setAttribute (name, string v)
                )

        )

    /// Reactive number-valued attribute driven by a signal.
    let bindNumberAttributeSignal (name: string) (signal: Signal<float>) : DomItem =
        bindNumberAttribute name (fun () -> signal.Value)

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
        Apply(fun element ->
            if Recording.active then
                Recording.dynamic <- true
            else
                let mutable prev = false
                let mutable first = true

                Signal.autorun (fun () ->
                    let v = callback ()

                    if first || v <> prev then
                        first <- false
                        prev <- v
                        setFlag element name v
                )

        )

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
            if Recording.active then
                Recording.dynamic <- true
            else
                let mutable prev: obj = null
                let mutable first = true

                Signal.autorun (fun () ->
                    let v = box (callback ())

                    if first || not (obj.ReferenceEquals(v, prev)) then
                        first <- false
                        prev <- v
                        element?(name) <- v
                )

        )

    /// Reactive property driven by a signal.
    let bindPropertySignal (name: string) (signal: Signal<'a>) : DomItem =
        bindProperty name (fun () -> signal.Value)

    /// Attach an event listener; the handler is cast to its concrete event type
    /// (erased) and its writes are auto-batched into one flush per event.
    /// Attach a listener whose handler runs batched, and under the row context
    /// the listener was created in, if any.
    let listen (element: Node) (name: string) (handler: Event -> unit) =
        let context = Signal.context ()

        element.addEventListener (
            name,
            Some(fun event ->
                if isNull context then
                    Signal.batch (fun () -> handler event)
                else
                    Signal.withContext context (fun () -> Signal.batch (fun () -> handler event))
            )
        )

    let onEvent (name: string) (handler: 'e -> unit) : DomItem =
        Apply(fun element ->
            if Recording.active then
                Recording.on element name (unbox handler)
            else
                listen element name (unbox handler)
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
                if Recording.active then
                    Recording.dynamic <- true
                else
                    let applied = HashSet<string>()

                    Signal.autorun (fun () ->
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

            )

        /// Toggle a single class token by a static flag.
        let toggle (name: string) (enabled: bool) : DomItem =
            Apply(fun element -> setToken element enabled name)

        /// Toggle a single class token by a reactive flag.
        let bindToggle (name: string) (callback: unit -> bool) : DomItem =
            Apply(fun element ->
                if Recording.active then
                    Recording.dynamic <- true
                else
                    let mutable prev = false
                    let mutable first = true

                    Signal.autorun (fun () ->
                        let v = callback ()

                        if first || v <> prev then
                            first <- false
                            prev <- v
                            setToken element v name
                    )

            )
