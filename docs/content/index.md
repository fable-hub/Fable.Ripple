---
title: Fable.Ripple
description: Focused F# libraries for building reactive Fable applications
layout: bare
---

<style>
    :root {
        --band-max: 1180px;
    }

    .section {
        border-bottom: 1px solid var(--sg-border);
    }

    .section--alt {
        background: var(--sg-bg-subtle);
    }

    .section--hero {
        background-image:
            linear-gradient(var(--sg-grid-line) 1px, transparent 1px),
            linear-gradient(90deg, var(--sg-grid-line) 1px, transparent 1px);
        background-size: 32px 32px;
    }

    .section__inner {
        max-width: var(--band-max);
        margin: 0 auto;
    }

    /* Grid children default to min-width: auto, which lets a code block's
       longest line push the whole page sideways. */
    .hero > *,
    .band > * {
        min-width: 0;
    }

    .hero {
        display: grid;
        grid-template-columns: 1fr 1.16fr;
        gap: 40px;
        align-items: center;
        padding: 72px 48px 56px;
    }

    .hero__eyebrow {
        font-family: var(--nacara-font-mono);
        font-size: 11.5px;
        letter-spacing: 0.1em;
        text-transform: uppercase;
        color: var(--sg-accent);
        margin: 0 0 12px;
    }

    .hero__title {
        margin: 0 0 16px;
        font-size: 44px;
        line-height: 1.06;
        letter-spacing: -0.034em;
    }

    .hero__lede {
        max-width: 38ch;
        font-family: var(--nacara-font-mono);
        font-size: 14.5px;
        line-height: 1.72;
        color: var(--sg-ink-body);
        margin: 0 0 24px;
    }

    .hero__actions {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        margin: 0 0 20px;
    }

    .button {
        display: inline-flex;
        align-items: center;
        font-family: var(--nacara-font-mono);
        font-size: 13px;
        font-weight: 500;
        padding: 9px 16px;
        border-radius: 5px;
        text-decoration: none;
    }

    .button:hover {
        text-decoration: none;
    }

    .button--primary {
        background: var(--sg-accent);
        color: var(--sg-accent-contrast);
        border: 1px solid var(--sg-accent);
        transition: background-color 0.15s, transform 0.15s;
    }

    .button--primary:hover {
        background: var(--sg-accent-hover);
    }

    .hero .button--primary:hover {
        transform: translateY(-1px);
    }

    .button--secondary {
        background: transparent;
        color: var(--sg-ink-body);
        border: 1px solid var(--sg-border);
    }

    .button--secondary:hover {
        border-color: var(--sg-accent);
        color: var(--sg-accent);
    }

    .install-command {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        width: fit-content;
        max-width: 100%;
        overflow-x: auto;
        font-family: var(--nacara-font-mono);
        font-size: 13px;
        border: 1px solid var(--sg-border);
        border-radius: 5px;
        padding: 9px 12px;
        background: var(--sg-bg);
    }

    .install-command > span {
        white-space: nowrap;
    }

    .install-command__cursor {
        flex: 0 0 auto;
    }

    .install-command__prompt {
        color: var(--sg-accent);
    }

    .install-command__cursor {
        width: 7px;
        height: 14px;
        background: var(--sg-accent);
        animation: install-command-blink 1.1s step-end infinite;
    }

    @keyframes install-command-blink {
        0%,
        50% {
            opacity: 1;
        }
        50.01%,
        100% {
            opacity: 0;
        }
    }

    .hero .nacara-code {
        border-radius: 7px;
    }

    .hero .nacara-code__title::before {
        content: "";
        display: inline-block;
        width: 8px;
        height: 8px;
        margin-right: 8px;
        border-radius: 50%;
        background: var(--sg-accent);
    }

    .feature-row {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
    }

    .feature {
        padding: 26px 24px;
        border-left: 1px solid var(--sg-border);
    }

    .feature:first-child {
        border-left: none;
    }

    .feature h3 {
        font-size: 16px;
        margin: 0 0 7px;
    }

    .feature p {
        font-family: var(--sg-font-prose);
        font-size: 1rem;
        line-height: 1.62;
        color: var(--sg-ink-body);
        margin: 0;
    }

    .band {
        display: grid;
        gap: 40px;
        align-items: center;
        padding: 40px 48px;
    }

    .band--code-left {
        grid-template-columns: 1.16fr 1fr;
    }

    .band--code-right {
        grid-template-columns: 1fr 1.16fr;
    }

    .band--code-below {
        grid-template-columns: 1fr;
        align-items: start;
    }

    .band--code-below .band__copy,
    .band--code-below .band__claims li > span {
        max-width: none;
    }

    .band__text .hero__eyebrow {
        font-size: 11.5px;
        margin: 0 0 13px;
    }

    .band__title {
        font-size: 28px;
        line-height: 1.14;
        letter-spacing: -0.03em;
        margin: 0;
    }

    .band__copy {
        max-width: 46ch;
        font-family: var(--sg-font-prose);
        font-size: 1rem;
        line-height: 1.62;
        color: var(--sg-ink-body);
        margin: 15px 0 0;
    }

    .band__claims {
        list-style: none;
        display: flex;
        flex-direction: column;
        gap: 8px;
        font-family: var(--sg-font-prose);
        font-size: 1rem;
        line-height: 1.55;
        color: var(--sg-ink-body);
        margin: 15px 0 0;
        padding: 0;
    }

    .band__claims li {
        display: flex;
        gap: 9px;
    }

    .band__claims li::before {
        content: "\2014";
        flex: 0 0 auto;
        font-size: 13px;
        color: var(--sg-ink-faint);
    }

    .band__claims li > span {
        max-width: 42ch;
    }

    .band__claims strong {
        color: var(--sg-ink);
        font-weight: 600;
    }

    /* Unchipped, per the design addendum: an identifier inside a claim is set
       in accent mono rather than Nacara's boxed inline code. */
    .band__claims code {
        font-family: var(--nacara-font-mono);
        font-size: 12.5px;
        color: var(--sg-accent);
        background: none;
        border: none;
        padding: 0;
    }

    .band__text .hero__actions {
        margin: 19px 0 0;
    }

    .band__text .install-command {
        margin-top: 15px;
    }

    /* Two columns still fit here, but the display sizes authored for a 1040px
       card do not. */
    @media (max-width: 1100px) {
        .hero__title {
            font-size: 36px;
        }

        .band__title {
            font-size: 26px;
        }
    }

    @media (max-width: 860px) {
        .hero {
            grid-template-columns: 1fr;
            padding: 44px 20px 36px;
        }

        .band,
        .band--code-left,
        .band--code-right {
            grid-template-columns: 1fr;
            padding: 36px 20px;
        }

        .band__text {
            order: -1;
        }

        .feature-row {
            grid-template-columns: 1fr;
        }

        .feature {
            padding: 22px 20px;
            border-left: none;
            border-top: 1px solid var(--sg-border);
        }

        .feature:first-child {
            border-top: none;
        }
    }

    @media (max-width: 520px) {
        .hero__title {
            font-size: 30px;
        }

        .hero__actions .button {
            flex: 1 1 auto;
            justify-content: center;
        }
    }
