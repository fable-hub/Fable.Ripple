namespace Fable.Ripple.Dom

open Glutinum.Web
open Fable.Core
open Fable.Core.JsInterop
open Fable.Ripple
open Base

/// The row handed to an `Html.template` render function: `Value` is the row being
/// built or updated. Read it inside a binding, never at the top level of the
/// render function, which runs once, and not after a callback has returned.
type internal RowSignal<'a>() =
    interface Signal<'a> with
        member _.Value =
            let row = Signal.context ()

            if isNull row then
                failwith (
                    "Html.template: the row was read with no row active. "
                    + "At the top level of the render function, wrap the read in a binding: Html.text (fun () -> ...). "
                    + "In code that runs later (a timer, a promise, an observer), read the row into a local first: let item = row.Value."
                )

            unbox row

        member this.Peek() = (this :> Signal<'a>).Value

/// Running a template function once, then cloning its skeleton per row.
module internal Template =

    let private opDown = 0
    let private opNext = 1
    let private opUp = 2

    /// The child indexes leading from `root` to `node`.
    let private pathOf (root: Node) (node: Node) : ResizeArray<int> =
        let path = ResizeArray<int>()
        let mutable n = node

        while not (obj.ReferenceEquals(n, root)) do
            let mutable index = 0
            let mutable s = n.previousSibling

            while s.IsSome do
                index <- index + 1
                s <- s.Value.previousSibling

            path.Add index
            n <- n.parentNode.Value

        path.Reverse()
        path

    /// Document order: a prefix sorts before what it prefixes.
    let private comparePaths (a: ResizeArray<int>) (b: ResizeArray<int>) =
        let n = min a.Count b.Count
        let mutable i = 0
        let mutable result = 0

        while result = 0 && i < n do
            result <- compare a.[i] b.[i]
            i <- i + 1

        if result <> 0 then
            result
        else
            compare a.Count b.Count

    /// Turn the recorded parts into a plan: hops between consecutive nodes in
    /// document order, and a slot for each part. Bindings are reordered to match.
    let private planOf
        (root: Node)
        (recorded: ResizeArray<Recorded>)
        : ResizeArray<int> * ResizeArray<Recorded>
        =
        let paths = Array.init recorded.Count (fun i -> pathOf root recorded.[i].Node)
        let order = Array.init recorded.Count id

        Array.sortInPlaceWith
            (fun i j ->
                match comparePaths paths.[i] paths.[j] with
                | 0 -> compare i j
                | c -> c
            )
            order

        let plan = ResizeArray<int>()
        let bindings = ResizeArray<Recorded>()
        let mutable at = ResizeArray<int>()

        for k in order do
            let target = paths.[k]

            let mutable common = 0

            while common < at.Count && common < target.Count && at.[common] = target.[common] do
                common <- common + 1

            let mutable depth = common

            if at.Count > common then
                for _ in 1 .. at.Count - common - 1 do
                    plan.Add opUp

                for _ in 1 .. target.[common] - at.[common] do
                    plan.Add opNext

                depth <- common + 1

            while depth < target.Count do
                plan.Add opDown

                for _ in 1 .. target.[depth] do
                    plan.Add opNext

                depth <- depth + 1

            plan.Add(3 + bindings.Count)
            bindings.Add recorded.[k]
            at <- target

        plan, bindings

    [<Emit("$0.cloneNode(true)")>]
    let private clone (node: Node) : Node = jsNative

    [<Emit("$0[$1]")>]
    let private opAt (plan: ResizeArray<int>) (i: int) : int = jsNative

    [<Emit("$0[$1]")>]
    let private bindingAt (bindings: ResizeArray<Recorded>) (i: int) : Recorded = jsNative

    let private apply (binding: Recorded) (node: Node) (row: obj) =
        match binding.Kind with
        | RecordedKind.Text ->
            let read: unit -> string = unbox binding.Fn
            let mutable prev: string = null

            Signal.autorun (fun () ->
                let v = read ()

                if not (obj.ReferenceEquals(v, prev)) then
                    prev <- v
                    node.nodeValue <- Some v
            )
        | RecordedKind.On -> listen node binding.Name (unbox binding.Fn)
        | RecordedKind.Apply ->
            let run: Element -> unit = unbox binding.Fn
            run (node :?> Element)
        | RecordedKind.Splice ->
            let run: Node -> unit = unbox binding.Fn
            run node
        | _ -> ()

    /// Clone the skeleton and replay the plan for `row`, under `row` as context.
    let realise
        (proto: Node)
        (plan: ResizeArray<int>)
        (bindings: ResizeArray<Recorded>)
        (row: 'a)
        : HTMLElement
        =
        let root = clone proto
        let mutable node = root
        let mutable i = 0
        let context = box row
        let saved = Signal.context ()
        Signal.setContext context

        try
            // The plan only walks hops the skeleton has, so every one of these is Some.
            while i < plan.Count do
                let op = opAt plan i

                if op = opDown then
                    node <- !!node.firstChild
                elif op = opNext then
                    node <- !!node.nextSibling
                elif op = opUp then
                    node <- !!node.parentNode
                else
                    apply (bindingAt bindings (op - 3)) node context

                i <- i + 1
        finally
            Signal.setContext saved

        root :?> HTMLElement

    /// Run `render` once against a `RowSignal`, recording the dynamic parts, and
    /// return a row builder that clones the result, with the signals the skeleton
    /// was built from.
    let compile (render: Signal<'a> -> DomItem) : ('a -> HTMLElement) * ResizeArray<Signal<obj>> =
        let prev = Recording.start ()
        let mutable recorded: ResizeArray<Recorded> = null
        let mutable dependencies: ResizeArray<Signal<obj>> = null

        let proto =
            try
                toElement (render (RowSignal<'a>()))
            finally
                let r, d = Recording.stop prev
                recorded <- r
                dependencies <- d

        let plan, bindings = planOf proto recorded
        (fun row -> realise proto plan bindings row), dependencies
