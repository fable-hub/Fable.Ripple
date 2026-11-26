module Fable.Ripple.Tests.Ownership

open Scriptorium.Nib.Assertion

open type Scriptorium.Quill.Test

open Fable.Ripple

let tests =
    testSequenced (
        "Ownership",
        [
            test (
                "an effect created by a re-run is disposed with the next run and with the root",
                fun _ ->
                    let trig = Var.create 0
                    let other = Var.create 0

                    let _, root =
                        Signal.root (fun () ->
                            Signal.autorun (fun () ->
                                trig.Value |> ignore
                                Signal.autorun (fun () -> other.Value |> ignore)
                            )
                        )

                    for i in 1..1000 do
                        trig.Value <- i

                    assertThat (Signal.observerCount other) (isEqualTo 1)
                    root.Dispose()
                    assertThat (Signal.observerCount other) (isEqualTo 0)
            )

            test (
                "bind keeps one inner signal",
                fun _ ->
                    let src = Var.create 0
                    let other = Var.create 0
                    let mutable seen = 0

                    let _, root =
                        Signal.root (fun () ->
                            let b = Signal.bind (fun x -> Signal.map ((+) x) other) src
                            Signal.autorun (fun () -> seen <- b.Value)
                        )

                    for i in 1..1000 do
                        src.Value <- i

                    other.Value <- 5
                    assertThat seen (isEqualTo 1005)
                    assertThat (Signal.observerCount other) (isEqualTo 1)
                    root.Dispose()
                    assertThat (Signal.observerCount other) (isEqualTo 0)
            )

            test (
                "a cleanup registered by a run runs before the next run",
                fun _ ->
                    let trig = Var.create 0
                    let log = ResizeArray<string>()

                    let _, root =
                        Signal.root (fun () ->
                            Signal.autorun (fun () ->
                                let n = trig.Value
                                log.Add $"run {n}"
                                Signal.onCleanup (fun () -> log.Add $"clean {n}")
                            )
                        )

                    trig.Value <- 1
                    root.Dispose()

                    assertThat
                        (List.ofSeq log)
                        (isEqualTo
                            [
                                "run 0"
                                "clean 0"
                                "run 1"
                                "clean 1"
                            ])
            )

            test (
                "a re-run flushed inside an unrelated root does not join that root",
                fun _ ->
                    let trig = Var.create 0
                    let other = Var.create 0
                    let log = ResizeArray<int>()

                    let _, owner =
                        Signal.root (fun () ->
                            Signal.autorun (fun () ->
                                let n = trig.Value

                                Signal.autorun (fun () ->
                                    if other.Value > 0 then
                                        log.Add n
                                )
                            )
                        )

                    let _, unrelated = Signal.root (fun () -> trig.Value <- 1)
                    unrelated.Dispose()
                    other.Value <- 1
                    assertThat (List.ofSeq log) (isEqualTo [ 1 ])
                    owner.Dispose()
                    assertThat (Signal.observerCount other) (isEqualTo 0)
            )

            test (
                "a root made by a run outlives the run until disposed",
                fun _ ->
                    let items = Var.create 0
                    let other = Var.create 0
                    let roots = ResizeArray<System.IDisposable>()

                    let _, owner =
                        Signal.root (fun () ->
                            Signal.autorun (fun () ->
                                items.Value |> ignore

                                let _, d =
                                    Signal.root (fun () ->
                                        Signal.autorun (fun () -> other.Value |> ignore)
                                    )

                                roots.Add d
                            )
                        )

                    items.Value <- 1
                    items.Value <- 2
                    assertThat (Signal.observerCount other) (isEqualTo 3)

                    for d in roots do
                        d.Dispose()

                    owner.Dispose()
                    assertThat (Signal.observerCount other) (isEqualTo 0)
            )

            test (
                "an effect created inside untracked by a re-run belongs to that run",
                fun _ ->
                    let trig = Var.create 0
                    let other = Var.create 0

                    let _, root =
                        Signal.root (fun () ->
                            Signal.autorun (fun () ->
                                trig.Value |> ignore

                                Signal.untracked (fun () ->
                                    Signal.autorun (fun () -> other.Value |> ignore)
                                )
                            )
                        )

                    for i in 1..100 do
                        trig.Value <- i

                    assertThat (Signal.observerCount other) (isEqualTo 1)
                    root.Dispose()
                    assertThat (Signal.observerCount other) (isEqualTo 0)
            )

            test (
                "a root inside untracked in a run is owned by its creator, not the run",
                fun _ ->
                    let trig = Var.create 0
                    let other = Var.create 0
                    let mutable last: System.IDisposable = null

                    let _, root =
                        Signal.root (fun () ->
                            Signal.autorun (fun () ->
                                trig.Value |> ignore

                                Signal.untracked (fun () ->
                                    let _, d =
                                        Signal.root (fun () ->
                                            Signal.autorun (fun () -> other.Value |> ignore)
                                        )

                                    last <- d
                                )
                            )
                        )

                    trig.Value <- 1
                    assertThat (Signal.observerCount other) (isEqualTo 2)
                    last.Dispose()
                    assertThat (Signal.observerCount other) (isEqualTo 1)
                    root.Dispose()
            )
        ]
    )
