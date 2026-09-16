namespace Fable.Ripple.Dom.Test

open System.Runtime.CompilerServices
open Fable.Core.JS
open Glutinum.Playwright
open Scriptorium.Nib.Browser
open Scriptorium.Quill

(*
    State - holds the loader function configured by RippleDomTest.setup.

    setup calls State.configure once, which builds the esbuild bundle and stores
    the resulting loader. Each testComponent call then invokes State.load to
    inject the bundle into a fresh Playwright page and return the root locator.
*)
module private State =

    let mutable private loader: (string -> Page -> Promise<Locator>) option = None

    let configure (componentsFile: string) =
        loader <- Some(ComponentLoader.create componentsFile)

    let load (componentName: string) (page: Page) : Promise<Locator> =
        match loader with
        | Some f -> f componentName page
        | None -> failwith "Call RippleDomTest.setup before using testComponent"

type RippleDomTest =

    /// <summary>
    /// Bundle <c>componentsFile</c> via esbuild and prepare the component loader.
    /// Must be called once before any <c>testComponent</c> calls, typically at
    /// module level in Main.fs.
    /// </summary>
    /// <param name="componentsFile">
    /// The F# source file that registers your components, e.g. <c>"Components.fs"</c>.
    /// Fable must have already compiled it to <c>Components.fs.js</c> in the same directory.
    /// </param>
    static member setup(componentsFile: string) = State.configure componentsFile

    /// <summary>
    /// Mount <c>componentName</c> in a fresh Playwright page and pass the root
    /// <c>Locator</c> to <c>body</c>.
    /// </summary>
    static member testComponent
        (
            name: string,
            componentName: string,
            body: Locator -> Promise<unit>,
            [<CallerFilePath>] ?filePath: string,
            [<CallerLineNumber>] ?lineNumber: int
        )
        : TestCase
        =
        BrowserTest.testPage (
            name,
            (fun page ->
                promise {
                    let! root = State.load componentName page
                    do! body root
                }
            ),
            ?filePath = filePath,
            ?lineNumber = lineNumber
        )

    /// <summary>
    /// Mount <c>componentName</c> in a fresh Playwright page and pass both the root
    /// <c>Locator</c> and the <c>Page</c> to <c>body</c>.
    /// </summary>
    static member testComponent
        (
            name: string,
            componentName: string,
            body: Locator -> Page -> Promise<unit>,
            [<CallerFilePath>] ?filePath: string,
            [<CallerLineNumber>] ?lineNumber: int
        )
        : TestCase
        =
        BrowserTest.testPage (
            name,
            (fun page ->
                promise {
                    let! root = State.load componentName page
                    do! body root page
                }
            ),
            ?filePath = filePath,
            ?lineNumber = lineNumber
        )

    /// <summary>Skipped variant of <c>testComponent</c>.</summary>
    static member xtestComponent
        (
            name: string,
            componentName: string,
            body: Locator -> Promise<unit>,
            [<CallerFilePath>] ?filePath: string,
            [<CallerLineNumber>] ?lineNumber: int
        )
        : TestCase
        =
        BrowserTest.xtestPage (
            name,
            (fun page ->
                promise {
                    let! root = State.load componentName page
                    do! body root
                }
            ),
            ?filePath = filePath,
            ?lineNumber = lineNumber
        )

    /// <summary>Skipped variant of <c>testComponent</c> (with page).</summary>
    static member xtestComponent
        (
            name: string,
            componentName: string,
            body: Locator -> Page -> Promise<unit>,
            [<CallerFilePath>] ?filePath: string,
            [<CallerLineNumber>] ?lineNumber: int
        )
        : TestCase
        =
        BrowserTest.xtestPage (
            name,
            (fun page ->
                promise {
                    let! root = State.load componentName page
                    do! body root page
                }
            ),
            ?filePath = filePath,
            ?lineNumber = lineNumber
        )

    /// <summary>Focused variant: only <c>ftest*</c> cases run.</summary>
    static member ftestComponent
        (
            name: string,
            componentName: string,
            body: Locator -> Promise<unit>,
            [<CallerFilePath>] ?filePath: string,
            [<CallerLineNumber>] ?lineNumber: int
        )
        : TestCase
        =
        BrowserTest.ftestPage (
            name,
            (fun page ->
                promise {
                    let! root = State.load componentName page
                    do! body root
                }
            ),
            ?filePath = filePath,
            ?lineNumber = lineNumber
        )

    /// <summary>Focused variant (with page).</summary>
    static member ftestComponent
        (
            name: string,
            componentName: string,
            body: Locator -> Page -> Promise<unit>,
            [<CallerFilePath>] ?filePath: string,
            [<CallerLineNumber>] ?lineNumber: int
        )
        : TestCase
        =
        BrowserTest.ftestPage (
            name,
            (fun page ->
                promise {
                    let! root = State.load componentName page
                    do! body root page
                }
            ),
            ?filePath = filePath,
            ?lineNumber = lineNumber
        )

    /// <summary>Debug variant: pauses the Playwright inspector on this test.</summary>
    static member dtestComponent
        (
            name: string,
            componentName: string,
            body: Locator -> Promise<unit>,
            [<CallerFilePath>] ?filePath: string,
            [<CallerLineNumber>] ?lineNumber: int
        )
        : TestCase
        =
        BrowserTest.dtestPage (
            name,
            (fun page ->
                promise {
                    let! root = State.load componentName page
                    do! body root
                }
            ),
            ?filePath = filePath,
            ?lineNumber = lineNumber
        )

    /// <summary>Debug variant (with page).</summary>
    static member dtestComponent
        (
            name: string,
            componentName: string,
            body: Locator -> Page -> Promise<unit>,
            [<CallerFilePath>] ?filePath: string,
            [<CallerLineNumber>] ?lineNumber: int
        )
        : TestCase
        =
        BrowserTest.dtestPage (
            name,
            (fun page ->
                promise {
                    let! root = State.load componentName page
                    do! body root page
                }
            ),
            ?filePath = filePath,
            ?lineNumber = lineNumber
        )