</style>

<section class="section section--hero">
<div class="section__inner hero">
    <div>
        <p class="hero__eyebrow">// Fine-grained reactivity for F#</p>
        <h1 class="hero__title">Reactive values.<br>Nothing else<br>recomputes.</h1>
        <p class="hero__lede">
            Focused F# libraries for reactive Fable applications. Each library does one thing
            and can be used on its own.
        </p>
        <div class="hero__actions">
            <a class="button button--primary" href="/Fable.Ripple/guide/introduction/">Get started</a>
            <a class="button button--secondary" href="https://github.com/fable-hub/Fable.Ripple">GitHub</a>
        </div>
        <div class="install-command">
            <span class="install-command__prompt">&gt;</span>
            <span>dotnet add package Fable.Ripple --prerelease</span>
            <span class="install-command__cursor"></span>
        </div>
    </div>

```fsharp live title="Todos.fs"
open Fable.Ripple

type Todo = { Text: string; Done: bool }

let todos =
    Var.create [
        { Text = "Write the guide"; Done = true }
        { Text = "Ship 1.0"; Done = false }
    ]

let remaining =
    Signal.computed (fun () ->
        todos.Value
        |> List.filter (fun t -> not t.Done)
        |> List.length
    )

printfn "Remaining = %d" remaining.Value

todos.Value <-
    todos.Value
    |> List.map (fun t ->
        if t.Text = "Ship 1.0" then
            printfn "Completed: Ship 1.0"
            { t with Done = true }
        else
            t
    )

printfn "Remaining = %d" remaining.Value
```

