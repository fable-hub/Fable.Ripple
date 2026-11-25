module Fable.Ripple.Tests.KeyMap

open Fable.Core.JsInterop
open Scriptorium.Nib.Assertion

open type Scriptorium.Quill.Test

open Fable.Ripple.Internal

type private Box(n: int) =
    member _.N = n

let private bucketCount (m: KeyMap<'k, 'v>) : int = m?buckets?size

let tests =
    testList (
        "KeyMap",
        [
            test (
                "object keys: delete keeps the other entries",
                fun _ ->
                    let m = KeyMap<int * string, Box>()
                    m.Set((1, "a"), Box 1)
                    m.Set((2, "b"), Box 2)
                    m.Delete((1, "a"))

                    assertThat (m.Has((1, "a"))) (isEqualTo false)
                    assertThat (m.Get((2, "b"))).N (isEqualTo 2)
                    assertThat m.Count (isEqualTo 1)
            )

            test (
                "object keys: deleting every entry leaves no bucket",
                fun _ ->
                    let m = KeyMap<int * string, Box>()

                    for i in 1..100 do
                        m.Set((i, "k"), Box i)

                    for i in 1..100 do
                        m.Delete((i, "k"))

                    assertThat m.Count (isEqualTo 0)
                    assertThat (bucketCount m) (isEqualTo 0)
            )
        ]
    )
