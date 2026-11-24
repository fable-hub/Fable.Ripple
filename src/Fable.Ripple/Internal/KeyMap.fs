namespace Fable.Ripple.Internal

open Fable.Core
open Fable.Core.JsInterop

/// Map keyed by F# equality. Primitive keys live in a JS `Map`; object keys are
/// bucketed by structural hash and compared with structural equality.
type internal KeyMap<'k, 'v when 'k: equality>() =
    let direct = JS.Constructors.Map.Create<'k, 'v>()
    let mutable buckets: JS.Map<int, ResizeArray<'k * 'v>> = Unchecked.defaultof<_>

    let isObject (k: 'k) =
        jsTypeof k = "object" && not (isNull (box k))

    let indexIn (bucket: ResizeArray<'k * 'v>) (k: 'k) =
        let mutable i = bucket.Count - 1

        while i >= 0 && not (Unchecked.equals (fst bucket.[i]) k) do
            i <- i - 1

        i

    let bucketOf (k: 'k) (create: bool) : ResizeArray<'k * 'v> =
        if isNull (box buckets) then
            if create then
                buckets <- JS.Constructors.Map.Create<int, ResizeArray<'k * 'v>>()

        if isNull (box buckets) then
            Unchecked.defaultof<_>
        else
            let h = hash k
            let b = buckets.get h

            if isNull (box b) && create then
                let b = ResizeArray()
                buckets.set (h, b) |> ignore
                b
            else
                b

    /// The value for `k`, or `null`/default when absent.
    member _.Get(k: 'k) : 'v =
        if isObject k then
            let b = bucketOf k false

            if isNull (box b) then
                Unchecked.defaultof<'v>
            else
                match indexIn b k with
                | -1 -> Unchecked.defaultof<'v>
                | i -> snd b.[i]
        else
            direct.get k

    member inline this.Has(k: 'k) : bool = not (isNull (box (this.Get k)))

    // `'v` must be a class: Fable erases the voption and tests ValueNone with `== null`.
    member inline this.TryGet(k: 'k) : 'v voption = unbox (this.Get k)

    member _.Set(k: 'k, v: 'v) =
        if isObject k then
            let b = bucketOf k true

            match indexIn b k with
            | -1 -> b.Add((k, v))
            | i -> b.[i] <- (k, v)
        else
            direct.set (k, v) |> ignore

    member _.Delete(k: 'k) =
        if isObject k then
            let b = bucketOf k false

            if not (isNull (box b)) then
                match indexIn b k with
                | -1 -> ()
                | i -> b.RemoveAt i
        else
            direct.delete k |> ignore

    member _.Clear() =
        direct.clear ()

        if not (isNull (box buckets)) then
            buckets.clear ()

    /// Number of entries.
    member _.Count: int =
        let mutable n = int direct.size

        if not (isNull (box buckets)) then
            buckets.forEach (fun b _ _ -> n <- n + b.Count)

        n

    /// Remove every entry whose value matches `drop`.
    member _.RemoveWhere(drop: 'v -> bool) =
        direct.forEach (fun v k _ ->
            if drop v then
                direct.delete k |> ignore
        )

        if not (isNull (box buckets)) then
            buckets.forEach (fun b _ _ -> b.RemoveAll(fun (_, v) -> drop v) |> ignore)
