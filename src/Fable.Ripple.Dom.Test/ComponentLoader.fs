namespace Fable.Ripple.Dom.Test

open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open Glutinum.Playwright

[<RequireQualifiedAccess>]
module ComponentLoader =

    let private fs: obj = importAll "fs"
    let private childProcess: obj = importAll "child_process"

    let private defaultBundlePath: string =
        emitJsExpr () "process.cwd() + '/fable_modules/fable-ripple-dom-test-bundle.iife.js'"

    /// <summary>
    /// Like <c>create</c> but uses a custom bundle path instead of the default
    /// <c>fable_modules/fable-ripple-dom-test-bundle.iife.js</c>.
    /// </summary>
    let createWith
        (componentsFile: string)
        (bundlePath: string)
        : string -> Page -> Promise<Locator>
        =
        let jsFile = componentsFile + ".js"

        childProcess?execSync (
            $"npx esbuild {jsFile} --bundle --format=iife --outfile={bundlePath}",
            {|
                stdio = "inherit"
            |}
        )
        |> ignore

        let bundleJs = fs?readFileSync (bundlePath, "utf8")

        fun (componentName: string) (page: Page) ->
            promise {
                do!
                    page.setContent
                        """
<html>
<body>
    <div id="root"></div>
</body>
</html>
                        """

                do! page.addScriptTag (AddScriptTagOptions(content = bundleJs))

                let! _ = page.evaluate ("window.__signalsMount('" + componentName + "', 'root')")
                ()

                return page.locator "#root"
            }

    /// <summary>
    /// Bundles <c>componentsFile</c> via esbuild into
    /// <c>fable_modules/fable-ripple-dom-test-bundle.iife.js</c>, then returns a
    /// loader function used internally by <c>testComponent</c>.
    /// </summary>
    /// <param name="componentsFile">
    /// The F# source file that registers your components, e.g. <c>"Components.fs"</c>.
    /// Fable must have already compiled it to <c>Components.fs.js</c> in the same directory.
    /// </param>
    let create (componentsFile: string) : string -> Page -> Promise<Locator> =
        createWith componentsFile defaultBundlePath
