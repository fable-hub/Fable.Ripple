# Fable.Ripple - design

A small, cross-platform (.NET + Fable/JS) fine-grained reactive layer. This is
the reactive engine only. The DOM layer is built on top of it (a DOM binding is
just a `Signal.subscribe` that writes to a node).

## Goals

- **Best-in-class update performance.** On a change, recompute the *minimal* set
  of nodes. A derived value whose inputs did not actually change is never
  recomputed, and its downstream is never touched.
- **No `memo` to remember.** Every derived value is a first-class `Signal<'T>`,
  which is a cached reactive node by construction. Combining inputs
  (`Signal.map2`, `signal { }`) never requires the user to opt into caching. This is
  the WebSharper.UI `View` model, not the Solid "run a thunk, remember to
  memoize" model.
- **Minimal magic.** No compiler, no source generator. `Signal<'T>` is an ordinary
  value; the `signal { }` CE is plain F# desugaring. Dependencies are tracked
  automatically by running computations, so you write normal F#.
- **Correctness first.** Glitch-free propagation, deterministic disposal.

Non-goals (for this layer): DOM, incremental collections/deltas (see "Rejected"),
async signals (roadmap), multithreading (the graph is single-threaded, like every
comparable library).

## Model

Two concepts, one type:

- `Signal<'T>` - a reactive value. Created as a **source** (`Signal.create`,
  writable) or a **computed** (`Signal.map`, `Signal.computed`, `signal { }`,
  read-only). Reading `.Value` inside a computation registers a dependency.
  Writing `.Value` on a computed throws.
- **Subscriptions** - `Signal.subscribe handler signal` runs `handler` immediately
  with the current value and again on every change; returns an `IDisposable`.
  This is the only "effect" primitive the reactive layer needs; DOM bindings are
  subscriptions.

Single type (cellx style) rather than a `Var`/`View` split (WebSharper style)
because it keeps the API flat - `Signal.map` works on any signal, `source.Value <- x`
just works - at the cost of compile-time write protection (computed writes throw
at runtime instead). A type-safe split can be layered later if wanted.

## The algorithm (why it is minimal)

Three-color marking + pull-based lazy evaluation with equality cutoff. This is
the algorithm behind cellx (`Actual`/`Dirty`/`Check`) and Reactively; modern
Solid, Preact Signals and Angular signals converged on the same shape.

Each node has a state:

- `Clean` - value is current.
- `Check` - a *transitive* dependency might have changed; verify before touching.
- `Dirty` - a *direct* dependency changed; must recompute.

**On write** (`source.Value <- v`, value actually differs):
mark direct observers `Dirty`, and propagate `Check` to everything downstream of
them. Propagation only recurses on a `Clean -> stale` transition, so a node is
visited once. Subscriptions hit during propagation are queued.

**On read / flush** (`updateIfNecessary`):
- If `Check`: walk sources and update each. A source that *actually* changed
  marks this node `Dirty` (see below). If after visiting the sources we are
  still `Check`, nothing real changed - **skip recompute**, settle to `Clean`.
- If `Dirty`: recompute. Compare the new value with the old using the signal's
  equality. **Only if it differs** do we store it and mark our observers `Dirty`.

This yields the two guarantees the design is about:

1. **Shared work runs once.** A computed read by several downstream nodes is
   pulled up to date once per change (diamonds do not double-compute).
2. **Equality cutoff.** If `a` changes but `f a` produces an equal value, every
   node past that point stays `Check`, is verified, and is *not* recomputed.

Dependencies are tracked dynamically: each recompute re-runs the function with
this node as the ambient reader and records exactly the signals read this time, so
conditional branches (and `bind`) rewire the graph automatically.

Default equality is `Unchecked.equals` (structural, no `equality` constraint on
`'T`, identical on .NET and Fable). Per-signal custom equality is available via
the `…With` variants (`createWith`/`computedWith`/`mapWith`/`map2With`).

## Consistency and batching

Writes flush synchronously by default, so a value read right after a write is
current (the same contract Ripple gave). `Signal.batch (fun () -> ...)` coalesces
multiple writes into a single flush; a subscription sees one update, not one per
write. A write performed from inside a running flush is absorbed into that flush
(re-entrancy guard) rather than starting a nested one.

## File layout

