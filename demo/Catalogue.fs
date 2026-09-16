module Demo.Catalogue

open Fable.Ripple.Dom
open Demo.Examples.Routing.UrlState

(*
    Routes and the example catalogue.

    There is exactly ONE table - `catalogue` below. The codecs, the nav, the
    titles and the source listings are all derived from it, so adding an example
    means adding one record and one file. The previous shape kept the route, the
    view, the title and the source import in four parallel places: already 30
    hand-maintained references for 8 examples.

    Section DU names carry an `Example` suffix on purpose - an unsuffixed
    `SevenGUIs` shadowed the `Demo.SevenGUIs` namespace and forced every view to
    be spelled `Demo.SevenGUIs.Counter.render`.

    Kept free of `Browser` and `Fable.UrlParser`: the docs site's .NET build
    touches this module directly, and a construct that reaches for `window`
    there throws.
*)

[<RequireQualifiedAccess>]
type SignalsExample =
    | Sources
    | Derived
    | AutoTracking
    | Combining
    | Builder
    | Effects
    | Sharing
    | Untracked

[<RequireQualifiedAccess>]
type RenderingExample =
    | Elements
    | Attributes
    | Classes
    | Bindings
    | AttributeVsProperty
    | Refs
    | Svg

[<RequireQualifiedAccess>]
type ControlFlowExample =
    | Show
    | Fragments
    | Switch
    | KeyedLists

[<RequireQualifiedAccess>]
type RoutingExample =
    | HashRouting
    | PathParameters
    | QueryStrings
    | ValuesDialect
    /// The one example whose route carries a payload: its whole state lives in
    /// the query string, so the query is part of the route rather than beside it.
    | UrlState of Examples.Routing.UrlState.ProductQuery

[<RequireQualifiedAccess>]
type UnderTheHoodExample =
    | Cutoff
    | WhenItRuns
    | Batching
    | Cleanup

[<RequireQualifiedAccess>]
type RecipeExample =
    | AsyncData
    | ClickToEdit
    | ActiveSearch
    | ModalDialog
    | DragToReorder
    | ManyRows
    | SvgChart
    | Wizard

[<RequireQualifiedAccess>]
type FormsExample =
    | SignUp
    | DynamicForm
    | FormList
    | Conditional
    | Wizard
    | FormState

[<RequireQualifiedAccess>]
type SevenGUIsExample =
    | Counter
    | Temperature
    | FlightBooker
    | Timer
    | Crud
    | CircleDrawer
    | Cells
    | Todos

[<RequireQualifiedAccess>]
type Route =
    | Home
    /// The catalogue: every section, every example, with its one-line summary.
    /// Not an `Entry` itself - it has no source listing and teaches nothing.
    | Examples
    | Signals of SignalsExample
    | Rendering of RenderingExample
    | ControlFlow of ControlFlowExample
    | Routing of RoutingExample
    | UnderTheHood of UnderTheHoodExample
    | Recipes of RecipeExample
    | Forms of FormsExample
    | SevenGUIs of SevenGUIsExample

/// One example: its route, what it teaches, how to render it, and where its
/// source lives (the key into the `import.meta.glob` in Sources.fs).
type Entry =
    {
        Route: Route
        Slug: string
        Title: string
        Description: string
        View: unit -> DomItem
        Source: string
    }

type Section =
    {
        Slug: string
        Title: string
        Entries: Entry list
    }

let private entry route slug title description view source =
    {
        Route = route
        Slug = slug
        Title = title
        Description = description
        View = view
        Source = source
    }

