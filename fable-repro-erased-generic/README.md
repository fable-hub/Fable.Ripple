# Erased generic argument - verification, not a reproduction

The claim (from `demo/Examples/UnderTheHood/ErasedGeneric.fs` and formerly
`docs/content/signals/fable-specifics.md`): using a `[<Struct; Erase>]` type as a
generic argument makes Fable emit a dangling `Signal$1` import.

```fsharp
let cells = Dictionary<string, Signal<float>>()   // claimed: don't
```

## Result: does NOT reproduce on Fable 5.13.0

`Main.fs` exercises five shapes against the real `Fable.Ripple.Signal<'T>`:

1. `Dictionary<string, Signal<float>>` - generic argument of a collection
2. a record field of type `Signal<float>` - record reflection path
3. a union case carrying `Signal<float>`
4. `typeof<Signal<float>>`
5. `ResizeArray<Signal<float>>.Contains` - structural equality path

All five compile with no warning, the output contains no `Signal$1` reference at
all, and it runs:

```
$ dotnet fable Repro.fsproj --outDir out --noCache
$ node out/Main.js
1.500000 Var`1 true
```

The only observable trace of the erasure: `typeof<Signal<float>>` reports the
underlying ``Var`1``, which is what the value is at runtime.

Also verified with a locally defined erased struct (single file and across two
files) - same clean output.

## Conclusion

The restriction was real on an earlier Fable but is fixed by 5.13.0 (same era as
the `ValueOption.iter` inlining fix, Fable PR #4836). The demo comment in
`ErasedGeneric.fs` and the docs page describe a limitation that no longer exists;
both should be updated rather than a bug reported.