The engine is split by concern, in compile order. Each internal module owns a
disjoint slice of the engine's mutable state, which is what makes them separable
at all (F# allows no cycles between files):

| File | Contents | Mutable state owned |
|---|---|---|
| `ReactiveNode.fs` | `NodeState`, the non-generic graph node | - |
| `Internal/Graph.fs` | dependency edges: link, unlink, dispose a node | - |
| `Internal/Tracking.fs` | read tracking, `updateIfNecessary`, `recompute`, `untracked` | `current`, `currentGets` |
| `Internal/Scheduler.fs` | staleness propagation, effect queue, `batch`/`flush` | `pending`, `batchDepth`, `flushing` |
| `Internal/Scope.fs` | `Scope`, registration, cleanups, teardown | `currentScope` |
| `Signal.fs` | `Signal<'T>`, `Effect` | - |
| `SignalModule.fs` | the public `Signal` module | - |
| `Builder.fs` | the `signal { }` CE | - |

`Internal` is `Fable.Ripple.Internal` and every module in it is `internal`, so
none of it is public API.

## Cross-platform notes

- No BCL surface beyond `ResizeArray`, `Object.ReferenceEquals`,
  `Unchecked.equals/defaultof`, `IDisposable` - all supported identically by
  Fable and .NET.
- The heterogeneous dependency graph lives on a non-generic base (`ReactiveNode`)
  holding the edges and marking state; typed values live in `Signal<'T>`, so on
  .NET values are not boxed into `obj`. Fable erases generics anyway.
- Edge lists (`Sources`/`Observers`) are lazily allocated as `voption` (absent
  until first used). `voption` is chosen over reference `option` because it is a
  struct (no `Some` wrapper allocation on .NET) and Fable erases it to the raw
  nullable value on JS - type-safe at zero allocation cost on both runtimes
  (verified by codegen + BenchmarkDotNet). Marking state is an `enum` (a plain
  int at runtime).
- Single-threaded: one ambient "current computation" global, no locks.

## `signal { }` computation expression

Removes the last bit of ceremony from combining inputs, with no `memo`:

```fsharp
let total = signal {
    let! sub  = subtotal      // and! ...  -> applicative, compiles to map2/map3
    and! disc = discountAmount
    return sub - disc
}
```

- `let! ... and! ...` uses `MergeSources` + `BindReturn` -> `Signal.map2` (static
  dependencies, cheap, the common case). No `map3`/`map4` in user code.
- `let!` alone uses `Bind` -> `Signal.bind` (dynamic dependencies) - only when you
  branch on a signal's value.

## Public API (v1)

```
Signal.create    : 'T -> Signal<'T>                       // writable source
Signal.constant  : 'T -> Signal<'T>                       // never changes
Signal.computed  : (unit -> 'T) -> Signal<'T>             // auto-tracked derived
Signal.map       : ('a -> 'b) -> Signal<'a> -> Signal<'b>
Signal.map2/map3 : ...                                   // applicative combine
Signal.bind      : ('a -> Signal<'b>) -> Signal<'a> -> Signal<'b>   // dynamic
Signal.createWith / computedWith / mapWith / map2With    // + custom cutoff equality
Signal.referenceEquals                                   // opt out of structural compare
Signal.subscribe : ('T -> unit) -> Signal<'T> -> IDisposable
Signal.peek      : Signal<'T> -> 'T                        // read, no dependency
Signal.batch     : (unit -> unit) -> unit
Signal.untracked : (unit -> 'a) -> 'a
Signal.root      : (unit -> 'a) -> 'a * IDisposable      // scope (see below)
Signal.onCleanup : (unit -> unit) -> unit               // cleanup for current scope
signal { ... }                                           // CE over the above
member Signal.Value  (get tracks + pulls; set writes a source)
member Signal.Set / Signal.Peek
```

## Rejected / deferred

- **Incremental collections (FSharp.Data.Adaptive `aset`/`alist` deltas).**
  Rejected: the delta datastructures cost more than they save below ~100k items,
  and add large Fable bundle + GC pressure. Collection updates belong in the DOM
  layer as keyed reconciliation over a `Signal<'a[]>`, not as value-level deltas.
- **`transact` requirement (FDA).** Rejected in favour of implicit synchronous
  flush + optional `batch`.
- **Compiler / positional memoization (SwiftUI/Compose).** Rejected: that is the
  magic this design avoids.

## Performance roadmap (post-correctness)

Ordered by expected payoff; all are internal, none change the API:

1. ~~**Source-slot reuse** (Reactively-style): reconcile the dependency array in
   place across recomputes instead of clear + rebuild, so a stable graph does
   zero edge mutation.~~ **Done.** Removed an O(n^2) blowup in wide fan-out
   (105x faster at N=10000 on JS); see `bench/Reactive/README.md`.
2. **Subscription-based activation** (cellx active/inactive): a computed keeps
   its observer links only while something downstream is subscribed; unobserved
   computeds are pure pull. Saves marking work and helps GC for detached graphs.
3. ~~**Per-signal custom equality** for cheap cutoff on large values.~~ **Done** -
   `createWith`/`computedWith`/`mapWith`/`map2With` + `referenceEquals`.
4. **Async signals** (WebSharper Snap-style) for data-fetch driven values.

## Ownership & disposal (B implemented; A proposed)

### The problem

A computed links to its sources on first evaluation and is then retained by each
source's `Observers` list forever; there is no way to dispose a `Signal<_>` (only
`subscribe` returns an `IDisposable`). So a dynamic UI that creates derived signals
per row/component and drops them leaks: the source's observer list grows without
bound and the derived signals are never collected (the lapsed-listener problem).
Two sub-problems: (a) abandoned computeds read once then dropped; (b) grouped
teardown - a component creates several signals + effects that should die together.