let private signalsSection =
    {
        Slug = "signals"
        Title = "Signals"
        Entries =
            [
                entry
                    (Route.Signals SignalsExample.Sources)
                    "sources"
                    "Sources"
                    "A Var holds a value you can change. A change re-runs only what reads it."
                    Examples.Signals.Sources.render
                    "./Examples/Signals/Sources.fs"

                entry
                    (Route.Signals SignalsExample.Derived)
                    "derived"
                    "Derived values"
                    "A value built from other signals is computed once, however many places read it."
                    Examples.Signals.Derived.render
                    "./Examples/Signals/Derived.fs"

                entry
                    (Route.Signals SignalsExample.Combining)
                    "combining"
                    "Combining signals"
                    "map2 and map3 build one value from several signals."
                    Examples.Signals.Combining.render
                    "./Examples/Signals/Combining.fs"

                entry
                    (Route.Signals SignalsExample.Builder)
                    "builder"
                    // A no-break space separates the braces so the nav never wraps between them.
                    "The signal { } builder"
                    "let! and and! combine signals without nesting maps."
                    Examples.Signals.Builder.render
                    "./Examples/Signals/Builder.fs"

                entry
                    (Route.Signals SignalsExample.AutoTracking)
                    "auto-tracking"
                    "Automatic dependencies"
                    "A computation depends on whatever it read the last time it ran. There is nothing to declare."
                    Examples.Signals.AutoTracking.render
                    "./Examples/Signals/AutoTracking.fs"

                entry
                    (Route.Signals SignalsExample.Untracked)
                    "untracked"
                    "Reading without subscribing"
                    "Peek and Signal.untracked read a value without depending on it."
                    Examples.Signals.Untracked.render
                    "./Examples/Signals/Untracked.fs"

                entry
                    (Route.Signals SignalsExample.Effects)
                    "effects"
                    "Effects"
                    "Run code when signals change, and stop it by disposing it."
                    Examples.Signals.Effects.render
                    "./Examples/Signals/Effects.fs"

                entry
                    (Route.Signals SignalsExample.Sharing)
                    "sharing"
                    "Sharing state"
                    "A Var in a module is shared by every page that reads it."
                    Examples.Signals.Sharing.render
                    "./Examples/Signals/Sharing.fs"
            ]
    }

let private renderingSection =
    {
        Slug = "rendering"
        Title = "Rendering"
        Entries =
            [
                entry
                    (Route.Rendering RenderingExample.Elements)
                    "elements"
                    "Elements and text"
                    "One list per element holds its attributes, events and children."
                    Examples.Rendering.Elements.render
                    "./Examples/Rendering/Elements.fs"

                entry
                    (Route.Rendering RenderingExample.Attributes)
                    "attributes"
                    "Reactive attributes"
                    "An attribute takes a value, a function or a signal, and updates itself."
                    Examples.Rendering.Attributes.render
                    "./Examples/Rendering/Attributes.fs"

                entry
                    (Route.Rendering RenderingExample.Classes)
                    "classes"
                    "Classes"
                    "Toggle one class without touching the others."
                    Examples.Rendering.Classes.render
                    "./Examples/Rendering/Classes.fs"

                entry
                    (Route.Rendering RenderingExample.Bindings)
                    "bindings"
                    "Two-way bindings"
                    "bindValue and bindChecked keep a field and a Var in step."
                    Examples.Rendering.Bindings.render
                    "./Examples/Rendering/Bindings.fs"

                entry
                    (Route.Rendering RenderingExample.AttributeVsProperty)
                    "attribute-vs-property"
                    "Attribute vs property"
                    "Why a value set as an attribute stops updating once the user types."
                    Examples.Rendering.AttributeVsProperty.render
                    "./Examples/Rendering/AttributeVsProperty.fs"

                entry
                    (Route.Rendering RenderingExample.Refs)
                    "refs"
                    "Refs"
                    "attr.ref gets the element before it is on the page, so focusing and measuring wait a tick."
                    Examples.Rendering.Refs.render
                    "./Examples/Rendering/Refs.fs"

                entry
                    (Route.Rendering RenderingExample.Svg)
                    "svg"
                    "SVG"
                    "SVG elements mix with HTML in the same list."
                    Examples.Rendering.SvgBasics.render
                    "./Examples/Rendering/SvgBasics.fs"
            ]
    }

