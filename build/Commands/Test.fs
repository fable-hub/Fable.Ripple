module EasyBuild.Commands.Test

open Spectre.Console.Cli
open SimpleExec
open EasyBuild.Workspace
open EasyBuild
open EasyBuild.Tools.Fable
open EasyBuild.Tools.Npm
open System
open System.Diagnostics
open System.ComponentModel
open System.Threading
open Microsoft.FSharp.Reflection

type Project =
    | Fable_UrlParser
    | Fable_Ripple
    | Fable_Ripple_Dom
    | Fable_Ripple_Dom_Hmr
    | Fable_Ripple_Form
    | AllProject

    static member All =
        FSharpType.GetUnionCases(typeof<Project>)
        |> Array.map (fun case -> FSharpValue.MakeUnion(case, [||]) :?> Project)
        |> Array.filter (fun du -> du <> AllProject)

    static member fromString(value: string) =
        match value.ToLowerInvariant() with
        | "all" -> AllProject
        | "url-parser" -> Fable_UrlParser
        | "ripple" -> Fable_Ripple
        | "ripple-dom" -> Fable_Ripple_Dom
        | "ripple-dom-hmr" -> Fable_Ripple_Dom_Hmr
        | "ripple-form" -> Fable_Ripple_Form
        | _ -> failwith $"Unknown project value: '%s{value}'"

    member this.ToArgs =
        match this with
        | AllProject -> "all"
        | Fable_UrlParser -> "url-parser"
        | Fable_Ripple -> "ripple"
        | Fable_Ripple_Dom -> "ripple-dom"
        | Fable_Ripple_Dom_Hmr -> "ripple-dom-hmr"
        | Fable_Ripple_Form -> "ripple-form"

    member this.Dir =
        match this with
        | AllProject ->
            failwith "All is not real project, it should be captured in the chain before"
        | Fable_UrlParser -> Workspace.tests.``Fable.UrlParser``.``.``
        | Fable_Ripple -> Workspace.tests.``Fable.Ripple``.``.``
        | Fable_Ripple_Dom -> Workspace.tests.``Fable.Ripple.Dom``.``.``
        | Fable_Ripple_Dom_Hmr -> Workspace.tests.``Fable.Ripple.Dom.Hmr``.``.``
        | Fable_Ripple_Form -> Workspace.tests.``Fable.Ripple.Form``.``.``

type TestSettings() =
    inherit CommandSettings()

    [<CommandArgument(0, "[PROJECT]")>]
    [<Description("""Project to test

Accepted values:
- url-parser
- ripple
- ripple-dom
- ripple-dom-hmr
- ripple-form
- all
    """)>]
    member val Project = "all" with get, set

    [<CommandOption("-w|--watch")>]
    member val IsWatch = false with get, set

/// The HMR suite needs `fable watch` and vite running at the same time, and edits
/// `.fs` files on disk to trigger a recompile.
let private runHmrSuite (dir: string) : bool =
    let port = "5401"

    let start (exe: string) (args: string) (onLine: string -> unit) =
        let info = ProcessStartInfo(exe, args)
        info.WorkingDirectory <- dir
        info.RedirectStandardOutput <- true
        info.RedirectStandardError <- true
        info.UseShellExecute <- false
        info.Environment.["PORT"] <- port

        let proc = new Process(StartInfo = info)

        let handle (e: DataReceivedEventArgs) =
            if not (isNull e.Data) then
                onLine e.Data

        proc.OutputDataReceived.Add handle
        proc.ErrorDataReceived.Add handle
        proc.Start() |> ignore
        proc.BeginOutputReadLine()
        proc.BeginErrorReadLine()
        proc

    let mutable watching = false

    let vite = start Utils.npx $"vite --port %s{port} --strictPort" ignore

    let fable =
        start
            "dotnet"
            "fable watch Fable.Ripple.Dom.Hmr.Tests.fsproj --exclude Fable.Ripple.Plugin"
            (fun line ->
                if line.Contains "Watching" then
                    watching <- true
            )

    try
        let deadline = DateTime.UtcNow.AddMinutes 3.0

        while not watching && not fable.HasExited && DateTime.UtcNow < deadline do
            Thread.Sleep 250

        if not watching then
            printfn "fable watch did not reach 'Watching'"
            false
        else
            let test = start "node" "test.mjs" (printfn "%s")
            test.WaitForExit()
            test.ExitCode = 0

    finally
        for proc in
            [
                fable
                vite
            ] do
            try
                proc.Kill true
            with _ ->
                ()

/// Returns whether the suite passed.
let private testProject (project: Project) (isWatch: bool) : bool =

    if project = Fable_Ripple_Dom_Hmr then
        runHmrSuite project.Dir

    elif isWatch then
        Fable.watch (
            workingDirectory = project.Dir,
            outDir = "fable-build",
            runScript = true,
            exclude = [ "Fable.Ripple.Plugin" ]
        )
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore

        true

    else

        try
            Fable.build (
                workingDirectory = project.Dir,
                runScript = true,
                exclude = [ "Fable.Ripple.Plugin" ]
            )

            true

        with :? ExitCodeException ->
            printfn
                $"""Error while testing %A{project}

Run the following command to test that combination in isolation:

./build.sh test %s{project.ToArgs}
"""

            false

type TestCommand() =
    inherit Command<TestSettings>()
    interface ICommandLimiter<TestSettings>

    override __.Execute(context, settings, ct) =
        let projectArg = Project.fromString settings.Project

        Npm.install ()

        let results =
            match projectArg with
            | AllProject ->
                Project.All
                |> Array.map (fun project -> project, testProject project settings.IsWatch)
            | project -> [| project, testProject project settings.IsWatch |]

        let failed = results |> Array.filter (snd >> not) |> Array.map fst

        if Array.isEmpty failed then
            0
        else
            printfn $"Failed: %A{failed}"
            1
