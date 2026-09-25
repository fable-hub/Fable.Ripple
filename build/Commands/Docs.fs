module EasyBuild.Commands.Docs

open System.ComponentModel
open System.IO
open Spectre.Console.Cli
open SimpleExec
open BlackFox.CommandLine
open EasyBuild.Tools.Fable
open EasyBuild.Tools.Npm
open EasyBuild.Workspace
open EasyBuild

type DocsSettings() =
    inherit CommandSettings()

    [<CommandOption("-p|--port <PORT>")>]
    [<Description("The port to serve on.")>]
    member val Port = 0 with get, set

// Options the command did not recognise are forwarded to the site as written.
let private forwarded (context: CommandContext) =
    let options =
        context.Remaining.Parsed
        |> Seq.collect (fun option ->
            [
                for value in option do
                    option.Key

                    if not (isNull value) then
                        value
            ]
        )

    let positionals =
        context.Remaining.Raw
        |> Seq.filter (fun argument -> not (argument.StartsWith "-"))

    Seq.append options positionals

let private compileVisuals () =
    // The Pages deploy commits `docs/output`, where a generated `.gitignore` would exclude these.
    Command.Run(
        "dotnet",
        "fable docs/visuals/Visuals.fsproj --outDir docs/static/visuals --noGitignore --exclude Fable.Ripple.Plugin",
        workingDirectory = Workspace.``.``
    )

    let out = Path.Combine(Workspace.``.``, "docs", "static", "visuals")
    File.Copy(Workspace.demo.styles.``schematic.css``, Path.Combine(out, "schematic.css"), true)
    File.Copy(Workspace.docs.visuals.``visuals.css``, Path.Combine(out, "visuals.css"), true)

let rec private copyTree (source: string) (destination: string) =
    Directory.CreateDirectory destination |> ignore

    for file in Directory.GetFiles source do
        File.Copy(file, Path.Combine(destination, Path.GetFileName file), true)

    for directory in Directory.GetDirectories source do
        copyTree directory (Path.Combine(destination, Path.GetFileName directory))

// The live-example plugin inlines one stylesheet, so the form package's css is appended to the snippets' own.
//
// The destination comes from VirtualWorkspace: it is generated and git-ignored, so a fresh
// checkout has no file for the real workspace to expose.
let private writeSnippetCss () =
    File.WriteAllText(
        VirtualWorkspace.docs.snippets.``app.css``,
        File.ReadAllText Workspace.docs.snippets.``app.base.css``
        + "\n"
        + File.ReadAllText(
            Path.Combine(Workspace.src.``Fable.Ripple.Form.Plain``.``.``, "assets", "plain.css")
        )
    )

let private compileDemo () =
    Npm.install ()
    Fable.build (workingDirectory = Workspace.demo.``.``, exclude = [ "Fable.Ripple.Plugin" ])

    // The site's base flows into the demo's asset URLs; the demo's own commands
    // keep serving it from '/'.
    Command.Run(
        Utils.npx,
        "vite build --base=/Fable.Ripple/demo/",
        workingDirectory = Workspace.demo.``.``
    )

    let target = Path.Combine(Workspace.``.``, "docs", "static", "demo")

    if Directory.Exists target then
        Directory.Delete(target, true)

    copyTree (Path.Combine(Workspace.demo.``.``, "dist")) target
    File.Delete(Path.Combine(target, "index.html"))

let private site (command: string) (watch: bool) (settings: DocsSettings) (extra: string seq) =
    let before =
        if watch then
            // The site's own watcher only re-renders content; dotnet watch restarts the
            // program when Site.fs changes. Without --no-hot-reload it patches in place
            // instead of restarting.
            [
                "watch"
                "--no-hot-reload"
            ]
        else
            [ "run" ]

    let arguments =
        before
        |> List.fold (fun line argument -> CmdLine.appendRaw argument line) CmdLine.empty
        |> CmdLine.appendPrefix "--project" Workspace.docs.``Docs.fsproj``
        |> CmdLine.appendRaw "--"
        |> CmdLine.appendRaw command
        |> fun line ->
            if settings.Port > 0 then
                CmdLine.appendPrefix "--port" (string settings.Port) line
            else
                line
        |> fun line -> extra |> Seq.fold (fun line argument -> CmdLine.appendRaw argument line) line
        |> CmdLine.toString

    Command.Run("dotnet", arguments, workingDirectory = Workspace.``.``)
    0

type BuildCommand() =
    inherit Command<DocsSettings>()
    interface ICommandLimiter<CommandSettings>

    override _.Execute(context, settings, _) =
        writeSnippetCss ()
        compileDemo ()
        compileVisuals ()
        site "build" false settings (forwarded context)

type CheckCommand() =
    inherit Command<DocsSettings>()
    interface ICommandLimiter<CommandSettings>

    override _.Execute(context, settings, _) =
        writeSnippetCss ()
        compileDemo ()
        compileVisuals ()
        site "check" false settings (forwarded context)

type CleanCommand() =
    inherit Command<DocsSettings>()
    interface ICommandLimiter<CommandSettings>

    override _.Execute(context, settings, _) =
        site "clean" false settings (forwarded context)

type DeployCommand() =
    inherit Command<DocsSettings>()
    interface ICommandLimiter<CommandSettings>

    override _.Execute(context, settings, _) =
        site "gh-pages" false settings (forwarded context)

type WatchSettings() =
    inherit DocsSettings()

    [<CommandOption("--host [HOST]")>]
    [<Description("Listen on an address other than localhost. On its own, every interface.")>]
    member val Host = FlagValue<string>() with get, set

    [<CommandOption("--no-restart")>]
    [<Description("Serve without rebuilding the site when its own code changes.")>]
    member val NoRestart = false with get, set

type WatchCommand() =
    inherit Command<WatchSettings>()
    interface ICommandLimiter<CommandSettings>

    override _.Execute(context, settings, _) =
        writeSnippetCss ()
        compileDemo ()
        compileVisuals ()

        let host =
            [
                if settings.Host.IsSet then
                    "--host"

                    if not (isNull settings.Host.Value) then
                        settings.Host.Value
            ]

        site "watch" (not settings.NoRestart) settings (Seq.append host (forwarded context))
