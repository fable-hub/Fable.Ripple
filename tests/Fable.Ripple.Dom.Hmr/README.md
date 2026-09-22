# Fable.Ripple.Dom.Hmr.Tests

Acceptance tests for `[<Component>]` hot reload, driving a real `fable watch` and Vite dev server 
and editing `.fs` files on disk.

## Run it

```sh
./build.sh test ripple-dom-hmr
```

It starts vite and `fable watch`, waits for them, runs `test.mjs`, then stops them.

## What is where

| path | what |
| --- | --- |
| `src/Counter.fs` | a plain component, no HMR code |
| `src/Widgets.fs` | a component taking arguments (label + handler) |
| `src/Table.fs` | a keyed list, for the cost measurement |
| `src/Pair.fs` | one component rendered twice, inside another component |
| `src/Rows.fs` | a component as an `each` row, and inside `Html.dynamic` |
| `src/Shared.fs` | a dependency, for testing propagation |
| `test.mjs` | 40 assertions |
| `measure.mjs` | what always-swapping costs |

Editing a shared dependency rebuilds every component downstream of it. `measure.mjs`
reports the cost: 69 ms for a 5000-row table with reactive rows, per save, against a
~1.5 s Fable recompile.

## Known gaps

- A component built inside a dynamic region gets a fresh identity on every rebuild of
  that region. `place` runs in whatever frame is current, and a dynamic re-render
  happens outside the enclosing boundary's build pass.
- `inline` members and `[<AttachMembers>]` members are skipped with no diagnostic.
  `ApplyMemberDeclarationPlugin` is never called for them, so the plugin has no hook
  to warn from.

The inspector built on this boundary is parked in `../../experiments/devtools`; it
needs library changes that are in `git stash`. Nothing here depends on it.