let private controlFlowSection =
    {
        Slug = "control-flow"
        Title = "Control Flow"
        Entries =
            [
                entry
                    (Route.ControlFlow ControlFlowExample.Show)
                    "show"
                    "Html.show"
                    "A branch is built when it appears and destroyed when it goes."
                    Examples.ControlFlow.Show.render
                    "./Examples/ControlFlow/Show.fs"

                entry
                    (Route.ControlFlow ControlFlowExample.Switch)
                    "switch"
                    "Html.switch"
                    "Switch views on one signal without losing what the user typed."
                    Examples.ControlFlow.Switch.render
                    "./Examples/ControlFlow/Switch.fs"

                entry
                    (Route.ControlFlow ControlFlowExample.KeyedLists)
                    "keyed-lists"
                    "Keyed lists"
                    "Rows are moved, not rebuilt, so each one keeps its own state."
                    Examples.ControlFlow.KeyedLists.render
                    "./Examples/ControlFlow/KeyedLists.fs"

                entry
                    (Route.ControlFlow ControlFlowExample.Fragments)
                    "fragments"
                    "Fragments"
                    "Add several children without a wrapper element, and adopt nodes built elsewhere."
                    Examples.ControlFlow.Fragments.render
                    "./Examples/ControlFlow/Fragments.fs"
            ]
    }

let private routingSection =
    {
        Slug = "routing"
        Title = "Routing and URLs"
        Entries =
            [
                entry
                    (Route.Routing RoutingExample.HashRouting)
                    "hash-routing"
                    "Hash routing"
                    "One list of codecs reads the address bar and builds every link."
                    Examples.Routing.HashRouting.render
                    "./Examples/Routing/HashRouting.fs"

                entry
                    (Route.Routing RoutingExample.PathParameters)
                    "path-parameters"
                    "Path parameters"
                    "Typed segments, and errors that say which part did not match."
                    Examples.Routing.PathParameters.render
                    "./Examples/Routing/PathParameters.fs"

                entry
                    (Route.Routing RoutingExample.QueryStrings)
                    "query-strings"
                    "Query strings"
                    "Required, optional, flag and repeated query parameters."
                    Examples.Routing.QueryStrings.render
                    "./Examples/Routing/QueryStrings.fs"

                entry
                    (Route.Routing RoutingExample.ValuesDialect)
                    "values-dialect"
                    "The Values dialect"
                    "Route codecs built from named fields instead of positions."
                    Examples.Routing.ValuesDialect.render
                    "./Examples/Routing/ValuesDialect.fs"

                entry
                    (Route.Routing(
                        RoutingExample.UrlState Examples.Routing.UrlState.ProductQuery.empty
                    ))
                    "url-state"
                    "State in the URL"
                    "Search, page and sort live in the query string, so reload and Back just work."
                    Examples.Routing.UrlState.render
                    "./Examples/Routing/UrlState.fs"
            ]
    }

let private underTheHoodSection =
    {
        Slug = "under-the-hood"
        Title = "Under the Hood"
        Entries =
            [
                entry
                    (Route.UnderTheHood UnderTheHoodExample.WhenItRuns)
                    "when-it-runs"
                    "When computations run"
                    "A derived value waits for its first read, and a write applies immediately."
                    Examples.UnderTheHood.WhenItRuns.render
                    "./Examples/UnderTheHood/WhenItRuns.fs"

                entry
                    (Route.UnderTheHood UnderTheHoodExample.Batching)
                    "batching"
                    "Batching"
                    "Several writes, one update: automatic in event handlers, Signal.batch everywhere else."
                    Examples.UnderTheHood.Batching.render
                    "./Examples/UnderTheHood/Batching.fs"

                entry
                    (Route.UnderTheHood UnderTheHoodExample.Cutoff)
                    "cutoff"
                    "Equality cutoff"
                    "When a value recomputes to the same result, nothing after it runs."
                    Examples.UnderTheHood.Cutoff.render
                    "./Examples/UnderTheHood/Cutoff.fs"

                entry
                    (Route.UnderTheHood UnderTheHoodExample.Cleanup)
                    "cleanup"
                    "Cleanup is automatic"
                    "A timer started in a row stops when the row goes."
                    Examples.UnderTheHood.Cleanup.render
                    "./Examples/UnderTheHood/Cleanup.fs"
            ]
    }

