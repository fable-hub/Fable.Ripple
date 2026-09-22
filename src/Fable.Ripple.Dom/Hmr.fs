namespace Fable.Ripple.Dom

open System
open System.Collections.Generic
open Browser
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fable.Ripple
open Fable.Ripple.Dom.Base

/// The runtime behind `[<Component>]`. Called only from code the plugin emits, and
/// the plugin emits none of it outside a Debug build.
module Hmr =

    let private key (url: string) (name: string) =
        let url =
            match url.IndexOf "?" with
            | -1 -> url
            | i -> url.Substring(0, i)

        url + "#" + name

    /// On globalThis, not in this module: this module can itself be hot-replaced.
    [<Emit("(globalThis.__RIPPLE_HMR__ || (globalThis.__RIPPLE_HMR__ = new Map()))")>]
    let private store: Dictionary<string, Var<obj>> = jsNative

    // --- state carried across a swap --------------------------------------------

    let private stash = Dictionary<string, Dictionary<string, obj>>()

    /// Where the boundary being built sits, and the tally for the ones it builds.
    type private Frame =
        {
            Path: string
            Counts: Dictionary<string, int>
            Seen: HashSet<string>
        }

    let private newFrame (path: string) =
        {
            Path = path
            Counts = Dictionary()
            Seen = HashSet()
        }

    let mutable private frame = newFrame ""

    /// The place a boundary takes among its siblings, fixed when it is built.
    ///
    /// Counted per component, so instances of one do not renumber instances of another.
    let private place (k: string) =
        let n =
            match frame.Counts.TryGetValue k with
            | true, n -> n
            | _ -> 0

        frame.Counts.[k] <- n + 1
        frame.Path + "/" + k + "#" + string n

    let private enter (path: string) =
        let saved = frame
        frame <- newFrame path
        saved

    let private leave (saved: Frame) =
        match stash.TryGetValue frame.Path with
        | true, bag ->
            let stale = bag.Keys |> Seq.filter (frame.Seen.Contains >> not) |> Seq.toArray

            for name in stale do
                bag.Remove name |> ignore
        | _ -> ()

        frame <- saved

    /// Keep a `Var` across a swap, keyed by the name it was bound to.
    ///
    /// `create` runs only the first time. A type change between versions falls back to
    /// a fresh Var rather than throwing.
    let adopt (name: string) (create: unit -> Var<'T>) : Var<'T> =
        frame.Seen.Add name |> ignore

        let bag =
            match stash.TryGetValue frame.Path with
            | true, bag -> bag
            | _ ->
                let bag = Dictionary<string, obj>()
                stash.[frame.Path] <- bag
                bag

        match bag.TryGetValue name with
        | true, v ->
            match v with
            | :? Var<'T> as typed -> typed
            | _ ->
                let fresh = create ()
                bag.[name] <- box fresh
                fresh
        | _ ->
            let fresh = create ()
            bag.[name] <- box fresh
            fresh

    // --- the boundary ------------------------------------------------------------

    [<Emit("(f => (f.__splice = true, f))((p, a) => $0(p, a))")>]
    let private spliceFn (run: Node -> Node -> unit) : Element -> unit = jsNative

    let splice (run: Node -> Node -> unit) : DomItem = Apply(spliceFn run)

    [<Emit("$0 != null && $0.__splice === true")>]
    let private isSplice (item: DomItem) : bool = jsNative

    [<Emit("$0($1, $2)")>]
    let private runSplice (item: DomItem) (parent: Node) (anchor: Node) : unit = jsNative

    [<Emit("globalThis.__rebuilds = (globalThis.__rebuilds || 0) + 1")>]
    let private countRebuild () : unit = jsNative

    [<Emit("performance.now()")>]
    let private now () : float = jsNative

    [<Emit("(globalThis.__rebuildMs = globalThis.__rebuildMs || []).push($0)")>]
    let private recordRebuild (ms: float) : unit = jsNative

    let fragment (items: DomItem list) : DomItem =
        splice (fun parent anchor ->
            let mutable rest = items

            while not (List.isEmpty rest) do
                match List.head rest with
                | Child node -> parent.insertBefore (node, anchor) |> ignore
                | item -> applyItem (parent :?> Element) item

                rest <- List.tail rest
        )

    [<Emit("$0.apply(null, $1)")>]
    let private applyArgs (fn: obj) (args: obj[]) : DomItem = jsNative

    // The focused element is located by its position under `root`, so an edit that
    // changes the shape around it loses focus.

    let private capture (root: Node) =
        let active = document.activeElement

        if isNull (box active) || isNull (box root) || not (root.contains active) then
            None
        else
            let rec pathTo (n: Node) acc =
                if obj.ReferenceEquals(n, root) then
                    Some acc
                else
                    match n.parentNode with
                    | null -> None
                    | p ->
                        let mutable i = 0
                        let mutable c = p.firstChild

                        while not (isNull c) && not (obj.ReferenceEquals(c, n)) do
                            i <- i + 1
                            c <- c.nextSibling

                        pathTo p (i :: acc)

            pathTo (active :> Node) []
            |> Option.map (fun p -> p, active?selectionStart, active?selectionEnd)

    let private restore (root: Node) (captured: (int list * obj * obj) option) =
        captured
        |> Option.iter (fun (path, selStart, selEnd) ->
            let mutable node = root
            let mutable ok = not (isNull (box root))

            for i in path do
                if ok then
                    let children = node.childNodes

                    if i < children.length then
                        node <- children.item i
                    else
                        ok <- false

            if ok && not (isNull (box node)) then
                let el: HTMLElement = unbox node

                if not (isNull (box el?focus)) then
                    el.focus ()

                    if not (isNull selStart) then
                        try
                            el?setSelectionRange (selStart, selEnd)
                        with _ ->
                            ()
        )

    let private boundaryOf (k: string) (impl: Var<obj>) (args: obj[]) : DomItem =
        let path = place k

        let build (f: obj) =
            Signal.root (fun () ->
                let saved = enter path
                let item = Signal.untracked (fun () -> applyArgs f args)
                leave saved
                item
            )

        let firstItem, firstDispose = build impl.Value

        match firstItem with
        // A component that renders one element is that element, so it can sit anywhere
        // a `DomItem` can - an `Html.each` row included, which needs one node per key.
        | Child node ->
            let mutable current = node
            let mutable dispose = firstDispose
            let mutable built = true

            Signal.effect (fun () ->
                let f = impl.Value

                if built then
                    built <- false
                else
                    countRebuild ()
                    let t0 = now ()
                    let parent = current.parentNode
                    let captured = capture current
                    dispose.Dispose()

                    let next, d = build f
                    dispose <- d

                    match next with
                    | Child fresh ->
                        if not (isNull parent) then
                            parent.replaceChild (fresh, current) |> ignore
                            replaceTrackedNode current fresh

                        current <- fresh
                        restore current captured
                    | _ -> ()

                    recordRebuild (now () - t0)
            )
            |> ignore

            Signal.onCleanup (fun () -> dispose.Dispose())
            Child current

        // A fragment, bare attributes or nothing at all has no single node to stand
        // for it, so it is rebuilt between anchors in its parent.
        | _ ->
            firstDispose.Dispose()

            Apply(fun parent ->
                let startA = document.createComment "b[" :> Node
                let endA = document.createComment "]b" :> Node
                parent.appendChild startA |> ignore
                parent.appendChild endA |> ignore

                let mutable current: IDisposable option = None

                let clear () =
                    current |> Option.iter (fun d -> d.Dispose())
                    current <- None
                    let mutable n = startA.nextSibling

                    while not (isNull n) && not (obj.ReferenceEquals(n, endA)) do
                        let next = n.nextSibling
                        parent.removeChild n |> ignore
                        n <- next

                Signal.effect (fun () ->
                    let f = impl.Value
                    countRebuild ()
                    let captured = capture parent
                    let t0 = now ()
                    clear ()

                    let item, dispose = build f

                    match item with
                    | Child node -> parent.insertBefore (node, endA) |> ignore
                    | Empty _ -> ()
                    | Apply run ->
                        if isSplice item then
                            runSplice item parent endA
                        else
                            run parent

                    current <- Some dispose

                    restore parent captured
                    recordRebuild (now () - t0)
                )
                |> ignore

                Signal.onCleanup clear
            )

    /// Emitted by the plugin in place of the component's own body.
    ///
    /// Always swaps: a re-executed module closes over the new imports, so keeping the
    /// old implementation serves stale values. The wrapper is variadic, so the boundary
    /// can re-invoke with the arguments the call site passed.
    [<Emit("(...a) => $0($1, $2, a)")>]
    let private variadic
        (build: string -> Var<obj> -> obj[] -> DomItem)
        (k: string)
        (impl: Var<obj>)
        : obj
        =
        jsNative

    let define (url: string) (name: string) (fn: obj) : obj =
        let k = key url name

        let impl =
            match store.TryGetValue k with
            | true, v ->
                v.Value <- fn
                v
            | _ ->
                let v = Var.create fn
                store.[k] <- v
                v

        variadic boundaryOf k impl
