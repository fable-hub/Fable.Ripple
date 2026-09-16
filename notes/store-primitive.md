# Idea: a Store primitive in Fable.Ripple

Status: to think about. Written 2026-09-15 during the Fable.Form on Fable.Ripple.Dom prototype (`prototypes/Fable.Form/`, since moved to `src/Fable.Ripple.Form`).

## The need

The form prototype keeps one `Var` per field. That is what makes updates cost nothing, but the form never sees a plain record of its values. Reset and dirty tracking work field by field. Loading a draft or taking a snapshot needs a mapping the user writes by hand (`FormValues<'Values>`, `Form.load`, `Form.snapshot`): one line per field, kept in sync manually.

Fable.Form does not have the problem because its model is a plain record. An Elmish adapter for the new form needs the same thing: a record that lives in the Elmish state and produces the `Var`s.

## The idea

A `Store<'T>` where `'T` is a record and every field is reachable as a `Var`:

```fsharp
let store = Store.create { Email = ""; Name = ""; MakePublic = None }
let email: Var<string> = store.Field(fun v -> v.Email)      // shape to decide
store.Set draft                                             // writes every field Var, one batch
let current: 'T = store.Snapshot()                         // or store.Value as a tracked read
```

Solid's `createStore` is the reference: fine-grained per field, one object for the whole state, `setStore` with a path or a whole value. It uses JS proxies. Options in F#/Fable:

- Reflection over the record type at runtime. Fable keeps record field names, so `Store.create` could build one `Var` per field and `Snapshot` could rebuild the record. Nested records and lists need a decision (nested stores, or plain values).
- A generated companion (source generator or a type provider) producing the record of `Var`s and the two mappings.
- Lenses (`Var.lens get set`) over one `Var<'T>`. Cheapest to write, but every keystroke rebuilds the record and re-evaluates every lens getter: O(fields) signal work per keystroke, even if the DOM stays untouched thanks to the cutoff.

## What it would give

- `Form.load` and `Form.snapshot` without a hand-written mapping.
- An Elmish adapter: the store is the model, `OnChange` is a subscription on it.
- The same for any settings page or editor, not only forms.

## Open questions

- Nested records and lists: nested stores, or leaves only.
- Identity for list items (the form list keys items by reference today).
- Equality and cutoff at the record level versus the field level.
- How it relates to `Var.createWith`, `Signal.computed` and scopes.

See `prototypes/Fable.Form/STEP-2-FINDINGS.md`, section "Proposed library additions", for the context.
