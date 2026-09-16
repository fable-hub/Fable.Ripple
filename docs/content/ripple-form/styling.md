---
title: Styling
---

The Plain renderer puts an `rf-*` class on every element it draws and ships `plain.css` to style them. The stylesheet is driven by custom properties, so matching a design means setting variables, not rewriting rules.

## Using the stylesheet

The package carries it under `fable/assets/`, so Fable copies it into `fable_modules` next to the package's sources, in a folder named after the package and its version:

```css
@import "./fable_modules/Fable.Ripple.Form.Plain.1.0.0/assets/plain.css";

:root {
    --rf-color-accent: #0b7285;
    --rf-radius: 0;
}
```

Use the version you installed in the path. Set the variables after the import, on `:root`{css} or on any ancestor of a form.

## The variables

| Variable | Default | Used for |
| --- | --- | --- |
| `--rf-gap` | `0.75rem` | space between fields; inside sections, lists and wizard steps |
| `--rf-gap-small` | `0.25rem` | label to control; vertical padding of controls and buttons |
| `--rf-font-size` | `1rem` | controls, checkboxes, radios |
| `--rf-font-size-label` | `0.875rem` | labels, buttons, status line, wizard steps |
| `--rf-font-size-help` | `0.75rem` | help text under a control |
| `--rf-font-family` | `inherit` | everything |
| `--rf-radius` | `6px` | controls, buttons, sections |
| `--rf-color-text` | `#1a1a1a` | text |
| `--rf-color-muted` | `#62626e` | help text, inactive steps, drag handles |
| `--rf-color-bg` | `#ffffff` | controls and buttons |
| `--rf-color-border` | `#d6d6dd` | controls, buttons, list items |
| `--rf-color-border-faint` | `#e4e4e8` | sections, inactive steps, sortable rows |
| `--rf-color-accent` | `#3b45ff` | submit button, focus, active and done steps, checked boxes |
| `--rf-color-accent-hover` | `#2a33e0` | submit button on hover |
| `--rf-color-accent-contrast` | `#ffffff` | text on the accent |
| `--rf-color-accent-wash` | `#eef0ff` | focus ring, sortable row bar |
| `--rf-color-error` | `#c0392b` | error text, invalid control's border |
| `--rf-color-error-wash` | `#fdf3f2` | invalid control's background |
| `--rf-color-success` | `#008350` | success status line |

## The classes

For your own stylesheet, or to override a rule:

| Class | Element |
| --- | --- |
| `rf-form` | the `form` |
| `rf-field`, `rf-field--validating` | one field: label, control, help |
| `rf-label` | the label |
| `rf-input`, `rf-select`, `rf-textarea`, `rf-input--invalid` | the control |
| `rf-help`, `rf-help--error` | the help line under a control |
| `rf-checkbox` | a label around a checkbox and its text |
| `rf-radios`, `rf-radio` | a radio group and each label in it |
| `rf-section`, `rf-section__title` | `fieldset` and `legend` |
| `rf-group` | the fields of `Form.group` |
| `rf-list`, `rf-list__items`, `rf-list__item` | a list, its rows, one row |
| `rf-button`, `rf-button--primary`, `rf-button--small`, `rf-button--loading` | submit, add and remove buttons |
| `rf-actions` | the row holding the buttons |
| `rf-status`, `rf-status--success` | the success message; errors use `rf-help rf-help--error` |
| `rf-steps`, `rf-steps__segment`, `--active`, `--done` | the wizard's step list |
| `rf-wizard-step`, `rf-wizard-step__title` | the current step |
| `rf-sortable`, `rf-sortable__row`, `rf-sortable__bar`, ... | the sortable list |

## Layout

`Form.section "Title"`{fsharp} wraps a sub-form in a `fieldset`{html} with a legend; `Form.group`{fsharp} puts its fields side by side. Anything else is a `Form.wrap`{fsharp}: a function that receives the fields and places them in your own markup.

```fsharp
let card (title: string) (form: Form<'A>) : Form<'A> =
    form
    |> Form.wrap (fun context items ->
        Html.div
            [
                attr.className "card"
                Html.h3 title
                yield! context.RenderItems context items
            ]
    )
```

## Another look for a field kind

Keep the field's attributes and change its markup: see [Customize the view](custom-view.md).