let private recipesSection =
    {
        Slug = "recipes"
        Title = "Recipes"
        Entries =
            [
                entry
                    (Route.Recipes RecipeExample.AsyncData)
                    "async-data"
                    "Async data"
                    "Loading, success and error states, and ignoring responses that arrive too late."
                    Examples.Recipes.AsyncData.render
                    "./Examples/Recipes/AsyncData.fs"

                entry
                    (Route.Recipes RecipeExample.ActiveSearch)
                    "active-search"
                    "Active search"
                    "Search as you type, with a hand-written debounce."
                    Examples.Recipes.ActiveSearch.render
                    "./Examples/Recipes/ActiveSearch.fs"

                entry
                    (Route.Recipes RecipeExample.ClickToEdit)
                    "click-to-edit"
                    "Click to edit"
                    "Show a value, edit it in place, then save or cancel."
                    Examples.Recipes.ClickToEdit.render
                    "./Examples/Recipes/ClickToEdit.fs"

                entry
                    (Route.Recipes RecipeExample.ModalDialog)
                    "modal-dialog"
                    "Modal dialog"
                    "The native dialog element, with Escape and focus return built in."
                    Examples.Recipes.ModalDialog.render
                    "./Examples/Recipes/ModalDialog.fs"

                entry
                    (Route.Recipes RecipeExample.Wizard)
                    "wizard"
                    "Multi-step form"
                    "Steps, validation, and answers that survive going back."
                    Examples.Recipes.Wizard.render
                    "./Examples/Recipes/Wizard.fs"

                entry
                    (Route.Recipes RecipeExample.DragToReorder)
                    "drag-to-reorder"
                    "Drag to reorder"
                    "Drag rows in a keyed list, and their state moves with them."
                    Examples.Recipes.DragToReorder.render
                    "./Examples/Recipes/DragToReorder.fs"

                entry
                    (Route.Recipes RecipeExample.SvgChart)
                    "svg-chart"
                    "SVG chart"
                    "Bars, a shared scale and a tooltip, all derived from the data."
                    Examples.Recipes.SvgChart.render
                    "./Examples/Recipes/SvgChart.fs"

                entry
                    (Route.Recipes RecipeExample.ManyRows)
                    "many-rows"
                    "1,000 and 10,000 rows"
                    "The js-framework-benchmark operations, timed on the page."
                    Examples.Recipes.ManyRows.render
                    "./Examples/Recipes/ManyRows.fs"
            ]
    }

let private formsSection =
    {
        Slug = "forms"
        Title = "Forms"
        Entries =
            [
                entry
                    (Route.Forms FormsExample.SignUp)
                    "sign-up"
                    "Sign up"
                    "One Var per field, typed output, a cross-field check and an async one."
                    Examples.Forms.SignUp.render
                    "./Examples/Forms/SignUp.fs"

                entry
                    (Route.Forms FormsExample.DynamicForm)
                    "dynamic-form"
                    "Dynamic form"
                    "andThen builds the rest of the form from a field's value."
                    Examples.Forms.DynamicForm.render
                    "./Examples/Forms/DynamicForm.fs"

                entry
                    (Route.Forms FormsExample.FormList)
                    "form-list"
                    "Form list"
                    "A list of sub-forms: add, remove, keyed rows that keep their text."
                    Examples.Forms.FormList.render
                    "./Examples/Forms/FormList.fs"

                entry
                    (Route.Forms FormsExample.Conditional)
                    "conditional"
                    "Conditional fields"
                    "showIf and disableIf follow a signal; a hidden field keeps its value."
                    Examples.Forms.Conditional.render
                    "./Examples/Forms/Conditional.fs"

                entry
                    (Route.Forms FormsExample.Wizard)
                    "wizard"
                    "Wizard"
                    "Typed steps, one of them conditional, validated one at a time."
                    Examples.Forms.Wizard.render
                    "./Examples/Forms/Wizard.fs"

                entry
                    (Route.Forms FormsExample.FormState)
                    "form-state"
                    "Dirty, reset, commit, load"
                    "Every field remembers its baseline; the buttons follow isDirty."
                    Examples.Forms.FormState.render
                    "./Examples/Forms/FormState.fs"
            ]
    }

