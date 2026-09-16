---
title: Performance
---

What a keystroke, a structural change and a first render cost, measured in Chrome on Linux with a probe that counts DOM mutations and times the event handler.

The keystroke numbers are compared with [Fable.Form.Simple](https://mangelmaxime.github.io/Fable.Form/), which has the same field API but keeps the values in an Elmish record and renders the whole form again on every change. The other sections measure Fable.Ripple.Form on its own.

## One keystroke

Typing one character in a text field. Each cell gives the number of DOM nodes touched and the time spent inside the event handler, without layout.

| Form | Elements | Fable.Form.Simple | Fable.Ripple.Form |
| --- | --- | --- | --- |
| Sign up | 37 | 37 nodes, 0.6 ms | 0 nodes, 0.1 ms |
| 10 fields | 54 | 54 nodes, 0.7 ms | 0 nodes, 0.1 ms |
| 50 fields | 254 | 254 nodes, 1.3 ms | 0 nodes, 0.1 ms |
| 200 fields | 1004 | 1004 nodes, 3.5 ms | 0 nodes, 0.1 ms |
| 1000 fields | 5004 | 5004 nodes, 36.1 ms | 0 nodes, 0.1 ms |

A keystroke writes one `Var`{fsharp}. The control already shows the text, so no node changes; what recomputes is the field's parser, its error, and one `append`{fsharp} per level up to the root. That chain costs the same at 10 fields and at 1000.

The times above stop when the handler returns; the browser lays the page out afterwards. Measured again on the 1000-field form with the layout forced inside the handler, so that it is counted:

| 1000 fields, one keystroke | Fable.Form.Simple | Fable.Ripple.Form |
| --- | --- | --- |
| Handler alone | 36.1 ms | 0.1 ms |
| Handler with the layout it causes | 51.6 ms | 0.9 ms |

Laying out 5004 rewritten nodes is the difference between the two rows on the left.

## Structural changes

Fable.Ripple.Form only. Each row is one user action on the demo forms: the nodes added and removed, the attribute and text writes, and the time inside the event handler.

| Action | Nodes added / removed | Attribute writes | Handler time |
| --- | --- | --- | --- |
| Dynamic form: choose the first branch | +8 / 0 | 1 | 0.5 ms |
| Dynamic form: switch to the other branch | +13 / -8 | 0 | 0.4 ms |
| Dynamic form: type in a branch field | 0 / 0 | 0 | 0.1 ms |
| Form list: add an item | +22 / 0 | 0 | 0.7 ms |
| Form list: type in an item | 0 / 0 | 0 | 0.0 ms |
| Form list: remove the first item | 0 / -22 | 0 | 0.2 ms |
| Sign up: submit with 5 errors | 0 / 0 | 9 | 0.6 ms |
| Sign up: type with the errors shown | 0 / 0 | 2, then 0 | 0.1 ms |

Only the branch or the item that changes is built or removed; the rest of the form is not touched. Showing errors writes attributes on the invalid fields and nothing else.

## First render

From the navigation to the end of layout. Mounting costs the same with both libraries; the difference is in the updates above.

| Form | Fable.Form.Simple | Fable.Ripple.Form |
| --- | --- | --- |
| 200 fields | 3.6 to 4.5 ms | 3.6 to 4.5 ms |
| 1000 fields | 18 to 25 ms | 18 to 25 ms |

## Memory

`andThen`{fsharp}, `showIf`{fsharp} and `list`{fsharp} build their subtrees in their own scope and dispose it when the branch or the item goes. The test suite checks that the number of subscriptions on the source `Var`{fsharp}s is the same before and after 50 branch switches and 50 add-and-remove cycles: nothing accumulates, and a form that lives long does not grow.
