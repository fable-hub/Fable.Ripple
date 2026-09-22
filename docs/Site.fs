module Docs.Site

open System
open System.IO
open Feliz.ViewEngine
open Nacara.Core
open Nacara.Plugins
open Nacara.Theme
open Demo.Catalogue

let apiOptions =
    { FSharpApi.defaults with
        Root = "reference"
        Title = "API reference"
        Sources =
            [
                let beside =
                    Reflection.Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName

                for name in
                    [
                        "Fable.Ripple"
                        "Fable.Ripple.Dom"
                        "Fable.UrlParser"
                        "Fable.Ripple.Form"
                        "Fable.Ripple.Form.Plain"
                    ] -> FSharpApiSource.create (Path.Combine(beside, $"%s{name}.dll"))
            ]
    }

let changelogs =
    [
        ChangelogSource.create "Fable.Ripple" "../src/Fable.Ripple/CHANGELOG.md"
        ChangelogSource.create "Fable.Ripple.Dom" "../src/Fable.Ripple.Dom/CHANGELOG.md"
        ChangelogSource.create "Fable.UrlParser" "../src/Fable.UrlParser/CHANGELOG.md"
        ChangelogSource.create "Fable.Ripple.Form" "../src/Fable.Ripple.Form/CHANGELOG.md"
        ChangelogSource.create
            "Fable.Ripple.Form.Plain"
            "../src/Fable.Ripple.Form.Plain/CHANGELOG.md"
    ]

// `MenuLink` renders its url as written, with no base url prepended - unlike
// `Menu.page`, which resolves against a real content page. Every entry here
// needs the site's own base url spelled out, matching /demo/'s embedded CSS
// and script tags below.
// Nested one level deeper than it looks like it needs to be: the theme renders a
// `MenuSection` flat at depth 0 and as a collapsible `<details>` below it.
let private demoMenu =
    [
        Menu.link "All examples" "/Fable.Ripple/demo/#/examples"

        Menu.section
            "Examples"
            (catalogue
             |> List.map (fun section ->
                 Menu.section
                     section.Title
                     (section.Entries
                      |> List.map (fun e ->
                          Menu.link e.Title $"/Fable.Ripple/demo/#/{section.Slug}/{e.Slug}"
                      ))
             ))
    ]

