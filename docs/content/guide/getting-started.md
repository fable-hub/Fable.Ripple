---
title: Getting started
---

This page builds a Fable project from nothing and runs a counter in the browser.

## Requirements

- .NET SDK 8.0 or later
- Node.js 20.19 or later

## Set up the project

:::steps

### Create the project

```bash frame="terminal"
dotnet new console -lang F# -o MyApp
cd MyApp
```

### Install Fable

The F# to JavaScript compiler, as a local tool.

```bash frame="terminal"
dotnet new tool-manifest
dotnet tool install fable
```

### Add the packages

They are in beta, so `--prerelease` is required.

```bash frame="terminal"
dotnet add package Fable.Ripple --prerelease
dotnet add package Fable.Ripple.Dom --prerelease
```

### Add Vite

It serves the compiled output during development and bundles it for production.

```bash frame="terminal"
npm init -y
npm install --save-dev vite
```

Set the package type to `module`.

```json title="package.json"
{
    "type": "module"
}
```

### Add the host page

Fable writes a `.js` file next to each `.fs` file, so `Program.fs` becomes `Program.fs.js`.

```html title="index.html"
<!doctype html>
<html lang="en">
    <head>
        <meta charset="utf-8" />
        <title>MyApp</title>
    </head>
    <body>
        <div id="root"></div>
        <script type="module" src="/Program.fs.js"></script>
    </body>
</html>
```

`<div id="root">`{html} is the element the app mounts into. Use any id you like, as long as it matches the one in the next step.

### Replace the contents of `Program.fs`

```fsharp title="Program.fs"
module Program

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

`Html.mount`{fsharp} renders an item into the element with the given id.

### Run it

Fable watches the F# sources and Vite serves the result, so this takes two terminals.

```bash frame="terminal" title="Terminal 1"
dotnet fable watch
```

```bash frame="terminal" title="Terminal 2"
npx vite
```

Open the address Vite prints. Click the button and the number changes. Editing `Program.fs` recompiles it and reloads the page.

:::

## Build for production

```bash frame="terminal"
dotnet fable
npx vite build
```

The bundle is written to `dist/`.

## Generated files

Fable's output sits next to the sources:

```text title=".gitignore"
*.fs.js
```

## Next steps

- [Fable.Ripple](../ripple/introduction.md) - sources, derived signals, effects and batching.
- [Fable.Ripple.Dom](../ripple-dom/introduction.md) - elements, attributes, events and bindings. The examples on those pages compile and run in the page.
- [Fable.UrlParser](../urlparser/introduction.md) - typed routes from URLs.
- [Fable.Ripple.Form](../ripple-form/introduction.md) - typed forms, one Var per field.
