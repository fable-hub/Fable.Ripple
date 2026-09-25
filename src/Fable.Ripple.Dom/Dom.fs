namespace Fable.Ripple.Dom

open System
open System.Collections.Generic
open Browser
open Browser.Types
open Fable.Core
open Fable.Ripple

/// Low-level DOM binding built directly on the signal core - no intermediate
/// VNode. Reactive bindings are `Signal.effect`s; list rows and dynamic subtrees
/// own a `Signal.root` scope so their effects/computeds tear down on removal.
module Dom =

    let el (tag: string) : HTMLElement = document.createElement tag

    /// Static text child.
    let text (parent: Node) (s: string) =
        parent.appendChild (document.createTextNode s) |> ignore

    /// Reactive text child: re-runs only when a signal it reads changes.
    let bindText (parent: Node) (f: unit -> string) =
        let t = document.createTextNode ""
        parent.appendChild t |> ignore
        Signal.effect (fun () -> t.nodeValue <- f ()) |> ignore

    /// Reactive attribute.
    let bindAttr (e: HTMLElement) (name: string) (f: unit -> string) =
        Signal.effect (fun () -> e.setAttribute (name, f ())) |> ignore

    (*
        Keyed list reconciliation (Solid `<For>` model): the list is driven by a
        signal of items; on change we diff by key and create / remove / move DOM,
        reusing the element (and its per-row scope) for surviving keys. A row that
        only mutates its own signals never triggers this - only its bound effects.
    *)

    /// One rendered list row: its element, the scope owning its effects, and the
    /// bookkeeping the diff needs.
    type private Row =
        {
            mutable Node: HTMLElement
            Dispose: IDisposable
            /// Pass number in which this row was last claimed; rows left unstamped
            /// after a diff are the removals (replaces a per-pass HashSet).
            mutable Seen: int
            /// Current position in `order`/`rows`. Always valid - set at creation and
            /// refreshed whenever positions shift - so a reorder can recover where a
            /// row came from without touching the DOM.
            mutable Index: int
        }

    /// Mark the longest increasing subsequence of `src` in `keep` (values < 0 are
    /// newly created rows and can never take part). Rows in the LIS already sit in
    /// the right relative order, so a reorder only has to move everything else -
    /// which is what lets the placement pass skip the DOM entirely for most rows.
    let private markLis (src: int[]) (keep: bool[]) =
        let n = src.Length
        let tails = Array.zeroCreate<int> n // tails.[l] = index of the smallest tail of a length-(l+1) run
        let prev = Array.zeroCreate<int> n
        let mutable len = 0

        for i in 0 .. n - 1 do
            let v = src.[i]

            if v >= 0 then
                let mutable lo = 0
                let mutable hi = len

                while lo < hi do
                    let mid = (lo + hi) / 2

                    if src.[tails.[mid]] < v then
                        lo <- mid + 1
                    else
                        hi <- mid

                prev.[i] <-
                    if lo > 0 then
                        tails.[lo - 1]
                    else
                        -1

                tails.[lo] <- i

                if lo = len then
                    len <- len + 1

        if len > 0 then
            let mutable k = tails.[len - 1]

            while k >= 0 do
                keep.[k] <- true
                k <- prev.[k]

    /// Render `items` into `parent` (rows placed before `anchor`, which may be
    /// null to append). Each row's `render` runs in its own `Signal.root`, disposed
    /// when the row leaves or the enclosing scope tears down.
    let keyedEach
        (parent: Node)
        (anchor: Node)
        (getItems: unit -> 'a[])
        (keyOf: 'a -> 'k)
        (render: 'a -> HTMLElement)
        : IDisposable
        =
        // Key -> row, needed only to resolve a key that moved. The append fast path
        // never looks anything up, so the index is built on demand the first time a
        // real diff runs (one Dictionary insert per row is ~4% of a mount) and kept
        // current from then on.
        let byKey = Dictionary<'k, Row>(HashIdentity.Structural)
        let mutable indexed = false
        let mutable order: 'k[] = [||]
        // Rows parallel to `order`, so the diff can reach a row by index instead
        // of hashing its key.
        let mutable rows: Row[] = [||]
        let mutable generation = 0

        let make (item: 'a) (index: int) : Row =
            let node, dispose = Signal.root (fun () -> render item)

            let row =
                {
                    Node = node
                    Dispose = dispose
                    Seen = 0
                    Index = index
                }

#if DEBUG
            Base.trackNode node (fun fresh -> row.Node <- unbox fresh)
#endif

            row

        /// Fill `byKey` from the current rows. Called only when a diff actually
        /// needs key lookups; a mount that only ever appends never pays for it.
        let reindex () =
            if not indexed then
                for i in 0 .. order.Length - 1 do
                    byKey.[order.[i]] <- rows.[i]

                indexed <- true

        // Is the previous order a prefix of the new keys (a pure append / a fresh
        // build from empty)? Then we can skip the full diff entirely.
        let isAppendOf (newKeys: 'k[]) =
            if order.Length > newKeys.Length then
                false
            else
                let mutable ok = true
                let mutable i = 0

                while ok && i < order.Length do
                    if not (obj.Equals(order.[i], newKeys.[i])) then
                        ok <- false

                    i <- i + 1

                ok

        // Are the new keys disjoint from the current ones? Builds the key index
        // (reused by the diff if we fall through). Early-exits on the first shared
        // key, so a reorder/edit - which shares its ends - pays almost nothing.
        let isDisjoint (newKeys: 'k[]) =
            reindex ()
            let mutable dj = true
            let mutable i = 0

            while dj && i < newKeys.Length do
                if byKey.ContainsKey newKeys.[i] then
                    dj <- false

                i <- i + 1

            dj

        // A Range delete over the (always contiguous) row span was measured
        // slower than both branches.
        let removeAllRowNodes () =
            let hasAnchor = not (obj.ReferenceEquals(anchor, null))

            let owned =
                if hasAnchor then
                    order.Length + 1
                else
                    order.Length

            if int parent.childNodes.length = owned then
                parent.textContent <- ""

                if hasAnchor then
                    parent.appendChild anchor |> ignore
            else
                for i in 0 .. rows.Length - 1 do
                    parent.removeChild rows.[i].Node |> ignore

        let reconcile (items: 'a[]) =
            let n = items.Length
            let newKeys = items |> Array.map keyOf

            // The general middle diff (prefix/suffix already trimmed), factored out
            // so the fast paths can bypass it entirely.
            let diffMiddle (start: int) (endOld: int) (endNew: int) =
                let newRows = Array.zeroCreate<Row> n

                for i in 0 .. start - 1 do
                    newRows.[i] <- rows.[i]

                let mutable oi = order.Length - 1
                let mutable ni = n - 1

                while ni > endNew do
                    newRows.[ni] <- rows.[oi]
                    oi <- oi - 1
                    ni <- ni - 1

                // Claim (or create) each row of the new middle, stamping it so the
                // removal pass below can spot the rows nobody claimed.
                generation <- generation + 1
                let count = endNew - start + 1

                // Old position of the row now at each new middle slot (-1 = created
                // this pass), the input to the LIS below.
                let srcIdx =
                    if count > 0 then
                        Array.zeroCreate<int> count
                    else
                        [||]

                for i in start..endNew do
                    let k = newKeys.[i]

                    // Same key still at this index - the bulk of a swap or an
                    // in-place edit - so reuse the row without hashing.
                    if i <= endOld && obj.Equals(order.[i], k) then
                        let e = rows.[i]
                        e.Seen <- generation
                        newRows.[i] <- e
                        srcIdx.[i - start] <- i
                    else
                        // First key that actually has to be looked up - only now is
                        // the index worth building. A pure removal, or an edit
                        // confined to the ends, never reaches here at all.
                        reindex ()

                        match byKey.TryGetValue k with
                        | true, e ->
                            e.Seen <- generation
                            newRows.[i] <- e
                            srcIdx.[i - start] <- e.Index
                        | _ ->
                            let e = make items.[i] i
                            e.Seen <- generation
                            byKey.[k] <- e
                            newRows.[i] <- e
                            srcIdx.[i - start] <- -1

                // Removals can only come from the old middle: prefix/suffix keys are
                // identical on both sides, and keys are unique. Reached by index via
                // `rows`, so this costs no hashing beyond the keys actually dropped.
                for i in start..endOld do
                    let e = rows.[i]

                    if e.Seen <> generation then
                        e.Dispose.Dispose()
                        parent.removeChild e.Node |> ignore

                        // Nothing to unindex if the index was never built.
                        if indexed then
                            byKey.Remove order.[i] |> ignore

                // Rows on the longest increasing subsequence of old positions are
                // already correctly ordered relative to each other, so only the rest
                // move. This is what removes the per-row `parentNode`/`nextSibling`
                // probes: a distant swap now touches the DOM twice, not 1000 times.
                let keep =
                    if count > 0 then
                        Array.zeroCreate<bool> count
                    else
                        [||]

                if count > 0 then
                    markLis srcIdx keep

                // Place only the middle, back-to-front. Kept rows still advance
                // `nextSibling`, since they are the reference for the row to their left.
                let mutable nextSibling =
                    if endNew + 1 < n then
                        newRows.[endNew + 1].Node :> Node
                    else
                        anchor

                for i in endNew .. -1 .. start do
                    let node = newRows.[i].Node

                    if not keep.[i - start] then
                        parent.insertBefore (node, nextSibling) |> ignore

                    nextSibling <- node

                order <- newKeys
                rows <- newRows

                // Indices shift from `start` on (the prefix keeps its own).
                for i in start .. n - 1 do
                    newRows.[i].Index <- i

            // Fast path: clearing the whole list. Every row scope still has to be
            // disposed, but the nodes can go in a single DOM operation when the list
            // owns the parent (no sibling content besides its anchor) instead of
            // N removeChild calls.
            if n = 0 then
                if order.Length > 0 then
                    for i in 0 .. rows.Length - 1 do
                        rows.[i].Dispose.Dispose()

                    removeAllRowNodes ()
                    byKey.Clear()
                    indexed <- false
                    order <- [||]
                    rows <- [||]

            // Fast path: pure append (incl. first build). Create only the new tail
            // and insert it in one DocumentFragment - O(added), not O(list).
            else if isAppendOf newKeys then
                if n > order.Length then
                    let frag = document.createDocumentFragment ()
                    let newRows = Array.zeroCreate<Row> n

                    for i in 0 .. order.Length - 1 do
                        newRows.[i] <- rows.[i]

                    for i in order.Length .. n - 1 do
                        let e = make items.[i] i

                        // Only keep the index current if one already exists; an
                        // append-only mount never builds it at all.
                        if indexed then
                            byKey.[newKeys.[i]] <- e

                        newRows.[i] <- e
                        frag.appendChild e.Node |> ignore

                    parent.insertBefore (frag, anchor) |> ignore
                    rows <- newRows

                order <- newKeys
            else
                // Trim the common prefix and suffix: those rows keep their key, their
                // entry and their DOM position, so only the middle needs diffing. A
                // removal or an edit at one end collapses to (almost) no work.
                let mutable start = 0
                let lim = min order.Length n

                while start < lim && obj.Equals(order.[start], newKeys.[start]) do
                    start <- start + 1

                let mutable endOld = order.Length - 1
                let mutable endNew = n - 1

                while endOld >= start
                      && endNew >= start
                      && obj.Equals(order.[endOld], newKeys.[endNew]) do
                    endOld <- endOld - 1
                    endNew <- endNew - 1

                // Full-replace fast path: no common prefix or suffix AND the key sets
                // are disjoint, so the diff would remove every old row and create
                // every new one. Batch it as clear + one DocumentFragment (like the
                // append path) instead of 2N removeChild/insertBefore calls.
                if start = 0 && endNew = n - 1 && isDisjoint newKeys then
                    for i in 0 .. rows.Length - 1 do
                        rows.[i].Dispose.Dispose()

                    removeAllRowNodes ()

                    let frag = document.createDocumentFragment ()
                    let newRows = Array.zeroCreate<Row> n

                    for i in 0 .. n - 1 do
                        let e = make items.[i] i
                        newRows.[i] <- e
                        frag.appendChild e.Node |> ignore

                    parent.insertBefore (frag, anchor) |> ignore
                    byKey.Clear()
                    indexed <- false
                    rows <- newRows
                    order <- newKeys
                else
                    diffMiddle start endOld endNew

        // The reconcile itself is an effect over the items signal.
        let sub = Signal.effect (fun () -> reconcile (getItems ()))

        let disposeAll () =
            sub.Dispose()

            // Walk `rows`, not `byKey` - the index may never have been built.
            for i in 0 .. rows.Length - 1 do
                rows.[i].Dispose.Dispose()
                parent.removeChild rows.[i].Node |> ignore

            byKey.Clear()
            indexed <- false
            order <- [||]
            rows <- [||]

        // Tear the whole list down with the enclosing component scope, too.
        Signal.onCleanup disposeAll

        { new IDisposable with
            member _.Dispose() = disposeAll ()
        }
