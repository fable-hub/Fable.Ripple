# Handoff: Fable.UI docs site redesign

Untracked scratch file for handing this session's context to another Claude Code instance.
Delete it when the work lands.

Repo: `/home/mmangel/Workspaces/Github/fable-hub/Fable.UI/main`
Branch: `main`, last commit `a6cc04d feat(docs): drive the demo's sidebar from Nacara's own menu`

## What the task is

Applying a supplied visual design ("Signal Grid", direction 2a) to the Nacara-built docs
site, and bringing the embedded demo app in line with it. Nothing is committed yet beyond
`a6cc04d`.

## Design source of truth

Two zips at repo root. **`Fable.Signals documentation UI(1).zip` supersedes the other** -
read its `README.md`, which is authoritative for colours, type and spacing. Key points:

- Three families, and the split is called "the most important rule in this document":
  - **IBM Plex Mono** - the interface: nav, sidebar, breadcrumbs, eyebrows, labels, code,
    signature blocks, install command, and the hero lede.
  - **IBM Plex Sans** - running prose only: article paragraphs, member descriptions,
    callouts, feature-card copy, prev/next titles. This is the change from the first zip,
    which had prose in mono and was rejected as unreadable.
  - **Space Grotesk 700** - headings and member/node names only.
  - The theme must default to Mono and *scope* Sans to the article content region.
- Accent `#3b45ff` light / `#6b73ff` fill + `#8a90ff` as-text in dark. Neutrals
  `#ffffff`/`#fafafb`/`#0b0b0d`/`#3a3a46`/`#62626e`/`#e4e4e8`.
- Measures: article prose `66ch`, member descriptions and callouts `68ch`, hero lede `38ch`.
- Grid backdrop appears in **exactly two places**: hero (32px) and graph container (24px).
- The API reference page in the mockup has **invented content** (fake `since` tags, fake
  members). Treat it as a layout template only. The user explicitly said not to follow it
  literally - adapt to the real data the FSharpApi plugin produces.
- Responsive behaviour is explicitly **not designed**; anything there is our own work.

## Hard-won rules (learned by being corrected - please respect these)

1. **`theme.css` holds only genuine deltas from Nacara's defaults.** Do not restate a
   property whose default already resolves correctly through a remapped variable. The user
   called this out directly ("only override the minimum needs of rules"). Read the real
   default CSS before adding anything - it is on disk at
   `/home/mmangel/Workspaces/Github/MangelMaxime/Nacara/main/src/Nacara.Theme.Default/assets/css/*.css`
   plus per-plugin CSS under `Nacara.Plugin.Search/assets/`, `Nacara.Plugin.FSharpApi/assets/`,
   `Nacara.Plugin.LiveExample/css/`. Several bugs came from styling against the theme CSS
   only and missing a plugin's own stylesheet.
2. **Stop fighting font-size.** Nacara's own type scale is sensible; earlier overrides made
   the sidebar/nav *smaller* than default. Also do not apply a value outside the context it
   was measured for - a blanket `letter-spacing: -0.02em` meant for a 44px hero headline was
   crushing 13px `h4`-`h6`.
3. **Never use a shorthand that clobbers a sibling modifier class.** `.nacara-api__signature
   { padding: … }` wiped out `.nacara-api__signature--hanging`'s compensating `padding-left`
   and clipped "module Dom" to "nodule Dom".
4. **Check font weights exist before switching a family.** Space Grotesk is self-hosted at
   700 only, so pointing `body` at it made every paragraph bold. IBM Plex Sans is a variable
   file covering 100-700, which is why it is safe for prose.
5. **Syntax colours: leave Nacara's defaults alone.** The user asked for the stock palette;
   there are no `--tok-*` overrides in `theme.css` and there should not be.
6. **Keep the demo and the docs site consistent now** - an earlier decision to keep the demo
   visually separate was reversed once the design handoff existed.
7. Comments: a pre-commit hook enforces CLAUDE.md's rule. Only external constraints or
   non-obvious invariants, one line, stated as fact. It will reject rationale.
8. Prose in replies: write `-`, never an em dash. A stop hook enforces it.
9. Commits: no `Co-Authored-By` trailer (hook enforces), Conventional Commits, only when asked.

## Work done, not yet committed

`docs/static/` (all new, untracked):
- `theme.css` - 175 lines. `@font-face` for all three families, `--sg-*` design tokens
  (light + `:root[data-theme="dark"]`), remaps of Nacara's `--nacara-*` variables, mono
  default on `body`, Sans scoped to `.nacara-content` **and** `.nacara-navbar`/`.nacara-sidebar`
  (the user asked for nav/menu in Sans too), Space Grotesk on headings + navbar brand, plus
  a handful of real structural deltas (callout flat wash, measures, prev/next weight) and
  the navbar overflow fix described below.
- `fonts/` - self-hosted woff2: Plex Mono 400/500/600, Plex Sans variable (copied from
  `demo/public/fonts`), Space Grotesk 700 (instanced from the Google Fonts variable TTF with
  `fonttools varLib.instancer` + woff2 compress), with both licence files.
- `demo/` and `visuals/` - build outputs, produced by `build.sh docs build`.

`docs/Site.fs` - links `theme.css` via `Theme.headExtra`; `Theme.menu "demo"` built from
`Demo.Catalogue.catalogue`; `LiveExample.highlighting TreeSitterHighlighting`; footer links
to Nacara.