</div>
</section>

<section class="section">
<div class="section__inner feature-row">
    <div class="feature">
        <h3>Synchronous</h3>
        <p>A write settles before the next line runs. No scheduler, no tick.</p>
    </div>
    <div class="feature">
        <h3>Glitch-free</h3>
        <p>A node runs once per write, after all its inputs are current.</p>
    </div>
    <div class="feature">
        <h3>Renderer-agnostic</h3>
        <p>The core knows nothing about the DOM. Rendering ships separately.</p>
    </div>
</div>
</section>

<section class="section section--alt">
<div class="section__inner band band--code-left">

```fsharp live preset=app title="Counter.fs"
open Fable.Ripple
open Fable.Ripple.Dom

let counter () =
    let count = Var.create 0

    Html.div
        [
            Html.button
                [
                    on.click (fun _ ->
                        count.Value <- count.Value + 1
                    )
                    Html.text "Count"
                ]
            Html.output count
        ]

Html.mount "app" (counter ())
```

<div class="band__text">
    <p class="hero__eyebrow">// Rendering</p>
    <h2 class="band__title">Signals, bound<br>straight to the DOM.</h2>
    <p class="band__copy">
        <strong>Fable.Ripple.Dom</strong> is an HTML DSL with no virtual tree. A signal knows
        which nodes read it, so a write updates those nodes and nothing else - no diff, no
        re-render of the component around them.
    </p>
    <ul class="band__claims">
        <li><span><strong>One list model</strong> - attributes, events and children are items in the same list.</span></li>
        <li><span><strong>Two-way bindings</strong> - <code>attr.bindValue</code> keeps an input and a <code>Var</code> in sync.</span></li>
        <li><span><strong>Control flow</strong> - <code>Html.show</code> and keyed <code>Html.each</code> add and remove real nodes.</span></li>
    </ul>
    <div class="hero__actions">
        <a class="button button--primary" href="/Fable.Ripple/ripple-dom/introduction/">Read the guide</a>
        <a class="button button--secondary" href="/Fable.Ripple/demo/">Browse the examples</a>
    </div>
    <div class="install-command">
        <span class="install-command__prompt">&gt;</span>
        <span>dotnet add package Fable.Ripple.Dom --prerelease</span>
    </div>
</div>
</div>
</section>

<section class="section">
<div class="section__inner band band--code-right">

<div class="band__text">
    <p class="hero__eyebrow">// Routing</p>
    <h2 class="band__title">URLs parsed into<br>your own types.</h2>
    <p class="band__copy">
        <strong>Fable.UrlParser</strong> turns a URL into a value of your route type. Parsers
        compose left to right, in the order of the URL, and a failure reports the attempt that
        got furthest rather than a blank no-match.
    </p>
    <ul class="band__claims">
        <li><span><strong>Typed segments</strong> - <code>string</code>, <code>int</code>, and your own conversions.</span></li>
        <li><span><strong>Query and fragment</strong> - required, optional, repeated and flag parameters.</span></li>
        <li><span><strong>Two-way codecs</strong> - one description parses a URL and builds it back.</span></li>
    </ul>
    <div class="hero__actions">
        <a class="button button--primary" href="/Fable.Ripple/urlparser/introduction/">Read the guide</a>
        <a class="button button--secondary" href="/Fable.Ripple/demo/">Browse the examples</a>
    </div>
    <div class="install-command">
        <span class="install-command__prompt">&gt;</span>
        <span>dotnet add package Fable.UrlParser --prerelease</span>
    </div>
