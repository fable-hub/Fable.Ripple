module EasyBuild.Commands.Bench

open Spectre.Console.Cli
open SimpleExec
open EasyBuild.Workspace
open EasyBuild.Tools.Fable
open EasyBuild.Tools.Npm

type BenchSettings() =
    inherit CommandSettings()

type BenchCommand() =
    inherit Command<BenchSettings>()
    interface ICommandLimiter<BenchSettings>

    override _.Execute(context, settings, ct) =
        Npm.install ()

        let benchDir = root + "bench"

        // Compile bench/*.fs -> *.fs.js. No runScript: BenchApi.fs touches
        // window/document, which would crash under Node.
        Fable.build (workingDirectory = benchDir)

        // Both scripts chdir to the repo root internally and drive esbuild.
        Command.Run("node", "bench/size.mjs", workingDirectory = root)
        Command.Run("node", "bench/measure.mjs", workingDirectory = root)

        0
