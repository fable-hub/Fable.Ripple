module Demo.Sources

open System.Collections.Generic
open Fable.Core
open Fable.Core.JsInterop
open Fable.Ripple
open Demo.Catalogue

(*
    The examples' own F# source, loaded verbatim so the code panel can show it.

    One `import.meta.glob` covers every example, keyed by path, so adding an
    example touches nothing in this file. Three things make it work, and all
    three are fragile enough to be worth stating:

    1. Vite's `?raw` query returns a file as a string. Without `eager` the map
       holds LOADERS rather than contents, so each listing is its own chunk and
       the main bundle carries none of them.

    2. Fable emits the expression verbatim (verified: the generated
       `Sources.fs.js` contains the literal `import.meta.glob(...)` call), and
       Vite transforms it because the emitted JS *is* the module it appears in.
       It has to be `emitJsExpr` - `import.meta` is not expressible in F#.

    3. Fable must be invoked with NO outDir (see build/Commands/Demo.fs). With
       an outDir the relative glob is re-based against the output directory,
       which contains no .fs files, and every listing silently goes missing.
*)

/// Path -> a LOADER for that file, e.g.
/// `"./7GUIs/Counter.fs"` -> `unit -> Promise<"module Demo...">`.
///
/// `eager: false` is the difference between this and the obvious version, and
/// it is worth its complexity: eager inlines all 56 listings into the main
/// bundle, which cost ~90 KB gzipped for text that most visitors never read.
/// Lazily, each becomes its own chunk fetched when its page is opened.
///
/// The price is that `forRoute` is now asynchronous, which is why this file has
/// a cache and a `Var` - see below. That pattern is the one Recipes / Async data
/// spells out, applied to the demo's own plumbing rather than to a fake server.
let private sources: obj =
    emitJsExpr
        ()
        "import.meta.glob(['./7GUIs/**/*.fs', './Examples/**/*.fs'], { query: '?raw', import: 'default' })"

// Scaffolding markers.
//
// An example file does two jobs: it teaches something, and it instruments
// itself so the page can put a number next to every claim. The second job is
// not what the reader came to read - `Repaint()` tokens, `Probe` declarations
// and twenty-line blocks of schematic model that draw a picture of code shown
// elsewhere on the same page.
//
// So the author marks those lines and the listing leaves them out. The markers
// are line comments, which means they are ordinary F# and survive `fantomas`
// (verified in all three positions, including trailing inside a list):
//
//     open Demo.Examples.Widgets // demo-hide-line
//
//     // demo-hide-next-line
//     let repaint = Repaint()
//
//     // demo-hide
//     let aNode = Node.source "Var a" (fun () -> string (a.Peek()))
//     let bNode = Node.source "Var b" (fun () -> string (b.Peek()))
//     // demo-show
//
// Shape borrowed from Fable's own `// fable-disable-line` family (PR #4808),
// which is ESLint's. Deliberately NOT the literate `hide` directive that
// `starlight-fsharp-literate` uses elsewhere in this repo: there it runs until
// the next prose block, and an example file has none - the same token would
// have had to mean two different things in one repository.
//
// Line comments here rather than a block, incidentally, because a block comment
// discussing the literate directives would nest itself shut. F# block comments
// nest, and that is exactly how this file failed to compile the first time.
//
// The markers themselves never reach the listing, in either view. Only the code
// they mark is conditional.
//
// One convention, because the alternative is a heuristic: a BLOCK hides exactly
// what is between its markers, blank lines included. So when the hidden code
// owns the blank line after it - which it usually does at the top of a function
// - put that blank inside the block. The only thing this file does on its own
// is collapse a RUN of blanks into one, which needs no judgement.
//
// Blocks NEST, and a block left open runs to the end of the file. Both matter:
// wrapping a wider region around an already-marked one is the natural way to
// say "and everything below here too", and it would be actively wrong for the
// inner block's `// demo-show` to reopen the outer one.

