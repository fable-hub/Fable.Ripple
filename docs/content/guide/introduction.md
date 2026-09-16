---
title: Introduction
---

Fable.Ripple is a family of F# libraries for building reactive web applications with [Fable](https://fable.io). Each library has a single responsibility:

| Package | What it does |
| --- | --- |
| `Fable.Ripple` | Reactive values. Write a `Var`{fsharp}, everything derived from it updates. |
| `Fable.Ripple.Dom` | HTML DSL bound to signals. A change updates the exact DOM node, nothing else. |
| `Fable.UrlParser` | Composable URL parsing for routes, query strings and fragments. |

They are designed to work together, but each one stands alone. You can use `Fable.Ripple` without the DOM layer, and `Fable.UrlParser` without either.

There is no virtual DOM. A signal knows which DOM nodes read it, so an update touches those nodes directly.

[Getting started](getting-started.md) sets up a project, installs the packages and runs a counter in the browser.
