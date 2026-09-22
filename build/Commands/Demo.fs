module EasyBuild.Commands.Demo

open Spectre.Console.Cli
open SimpleExec
open EasyBuild.Workspace
open System
open System.IO
open BlackFox.CommandLine
open EasyBuild.Tools.DotNet
open EasyBuild.Tools.Vite
open EasyBuild.Tools.Fable
open EasyBuild.Tools.Npm

type DemoSettings() =
    inherit CommandSettings()

    [<CommandOption("-w|--watch")>]
    member val IsWatch = false with get, set

type DemoCommand() =
    inherit Command<DemoSettings>()
    interface ICommandLimiter<DemoSettings>

    override __.Execute(context, settings, ct) =

        Npm.install ()

        if settings.IsWatch then
            [
                Fable.watch (
                    workingDirectory = Workspace.demo.``.``,
                    verbose = true,
                    exclude = [ "Fable.Ripple.Plugin" ]
                )
                Vite.watch (workingDirectory = Workspace.demo.``.``)
            ]
            |> List.map Async.AwaitTask
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore

        else
            // No outDir, deliberately. demo/Sources.fs imports the demo sources
            // through Vite's `?raw` suffix, and those relative specifiers only
            // resolve while Fable emits next to the sources - an outDir would
            // re-base them into the output directory, where no .fs files exist.
            Fable.build (
                workingDirectory = Workspace.demo.``.``,
                exclude = [ "Fable.Ripple.Plugin" ]
            )

            Vite.build (workingDirectory = Workspace.demo.``.``)

        0
