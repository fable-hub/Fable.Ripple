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
