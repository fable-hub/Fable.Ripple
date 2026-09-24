# Fable.Ripple

Fine-grained reactive libraries for [Fable](https://fable.io) applications.

A `Var` holds a value. Everything derived from it updates when it changes, and in the browser an update touches only the DOM nodes that read it. There is no virtual DOM.

Documentation: <https://fable-hub.github.io/Fable.Ripple/>

## Packages

| Package | Description |
| --- | --- |
| [Fable.Ripple](https://www.nuget.org/packages/Fable.Ripple) | Reactive values: sources, derived signals, effects and batching. |
| [Fable.Ripple.Dom](https://www.nuget.org/packages/Fable.Ripple.Dom) | HTML and SVG elements, attributes and events bound to signals. |
| [Fable.UrlParser](https://www.nuget.org/packages/Fable.UrlParser) | Typed URL parsing and building for paths, query strings and fragments. |
| [Fable.Ripple.Form](https://www.nuget.org/packages/Fable.Ripple.Form) | Typed forms on Fable.Ripple.Dom: the form model, combinators, validation, wizards. No rendering opinion. |
| [Fable.Ripple.Form.Plain](https://www.nuget.org/packages/Fable.Ripple.Form.Plain) | The default renderer: field kinds, form list, views. `rf-*` classes, no CSS framework. |
| [Fable.Ripple.Dom.Test](https://www.nuget.org/packages/Fable.Ripple.Dom.Test) | Component tests for Fable.Ripple.Dom, run in Chromium through Playwright. |

`Fable.Ripple` works without the DOM layer, and `Fable.UrlParser` works without either. `Fable.Ripple.Form` needs one renderer package.

## Installation

The packages are in beta, so `--prerelease` is required.

```bash
dotnet add package Fable.Ripple --prerelease
dotnet add package Fable.Ripple.Dom --prerelease
```

## Example

```fsharp
open Fable.Ripple
open Fable.Ripple.Dom

let count = Var.create 0

let view =
    Html.div
        [
            Html.button
                [
                    on.click (fun _ -> count.Value <- count.Value + 1)
                    Html.text "Count"
                ]
            Html.output count
        ]

Html.mount "root" view
```

[Getting started](https://fable-hub.github.io/Fable.Ripple/guide/getting-started/) sets up the project, the host page and Vite.

## Development

`build.sh` (or `build.bat`) is the entry point.

| Command | Description |
| --- | --- |
| `./build.sh test` | Run every test suite. `./build.sh test ripple-dom` or `./build.sh test ripple-form` runs one. |
| `./build.sh docs watch` | Serve the documentation site. The demo under `/demo/` is built once, when it starts. |
| `./build.sh docs check` | Build every page and fail on any error. |

## License

MIT