let theme =
    Theme.defaults
    |> Theme.headExtra
        [
            Html.link
                [
                    prop.rel "stylesheet"
                    prop.href "/Fable.Ripple/theme.css"
                ]
        ]
    |> Theme.navbar
        [
            NavbarSection("Guide", "guide", "/guide/introduction/")
            NavbarDropdown(
                "Packages",
                [
                    NavbarDescribed(
                        "Fable.Ripple",
                        "Reactive values and derived signals.",
                        "/ripple/introduction/"
                    )
                    NavbarDescribed(
                        "Fable.Ripple.Dom",
                        "HTML DSL with no virtual tree.",
                        "/ripple-dom/introduction/"
                    )
                    NavbarDescribed(
                        "Fable.UrlParser",
                        "URLs parsed into your own types.",
                        "/urlparser/introduction/"
                    )
                    NavbarDescribed(
                        "Fable.Ripple.Form",
                        "Typed forms, one Var per field.",
                        "/ripple-form/introduction/"
                    )
                ]
            )
            NavbarDivider
            NavbarSection("Reference", "reference", "/reference/")
            NavbarSection("Changelog", "changelog", "/changelog/fable-ripple/")
            NavbarSection("Demos", "demo", "/demo/")
        ]
    |> Theme.menu
        "guide"
        [
            Menu.section
                "Guide"
                [
                    Menu.page "guide/introduction.md"
                    Menu.page "guide/getting-started.md"
                ]
        ]
    |> Theme.menu
        "ripple"
        [
            Menu.section "Getting started" [ Menu.page "ripple/introduction.md" ]
            Menu.section
                "Reactivity"
                [
                    Menu.page "ripple/sources.md"
                    Menu.page "ripple/derived.md"
                    Menu.page "ripple/effects.md"
                    Menu.page "ripple/batching.md"
                    Menu.page "ripple/cutoff.md"
                ]
        ]
    |> Theme.menu
        "ripple-dom"
        [
            Menu.section "Getting started" [ Menu.page "ripple-dom/introduction.md" ]
            Menu.section
                "Rendering"
                [
                    Menu.page "ripple-dom/elements.md"
                    Menu.page "ripple-dom/events.md"
                    Menu.page "ripple-dom/bindings.md"
                    Menu.page "ripple-dom/control-flow.md"
                    Menu.page "ripple-dom/svg.md"
                ]
            // Menu.section "Patterns" [ Menu.page "ripple-dom/async-data.md" ]
            Menu.section "Navigation" [ Menu.page "ripple-dom/routing.md" ]
            Menu.section "Development" [ Menu.page "ripple-dom/hot-reload.md" ]
            Menu.section "Testing" [ Menu.page "ripple-dom/testing.md" ]
            Menu.section "Under the hood" [ Menu.page "ripple-dom/performance.md" ]
        ]
    |> Theme.menu
        "urlparser"
        [
            Menu.section "Getting started" [ Menu.page "urlparser/introduction.md" ]
            Menu.section
                "Parsing"
                [
                    Menu.page "urlparser/paths.md"
                    Menu.page "urlparser/query-parameters.md"
                    Menu.page "urlparser/fragments-and-hash-routing.md"
                ]
            Menu.section "Building URLs" [ Menu.page "urlparser/two-way-codecs.md" ]
        ]
    |> Theme.menu
        "ripple-form"
        [
            Menu.section
                "Getting started"
                [
                    Menu.page "ripple-form/introduction.md"
                    Menu.page "ripple-form/first-form.md"
                ]
            Menu.section
                "Guides"
                [
                    Menu.page "ripple-form/fields.md"
                    Menu.page "ripple-form/validation.md"
                    Menu.page "ripple-form/dynamic-forms.md"
                    Menu.page "ripple-form/lists.md"
                    Menu.page "ripple-form/wizards.md"
                    Menu.page "ripple-form/saving.md"
                    Menu.page "ripple-form/styling.md"
                ]
            Menu.section
                "Extending"
                [
                    Menu.page "ripple-form/custom-field.md"
                    Menu.page "ripple-form/custom-view.md"
                    Menu.page "ripple-form/custom-renderer.md"
                ]
            Menu.section "Under the hood" [ Menu.page "ripple-form/performance.md" ]
        ]
    |> Theme.menu "demo" demoMenu
    |> Theme.navbarEnd
        [
            NavbarDynamicWidget Search.trigger
            NavbarIcon("GitHub", "https://github.com/fable-hub/Fable.Ripple", Icons.github)
        ]
    |> Theme.editUrl "https://github.com/fable-hub/Fable.Ripple/edit/main/docs"
    |> Theme.footer (
        Html.p
            [
                Html.text "Built with "
                Html.a
                    [
                        prop.href "https://mangelmaxime.github.io/Nacara/"
                        prop.text "Nacara"
                    ]
            ]
    )

let reference =
    FSharpApi.collection "reference" DocFrontMatter.decoder apiOptions
    |> Collection.title _.Title
    |> Collection.layout (Theme.layout theme)

let changelog =
    Changelog.collection "changelog" DocFrontMatter.decoder changelogs
    |> Collection.title _.Title
    |> Collection.routePrefix "changelog"
    |> Collection.layout (Theme.layout theme)

let site =
    Site.create "Fable.Ripple"
    |> Site.origin "https://fable-hub.github.io"
    |> Site.baseUrl "/Fable.Ripple/"
    |> Site.output "output"
    |> Site.staticFiles "static"
    |> Markdown.register
    |> TreeSitter.registerWith (TreeSitter.browser [ "fsharp" ])
    |> Changelog.registerWith "changelog" changelogs
    |> FSharpApi.register apiOptions
    |> Search.register
    |> Sitemap.register
    |> LightningCss.register
    |> Nuglify.minifyHtml
    |> Nuglify.minifyJs
    |> LiveExample.registerWith (
        LiveExample.preset (
            LiveExamplePreset.create "console"
            |> LiveExamplePreset.project "snippets/Snippets.fsproj"
            |> LiveExamplePreset.asDefault
        )
        >> LiveExample.stats true
        >> LiveExample.highlighting TreeSitterHighlighting
        >> LiveExample.preset (
            LiveExamplePreset.create "app"
            |> LiveExamplePreset.template "snippets/app.html"
            |> LiveExamplePreset.css "snippets/app.css"
        )
    )
    |> GitHubPages.register
    |> Theme.register theme
    |> Site.collection (Theme.docs theme "content")
    |> Site.collection reference
    |> Site.collection changelog

[<EntryPoint>]
let main argv = Nacara.run site argv
