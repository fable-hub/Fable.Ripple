/// <summary>Packs every package and pushes it to nuget.org, in the order a consumer restores them.</summary>
module EasyBuild.Commands.Release

open System
open System.IO
open Spectre.Console.Cli
open EasyBuild.Tools.DotNet
open EasyBuild.Workspace

// Dependency order: a package is pushed after everything it references, so a consumer
// restoring the moment the push lands never sees a dangling dependency.
let private packages =
    [
        Workspace.src.``Fable.Ripple``.``.``
        Workspace.src.``Fable.Ripple.Plugin``.``.``
        Workspace.src.``Fable.Ripple.Dom``.``.``
        Workspace.src.``Fable.Ripple.Dom.Test``.``.``
        Workspace.src.``Fable.UrlParser``.``.``
        Workspace.src.``Fable.Ripple.Form``.``.``
        Workspace.src.``Fable.Ripple.Form.Plain``.``.``
    ]

type ReleaseSettings() =
    inherit CommandSettings()

type ReleaseCommand() =
    inherit Command<ReleaseSettings>()
    interface ICommandLimiter<CommandSettings>

    override _.Execute(_, _, _) =
        let apiKey = Environment.GetEnvironmentVariable "NUGET_KEY"

        if String.IsNullOrWhiteSpace apiKey then
            printfn "NUGET_KEY is not set"
            1
        else
            packages
            |> List.iter (fun package ->
                let nupkg = DotNet.pack package

                DotNet.nugetPush (nupkg, apiKey = apiKey, skipDuplicate = true)

                printfn $"pushed %s{Path.GetFileName package}"
            )

            0
