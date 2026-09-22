---
title: Hot reload
---

Editing a component updates the page in place. The page is not reloaded and the state the component holds is kept.

Mark a component with `[<Component>]`{fsharp} and nothing else changes:

```fsharp title="Counter.fs"
module Counter

open Fable.Ripple
open Fable.Ripple.Dom

[<Component>]
let view () =
    let count = Var.create 0

    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> count.Value <- count.Value + 1)
                    Html.text "Count"
                ]
            Html.output count
        ]
```

Click the button five times, change the label to `"Add"`, save. The button says `Add` and the count still reads 5.

The attribute goes on each component. A module-level or type-level attribute is ignored. A static member works the same way:

```fsharp
type Counter =
    [<Component>]
    static member view() = ...
```

## What is kept

A `Var`{fsharp} bound with `let`{fsharp} directly in the body of a component keeps its value across an edit. How it was produced does not matter, only its type.

Each instance of a component keeps its own state.

Focus, the caret position and the text selection are restored after the update.

## What resets

- A binding you rename.
- A component you move to another file, or rename.
- A binding whose type changes between the two versions.
- A `Var`{fsharp} created inside a lambda, such as one per row of a list.
- A `Var`{fsharp} at module level, outside the component body.

## How an edit reaches the screen

An edit travels through the files that import the one you changed, and stops at the first file containing a `[<Component>]`{fsharp}. Every component in that file is rendered again. So editing a file that several components depend on rebuilds all of them.

If no file in that chain contains a `[<Component>]`{fsharp}, the page reloads. This is what happens when you edit the module that calls `Html.mount`{fsharp}.

## Production builds

`[<Component>]`{fsharp} does nothing outside a debug build. The member compiles exactly as written and no hot reload code is emitted.

```bash frame="terminal"
dotnet fable          # release, no hot reload
dotnet fable watch    # debug, hot reload
```

## Limits

- A component is rendered again in full. There is no diff, so the DOM it produced is replaced.
- Focus is restored by the element's position in the tree. An edit that changes the shape around the focused element loses focus.
- `inline`{fsharp} members and members of a type marked `[<AttachMembers>]`{fsharp} are skipped without a warning. Such a component is compiled as written and does not hot reload.