let private sevenGUIsSection =
    {
        Slug = "seven-guis"
        Title = "7GUIs"
        Entries =
            [
                entry
                    (Route.SevenGUIs SevenGUIsExample.Counter)
                    "counter"
                    "Counter"
                    "The smallest reactive app: a value and a button that changes it."
                    SevenGUIs.Counter.render
                    "./7GUIs/Counter.fs"

                entry
                    (Route.SevenGUIs SevenGUIsExample.Temperature)
                    "temperature"
                    "Temperature Converter"
                    "Two fields that convert into each other, each validating its own input."
                    SevenGUIs.TemperatureConverter.render
                    "./7GUIs/TemperatureConverter.fs"

                entry
                    (Route.SevenGUIs SevenGUIsExample.FlightBooker)
                    "flight-booker"
                    "Flight Booker"
                    "The form decides which flights can be booked."
                    SevenGUIs.FlightBooker.render
                    "./7GUIs/FlightBooker.fs"

                entry
                    (Route.SevenGUIs SevenGUIsExample.Timer)
                    "timer"
                    "Timer"
                    "A progress gauge driven by a timer that stops when you leave."
                    SevenGUIs.Timer.render
                    "./7GUIs/Timer.fs"

                entry
                    (Route.SevenGUIs SevenGUIsExample.Crud)
                    "crud"
                    "CRUD"
                    "Create, update and delete people in a filtered list."
                    SevenGUIs.Crud.render
                    "./7GUIs/Crud.fs"

                entry
                    (Route.SevenGUIs SevenGUIsExample.CircleDrawer)
                    "circle-drawer"
                    "Circle Drawer"
                    "Undo and redo, hover selection, a right-click menu and a dialog."
                    SevenGUIs.CircleDrawer.render
                    "./7GUIs/CircleDrawer.fs"

                entry
                    (Route.SevenGUIs SevenGUIsExample.Cells)
                    "cells"
                    "Cells"
                    "A 100 by 26 spreadsheet where every formula is a derived value."
                    SevenGUIs.Cells.render
                    "./7GUIs/Cells.fs"

                entry
                    (Route.SevenGUIs SevenGUIsExample.Todos)
                    "todos"
                    "Todos"
                    "TodoMVC: add, edit, complete and filter todos."
                    SevenGUIs.Todos.render
                    "./7GUIs/Todos.fs"
            ]
    }

let catalogue: Section list =
    [
        signalsSection
        renderingSection
        controlFlowSection
        routingSection
        underTheHoodSection
        recipesSection
        formsSection
        sevenGUIsSection
    ]

let all: Entry list = catalogue |> List.collect (fun s -> s.Entries)

/// Where `#/` sends the reader. Derived rather than named, so reordering the
/// catalogue moves the front door with it.
let firstRoute: Route =
    (List.head catalogue).Entries |> List.head |> (fun e -> e.Route)

/// A route with any per-example payload stripped back to its default, so it can
/// be looked up in the catalogue. Only the URL-state example carries one: its
/// query string is part of its route, and every distinct query would otherwise
/// be a route the catalogue has never heard of.
let canonical (route: Route) : Route =
    match route with
    | Route.Routing(RoutingExample.UrlState _) ->
        Route.Routing(RoutingExample.UrlState Examples.Routing.UrlState.ProductQuery.empty)
    | other -> other

let entryOf (route: Route) : Entry option =
    let key = canonical route
    all |> List.tryFind (fun e -> e.Route = key)

let sectionOf (route: Route) : Section option =
    let key = canonical route

    catalogue
    |> List.tryFind (fun s -> s.Entries |> List.exists (fun e -> e.Route = key))