[<Literal>]
let private HideLine = "// demo-hide-line"

[<Literal>]
let private HideNextLine = "// demo-hide-next-line"

[<Literal>]
let private HideStart = "// demo-hide"

[<Literal>]
let private HideEnd = "// demo-show"

/// Drop leading and trailing blank lines, and collapse a run of them to one.
/// Removing a block that had a blank line either side would otherwise leave a
/// visible hole exactly where the point was to leave nothing.
let private collapseBlanks (lines: string list) =
    lines
    |> List.fold
        (fun acc line ->
            match acc, line.Trim() with
            | [], "" -> acc
            | previous :: _, "" when previous = "" -> acc
            | _ -> line :: acc
        )
        []
    |> List.skipWhile (fun line -> line.Trim() = "")
    |> List.rev

/// The part of a file a reader studies, plus anything wrong with the markers
/// themselves.
type private Split =
    {
        Trimmed: string
        /// Anything that makes the listing not the file. Three kinds, all
        /// silent without this: an unclosed block hides everything below it, a
        /// stray `// demo-show` reopens a block its author did not open, and a
        /// block that ends mid-call leaves the call's arguments stranded.
        Problems: string list
    }

/// The part of a file a reader studies, with the markers and the code they mark
/// removed.
///
/// An own-line marker must match exactly and a trailing one must end the line,
/// which is what stops `WriteProtection.fs` - it embeds F# source, comments and
/// all, inside string literals - from matching by accident.
let private split (code: string) : Split =
    // A STACK of opening line numbers, not a flag. Blocks nest: an author
    // wrapping a wider region around one that is already marked would otherwise
    // have their outer block closed by the inner one's `// demo-show`. Keeping
    // the line numbers rather than a count is what lets an unclosed block name
    // itself below.
    let opened = ResizeArray<int * int>()
    let problems = ResizeArray<string>()
    let mutable skipNext = false
    let mutable lineNumber = 0
    let trimmed = ResizeArray<string>()

    let indentOf (s: string) = s.Length - s.TrimStart().Length

    /// A block that just closed, and where it opened. The line AFTER it decides
    /// whether the block cut a call in half - see the check below.
    let mutable justClosed = None

    for raw in code.Replace("\r\n", "\n").Split('\n') do
        lineNumber <- lineNumber + 1
        let line = raw.Trim()

        if line = HideStart then
            opened.Add(lineNumber, indentOf raw)
        elif line = HideEnd then
            if opened.Count = 0 then
                problems.Add $"line {lineNumber}: `{HideEnd}` closes nothing"
            else
                justClosed <- Some opened.[opened.Count - 1]
                opened.RemoveAt(opened.Count - 1)
        elif line = HideNextLine then
            skipNext <- true
        else
            // A visible line indented deeper than the marker that opened the
            // block means the block ended inside an expression, stranding its
            // arguments - `readout` hidden while its label and thunk stay. The
            // listing then shows F# that would not compile.
            match justClosed with
            | Some(startLine, startIndent) when line <> "" ->
                if opened.Count = 0 && indentOf raw > startIndent then
                    problems.Add
                        $"line {startLine}: block ends mid-expression, so line {lineNumber} is left stranded"

                justClosed <- None
            | _ -> ()

            let trailing = line.EndsWith HideLine

            let cleaned =
                if trailing then
                    raw.Substring(0, raw.LastIndexOf HideLine).TrimEnd()
                else
                    raw

            if not (opened.Count > 0 || skipNext || trailing) then
                trimmed.Add cleaned

            skipNext <- false

    for startLine, _ in opened do
        problems.Add
            $"line {startLine}: `{HideStart}` is never closed, so every line below it is hidden"

    let join (lines: ResizeArray<string>) =
        lines |> List.ofSeq |> collapseBlanks |> String.concat "\n"

    {
        Trimmed = join trimmed
        Problems = List.ofSeq problems
    }

