# Fable.Ripple.Plugin

The Fable compiler plugin behind `[<Component>]`.

## What it does today

Turns a component into a hot-reload boundary. `[<Component>] let render () = ...`
becomes a value whose initialiser registers the implementation and self-accepts:

```js
export const render = (() => {
  if (import.meta.hot) { import.meta.hot.accept(); }
  return define(import.meta.url, "render", (() => {
    const count = hmrVar("count", 0);
    ...
  }));
})();
```

Two rewrites: the member becomes a variadic value so call sites are untouched, and
`let x = Var.create v` becomes `let x = hmrVar "x" v` so the state survives a swap.

No-op outside Debug (`PluginHelper.Options.DebugMode`). Note `dotnet fable` defaults to
Release while `dotnet fable watch` defaults to Debug.

## Why it is not called Fable.Ripple.Hmr

`[<Component>]` marks a component; hot reload is the first thing built on that, not the
only one. An earlier revision also emitted, from the same attribute:

- `Debug.label` for every `Var` and `Signal` binding, named from the binding itself
- `file:line` for components and for each reactive binding, for click-to-source

That work is parked in `experiments/devtools`. Other things the attribute could carry
later: source locations on elements, dev-mode diagnostics for components that cannot be
a boundary, hydration markers.

## The runtime

The emitted calls resolve to `Fable.Ripple.Dom/Hmr.fs`, which the plugin locates among
`PluginHelper.SourceFiles` and imports by relative output path. A project using
`[<Component>]` without referencing `Fable.Ripple.Dom` gets a compile error naming the
missing runtime.

## How a consumer references it

A normal `ProjectReference`, plus Fable's exclude flag naming this project:

```sh
dotnet fable watch App.fsproj --exclude Fable.Ripple.Plugin
```

The flag is documented as "intended for plugin development". It makes Fable run
`dotnet build` on the excluded project and reference the resulting DLL instead of
merging its F# sources, which it cannot compile to JavaScript
(fable-compiler/Fable#3625). The name matched is the project FILE's name without its
extension, which is why this is `Fable.Ripple.Plugin.fsproj`. Feliz wires
`Feliz.CompilerPlugins` up the same way.

Without the flag Fable reports the symptom rather than the cause:
`Cannot reference entity from .dll reference, Fable packages must include F# sources`.

## Constraints worth knowing

- **This is not a Fable package.** It ships a compiled assembly the Fable CLI loads,
  not F# sources for Fable to compile.
- **`FSharp.Core` must be no newer than the one the Fable CLI loads**, 10.0.0.0 for
  Fable 5.13. A newer one throws `Could not load file or assembly 'FSharp.Core'` from
  inside `Transform`, in the *consumer's* build. Older is fine: Feliz pins 8.0.401.
- **`Fable.AST` must match the compiler.** Pinned to 5.0.0 for Fable 5.13.
- The attribute has to sit on each member. A module- or type-level attribute is silently
  ignored, because `ApplyMemberDeclarationPlugin` only reads `memb.Attributes`. So are
  `inline` members and members of `[<AttachMembers>]` types.

## Not in the release list

`build/Commands/Release.fs` names the projects it publishes, and this is not one of
them yet.