</div>

```fsharp live title="Routes.fs"
open Fable.UrlParser

type Route =
    | Home
    | Blog of id: int
    | Search of query: string * page: int option

let routes =
    [
        Parser.succeed Home

        Parser.succeed (fun id -> Blog id)
        |> Parser.segment "blog"
        |> Parser.int

        Parser.succeed (fun q page -> Search(q, page))
        |> Parser.segment "search"
        |> Parser.Query.Required.string "q"
        |> Parser.Query.Optional.int "page"
    ]

[ "/blog/42"; "/search?q=signals&page=3"; "/blog/latest" ]
|> List.iter (fun url ->
    Parser.tryParsePath routes url |> printfn "%s -> %A" url
)
```

</div>
</section>

<section class="section section--alt">
<div class="section__inner band band--code-below">

<div class="band__text">
    <p class="hero__eyebrow">// Forms</p>
    <h2 class="band__title">Typed forms,<br>one Var per field.</h2>
    <p class="band__copy">
        <strong>Fable.Ripple.Form</strong> composes fields into a form whose output is your own
        record. A parser turns each value into a typed result, and a parser that reads another
        field re-runs when that field changes. A keystroke writes one Var and touches no node.
    </p>
    <ul class="band__claims">
        <li><span><strong>Combinators</strong> - <code>append</code>, <code>andThen</code>, <code>optional</code>, <code>showIf</code>, <code>list</code>.</span></li>
        <li><span><strong>Validation</strong> - on submit, blur or change; async checks with debounce.</span></li>
        <li><span><strong>Renderers</strong> - Bulma, plain classes, or in-place editing. Or write your own.</span></li>
    </ul>
    <div class="hero__actions">
        <a class="button button--primary" href="/Fable.Ripple/ripple-form/introduction/">Read the guide</a>
        <a class="button button--secondary" href="/Fable.Ripple/demo/">Browse the examples</a>
    </div>
    <div class="install-command">
        <span class="install-command__prompt">&gt;</span>
        <span>dotnet add package Fable.Ripple.Form.Plain --prerelease</span>
    </div>
</div>

```fsharp live preset=app title="SignUp.fs"
open Fable.Ripple
open Fable.Ripple.Dom
open Fable.Ripple.Form
open Fable.Ripple.Form.Plain

let email = Var.create ""
let password = Var.create ""
let repeat = Var.create ""

let form =
    Form.succeed (fun email password _ -> email, password)
    |> Form.append (
        EmailField.create "email"
        |> EmailField.withLabel "Email"
        |> Field.create email (fun value ->
            if value.Contains "@" then Ok value else Error "An email needs an @"
        )
        |> Form.emailField
    )
    |> Form.append (
        PasswordField.create "password"
        |> PasswordField.withLabel "Password"
        |> Field.create password Ok
        |> Form.passwordField
    )
    |> Form.append (
        PasswordField.create "repeat"
        |> PasswordField.withLabel "Repeat password"
        |> Field.create repeat (fun value ->
            if value = password.Value then Ok() else Error "The passwords do not match"
        )
        |> Form.passwordField
    )

let state = Var.create View.Idle

Html.mount
    "app"
    (Form.View.asHtml
        {
            OnSubmit = fun (email, _) -> state.Value <- View.Success $"Welcome, %s{email}."
            State = state
            ErrorVisibility = View.errorVisibility ()
            Action = View.Action.SubmitOnly "Sign up"
            Validation = ValidateOnBlur
        }
        form)
```

</div>
</section>
