# Fable.Ripple - design

A small, cross-platform (.NET + Fable/JS) fine-grained reactive engine. The DOM
layer is built on top (a DOM binding is just a `Signal.subscribe` writing to a node).

## Goals

- **Minimal recomputation.** On a change, recompute only the nodes whose inputs
  actually changed; everything downstream of an unchanged value is untouched.
- **No `memo` to remember.** Every derived value is a first-class, cached
  `Signal<'T>` by construction (the WebSharper.UI `View` model, not Solid's
  "run a thunk, remember to memoize").
- **No magic.** No compiler or source generator. Dependencies are tracked by
  running the computation, so you write normal F#.
- **Correctness first.** Glitch-free propagation, deterministic disposal.

Single-threaded, like every comparable library.

## Model

One type, two roles: a `Signal<'T>` is either a **source** (`Signal.create`,
writable) or a **computed** (`Signal.map`/`computed`/`signal { }`, read-only;
writing throws). Reading `.Value` inside a computation registers a dependency.
`Signal.subscribe` runs a handler now and on every change, returning an
`IDisposable` - the only effect primitive the engine needs.

Single type (cellx style) over a `Var`/`View` split (WebSharper) keeps the API
flat, at the cost of compile-time write protection (computed writes throw at
runtime). A type-safe split can be layered later.

## Algorithm

Three-color marking + pull-based lazy evaluation with equality cutoff - the shape
cellx, Reactively, Solid, Preact Signals and Angular signals all converged on.
Each node is `Clean` (current), `Check` (a transitive dep *might* have changed),
or `Dirty` (a direct dep changed).

- **On write** (value differs): mark direct observers `Dirty`, propagate `Check`
  downstream. Propagation recurses only on a `Clean -> stale` transition, so each
  node is visited once; subscriptions hit are queued.
- **On read/flush**: if `Check`, update sources first - if none actually changed,
  settle to `Clean` without recomputing. If `Dirty`, recompute and compare against
  the old value; only a real difference is stored and marks observers `Dirty`.

This gives the two guarantees the design is about: **shared work runs once**
(diamonds don't double-compute) and **equality cutoff** (an equal result stops
propagation). Dependencies are re-recorded on every recompute, so branches and
`bind` rewire the graph automatically.

Default equality is `Unchecked.equals` (structural, identical on .NET and Fable);
`…With` variants + `referenceEquals` give per-signal custom cutoff.

## Consistency

Writes flush synchronously, so a read right after a write is current.
`Signal.batch` coalesces writes into one flush; a write from inside a running
flush is absorbed into it (re-entrancy guard), not nested.

## Ownership & disposal

A computed stays in each source's observer list until unlinked, so a UI that
creates derived signals per row/component and drops them would leak against any
longer-lived source. **Scopes** fix this deterministically (no weak refs):
`Signal.root` collects every computed/effect created inside it, and disposing the
scope unlinks them all at once. Scopes nest; disposing a parent tears down
children. The DOM layer opens a scope per list-row / dynamic subtree, so removal
disposes the bindings. `Signal.onCleanup` registers a teardown callback with the
current scope; `Signal.observerCount` is a diagnostic.

## Rejected / deferred

- **Incremental collections (FDA `aset`/`alist` deltas).** Deltas cost more than
  they save below ~100k items and bloat the Fable bundle; collection updates
  belong in the DOM layer as keyed reconciliation over a `Signal<'a[]>`.
- **`transact` requirement (FDA).** Replaced by implicit synchronous flush +
  optional `batch`.
- **Compiler / positional memoization (SwiftUI/Compose).** The magic this design
  avoids.
- **Lazy activation (cellx active/inactive).** Not needed for correctness once
  scopes track computeds; revisit only if an unobserved-graph benchmark justifies
  the activation state machine.
- **Async signals** (WebSharper Snap-style) - roadmap.
