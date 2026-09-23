module Fable.Ripple.Tests.Signal

open Scriptorium.Nib.Assertion

open type Scriptorium.Quill.Test
open Scriptorium.Quill.Prelude

open Fable.Ripple

// Sequenced: every test drives the one global reactive engine (single-threaded
// by design, like Solid/cellx), so they must not run in parallel.
let tests =
    testSequenced (
        "Signal",
        [

            (*
                Basics
            *)

            test (
                "source read returns its value",
                fun _ -> assertThat (Var.create 1).Value (isEqualTo 1)
            )

            test (
                "map derives and follows its source",
                fun _ ->
                    let a = Var.create 2
                    let b = Signal.map (fun x -> x * 10) a
                    assertThat b.Value (isEqualTo 20)
                    a.Value <- 3
                    assertThat b.Value (isEqualTo 30)
            )

            test (
                "map2 combines two signals",
                fun _ ->
                    let a = Var.create 1
                    let b = Var.create 2
                    let c = Signal.map2 (+) a b
                    assertThat c.Value (isEqualTo 3)
                    a.Value <- 10
                    assertThat c.Value (isEqualTo 12)
            )

            test (
                "writing a computed through a downcast throws",
                fun _ ->
                    let a = Var.create 1
                    let b = Signal.map (fun x -> x * 10) a

                    let threw =
                        try
                            (b :?> Var<int>).Value <- 5
                            false
                        with _ ->
                            true

                    assertThat threw (isEqualTo true)
                    assertThat b.Value (isEqualTo 10)
            )

            test (
                "computed is lazy until first read",
                fun _ ->
                    let a = Var.create 1
                    let runs = ref 0

                    let c =
                        Signal.computed (fun () ->
                            runs.Value <- runs.Value + 1
                            a.Value
                        )

                    assertThat runs.Value (isEqualTo 0)
                    assertThat c.Value (isEqualTo 1)
                    assertThat runs.Value (isEqualTo 1)
            )

            test (
                "constant never changes and never recomputes dependents",
                fun _ ->
                    let k = Signal.constant 42
                    assertThat k.Value (isEqualTo 42)

                    let runs = ref 0

                    let d =
                        Signal.map
                            (fun v ->
                                runs.Value <- runs.Value + 1
                                v + 1
                            )
                            k

                    assertThat d.Value (isEqualTo 43)
                    assertThat runs.Value (isEqualTo 1)
                    // Re-reading recomputes nothing: a constant has no changing deps.
                    assertThat d.Value (isEqualTo 43)
                    assertThat runs.Value (isEqualTo 1)
            )

            test (
                "map3 combines three signals",
                fun _ ->
                    let a = Var.create 1
                    let b = Var.create 2
                    let c = Var.create 3
                    let r = Signal.map3 (fun x y z -> x + y + z) a b c
                    assertThat r.Value (isEqualTo 6)
                    b.Value <- 20
                    assertThat r.Value (isEqualTo 24)
                    c.Value <- 300
                    assertThat r.Value (isEqualTo 321)
            )

            test (
                "Set writes the same as the Value setter",
                fun _ ->
                    let a = Var.create 1
                    let b = Signal.map (fun v -> v + 100) a
                    a.Set 5
                    assertThat a.Value (isEqualTo 5)
                    assertThat b.Value (isEqualTo 105)
            )

            (*
                Subscriptions
            *)

            test (
                "subscribe fires with current value then on each change",
                fun _ ->
                    let a = Var.create 1
                    let seen = ResizeArray<int>()
                    use _ = Signal.subscribe seen.Add a
                    a.Value <- 2
                    a.Value <- 3

                    assertThat
                        (List.ofSeq seen)
                        (isEqualTo
                            [
                                1
                                2
                                3
                            ])
            )

            test (
                "dispose stops a subscription",
                fun _ ->
                    let a = Var.create 1
                    let seen = ResizeArray<int>()
                    let sub = Signal.subscribe seen.Add a
                    a.Value <- 2
                    sub.Dispose()
                    a.Value <- 3

                    assertThat
                        (List.ofSeq seen)
                        (isEqualTo
                            [
                                1
                                2
                            ])
            )

            test (
                "effect re-runs on change of any signal it reads",
                fun _ ->
                    let a = Var.create 1
                    let b = Var.create 10
                    let runs = ResizeArray<int>()
                    use _ = Signal.effect (fun () -> runs.Add(a.Value + b.Value))
                    a.Value <- 2 // -> 12
                    b.Value <- 20 // -> 22

                    assertThat
                        (List.ofSeq runs)
                        (isEqualTo
                            [
                                11
                                12
                                22
                            ])
            )

            test (
                "a write from inside an effect is handled in the same flush",
                fun _ ->
                    let a = Var.create 0
                    let b = Var.create 0
                    let seen = ResizeArray<int>()
                    // The effect mirrors a into b - a write while the flush is running,
                    // which must extend the current flush rather than be dropped.
                    use _ = Signal.effect (fun () -> b.Value <- a.Value)
                    use _ = Signal.subscribe seen.Add b
                    a.Value <- 5

                    assertThat b.Value (isEqualTo 5)

                    assertThat
                        (List.ofSeq seen)
                        (isEqualTo
                            [
                                0
                                5
                            ])
            )

            (*
                Minimal recomputation
            *)

            test (
                "shared computed recomputes once per change (diamond)",
                fun _ ->
                    let a = Var.create 1
                    let runs = ref 0

                    let b =
                        Signal.computed (fun () ->
                            runs.Value <- runs.Value + 1
                            a.Value + 1
                        )

                    let c = Signal.map (fun x -> x * 2) b
                    let d = Signal.map (fun x -> x + 3) b
                    let e = Signal.map2 (+) c d
                    use _ = Signal.subscribe ignore e
                    let before = runs.Value
                    a.Value <- 2
                    assertThat (runs.Value - before) (isEqualTo 1)
            )

            test (
                "equality cutoff skips downstream recompute",
                fun _ ->
                    let a = Var.create 5
                    let parity = Signal.map (fun x -> x % 2) a
                    let runs = ref 0

                    let sink =
                        Signal.map
                            (fun p ->
                                runs.Value <- runs.Value + 1
                                p * 100
                            )
                            parity

                    use _ = Signal.subscribe ignore sink
                    let before = runs.Value
                    a.Value <- 7 // still odd -> parity unchanged -> no recompute
                    assertThat (runs.Value - before) (isEqualTo 0)
                    a.Value <- 8 // even -> parity changes -> one recompute
                    assertThat (runs.Value - before) (isEqualTo 1)
            )

            test (
                "createWith custom equality suppresses sub-threshold writes",
                fun _ ->
                    // Two values are "equal" if within 1, so small writes are no-ops.
                    let a = Var.createWith (fun x y -> abs (x - y) <= 1) 0
                    let runs = ref 0

                    let derived =
                        Signal.map
                            (fun v ->
                                runs.Value <- runs.Value + 1
                                v * 10
                            )
                            a

                    use _ = Signal.subscribe ignore derived
                    let before = runs.Value
                    a.Value <- 1 // within 1 of 0 -> treated equal -> no propagation
                    assertThat (runs.Value - before) (isEqualTo 0)
                    a.Value <- 5 // differs by 5 -> propagates
                    assertThat (runs.Value - before) (isEqualTo 1)
                    assertThat derived.Value (isEqualTo 50)
            )

            test (
                "mapWith custom equality controls downstream cutoff",
                fun _ ->
                    let a = Var.create 0
                    // Result equality that always reports "equal" -> never propagates.
                    let m = Signal.mapWith (fun _ _ -> true) id a
                    let runs = ref 0

                    let sink =
                        Signal.map
                            (fun v ->
                                runs.Value <- runs.Value + 1
                                v
                            )
                            m

                    use _ = Signal.subscribe ignore sink
                    let before = runs.Value
                    a.Value <- 1
                    a.Value <- 2
                    assertThat (runs.Value - before) (isEqualTo 0)
            )

            test (
                "map2With custom equality controls downstream cutoff",
                fun _ ->
                    let a = Var.create 0
                    let b = Var.create 0
                    // Result equality that always reports "equal" -> never propagates.
                    let m = Signal.map2With (fun _ _ -> true) (+) a b
                    let runs = ref 0

                    let sink =
                        Signal.map
                            (fun v ->
                                runs.Value <- runs.Value + 1
                                v
                            )
                            m

                    use _ = Signal.subscribe ignore sink
                    let before = runs.Value
                    a.Value <- 1
                    b.Value <- 2
                    assertThat (runs.Value - before) (isEqualTo 0)
            )

            test (
                "computedWith custom equality caps downstream recompute",
                fun _ ->
                    let a = Var.create 0
                    // Bucket by tens: only crossing a boundary should propagate.
                    let bucket = Signal.computedWith (=) (fun () -> a.Value / 10)
                    let runs = ref 0

                    let sink =
                        Signal.map
                            (fun v ->
                                runs.Value <- runs.Value + 1
                                v
                            )
                            bucket

                    use _ = Signal.subscribe ignore sink
                    let before = runs.Value
                    a.Value <- 5 // still bucket 0 -> no downstream recompute
                    assertThat (runs.Value - before) (isEqualTo 0)
                    a.Value <- 12 // bucket 1 -> one recompute
                    assertThat (runs.Value - before) (isEqualTo 1)
            )

            test (
                "referenceEquals treats a new equal-by-value reference as a change",
                fun _ ->
                    let box1 = ref 1
                    let box2 = ref 1 // structurally equal, different reference
                    let a = Var.createWith Signal.referenceEquals box1
                    let runs = ref 0

                    use _ =
                        Signal.effect (fun () ->
                            a.Value |> ignore
                            runs.Value <- runs.Value + 1
                        )

                    let before = runs.Value
                    a.Value <- box1 // same reference -> no propagation
                    assertThat (runs.Value - before) (isEqualTo 0)
                    a.Value <- box2 // different reference -> propagates
                    assertThat (runs.Value - before) (isEqualTo 1)
            )

            (*
                Dynamic dependencies
            *)

            test (
                "bind rewires dependencies dynamically",
                fun _ ->
                    let cond = Var.create true
                    let a = Var.create 1
                    let b = Var.create 2

                    let r =
                        Signal.bind
                            (fun c ->
                                if c then
                                    a.Signal
                                else
                                    b.Signal
                            )
                            cond

                    assertThat r.Value (isEqualTo 1)
                    b.Value <- 20 // r depends on a, not b
                    assertThat r.Value (isEqualTo 1)
                    cond.Value <- false // now depends on b
                    assertThat r.Value (isEqualTo 20)
                    a.Value <- 100 // no longer depends on a
                    assertThat r.Value (isEqualTo 20)
            )

            test (
                "computed with a growing then shrinking dependency set",
                fun _ ->
                    let n = Var.create 2
                    let a = Var.create 1
                    let b = Var.create 10
                    let c = Var.create 100

                    let all =
                        [|
                            a
                            b
                            c
                        |]

                    // Sum of the first `n` of [a; b; c] - the dependency set
                    // changes with n, exercising edge append and truncate.
                    let sum =
                        Signal.computed (fun () ->
                            let k = n.Value
                            let mutable acc = 0

                            for i in 0 .. k - 1 do
                                acc <- acc + all.[i].Value

                            acc
                        )

                    assertThat sum.Value (isEqualTo 11) // a + b
                    c.Value <- 1000 // not a dependency yet
                    assertThat sum.Value (isEqualTo 11)
                    n.Value <- 3 // grow: now depends on c too
                    assertThat sum.Value (isEqualTo 1011)
                    b.Value <- 20
                    assertThat sum.Value (isEqualTo 1021)
                    n.Value <- 1 // shrink: depends on a only
                    assertThat sum.Value (isEqualTo 1)
                    b.Value <- 999 // no longer a dependency
                    assertThat sum.Value (isEqualTo 1)
            )

            (*
                Batching / peek / untracked
            *)

            test (
                "batch coalesces writes into a single update",
                fun _ ->
                    let a = Var.create 1
                    let b = Var.create 1
                    let sum = Signal.map2 (+) a b
                    let runs = ref 0
                    use _ = Signal.subscribe (fun _ -> runs.Value <- runs.Value + 1) sum
                    let before = runs.Value

                    Signal.batch (fun () ->
                        a.Value <- 10
                        b.Value <- 20
                    )

                    assertThat (runs.Value - before) (isEqualTo 1)
                    assertThat sum.Value (isEqualTo 30)
            )

            test (
                "peek reads without creating a dependency",
                fun _ ->
                    let a = Var.create 1
                    let b = Var.create 1
                    let c = Signal.computed (fun () -> a.Value + Signal.peek b)
                    assertThat c.Value (isEqualTo 2)
                    b.Value <- 100 // peeked -> no update
                    assertThat c.Value (isEqualTo 2)
                    a.Value <- 5 // tracked -> updates, re-peeks b (100)
                    assertThat c.Value (isEqualTo 105)
            )

            test (
                "untracked reads inside a computation create no dependency",
                fun _ ->
                    let a = Var.create 1
                    let b = Var.create 10
                    let runs = ref 0

                    let c =
                        Signal.computed (fun () ->
                            runs.Value <- runs.Value + 1
                            a.Value + Signal.untracked (fun () -> b.Value)
                        )

                    assertThat c.Value (isEqualTo 11)
                    let before = runs.Value
                    b.Value <- 100 // read untracked -> no recompute, still cached
                    assertThat (runs.Value - before) (isEqualTo 0)
                    assertThat c.Value (isEqualTo 11)
                    a.Value <- 2 // tracked -> recompute, re-reads b untracked (100)
                    assertThat c.Value (isEqualTo 102)
            )

            test (
                "nested batch flushes once, when the outermost batch exits",
                fun _ ->
                    let a = Var.create 0
                    let b = Var.create 0
                    let sum = Signal.map2 (+) a b
                    let runs = ref 0
                    use _ = Signal.subscribe (fun _ -> runs.Value <- runs.Value + 1) sum
                    let before = runs.Value

                    Signal.batch (fun () ->
                        a.Value <- 1

                        Signal.batch (fun () ->
                            b.Value <- 2
                            a.Value <- 3
                        )

                        // Inner batch exited but the outer is still open -> no flush yet.
                        assertThat (runs.Value - before) (isEqualTo 0)
                    )

                    assertThat (runs.Value - before) (isEqualTo 1)
                    assertThat sum.Value (isEqualTo 5)
            )

            (*
                Ownership / disposal (the leak fix)
            *)

            test (
                "disposing a scope unlinks its computeds from an external source",
                fun _ ->
                    let ext = Var.create 0 // long-lived, created outside the scope

                    let _, owner =
                        Signal.root (fun () ->
                            let signals = Array.init 100 (fun i -> Signal.map (fun v -> v + i) ext)

                            for c in signals do
                                c.Value |> ignore // read -> links to ext
                        )

                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 100)
                    owner.Dispose()
                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 0) // no leak
            )

            test (
                "disposing a scope stops effects created inside it",
                fun _ ->
                    let s = Var.create 0
                    let seen = ResizeArray<int>()

                    let _, owner = Signal.root (fun () -> Signal.subscribe seen.Add s |> ignore)

                    s.Value <- 1 // effect fires
                    owner.Dispose()
                    s.Value <- 2 // effect gone

                    assertThat
                        (List.ofSeq seen)
                        (isEqualTo
                            [
                                0
                                1
                            ])
            )

            test (
                "disposing a scope runs onCleanup callbacks",
                fun _ ->
                    let log = ResizeArray<string>()

                    let _, owner =
                        Signal.root (fun () ->
                            Signal.onCleanup (fun () -> log.Add "a")
                            Signal.onCleanup (fun () -> log.Add "b")
                        )

                    assertThat (List.ofSeq log) (isEqualTo [])
                    owner.Dispose()

                    assertThat
                        (List.ofSeq log)
                        (isEqualTo
                            [
                                "a"
                                "b"
                            ])
            )

            test (
                "disposing a parent scope tears down nested child scopes",
                fun _ ->
                    let ext = Var.create 0

                    let _, parent =
                        Signal.root (fun () ->
                            // A nested scope whose disposer we intentionally drop:
                            // the parent must still tear it down.
                            Signal.root (fun () ->
                                let c = Signal.map (fun v -> v + 1) ext
                                c.Value |> ignore
                            )
                            |> ignore
                        )

                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 1)
                    parent.Dispose()
                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 0)
            )

            test (
                "disposing sibling scopes one by one keeps observerCount exact",
                fun _ ->
                    let ext = Var.create 0

                    let owners =
                        Array.init
                            10
                            (fun i ->
                                Signal.root (fun () ->
                                    (Signal.map (fun v -> v + i) ext).Value |> ignore
                                )
                                |> snd
                            )

                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 10)
                    owners.[0].Dispose()
                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 9)
                    owners.[3].Dispose()
                    owners.[7].Dispose()
                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 7)
                    owners.[3].Dispose() // a second dispose is a no-op
                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 7)

                    for owner in owners do
                        owner.Dispose()

                    assertThat (Signal.observerCount ext.Signal) (isEqualTo 0)
            )

            test (
                "effects of disposed sibling scopes never run again, while live ones still do",
                fun _ ->
                    let src = Var.create 0
                    let runs = Array.zeroCreate<int> 10

                    let owners =
                        Array.init
                            10
                            (fun i ->
                                Signal.root (fun () ->
                                    Signal.effect (fun () ->
                                        src.Value |> ignore
                                        runs.[i] <- runs.[i] + 1
                                    )
                                    |> ignore
                                )
                                |> snd
                            )

                    // Two of ten: too few for their entries in `src` to be swept yet,
                    // so this checks that propagation skips them.
                    owners.[2].Dispose()
                    owners.[5].Dispose()
                    src.Value <- 1

                    assertThat
                        (List.ofArray runs)
                        (isEqualTo
                            [
                                2
                                2
                                1
                                2
                                2
                                1
                                2
                                2
                                2
                                2
                            ])
            )

            test (
                "disposing many sibling scopes that share a source takes linear time",
                fun _ ->
                    // The shape of every keyed-list row reading one shared signal.
                    // A full pass over the observer list per scope is n^2 / 2 steps
                    // (1.25 billion here, seconds); swept lazily it stays linear.
                    let src = Var.create 0
                    let n = 50000

                    let owners =
                        Array.init
                            n
                            (fun _ ->
                                Signal.root (fun () ->
                                    Signal.effect (fun () -> src.Value |> ignore) |> ignore
                                )
                                |> snd
                            )

                    let stopwatch = UniversalStopwatch()

                    for owner in owners do
                        owner.Dispose()

                    let elapsed = stopwatch.ElapsedMs()
                    assertThat (Signal.observerCount src.Signal) (isEqualTo 0)
                    assertThat elapsed (isLessThan 1000)
            )

            (*
                Exception safety
            *)

            test (
                "a throwing compute recovers without corrupting other signals",
                fun _ ->
                    let a = Var.create 1

                    let bad =
                        Signal.computed (fun () ->
                            if a.Value > 0 then
                                failwith "boom"
                            else
                                a.Value
                        )

                    let threw =
                        try
                            bad.Value |> ignore
                            false
                        with _ ->
                            true

                    assertThat threw (isEqualTo true)
                    // Unrelated signals still evaluate and update correctly.
                    let x = Var.create 10
                    let y = Signal.map (fun v -> v + 1) x
                    assertThat y.Value (isEqualTo 11)
                    x.Value <- 5
                    assertThat y.Value (isEqualTo 6)
                    // And the throwing signal recovers once its input stops throwing.
                    a.Value <- 0
                    assertThat bad.Value (isEqualTo 0)
            )

            test (
                "a throwing effect during flush does not wedge the engine",
                fun _ ->
                    let a = Var.create 1

                    let bad =
                        Signal.computed (fun () ->
                            if a.Value > 5 then
                                failwith "boom"
                            else
                                a.Value * 10
                        )

                    let seenBad = ResizeArray<int>()
                    use _ = Signal.subscribe seenBad.Add bad

                    let threw =
                        try
                            a.Value <- 10 // flush runs the effect -> bad recomputes -> throws
                            false
                        with _ ->
                            true

                    assertThat threw (isEqualTo true)
                    // A fresh subscription still fires on later changes (flush not wedged).
                    let s = Var.create 1
                    let seen = ResizeArray<int>()
                    use _ = Signal.subscribe seen.Add s
                    s.Value <- 2

                    assertThat
                        (List.ofSeq seen)
                        (isEqualTo
                            [
                                1
                                2
                            ])
            )

            (*
                signal { } computation expression
            *)

            test (
                "signal CE applicative (and!) combines inputs",
                fun _ ->
                    let a = Var.create 3
                    let b = Var.create 4

                    let c =
                        signal {
                            let! x = a
                            and! y = b
                            return x * y
                        }

                    assertThat c.Value (isEqualTo 12)
                    a.Value <- 5
                    assertThat c.Value (isEqualTo 20)
            )

            test (
                "signal CE monadic (let!) binds",
                fun _ ->
                    let pick = Var.create true
                    let a = Var.create 1
                    let b = Var.create 2

                    let c =
                        signal {
                            let! p = pick

                            return!
                                (if p then
                                     a.Signal
                                 else
                                     b.Signal)
                        }

                    assertThat c.Value (isEqualTo 1)
                    pick.Value <- false
                    assertThat c.Value (isEqualTo 2)
            )

            test (
                "signal CE bare return is a constant (Return)",
                fun _ ->
                    let c = signal { return 7 }
                    assertThat c.Value (isEqualTo 7)

                    // Behaves like Signal.constant: a dependent computes once and is
                    // never invalidated.
                    let runs = ref 0

                    let d =
                        Signal.map
                            (fun v ->
                                runs.Value <- runs.Value + 1
                                v + 1
                            )
                            c

                    assertThat d.Value (isEqualTo 8)
                    assertThat d.Value (isEqualTo 8)
                    assertThat runs.Value (isEqualTo 1)
            )

            test (
                "signal CE bare return! forwards a signal (ReturnFrom)",
                fun _ ->
                    let a = Var.create 5
                    let c = signal { return! a.Signal }
                    assertThat c.Value (isEqualTo 5)
                    a.Value <- 6
                    assertThat c.Value (isEqualTo 6)
            )

            test (
                "signal CE single let! + return maps one source (BindReturn)",
                fun _ ->
                    let a = Var.create 3

                    let c =
                        signal {
                            let! x = a
                            return x + 1
                        }

                    assertThat c.Value (isEqualTo 4)
                    a.Value <- 10
                    assertThat c.Value (isEqualTo 11)
            )

            test (
                "signal CE applicative combines three sources (chained and!)",
                fun _ ->
                    let a = Var.create 1
                    let b = Var.create 2
                    let c = Var.create 3

                    let r =
                        signal {
                            let! x = a
                            and! y = b
                            and! z = c
                            return x + y + z
                        }

                    assertThat r.Value (isEqualTo 6)
                    b.Value <- 20
                    assertThat r.Value (isEqualTo 24)
                    c.Value <- 300
                    assertThat r.Value (isEqualTo 321)
            )

        ]
    )