`docs/content/index.md` - the landing page. `layout: bare`, page-scoped `<style>`, hero
(eyebrow / three-line headline / lede / buttons / install command with blinking cursor) plus
a live `fsharp live title="Todos.fs"` console example, the three-column feature row, and a
new Signals.Dom band (`.band`, mirrored: live `preset=app` counter on the left, copy right).
Bands share `max-width: var(--band-max)` = 1180px, centred.

`demo/styles/tokens.css` - rewritten: same token *names*, Signal Grid values, new
`--font-display` (Space Grotesk) and `--ink-body`, `html[data-theme="dark"]` block. `--ink`
stays the strong near-black because the demo uses it for node strokes and panel marks, not
just text. `@font-face` deliberately removed - the host docs page declares the faces, and
the demo's `/fonts/...` paths 404 when embedded; the standalone-only `demo/index.html` now
carries them in a `<style>` block instead.

`demo/styles/base.css` - headings to Space Grotesk (no more uppercase mono), body to
`--ink-body`, buttons to 5px radius with accent-on-hover.
`demo/styles/examples/apps.css` - four `oklch(var(--lch-ink) / …)` composites replaced with
`color-mix`, since the `--lch-*` layer is gone.
`demo/styles/schematic.css` - `.sch-pulse` is now accent-filled with a translucent accent
halo (wide semi-transparent stroke + `paint-order: stroke`, because `box-shadow` does not
apply to SVG).
`demo/Examples/Schematic.fs` - animation slowed: `stagger` 400→650ms, `dotDuration`
330→520ms, flash 420→500ms. `travel` now defers `el.animate(...)` by one
`requestAnimationFrame` - hypothesis for "no dot on the first button press" is that
`offset-path` has not been through layout on the edge's first animation. **Unverified.**
`docs/visuals/visuals.css` - `[data-visual]` tokens now reference `--sg-*` instead of a
frozen copy of the demo's old palette; 24px grid backdrop on `.sch-frame`; the diagram
buttons got the secondary-button treatment (they never inherited the demo's `base.css`).
`build/Commands/Docs.fs` - `Workspace.docs.``Docs.fsproj``` casing fix after the user renamed
the project file; `compileDemo()` deletes the Vite `index.html` so it cannot collide with the
Nacara-rendered `/demo/` page.

## Verified vs not

Verified with Playwright: no horizontal overflow at 1600/1440/1300/1200/1199/1150/1100/1000/
960/941/940/900/860/768/600/520/414/375 on the landing page, `/signals/derived/` and the Dom
reference page. Landing page screenshotted at 1440 and 900 and read - code cards no longer
clip, hero and band layouts look right.

The navbar overflow the user reported was **not** the landing page: `.nacara-search__trigger`
carries `min-width: 18rem` from `Nacara.Plugin.Search`, and with seven navbar sections the
row's intrinsic width was 1137px, overflowing every viewport between ~861px (where Nacara's
own rule hides the links) and ~1140px. Fixed in `theme.css` with two media queries: collapse
the search box to icon-only below 1200px, and hide the nav links at 940px instead of 860px.

**Not verified:** the `.sch-pulse` halo and the animation timing, the first-press dot fix,
dark mode anywhere, and the demo app itself after the token rewrite.

## Open items

- Confirm the dot halo/animation and the first-press fix in a browser; if the dot still
  misses the first press, look at the run-counter baseline in the edge's `Signal.effect` in
  `Examples/Schematic.fs` (it skips its first observation via a `previousRuns = -1` sentinel).
- Check dark mode across docs, reference, landing and demo.
- The band's copy is vertically centred against a taller code card; may want `align-items`
  tuning.
- Consequence of the widescreen cap: the hero's grid backdrop and the bands' divider rules
  stop at 1180px, so the page reads as a centred sheet. Alternative is full-bleed bands with
  capped inner content, which needs wrapper divs per band. User has not chosen.
- Nacara upstream ideas discussed but not acted on: exposing the live-example tree-sitter
  highlighter as a web component, and a sidebar glyph slot for the reference page's
  module/type markers (`Menu.badge` + `data-kind` is the existing workaround).

## Tooling

- Build: `./build.sh docs build` and `./build.sh docs check` (the latter is the link/render
  check). Always use `build.sh`, not raw `dotnet`. The demo alone: `./build.sh demo -w`.
- Playwright MCP was just added (`claude mcp add playwright -- npx -y @playwright/mcp@latest`)
  and should be available in a fresh session as `mcp__playwright__browser_*`. Prefer it.
- Manual fallback used this session (works, no MCP needed): the built site expects to be
  served under the `/Fable.UI/` base, so symlink it and serve:
  ```
  mkdir -p /tmp/preview/Fable.UI
  for f in docs/output/*; do ln -sfn "$PWD/$f" /tmp/preview/Fable.UI/$(basename $f); done
  cd /tmp/preview && python3 -m http.server 8200   # then http://127.0.0.1:8200/Fable.UI/
  ```
  Scripts left at `/tmp/shot.mjs` (screenshots), `/tmp/overflow.mjs` (names the element that
  exceeds the viewport), `/tmp/sweep.mjs` (width sweep). They import Playwright from the
  repo's `node_modules`.
- A local Nacara checkout is at `/home/mmangel/Workspaces/Github/MangelMaxime/Nacara/main` -
  use it to read real default CSS and plugin source rather than guessing or decompiling.
