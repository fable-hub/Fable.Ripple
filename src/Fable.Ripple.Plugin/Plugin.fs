namespace Fable.Ripple.Dom

open System.IO

open Fable
open Fable.AST
open Fable.AST.Fable

[<assembly: ScanForPlugins>]
do ()

/// A relative ES module specifier from one output file to another.
module internal Paths =

    let relative (fromFile: string) (toFile: string) =
        let path = Path.GetRelativePath(Path.GetDirectoryName(fromFile: string), toFile)

        // An import specifier is always forward slashes, and a bare `Hmr.fs.js` reads
        // as a package rather than a sibling file.
        let path = path.Replace('\\', '/')

        if path.StartsWith ".." then
            path
        else
            "./" + path

/// Marks a component. The plugin turns it into a value holding an HMR boundary,
/// and emits this module's `import.meta.hot` wiring alongside it.
type ComponentAttribute() =
    inherit MemberDeclarationPluginAttribute()

    override _.FableMinimumVersion = "5.0"

    override _.Transform(helper, _file, decl) =
        if not helper.Options.DebugMode then
            decl
        else

            let runtime =
                helper.SourceFiles
                |> Seq.tryFind (fun file -> file.EndsWith "Fable.Ripple.Dom/Hmr.fs")

            match runtime with
            | None ->
                helper.LogError
                    "[<Component>] needs Fable.Ripple.Dom referenced; its Hmr.fs was not found"

                decl
            | Some runtimeSource ->

                let importPath =
                    Paths.relative (helper.GetOutputPath()) (helper.GetOutputPath runtimeSource)

                /// An identifier can contain a quote or a `$N` the macro would expand, so
                /// names are passed as arguments rather than spliced into one.
                let str (value: string) = Value(StringConstant value, None)

                let import (selector: string) =
                    Import(
                        {
                            Selector = selector
                            Path = importPath
                            Kind = UserImport false
                        },
                        Any,
                        None
                    )

                /// `Fable.Ripple.Var<_>` - a binding whose state is carried across a swap.
                let isVar (t: Type) =
                    match t with
                    | DeclaredType(ent, _) ->
                        try
                            (helper.GetEntity ent).FullName = "Fable.Ripple.Var`1"
                        with _ ->
                            false
                    | _ -> false

                /// Rewrite `let x = <expr : Var<_>>` into `adopt "x" (fun () -> <expr>)`.
                ///
                /// The walk stops at lambdas: a binding inside one runs any number of
                /// times, and every run would collide on that name.
                let rec rewriteVars (e: Expr) =
                    match e with
                    | Let(ident, value, body) ->
                        let value =
                            if isVar ident.Type then
                                Emit(
                                    {
                                        Macro = "$0($1, $2)"
                                        IsStatement = false
                                        CallInfo =
                                            CallInfo.Create(
                                                args =
                                                    [
                                                        import "adopt"
                                                        str ident.Name
                                                        Delegate([], value, None, Tags.empty)
                                                    ]
                                            )
                                    },
                                    value.Type,
                                    value.Range
                                )
                            else
                                rewriteVars value

                        Let(ident, value, rewriteVars body)
                    | Sequential xs -> Sequential(List.map rewriteVars xs)
                    | IfThenElse(cond, thenExpr, elseExpr, r) ->
                        IfThenElse(rewriteVars cond, rewriteVars thenExpr, rewriteVars elseExpr, r)
                    | Lambda _
                    | Delegate _ -> e
                    | e -> e

                let original =
                    Delegate(decl.Args, rewriteVars decl.Body, Some decl.Name, Tags.empty)

                // Vite evaluates the incoming module before the outgoing module's
                // callbacks, so registering in the module body is itself the swap.
                let macro =
                    String.concat
                        "\n"
                        [
                            "(() => {"
                            "  if (import.meta.hot) { import.meta.hot.accept(); }"
                            "  return $0(import.meta.url, $1, $2);"
                            "})()"
                        ]

                let body =
                    Emit(
                        {
                            Macro = macro
                            IsStatement = false
                            CallInfo =
                                CallInfo.Create(
                                    args =
                                        [
                                            import "define"
                                            str decl.Name
                                            original
                                        ]
                                )
                        },
                        decl.Body.Type,
                        None
                    )

                { decl with
                    Args = []
                    Body = body
                    MemberRef = GeneratedMember.Value(decl.Name, decl.Body.Type, isInstance = false)
                }

    override _.TransformCall(_helper, _memb, expr) = expr