/// A listing, ready to inject.
type Listing =
    {
        Name: string
        /// The lines the panel colours and the copy button copies.
        Text: string
    }

(*
    Loading, and why there is a `Var` in here.

    A listing arrives asynchronously now, so something has to hold "not here
    yet" and then "here". That something is a signal: `ready` maps a path to its
    listing, and every code panel reads it. When a fetch lands, the
    write flushes and the panel that was waiting appears - no callback threading,
    no re-render call, and no way for two panels of the same file to disagree.

    `pending` is a plain `HashSet` rather than a signal: nothing renders from it,
    it exists only to stop a second request for a file already in flight.
*)

let private ready = Var.create Map.empty<string, Listing>
let private pending = HashSet<string>()

let private beginLoad (path: string) (name: string) =
    if not (pending.Contains path) then
        pending.Add path |> ignore

        // A missing key is `undefined`, not `null`, so this is the loose check
        // rather than a `| null ->` pattern.
        let loader: obj = sources?(path)

        if isNullOrUndefined loader then
            Browser.Dom.console.warn (
                "No source for "
                + path
                + " - restart the dev server if that file was added after it started."
            )
        else
            let load: unit -> JS.Promise<string> = unbox loader

            load ()
            |> Promise.map (fun code ->
                // Trailing whitespace is trimmed: every file ends with a newline
                // (.editorconfig enforces it) and the listing would render that as a
                // final blank line, which reads as a stray margin under it.
                let parts = split (code.TrimEnd())

                // The markers are the second table the compiler cannot check -
                // `Router.validate` covers the first. Reported here rather than
                // at startup because listings load lazily, so this fires the
                // moment the broken page is opened.
                for problem in parts.Problems do
                    Browser.Dom.console.error ($"{path} - {problem}")

                let listing =
                    {
                        Name = name
                        Text = parts.Trimmed
                    }

                ready.Value <- Map.add path listing (ready.Peek())
            )
            |> Promise.catch (fun error ->
                Browser.Dom.console.warn ("Could not load " + path + ": " + error.Message)
            )
            |> ignore

/// Put text on the clipboard. Not in Fable's DOM bindings, so one line of
/// interop - the same shape as `performance.now()` in the Recipes examples.
///
/// Guarded, and that is the load-bearing part: `navigator.clipboard` is
/// UNDEFINED outside a secure context - plain HTTP on a LAN address, which is
/// exactly how `--watch` serves with `host: true`. Reading `.writeText` off
/// undefined throws synchronously instead of rejecting, which would sail past
/// a `Promise.catch` and leave the button claiming success. The guard turns
/// every failure into a rejection, so one handler covers them all.
let copyToClipboard (text: string) : JS.Promise<unit> =
    emitJsExpr
        text
        "(navigator.clipboard ? navigator.clipboard.writeText($0) : Promise.reject(new Error('clipboard unavailable')))"

/// Pages whose file is mostly apparatus for the page itself rather than the
/// thing being taught, so a full listing would bury the point rather than make
/// it. They carry their own specimens instead - see `Code.block`.
let private withoutListing = set [ "./Examples/Signals/Builder.fs" ]

/// The listing for a route.
///
/// `None` means either "this route has no listing" (the home page) or "not
/// loaded yet" - and the second resolves itself, because the read of `ready`
/// below subscribes the caller to the load.
let listingForRoute (route: Route) : Listing option =
    entryOf route
    |> Option.filter (fun e -> not (withoutListing.Contains e.Source))
    |> Option.bind (fun e ->
        let name = e.Source.Substring(e.Source.LastIndexOf '/' + 1)

        // A tracked read: when the fetch lands, whoever called this re-runs.
        match Map.tryFind e.Source ready.Value with
        | Some listing -> Some listing
        | None ->
            beginLoad e.Source name
            None
    )

/// The listing to show right now: (file name, source).
let sourceForRoute (route: Route) : (string * string) option =
    listingForRoute route |> Option.map (fun listing -> listing.Name, listing.Text)
