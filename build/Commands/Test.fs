module EasyBuild.Commands.Test

open Spectre.Console.Cli
open SimpleExec
open EasyBuild.Workspace
open EasyBuild.Tools.Fable
open EasyBuild.Tools.Npm
open System.ComponentModel
open Microsoft.FSharp.Reflection

type Project =
    | Fable_UrlParser
    | Fable_Ripple
    | Fable_Ripple_Dom
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
        | "ripple-form" -> Fable_Ripple_Form
        | _ -> failwith $"Unknown project value: '%s{value}'"

    member this.ToArgs =
        match this with
        | AllProject -> "all"
        | Fable_UrlParser -> "url-parser"
        | Fable_Ripple -> "ripple"
        | Fable_Ripple_Dom -> "ripple-dom"
        | Fable_Ripple_Form -> "ripple-form"

    member this.Dir =
        match this with
        | AllProject ->
            failwith "All is not real project, it should be captured in the chain before"
        | Fable_UrlParser -> Workspace.tests.``Fable.UrlParser``.``.``
        | Fable_Ripple -> Workspace.tests.``Fable.Ripple``.``.``
        | Fable_Ripple_Dom -> Workspace.tests.``Fable.Ripple.Dom``.``.``
        | Fable_Ripple_Form -> Workspace.tests.``Fable.Ripple.Form``.``.``

type TestSettings() =
    inherit CommandSettings()

    [<CommandArgument(0, "[PROJECT]")>]
    [<Description("""Project to test

Accepted values:
- url-parser
- ripple
- ripple-dom
- ripple-form
- all
    """)>]
    member val Project = "all" with get, set

    [<CommandOption("-w|--watch")>]
    member val IsWatch = false with get, set

/// Returns whether the suite passed.
let private testProject (project: Project) (isWatch: bool) : bool =

    if isWatch then
        Fable.watch (workingDirectory = project.Dir, outDir = "fable-build", runScript = true)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore

        true

    else

        try
            Fable.build (workingDirectory = project.Dir, runScript = true)
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