FDA and WebSharper.UI avoid (a) with **weak references**. We reject weak refs:
non-deterministic, and `WeakRef` on JS is ES2021 with weak Fable support.

**The leak only occurs in one case.** If a component's source, computeds and
effects are all created and dropped together, they form an isolated island that
GC collects even with no cleanup. The leak is specifically a computed linked to a
**longer-lived (external) source**: when the component dies, that computed stays
in the external source's `Observers` and never unlinks.

Two mechanisms address this; **B is required and sufficient, A is an optional
later layer** (see below):

### Mechanism B - scopes (IMPLEMENTED). Deterministic teardown + leak fix.

- A **scope** collects the effects *and computeds* created within it, so a whole
  component/row tears down at once (the prototype's `collect`, ported; explicit,
  no weak refs). This is Solid's owner tree.
- `Signal.map`/`computed`/`subscribe` inside a scope auto-register with the current
  scope. Disposing the scope unlinks every registered computed from its sources
  and disposes every effect - which deterministically fixes the external-source
  leak (Case 2 above) without any activation machinery.
- The DOM layer needs this regardless: a removed subtree must dispose its
  bindings, and you will not hand-track each binding's disposable.
- Shipped as `Signal.root`, `Signal.onCleanup`, with `Signal.observerCount` as a
  diagnostic. `Signal.root` scopes nest; disposing a parent tears down children.
  Users do not call these directly - the DOM layer's `each`/`dyn`/component
  boundaries open the scopes (as `keyedEach` calls `collect` in the prototype).
  Covered by four tests, incl. the leak test (`observerCount` 100 -> 0 on dispose).

### Mechanism A - lazy activation (cellx active/inactive). OPTIONAL, later.

Not needed for correctness once B tracks computeds. It adds two things on top:
automatic cleanup for a computed abandoned *outside* any scope (an edge case),
and skipping marking work for unobserved-but-still-alive subgraphs (the roadmap
item #2 perf win). It costs an activation state machine and a semantic change to
unobserved pull (below). Defer until a build/teardown or unobserved-graph
benchmark justifies it.

- A node is **active** iff it is transitively observed by a live subscription
  (effects are the always-active roots).
- A computed **links to its sources only while active**. Gaining its first
  observer activates it -> it adds itself to each source's `Observers`
  (recursively activating sources). Losing its last observer deactivates it ->
  removes itself from each source's `Observers` (recursively deactivating).
- A **top-level pull** (`computed.Value` with no observer) computes on demand but
  retains no links -> the node stays inactive and is GC-eligible once dropped.
  **No leak.**
- Bonus: inactive subgraphs get no marking work - this is roadmap item #2, so the
  leak fix and the perf optimization are the *same* change.

One semantic change to weigh: an unobserved computed can't be push-invalidated,
so each top-level `.Value` re-verifies its sources rather than trusting a cached
value (cellx keeps the cached value and re-pulls sources to check - still cheap).

### API additions (minimal)

```
Signal.root      : (unit -> 'a) -> 'a * IDisposable   // open a scope; dispose tears down everything created inside
Signal.onCleanup : (unit -> unit) -> unit             // register cleanup with the current scope (optional)
```

Under B, computeds auto-unlink when their owning scope is disposed, so there is
no user-facing `Signal.dispose`. `Signal.root` is the single teardown handle.

### Decisions for review

1. **Scope shape** - `Signal.root` returning a disposer, plus an ambient scope stack
   so `Signal.map`/`computed`/`subscribe` auto-register. Recommend both.
2. **Escape hatch** - a way to create a signal outside the current scope (e.g. a
   genuinely global derived value) so it is not torn down with a transient scope.
3. **Defer A?** - recommend yes: ship B (correct + deterministic), revisit A only
   if benchmarks justify the activation machinery and the unobserved-pull change.
4. **Implementation risk (B)** - disposal unlinks observer lists; an effect that
   disposes its own scope mid-flush needs a guard so the flush loop isn't mutating
   a list it is iterating (defer the teardown to end-of-flush).

### Estimate

B only: ~60-90 lines in the core + tests, the key one being the leak test that is
red today - `source.Observers` returns to 0 after the owning scope is disposed.
