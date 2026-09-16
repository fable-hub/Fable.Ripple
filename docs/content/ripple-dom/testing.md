---
title: Testing
---

`Fable.Ripple.Dom.Test` runs component tests in Chromium through [Playwright](https://playwright.dev). Each test mounts one component on a fresh page and asserts against the rendered DOM.

Tests are written with [Scriptorium.Quill](https://www.nuget.org/packages/Scriptorium.Quill) and asserted with [Scriptorium.Nib.Browser](https://www.nuget.org/packages/Scriptorium.Nib.Browser). Both come with the package.

## Requirements

- Fable installed as a local tool. See [Getting started](../guide/getting-started.md).
- Node.js 20.19 or later.

## Set up a test project

:::steps

### Create the project and add the package

```bash frame="terminal"
dotnet new console -lang F# -o MyApp.Tests
cd MyApp.Tests
dotnet add package Fable.Ripple.Dom.Test --prerelease
```

Add a reference to the project that holds your components.

```bash frame="terminal"
dotnet add reference ../MyApp/MyApp.fsproj
```

### Install Playwright, esbuild and Chromium

```bash frame="terminal"
npm init -y
npm install --save-dev playwright esbuild
npx playwright install chromium
```

Set the package type to `module`.

```json title="package.json"
{
    "type": "module"
}
```

### Register the components in `Components.fs`

```fsharp title="Components.fs"
module MyApp.Tests.Components

open Fable.Ripple
open Fable.Ripple.Dom
open type Fable.Ripple.Dom.Test.RippleRegistry

let counter () : DomItem =
    let count = Var.create 0

    Html.div
        [
            Html.div [ Html.text count ]
            Html.button
                [
                    on.click (fun _ -> count.Value <- count.Value + 1)
                    Html.text "Count"
                ]
        ]

register ("Counter", counter)
```

### Write the tests in `Main.fs`

```fsharp title="Main.fs"
module MyApp.Tests.Main

open Scriptorium.Nib.Browser
open type Scriptorium.Nib.Browser.UserEvents
open type Fable.Ripple.Dom.Test.RippleDomTest
open type Scriptorium.Quill.Runner
open type Scriptorium.Quill.Test

do setup "Components.fs"

[<EntryPoint>]
let main _ =
    runTests (
        testList (
            "Counter",
            [
                testComponent (
                    "increments when Count is clicked",
                    "Counter",
                    fun root ->
                        promise {
                            do! click (root.locator "button")
                            do! assertLocator (root.locator "div > div") (haveText "1")
                        }
                )
            ]
        )
    )
```

### Update the project file

Replace `Program.fs` with `Components.fs` and `Main.fs`, in that order.

```xml title="MyApp.Tests.fsproj"
<ItemGroup>
    <Compile Include="Components.fs" />
    <Compile Include="Main.fs" />
</ItemGroup>
```

### Run the tests

From the test project folder.

```bash frame="terminal"
dotnet fable --runScript
```

The process exits with a non-zero code when a test fails.

:::

`Components.fs` runs in the browser and `Main.fs` runs in Node.js. Refer to components by name only: code in `Main.fs` that uses `Components.fs` loads it into Node.js, where there is no `window`.

## Registering components

`register` takes a name and a `unit -> DomItem` factory. The factory runs once per test, so every test starts from new state.

## Writing tests

`testComponent` mounts the component inside `<div id="root">` and passes that element's locator to the test. The second overload also passes the Playwright `Page`. `page.evaluate` needs `open Glutinum.Playwright`.

```fsharp
testComponent (
    "reads the title",
    "Counter",
    fun root page ->
        promise {
            let! title = page.evaluate "document.title"
            ()
        }
)
```

Assertions retry until they pass or 5 seconds elapse. That is also the test timeout, so a failing assertion is reported as `Test timed out after 5000ms`.

## Assertions

Chain assertions with `>>.` and pass them to `assertLocator`. `not'` inverts one.

| Assertion | Passes when the element |
| --- | --- |
| `haveText text` | has exactly this text |
| `containText text` | contains this text |
| `haveValue value` | is a field with this value |
| `haveAttribute name value` | has this attribute value |
| `haveClass classes` | has exactly this `class` attribute |
| `containClass classes` | has all of these classes |
| `haveCSS property value` | has this computed style |
| `haveCount n` | matches exactly `n` elements |
| `toBeVisible` / `beHidden` | is visible / hidden |
| `beChecked` | is a checked checkbox or radio |
| `beEnabled` / `beDisabled` | is enabled / disabled |
| `beFocused` | has focus |
| `beEditable` | is editable |
| `beEmpty` | has no content |

## User events

`UserEvents` acts on a locator: `click`, `fill`, `press`, `check`, `uncheck`, `selectOption`, `focus`, `blur` and `hover`.

## Reference

### `RippleDomTest.setup`

**type:** `componentsFile: string -> unit`

Bundles the compiled `componentsFile` with esbuild into `fable_modules/fable-ripple-dom-test-bundle.iife.js`. Call it once, before the tests run. Fable must already have compiled the file next to its source.

### `RippleDomTest.testComponent`

**type:** `name: string * componentName: string * body: (Locator -> Promise<unit>) -> TestCase`

Runs `body` against the component registered as `componentName`, in a new headless Chromium page.

### `RippleDomTest.ftestComponent`

Focused variant of `testComponent`. When any focused test exists, only focused tests run.

### `RippleDomTest.xtestComponent`

Skipped variant of `testComponent`.

### `RippleDomTest.dtestComponent`

Debug variant of `testComponent`. Pauses the test in the Playwright inspector.

### `RippleRegistry.register`

**type:** `name: string * view: (unit -> DomItem) -> unit`

Registers a component under `name`.

### `ComponentLoader.createWith`

**type:** `componentsFile: string -> bundlePath: string -> (string -> Page -> Promise<Locator>)`

Bundles `componentsFile` to `bundlePath` and returns the loader that mounts a component by name. `setup` calls it with the default bundle path.
