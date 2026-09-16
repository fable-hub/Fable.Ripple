module EasyBuild.Main

open Spectre.Console.Cli
open EasyBuild.Commands.Demo
open EasyBuild.Commands.Test
open EasyBuild.Commands.Bench
open EasyBuild.Commands.Docs
open EasyBuild.Commands.Release
open SimpleExec

[<EntryPoint>]
let main args =

    if System.Environment.GetEnvironmentVariable("ACT") = null then
        Command.Run("dotnet", "husky install")

    let app = CommandApp()

    app.Configure(fun config ->
        config.Settings.ApplicationName <- "./build.sh"

        config
            .AddCommand<DemoCommand>("demo")
            .WithDescription("Run the demo")
            .WithExample("demo")
            .WithExample("demo --watch")
        |> ignore

        config
            .AddCommand<TestCommand>("test")
            .WithDescription("Run the tests")
            .WithExample("test")
            .WithExample("test --watch")
        |> ignore

        config
            .AddCommand<BenchCommand>("bench")
            .WithDescription("Measure bundle size + large-list performance")
            .WithExample("bench")
        |> ignore

        config
            .AddCommand<ReleaseCommand>("release")
            .WithDescription("Pack every package and push it to nuget.org")
            .WithExample("release")
        |> ignore

        config.AddBranch(
            "docs",
            fun (docs: IConfigurator<CommandSettings>) ->
                docs.SetDescription "Write and build the documentation site"

                docs
                    .AddCommand<WatchCommand>("watch")
                    .WithDescription("Serve it, rebuilding as you write")
                    .WithExample("docs watch")
                    .WithExample("docs watch --host")
                |> ignore

                docs
                    .AddCommand<BuildCommand>("build")
                    .WithDescription("Build it into docs/output")
                    .WithExample("docs build")
                |> ignore

                docs
                    .AddCommand<CheckCommand>("check")
                    .WithDescription("Build it all, write none of it, fail on anything wrong")
                    .WithExample("docs check")
                |> ignore

                docs
                    .AddCommand<CleanCommand>("clean")
                    .WithDescription("Remove what a build wrote")
                    .WithExample("docs clean")
                |> ignore

                docs
                    .AddCommand<DeployCommand>("deploy")
                    .WithDescription("Publish the last build to the gh-pages branch")
                    .WithExample("docs deploy --dry-run")
                    .WithExample("docs deploy")
                |> ignore
        )
        |> ignore
    )

    app.Run(args)
